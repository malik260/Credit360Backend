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
            tbl_Collateral_Misc_Notes = new HashSet<tbl_Collateral_Misc_Notes>();
        }

        [Key]
        public int MiscellaneousId { get; set; }

        public int ColleralCustomerId { get; set; }

<<<<<<< HEAD
        [Required]
        [StringLength(100)]
        public string CollateralDesc { get; set; }
=======
        public bool IsOwnedByCustomer { get; set; }
>>>>>>> 1234ffcc6749be1304beaae4f63e4587aa7a4aba

        [StringLength(50)]
        public string NameOfSecurity { get; set; }

        [Column(TypeName = "money")]
<<<<<<< HEAD
        public decimal UnitValue { get; set; }

        [StringLength(500)]
        public string Remarks { get; set; }

        public int CreatedBy { get; set; }

        public int? LastUpdatedBy { get; set; }

        public DateTime DateTimeCreated { get; set; }
=======
        public decimal SecurityValue { get; set; }
>>>>>>> 1234ffcc6749be1304beaae4f63e4587aa7a4aba

        [Required]
        [StringLength(100)]
        public string Note { get; set; }

        public virtual tbl_Collateral_Customer tbl_Collateral_Customer { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<tbl_Collateral_Misc_Notes> tbl_Collateral_Misc_Notes { get; set; }
    }
}
