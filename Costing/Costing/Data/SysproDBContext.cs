using Costing.Models;
using Microsoft.EntityFrameworkCore;

namespace Costing.Data
{
    public class SysproDBContext : DbContext
    {
        public DbSet<Syspro> SysWorkCentres { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseSqlServer(
                $@"Server={Costing.Properties.Settings.Default.SysproServer};Database={Costing.Properties.Settings.Default.SysproDB};Trusted_Connection=True;TrustServerCertificate=True;");
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // Tells EF Core that this model doesn't have a Primary Key and is read-only
            modelBuilder.Entity<Syspro>().HasNoKey();
        }
    }
}