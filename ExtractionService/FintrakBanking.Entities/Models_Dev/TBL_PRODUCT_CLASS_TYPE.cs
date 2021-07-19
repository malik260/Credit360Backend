namespace FintrakBanking.Entities.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("FINTRAKBANKING.TBL_PRODUCT_CLASS_TYPE")]
    public partial class TBL_PRODUCT_CLASS_TYPE
    {
        public TBL_PRODUCT_CLASS_TYPE()
        {
            TBL_PRODUCT_CLASS = new HashSet<TBL_PRODUCT_CLASS>();
        }

        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public short PRODUCTCLASSTYPEID { get; set; }

        [Required]
        [StringLength(50)]
        public string PRODUCTCLASSTYPENAME { get; set; }

        public virtual ICollection<TBL_PRODUCT_CLASS> TBL_PRODUCT_CLASS { get; set; }
    }
}
