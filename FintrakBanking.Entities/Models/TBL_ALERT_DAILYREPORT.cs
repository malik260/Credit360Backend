using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FintrakBanking.Entities.Models
{
    [Table("TBL_ALERT_DAILYREPORT")]
    public partial class TBL_ALERT_DAILYREPORT
    {
        [Key]
        public int ID { get; set; }
        public DateTime PROCESSINGSTARTDATE { get; set; }
        public DateTime? PROCESSINGENDDATE { get; set; }
        public DateTime PROCESSINGDATE { get; set; }
        public string SUCCESSFULPROCESSINGIND { get; set; }
    }
}
