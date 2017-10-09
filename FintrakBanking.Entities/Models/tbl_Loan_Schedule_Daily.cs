namespace FintrakBanking.Entities.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("credit.tbl_Loan_Schedule_Daily")]
    public partial class tbl_Loan_Schedule_Daily
    {
        [Key]
        public int DailyScheduleId { get; set; }

        public int LoanId { get; set; }

        public int PaymentNumber { get; set; }

        [Column(TypeName = "date")]
        public DateTime Date { get; set; }

        [Column(TypeName = "date")]
        public DateTime PaymentDate { get; set; }

        [Column(TypeName = "money")]
        public decimal OpeningBalance { get; set; }

        [Column(TypeName = "money")]
        public decimal StartPrincipalAmount { get; set; }

        [Column(TypeName = "money")]
        public decimal DailyPaymentAmount { get; set; }

        [Column(TypeName = "money")]
        public decimal DailyInterestAmount { get; set; }

        [Column(TypeName = "money")]
        public decimal DailyPrincipalAmount { get; set; }

        [Column(TypeName = "money")]
        public decimal ClosingBalance { get; set; }

        [Column(TypeName = "money")]
        public decimal EndPrincipalAmount { get; set; }

        [Column(TypeName = "money")]
        public decimal AccruedInterest { get; set; }

        [Column(TypeName = "money")]
        public decimal AmortisedCost { get; set; }

        public double InterestRate { get; set; }

        [Column(TypeName = "money")]
        public decimal AmortisedOpeningBalance { get; set; }

        [Column(TypeName = "money")]
        public decimal AmortisedStartPrincipalAmount { get; set; }

        [Column(TypeName = "money")]
        public decimal AmortisedDailyPaymentAmount { get; set; }

        [Column(TypeName = "money")]
        public decimal AmortisedDailyInterestAmount { get; set; }

        [Column(TypeName = "money")]
        public decimal AmortisedDailyPrincipalAmount { get; set; }

        [Column(TypeName = "money")]
        public decimal AmortisedClosingBalance { get; set; }

        [Column(TypeName = "money")]
        public decimal AmortisedEndPrincipalAmount { get; set; }

        [Column(TypeName = "money")]
        public decimal AmortisedAccruedInterest { get; set; }

        [Column(TypeName = "money")]
        public decimal Amortised_AmortisedCost { get; set; }

        [Column(TypeName = "money")]
        public decimal DiscountPremium { get; set; }

        [Column(TypeName = "money")]
        public decimal UnEarnedFee { get; set; }

        [Column(TypeName = "money")]
        public decimal EarnedFee { get; set; }

        public double EffectiveInterestRate { get; set; }

        public int NumberOfPeriods { get; set; }

        [Column(TypeName = "money")]
        public decimal BallonAmount { get; set; }

        public int CreatedBy { get; set; }

        public DateTime DateTimeCreated { get; set; }

        public int? LastUpdatedBy { get; set; }

        public DateTime? DateTimeUpdated { get; set; }
    }
}
