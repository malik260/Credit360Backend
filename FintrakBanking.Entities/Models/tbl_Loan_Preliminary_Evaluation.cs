namespace FintrakBanking.Entities.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("credit.tbl_Loan_Preliminary_Evaluation")]
    public partial class tbl_Loan_Preliminary_Evaluation
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public tbl_Loan_Preliminary_Evaluation()
        {
            tbl_Loan_Application = new HashSet<tbl_Loan_Application>();
        }

        [Key]
        public int LoanPreliminaryEvaluationId { get; set; }

        [Required]
        [StringLength(50)]
        public string PreliminaryEvaluationCode { get; set; }

        public int CompanyId { get; set; }

        public short BranchId { get; set; }

        public int CustomerId { get; set; }

        public int RelationshipOfficerId { get; set; }

        public int RelationshipManagerId { get; set; }

        [Required]
        [StringLength(150)]
        public string ProjectDescription { get; set; }

        [Required]
        [StringLength(300)]
        public string ClientDescription { get; set; }

        [StringLength(300)]
        public string OwnershipStructure { get; set; }

        [Required]
        [StringLength(500)]
        public string ProjectFinancingPlan { get; set; }

        [Required]
        [StringLength(150)]
        public string ExistingExposure { get; set; }

        [Required]
        [StringLength(150)]
        public string BankRole { get; set; }

        [Required]
        [StringLength(500)]
        public string CollateralArrangement { get; set; }

        [Required]
        public string ProposedTermsAndConditions { get; set; }

        [Required]
        [StringLength(500)]
        public string ImplementationArrangements { get; set; }

        [Required]
        public string MarketDemand { get; set; }

        [Required]
        public string BusinessProfile { get; set; }

        [Required]
        public string RisksAndConcerns { get; set; }

        [Required]
        [StringLength(500)]
        public string PrudentialExposureLimitImplications { get; set; }

        [Required]
        [StringLength(500)]
        public string EnvironmentalImpact { get; set; }

        [Required]
        [StringLength(500)]
        public string PortfolioStrategicAlignment { get; set; }

        [Required]
        [StringLength(500)]
        public string CommercialViabilityAssessment { get; set; }

        [Required]
        [StringLength(50)]
        public string TaxIdentificationNumber { get; set; }

        [Required]
        [StringLength(50)]
        public string RegistrationNumber { get; set; }

        public short ApprovalStatusId { get; set; }

        public bool IsCurrent { get; set; }

        public bool SentForEvaluation { get; set; }

        public bool SentForLoanApplication { get; set; }

        [Column(TypeName = "money")]
        public decimal LoanAmount { get; set; }

        public short ProductClassId { get; set; }

        public short SubSectorId { get; set; }

        public short LoanTypeId { get; set; }

        public int CreatedBy { get; set; }

        public DateTime DateTimeCreated { get; set; }

        public int? LastUpdatedBy { get; set; }

        public DateTime? DateTimeUpdated { get; set; }

        public DateTime? DateApproved { get; set; }

        public bool Deleted { get; set; }

        public int? DeletedBy { get; set; }

        public DateTime? DateTimeDeleted { get; set; }

        public virtual tbl_Branch tbl_Branch { get; set; }

        public virtual tbl_Company tbl_Company { get; set; }

        public virtual tbl_Customer tbl_Customer { get; set; }

        public virtual tbl_Product_Class tbl_Product_Class { get; set; }

        public virtual tbl_Staff tbl_Staff { get; set; }

        public virtual tbl_Staff tbl_Staff1 { get; set; }

        public virtual tbl_Sub_Sector tbl_Sub_Sector { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<tbl_Loan_Application> tbl_Loan_Application { get; set; }

        public virtual tbl_Loan_Type tbl_Loan_Type { get; set; }
    }
}
