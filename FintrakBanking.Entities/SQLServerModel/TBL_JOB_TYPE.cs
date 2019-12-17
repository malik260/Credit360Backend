namespace FintrakBanking.Entities.SQLServerModel
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("core.TBL_JOB_TYPE")]
    public partial class TBL_JOB_TYPE
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public TBL_JOB_TYPE()
        {
            TBL_JOB_REQUEST = new HashSet<TBL_JOB_REQUEST>();
            TBL_JOB_REQUEST_STATUS_FEEDBAK = new HashSet<TBL_JOB_REQUEST_STATUS_FEEDBAK>();
            TBL_JOB_TYPE_DEPARTMENT = new HashSet<TBL_JOB_TYPE_DEPARTMENT>();
            TBL_JOB_TYPE_SUB = new HashSet<TBL_JOB_TYPE_SUB>();
        }

        [Key]
        public short JOBTYPEID { get; set; }

        [Required]
        //[StringLength(100)]
        public string JOBTYPENAME { get; set; }

        public bool INUSE { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<TBL_JOB_REQUEST> TBL_JOB_REQUEST { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<TBL_JOB_REQUEST_STATUS_FEEDBAK> TBL_JOB_REQUEST_STATUS_FEEDBAK { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<TBL_JOB_TYPE_DEPARTMENT> TBL_JOB_TYPE_DEPARTMENT { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<TBL_JOB_TYPE_SUB> TBL_JOB_TYPE_SUB { get; set; }
    }
}
