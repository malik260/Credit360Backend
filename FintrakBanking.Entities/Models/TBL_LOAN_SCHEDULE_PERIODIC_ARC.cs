namespace FintrakBanking.Entities.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("credit.TBL_LOAN_SCHEDULE_PERIODIC_ARC")]
    public partial class TBL_LOAN_SCHEDULE_PERIODIC_ARC
    {
        [Key]
        public int PERIODICSCHEDULEID { get; set; }

        [Column(TypeName = "date")]
        public DateTime ARCHIVEDATE { get; set; }

        [Required]
        [StringLength(50)]
        public string ARCHIVEBATCHCODE { get; set; }

        public int LOANID { get; set; }

        public int PAYMENTNUMBER { get; set; }

        [Column(TypeName = "date")]
        public DateTime PAYMENTDATE { get; set; }

        [Column(TypeName = "money")]
        public decimal STARTPRINCIPALAMOUNT { get; set; }

        [Column(TypeName = "money")]
        public decimal PERIODPAYMENTAMOUNT { get; set; }

        [Column(TypeName = "money")]
        public decimal PERIODINTERESTAMOUNT { get; set; }

        [Column(TypeName = "money")]
        public decimal PERIODPRINCIPALAMOUNT { get; set; }

        [Column(TypeName = "money")]
        public decimal ENDPRINCIPALAMOUNT { get; set; }

        public double INTERESTRATE { get; set; }

        [Column(TypeName = "money")]
        public decimal AMORTISEDSTARTPRINCIPALAMOUNT { get; set; }

        [Column(TypeName = "money")]
        public decimal AMORTISEDPERIODPAYMENTAMOUNT { get; set; }

        [Column(TypeName = "money")]
        public decimal AMORTISEDPERIODINTERESTAMOUNT { get; set; }

        [Column(TypeName = "money")]
        public decimal AMORTISEDPERIODPRINCIPALAMOUNT { get; set; }

        [Column(TypeName = "money")]
        public decimal AMORTISEDENDPRINCIPALAMOUNT { get; set; }

        public double EFFECTIVEINTERESTRATE { get; set; }

        public int CREATEDBY { get; set; }

        public DateTime DATETIMECREATED { get; set; }

        public int? LASTUPDATEDBY { get; set; }

        public DateTime? DATETIMEUPDATED { get; set; }
    }
}
