using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FintrakBanking.Entities.Models
{
   
    [Table("TBL_ALERT_TEMPLATE_TYPE")]
    public partial class TBL_ALERT_TEMPLATE_TYPE
    {
        [Key]
        public int ALERTTEMPLATETYPEID { get; set; }
        public string PLACEHOLDER { get; set; }

    }
}
