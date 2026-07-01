using Costing.Helpers;
using Costing.UserControls;
using System.Windows;
using System.Windows.Input;

namespace Costing.Windows
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            ApplyRoleSecurity();
            MainFrame.Content = new UserControls.AllocationsView();
        }

        private void btStaffCosts_Click(object sender, RoutedEventArgs e)
        {

            MainMenuSidebar.Visibility = Visibility.Collapsed;
            SecondarySidebarFrame.Visibility = Visibility.Visible;

            SecondarySidebarFrame.Content = new StaffCostsView();

            MainFrame.Content = new Costing.UserControls.CalculatedStaffView();
        }

        public void ShowMainMenu()
        {
            SecondarySidebarFrame.Visibility = Visibility.Collapsed;
            SecondarySidebarFrame.Content = null;
            MainMenuSidebar.Visibility = Visibility.Visible;

            MainFrame.Content = null;
        }

        private void btStaff_Click(object sender, RoutedEventArgs e)
        {
            MainFrame.Content = new StaffView();
        }

        private void btLogout_Click(object sender, RoutedEventArgs e)
        {
            Mouse.OverrideCursor = Cursors.Hand;
            Application.Current.Shutdown();
        }

        private void ApplyRoleSecurity()
        {
            // if somehow no session exists, boot them out or default to locked down
            if (AppSession.CurrentUser == null)
            {
                MessageBox.Show("No active user session found. Closing application.", "Security Error", MessageBoxButton.OK, MessageBoxImage.Error);
                this.Close();
                return;
            }

            if (!AppSession.IsAdmin)
            {
                // BASIC USER
                btStaffCosts.Visibility = Visibility.Collapsed;
                btStaff.Visibility = Visibility.Collapsed;
                btAuditLogs.Visibility = Visibility.Collapsed;
                btSettings.Visibility = Visibility.Collapsed;
                btOverheads.Visibility = Visibility.Collapsed;

            }
            else
            {
                // ADMIN
                btStaffCosts.Visibility = Visibility.Visible;
                btStaff.Visibility = Visibility.Visible;
                btAuditLogs.Visibility = Visibility.Visible;
                btOverheads.Visibility = Visibility.Visible;
                btSettings.Visibility = Visibility.Visible;
            }
        }

        private void btAllocations_Click(object sender, RoutedEventArgs e)
        {
            MainFrame.Content = new UserControls.AllocationsView();
        }


        private void btSettings_Click(object sender, RoutedEventArgs e)
        {
            MainFrame.Content = new Costing.UserControls.SettingsView();
        }

        private void btAuditLogs_Click(object sender, RoutedEventArgs e)
        {
            MainFrame.Content = new UserControls.AuditLogsView();
        }

        private void btOverheads_Click(object sender, RoutedEventArgs e)
        {
            MainFrame.Content = new Costing.UserControls.ucrOverheads();
        }

        public void ShowSettingsView()
        {
            this.MainFrame.Content = new Costing.UserControls.SettingsView();
            btSettings.IsChecked = true;
        }
    }
}