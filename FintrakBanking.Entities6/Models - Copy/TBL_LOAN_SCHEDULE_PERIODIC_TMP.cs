namespace FintrakBanking.Entities.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("FINTRAKBANKING.TBL_LOAN_SCHEDULE_PERIODIC_TMP")]
    public partial class TBL_LOAN_SCHEDULE_PERIODIC_TMP
    {
        [Key]
        [Column(Order = 0)]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public int PERIODICSCHEDULEID { get; set; }

        [Key]
        [Column(Order = 1)]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public int LOANID { get; set; }

        [Key]
        [Column(Order = 2)]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public int PAYMENTNUMBER { get; set; }

        [Key]
        [Column(Order = 3)]
        public decimal STARTPRINCIPALAMOUNT { get; set; }

        [Key]
        [Column(Order = 4)]
        public decimal PERIODPAYMENTAMOUNT { get; set; }

        [Key]
        [Column(Order = 5)]
        public decimal PERIODINTERESTAMOUNT { get; set; }

        [Key]
        [Column(Order = 6)]
        public decimal PERIODPRINCIPALAMOUNT { get; set; }

        [Key]
        [Column(Order = 7)]
        public decimal ENDPRINCIPALAMOUNT { get; set; }

        [Key]
        [Column(Order = 8)]
        public decimal INTERESTRATE { get; set; }

        [Key]
        [Column(Order = 9)]
        public decimal AMORTISEDSTARTPRINCIPALAMOUNT { get; set; }

        [Key]
        [Column(Order = 10)]
        public decimal AMORTISEDPERIODPAYMENTAMOUNT { get; set; }

        [Key]
        [Column(Order = 11)]
        public decimal AMORTISEDPERIODINTERESTAMOUNT { get; set; }

        [Key]
        [Column(Order = 12)]
        public decimal AMORTISEDPERIODPRINCIPALAMOUNT { get; set; }

        [Key]
        [Column(Order = 13)]
        public decimal AMORTISEDENDPRINCIPALAMOUNT { get; set; }

        [Key]
        [Column(Order = 14)]
        public decimal EFFECTIVEINTERESTRATE { get; set; }

        [Key]
        [Column(Order = 15)]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public int CREATEDBY { get; set; }

        [Key]
        [Column(Order = 16)]
        public DateTime DATETIMECREATED { get; set; }

        public int? LASTUPDATEDBY { get; set; }

        public DateTime? DATETIMEUPDATED { get; set; }

        public DateTime? PAYMENTDATE { get; set; }
    }
}
