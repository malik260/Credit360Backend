namespace FintrakBanking.Entities.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("TBL_JOB_TYPE_REASSIGNMENT")]
    public partial class TBL_JOB_TYPE_REASSIGNMENT
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public TBL_JOB_TYPE_REASSIGNMENT()
        {
            TBL_JOB_TYPE = new HashSet<TBL_JOB_TYPE>();
        }

        [Key]
        public short REASSIGNMENTID { get; set; }
        public short JOBTYPEID { get; set; }
        public int STAFFID { get; set; }
        public int COMPANYID { get; set; }
        public int CREATEDBY { get; set; }
        public int LASTUPDATEDBY { get; set; }
        public DateTime DATETIMECREATED { get; set; }
        public DateTime DATETIMEUPDATED { get; set; }
        public bool DELETED { get; set; }
        public int DELETEDBY { get; set; }
        public DateTime DATETIMEDELETED { get; set; }


        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<TBL_JOB_TYPE> TBL_JOB_TYPE { get; set; }
    }
}
