using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FintrakBanking.Entities.Models
{
    [Table("TBL_LOAN_RECOVERY_COMMISSION_BATCH")]
    public partial class TBL_LOAN_RECOVERY_COMMISSION_BATCH
    {
        [Key]
        public int LOANRECOVERYCOMMISSIONBATCHID { get; set; }
        public string REFERENCEID { get; set; }
        public string LOANREFERENCENUMBER { get; set; }
        public string CUSTOMERID { get; set; }
        public int ACCREDITEDCONSULTANT { get; set; }
        public DateTime DATETIMECREATED { get; set; }
        public int CREATEDBY { get; set; }
        public int? OPERATIONID { get; set; }
        public bool OPERATIONCOMPLETED { get; set; }
        public int? APPROVALSTATUSID { get; set; }
        public decimal? TOTALAMOUNTRECOVERY { get; set; }
        public decimal? AMOUNTRECOVERED { get; set; }
        public int LOANID { get; set; }
        public string MISCODE { get; set; }
        public string REGION { get; set; }
        public bool DELETED { get; set; }
        public int? DELETEDBY { get; set; }
        public int? LOANRECOVERYREPORTBATCHID { get; set; }

        public decimal? COMMISSIONAMOUNTLESSWHT { get; set; }
        public decimal? WHTAMOUNT { get; set; }
        public decimal? WHTRATE { get; set; }
        public decimal? COMMISSIONAMOUNT { get; set; }

    }
}
