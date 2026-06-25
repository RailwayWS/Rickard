using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Costing.Models
{
    [Table("WorkCentres")]
    public class WorkCentre
    {
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [Column("id")]
        public int Id { get; set; }

        [Key]
        [Column("wc_code")]
        public string WcCode { get; set; }

        [Column("wc_description")]
        public string WcDescription { get; set; }

        [Column("cc_code")]
        public string CcCode { get; set; }

        [Column("type")]
        public string Type { get; set; }

        [Column("staff")]
        public string Staff { get; set; }
    }
}