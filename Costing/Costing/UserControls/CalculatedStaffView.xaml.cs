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

        #region Calculations
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
                List<BasicEmployee> liveStaff;

                using (var context = new CostingDbContext())
                {
                    // Every row in Allocations = one employee+workcentre assignment
                    allocations = await context.Allocations
                        .Where(a => !string.IsNullOrEmpty(a.WorkCentre) && a.Portion > 0)
                        .ToListAsync();

                    liveDbCosts = await context.StaffCosts.ToListAsync();
                    liveStaff = await context.Staff.ToListAsync();
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
                    var staffMember = liveStaff.FirstOrDefault(s => s.Code == alloc.Code);
                    if (staffMember == null) continue;

                    decimal ratePerHour = staffMember.Rate;
                    decimal efficiency = 1m; // Default to 100% if not found in Costing Sheet
                    if (excelData.TryGetValue(alloc.Code, out var excelRow))
                    {
                        efficiency = excelRow.Efficiency == 0 ? 1m : excelRow.Efficiency;
                    }

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

                while (CalculatedDataGrid.Columns.Count > 9)
                {
                    CalculatedDataGrid.Columns.RemoveAt(7);
                }

                int insertIndex = 7;

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

        #endregion

        #region Audit function

        private void btnRecordAudit_Click(object sender, RoutedEventArgs e)
        {
            var vm = this.DataContext as CalculatedStaffViewModel;

            // Prevent users from trying to log an empty screen
            if (vm == null || vm.OCCalculatedStaff.Count == 0)
            {
                MessageBox.Show("Please run calculations first. There is no data to record.", "No Data", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            txtAuditName.Text = "";
            pnlAuditCategories.Children.Clear();

            // Look at the first calculated employee to see what dynamic categories currently exist
            var firstEmp = vm.OCCalculatedStaff.FirstOrDefault();
            if (firstEmp != null && firstEmp.DynamicCosts != null)
            {
                foreach (var cost in firstEmp.DynamicCosts)
                {
                    var cb = new CheckBox
                    {
                        Content = cost.CategoryName,
                        Foreground = new System.Windows.Media.SolidColorBrush((System.Windows.Media.Color)System.Windows.Media.ColorConverter.ConvertFromString("#CBD5E1")),
                        Margin = new Thickness(0, 0, 8, 8),
                        Tag = cost.CategoryName,
                        IsChecked = true
                    };

                    cb.Style = (Style)Application.Current.TryFindResource("ChipCheckBoxStyle");

                    pnlAuditCategories.Children.Add(cb);
                }
            }

            pnlAuditInput.Visibility = Visibility.Visible;
        }

        private void btnCancelAudit_Click(object sender, RoutedEventArgs e)
        {
            pnlAuditInput.Visibility = Visibility.Collapsed;
        }

        private async void btnConfirmAudit_Click(object sender, RoutedEventArgs e)
        {
            string snapshotName = txtAuditName.Text.Trim();

            if (string.IsNullOrWhiteSpace(snapshotName))
            {
                MessageBox.Show("Please enter a valid snapshot name.", "Required", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            // Figure out exactly which categories the user left checked
            var selectedCategories = new System.Collections.Generic.List<string>();
            foreach (CheckBox cb in pnlAuditCategories.Children)
            {
                if (cb.IsChecked == true)
                {
                    selectedCategories.Add(cb.Tag.ToString());
                }
            }

            if (selectedCategories.Count == 0)
            {
                MessageBox.Show("Please select at least one category to audit.", "Required", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var vm = this.DataContext as CalculatedStaffViewModel;
            if (vm == null || vm.OCCalculatedStaff.Count == 0) return;

            // Hide the panel and show the loading cursor
            pnlAuditInput.Visibility = Visibility.Collapsed;
            Mouse.OverrideCursor = Cursors.Wait;

            try
            {
                using (var context = new CostingDbContext())
                {
                    // get the current rates from the DB
                    var liveDbCosts = await context.StaffCosts.ToListAsync();
                    DateTime exactTimestamp = DateTime.Now;
                    var newSnapshots = new System.Collections.Generic.List<AuditLog>();

                    // Loop through every calculated employee
                    foreach (var emp in vm.OCCalculatedStaff)
                    {
                        // Create the Parent Record
                        var snap = new AuditLog
                        {
                            Code = emp.Code,
                            EmployeeName = emp.Name,
                            SnapshotDate = exactTimestamp,
                            SnapshotName = snapshotName,
                            Costs = new System.Collections.Generic.List<AuditLogCost>()
                        };

                        // Loop through only the CHECKED categories and create Child Records
                        foreach (var cat in selectedCategories)
                        {
                            // Find the rate used
                            decimal? rateUsed = liveDbCosts.FirstOrDefault(c => c.Category.Equals(cat, System.StringComparison.OrdinalIgnoreCase))?.Value;

                            snap.Costs.Add(new AuditLogCost
                            {
                                CategoryName = cat,
                                Amount = emp[cat],
                                RateUsed = rateUsed
                            });
                        }

                        newSnapshots.Add(snap);
                    }

                    context.AuditLogs.AddRange(newSnapshots);
                    await context.SaveChangesAsync();
                }

                MessageBox.Show($"Audit log '{snapshotName}' successfully recorded with {selectedCategories.Count} categories!", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (System.Exception ex)
            {
                string errorMsg = ex.Message;
                if (ex.InnerException != null)
                {
                    errorMsg += "\n\nINNER EXCEPTION:\n" + ex.InnerException.Message;
                }
                MessageBox.Show("Error recording audit log:\n\n" + errorMsg, "Database Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                Mouse.OverrideCursor = null;
            }
        }

        #endregion
    }
}