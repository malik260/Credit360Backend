namespace FintrakBanking.Entities.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("FINTRAKBANKING.TBL_LOAN_SCHEDULE_PERIODIC")]
    public partial class TBL_LOAN_SCHEDULE_PERIODIC
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public int PERIODICSCHEDULEID { get; set; }

        public int LOANID { get; set; }

        public int PAYMENTNUMBER { get; set; }

        public decimal STARTPRINCIPALAMOUNT { get; set; }

        public decimal PERIODPAYMENTAMOUNT { get; set; }

        public decimal PERIODINTERESTAMOUNT { get; set; }

        public decimal PERIODPRINCIPALAMOUNT { get; set; }

        public decimal ENDPRINCIPALAMOUNT { get; set; }

        public decimal INTERESTRATE { get; set; }

        public decimal? PREVIOUSINTERESTRATE { get; set; }

        public decimal PREVIOUSINTERESTAMOUNT { get; set; }

        public decimal PREVIOUSPRINCIPALAMOUNT { get; set; }

        public decimal AMORTISEDSTARTPRINCIPALAMOUNT { get; set; }

        public decimal AMORTISEDPERIODPAYMENTAMOUNT { get; set; }

        public decimal AMORTISEDPERIODINTERESTAMOUNT { get; set; }

        public decimal AMORTISEDPERIODPRINCIPALAMOUNT { get; set; }

        public decimal AMORTISEDENDPRINCIPALAMOUNT { get; set; }

        public decimal EFFECTIVEINTERESTRATE { get; set; }

        public decimal? PREVIOUSEFFECTIVEINTERESTRATE { get; set; }

        public int CREATEDBY { get; set; }

        public DateTime DATETIMECREATED { get; set; }

        public int? LASTUPDATEDBY { get; set; }

        public DateTime? DATETIMEUPDATED { get; set; }

        public DateTime? PAYMENTDATE { get; set; }

        public virtual TBL_LOAN TBL_LOAN { get; set; }
    }
}
