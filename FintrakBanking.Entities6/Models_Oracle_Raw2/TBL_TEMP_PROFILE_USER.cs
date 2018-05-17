namespace FintrakBanking.Entities.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("FINTRAKBANKING.TBL_TEMP_PROFILE_USER")]
    public partial class TBL_TEMP_PROFILE_USER
    {
        public TBL_TEMP_PROFILE_USER()
        {
            TBL_TEMP_PROFILE_ADTN_ACTIVITY = new HashSet<TBL_TEMP_PROFILE_ADTN_ACTIVITY>();
            TBL_TEMP_PROFILE_USERGROUP = new HashSet<TBL_TEMP_PROFILE_USERGROUP>();
        }

        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public int TEMPUSERID { get; set; }

        public int TEMPSTAFFID { get; set; }

        [Required]
        [StringLength(50)]
        public string USERNAME { get; set; }

        [Required]
        [StringLength(2000)]
        public string PASSWORD { get; set; }

        public int? ISFIRSTLOGINATTEMPT { get; set; }

        public int ISLOCKED { get; set; }

        public int? FAILEDLOGONATTEMPT { get; set; }

        [StringLength(500)]
        public string SECURITYQUESTION { get; set; }

        [StringLength(500)]
        public string SECURITYANSWER { get; set; }

        public DateTime? NEXTPASSWORDCHANGEDATE { get; set; }

        public int ISACTIVE { get; set; }

        public DateTime? DEACTIVATEDDATE { get; set; }

        public DateTime? LASTLOGINDATE { get; set; }

        public DateTime? LASTLOCKOUTDATE { get; set; }

        [StringLength(36)]
        public string LOGINCODE { get; set; }

        public int? CREATEDBY { get; set; }

        public int? LASTUPDATEDBY { get; set; }

        public DateTime? DATETIMECREATED { get; set; }

        public DateTime? DATETIMEUPDATED { get; set; }

        public int APPROVALSTATUS { get; set; }

        public DateTime? DATEAPPROVED { get; set; }

        public int ISCURRENT { get; set; }

        public int APPROVALSTATUSID { get; set; }

        public virtual ICollection<TBL_TEMP_PROFILE_ADTN_ACTIVITY> TBL_TEMP_PROFILE_ADTN_ACTIVITY { get; set; }

        public virtual ICollection<TBL_TEMP_PROFILE_USERGROUP> TBL_TEMP_PROFILE_USERGROUP { get; set; }

        public virtual TBL_TEMP_STAFF TBL_TEMP_STAFF { get; set; }
    }
}
