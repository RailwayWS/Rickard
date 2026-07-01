using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Costing.Models
{
    [Table("AuditLogs")]
    public class AuditLog
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [Column("id")]
        public int Id { get; set; }

        [Column("code")]
        public string Code { get; set; }

        [Column("employee_name")]
        public string EmployeeName { get; set; }

        [Column("snapshot_date")]
        public DateTime SnapshotDate { get; set; }

        [Column("snapshot_name")]
        public string SnapshotName { get; set; }

        // Relational link to the dynamic categories
        public virtual List<AuditLogCost> Costs { get; set; } = new List<AuditLogCost>();

        [NotMapped]
        public decimal this[string categoryName]
        {
            get
            {
                var cost = Costs?.FirstOrDefault(c => c.CategoryName.Equals(categoryName, StringComparison.OrdinalIgnoreCase));
                return cost?.Amount ?? 0m;
            }
        }
    }
}