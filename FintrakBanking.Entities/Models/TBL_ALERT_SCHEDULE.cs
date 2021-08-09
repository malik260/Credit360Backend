using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FintrakBanking.Entities.Models
{
    [Table("TBL_ALERT_SCHEDULE")]
    public partial class TBL_ALERT_SCHEDULE
    {
        [Key]
        public int ALERTSCHEDULEID { get; set; }
        public int FREQUENCYID { get; set; }
        public string ALERTTIME { get; set; }
        public string TIMEFROM { get; set; }
        public string TIMETO { get; set; }
        public int ALERTTITLEID { get; set; }
    }
}
