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
    [Table("LoginUsers", Schema = "dbo")]
    public class LoginUser
    {
        [Key]
        [Column("UserName")]
        public string UserName { get; set; }

        [Column("Password")]
        public string Password { get; set; }

        [Column("Email")]
        public string Email { get; set; }

        [Column("UserName")]
        public string Role { get; set; }

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
