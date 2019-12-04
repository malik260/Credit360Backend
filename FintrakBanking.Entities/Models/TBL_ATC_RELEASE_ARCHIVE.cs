using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FintrakBanking.Entities.Models
{
     [Table("TBL_ATC_RELEASE_ARCHIVE")]
     public class TBL_ATC_RELEASE_ARCHIVE
     {
        [Key]
        public int ATCRELEASEARCHIVEID { get; set; }
        public int UNITNUMBER { get; set; }
        public int ATCRELEASEID { get; set; }
        public int UNITTORELEASE { get; set; }
        public int UNITBALANCE { get; set; }
        public DateTime DATETIMECREATED { get; set; }
        public short APPROVALSTATUSID { get; set; }
        public int CREATEDBY { get; set; }
        public int ATCLODGMENTID { get; set; }
    }
}
