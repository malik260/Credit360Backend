using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FintrakBanking.Entities.Models
{
    [Table("TBL_CUSTOMER_EMAIL")]
    public class TBL_CUSTOMER_EMAIL
    {
        [Key]
        public string CUSTOMERNO { get; set; }
        public string EMAIL { get; set; }
        public string MOBILENO { get; set; }
    }
}
