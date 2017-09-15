namespace FintrakBanking.Entities.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("core.tbl_CASA")]
    public partial class tbl_CASA
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public tbl_CASA()
        {
            tbl_Finance_Transaction = new HashSet<tbl_Finance_Transaction>();
            tbl_Loan_Application = new HashSet<tbl_Loan_Application>();
            tbl_Loan_Archive = new HashSet<tbl_Loan_Archive>();
            tbl_Loan_Archive1 = new HashSet<tbl_Loan_Archive>();
            tbl_Loan_Contingent = new HashSet<tbl_Loan_Contingent>();
            tbl_Loan = new HashSet<tbl_Loan>();
            tbl_Loan_Revolving = new HashSet<tbl_Loan_Revolving>();
            tbl_Loan1 = new HashSet<tbl_Loan>();
        }

        [Key]
        public int CasaAccountId { get; set; }

        [Required]
        [StringLength(50)]
        public string ProductAccountNumber { get; set; }

        [Required]
        [StringLength(100)]
        public string ProductAccountName { get; set; }

        public int CustomerId { get; set; }

        public short ProductId { get; set; }

        public int CompanyId { get; set; }

        public short BranchId { get; set; }

        public short CurrencyId { get; set; }

        public bool IsCurrentAccount { get; set; }

        public int? Tenor { get; set; }

        public decimal? InterestRate { get; set; }

        [Column(TypeName = "date")]
        public DateTime? EffectiveDate { get; set; }

        [Column(TypeName = "date")]
        public DateTime? TerminalDate { get; set; }

        public int? ActionBy { get; set; }

        public DateTime? ActionDate { get; set; }

        public short AccountStatusId { get; set; }

        public int? OperationId { get; set; }

        [Column(TypeName = "money")]
        public decimal AvailableBalance { get; set; }

        [Column(TypeName = "money")]
        public decimal LedgerBalance { get; set; }

        public int? RelationshipOfficerId { get; set; }

        public int? RelationshipManagerId { get; set; }

        [StringLength(50)]
        public string MISCode { get; set; }

        [StringLength(50)]
        public string TeamMISCode { get; set; }

        [Column(TypeName = "money")]
        public decimal? OverdraftAmount { get; set; }

        [Column(TypeName = "money")]
        public decimal? OverdraftInterestRate { get; set; }

        [Column(TypeName = "date")]
        public DateTime? OverdraftExpiryDate { get; set; }

        public bool? HasOverdraft { get; set; }

        [Column(TypeName = "money")]
        public decimal LienAmount { get; set; }

        public bool HasLien { get; set; }

        public short PostNoStatusId { get; set; }

        [StringLength(50)]
        public string OldProductAccountNumber1 { get; set; }

        [StringLength(50)]
        public string OldProductAccountNumber2 { get; set; }

        [StringLength(50)]
        public string OldProductAccountNumber3 { get; set; }

        [StringLength(50)]
        public string RefreshBatchId { get; set; }

        public DateTime? LastRefreshDatetime { get; set; }

        public int? CreatedBy { get; set; }

        public int? LastUpdatedBy { get; set; }

        public DateTime? DateTimeCreated { get; set; }

        public DateTime? DateTimeUpdated { get; set; }

        public bool Deleted { get; set; }

        public int? DeletedBy { get; set; }

        public DateTime? DateTimeDeleted { get; set; }

        public short? AprovalStatusId { get; set; }

        public short CustomerSensitivityLevelId { get; set; }

        public virtual tbl_Branch tbl_Branch { get; set; }

        public virtual tbl_CASA_AccountStatus tbl_CASA_AccountStatus { get; set; }

        public virtual tbl_CASA_PostNoStatus tbl_CASA_PostNoStatus { get; set; }

        public virtual tbl_Company tbl_Company { get; set; }

        public virtual tbl_Currency tbl_Currency { get; set; }

        public virtual tbl_Customer tbl_Customer { get; set; }

        public virtual tbl_Customer_Sensitivity_Level tbl_Customer_Sensitivity_Level { get; set; }

        public virtual tbl_Product tbl_Product { get; set; }

        public virtual tbl_Staff tbl_Staff { get; set; }

        public virtual tbl_Staff tbl_Staff1 { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<tbl_Finance_Transaction> tbl_Finance_Transaction { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<tbl_Loan_Application> tbl_Loan_Application { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<tbl_Loan_Archive> tbl_Loan_Archive { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<tbl_Loan_Archive> tbl_Loan_Archive1 { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<tbl_Loan_Contingent> tbl_Loan_Contingent { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<tbl_Loan> tbl_Loan { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<tbl_Loan_Revolving> tbl_Loan_Revolving { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<tbl_Loan> tbl_Loan1 { get; set; }
    }
}
