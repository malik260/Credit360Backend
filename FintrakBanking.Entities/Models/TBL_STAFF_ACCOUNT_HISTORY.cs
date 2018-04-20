namespace FintrakBanking.Entities.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("core.TBL_STAFF_ACCOUNT_HISTORY")]
    public partial class TBL_STAFF_ACCOUNT_HISTORY
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public TBL_STAFF_ACCOUNT_HISTORY()
        {
            TBL_STAFF_ACCOUNT_HISTORY_DTL = new HashSet<TBL_STAFF_ACCOUNT_HISTORY_DTL>();
        }

        [Key]
        public int STAFFACCOUNTHISTORYID { get; set; }

        public int STAFFID { get; set; }

        [Column(TypeName = "date")]
        public DateTime STARTDATE { get; set; }

        [Column(TypeName = "date")]
        public DateTime ENDDATE { get; set; }

        public int NEWSTAFFID { get; set; }

        [Required]
        [StringLength(2000)]
        public string REASONFORCHANGE { get; set; }

        public short APPROVALSTATUSID { get; set; }

        public int CREATEDBY { get; set; }

        public DateTime DATETIMECREATED { get; set; }

        public int? LASTUPDATEDBY { get; set; }

        public DateTime? DATETIMEUPDATED { get; set; }

        public bool DELETED { get; set; }

        public int? DELETEDBY { get; set; }

        public DateTime? DATETIMEDELETED { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<TBL_STAFF_ACCOUNT_HISTORY_DTL> TBL_STAFF_ACCOUNT_HISTORY_DTL { get; set; }
    }
}
