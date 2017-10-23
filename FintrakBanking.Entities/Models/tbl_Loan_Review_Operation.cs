namespace FintrakBanking.Entities.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("credit.tbl_Loan_Review_Operation")]
    public partial class tbl_Loan_Review_Operation
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public tbl_Loan_Review_Operation()
        {
            tbl_Loan_Review_Operation_Irregular_Schedule = new HashSet<tbl_Loan_Review_Operation_Irregular_Schedule>();
        }

        [Key]
        public int LoanReviewOperationId { get; set; }

        public int LoanId { get; set; }

        public int ProductTypeId { get; set; }

        public int OperationTypeId { get; set; }

        [Column(TypeName = "date")]
        public DateTime EffectiveDate { get; set; }

        [Required]
        public string ReviewDetails { get; set; }

        public double? InterateRate { get; set; }

        [Column(TypeName = "money")]
        public decimal? Prepayment { get; set; }

        public int? PrincipalFrequencyTypeId { get; set; }

        public int? InterestFrequencyTypeId { get; set; }

        [Column(TypeName = "date")]
        public DateTime? PrincipalFirstPaymentDate { get; set; }

        [Column(TypeName = "date")]
        public DateTime? InterestFirstPaymentDate { get; set; }

        [Column(TypeName = "date")]
        public DateTime? MaturityDate { get; set; }

        public int? Tenor { get; set; }

        public int? CASA_AccountId { get; set; }

        [Column(TypeName = "money")]
        public decimal? OverDraftTopup { get; set; }

        [Column(TypeName = "money")]
        public decimal? Fee_Charges { get; set; }

        public int ApprovalStatusId { get; set; }

        public bool IsManagementInterestRate { get; set; }

        public bool OperationCompleted { get; set; }

        public int CreatedBy { get; set; }

        [Column(TypeName = "date")]
        public DateTime DateCreated { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<tbl_Loan_Review_Operation_Irregular_Schedule> tbl_Loan_Review_Operation_Irregular_Schedule { get; set; }
    }
}
