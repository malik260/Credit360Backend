using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FintrakBanking.Entities.Models
{
    [Table("TBL_ALERT_STAFF_ROLE")]
    public partial class TBL_ALERT_STAFF_ROLE
    {
        [Key]
        public int ALERTSTAFFROLEID { get; set; }
        public int ALERTTITLEID { get; set; }
        public int STAFFROLEID { get; set; }
    }
}
