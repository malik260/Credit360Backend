namespace FintrakBanking.Entities.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("credit.tbl_Loan_Application")]
    public partial class tbl_Loan_Application
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public tbl_Loan_Application()
        {
            tbl_Credit_Appraisal_Memorandum = new HashSet<tbl_Credit_Appraisal_Memorandum>();
            tbl_Loan = new HashSet<tbl_Loan>();
            tbl_Loan_Condition_Precedent = new HashSet<tbl_Loan_Condition_Precedent>();
            tbl_Loan_Archive = new HashSet<tbl_Loan_Archive>();
            tbl_Loan_Collateral_Mapping = new HashSet<tbl_Loan_Collateral_Mapping>();
            tbl_Loan_Contingent = new HashSet<tbl_Loan_Contingent>();
            tbl_Loan_Application_Collateral = new HashSet<tbl_Loan_Application_Collateral>();
            tbl_Loan_Revolving = new HashSet<tbl_Loan_Revolving>();
            tbl_Risk_Assessment = new HashSet<tbl_Risk_Assessment>();
        }

        [Key]
        public int LoanApplicationId { get; set; }

        [Required]
        [StringLength(50)]
        public string ApplicationReferenceNumber { get; set; }

        public int? LoanPreliminaryEvaluationId { get; set; }

        public int CompanyId { get; set; }

        public int? CustomerId { get; set; }

        public short BranchId { get; set; }

        public short CurrencyId { get; set; }

        public short ProductId { get; set; }

        public short SubSectorId { get; set; }

        public int CasaAccountId { get; set; }

        public double ExchangeRate { get; set; }

        public int? CustomerGroupId { get; set; }

        public short LoanTypeId { get; set; }

        public int RelationshipOfficerId { get; set; }

        public int RelationshipManagerId { get; set; }

        [Column(TypeName = "date")]
        public DateTime ApplicationDate { get; set; }

        [Column(TypeName = "money")]
        public decimal PrincipalAmount { get; set; }

        public double InterestRate { get; set; }

        public int Tenor { get; set; }

        [Required]
        public string LoanInformation { get; set; }

        [Required]
        [StringLength(50)]
        public string MISCode { get; set; }

        [Required]
        [StringLength(50)]
        public string TeamMISCode { get; set; }

        public bool IsRealatedParty { get; set; }

        public bool IsPoliticallyExposed { get; set; }

        public int CreatedBy { get; set; }

        public DateTime DateTimeCreated { get; set; }

        public int? LastUpdatedBy { get; set; }

        public DateTime? DateTimeUpdated { get; set; }

        public bool Deleted { get; set; }

        public int? DeletedBy { get; set; }

        public DateTime? DateTimeDeleted { get; set; }

        public DateTime SystemDateTime { get; set; }

        public int ApprovalStatusId { get; set; }

        public short ApplicationStatusId { get; set; }

        public DateTime? DateActedOn { get; set; }

        public int? ActedOnBy { get; set; }

        public bool SubmittedForAppraisal { get; set; }

        public virtual tbl_Branch tbl_Branch { get; set; }

        public virtual tbl_CASA tbl_CASA { get; set; }

        public virtual tbl_Company tbl_Company { get; set; }

        public virtual tbl_Currency tbl_Currency { get; set; }

        public virtual tbl_Customer tbl_Customer { get; set; }

        public virtual tbl_Customer_Group tbl_Customer_Group { get; set; }

        public virtual tbl_Product tbl_Product { get; set; }

        public virtual tbl_Staff tbl_Staff { get; set; }

        public virtual tbl_Staff tbl_Staff1 { get; set; }

        public virtual tbl_Sub_Sector tbl_Sub_Sector { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<tbl_Credit_Appraisal_Memorandum> tbl_Credit_Appraisal_Memorandum { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<tbl_Loan> tbl_Loan { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<tbl_Loan_Condition_Precedent> tbl_Loan_Condition_Precedent { get; set; }

        public virtual tbl_Loan_Application_Status tbl_Loan_Application_Status { get; set; }

        public virtual tbl_Loan_Preliminary_Evaluation tbl_Loan_Preliminary_Evaluation { get; set; }

        public virtual tbl_Loan_Type tbl_Loan_Type { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<tbl_Loan_Archive> tbl_Loan_Archive { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<tbl_Loan_Collateral_Mapping> tbl_Loan_Collateral_Mapping { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<tbl_Loan_Contingent> tbl_Loan_Contingent { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<tbl_Loan_Application_Collateral> tbl_Loan_Application_Collateral { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<tbl_Loan_Revolving> tbl_Loan_Revolving { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<tbl_Risk_Assessment> tbl_Risk_Assessment { get; set; }
    }
}
