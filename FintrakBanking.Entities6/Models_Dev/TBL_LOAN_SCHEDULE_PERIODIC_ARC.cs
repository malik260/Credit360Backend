namespace FintrakBanking.Entities.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("FINTRAKBANKING.TBL_LOAN_SCHEDULE_PERIODIC_ARC")]
    public partial class TBL_LOAN_SCHEDULE_PERIODIC_ARC
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public int PERIODICSCHEDULEID { get; set; }

        [Required]
        [StringLength(50)]
        public string ARCHIVEBATCHCODE { get; set; }

        public int LOANID { get; set; }

        public int PAYMENTNUMBER { get; set; }

        public decimal STARTPRINCIPALAMOUNT { get; set; }

        public decimal PERIODPAYMENTAMOUNT { get; set; }

        public decimal PERIODINTERESTAMOUNT { get; set; }

        public decimal PERIODPRINCIPALAMOUNT { get; set; }

        public decimal ENDPRINCIPALAMOUNT { get; set; }

        public double INTERESTRATE { get; set; }

        public decimal AMORTISEDSTARTPRINCIPALAMOUNT { get; set; }

        public decimal AMORTISEDPERIODPAYMENTAMOUNT { get; set; }

        public decimal AMORTISEDPERIODINTERESTAMOUNT { get; set; }

        public decimal AMORTISEDPERIODPRINCIPALAMOUNT { get; set; }

        public decimal AMORTISEDENDPRINCIPALAMOUNT { get; set; }

        public double EFFECTIVEINTERESTRATE { get; set; }

        public int CREATEDBY { get; set; }

        public DateTime DATETIMECREATED { get; set; }

        public int? LASTUPDATEDBY { get; set; }

        public DateTime? DATETIMEUPDATED { get; set; }

        public DateTime PAYMENTDATE { get; set; }

        public DateTime? ARCHIVEDATE { get; set; }
    }
}
