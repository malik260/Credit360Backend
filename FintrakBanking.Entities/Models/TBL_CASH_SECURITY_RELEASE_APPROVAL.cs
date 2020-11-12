using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FintrakBanking.Entities.Models
{
    [Table("TBL_CASH_SECURITY_RELEASE_APPROVAL")]
    public partial class TBL_CASH_SECURITY_RELEASE_APPROVAL
    {
        [Key]
        public int CASHSECURITYRELEASEID { get; set; }
        public int COLLATERALCUSTOMERID { get; set; }
        public int LOANAPPLICATIONDETAILID { get; set; }
        public int LOANAPPLICATIONID { get; set; }
        public int CUSTOMERID { get; set; }
        public decimal? LIENAMOUNT { get; set; }
        public int CREATEDBY { get; set; }
        public DateTime DATETIMECREATED { get; set; }
        public int APPROVALSTATUSID { get; set; }
        public string COMMENT { get; set; }

    }
}
