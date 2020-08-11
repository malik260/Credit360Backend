using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FintrakBanking.Entities.Models
{
    [Table("TBL_LOAN_RECOVERY_COMMISSION_APPROVAL")]
    public partial class TBL_LOAN_RECOVERY_COMMISSION_APPROVAL
    {
        [Key]
        public int LOANRECOVERYCOMMISSIONAPPROVALID { get; set; }
        public string REFERENCEID { get; set; }
        public DateTime DATETIMECREATED { get; set; }
        public int CREATEDBY { get; set; }
        public int? OPERATIONID { get; set; }
        public int? APPROVALSTATUSID { get; set; }
        public string AGENTACCOUNTNUMBER { get; set; }
        public DateTime? DATEOFENGAGEMENT { get; set; }
        public string COMMENT { get; set; }
        public DateTime? COLLECTIONDATE { get; set; }
        public string MODEOFCOLLECTION { get; set; }
        public decimal? COMMISSIONRATE { get; set; }

    }
}
