namespace FintrakBanking.Entities.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("TBL_MIS_INFO")]
    public partial class TBL_MIS_INFO
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public TBL_MIS_INFO()
        {
            TBL_MIS_INFO1 = new HashSet<TBL_MIS_INFO>();
            TBL_TEMP_STAFF = new HashSet<TBL_TEMP_STAFF>();
        }

        [Key]
        public int MISINFOID { get; set; }

        [Required]
        //[StringLength(50)]
        public string MISCODE { get; set; }

        [Required]
        //[StringLength(50)]
        public string MISNAME { get; set; }

        public short? MISTYPEID { get; set; }

        public int? COMPANYID { get; set; }

        public int? PARENTMISINFOID { get; set; }

        public int? CREATEDBY { get; set; }

        public int? LASTUPDATEDBY { get; set; }

        public DateTime? DATETIMECREATED { get; set; }

        public DateTime? DATETIMEUPDATED { get; set; }

        public virtual TBL_COMPANY TBL_COMPANY { get; set; }

        public virtual TBL_MIS_TYPE TBL_MIS_TYPE { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<TBL_MIS_INFO> TBL_MIS_INFO1 { get; set; }

        public virtual TBL_MIS_INFO TBL_MIS_INFO2 { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<TBL_TEMP_STAFF> TBL_TEMP_STAFF { get; set; }
    }
}
