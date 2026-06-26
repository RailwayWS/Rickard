 using Costing.Models;
using Costing.Viewmodels;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace Costing.UserControls
{
    public partial class CalculatedStaffView : UserControl
    {
        public CalculatedStaffView()
        {
            InitializeComponent();
        }

        private async void btnRunCalculations_Click(object sender, RoutedEventArgs e)
        {
            var vm = this.DataContext as CalculatedStaffViewModel;
            if (vm == null) return;

            string filePath = Costing.Properties.Settings.Default.ImportPathCosting;

            if (string.IsNullOrWhiteSpace(filePath))
            {
                MessageBox.Show("No default Costing Sheet path found. Please configure it in the Settings tab first.",
                                "Settings Required", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            if (!File.Exists(filePath))
            {
                MessageBox.Show($"Could not find the Excel file at:\n{filePath}",
                                "File Not Found", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            Mouse.OverrideCursor = Cursors.Wait;
            Button actionButton = sender as Button; // disblae button

            pnlProgress.Visibility = Visibility.Visible;
            pbCalculations.Value = 0;
            txtProgress.Text = "Reading Excel file...";

            try
            {
                // Extract raw parameters from Excel
                List<CalculatedStaff> rawRecords = await Task.Run(() => Helpers.ExcelHelper.GetRawCalculatedStaffFromExcel(filePath));

                // Fetch variables from SQL
                List<StaffCost> liveDbCosts = await Helpers.DatabaseHelper.GetAllStaffCostsAsync();

                var progressReporter = new Progress<int>(percent =>
                {
                    pbCalculations.Value = percent;
                    txtProgress.Text = $"Calculating: {percent}%";
                });

                // Engine runs in background
                List<CalculatedStaff> finalRecords = await Task.Run(() =>
                    Helpers.CalculationEngine.ProcessCalculations(rawRecords, liveDbCosts, progressReporter)
                );

                while (CalculatedDataGrid.Columns.Count > 8)
                {
                    CalculatedDataGrid.Columns.RemoveAt(CalculatedDataGrid.Columns.Count - 2);
                }

                txtProgress.Text = "Rebuilding Grid Columns...";

                // add dynamic columns
                int insertIndex = CalculatedDataGrid.Columns.Count - 1; // Insert right before TOTAL

                foreach (var dbCost in liveDbCosts)
                {
                    var newColumn = new DataGridTextColumn
                    {
                        Header = dbCost.Category,
                        // uses indexer in Calculated staff model
                        Binding = new System.Windows.Data.Binding($"[{dbCost.Category}]")
                        {
                            StringFormat = "{0:N2}"
                        },
                        Width = new DataGridLength(80)
                    };

                    CalculatedDataGrid.Columns.Insert(insertIndex, newColumn);
                    insertIndex++;
                }

                // Update the UI
                txtProgress.Text = "Rendering final data...";
                vm.OCCalculatedStaff.Clear();
                foreach (var record in finalRecords)
                {
                    vm.OCCalculatedStaff.Add(record);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("An error occurred during calculation:\n\n" + ex.Message, "Calculation Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                Mouse.OverrideCursor = Cursors.Arrow;
                if (sender is Button btn) btn.IsEnabled = true;
                pnlProgress.Visibility = Visibility.Collapsed;
            }
        }

        private async void btnSaveToDb_Click(object sender, RoutedEventArgs e)
        {
            var vm = this.DataContext as CalculatedStaffViewModel;

            if (vm == null || vm.OCCalculatedStaff.Count == 0)
            {
                MessageBox.Show("There is no calculated data to save. Please run calculations first.", "No Data", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            Mouse.OverrideCursor = Cursors.Wait;

            try
            {
                // Push the entire calculated list through our Upsert helper
                await Helpers.DatabaseHelper.SaveCalculatedStaffToDatabaseAsync(vm.OCCalculatedStaff);

                MessageBox.Show("Calculated data has been synced with the database!", "Save Complete", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error saving to SQL Server:\n\n" + ex.Message, "Database Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                Mouse.OverrideCursor = null;
            }
        }
    }
}