namespace FintrakBanking.Entities.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("credit.tbl_Collateral_Type")]
    public partial class tbl_Collateral_Type
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public tbl_Collateral_Type()
        {
            tbl_Collateral_Customer = new HashSet<tbl_Collateral_Customer>();
            tbl_Temp_Collateral_Customer = new HashSet<tbl_Temp_Collateral_Customer>();
            tbl_Collateral_Type_Sub = new HashSet<tbl_Collateral_Type_Sub>();
            tbl_Collateral_Valuebase_Type = new HashSet<tbl_Collateral_Valuebase_Type>();
            tbl_Product_CollateralType = new HashSet<tbl_Product_CollateralType>();
            tbl_Temp_Product_CollateralType = new HashSet<tbl_Temp_Product_CollateralType>();
            tbl_Temp_Product_CollateralType1 = new HashSet<tbl_Temp_Product_CollateralType>();
        }

        [Key]
        public int CollateralTypeId { get; set; }

        [Required]
        [StringLength(250)]
        public string CollateralTypeName { get; set; }

        public int CompanyId { get; set; }

        [StringLength(500)]
        public string Details { get; set; }

        public bool RequireInsurancePolicy { get; set; }

        public int? ChargeGLAccountId { get; set; }

        public int CreatedBy { get; set; }

        public int? LastUpdatedBy { get; set; }

        public DateTime DateTimeCreated { get; set; }

        public DateTime? DateTimeUpdated { get; set; }

        public bool Deleted { get; set; }

        public int? DeletedBy { get; set; }

        public DateTime? DateTimeDeleted { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<tbl_Collateral_Customer> tbl_Collateral_Customer { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<tbl_Temp_Collateral_Customer> tbl_Temp_Collateral_Customer { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<tbl_Collateral_Type_Sub> tbl_Collateral_Type_Sub { get; set; }

        public virtual tbl_Chart_Of_Account tbl_Chart_Of_Account { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<tbl_Collateral_Valuebase_Type> tbl_Collateral_Valuebase_Type { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<tbl_Product_CollateralType> tbl_Product_CollateralType { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<tbl_Temp_Product_CollateralType> tbl_Temp_Product_CollateralType { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<tbl_Temp_Product_CollateralType> tbl_Temp_Product_CollateralType1 { get; set; }
    }
}
