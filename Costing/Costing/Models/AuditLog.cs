using Microsoft.EntityFrameworkCore;
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

        [Column("bonus")]
        public decimal Bonus { get; set; }

        [Column("uif")]
        public decimal UIF { get; set; }

        [Column("sdl")]
        public decimal SDL { get; set; }

        [Column("bonus_rate")]
        [Precision(15, 5)]
        public decimal? BonusRate { get; set; }

        [Column("uif_rate")]
        [Precision(15, 5)]
        public decimal? UifRate { get; set; }

        [Column("sdl_rate")]
        [Precision(15, 5)]
        public decimal? SdlRate { get; set; }
    }
}