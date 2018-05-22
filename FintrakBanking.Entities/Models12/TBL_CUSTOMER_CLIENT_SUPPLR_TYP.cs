namespace FintrakBanking.Entities.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("TBL_CUSTOMER_CLIENT_SUPPLR_TYP")]
    public partial class TBL_CUSTOMER_CLIENT_SUPPLR_TYP
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public TBL_CUSTOMER_CLIENT_SUPPLR_TYP()
        {
            TBL_CUSTOMER_CLIENT_SUPPLIER = new HashSet<TBL_CUSTOMER_CLIENT_SUPPLIER>();
        }

        [Key]
        public short CLIENT_SUPPLIERTYPEID { get; set; }

        [Required]
        [StringLength(50)]
        public string CLIENT_SUPPLIERTYPENAME { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<TBL_CUSTOMER_CLIENT_SUPPLIER> TBL_CUSTOMER_CLIENT_SUPPLIER { get; set; }
    }
}
