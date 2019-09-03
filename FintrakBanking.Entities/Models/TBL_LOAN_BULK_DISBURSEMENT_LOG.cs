namespace FintrakBanking.Entities.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("TBL_LOAN_BULK_DISBURSEMENT")]
    public partial class TBL_LOAN_BULK_DISBURSEMENT
    {
        [Key]
        public int MULTIPLEBULKDISBURSEMENTID { get; set; }
        public int LOANAPPLICATIONDETAILID { get; set; }
        public int SCHEMEID { get; set; }
        public string SCHEMECODE { get; set; }
        public string CUSTOMERCODE { get; set; }
        public int CUSTOMERID { get; set; }
        public int CURRENCYID { get; set; }
        public double LOANAMOUNT { get; set; }
        public int TENOR { get; set; }
        public double INTERESTRATE { get; set; }
        public DateTime EFFECTIVEDATE { get; set; }
        public DateTime MATURITYDATE { get; set; }
        public int CASAACCOUNTID { get; set; }
        public int? CASAACCOUNTID2 { get; set; }
        public int INTERESTPAYMENTFREQUENCYID { get; set; }
        public int PRINCIPALPAYMENTFREQUENCYID { get; set; }
        public bool SHOULDDISBURSE { get; set; }
        public short APPROVALSTATUS { get; set; }

        //public DateTime DATETIMECREATED { get; set; }
        //public DateTime DATETIMEUPDATED { get; set; }
        //public bool DELETED { get; set; }
        //public int DELETEDBY { get; set; }
        //public DateTime DATETIMEDELETED { get; set; }

    }
}









