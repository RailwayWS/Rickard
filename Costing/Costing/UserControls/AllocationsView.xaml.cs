using Costing.Data;
using Costing.Helpers;
using Costing.Models;
using Costing.Other; // To access your custom Message class
using Costing.Viewmodels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace Costing.UserControls
{
    public partial class AllocationsView : UserControl
    {
        #region Controls
        
        AllocationsViewModel vmAllocations = new AllocationsViewModel();
        
        #endregion

        public AllocationsView()
        {
            InitializeComponent();
            
            this.Loaded += Allocations_Loaded;
            DataContext = vmAllocations;
        }

        private void Allocations_Loaded(object sender, RoutedEventArgs e)
        {
            GetAllocationsData();
        }

        #region Get Data Methods
        
        private void GetAllocationsData()
        {
            Mouse.OverrideCursor = Cursors.Wait;

            try
            {
                DatabaseHelper.GetSysproData();

                using (var context = new CostingDbContext())
                {
                    // Load the dropdown options
                    var workCentres = context.WorkCentres.OrderBy(w => w.WcDescription).ToList();
                    vmAllocations.OCWorkCentres = new System.Collections.ObjectModel.ObservableCollection<WorkCentre>(workCentres);

                    // Load lists from the database
                    var allEmployees = context.Staff.ToList();
                    var existingAllocations = context.Allocations.ToList();

                    var displayList = new List<Allocation>();

                    foreach (var emp in allEmployees)
                    {
                        var savedAlloc = existingAllocations.FirstOrDefault(a => a.Code == emp.Code);

                        if (savedAlloc != null)
                        {
                            displayList.Add(savedAlloc);
                        }
                        else
                        {
                            displayList.Add(new Allocation
                            {
                                Code = emp.Code,
                                Name = emp.Name
                            });
                        }
                    }

                    // Bind it to the grid
                    vmAllocations.OCAllocations = new System.Collections.ObjectModel.ObservableCollection<Allocation>(displayList.OrderBy(a => a.Name));
                }
            }
            catch (Exception ex)
            {
                string errorMessage = ex.Message;
                if (ex.InnerException != null) errorMessage += "\n\nDetails: " + ex.InnerException.Message;

                Message errmess = new Message($"Error loading data: \n{errorMessage}");
                errmess.ShowDialog();
            }
            finally
            {
                Mouse.OverrideCursor = null;
            }
        }
        
        #endregion

        #region Save Method
        
        private async void btnSave_Click(object sender, RoutedEventArgs e)
        {
            if (vmAllocations == null || vmAllocations.OCAllocations == null) return;
            if (sender is Button btn) btn.IsEnabled = false;

            Mouse.OverrideCursor = Cursors.Wait;

            try
            {

                var uiData = vmAllocations.OCAllocations.ToList();

                await Task.Run(() =>
                {
                    using (var context = new CostingDbContext())
                    {
                        var allWorkCentres = context.WorkCentres.ToList();
                        var existingAllocations = context.Allocations.ToList();

                        foreach (var uiAllocation in uiData)
                        {
                            if (string.IsNullOrEmpty(uiAllocation.WorkCentre)) continue;

                            var dbRecord = context.Allocations.FirstOrDefault(a => a.Code == uiAllocation.Code);
                            var matchingWC = allWorkCentres.FirstOrDefault(w => w.WcCode == uiAllocation.WorkCentre);
                            string newCostCentre = matchingWC != null ? matchingWC.CcCode : null;

                            if (dbRecord != null)
                            {
                                dbRecord.WorkCentre = uiAllocation.WorkCentre;
                                dbRecord.CostCentre = newCostCentre;
                            }
                            else
                            {
                                context.Allocations.Add(new Allocation
                                {
                                    Code = uiAllocation.Code,
                                    Name = uiAllocation.Name,
                                    WorkCentre = uiAllocation.WorkCentre,
                                    CostCentre = newCostCentre
                                });
                            }
                        }

                        context.SaveChanges();
                    }
                });

                GetAllocationsData();

                Message msg = new Message("All employee allocations have been successfully saved!");
                msg.ShowDialog();
            }
            catch (Exception ex)
            {
                string errorMessage = ex.Message;
                if (ex.InnerException != null) errorMessage += "\n\n" + ex.InnerException.Message;

                Message errmess = new Message($"Error saving allocations: \n{errorMessage}");
                errmess.ShowDialog();
            }
            finally
            {
                Mouse.OverrideCursor = null;
            }
        }
        
        #endregion
    }
}