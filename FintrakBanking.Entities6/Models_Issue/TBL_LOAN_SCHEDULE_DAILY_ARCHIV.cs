namespace FintrakBanking.Entities.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("TBL_LOAN_SCHEDULE_DAILY_ARCHIV")]
    public partial class TBL_LOAN_SCHEDULE_DAILY_ARCHIV
    {
        [Key]
        public int DAILYSCHEDULEID { get; set; }

        //[Column(TypeName = "date")]
        public DateTime ARCHIVEDATE { get; set; }

        [Required]
        [StringLength(50)]
        public string ARCHIVEBATCHCODE { get; set; }

        public int LOANID { get; set; }

        public int PAYMENTNUMBER { get; set; }

        //[Column(TypeName = "date")]
        public DateTime DATE { get; set; }

        //[Column(TypeName = "date")]
        public DateTime PAYMENTDATE { get; set; }

        //[Column(TypeName = "money")]
        public decimal OPENINGBALANCE { get; set; }

        //[Column(TypeName = "money")]
        public decimal STARTPRINCIPALAMOUNT { get; set; }

        //[Column(TypeName = "money")]
        public decimal DAILYPAYMENTAMOUNT { get; set; }

        //[Column(TypeName = "money")]
        public decimal DAILYINTERESTAMOUNT { get; set; }

        //[Column(TypeName = "money")]
        public decimal DAILYPRINCIPALAMOUNT { get; set; }

        //[Column(TypeName = "money")]
        public decimal CLOSINGBALANCE { get; set; }

        //[Column(TypeName = "money")]
        public decimal ENDPRINCIPALAMOUNT { get; set; }

        //[Column(TypeName = "money")]
        public decimal ACCRUEDINTEREST { get; set; }

        //[Column(TypeName = "money")]
        public decimal AMORTISEDCOST { get; set; }

        public double INTERESTRATE { get; set; }

        //[Column(TypeName = "money")]
        public decimal AMORTISEDOPENINGBALANCE { get; set; }

        //[Column(TypeName = "money")]
        public decimal AMORTISEDSTARTPRINCIPALAMOUNT { get; set; }

        //[Column(TypeName = "money")]
        public decimal AMORTISEDDAILYPAYMENTAMOUNT { get; set; }

        //[Column(TypeName = "money")]
        public decimal AMORTISEDDAILYINTERESTAMOUNT { get; set; }

        //[Column(TypeName = "money")]
        public decimal AMORTISEDDAILYPRINCIPALAMOUNT { get; set; }

        //[Column(TypeName = "money")]
        public decimal AMORTISEDCLOSINGBALANCE { get; set; }

        //[Column(TypeName = "money")]
        public decimal AMORTISEDENDPRINCIPALAMOUNT { get; set; }

        //[Column(TypeName = "money")]
        public decimal AMORTISEDACCRUEDINTEREST { get; set; }

        //[Column(TypeName = "money")]
        public decimal AMORTISED_AMORTISEDCOST { get; set; }

        //[Column(TypeName = "money")]
        public decimal DISCOUNTPREMIUM { get; set; }

        //[Column(TypeName = "money")]
        public decimal UNEARNEDFEE { get; set; }

        //[Column(TypeName = "money")]
        public decimal EARNEDFEE { get; set; }

        public double EFFECTIVEINTERESTRATE { get; set; }

        public int NUMBEROFPERIODS { get; set; }

        //[Column(TypeName = "money")]
        public decimal BALLONAMOUNT { get; set; }

        public int CREATEDBY { get; set; }

        public DateTime DATETIMECREATED { get; set; }

        public int? LASTUPDATEDBY { get; set; }

        public DateTime? DATETIMEUPDATED { get; set; }
    }
}
