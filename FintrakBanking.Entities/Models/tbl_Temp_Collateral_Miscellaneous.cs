namespace FintrakBanking.Entities.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("temp.tbl_Temp_Collateral_Miscellaneous")]
    public partial class tbl_Temp_Collateral_Miscellaneous
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public tbl_Temp_Collateral_Miscellaneous()
        {
            tbl_Temp_Collateral_Miscellaneous_Notes = new HashSet<tbl_Temp_Collateral_Miscellaneous_Notes>();
        }

        [Key]
        public int CollateralMiscellaneousId { get; set; }

        public int CollateralCustomerId { get; set; }

        public bool IsOwnedByCustomer { get; set; }

        [StringLength(50)]
        public string NameOfSecurity { get; set; }

        [Column(TypeName = "money")]
        public decimal SecurityValue { get; set; }

        [Required]
        [StringLength(100)]
        public string Note { get; set; }

        public virtual tbl_Temp_Collateral_Customer tbl_Temp_Collateral_Customer { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<tbl_Temp_Collateral_Miscellaneous_Notes> tbl_Temp_Collateral_Miscellaneous_Notes { get; set; }
    }
}
