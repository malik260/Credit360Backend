using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FintrakBanking.Entities.Models
{
    [Table("TBL_ALERT_GENERAL_TEMPLATE")]
    public partial class TBL_ALERT_GENERAL_TEMPLATE
    {
        [Key]
        public int TEMPLATEID { get; set; }
        public string TEMPLATEBODY { get; set; }
    }
}
