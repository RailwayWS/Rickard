using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Costing.Models
{
    [Table("AuditLogCosts")]
    public class AuditLogCost
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [Column("id")]
        public int Id { get; set; }

        // Links back to the main AuditLog record
        [Column("audit_log_id")]
        public int AuditLogId { get; set; }

        [ForeignKey("AuditLogId")]
        public virtual AuditLog AuditLogRecord { get; set; }

        [Column("category_name")]
        public string CategoryName { get; set; }

        [Column("amount")]
        public decimal Amount { get; set; }

        [Column("rate_used")]
        [Precision(15, 5)]
        public decimal? RateUsed { get; set; }
    }
}