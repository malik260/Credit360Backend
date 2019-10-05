using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FintrakBanking.Entities.Models
{
    [Table("TBL_ALERT_FREQUENCY")]
    public partial class TBL_ALERT_FREQUENCY
    {
        [Key]
        public short ALERTFREQUENCYID {get; set;}
        public string DESCRIPTION { get; set; }
        public string FREQUENCYMODE { get; set; }
    }
}
