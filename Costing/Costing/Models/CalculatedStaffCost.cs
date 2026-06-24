using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Costing.Models
{
    [Table("CalculatedStaffCosts")]
    public class CalculatedStaffCost
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [Column("id")]
        public int Id { get; set; }

        // This links back to the main employee record
        [Column("calculated_staff_id")]
        public int CalculatedStaffId { get; set; }

        [ForeignKey("CalculatedStaffId")]
        public virtual CalculatedStaff CalculatedStaffRecord { get; set; }

        [Column("category_name")]
        public string CategoryName { get; set; }

        [Column("amount")]
        public decimal Amount { get; set; }
    }
}