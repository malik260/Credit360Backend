namespace FintrakBanking.Entities.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("FINTRAKBANKING.TBL_LOAN_SCHEDULE_DAILY_ARCHIV")]
    public partial class TBL_LOAN_SCHEDULE_DAILY_ARCHIV
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public int DAILYSCHEDULEID { get; set; }

        [Required]
        [StringLength(50)]
        public string ARCHIVEBATCHCODE { get; set; }

        public int LOANID { get; set; }

        public int PAYMENTNUMBER { get; set; }

        public decimal OPENINGBALANCE { get; set; }

        public decimal STARTPRINCIPALAMOUNT { get; set; }

        public decimal DAILYPAYMENTAMOUNT { get; set; }

        public decimal DAILYINTERESTAMOUNT { get; set; }

        public decimal DAILYPRINCIPALAMOUNT { get; set; }

        public decimal CLOSINGBALANCE { get; set; }

        public decimal ENDPRINCIPALAMOUNT { get; set; }

        public decimal ACCRUEDINTEREST { get; set; }

        public decimal AMORTISEDCOST { get; set; }

        public decimal INTERESTRATE { get; set; }

        public decimal AMORTISEDOPENINGBALANCE { get; set; }

        public decimal AMORTISEDSTARTPRINCIPALAMOUNT { get; set; }

        public decimal AMORTISEDDAILYPAYMENTAMOUNT { get; set; }

        public decimal AMORTISEDDAILYINTERESTAMOUNT { get; set; }

        public decimal AMORTISEDDAILYPRINCIPALAMOUNT { get; set; }

        public decimal AMORTISEDCLOSINGBALANCE { get; set; }

        public decimal AMORTISEDENDPRINCIPALAMOUNT { get; set; }

        public decimal AMORTISEDACCRUEDINTEREST { get; set; }

        public decimal AMORTISED_AMORTISEDCOST { get; set; }

        public decimal DISCOUNTPREMIUM { get; set; }

        public decimal UNEARNEDFEE { get; set; }

        public decimal EARNEDFEE { get; set; }

        public decimal EFFECTIVEINTERESTRATE { get; set; }

        public int NUMBEROFPERIODS { get; set; }

        public decimal BALLONAMOUNT { get; set; }

        public int CREATEDBY { get; set; }

        public DateTime DATETIMECREATED { get; set; }

        public int? LASTUPDATEDBY { get; set; }

        public DateTime? DATETIMEUPDATED { get; set; }

        public DateTime? PAYMENTDATE { get; set; }

        public DateTime? DATE_ { get; set; }

        public DateTime? ARCHIVEDATE { get; set; }
    }
}
