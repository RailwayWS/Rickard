using System;
using System.Data;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Costing.Viewmodels;
using Costing.Models;
using Costing.Helpers;

namespace Costing.UserControls
{
    /// <summary>
    /// Interaction logic for StaffView.xaml
    /// </summary>
    public partial class StaffView : UserControl
    {
        public StaffView()
        {
            InitializeComponent();
        }

        private void btnImport_Click(object sender, RoutedEventArgs e)
        {
            // cast DataContect into Viewmodel to gain access to OC
            var vm = this.DataContext as StaffViewModel;

            string filePath = @"C:\Users\jmostert\Desktop\rick temp\project\Wages.xlsx";
            if (!File.Exists(filePath))
            {
                MessageBox.Show($"Could not find the file at:\n{filePath}", "File Not Found", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            Mouse.OverrideCursor = Cursors.Wait;

             try
            {
                vm.OCStaff.Clear();

                DataTable importedData = ExcelHelper.ImportWagesToDataTable(filePath);

                foreach (DataRow row in importedData.Rows)
                {
                    vm.OCStaff.Add(new BasicEmployee
                    {
                        Code = row["Code"].ToString(),
                        Name = row["Name"].ToString(),
                        CostCentre = row["CostCentre"].ToString(),
                        JobDescription = row["JobDescrip"].ToString(),
                        Rate = Convert.ToDecimal(row["Rate"])
                    });
                }
            }

            catch (Exception ex)
            {
                MessageBox.Show("Error importing file: \n\n" + ex.Message,
                                "Import Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }

            finally
            {
                Mouse.OverrideCursor = Cursors.Arrow;
            }
        }


        private async void btnSaveToDb_Click(object sender, RoutedEventArgs e) 
        {
            var vm = this.DataContext as StaffViewModel;

            // cant save when empty
            if (vm == null || vm.OCStaff.Count == 0)
            {
                MessageBox.Show("There is no data to save. Please import a file first.", "No Data", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            Mouse.OverrideCursor = Cursors.Wait;

            try
            {
                await Task.Run(() => Helpers.DatabaseHelper.SaveStaffToDatabase(vm.OCStaff));
                MessageBox.Show("Staff data has been successfully saved to the RA-ERP Costing database!", "Save Complete", MessageBoxButton.OK, MessageBoxImage.Information);
            }

            catch (Exception ex)
            {
                MessageBox.Show("Error saving to database: \n\n" + ex.Message,
                                "Database Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                Mouse.OverrideCursor = Cursors.Arrow;
            }

        }
    }
}
