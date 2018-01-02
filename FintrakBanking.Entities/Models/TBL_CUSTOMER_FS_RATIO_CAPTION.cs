namespace FintrakBanking.Entities.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("core.TBL_CUSTOMER_FS_RATIO_CAPTION")]
    public partial class TBL_CUSTOMER_FS_RATIO_CAPTION
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public TBL_CUSTOMER_FS_RATIO_CAPTION()
        {
            TBL_CUSTOMER_FS_RATIO_DETAIL = new HashSet<TBL_CUSTOMER_FS_RATIO_DETAIL>();
        }

        [Key]
        public short RATIOCAPTIONID { get; set; }

        [Required]
        [StringLength(200)]
        public string RATIOCAPTION { get; set; }

        public short RATIOTYPEID { get; set; }

        public int COMPANYID { get; set; }

        public bool ANNUALISED { get; set; }

        public int POSITION { get; set; }

        public int CREATEDBY { get; set; }

        public DateTime DATETIMECREATED { get; set; }

        public int? LASTUPDATEDBY { get; set; }

        public DateTime? DATETIMEUPDATED { get; set; }

        public bool DELETED { get; set; }

        public int? DELETEDBY { get; set; }

        public DateTime? DATETIMEDELETED { get; set; }

        public virtual TBL_COMPANY TBL_COMPANY { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<TBL_CUSTOMER_FS_RATIO_DETAIL> TBL_CUSTOMER_FS_RATIO_DETAIL { get; set; }
    }
}
