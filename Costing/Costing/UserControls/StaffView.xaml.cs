using Costing.Helpers;
using Costing.Models;
using Costing.Viewmodels;
using System.Data;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

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

            string filePath = Costing.Properties.Settings.Default.ImportPathWages;

            if (string.IsNullOrWhiteSpace(filePath))
            {
                MessageBox.Show("No default import path found. Please configure your Default Import Path in the Settings tab first.",
                                "Settings Required", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            if (!File.Exists(filePath))
            {
                MessageBox.Show($"Could not find the file at:\n{filePath}\n\nPlease check your Settings tab.",
                                "File Not Found", MessageBoxButton.OK, MessageBoxImage.Warning);
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
                var employeesToDelete = await Helpers.DatabaseHelper.GetEmployeesToDeleteAsync(vm.OCStaff);

                if (employeesToDelete.Any())
                {
                    Mouse.OverrideCursor = Cursors.Arrow;

                    string missingList = string.Join("\n", employeesToDelete);

                    var result = MessageBox.Show($"The following employees are no longer in the imported Wages table.\n\nBy clicking OK, you will permanently delete them (and all their calculated history) from the system:\n\n{missingList}\n\nDo you wish to proceed?",
                                                 "Confirm Deletion", MessageBoxButton.OKCancel, MessageBoxImage.Warning);

                    if (result != MessageBoxResult.OK)
                    {
                        return;
                    }

                    Mouse.OverrideCursor = Cursors.Wait;
                }

                await Helpers.DatabaseHelper.SaveStaffToDatabaseAsync(vm.OCStaff);
                MessageBox.Show("Staff data has been successfully saved to the RA-ERP Costing database!", "Save Complete", MessageBoxButton.OK, MessageBoxImage.Information);
            }

            catch (Exception ex)
            {
                string errorMsg = ex.Message;

                if (ex.InnerException != null)
                {
                    errorMsg += "\n\nINNER EXCEPTION:\n" + ex.InnerException.Message;
                }

                MessageBox.Show("Error saving to database:\n\n" + errorMsg,
                                "Database Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                Mouse.OverrideCursor = Cursors.Arrow;
            }

        }
    }
}
