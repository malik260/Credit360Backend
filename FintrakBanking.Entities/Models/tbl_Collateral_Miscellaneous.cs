namespace FintrakBanking.Entities.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("credit.tbl_Collateral_Miscellaneous")]
    public partial class tbl_Collateral_Miscellaneous
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public tbl_Collateral_Miscellaneous()
        {
            tbl_Collateral_Miscellaneous_Notes = new HashSet<tbl_Collateral_Miscellaneous_Notes>();
        }

        [Key]
        public int CollateralMiscellaneousId { get; set; }

        public int CollateralCustomerId { get; set; }

        [Required]
        [StringLength(100)]
        public string CollateralDescription { get; set; }

        public int Units { get; set; }

        [Column(TypeName = "money")]
        public decimal UnitValue { get; set; }

        [StringLength(500)]
        public string Remark { get; set; }

        public int CreatedBy { get; set; }

        public int? LastUpdatedBy { get; set; }

        public DateTime DateTimeCreated { get; set; }

        public DateTime? DateTimeUpdated { get; set; }

        public bool Deleted { get; set; }

        public int? DeletedBy { get; set; }

        public DateTime? DateTimeDeleted { get; set; }

        public virtual tbl_Collateral_Customer tbl_Collateral_Customer { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<tbl_Collateral_Miscellaneous_Notes> tbl_Collateral_Miscellaneous_Notes { get; set; }
    }
}
