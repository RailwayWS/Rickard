using Costing.Models;
using Microsoft.EntityFrameworkCore;

namespace Costing.Data
{
    public class CostingDbContext : DbContext
    {
        public DbSet<BasicEmployee> Staff { get; set; }

        public DbSet<StaffCost> StaffCosts { get; set; }

        public DbSet<CalculatedStaff> CalculatedStaffRecords { get; set; }

        public DbSet<CalculatedStaffCost> CalculatedStaffCosts { get; set; }

        public DbSet<Allocation> Allocations { get; set; }

        public DbSet<CostCentre> CostCentres { get; set; }

        public DbSet<WorkCentre> WorkCentres { get; set; }

        public DbSet<LoginUser> LoginUsers { get; set; }

        public DbSet<AuditLog> AuditLogs { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseSqlServer(
                $@"Server={Costing.Properties.Settings.Default.CostingServer};Database={Costing.Properties.Settings.Default.CostingDB};Trusted_Connection=True;ConnectRetryCount=0;TrustServerCertificate=True;");
        }
    }

}
