using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FintrakBanking.Entities.Models
{
    [Table("TBL_LOAN_RECOVERY_COMMISSION_INTERNAL")]
    public partial class TBL_LOAN_RECOVERY_COMMISSION_INTERNAL
    {
        [Key]
        public int LOANRECOVERYCOMMISSIONID { get; set; }
        public DateTime DATETIMECREATED { get; set; }
        public int CREATEDBY { get; set; }
        public string AGENTACCOUNTNUMBER { get; set; }
        public string COMMENT { get; set; }
        public decimal? COMMISSIONRATE { get; set; }
        public decimal? COMMISSIONPAYABLE { get; set; }
        public int? ACCREDITEDCONSULTANT { get; set; }
        public decimal? TOTALRECOVERYAMOUNT { get; set; }
        public decimal? AMOUNTRECOVERED { get; set; }
        public DateTime? VALIDATERECOVERYMONTH { get; set; }
    }
}
