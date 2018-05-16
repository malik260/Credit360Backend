namespace FintrakBanking.Entities.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("FINTRAKBANKING.TBL_PRODUCT_GROUP")]
    public partial class TBL_PRODUCT_GROUP
    {
        public TBL_PRODUCT_GROUP()
        {
            TBL_PRODUCT_TYPE = new HashSet<TBL_PRODUCT_TYPE>();
        }

        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public int PRODUCTGROUPID { get; set; }

        [Required]
        [StringLength(50)]
        public string PRODUCTGROUPCODE { get; set; }

        [Required]
        [StringLength(50)]
        public string PRODUCTGROUPNAME { get; set; }

        public int CREATEDBY { get; set; }

        public DateTime? DATETIMECREATED { get; set; }

        public int DELETED { get; set; }

        public DateTime? DATETIMEDELETED { get; set; }

        public virtual ICollection<TBL_PRODUCT_TYPE> TBL_PRODUCT_TYPE { get; set; }
    }
}
