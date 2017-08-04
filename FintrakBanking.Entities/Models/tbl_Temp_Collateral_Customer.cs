namespace FintrakBanking.Entities.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("temp.tbl_Temp_Collateral_Customer")]
    public partial class tbl_Temp_Collateral_Customer
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public tbl_Temp_Collateral_Customer()
        {
            tbl_Temp_Collateral_Casa = new HashSet<tbl_Temp_Collateral_Casa>();
            tbl_Temp_Collateral_Documents = new HashSet<tbl_Temp_Collateral_Documents>();
            tbl_Temp_Collateral_Marketable_Security = new HashSet<tbl_Temp_Collateral_Marketable_Security>();
            tbl_Temp_Collateral_Deposit = new HashSet<tbl_Temp_Collateral_Deposit>();
            tbl_Temp_Collateral_Gaurantee = new HashSet<tbl_Temp_Collateral_Gaurantee>();
            tbl_Temp_Collateral_Miscellaneous = new HashSet<tbl_Temp_Collateral_Miscellaneous>();
            tbl_Temp_Collateral_Immovable_Property = new HashSet<tbl_Temp_Collateral_Immovable_Property>();
        }

        [Key]
        public int CollateralCustomerId { get; set; }

        public int CollateralTypeId { get; set; }

        [Required]
        [StringLength(50)]
        public string CollateralCode { get; set; }

        public short CurrencyId { get; set; }

        public int CompanyId { get; set; }

        public bool AllowSharing { get; set; }

        public bool IsLocationBased { get; set; }

        public int? ValuationCycle { get; set; }

        public double HairCut { get; set; }

        public int CustomerId { get; set; }

        [StringLength(50)]
        public string CamRefNumber { get; set; }

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

        public bool IsCurrent { get; set; }

        public short ApprovalStatusId { get; set; }

        public virtual tbl_Approval_Status tbl_Approval_Status { get; set; }

        public virtual tbl_Company tbl_Company { get; set; }

        public virtual tbl_Customer tbl_Customer { get; set; }

        public virtual tbl_Collateral_Type tbl_Collateral_Type { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<tbl_Temp_Collateral_Casa> tbl_Temp_Collateral_Casa { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<tbl_Temp_Collateral_Documents> tbl_Temp_Collateral_Documents { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<tbl_Temp_Collateral_Marketable_Security> tbl_Temp_Collateral_Marketable_Security { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<tbl_Temp_Collateral_Deposit> tbl_Temp_Collateral_Deposit { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<tbl_Temp_Collateral_Gaurantee> tbl_Temp_Collateral_Gaurantee { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<tbl_Temp_Collateral_Miscellaneous> tbl_Temp_Collateral_Miscellaneous { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<tbl_Temp_Collateral_Immovable_Property> tbl_Temp_Collateral_Immovable_Property { get; set; }
    }
}
