using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FintrakBanking.Entities.Models
{
    [Table("TBL_DOCUMENTATION_FILLING_APPROVAL")]
    public partial class TBL_DOCUMENTATION_FILLING_APPROVAL
    {
        [Key]
        public int DOCUMENTATIONFILLINGID { get; set; }
        public int LOANID { get; set; }
        public int CREATEDBY { get; set; }
        public DateTime DATETIMECREATED { get; set; }
        public int APPROVALSTATUSID { get; set; }
        public string MODULE { get; set; }
        public string COMMENT { get; set; }
        public int REQUESTID { get; set; }
        public string LOANREFERENCE { get; set; }
    }
}
