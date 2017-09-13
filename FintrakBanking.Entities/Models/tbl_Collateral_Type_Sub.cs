namespace FintrakBanking.Entities.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("credit.tbl_Collateral_Type_Sub")]
    public partial class tbl_Collateral_Type_Sub
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public tbl_Collateral_Type_Sub()
        {
            tbl_Collateral_Casa = new HashSet<tbl_Collateral_Casa>();
            tbl_Collateral_Immovable_Property = new HashSet<tbl_Collateral_Immovable_Property>();
            tbl_Collateral_Plant_And_Equipment = new HashSet<tbl_Collateral_Plant_And_Equipment>();
            tbl_Temp_Collateral_Casa = new HashSet<tbl_Temp_Collateral_Casa>();
            tbl_Temp_Collateral_Deposit = new HashSet<tbl_Temp_Collateral_Deposit>();
            tbl_Temp_Collateral_Immovable_Property = new HashSet<tbl_Temp_Collateral_Immovable_Property>();
            tbl_Temp_Collateral_Plant_And_Equipment = new HashSet<tbl_Temp_Collateral_Plant_And_Equipment>();
        }

        [Key]
        public short CollateralSubTypeId { get; set; }

        [Required]
        [StringLength(50)]
        public string CollateralSubTypeName { get; set; }

        public int CollateralTypeId { get; set; }

        public double Haircut { get; set; }

        public int RevaluationDuration { get; set; }

        public int CreatedBy { get; set; }

        public int? LastUpdatedBy { get; set; }

        public DateTime DateTimeCreated { get; set; }

        public DateTime? DateTimeUpdated { get; set; }

        public bool Deleted { get; set; }

        public int? DeletedBy { get; set; }

        public DateTime? DateTimeDeleted { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<tbl_Collateral_Casa> tbl_Collateral_Casa { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<tbl_Collateral_Immovable_Property> tbl_Collateral_Immovable_Property { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<tbl_Collateral_Plant_And_Equipment> tbl_Collateral_Plant_And_Equipment { get; set; }

        public virtual tbl_Collateral_Type tbl_Collateral_Type { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<tbl_Temp_Collateral_Casa> tbl_Temp_Collateral_Casa { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<tbl_Temp_Collateral_Deposit> tbl_Temp_Collateral_Deposit { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<tbl_Temp_Collateral_Immovable_Property> tbl_Temp_Collateral_Immovable_Property { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<tbl_Temp_Collateral_Plant_And_Equipment> tbl_Temp_Collateral_Plant_And_Equipment { get; set; }
    }
}
