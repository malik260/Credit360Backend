using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FintrakBanking.Entities.Models
{
    [Table("TBL_LMSR_APPLICATION_DETL_ARCH")]
    public partial class TBL_LMSR_APPLICATION_DETL_ARCH
    {
        [Key]
        public int LOANREVIEWAPPLICATIONARCHID { get; set; }
        public int LOANREVIEWAPPLICATIONID { get; set; }
        public int LOANAPPLICATIONID { get; set; }
        public int LOANID { get; set; }
        public short? CURRENCYID { get; set; }
        public short PRODUCTID { get; set; }
        public int OPERATIONID { get; set; }
        public int APPROVALSTATUSID { get; set; }
        public short LOANSYSTEMTYPEID { get; set; }
        public int CUSTOMERID { get; set; }
        public bool DELETED { get; set; }
        public string REVIEWDETAILS { get; set; }
        public short REVIEWSTAGEID { get; set; }
        public string REPAYMENTTERMS { get; set; }
        public int REPAYMENTSCHEDULEID { get; set; }
        public int CREATEDBY { get; set; }
        public DateTime DATETIMECREATED { get; set; }
        public int PROPOSEDTENOR { get; set; }
        public double PROPOSEDINTERESTRATE { get; set; }
        public decimal PROPOSEDAMOUNT { get; set; }
        public int APPROVEDTENOR { get; set; }
        public double APPROVEDINTERESTRATE { get; set; }
        public decimal APPROVEDAMOUNT { get; set; }
        public bool OPERATIONPERFORMED { get; set; }
        public string MANAGEMENTPOSITION { get; set; }
        public decimal? CUSTOMERPROPOSEDAMOUNT { get; set; }
        public DateTime ARCHIVEDATE { get; set; }

    }
}
