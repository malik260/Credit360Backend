namespace FintrakBanking.Entities.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("credit.TBL_COLLATERAL_MISCELLANEOUS")]
    public partial class TBL_COLLATERAL_MISCELLANEOUS
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public TBL_COLLATERAL_MISCELLANEOUS()
        {
            TBL_COLLATERAL_MISC_NOTES = new HashSet<TBL_COLLATERAL_MISC_NOTES>();
        }

        [Key]
        public int COLLATERALMISCELLANEOUSID { get; set; }

        public int COLLATERALCUSTOMERID { get; set; }

        public bool ISOWNEDBYCUSTOMER { get; set; }

        [StringLength(50)]
        public string NAMEOFSECURITY { get; set; }

        [Column(TypeName = "money")]
        public decimal SECURITYVALUE { get; set; }

        [Required]
        [StringLength(100)]
        public string NOTE { get; set; }

        public virtual TBL_COLLATERAL_CUSTOMER TBL_COLLATERAL_CUSTOMER { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<TBL_COLLATERAL_MISC_NOTES> TBL_COLLATERAL_MISC_NOTES { get; set; }
    }
}
