using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FintrakBanking.Entities.Models
{
    [Table("TBL_LOAN_RECOVERY_PAYMENT")]
    public class TBL_LOAN_RECOVERY_PAYMENT
    {
        [Key]
        public short LOANRECOVERYPAYMENTID { get; set; }
        public short LOANREVIEWOPERATIONID { get; set; }
        public decimal PAYMENTAMOUNT { get; set; }
        public DateTime PAYMENTDATE { get; set; }
        public int APPROVALSTATUSID { get; set; }
        public int CREATEDBY { get; set; }
        public DateTime DATETIMECREATED { get; set; }
        public int? LASTUPDATEDBY { get; set; }
        public DateTime? DATETIMEUPDATED { get; set; }
        public bool DELETED { get; set; }
        public int? DELETEDBY { get; set; }
        public DateTime? DATETIMEDELETED { get; set; }
    }
}
