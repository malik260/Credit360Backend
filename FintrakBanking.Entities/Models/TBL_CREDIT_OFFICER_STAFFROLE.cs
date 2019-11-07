using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FintrakBanking.Entities.Models
{
    [Table("TBL_CREDIT_OFFICER_STAFFROLE")]
    public partial class TBL_CREDIT_OFFICER_STAFFROLE
    {
        [Key]
        public int CREDITOFFICERROLEID { get; set; }
        public int STAFFROLEID { get; set; }

    }
}
