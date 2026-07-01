using Microsoft.Office.Interop.Excel;
using System.Data;
using System.IO;
using System.Windows.Input;
using Range = Microsoft.Office.Interop.Excel.Range;



namespace Costing.Helpers
{

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

        public static Dictionary<string, (decimal RatePerHour, decimal Efficiency)>
            GetRateAndEfficiencyFromExcel(string filepath)
        {
            var result = new Dictionary<string, (decimal, decimal)>(
                System.StringComparer.OrdinalIgnoreCase);

            if (!File.Exists(filepath)) return result;

            Microsoft.Office.Interop.Excel.Application XLApp = new Microsoft.Office.Interop.Excel.Application();
            Workbook XLWB = XLApp.Workbooks.Open(filepath);
            Worksheet XLWS = (Worksheet)XLWB.ActiveSheet;

            int lastUsedRow = XLWS.Cells.Find("*",
                System.Reflection.Missing.Value, System.Reflection.Missing.Value,
                System.Reflection.Missing.Value,
                XlSearchOrder.xlByRows, XlSearchDirection.xlPrevious,
                false, System.Reflection.Missing.Value, System.Reflection.Missing.Value).Row;

            // Track the last seen code so merged/blank code cells still resolve
            string currentCode = "";
            decimal currentRate = 0m;

            for (int row = 2; row <= lastUsedRow; row++)
            {
                string rawCode = System.Convert.ToString(((Range)XLWS.Cells[row, 1]).Value2);
                string rawEff = System.Convert.ToString(((Range)XLWS.Cells[row, 29]).Value2); // col AC

                // Update current code/rate whenever the code cell has a value
                if (!string.IsNullOrWhiteSpace(rawCode) &&
                    !rawCode.Equals("Code", System.StringComparison.OrdinalIgnoreCase))
                {
                    currentCode = rawCode.Trim();

                    string rateStr = System.Convert.ToString(((Range)XLWS.Cells[row, 9]).Value2); // col I
                    decimal.TryParse(rateStr, out currentRate);
                }

                if (string.IsNullOrWhiteSpace(currentCode)) continue;

                // Only record the row if efficiency is present
                if (!string.IsNullOrWhiteSpace(rawEff))
                {
                    decimal.TryParse(rawEff, out decimal eff);
                    result[currentCode] = (currentRate, eff);
                }
            }

            XLWB.Close();
            XLApp.Quit();
            System.Runtime.InteropServices.Marshal.ReleaseComObject(XLWS);
            System.Runtime.InteropServices.Marshal.ReleaseComObject(XLWB);
            System.Runtime.InteropServices.Marshal.ReleaseComObject(XLApp);

            return result;
        }


        public static void ExportAuditToExcel(IEnumerable<Costing.Models.AuditLog> auditData, string filePath)
        {
            var lines = new List<string>();

            // Find all unique categories present in this snapshot
            var allCategories = auditData
                .Where(log => log.Costs != null)
                .SelectMany(log => log.Costs)
                .Select(cost => cost.CategoryName)
                .Distinct()
                .OrderBy(name => name)
                .ToList();

            // Build the header
            var headers = new List<string> { "Code", "Employee Name", "Snapshot Date", "Snapshot Name" };
            foreach (var cat in allCategories)
            {
                headers.Add($"{cat} Amount");
                headers.Add($"{cat} Rate");
            }
            lines.Add(string.Join(";", headers));

            // Build the rows
            foreach (var log in auditData)
            {
                var row = new List<string>();

                row.Add(log.Code ?? "");
                row.Add(log.EmployeeName?.Replace(";", " ") ?? "");
                row.Add(log.SnapshotDate.ToString("yyyy-MM-dd HH:mm"));
                row.Add(log.SnapshotName?.Replace(";", " ") ?? "");

                // Match the costs to headers
                foreach (var cat in allCategories)
                {
                    var cost = log.Costs?.FirstOrDefault(c => c.CategoryName.Equals(cat, StringComparison.OrdinalIgnoreCase));

                    if (cost != null)
                    {
                        row.Add(cost.Amount.ToString("0.00", System.Globalization.CultureInfo.InvariantCulture));
                        row.Add(cost.RateUsed.HasValue ? cost.RateUsed.Value.ToString("0.00000", System.Globalization.CultureInfo.InvariantCulture) : "");
                    }
                    else
                    {
                        row.Add("0.00");
                        row.Add("");
                    }
                }

                lines.Add(string.Join(";", row));
            }

            System.IO.File.WriteAllLines(filePath, lines);
        }

    }
}