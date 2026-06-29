using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Costing.Models
{
    [Table("Allocations")]
    public class Allocation
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [Column("id")]
        public int Id { get; set; }

        [Column("code")]
        public string Code { get; set; }

        [Column("name")]
        public string Name { get; set; }

        [Column("work_centre")]
        public string WorkCentre { get; set; }

        [Column("cost_centre")]
        public string CostCentre { get; set; }

        [Column("portion")]
        public decimal? Portion { get; set; }
    }
}