using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.ComponentModel.DataAnnotations;

namespace Costing.Models
{

    public class LoginUser
    {

        public string UserName { get; set; }
        public string Password { get; set; }
        public string Email { get; set; }
        //public string SysproUser { get; set; }
        //public string SysproPassword { get; set; }


    }

    [Table("Basic", Schema ="dbo")]
    public class BasicEmployee
    {
        [Key]
        [Column("code")]
        public string Code { get; set; }

        [Column("name")]
        public string Name { get; set; }

        [Column("cost_centre")]
        public string CostCentre { get; set; }

        [Column("job_description")]
        public string JobDescription { get; set; }

        [Column("rate_per_hour")]
        public decimal Rate { get; set; }
    }


}
