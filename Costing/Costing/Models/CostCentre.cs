using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Costing.Models
{
    [Table("CostCentres")]
    public class CostCentre
    {
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [Column("id")]
        public int Id { get; set; }

        [Key]
        [Column("cc_code")]
        public string CcCode { get; set; }

        [Column("cc_description")]
        public string CcDescription { get; set; }
    }
}