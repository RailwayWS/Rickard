using Costing.Data;
using Costing.Models;
using Costing.Other;
using Costing.Viewmodels;
using Microsoft.EntityFrameworkCore;
using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace Costing.UserControls
{
    public partial class SettingsView : UserControl
    {
        #region Controls

        SettingsViewModel vmSettings = new SettingsViewModel();
        CostingDbContext _context = new CostingDbContext();

        #endregion

        public SettingsView()
        {
            InitializeComponent();
            DataContext = vmSettings;
        }

        #region Tab Navigation
        private void tbTabcontrol_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (e.Source is TabControl tabControl && tabControl.SelectedItem is TabItem selected)
            {
                switch (selected.Name)
                {
                    case "tiServerSettings":
                        LoadApplicationSettings();
                        break;
                    case "tiUsers":
                        FillUsers();
                        break;
                    case "tiWorkCentres":
                        FillWorkCentres();
                        break;
                }
            }
        }
        #endregion

        #region Tab 1: Server Settings & Imports
        private void LoadApplicationSettings()
        {
            // Only load if empty to avoid overriding unsaved text box changes when swapping tabs
            if (string.IsNullOrEmpty(vmSettings.SysproServer))
            {
                Mouse.OverrideCursor = Cursors.Wait;
                try
                {
                    vmSettings.SysproServer = Costing.Properties.Settings.Default.SysproServer;
                    vmSettings.SysproDB = Costing.Properties.Settings.Default.SysproDB;
                    vmSettings.ImportPathWages = Costing.Properties.Settings.Default.ImportPathWages;
                    vmSettings.ImportPathCosting = Costing.Properties.Settings.Default.ImportPathCosting;

                    vmSettings.CostingServer = Costing.Properties.Settings.Default.CostingServer;
                    vmSettings.CostingDB = Costing.Properties.Settings.Default.CostingDB;

                    vmSettings.SysproServer = Costing.Properties.Settings.Default.SysproServer;
                    vmSettings.SysproDB = Costing.Properties.Settings.Default.SysproDB;

                    vmSettings.TempFolder = Costing.Properties.Settings.Default.TempFolder;
                    vmSettings.ImportPathWages = Costing.Properties.Settings.Default.ImportPathWages;
                    vmSettings.ImportPathCosting = Costing.Properties.Settings.Default.ImportPathCosting;
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Error compiling settings data: \n{ex.Message}");
                }
                finally
                {
                    Mouse.OverrideCursor = null;
                }
            }
        }

        private void btnBrowsePath_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new Microsoft.Win32.OpenFileDialog { Filter = "Excel Worksheets (*.xlsx)|*.xlsx", Title = "Select Wages File" };
            if (dialog.ShowDialog() == true) vmSettings.ImportPathWages = dialog.FileName;
        }

        private void btnBrowseCostingPath_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new Microsoft.Win32.OpenFileDialog { Filter = "Excel Worksheets (*.xlsx)|*.xlsx", Title = "Select Costing File" };
            if (dialog.ShowDialog() == true) vmSettings.ImportPathCosting = dialog.FileName;
        }

        private void btnSaveSettings_Click(object sender, RoutedEventArgs e)
        {
            if (vmSettings == null) return;
            try
            {
                Costing.Properties.Settings.Default.SysproServer = vmSettings.SysproServer;
                Costing.Properties.Settings.Default.SysproDB = vmSettings.SysproDB;
                Costing.Properties.Settings.Default.ImportPathWages = vmSettings.ImportPathWages;
                Costing.Properties.Settings.Default.ImportPathCosting = vmSettings.ImportPathCosting;

                Costing.Properties.Settings.Default.CostingServer = vmSettings.CostingServer;
                Costing.Properties.Settings.Default.CostingDB = vmSettings.CostingDB;

                Costing.Properties.Settings.Default.SysproServer = vmSettings.SysproServer;
                Costing.Properties.Settings.Default.SysproDB = vmSettings.SysproDB;

                Costing.Properties.Settings.Default.TempFolder = vmSettings.TempFolder;
                Costing.Properties.Settings.Default.ImportPathWages = vmSettings.ImportPathWages;
                Costing.Properties.Settings.Default.ImportPathCosting = vmSettings.ImportPathCosting;

                Costing.Properties.Settings.Default.Save();

                MessageBox.Show("Application configurations updated successfully!");
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Configuration commit aborted: \n{ex.Message}");
            }
        }
        #endregion

        #region Tab 2: Users Logic
        private async void FillUsers()
        {
            // Only query the database once. Let EF Core handle memory cache after that.
            if (vmSettings.OclUsers == null || !vmSettings.OclUsers.Any())
            {
                Mouse.OverrideCursor = Cursors.Wait;
                try
                {
                    await _context.LoginUsers.LoadAsync();

                    // Bind directly to the Local Cache tracked by EF Core
                    vmSettings.OclUsers = _context.LoginUsers.Local.ToObservableCollection();
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Error loading users: {ex.Message}");
                }
                finally
                {
                    Mouse.OverrideCursor = null;
                }
            }
        }

        private void btDeleteUser_Click(object sender, RoutedEventArgs e)
        {
            // VisualTreeHelper to find which row the clicked button belongs to
            Button bt = sender as Button;
            DependencyObject dpo = VisualTreeHelper.GetParent(bt);

            while (dpo != null && !(dpo is DataGridRow))
            {
                dpo = VisualTreeHelper.GetParent(dpo);
            }

            if (dpo is DataGridRow dgr && dgr.Item is LoginUser user)
            {
                // Removing from ObservableCollection automatically flags it for deletion in EF Core Local Cache
                vmSettings.OclUsers.Remove(user);
            }
        }

        private async void btnSaveUsers_Click(object sender, RoutedEventArgs e)
        {
            Mouse.OverrideCursor = Cursors.Wait;
            try
            {
                // Commits all Adds, Edits, and Deletes at once
                await _context.SaveChangesAsync();
                MessageBox.Show("All user changes saved successfully!", "Notification", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Failed to save users: \n{ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                Mouse.OverrideCursor = null;
            }
        }

        private void btnShowAddUserForm_Click(object sender, RoutedEventArgs e)
        {
            // Clear out any old text
            txtNewUserName.Text = "";
            txtNewPassword.Password = "";
            txtNewEmail.Text = "";
            cmbNewRole.SelectedIndex = 1; // Defaults to "Standard"

            // Show the popup
            pnlAddUser.Visibility = Visibility.Visible;
        }

        private void btnCancelAddUser_Click(object sender, RoutedEventArgs e)
        {
            // Hide the popup
            pnlAddUser.Visibility = Visibility.Collapsed;
        }

        private void btnConfirmAddUser_Click(object sender, RoutedEventArgs e)
        {
            string username = txtNewUserName.Text.Trim();
            string password = txtNewPassword.Password;
            string email = txtNewEmail.Text.Trim();
            string role = cmbNewRole.Text;

            if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(password))
            {
                MessageBox.Show("Username and Password are required fields.", "Validation Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (vmSettings.OclUsers.Any(u => u.UserName.Equals(username, StringComparison.OrdinalIgnoreCase)))
            {
                MessageBox.Show("This username already exists. Please choose another.", "Duplicate User", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var newUser = new LoginUser
            {
                UserName = username,
                Password = password,
                Email = email,
                Role = role
            };

            vmSettings.OclUsers.Add(newUser);

            pnlAddUser.Visibility = Visibility.Collapsed;
        }
        #endregion

        #region Tab 3: Work Centres Logic
        private async void FillWorkCentres()
        {
            if (vmSettings.OCWorkCentres == null || !vmSettings.OCWorkCentres.Any())
            {
                Mouse.OverrideCursor = Cursors.Wait;
                try
                {
                    // Load into EF Core Local tracking
                    await _context.WorkCentres.LoadAsync();
                    vmSettings.OCWorkCentres = _context.WorkCentres.Local.ToObservableCollection();
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Error loading Work Centres: {ex.Message}");
                }
                finally
                {
                    Mouse.OverrideCursor = null;
                }
            }
        }

        private async void btnSyncWorkCentres_Click(object sender, RoutedEventArgs e)
        {
            Mouse.OverrideCursor = Cursors.Wait;
            try
            {
                using (var sysContext = new SysproDBContext())
                {
                    // use LEFT JOIN to get the Cost Centre description from SYSPRO Cost Centre table
                    string sql = @"
                        SELECT 
                            w.WorkCentre AS WorkCentreCode, 
                            w.Description AS WorkCentreDesc, 
                            w.CostCentre AS CostCentreCode, 
                            c.Description AS CostCentreDesc 
                        FROM dbo.BomWorkCentre w
                        LEFT JOIN dbo.BomCostCentre c ON w.CostCentre = c.CostCentre";

                    var sysproData = await sysContext.SysWorkCentres.FromSqlRaw(sql).ToListAsync();

                    // Iterate and Sync
                    foreach (var sysWc in sysproData)
                    {
                        var localWc = vmSettings.OCWorkCentres.FirstOrDefault(w => w.WcCode == sysWc.WorkCentreCode);

                        if (localWc == null)
                        {
                            // Add missing SYSPRO Work Centre to local DB
                            vmSettings.OCWorkCentres.Add(new WorkCentre
                            {
                                WcCode = sysWc.WorkCentreCode,
                                WcDescription = sysWc.WorkCentreDesc,
                                CcCode = sysWc.CostCentreCode,
                                CostCentreDesc = sysWc.CostCentreDesc,
                                Staff = "0" 
                            });
                        }
                        else
                        {
                            // If it already exists, overwrite the descriptions in case SYSPRO was updated
                            localWc.WcDescription = sysWc.WorkCentreDesc;
                            localWc.CcCode = sysWc.CostCentreCode;
                            localWc.CostCentreDesc = sysWc.CostCentreDesc;
                        }
                    }
                }

                MessageBox.Show("SYSPRO Sync complete! \n\nPlease click 'Save Work Centres' to commit these updates to your local database.", "Sync Successful", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"SYSPRO Sync Failed: \n{ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                Mouse.OverrideCursor = null;
            }
        }

        private async void btnSaveWorkCentres_Click(object sender, RoutedEventArgs e)
        {
            Mouse.OverrideCursor = Cursors.Wait;
            try
            {
                // Commits the sync to the Costing DB
                await _context.SaveChangesAsync();
                MessageBox.Show("Work Centres saved successfully!", "Notification", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Failed to save Work Centres: \n{ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                Mouse.OverrideCursor = null;
            }
        }
        #endregion
    }
}