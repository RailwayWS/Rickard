using Costing.Data;
using Costing.Viewmodels;
using Microsoft.EntityFrameworkCore;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace Costing.UserControls
{
    public partial class AuditLogsView : UserControl
    {
        public AuditLogsView()
        {
            InitializeComponent();
        }

        private async void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            Mouse.OverrideCursor = Cursors.Wait;

            try
            {
                using (var context = new CostingDbContext())
                {
                    // Fetch all unique snapshot names to populate the dropdown
                    var snapshots = await context.AuditLogs
                                                 .Select(a => a.SnapshotName)
                                                 .Distinct()
                                                 .ToListAsync();

                    cmbSnapshots.ItemsSource = snapshots;

                    if (snapshots.Any())
                    {
                        cmbSnapshots.SelectedIndex = 0; // Auto-select the first one
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error loading audit snapshots:\n\n" + ex.Message, "Database Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                Mouse.OverrideCursor = null;
            }
        }

        private async void cmbSnapshots_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (cmbSnapshots.SelectedItem == null) return;

            string selectedSnapshot = cmbSnapshots.SelectedItem.ToString();
            var vm = this.DataContext as AuditLogsViewModel;

            if (vm == null) return;

            Mouse.OverrideCursor = Cursors.Wait;

            try
            {
                using (var context = new CostingDbContext())
                {
                    // Fetch only the records tied to the selected snapshot name
                    var logData = await context.AuditLogs
                                               .Where(a => a.SnapshotName == selectedSnapshot)
                                               .ToListAsync();

                    vm.OCAuditLogs.Clear();
                    foreach (var record in logData)
                    {
                        vm.OCAuditLogs.Add(record);
                    }

                    vm.SelectedSnapshot = logData.FirstOrDefault();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error fetching log data:\n\n" + ex.Message, "Database Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                Mouse.OverrideCursor = null;
            }
        }

        #region Export and delete buttons

        private async void btnDelete_Click(object sender, RoutedEventArgs e)
        {
            if (cmbSnapshots.SelectedItem == null)
            {
                MessageBox.Show("Please select a snapshot to delete.", "No Selection", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            string selectedSnapshot = cmbSnapshots.SelectedItem.ToString();

            var result = MessageBox.Show($"Are you sure you want to permanently delete the audit snapshot '{selectedSnapshot}'?\n\nThis will remove all associated records and cannot be undone.",
                                         "Confirm Delete", MessageBoxButton.YesNo, MessageBoxImage.Warning);

            if (result != MessageBoxResult.Yes) return;

            Mouse.OverrideCursor = Cursors.Wait;

            try
            {
                using (var context = new CostingDbContext())
                {
                    // Grab all rows belonging to this snapshot and wipe them
                    var logsToDelete = await context.AuditLogs.Where(a => a.SnapshotName == selectedSnapshot).ToListAsync();
                    context.AuditLogs.RemoveRange(logsToDelete);
                    await context.SaveChangesAsync();

                    // Clear the grid
                    var vm = this.DataContext as AuditLogsViewModel;
                    if (vm != null)
                    {
                        vm.OCAuditLogs.Clear();
                        vm.SelectedSnapshot = null;
                    }

                    // Refresh the ComboBox list
                    var remainingSnapshots = await context.AuditLogs.Select(a => a.SnapshotName).Distinct().ToListAsync();
                    cmbSnapshots.ItemsSource = remainingSnapshots;

                    if (remainingSnapshots.Any())
                    {
                        cmbSnapshots.SelectedIndex = 0;
                    }
                    else
                    {
                        cmbSnapshots.SelectedItem = null;
                    }
                }

                MessageBox.Show($"Audit snapshot '{selectedSnapshot}' was successfully deleted.", "Deleted", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error deleting snapshot:\n\n" + ex.Message, "Database Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                Mouse.OverrideCursor = null;
            }
        }

        private void btnExport_Click(object sender, RoutedEventArgs e)
        {
            var vm = this.DataContext as AuditLogsViewModel;

            if (vm == null || vm.OCAuditLogs.Count == 0)
            {
                MessageBox.Show("There is no data to export. Please select a snapshot first.", "No Data", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            Microsoft.Win32.SaveFileDialog saveDialog = new Microsoft.Win32.SaveFileDialog();
            saveDialog.Filter = "Excel CSV File (*.csv)|*.csv";
            saveDialog.Title = "Export Audit Log";

            string safeSnapshotName = cmbSnapshots.SelectedItem.ToString().Replace(" ", "_").Replace("/", "-");
            saveDialog.FileName = $"AuditLog_{safeSnapshotName}_{DateTime.Now:yyyyMMdd}.csv";

            if (saveDialog.ShowDialog() == true)
            {
                Mouse.OverrideCursor = Cursors.Wait;

                try
                {
                    Costing.Helpers.ExcelHelper.ExportAuditToExcel(vm.OCAuditLogs, saveDialog.FileName);

                    MessageBox.Show($"Export complete!\n\nFile saved to:\n{saveDialog.FileName}", "Export Success", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Error exporting file:\n\n" + ex.Message, "Export Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
                finally
                {
                    Mouse.OverrideCursor = null;
                }
            }
        }

        #endregion

    }
}