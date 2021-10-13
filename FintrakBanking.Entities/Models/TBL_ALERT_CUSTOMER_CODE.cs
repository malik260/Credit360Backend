using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FintrakBanking.Entities.Models
{
    [Table("TBL_ALERT_CUSTOMER_CODE")]
    public partial class TBL_ALERT_CUSTOMER_CODE
    {
        [Key]
        public int CUSTOMERCODEID { get; set; }
        public string CUSTOMERCODE { get; set; }
        public DateTime? DATETIMECREATED { get; set; }

    }
}
