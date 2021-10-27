namespace FintrakBanking.Entities.SQLServerModel
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("core.TBL_PROFILE_USER")]
    public partial class TBL_PROFILE_USER
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public TBL_PROFILE_USER()
        {
            TBL_PROFILE_ADDITIONALACTIVITY = new HashSet<TBL_PROFILE_ADDITIONALACTIVITY>();
            TBL_PROFILE_PASSWORD_HISTORY = new HashSet<TBL_PROFILE_PASSWORD_HISTORY>();
            TBL_PROFILE_PRIVILEDGE_ACTIVIT = new HashSet<TBL_PROFILE_PRIVILEDGE_ACTIVIT>();
            TBL_PROFILE_USERGROUP = new HashSet<TBL_PROFILE_USERGROUP>();
        }

        [Key]
        public int USERID { get; set; }

        public int STAFFID { get; set; }

        [Required]
        //[StringLength(50)]
        public string USERNAME { get; set; }

        [Required]
        //[StringLength(2000)]
        public string PASSWORD { get; set; }

        public bool? ISFIRSTLOGINATTEMPT { get; set; }

        public bool ISLOCKED { get; set; }

        public int? FAILEDLOGONATTEMPT { get; set; }

        //[StringLength(500)]
        public string SECURITYQUESTION { get; set; }

        //[StringLength(500)]
        public string SECURITYANSWER { get; set; }

        public DateTime? NEXTPASSWORDCHANGEDATE { get; set; }

        public bool ISACTIVE { get; set; }

        public DateTime? DEACTIVATEDDATE { get; set; }

        public DateTime? LASTLOGINDATE { get; set; }

        public DateTime? LASTLOCKOUTDATE { get; set; }

        //[StringLength(100)]
        public string LOGINCODE { get; set; }

        public int? CREATEDBY { get; set; }

        public int? LASTUPDATEDBY { get; set; }

        public DateTime? DATETIMECREATED { get; set; }

        public DateTime? DATETIMEUPDATED { get; set; }

        public bool APPROVALSTATUS { get; set; }

        public DateTime? DATEAPPROVED { get; set; }

        public short APPROVALSTATUSID { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<TBL_PROFILE_ADDITIONALACTIVITY> TBL_PROFILE_ADDITIONALACTIVITY { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<TBL_PROFILE_PASSWORD_HISTORY> TBL_PROFILE_PASSWORD_HISTORY { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<TBL_PROFILE_PRIVILEDGE_ACTIVIT> TBL_PROFILE_PRIVILEDGE_ACTIVIT { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<TBL_PROFILE_USERGROUP> TBL_PROFILE_USERGROUP { get; set; }

        public virtual TBL_STAFF TBL_STAFF { get; set; }
    }
}
