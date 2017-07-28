namespace FintrakBanking.Entities.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("credit.tbl_Collateral_Customer")]
    public partial class tbl_Collateral_Customer
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public tbl_Collateral_Customer()
        {
            tbl_Collateral_Documents = new HashSet<tbl_Collateral_Documents>();
            tbl_Collateral_Deposit = new HashSet<tbl_Collateral_Deposit>();
            tbl_Collateral_Gaurantee = new HashSet<tbl_Collateral_Gaurantee>();
            tbl_Collateral_InsurancePolicy = new HashSet<tbl_Collateral_InsurancePolicy>();
            tbl_Collateral_Machine_Detail = new HashSet<tbl_Collateral_Machine_Detail>();
            tbl_Collateral_Marketable_Security = new HashSet<tbl_Collateral_Marketable_Security>();
            tbl_Collateral_Miscellaneous = new HashSet<tbl_Collateral_Miscellaneous>();
            tbl_Collateral_PreciousMetal = new HashSet<tbl_Collateral_PreciousMetal>();
            tbl_Collateral_Property = new HashSet<tbl_Collateral_Property>();
            tbl_Collateral_Vehicle = new HashSet<tbl_Collateral_Vehicle>();
        }

        [Key]
        public int CollateralCustomerId { get; set; }

        public int CollateralTypeId { get; set; }

        [Required]
        [StringLength(50)]
        public string CollateralCode { get; set; }

        [Column(TypeName = "money")]
        public decimal CollateralValue { get; set; }

        public short CurrencyId { get; set; }

        public int CompanyId { get; set; }

        [Column(TypeName = "money")]
        public decimal? LimitContribution { get; set; }

        [Column(TypeName = "date")]
        public DateTime CollateralValueDate { get; set; }

        public int? GraceDays { get; set; }

        public int? Quantity { get; set; }

        public int? ChargeTypeId { get; set; }

        public short? SeniorityOfClaimId { get; set; }

        public decimal? LendableMargin { get; set; }

        public short? ValuerId { get; set; }

        public int CustomerId { get; set; }

        [Column(TypeName = "date")]
        public DateTime? StartDate { get; set; }

        [Column(TypeName = "date")]
        public DateTime? EndDate { get; set; }

        [Column(TypeName = "date")]
        public DateTime? RevisionDate { get; set; }

        [StringLength(50)]
        public string CamRefNumber { get; set; }

        public bool RequireFieldInvestigation { get; set; }

        public bool RequireValuation { get; set; }

        public bool RequireCheck { get; set; }

        public bool AllowShare { get; set; }

        [Column(TypeName = "date")]
        public DateTime? RevaluationDate { get; set; }

        [Column(TypeName = "date")]
        public DateTime? LastValuationDate { get; set; }

        [StringLength(500)]
        public string ValuationSource { get; set; }

        [Column(TypeName = "money")]
        public decimal? ValuationAmount { get; set; }

        public bool ReleaseCollateral { get; set; }

        public DateTime? ReleaseDate { get; set; }

        public int CreatedBy { get; set; }

        public int? LastUpdatedBy { get; set; }

        public DateTime DateTimeCreated { get; set; }

        public DateTime? DateTimeUpdated { get; set; }

        public bool Deleted { get; set; }

        public int? DeletedBy { get; set; }

        public DateTime? DateTimeDeleted { get; set; }

        public int ApprovalStatus { get; set; }

        public DateTime? DateActedOn { get; set; }

        public int? ActedOnBy { get; set; }

        public virtual tbl_Company tbl_Company { get; set; }

        public virtual tbl_Customer tbl_Customer { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<tbl_Collateral_Documents> tbl_Collateral_Documents { get; set; }

        public virtual tbl_Collateral_Type tbl_Collateral_Type { get; set; }

        public virtual tbl_Collateral_Valuers tbl_Collateral_Valuers { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<tbl_Collateral_Deposit> tbl_Collateral_Deposit { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<tbl_Collateral_Gaurantee> tbl_Collateral_Gaurantee { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<tbl_Collateral_InsurancePolicy> tbl_Collateral_InsurancePolicy { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<tbl_Collateral_Machine_Detail> tbl_Collateral_Machine_Detail { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<tbl_Collateral_Marketable_Security> tbl_Collateral_Marketable_Security { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<tbl_Collateral_Miscellaneous> tbl_Collateral_Miscellaneous { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<tbl_Collateral_PreciousMetal> tbl_Collateral_PreciousMetal { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<tbl_Collateral_Property> tbl_Collateral_Property { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<tbl_Collateral_Vehicle> tbl_Collateral_Vehicle { get; set; }
    }
}
