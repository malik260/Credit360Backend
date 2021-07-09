namespace FintrakBanking.Entities.SQLServerModel
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("core.TBL_PRODUCT_CLASS_TYPE")]
    public partial class TBL_PRODUCT_CLASS_TYPE
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public TBL_PRODUCT_CLASS_TYPE()
        {
            TBL_PRODUCT_CLASS = new HashSet<TBL_PRODUCT_CLASS>();
        }

        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public short PRODUCTCLASSTYPEID { get; set; }

        [Required]
        //[StringLength(50)]
        public string PRODUCTCLASSTYPENAME { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<TBL_PRODUCT_CLASS> TBL_PRODUCT_CLASS { get; set; }
    }
}
