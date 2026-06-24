using Microsoft.Office.Interop.Excel;
using System.Data;
using System.IO;
using System.Windows;
using System.Windows.Input;



namespace Costing.Helpers {

    public class ExcelHelper
    {
        public static System.Data.DataTable ImportWagesToDataTable(string filepath)
        {

            System.Data.DataTable dtDetail = new System.Data.DataTable();

            dtDetail.Columns.Add("Code", typeof(string));
            dtDetail.Columns.Add("Name", typeof(string));
            dtDetail.Columns.Add("CostCentre", typeof(string));
            dtDetail.Columns.Add("JobDescrip", typeof(string));
            dtDetail.Columns.Add("Rate", typeof(decimal));

            Mouse.OverrideCursor = Cursors.Wait;

            if (File.Exists(filepath))
            {
                Mouse.OverrideCursor = Cursors.Wait;
                Microsoft.Office.Interop.Excel.Application XLApp = new Microsoft.Office.Interop.Excel.Application();
                Workbook XLWB = XLApp.Workbooks.Open(filepath);
                Worksheet XLWS = (Worksheet)XLWB.ActiveSheet;

                // Find the last real row
                int lastUsedRow = XLWS.Cells.Find("*", System.Reflection.Missing.Value,
                                                System.Reflection.Missing.Value, System.Reflection.Missing.Value,
                                                Microsoft.Office.Interop.Excel.XlSearchOrder.xlByRows, Microsoft.Office.Interop.Excel.XlSearchDirection.xlPrevious,
                                                false, System.Reflection.Missing.Value, System.Reflection.Missing.Value).Row;


                for (int rowindex = 2; rowindex <= lastUsedRow; rowindex++)
                {

                    DataRow nr = dtDetail.NewRow();

                    nr["Code"] = Convert.ToString(((Microsoft.Office.Interop.Excel.Range)XLWS.Cells[rowindex, 1]).Value2);
                    nr["Name"] = Convert.ToString(((Microsoft.Office.Interop.Excel.Range)XLWS.Cells[rowindex, 2]).Value2);
                    nr["CostCentre"] = Convert.ToString(((Microsoft.Office.Interop.Excel.Range)XLWS.Cells[rowindex, 3]).Value2);
                    nr["JobDescrip"] = Convert.ToString(((Microsoft.Office.Interop.Excel.Range)XLWS.Cells[rowindex, 4]).Value2);
                    nr["Rate"] = Convert.ToString(((Microsoft.Office.Interop.Excel.Range)XLWS.Cells[rowindex, 5]).Value2);


                    dtDetail.Rows.Add(nr);


                }
                XLWB.Close();
                XLApp.Quit();

                // Clean up Excel processes
                System.Runtime.InteropServices.Marshal.ReleaseComObject(XLWS);
                System.Runtime.InteropServices.Marshal.ReleaseComObject(XLWB);
                System.Runtime.InteropServices.Marshal.ReleaseComObject(XLApp);
            }

            return dtDetail;

        }

        public static List<string> GetCategoriesFromExcel(string filepath)
        {
            List<string> categories = new List<string>();
            if (File.Exists(filepath))
            {
                Microsoft.Office.Interop.Excel.Application XLApp = new Microsoft.Office.Interop.Excel.Application();
                Workbook XLWB = XLApp.Workbooks.Open(filepath);
                Worksheet XLWS = (Worksheet)XLWB.Worksheets[1];

                // from S to AA in wages sheet
                for (int colIndex = 19; colIndex <= 27; colIndex++)
                {
                    var cellvalue = ((Microsoft.Office.Interop.Excel.Range)XLWS.Cells[1, colIndex]).Value2;

                    if (cellvalue != null)
                    {
                        categories.Add(Convert.ToString(cellvalue));
                    }
                }

                XLWB.Close();
                XLApp.Quit();

                // Clean up Excel processes
                System.Runtime.InteropServices.Marshal.ReleaseComObject(XLWS);
                System.Runtime.InteropServices.Marshal.ReleaseComObject(XLWB);
                System.Runtime.InteropServices.Marshal.ReleaseComObject(XLApp);
            }

            return categories;
        }


        public static List<Costing.Models.CalculatedStaff> GetRawCalculatedStaffFromExcel(string filepath)
        {
            List<Costing.Models.CalculatedStaff> staffList = new List<Costing.Models.CalculatedStaff>();

            if (File.Exists(filepath))
            {
                Microsoft.Office.Interop.Excel.Application XLApp = new Microsoft.Office.Interop.Excel.Application();
                Microsoft.Office.Interop.Excel.Workbook XLWB = XLApp.Workbooks.Open(filepath);
                Microsoft.Office.Interop.Excel.Worksheet XLWS = (Microsoft.Office.Interop.Excel.Worksheet)XLWB.ActiveSheet;

                int lastUsedRow = XLWS.Cells.Find("*", System.Reflection.Missing.Value,
                                                System.Reflection.Missing.Value, System.Reflection.Missing.Value,
                                                Microsoft.Office.Interop.Excel.XlSearchOrder.xlByRows, Microsoft.Office.Interop.Excel.XlSearchDirection.xlPrevious,
                                                false, System.Reflection.Missing.Value, System.Reflection.Missing.Value).Row;

                // temp variables
                string currentCode = "";
                string currentName = "";
                decimal currentRate = 0m;

                for (int rowindex = 2; rowindex <= lastUsedRow; rowindex++)
                {
                    string rawCode = Convert.ToString(((Microsoft.Office.Interop.Excel.Range)XLWS.Cells[rowindex, 1]).Value2);
                    string rawName = Convert.ToString(((Microsoft.Office.Interop.Excel.Range)XLWS.Cells[rowindex, 2]).Value2); // Columns B
                    string rawWorkCentre = Convert.ToString(((Microsoft.Office.Interop.Excel.Range)XLWS.Cells[rowindex, 12]).Value2); // Column L
                    string rawEff = Convert.ToString(((Microsoft.Office.Interop.Excel.Range)XLWS.Cells[rowindex, 29]).Value2); // Column AC

                    // skip if workcewntre and efficiency is empty
                    if (string.IsNullOrWhiteSpace(rawWorkCentre) && string.IsNullOrWhiteSpace(rawEff))
                    {
                        continue;
                    }

                    // If the code cell has value we update temp vars
                    if (!string.IsNullOrWhiteSpace(rawCode) || !string.IsNullOrWhiteSpace(rawName))
                    {
                        currentCode = rawCode;
                        currentName = Convert.ToString(((Microsoft.Office.Interop.Excel.Range)XLWS.Cells[rowindex, 2]).Value2);

                        // Parse and remember their rate
                        string rateStr = Convert.ToString(((Microsoft.Office.Interop.Excel.Range)XLWS.Cells[rowindex, 9]).Value2);
                        decimal.TryParse(rateStr, out currentRate);
                    }

                    // PARSE THE REMAINING MATH INPUTS
                    decimal allocation = 0m;
                    string rawAlloc = Convert.ToString(((Microsoft.Office.Interop.Excel.Range)XLWS.Cells[rowindex, 16]).Value2);
                    if (!string.IsNullOrWhiteSpace(rawAlloc))
                    {
                        decimal.TryParse(rawAlloc, out allocation);
                    }

                    decimal efficiency = 1m; // default to 1
                    if (!string.IsNullOrWhiteSpace(rawEff))
                    {
                        decimal.TryParse(rawEff, out efficiency);
                    }

                    if (string.IsNullOrWhiteSpace(currentCode) || currentCode.Equals("Code", StringComparison.OrdinalIgnoreCase))
                    {
                        continue;
                    }

                    staffList.Add(new Costing.Models.CalculatedStaff
                    {
                        Code = currentCode,
                        Name = currentName,
                        WorkCentre = rawWorkCentre,
                        RatePerHour = currentRate,
                        Allocation = allocation,
                        Efficiency = efficiency
                    });
                }

                XLWB.Close();
                XLApp.Quit();

                // Clean up Excel processes
                System.Runtime.InteropServices.Marshal.ReleaseComObject(XLWS);
                System.Runtime.InteropServices.Marshal.ReleaseComObject(XLWB);
                System.Runtime.InteropServices.Marshal.ReleaseComObject(XLApp);
            }

            return staffList;
        }

    }
}
