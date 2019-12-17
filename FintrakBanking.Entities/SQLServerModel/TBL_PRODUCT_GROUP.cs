namespace FintrakBanking.Entities.SQLServerModel
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("core.TBL_PRODUCT_GROUP")]
    public partial class TBL_PRODUCT_GROUP
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public TBL_PRODUCT_GROUP()
        {
            TBL_PRODUCT_TYPE = new HashSet<TBL_PRODUCT_TYPE>();
        }

        [Key]
        public short PRODUCTGROUPID { get; set; }

        [Required]
        //[StringLength(50)]
        public string PRODUCTGROUPCODE { get; set; }

        [Required]
        //[StringLength(50)]
        public string PRODUCTGROUPNAME { get; set; }

        public int CREATEDBY { get; set; }

        public DateTime? DATETIMECREATED { get; set; }

        public bool DELETED { get; set; }

        public DateTime? DATETIMEDELETED { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<TBL_PRODUCT_TYPE> TBL_PRODUCT_TYPE { get; set; }
    }
}
