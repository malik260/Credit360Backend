using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FintrakBanking.Entities.Models
{
    [Table("TBL_ENDOFDAY_MONITORING")]
    public partial class TBL_ENDOFDAY_MONITORING
    {
        [Key]
        public int ENDOFDAYMONITORINGID { get; set; }
        public string EODOPERATION { get; set; }
        public DateTime STARTDATETIME { get; set; }
        public DateTime ENDDATETIME { get; set; }
        public DateTime EODDATE { get; set; }
        public DateTime DATETIMECREATED { get; set; }
        public bool STATUS { get; set; }
        public string PROCESSBY { get; set; }
    }
}
