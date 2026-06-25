using Costing.Data;
using Costing.Helpers;
using Costing.Models;
using Costing.Viewmodels;
using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace Costing.UserControls
{
    public partial class AllocationsView : UserControl
    {
        private AllocationsViewModel _viewModel;

        public AllocationsView()
        {
            InitializeComponent();
            _viewModel = new AllocationsViewModel();
            this.DataContext = _viewModel;
            this.Loaded += UserControl_Loaded;
        }

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            Mouse.OverrideCursor = Cursors.Wait;

            try
            {
                DatabaseHelper.GetSysproData();
                _viewModel.LoadData();
            }
            catch (Exception ex)
            {
                string errorMessage = ex.Message;
                if (ex.InnerException != null) errorMessage += "\n\nDetails: " + ex.InnerException.Message;

                MessageBox.Show("Error loading data: \n" + errorMessage, "Database Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                Mouse.OverrideCursor = null;
            }
        }

        private void WorkCentre_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            // If a Work Centre is chosen update the Cost Centre
            if (sender is ComboBox comboBox &&
                comboBox.SelectedItem is Costing.Models.WorkCentre selectedWc &&
                comboBox.DataContext is Costing.Models.Allocation currentRow)
            {
                currentRow.CostCentre = selectedWc.CcCode;
            }
        }

        private void btnSave_Click(object sender, RoutedEventArgs e)
        {
            if (_viewModel == null || _viewModel.OCAllocations == null) return;

            Mouse.OverrideCursor = Cursors.Wait;

            try
            {
                using (var context = new CostingDbContext())
                {
                    var allWorkCentres = context.WorkCentres.ToList();

                    foreach (var uiAllocation in _viewModel.OCAllocations)
                    {
                        // Skip people who haven't been assigned a work centre yet
                        if (string.IsNullOrEmpty(uiAllocation.WorkCentre)) continue;

                        var dbRecord = context.Allocations.FirstOrDefault(a => a.Code == uiAllocation.Code);

                        var matchingWC = allWorkCentres.FirstOrDefault(w => w.WcCode == uiAllocation.WorkCentre);
                        string newCostCentre = matchingWC != null ? matchingWC.CcCode : null;

                        if (dbRecord != null)
                        {
                            // UPDATE
                            dbRecord.WorkCentre = uiAllocation.WorkCentre;
                            dbRecord.CostCentre = newCostCentre;
                        }
                        else
                        {
                            // INSERT
                            context.Allocations.Add(new Allocation
                            {
                                Code = uiAllocation.Code,
                                Name = uiAllocation.Name,
                                WorkCentre = uiAllocation.WorkCentre,
                                CostCentre = newCostCentre
                            });
                        }
                    }

                    // Save everything at once
                    context.SaveChanges();
                }
                _viewModel.LoadData();
                MessageBox.Show("All employee allocations have been successfully saved!", "Save Complete", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                string errorMessage = ex.Message;
                if (ex.InnerException != null) errorMessage += "\n\n" + ex.InnerException.Message;

                MessageBox.Show("Error saving allocations: \n" + errorMessage, "Save Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                Mouse.OverrideCursor = null;
            }
        }
    }
}