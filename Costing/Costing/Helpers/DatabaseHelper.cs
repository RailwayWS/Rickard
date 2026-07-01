using Costing.Data;
using Costing.Models;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;


namespace Costing.Helpers
{
    public class DatabaseHelper
    {

        #region Staff saving and deleting
        public static async Task<List<string>> GetEmployeesToDeleteAsync(IEnumerable<BasicEmployee> staffList)
        {
            using (var db = new CostingDbContext())
            {
                var excelCodes = staffList.Select(e => e.Code).ToList();

                var allDbStaff = await db.Staff.ToListAsync();

                var toDelete = allDbStaff.Where(dbEmp => !excelCodes.Contains(dbEmp.Code)).ToList();

                return toDelete.Select(e => $"{e.Code} - {e.Name}").ToList();
            }
        }

        public static async Task SaveStaffToDatabaseAsync(IEnumerable<BasicEmployee> staffList)
        {
            using (var db = new CostingDbContext())
            {
                // Upsert
                var incomingCodes = staffList.Select(e => e.Code).ToList();
                var existingStaff = await db.Staff
                    .Where(e => incomingCodes.Contains(e.Code))
                    .ToListAsync();

                foreach (var emp in staffList)
                {
                    var existingEmployee = existingStaff.FirstOrDefault(e => e.Code == emp.Code);

                    if (existingEmployee != null)
                    {
                        existingEmployee.Name = emp.Name;
                        existingEmployee.CostCentre = emp.CostCentre;
                        existingEmployee.JobDescription = emp.JobDescription;
                        existingEmployee.Rate = emp.Rate;
                    }
                    else
                    {
                        db.Staff.Add(emp);
                    }
                }

                var allDbStaff = await db.Staff.ToListAsync();
                var toDelete = allDbStaff.Where(dbEmp => !incomingCodes.Contains(dbEmp.Code)).ToList();

                if (toDelete.Any())
                {
                    var codesToDelete = toDelete.Select(e => e.Code).ToList();

                    using var transaction = await db.Database.BeginTransactionAsync();
                    try
                    {
                        foreach (var oldEmp in toDelete)
                        {
                            // Remove Allocations
                            var orphanedAllocations = db.Allocations.Where(a => a.Code == oldEmp.Code).ToList();
                            if (orphanedAllocations.Any()) db.Allocations.RemoveRange(orphanedAllocations);

                            // Remove Calculated Costs
                            var parentIds = db.CalculatedStaffRecords
                                              .Where(r => r.Code == oldEmp.Code)
                                              .Select(r => r.Id)
                                              .ToList();

                            var orphanedCosts = db.CalculatedStaffCosts
                                                  .Where(cost => parentIds.Contains(cost.CalculatedStaffId))
                                                  .ToList();
                            if (orphanedCosts.Any()) db.CalculatedStaffCosts.RemoveRange(orphanedCosts);

                            // Remove Calculated Records
                            var orphanedCalculations = db.CalculatedStaffRecords
                                                         .Where(c => c.Code == oldEmp.Code)
                                                         .ToList();
                            if (orphanedCalculations.Any()) db.CalculatedStaffRecords.RemoveRange(orphanedCalculations);

                            // AuditLog rows are intentionally NOT removed
                        }

                        await db.SaveChangesAsync();

                        foreach (var oldEmp in toDelete)
                        {
                            db.Staff.Remove(oldEmp);
                        }

                        await db.SaveChangesAsync();
                        await transaction.CommitAsync();
                    }
                    catch
                    {
                        await transaction.RollbackAsync();
                        throw;
                    }
                }
                else
                {
                    await db.SaveChangesAsync();
                }
            }
        }

        #endregion

        #region save calculations functions

        public static async Task SaveStaffCostsToDatabaseAsync(IEnumerable<StaffCost> staffCostsList)
        {
            using (var db = new CostingDbContext())
            {
                foreach (var cost in staffCostsList)
                {
                    var existingCost = await db.StaffCosts.FirstOrDefaultAsync(c => c.Id == cost.Id && cost.Id != 0);

                    if (existingCost != null)
                    {
                        // If it exists, UPDATE the type and value
                        existingCost.Category = cost.Category;
                        existingCost.Type = cost.Type;
                        existingCost.Value = cost.Value;
                        existingCost.BaseCategory = cost.BaseCategory;
                        existingCost.MaxLimit = cost.MaxLimit;
                    }
                    else
                    {
                        // If it does not exist, INSERT it as a new row.
                        var newCost = new StaffCost
                        {
                            Category = cost.Category,
                            Type = cost.Type,
                            Value = cost.Value,
                            BaseCategory = cost.BaseCategory,
                            MaxLimit = cost.MaxLimit
                        };
                        db.StaffCosts.Add(newCost);
                    }
                }

                // save all changes
                await db.SaveChangesAsync();
            }
        }

        public static async Task<List<StaffCost>> GetAllStaffCostsAsync()
        {
            using (var db = new CostingDbContext())
            {
                return await db.StaffCosts.ToListAsync();
            }
        }

        public static async Task DeleteStaffCostFromDatabaseAsync(StaffCost costToDelete)
        {
            using (var db = new CostingDbContext())
            {
                var existingCost = await db.StaffCosts.FirstOrDefaultAsync(c => c.Category == costToDelete.Category);

                if (existingCost != null)
                {
                    db.StaffCosts.Remove(existingCost);
                    await db.SaveChangesAsync();
                }
            }
        }

        public static async Task SaveCalculatedStaffToDatabaseAsync(IEnumerable<CalculatedStaff> calculatedList)
        {
            using (var db = new CostingDbContext())
            {

                foreach (var calc in calculatedList)
                {
                    // find parent
                    var existingRecord = await db.CalculatedStaffRecords
                        .Include(c => c.DynamicCosts)
                        .FirstOrDefaultAsync(c => c.Code == calc.Code && c.WorkCentre == calc.WorkCentre);

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

                await db.SaveChangesAsync();
            }
        }

        #endregion

        #region Get cost/work centres from syspro

        public static async Task GetSysproDataAsync()
        {
            var newCostCentres = new List<CostCentre>();
            var newWorkCentres = new List<WorkCentre>();

            string sysproConnStr = $"Server={Costing.Properties.Settings.Default.SysproServer};Database={Costing.Properties.Settings.Default.SysproDB};Trusted_Connection=True;TrustServerCertificate=True;";

            using (SqlConnection con = new SqlConnection(sysproConnStr))
            {
                await con.OpenAsync();

                // Grab Cost Centres
                using (SqlCommand cmd = new SqlCommand("SELECT CostCentre, Description FROM dbo.BomCostCentre", con))
                using (SqlDataReader reader = await cmd.ExecuteReaderAsync())
                {
                    while (await reader.ReadAsync())
                    {
                        newCostCentres.Add(new CostCentre
                        {
                            CcCode = reader["CostCentre"]?.ToString().Trim(),
                            CcDescription = reader["Description"]?.ToString().Trim()
                        });
                    }
                }

                // Grab Work Centres
                using (SqlCommand cmd = new SqlCommand("SELECT WorkCentre, Description, CostCentre FROM dbo.BomWorkCentre", con))
                using (SqlDataReader reader = await cmd.ExecuteReaderAsync())
                {
                    while (await reader.ReadAsync())
                    {
                        newWorkCentres.Add(new WorkCentre
                        {
                            WcCode = reader["WorkCentre"]?.ToString().Trim(),
                            WcDescription = reader["Description"]?.ToString().Trim(),
                            CcCode = reader["CostCentre"]?.ToString().Trim(),
                            Type = "",
                            Staff = ""
                        });
                    }
                }
            }

            // SAVE TO LOCAL DB
            using (var context = new CostingDbContext())
            {
                // Pull what is currently in our local database
                var existingCostCentres = await context.CostCentres.ToListAsync();
                var existingWorkCentres = await context.WorkCentres.ToListAsync();

                // UPSERT COST CENTRES
                foreach (var newCc in newCostCentres)
                {
                    var match = existingCostCentres.FirstOrDefault(c => c.CcCode == newCc.CcCode);
                    if (match != null)
                    {
                        // Update existing
                        match.CcDescription = newCc.CcDescription;
                    }
                    else
                    {
                        // Insert new
                        context.CostCentres.Add(newCc);
                    }
                }

                // Save parents so they exist before we process children
                await context.SaveChangesAsync();

                // UPSERT WORK CENTRES
                foreach (var newWc in newWorkCentres)
                {
                    var match = existingWorkCentres.FirstOrDefault(w => w.WcCode == newWc.WcCode);
                    if (match != null)
                    {
                        // Update existing
                        match.WcDescription = newWc.WcDescription;
                        match.CcCode = newWc.CcCode;
                    }
                    else
                    {
                        // Insert new
                        context.WorkCentres.Add(newWc);
                    }
                }
                await context.SaveChangesAsync();
            }
        }
        #endregion

        #region Overhead / GL
        public static async Task<List<GLPeriodControl>> GetSysproPeriodsAsync()
        {
            var periods = new List<GLPeriodControl>();
            string company = Costing.Properties.Settings.Default.SysproDB.Substring(Costing.Properties.Settings.Default.SysproDB.Length - 1, 1);
            string sysproConnStr = $"Server={Costing.Properties.Settings.Default.SysproServer};Database={Costing.Properties.Settings.Default.SysproDB};Trusted_Connection=True;TrustServerCertificate=True;";

            using (Microsoft.Data.SqlClient.SqlConnection con = new Microsoft.Data.SqlClient.SqlConnection(sysproConnStr))
            {
                await con.OpenAsync();

                // Get the current active period and year
                using (Microsoft.Data.SqlClient.SqlCommand cmd = new Microsoft.Data.SqlClient.SqlCommand("SELECT Company, GlPeriod, GlYear FROM dbo.GenControl WHERE Company = @comp", con))
                {
                    cmd.Parameters.AddWithValue("@comp", company);

                    using (Microsoft.Data.SqlClient.SqlDataReader reader = await cmd.ExecuteReaderAsync())
                    {
                        if (await reader.ReadAsync())
                        {
                            int currentYear = Convert.ToInt32(reader["GlYear"]);
                            int maxPeriod = Convert.ToInt32(reader["GlPeriod"]);

                            for (int i = 1; i <= maxPeriod; i++)
                            {
                                periods.Add(new GLPeriodControl
                                {
                                    Company = company,
                                    GlYear = currentYear,
                                    GlPeriod = i
                                });
                            }
                        }
                    }
                }
            }
            return periods;
        }

        public static async Task<List<GLAccount>> GetOverheadAccountsAsync(int year, int period)
        {
            var accounts = new List<GLAccount>();
            string company = Costing.Properties.Settings.Default.SysproDB.Substring(Costing.Properties.Settings.Default.SysproDB.Length - 1, 1);
            string sysproConnStr = $"Server={Costing.Properties.Settings.Default.SysproServer};Database={Costing.Properties.Settings.Default.SysproDB};Trusted_Connection=True;TrustServerCertificate=True;";

            using (Microsoft.Data.SqlClient.SqlConnection con = new Microsoft.Data.SqlClient.SqlConnection(sysproConnStr))
            {
                await con.OpenAsync();

                // Dynamically select the correct Closing Balance column based on the period
                string balanceColumn = $"dbo.GenHistory.ClosingBalPer{period}";

                string query = $@"
                    SELECT dbo.GenMaster.Company, 
                           dbo.GenMaster.GlCode, 
                           dbo.GenMaster.Description, 
                           dbo.GenMaster.GlGroup, 
                           dbo.GenHistory.GlYear, 
                           {balanceColumn} as Value 
                    FROM dbo.GenHistory 
                    INNER JOIN dbo.GenMaster 
                            ON dbo.GenHistory.Company = dbo.GenMaster.Company 
                           AND dbo.GenHistory.GlCode = dbo.GenMaster.GlCode 
                    WHERE dbo.GenMaster.Company = @comp 
                      AND dbo.GenHistory.GlYear = @year 
                      AND (dbo.GenMaster.GlGroup = '05' OR dbo.GenMaster.GlGroup = '04') 
                    ORDER BY dbo.GenMaster.GlGroup";

                using (Microsoft.Data.SqlClient.SqlCommand cmd = new Microsoft.Data.SqlClient.SqlCommand(query, con))
                {
                    cmd.Parameters.AddWithValue("@comp", company);
                    cmd.Parameters.AddWithValue("@year", year);

                    using (Microsoft.Data.SqlClient.SqlDataReader reader = await cmd.ExecuteReaderAsync())
                    {
                        while (await reader.ReadAsync())
                        {
                            accounts.Add(new GLAccount
                            {
                                Company = reader["Company"]?.ToString(),
                                GlCode = reader["GlCode"]?.ToString(),
                                Description = reader["Description"]?.ToString(),
                                GlGroup = reader["GlGroup"]?.ToString(),
                                GlYear = Convert.ToInt32(reader["GlYear"]),
                                GlPeriod = period, // Inject the selected period for the Annualised math
                                Value = reader["Value"] != DBNull.Value ? Convert.ToDecimal(reader["Value"]) : 0m
                            });
                        }
                    }
                }
            }
            return accounts;
        }
        #endregion
    }
}