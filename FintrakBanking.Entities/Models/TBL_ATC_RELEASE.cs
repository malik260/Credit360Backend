using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FintrakBanking.Entities.Models
{
    [Table("TBL_ATC_RELEASE")]
    public class TBL_ATC_RELEASE
    {
        [Key]
        public int ATCRELEASEID { get; set; }
        public int UNITNUMBER { get; set; }
        public int UNITTORELEASE { get; set; }
        public int UNITBALANCE { get; set; }
        public DateTime DATETIMECREATED { get; set; }
        public short APPROVALSTATUSID { get; set; }
        public int CREATEDBY { get; set; }
        public bool DELETED { get; set; }
        public int ATCLODGMENTID { get; set; }
        public DateTime? DATETIMEAPPROVED { get; set; }
        public int? LASTUPDATEDBY { get; set; }
        public DateTime? DATETIMEUPDATED { get; set; }
    }
}
