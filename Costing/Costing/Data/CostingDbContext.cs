using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Costing.Models;

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

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseSqlServer(
                @"Server=RA-ERP;Database=Costing;Trusted_Connection=True;ConnectRetryCount=0; TrustServerCertificate=True;");
        }

    }

}
