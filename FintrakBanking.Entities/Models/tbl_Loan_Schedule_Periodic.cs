namespace FintrakBanking.Entities.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("credit.tbl_Loan_Schedule_Periodic")]
    public partial class tbl_Loan_Schedule_Periodic
    {
        [Key]
        public int PeriodicScheduleId { get; set; }

        public int LoanId { get; set; }

        public int PaymentNumber { get; set; }

        [Column(TypeName = "date")]
        public DateTime PaymentDate { get; set; }

        [Column(TypeName = "money")]
        public decimal StartPrincipalAmount { get; set; }

        [Column(TypeName = "money")]
        public decimal PeriodPaymentAmount { get; set; }

        [Column(TypeName = "money")]
        public decimal PeriodInterestAmount { get; set; }

        [Column(TypeName = "money")]
        public decimal PeriodPrincipalAmount { get; set; }

        [Column(TypeName = "money")]
        public decimal EndPrincipalAmount { get; set; }

        public double InterestRate { get; set; }

        [Column(TypeName = "money")]
        public decimal AmortisedStartPrincipalAmount { get; set; }

        [Column(TypeName = "money")]
        public decimal AmortisedPeriodPaymentAmount { get; set; }

        [Column(TypeName = "money")]
        public decimal AmortisedPeriodInterestAmount { get; set; }

        [Column(TypeName = "money")]
        public decimal AmortisedPeriodPrincipalAmount { get; set; }

        [Column(TypeName = "money")]
        public decimal AmortisedEndPrincipalAmount { get; set; }

        public double EffectiveInterestRate { get; set; }

        public int CreatedBy { get; set; }

        public DateTime DateTimeCreated { get; set; }

        public int? LastUpdatedBy { get; set; }

        public DateTime? DateTimeUpdated { get; set; }

        public virtual tbl_Loan tbl_Loan { get; set; }
    }
}
