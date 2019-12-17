namespace FintrakBanking.Entities.SQLServerModel
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("core.TBL_JOB_REQUEST_STATUS")]
    public partial class TBL_JOB_REQUEST_STATUS
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public TBL_JOB_REQUEST_STATUS()
        {
            TBL_JOB_REQUEST = new HashSet<TBL_JOB_REQUEST>();
            TBL_JOB_REQUEST_STATUS_FEEDBAK = new HashSet<TBL_JOB_REQUEST_STATUS_FEEDBAK>();
        }

        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public short REQUESTSTATUSID { get; set; }

        [Required]
        //[StringLength(50)]
        public string STATUSNAME { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<TBL_JOB_REQUEST> TBL_JOB_REQUEST { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<TBL_JOB_REQUEST_STATUS_FEEDBAK> TBL_JOB_REQUEST_STATUS_FEEDBAK { get; set; }
    }
}
