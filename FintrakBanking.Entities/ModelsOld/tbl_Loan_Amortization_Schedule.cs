namespace FintrakBanking.Entities.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("credit.tbl_Loan_Amortization_Schedule")]
    public partial class tbl_Loan_Amortization_Schedule
    {
        [Key]
        public short LoanAmortizationScheduleId { get; set; }

        public int? LoanId { get; set; }

        public int DayNumber { get; set; }

        public decimal? PrincipalBalance { get; set; }

        public DateTime? NextPayDay { get; set; }

        public decimal? InterestAccrual { get; set; }

        public decimal? InterestPayment { get; set; }

        public decimal? PrincipalRepayment { get; set; }

        public decimal? TotalRepayment { get; set; }

        public decimal? EndBalance { get; set; }

        public decimal? FeeCharged { get; set; }

        public decimal? CumulativeInterest { get; set; }

        public decimal? CumulativePrincipalRepayment { get; set; }

        public bool? Paid { get; set; }

        public virtual tbl_Loan tbl_Loan { get; set; }
    }
}
