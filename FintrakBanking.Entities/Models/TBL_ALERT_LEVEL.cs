using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FintrakBanking.Entities.Models
{
    [Table("TBL_ALERT_LEVEL")]
    public partial class TBL_ALERT_LEVEL
    {
        [Key]
        public int ALERTLEVELID { get; set; }
        public string EMAILLIST { get; set; }
        public string LEVELCODE { get; set; }
        public int ALERTTITLEID { get; set; }
    }
}
