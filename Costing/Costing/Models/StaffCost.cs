using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Costing.Models
{
    [Table("StaffCosts", Schema ="dbo")]
    public class StaffCost
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [Column("id")]
        public int Id { get; set; }

        [Column("category")]
        public string Category { get; set; }

        [Column("type")]
        public string Type { get; set; }

        [Column("value")]
        [Precision(15, 5)]
        public decimal Value { get; set; }

        [Column("BaseCategory")]
        public string BaseCategory { get; set; }
    }
}
