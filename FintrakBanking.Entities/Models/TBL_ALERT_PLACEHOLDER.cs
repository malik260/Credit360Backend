using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FintrakBanking.Entities.Models
{
    [Table("TBL_ALERT_PLACEHOLDER")]
    public partial class TBL_ALERT_PLACEHOLDER
    {
        [Key]
        public short PLACEHOLDERID { get; set; }
        public string PLACEHOLDER { get; set; }
        public string DESCRIPTION { get; set; }
        public string TYPE { get; set; }
        public int? ALERTTITLEID { get; set; }
        public string PARTITION { get; set; }
    }
}
