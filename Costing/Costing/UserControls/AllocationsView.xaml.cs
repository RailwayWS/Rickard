using Costing.Data;
using Costing.Models;
using Costing.Other;
using Costing.Viewmodels;
using Microsoft.EntityFrameworkCore;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace Costing.UserControls
{
    public partial class AllocationsView : UserControl
    {
        #region Fields

        private readonly AllocationsViewModel vmAllocations = new AllocationsViewModel();

        #endregion

        public AllocationsView()
        {
            InitializeComponent();
            DataContext = vmAllocations;
            this.Loaded += Allocations_Loaded;
        }

        private async void Allocations_Loaded(object sender, RoutedEventArgs e)
        {
            // gets data from work centres, staff and allocations
            await LoadDataAsync();
        }


        #region Data

        private async Task LoadDataAsync()
        {
            Mouse.OverrideCursor = Cursors.Wait;

            try
            {
                using (var context = new CostingDbContext())
                {
                    var workCentres = await context.WorkCentres
                        .OrderBy(w => w.WcDescription)
                        .ToListAsync();

                    vmAllocations.OCWorkCentres =
                        new System.Collections.ObjectModel.ObservableCollection<WorkCentre>(workCentres);

                    var allEmployees = await context.Staff
                        .OrderBy(s => s.Name)
                        .ToListAsync();

                    var allAllocations = await context.Allocations.ToListAsync();

                    var summaryList = new List<EmployeeAllocationSummary>();

                    foreach (var emp in allEmployees)
                    {
                        var empRows = allAllocations
                            .Where(a => a.Code == emp.Code)
                            .ToList();

                        summaryList.Add(BuildSummary(emp.Code, emp.Name, empRows, workCentres));
                    }

                    vmAllocations.OCAllocations =
                        new System.Collections.ObjectModel.ObservableCollection<EmployeeAllocationSummary>(summaryList);
                }
            }
            catch (Exception ex)
            {
                ShowError("Error loading data", ex);
            }
            finally
            {
                Mouse.OverrideCursor = null;
            }
        }

        private async Task SaveEmployeeAllocationsAsync(
            string employeeCode,
            string employeeName,
            List<AllocationRow> newRows,
            List<WorkCentre> workCentres)
        {
            Mouse.OverrideCursor = Cursors.Wait;

            try
            {
                using (var context = new CostingDbContext())
                {
                    var existing = await context.Allocations
                        .Where(a => a.Code == employeeCode)
                        .ToListAsync();

                    context.Allocations.RemoveRange(existing);

                    foreach (var row in newRows)
                    {
                        if (string.IsNullOrEmpty(row.WorkCentre) || row.Portion <= 0) continue;

                        var matchingWC = workCentres.FirstOrDefault(w => w.WcCode == row.WorkCentre);

                        context.Allocations.Add(new Allocation
                        {
                            Code = employeeCode,
                            Name = employeeName,
                            WorkCentre = row.WorkCentre,
                            CostCentre = matchingWC?.CcCode,
                            Portion = row.Portion
                        });
                    }

                    await context.SaveChangesAsync();
                }

                await RefreshSingleRowAsync(employeeCode);

                Message msg = new Message($"Allocations for {employeeName} saved successfully!");
                Mouse.OverrideCursor = null;
                msg.ShowDialog();
            }
            catch (Exception ex)
            {
                ShowError("Error saving allocations", ex);
            }
            finally
            {

            }
        }

        private async Task RefreshSingleRowAsync(string employeeCode)
        {
            try
            {
                using (var context = new CostingDbContext())
                {
                    var emp = await context.Staff.FirstOrDefaultAsync(s => s.Code == employeeCode);
                    if (emp == null) return;

                    var rows = await context.Allocations
                        .Where(a => a.Code == employeeCode)
                        .ToListAsync();

                    var workCentres = vmAllocations.OCWorkCentres.ToList();
                    var updated = BuildSummary(emp.Code, emp.Name, rows, workCentres);

                    var existing = vmAllocations.OCAllocations.FirstOrDefault(s => s.Code == employeeCode);
                    if (existing != null)
                    {
                        existing.AllocationSummary = updated.AllocationSummary;
                        existing.TotalPortion = updated.TotalPortion;
                    }
                }
            }
            catch
            {
                await LoadDataAsync();
            }
        }

        #endregion

        #region Buttons

        private async void BtnAssign_Click(object sender, RoutedEventArgs e)
        {
            if (sender is not Button btn) return;
            if (btn.Tag is not EmployeeAllocationSummary summary) return;

            List<Allocation> existingRows;
            List<WorkCentre> workCentres;

            Mouse.OverrideCursor = Cursors.Wait;
            try
            {
                using (var context = new CostingDbContext())
                {
                    existingRows = await context.Allocations
                        .Where(a => a.Code == summary.Code)
                        .ToListAsync();

                    workCentres = await context.WorkCentres
                        .OrderBy(w => w.WcDescription)
                        .ToListAsync();
                }
            }
            catch (Exception ex)
            {
                ShowError("Error loading employee allocations", ex);
                return;
            }
            finally
            {
                Mouse.OverrideCursor = null;
            }

            var dialog = new AllocationDialog(summary.Code, summary.Name, existingRows)
            {
                WorkCentres = workCentres,
                Owner = Window.GetWindow(this)
            };

            bool? result = dialog.ShowDialog();
            if (result != true) return;

            await SaveEmployeeAllocationsAsync(summary.Code, summary.Name, dialog.ResultRows, workCentres);
        }

        #endregion

        #region Helpers

        private static EmployeeAllocationSummary BuildSummary(
            string code,
            string name,
            List<Allocation> rows,
            List<WorkCentre> workCentres)
        {
            decimal total = rows.Sum(r => r.Portion ?? 0m);

            string summary;
            if (!rows.Any() || rows.All(r => string.IsNullOrEmpty(r.WorkCentre)))
            {
                summary = "Not assigned";
            }
            else
            {
                summary = string.Join("  |  ", rows
                    .Where(r => !string.IsNullOrEmpty(r.WorkCentre))
                    .Select(r =>
                    {
                        var wc = workCentres.FirstOrDefault(w => w.WcCode == r.WorkCentre);
                        string label = wc != null ? wc.WcDescription : r.WorkCentre;
                        decimal portion = r.Portion ?? 0m;
                        return $"{label} ({portion:0%})";
                    }));
            }

            return new EmployeeAllocationSummary
            {
                Code = code,
                Name = name,
                AllocationSummary = summary,
                TotalPortion = total
            };
        }


        private static void ShowError(string title, Exception ex)
        {
            string msg = ex.Message;
            if (ex.InnerException != null) msg += "\n\nDetails: " + ex.InnerException.Message;

            Message errmess = new Message($"{title}:\n{msg}");
            errmess.ShowDialog();
        }
    }

        #endregion
}