using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FintrakBanking.Entities.Models
{
    [Table("TBL_COLLECTION_RETAIL_CRON_SETUP")]
    public partial class TBL_COLLECTION_RETAIL_CRON_SETUP
    {
        [Key]
        public int CRONJOBID { get; set; }
        public DateTime STARTDATE { get; set; }
        public string STARTTIME { get; set; }
        public DateTime ENDDATE { get; set; }
        public string ENDTIME { get; set; }
        public int CRONNATURE { get; set; }
        public bool DELETED { get; set; }
        public int? DELETEDBY { get; set; }
        public DateTime? DATETIMEDELETED { get; set; }
        public int CREATEDBY { get; set; }
        public DateTime DATETIMECREATED { get; set; }
    }
}
