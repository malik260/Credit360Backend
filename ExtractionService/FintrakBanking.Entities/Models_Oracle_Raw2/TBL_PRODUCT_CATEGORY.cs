namespace FintrakBanking.Entities.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("FINTRAKBANKING.TBL_PRODUCT_CATEGORY")]
    public partial class TBL_PRODUCT_CATEGORY
    {
        public TBL_PRODUCT_CATEGORY()
        {
            TBL_PRODUCT = new HashSet<TBL_PRODUCT>();
            TBL_TEMP_PRODUCT = new HashSet<TBL_TEMP_PRODUCT>();
        }

        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public int PRODUCTCATEGORYID { get; set; }

        [Required]
        [StringLength(50)]
        public string PRODUCTCATEGORYNAME { get; set; }

        public virtual ICollection<TBL_PRODUCT> TBL_PRODUCT { get; set; }

        public virtual ICollection<TBL_TEMP_PRODUCT> TBL_TEMP_PRODUCT { get; set; }
    }
}
