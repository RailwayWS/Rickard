using Costing.Helpers;
using Costing.Models;
using Costing.Viewmodels;
using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Costing.UserControls
{
    /// <summary>
    /// Interaction logic for StaffCostsView.xaml
    /// </summary>
    public partial class StaffCostsView : UserControl
    {
        public StaffCostsView()
        {
            InitializeComponent();

        }

        private bool _isAddingNew = false;

        public void btnBack_Click(object sender, RoutedEventArgs e)
        {
            var mainWindow = Window.GetWindow(this) as Costing.Windows.MainWindow;
            mainWindow.ShowMainMenu();
        }

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            var vm = this.DataContext as StaffCostsViewModel;
            if (vm == null) return;

            Mouse.OverrideCursor = Cursors.Wait;

            try
            {
                vm.OCStaffCosts.Clear();

                // Fetch directly from DB and load into UI
                var savedDbCosts = Helpers.DatabaseHelper.GetAllStaffCosts();

                foreach (var cost in savedDbCosts)
                {
                    vm.OCStaffCosts.Add(cost);
                }

                vm.SelectedStaffCost = vm.OCStaffCosts.FirstOrDefault();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error loading Staff Costs from database: \n\n" + ex.Message, "Database Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                Mouse.OverrideCursor = null;
            }

            RefreshBaseCategoryCheckboxes();
        }


        private void btnAdd_Click(object sender, RoutedEventArgs e)
        {
            _isAddingNew = true;
            txtFormTitle.Text = "Add New Category";

            // Clear the form to make everything blank
            txtFormCategory.Text = "";
            cmbFormType.Text = "";
            txtFormValue.Text = "0";

            // Show the form
            FormPanel.Visibility = Visibility.Visible;
        }

        private void btnEdit_Click(object sender, RoutedEventArgs e)
        {
            var vm = this.DataContext as StaffCostsViewModel;
            if (vm == null || vm.SelectedStaffCost == null)
            {
                MessageBox.Show("Please select a category to edit first.", "No Selection", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            _isAddingNew = false;
            txtFormTitle.Text = "Edit Category";

            // Load the currently selected values into the form
            txtFormCategory.Text = vm.SelectedStaffCost.Category;
            cmbFormType.Text = vm.SelectedStaffCost.Type;
            txtFormValue.Text = vm.SelectedStaffCost.Value.ToString("0.#####");


            string existingBases = vm.SelectedStaffCost.BaseCategory ?? "";
            var basesList = existingBases.Split(',').Select(s => s.Trim()).ToList();

            foreach (CheckBox cb in pnlBaseCategories.Children)
            {
                // Tick the box if its name exists in the database string
                cb.IsChecked = basesList.Contains(cb.Tag.ToString());
            }

            // Show the form
            FormPanel.Visibility = Visibility.Visible;
        }

        private void btnDelete_Click(object sender, RoutedEventArgs e)
        {
            var vm = this.DataContext as StaffCostsViewModel;

            if (vm == null || vm.SelectedStaffCost == null)
            {
                MessageBox.Show("Please select a category to delete first.", "No Selection", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            var costToDelete = vm.SelectedStaffCost;

            var result = MessageBox.Show($"Are you sure you want to permanently delete '{costToDelete.Category}'?",
                                         "Confirm Delete", MessageBoxButton.YesNo, MessageBoxImage.Warning);
            if (result == MessageBoxResult.Yes)
            {
                try
                {
                    // Remove from DB
                    Helpers.DatabaseHelper.DeleteStaffCostFromDatabase(costToDelete);

                    // Remove from UI 
                    vm.OCStaffCosts.Remove(costToDelete);

                    RefreshBaseCategoryCheckboxes();

                    // Select the next available item and hide the form
                    vm.SelectedStaffCost = vm.OCStaffCosts.FirstOrDefault();
                    FormPanel.Visibility = Visibility.Collapsed;
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Error deleting from database: \n\n" + ex.Message,
                                    "Database Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private void btnCancelForm_Click(object sender, RoutedEventArgs e)
        {
            FormPanel.Visibility = Visibility.Collapsed;
        }

        private void btnConfirmForm_Click(object sender, RoutedEventArgs e)
        {
            var vm = this.DataContext as StaffCostsViewModel;
            if (vm == null) return;

            string cleanValue = txtFormValue.Text.Replace(",", ".");
            if (!decimal.TryParse(cleanValue, NumberStyles.Any, CultureInfo.InvariantCulture, out decimal parsedValue))
            {
                MessageBox.Show("Please enter a valid number for the Cost Value.", "Invalid Input", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var checkedNames = new List<string>();
            foreach (CheckBox cb in pnlBaseCategories.Children)
            {
                if (cb.IsChecked == true)
                {
                    checkedNames.Add(cb.Tag.ToString());
                }
            }
            string finalBaseCategoryString = string.Join(",", checkedNames);

            if (_isAddingNew)
            {
                // ADD LOGIC
                int newId = vm.OCStaffCosts.Any() ? vm.OCStaffCosts.Max(x => x.Id) + 1 : 1;
                var newCost = new StaffCost
                {
                    Id = newId,
                    Category = txtFormCategory.Text,
                    Type = cmbFormType.Text,
                    Value = parsedValue,
                    BaseCategory = finalBaseCategoryString
                };

                vm.OCStaffCosts.Add(newCost);
                vm.SelectedStaffCost = newCost;
            }
            else
            {
                // EDIT LOGIC
                vm.SelectedStaffCost.Category = txtFormCategory.Text;
                vm.SelectedStaffCost.Type = cmbFormType.Text;
                vm.SelectedStaffCost.Value = parsedValue;
                vm.SelectedStaffCost.BaseCategory = finalBaseCategoryString;

                // Force UI refresh
                var tempItem = vm.SelectedStaffCost;
                vm.SelectedStaffCost = null;
                vm.SelectedStaffCost = tempItem;
            }

            // Auto-Save to Database immediately after adding/editing
            try
            {
                Helpers.DatabaseHelper.SaveStaffCostsToDatabase(vm.OCStaffCosts);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Values were updated on screen but failed to save to the database:\n\n" + ex.Message, "Database Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }

            // Hide the form after a successful apply
            FormPanel.Visibility = Visibility.Collapsed;

            RefreshBaseCategoryCheckboxes();
        }

        private void RefreshBaseCategoryCheckboxes()
        {
            pnlBaseCategories.Children.Clear();

            var dbCosts = Costing.Helpers.DatabaseHelper.GetAllStaffCosts();
            var categoryNames = dbCosts.Select(c => c.Category).ToList();

            categoryNames.Insert(0, "Allocation");
            categoryNames.Insert(0, "RatePerHour");
            categoryNames.Insert(0, "Base Rate");

            foreach (var name in categoryNames)
            {
                var cb = new CheckBox
                {
                    Content = name,
                    Foreground = new System.Windows.Media.SolidColorBrush((System.Windows.Media.Color)System.Windows.Media.ColorConverter.ConvertFromString("#CBD5E1")), // Made this lighter for your dark theme!
                    Margin = new Thickness(0, 0, 15, 8),
                    Tag = name
                };

                pnlBaseCategories.Children.Add(cb);
            }
        }

        // This runs every time a user ticks or unticks a box


        private void cmbFormType_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (cmbFormType.SelectedItem != null)
            {
                string selectedType = cmbFormType.SelectedItem.ToString();

                if (selectedType == "Fixed")
                {
                    foreach (CheckBox cb in pnlBaseCategories.Children)
                    {
                        cb.IsChecked = false;
                    }
                    pnlBaseCategories.IsEnabled = false;
                }
                else
                {
                    pnlBaseCategories.IsEnabled = true;
                }
            }
        }

    }
}