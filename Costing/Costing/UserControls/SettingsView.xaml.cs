using Costing.Other;
using Costing.Viewmodels;
using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace Costing.UserControls
{
    public partial class SettingsView : UserControl
    {
        #region Controls

        SettingsViewModel vmSettings = new SettingsViewModel();

        #endregion

        public SettingsView()
        {
            InitializeComponent();

            this.Loaded += Settings_Loaded;
            DataContext = vmSettings;
        }

        private void Settings_Loaded(object sender, RoutedEventArgs e)
        {
            LoadApplicationSettings();
        }

        #region Get Data Methods

        private void LoadApplicationSettings()
        {
            Mouse.OverrideCursor = Cursors.Wait;

            try
            {
                // Pull directly from the global system config context
                vmSettings.SysproServer = Costing.Properties.Settings.Default.SysproServer;
                vmSettings.SysproDB = Costing.Properties.Settings.Default.SysproDB;

                // Pulls path variables mapping securely
                vmSettings.ImportPathWages = Costing.Properties.Settings.Default.ImportPathWages;
                vmSettings.ImportPathCosting = Costing.Properties.Settings.Default.ImportPathCosting;
            }
            catch (Exception ex)
            {
                Message errmess = new Message($"Error compiling settings data: \n{ex.Message}");
                errmess.ShowDialog();
            }
            finally
            {
                Mouse.OverrideCursor = Cursors.Arrow;
            }
        }

        #endregion

        #region Operational UI Actions

        private void btnBrowsePath_Click(object sender, RoutedEventArgs e)
        {
            // Simple Win32 file selector mapping to make path assignment effortless
            var dialog = new Microsoft.Win32.OpenFileDialog
            {
                Filter = "Excel Worksheets (*.xlsx)|*.xlsx",
                Title = "Select Default Wages Spreadsheet Reference Source"
            };

            if (dialog.ShowDialog() == true)
            {
                vmSettings.ImportPathWages = dialog.FileName;
            }
        }

        private void btnBrowseCostingPath_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new Microsoft.Win32.OpenFileDialog
            {
                Filter = "Excel Worksheets (*.xlsx)|*.xlsx",
                Title = "Select Default Costing Sheet Reference Source"
            };

            if (dialog.ShowDialog() == true)
            {
                vmSettings.ImportPathCosting = dialog.FileName;
            }
        }

        #endregion

        #region Save Method

        private void btnSaveSettings_Click(object sender, RoutedEventArgs e)
        {
            if (vmSettings == null) return;

            try
            {
                // Write the local UI state values directly back to application configuration context
                Costing.Properties.Settings.Default.SysproServer = vmSettings.SysproServer;
                Costing.Properties.Settings.Default.SysproDB = vmSettings.SysproDB;
                Costing.Properties.Settings.Default.ImportPathWages = vmSettings.ImportPathWages;
                Costing.Properties.Settings.Default.ImportPathCosting = vmSettings.ImportPathCosting;

                // Commit variables into localized permanent machine runtime state storage
                Costing.Properties.Settings.Default.Save();

                Message msg = new Message("Application configurations updated successfully!");
                msg.ShowDialog();
            }
            catch (Exception ex)
            {
                Message errmess = new Message($"Configuration commit execution aborted: \n{ex.Message}");
                errmess.ShowDialog();
            }
        }

        #endregion
    }
}