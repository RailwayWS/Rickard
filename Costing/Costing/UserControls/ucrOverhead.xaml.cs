using Costing.Viewmodels;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace Costing.UserControls
{
    public partial class ucrOverheads : UserControl
    {
        public ucrOverheads()
        {
            InitializeComponent();
            this.Loaded += ucrOverheads_Loaded;
        }

        private async void ucrOverheads_Loaded(object sender, RoutedEventArgs e)
        {
            Mouse.OverrideCursor = Cursors.Wait;
            try
            {
                await GetCurrentPeriod();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error getting SYSPRO data: \n{ex.Message}", "Warning", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
            finally
            {
                Mouse.OverrideCursor = null;
            }
        }

        private async Task GetCurrentPeriod()
        {
            var vm = this.DataContext as OverheadViewModel;
            if (vm == null) return;

            // Fetch periods
            var periods = await Helpers.DatabaseHelper.GetSysproPeriodsAsync();

            if (periods.Any())
            {
                txtGLYear.Text = periods.First().GlYear.ToString();

                vm.OCLPeriods.Clear();
                foreach (var p in periods)
                {
                    vm.OCLPeriods.Add(p);
                }

                // Select the highest/current period by default, which triggers the data load
                int maxPeriod = periods.Max(x => x.GlPeriod);
                vm.SelectedPeriod = vm.OCLPeriods.FirstOrDefault(x => x.GlPeriod == maxPeriod);
            }
        }

        private async void cmbGLPeriod_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var vm = this.DataContext as OverheadViewModel;
            if (vm == null || vm.SelectedPeriod == null) return;

            Mouse.OverrideCursor = Cursors.Wait;
            try
            {
                int year = Convert.ToInt32(txtGLYear.Text);
                int selectedPeriod = vm.SelectedPeriod.GlPeriod;

                // Fetch Accounts
                var accounts = await Helpers.DatabaseHelper.GetOverheadAccountsAsync(year, selectedPeriod);

                vm.OCLGlAccounts.Clear();
                foreach (var acc in accounts)
                {
                    vm.OCLGlAccounts.Add(acc);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error fetching overhead accounts: \n{ex.Message}", "Database Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                Mouse.OverrideCursor = null;
            }
        }
    }
}