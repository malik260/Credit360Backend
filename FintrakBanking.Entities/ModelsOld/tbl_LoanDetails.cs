namespace FintrakBanking.Entities.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    public partial class tbl_LoanDetails
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public int LoanDetailsId { get; set; }

        public int ProductId { get; set; }

        public int CasaAccountId { get; set; }

        public int LoanId { get; set; }

        public int CustomerId { get; set; }

        [Required]
        [StringLength(50)]
        public string LoanReferenceNumber { get; set; }

        public int Tenor { get; set; }

        public short TenorModeId { get; set; }

        public short PrincipalFrequencyTypeId { get; set; }

        public short InterestFrequencyTypeId { get; set; }

        public short FeeFrequencyTypeId { get; set; }

        public double InterestRate { get; set; }

        [Column(TypeName = "date")]
        public DateTime EffectiveDate { get; set; }

        [Column(TypeName = "date")]
        public DateTime TerminalDate { get; set; }

        [Column(TypeName = "money")]
        public decimal PrincipalAmount { get; set; }

        public int PrincipalInstallmentLeft { get; set; }

        public int InterestInstallmentLeft { get; set; }

        [Column(TypeName = "money")]
        public decimal? EquityContribution { get; set; }

        public decimal? FeePercent { get; set; }

        [Column(TypeName = "date")]
        public DateTime? FirstPrincipalPaymentDate { get; set; }

        [Column(TypeName = "date")]
        public DateTime? FirstInterestPaymentDate { get; set; }

        [Column(TypeName = "money")]
        public decimal? OutstandingPrincipal { get; set; }

        public int? PrincipalAdditionCount { get; set; }

        public int? PrincipalReductionCount { get; set; }

        public bool FixedPrincipal { get; set; }

        public bool ProfileLoan { get; set; }

        public bool Scheduled { get; set; }

        public bool? IsScheduledPrepayment { get; set; }

        [Column(TypeName = "money")]
        public decimal? ScheduledPrepaymentAmount { get; set; }

        [Column(TypeName = "date")]
        public DateTime? ScheduledPrepaymentDate { get; set; }

        public short? ScheduledPrepaymentFrequencyTypeId { get; set; }
    }
}
