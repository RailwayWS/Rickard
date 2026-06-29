using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Costing.Models
{
    [Table("CompleteCalculated")]
    public class CalculatedStaff
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

        [Column("rate_per_hour")]
        public decimal RatePerHour { get; set; }

        [Column("allocation")]
        public decimal Allocation { get; set; }

        [Column("efficiency")]
        public decimal Efficiency { get; set; }

        [Column("rate")]
        public decimal Rate { get; set; }

        [Column("total")]
        public decimal Total { get; set; }

        [NotMapped]
        public decimal Amount => Efficiency != 0 ? Total / Efficiency : 0m;

        public virtual List<CalculatedStaffCost> DynamicCosts { get; set; } = new List<CalculatedStaffCost>();

        // INDEXER - allows to dynamicly add categories/columns
        [NotMapped] // This tells the db to ignore this
        public decimal this[string categoryName]
        {
            get
            {
                // returns value that wpf grid asks for
                var cost = DynamicCosts?.FirstOrDefault(c => c.CategoryName.Equals(categoryName, StringComparison.OrdinalIgnoreCase));
                return cost != null ? cost.Amount : 0m;
            }
        }
    }
}