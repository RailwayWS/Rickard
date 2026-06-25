using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Costing.Viewmodels;

namespace Costing.UserControls
{
    public partial class SettingsView : UserControl
    {
        public SettingsView()
        {
            InitializeComponent();
            this.DataContext = new SettingsViewModel();
        }

        private void btnSaveSettings_Click(object sender, RoutedEventArgs e)
        {
            var vm = this.DataContext as SettingsViewModel;

            if (vm == null)
            {
                return;
            }

            Mouse.OverrideCursor = Cursors.Wait;

            try
            {
                // TODO
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error saving settings: \n\n" + ex.Message,
                                "Settings Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                Mouse.OverrideCursor = Cursors.Arrow;
            }
        }
    }
}