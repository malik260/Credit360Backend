namespace FintrakBanking.Entities.SQLServerModel
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("core.TBL_MIS_TYPE")]
    public partial class TBL_MIS_TYPE
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public TBL_MIS_TYPE()
        {
            TBL_MIS_INFO = new HashSet<TBL_MIS_INFO>();
        }

        [Key]
        public short MISTYPEID { get; set; }

        [Required]
        //[StringLength(50)]
        public string MISTYPE { get; set; }

        //[StringLength(50)]
        public string CATEGORY { get; set; }

        public int? CREATEDBY { get; set; }

        public int? LASTUPDATEDBY { get; set; }

        public DateTime? DATETIMECREATED { get; set; }

        public DateTime? DATETIMEUPDATED { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<TBL_MIS_INFO> TBL_MIS_INFO { get; set; }
    }
}
