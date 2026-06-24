using Costing.Data;
using Costing.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

namespace Costing.Helpers
{
    public class DatabaseHelper
    {
        public static void SaveStaffToDatabase(IEnumerable<BasicEmployee> staffList)
        {
            // using opens and closes the db connection
            using (var db = new CostingDbContext()) 
            {
                foreach (var emp in staffList) {
                    //look for emp in db
                    var existingEmployee = db.Staff.FirstOrDefault(e => e.Code == emp.Code);

                    if (existingEmployee != null) 
                    { 
                        // if exists, update
                        existingEmployee.Name = emp.Name;
                        existingEmployee.CostCentre = emp.CostCentre;
                        existingEmployee.JobDescription = emp.JobDescription;
                        existingEmployee.Rate = emp.Rate;
                    }
                    // add when they dont exist
                    else { db.Staff.Add(emp); }
                }

                // add all changes at once
                db.SaveChanges();
            }
        }

        public static void SaveStaffCostsToDatabase(IEnumerable<StaffCost> staffCostsList)
        {
            using (var db = new CostingDbContext())
            {
                foreach (var cost in staffCostsList)
                {
                    var existingCost = db.StaffCosts.FirstOrDefault(c => c.Id == cost.Id && cost.Id != 0);

                    if (existingCost != null)
                    {
                        // If it exists, UPDATE the type and value
                        existingCost.Category = cost.Category;
                        existingCost.Type = cost.Type;
                        existingCost.Value = cost.Value;
                        existingCost.BaseCategory = cost.BaseCategory;
                    }
                    else
                    {
                        // If it does not exist, INSERT it as a new row.
                        var newCost = new StaffCost
                        {
                            Category = cost.Category,
                            Type = cost.Type,
                            Value = cost.Value,
                            BaseCategory = cost.BaseCategory
                        };
                        db.StaffCosts.Add(newCost);
                    }
                }

                // save all changes
                db.SaveChanges();
            }
        }

        public static List<StaffCost> GetAllStaffCosts()
        {
            using ( var db = new CostingDbContext()){
                return db.StaffCosts.ToList();
            }
        }

        public static void DeleteStaffCostFromDatabase(StaffCost costToDelete)
        {
            using (var db = new CostingDbContext())
            {
                var existingCost = db.StaffCosts.FirstOrDefault(c => c.Category == costToDelete.Category);

                if(existingCost != null)
                {
                    db.StaffCosts.Remove(existingCost);
                    db.SaveChanges();
                }
            }
        }

        public static void SaveCalculatedStaffToDatabase(IEnumerable<CalculatedStaff> calculatedList)
        {
            using (var db = new CostingDbContext())
            {
                #region rogue vals
                // find values in costing thats not in wages ( will probably remove this later )
                var validCodes = db.Staff.Select(e => e.Code.Trim()).ToList();

                var rogueRecords = calculatedList.Where(c => string.IsNullOrWhiteSpace(c.Code) || !validCodes.Contains(c.Code.Trim())).ToList();

                if (rogueRecords.Any())
                {
                    var badCodes = rogueRecords.Select(r =>
                        string.IsNullOrWhiteSpace(r.Code) ? "[BLANK CODE - Check Excel for ghost rows]" : r.Code.Trim()
                    ).Distinct().ToList();

                    string missingList = string.Join("\n", badCodes);

                    System.Windows.MessageBox.Show(
                        "Cannot save to database! The following Employee Codes exist in the Excel calculation, but do NOT exist in your master Staff list. \n\nPlease add them to the Staff Management screen first:\n\n" + missingList,
                        "Missing Employees Detected",
                        System.Windows.MessageBoxButton.OK,
                        System.Windows.MessageBoxImage.Error);

                    return;
                }
                #endregion

                foreach (var calc in calculatedList)
                {
                    // find parent
                    var existingRecord = db.CalculatedStaffRecords.Include(c => c.DynamicCosts).FirstOrDefault(c => c.Code == calc.Code && c.WorkCentre == calc.WorkCentre);

                    if (existingRecord != null)
                    {
                        //  UPDATE parent fields
                        existingRecord.Name = calc.Name;
                        existingRecord.WorkCentre = calc.WorkCentre;
                        existingRecord.RatePerHour = calc.RatePerHour;
                        existingRecord.Allocation = calc.Allocation;
                        existingRecord.Efficiency = calc.Efficiency;
                        existingRecord.Rate = calc.Rate;
                        existingRecord.Total = calc.Total;


                        //Explicitly tell the specific DbSet to delete the old children
                        if (existingRecord.DynamicCosts != null && existingRecord.DynamicCosts.Any())
                        {
                            db.CalculatedStaffCosts.RemoveRange(existingRecord.DynamicCosts.ToList());
                        }

                        var newCosts = calc.DynamicCosts.Select(row => new CalculatedStaffCost
                        {
                            CategoryName = row.CategoryName,
                            Amount = row.Amount,
                            CalculatedStaffId = existingRecord.Id // Link to parent
                        });

                        // Pass the whole list to the DbSet at once
                        db.CalculatedStaffCosts.AddRange(newCosts);
                    }
                    else
                    {
                        // INSERT
                        db.CalculatedStaffRecords.Add(calc);
                    }
                }

                db.SaveChanges();
            }
        }
    }
}
