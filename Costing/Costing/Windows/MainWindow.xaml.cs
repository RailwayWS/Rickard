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
using Costing.UserControls;

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
    }
}