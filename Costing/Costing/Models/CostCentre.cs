using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Costing.Models
{
    [Table("CostCentres")]
    public class CostCentre
    {

        [Key]
        [Column("cc_code")]
        public string CcCode { get; set; }

        [Column("cc_description")]
        public string CcDescription { get; set; }
    }
}