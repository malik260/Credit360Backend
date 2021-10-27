using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FintrakBanking.Entities.Models
{
    [Table("TBL_FACILITY_MODIFICATION")]
    public partial class TBL_FACILITY_MODIFICATION
    {
        [Key]
        public int FACILITYMODIFICATIONID { get; set; }
        public int LOANAPPLICATIONDETAILID { get; set; }
        public int APPROVEDTENOR { get; set; }
        public short APPROVEDPRODUCTID { get; set; }
        public int TENORMODEID { get; set; }
        public short SUBSECTORID { get; set; }
        public int PRODUCTCLASSID { get; set; }
        public int REPAYMENTSCHEDULEID { get; set; }
        public int INTERESTREPAYMENTID { get; set; }
        public int LOANDETAILREVIEWTYPEID { get; set; }
        public decimal APPROVEDAMOUNT { get; set; }
        public double APPROVEDINTERESTRATE { get; set; }
        public string REPAYMENTTERMS { get; set; }
        public string INTERESTREPAYMENT { get; set; }
        public short APPROVALSTATUSID { get; set; }

        public bool DELETED { get; set; }
        public int CREATEDBY { get; set; }
        public int LASTUPDATEDBY { get; set; }
        public int DELETEDBY { get; set; }
        public DateTime DATETIMECREATED { get; set; }
        public DateTime? DATETIMEUPDATED { get; set; }
        public DateTime? DATETIMEDELETED { get; set; }
    }
}
