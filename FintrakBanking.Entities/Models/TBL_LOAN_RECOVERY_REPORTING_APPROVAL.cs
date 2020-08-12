using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FintrakBanking.Entities.Models
{
    [Table("TBL_LOAN_RECOVERY_REPORTING_APPROVAL")]
    public partial class TBL_LOAN_RECOVERY_REPORTING_APPROVAL
    {
        [Key]
        public int LOANRECOVERYREPORTAPPROVALID { get; set; }
        public string REFERENCEID { get; set; }
        public DateTime DATETIMECREATED { get; set; }
        public int CREATEDBY { get; set; }
        public int? OPERATIONID { get; set; }
        public int? APPROVALSTATUSID { get; set; }
        public string MISCODE { get; set; }
        public string REGION { get; set; }
        public string COMMENT { get; set; }
    }
}
