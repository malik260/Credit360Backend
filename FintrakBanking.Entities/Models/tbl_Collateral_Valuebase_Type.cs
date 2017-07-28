namespace FintrakBanking.Entities.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("credit.tbl_Collateral_Valuebase_Type")]
    public partial class tbl_Collateral_Valuebase_Type
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public tbl_Collateral_Valuebase_Type()
        {
            tbl_Collateral_Plant_And_Equipment = new HashSet<tbl_Collateral_Plant_And_Equipment>();
        }

        [Key]
        public short CollateralValueBaseTypeId { get; set; }

        [Required]
        [StringLength(100)]
        public string ValueBaseTypeName { get; set; }

        public int CollateralTypeId { get; set; }

        [StringLength(500)]
        public string Remark { get; set; }

        public int CreatedBy { get; set; }

        public int? LastUpdatedBy { get; set; }

        public DateTime DateTimeCreated { get; set; }

        public DateTime? DateTimeUpdated { get; set; }

        public bool Deleted { get; set; }

        public int? DeletedBy { get; set; }

        public DateTime? DateTimeDeleted { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<tbl_Collateral_Plant_And_Equipment> tbl_Collateral_Plant_And_Equipment { get; set; }

        public virtual tbl_Collateral_Type tbl_Collateral_Type { get; set; }
    }
}
