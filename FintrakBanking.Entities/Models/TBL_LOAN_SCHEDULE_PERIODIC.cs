namespace FintrakBanking.Entities.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("TBL_LOAN_SCHEDULE_PERIODIC")]
    public partial class TBL_LOAN_SCHEDULE_PERIODIC
    {
        //public TBL_LOAN_SCHEDULE_PERIODIC()
        //{
        //    TBL_LOAN_EXTERNAL = new TBL_LOAN_EXTERNAL();
        //}

        [Key]
        public int PERIODICSCHEDULEID { get; set; }

        public int LOANID { get; set; }

        public int PAYMENTNUMBER { get; set; }

        //[Column(TypeName = "date")]
        public DateTime PAYMENTDATE { get; set; }

        //[Column(TypeName = "money")]
        public decimal STARTPRINCIPALAMOUNT { get; set; }

        //[Column(TypeName = "money")]
        public decimal PERIODPAYMENTAMOUNT { get; set; }

        //[Column(TypeName = "money")]
        public decimal PERIODINTERESTAMOUNT { get; set; }

        //[Column(TypeName = "money")]
        public decimal PERIODPRINCIPALAMOUNT { get; set; }

        //[Column(TypeName = "money")]
        public decimal ENDPRINCIPALAMOUNT { get; set; }

        public double INTERESTRATE { get; set; }

        public double? PREVIOUSINTERESTRATE { get; set; }

        //[Column(TypeName = "money")]
        public decimal PREVIOUSINTERESTAMOUNT { get; set; }

        //[Column(TypeName = "money")]
        public decimal PREVIOUSPRINCIPALAMOUNT { get; set; }

        //[Column(TypeName = "money")]
        public decimal AMORTISEDSTARTPRINCIPALAMOUNT { get; set; }

        //[Column(TypeName = "money")]
        public decimal AMORTISEDPERIODPAYMENTAMOUNT { get; set; }

        //[Column(TypeName = "money")]
        public decimal AMORTISEDPERIODINTERESTAMOUNT { get; set; }

        //[Column(TypeName = "money")]
        public decimal AMORTISEDPERIODPRINCIPALAMOUNT { get; set; }

        //[Column(TypeName = "money")]
        public decimal AMORTISEDENDPRINCIPALAMOUNT { get; set; }

        public double EFFECTIVEINTERESTRATE { get; set; }

        public double? PREVIOUSEFFECTIVEINTERESTRATE { get; set; }

        public int CREATEDBY { get; set; }
        public int? EXTERNALLOANID { get; set; }

        public DateTime DATETIMECREATED { get; set; }

        public int? LASTUPDATEDBY { get; set; }

        public DateTime? DATETIMEUPDATED { get; set; }

        public virtual TBL_LOAN TBL_LOAN { get; set; }
        //public virtual TBL_LOAN_EXTERNAL TBL_LOAN_EXTERNAL { get; set; }

    }
}
