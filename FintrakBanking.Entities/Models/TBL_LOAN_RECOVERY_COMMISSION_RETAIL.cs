using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FintrakBanking.Entities.Models
{
    [Table("TBL_LOAN_RECOVERY_COMMISSION_RETAIL")]
    public partial class TBL_LOAN_RECOVERY_COMMISSION_RETAIL
    {
        [Key]
        public int LOANRECOVERYCOMMISSIONID { get; set; }
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
        public decimal? COMMISSIONPAYABLE { get; set; }
        public int? ACCREDITEDCONSULTANT { get; set; }
        public decimal? TOTALRECOVERYAMOUNT { get; set; }
        public decimal? AMOUNTRECOVERED { get; set; }
        public int? LOANASSIGNID { get; set; }
        public int? LOANID { get; set; }
        public string LOANREFERENCE { get; set; }
        public int? PRODUCTID { get; set; }
        public int? PRODUCTCLASSID { get; set; }
    }
}
