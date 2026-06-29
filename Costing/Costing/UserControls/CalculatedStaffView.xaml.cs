using Costing.Data;
using Costing.Models;
using Costing.Viewmodels;
using Microsoft.EntityFrameworkCore;
using System.IO;
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

            if (sender is Button btn) btn.IsEnabled = false;
            Mouse.OverrideCursor = Cursors.Wait;
            pnlProgress.Visibility = Visibility.Visible;
            pbCalculations.Value = 0;
            txtProgress.Text = "Reading Excel file...";

            try
            {
                // Read RatePerHour + Efficiency from Excel (keyed by Code)
                var excelData = await Task.Run(() =>
                    Helpers.ExcelHelper.GetRateAndEfficiencyFromExcel(filePath));

                // Load Allocations and Staff costs from DB
                txtProgress.Text = "Loading allocations from database...";

                List<Allocation> allocations;
                List<StaffCost> liveDbCosts;

                using (var context = new CostingDbContext())
                {
                    // Every row in Allocations = one employee+workcentre assignment
                    allocations = await context.Allocations
                        .Where(a => !string.IsNullOrEmpty(a.WorkCentre) && a.Portion > 0)
                        .ToListAsync();

                    liveDbCosts = await context.StaffCosts.ToListAsync();
                }

                if (!allocations.Any())
                {
                    MessageBox.Show("No allocations found in the database.\n\nPlease assign employees to work centres on the Allocations screen first.",
                                    "No Allocations", MessageBoxButton.OK, MessageBoxImage.Information);
                    return;
                }

                // One CalculatedStaff per Allocation row.
                // RatePerHour + Efficiency  from Excel, everything else from DB.
                txtProgress.Text = "Building calculation inputs...";

                var rawRecords = new List<CalculatedStaff>();

                foreach (var alloc in allocations)
                {
                    excelData.TryGetValue(alloc.Code, out var excelRow);
                    // If the employee isn't in the Excel sheet we default to 0/1
                    decimal ratePerHour = excelRow.RatePerHour;
                    decimal efficiency = excelRow.Efficiency == 0 ? 1m : excelRow.Efficiency;

                    rawRecords.Add(new CalculatedStaff
                    {
                        Code = alloc.Code,
                        Name = alloc.Name,
                        WorkCentre = alloc.WorkCentre,
                        RatePerHour = ratePerHour,
                        Allocation = alloc.Portion ?? 0m,
                        Efficiency = efficiency
                    });
                }

                // Run calculation engine
                var progressReporter = new Progress<int>(percent =>
                {
                    pbCalculations.Value = percent;
                    txtProgress.Text = $"Calculating: {percent}%";
                });

                List<CalculatedStaff> finalRecords = await Task.Run(() =>
                    Helpers.CalculationEngine.ProcessCalculations(rawRecords, liveDbCosts, progressReporter));

                //Rebuild dynamic columns
                txtProgress.Text = "Rebuilding grid columns...";

                // Remove any previously added dynamic columns (keep the first 8 static ones)
                while (CalculatedDataGrid.Columns.Count > 9)
                    CalculatedDataGrid.Columns.RemoveAt(CalculatedDataGrid.Columns.Count - 2);

                int insertIndex = CalculatedDataGrid.Columns.Count - 2; // before TOTAL

                foreach (var dbCost in liveDbCosts)
                {
                    CalculatedDataGrid.Columns.Insert(insertIndex, new DataGridTextColumn
                    {
                        Header = dbCost.Category,
                        Binding = new System.Windows.Data.Binding($"[{dbCost.Category}]")
                        {
                            StringFormat = "{0:N2}"
                        },
                        Width = new DataGridLength(80)
                    });
                    insertIndex++;
                }

                // Push to grid
                txtProgress.Text = "Rendering final data...";
                vm.OCCalculatedStaff.Clear();
                foreach (var record in finalRecords)
                    vm.OCCalculatedStaff.Add(record);
            }
            catch (Exception ex)
            {
                MessageBox.Show("An error occurred during calculation:\n\n" + ex.Message,
                                "Calculation Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                Mouse.OverrideCursor = null;
                if (sender is Button finalBtn) finalBtn.IsEnabled = true;
                pnlProgress.Visibility = Visibility.Collapsed;
            }
        }

        private async void btnSaveToDb_Click(object sender, RoutedEventArgs e)
        {
            var vm = this.DataContext as CalculatedStaffViewModel;

            if (vm == null || vm.OCCalculatedStaff.Count == 0)
            {
                MessageBox.Show("There is no calculated data to save. Please run calculations first.",
                                "No Data", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            Mouse.OverrideCursor = Cursors.Wait;

            try
            {
                await Helpers.DatabaseHelper.SaveCalculatedStaffToDatabaseAsync(vm.OCCalculatedStaff);
                MessageBox.Show("Calculated data has been synced with the database!",
                                "Save Complete", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error saving to SQL Server:\n\n" + ex.Message,
                                "Database Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                Mouse.OverrideCursor = null;
            }
        }
    }
}