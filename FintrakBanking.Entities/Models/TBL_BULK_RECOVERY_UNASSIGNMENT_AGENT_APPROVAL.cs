using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FintrakBanking.Entities.Models
{
    [Table("TBL_BULK_RECOVERY_UNASSIGNMENT_AGENT_APPROVAL")]
    public partial class TBL_BULK_RECOVERY_UNASSIGNMENT_AGENT_APPROVAL
    {
        [Key]
        public int BULKRECOVERYUNASSIGNAPPROVALID { get; set; }
        public int ACCREDITEDCONSULTANTID { get; set; }
        public string REFERENCEBATCHID { get; set; }
        public int APPROVALSTATUSID { get; set; }
        public int OPERATIONID { get; set; }
        public DateTime REQUESTDATE { get; set; }
        public string SOURCE { get; set; }
        public int? LOANID { get; set; }
    }
}
