using Costing.Helpers;
using Costing.UserControls;
using System.Runtime.CompilerServices;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Xml.Serialization;

namespace Costing.Windows
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            ApplyRoleSecurity();
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
                btSettings.Visibility = Visibility.Collapsed;

                // TODO: Load the allocations view immediately
            }
            else
            {
                // ADMIN
                btStaffCosts.Visibility = Visibility.Visible;
                btStaff.Visibility = Visibility.Visible;
                btSettings.Visibility = Visibility.Visible;
            }
        }

        private void btAllocations_Click(object sender, RoutedEventArgs e)
        {
            //todo
        }
    }
}