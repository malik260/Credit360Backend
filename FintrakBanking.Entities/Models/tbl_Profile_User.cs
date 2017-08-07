namespace FintrakBanking.Entities.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("core.tbl_Profile_User")]
    public partial class tbl_Profile_User
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public tbl_Profile_User()
        {
            tbl_Profile_AdditionalActivity = new HashSet<tbl_Profile_AdditionalActivity>();
            tbl_Profile_Priviledge_Activity = new HashSet<tbl_Profile_Priviledge_Activity>();
            tbl_Profile_UserGroup = new HashSet<tbl_Profile_UserGroup>();
        }

        [Key]
        public int UserId { get; set; }

        public int StaffId { get; set; }

        [Required]
        [StringLength(50)]
        public string Username { get; set; }

        [Required]
        [StringLength(2000)]
        public string Password { get; set; }

        public bool? IsFirstLoginAttempt { get; set; }

        public bool IsLocked { get; set; }

        public int? FailedLogonAttempt { get; set; }

        [StringLength(500)]
        public string SecurityQuestion { get; set; }

        [StringLength(500)]
        public string SecurityAnswer { get; set; }

        public DateTime? NextPasswordChangeDate { get; set; }

        public bool IsActive { get; set; }

        public DateTime? DeactivatedDate { get; set; }

        public DateTime? LastLoginDate { get; set; }

        public int? CreatedBy { get; set; }

        public int? LastUpdatedBy { get; set; }

        public DateTime? DateTimeCreated { get; set; }

        public DateTime? DateTimeUpdated { get; set; }

        public bool ApprovalStatus { get; set; }

        public DateTime? DateApproved { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<tbl_Profile_AdditionalActivity> tbl_Profile_AdditionalActivity { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<tbl_Profile_Priviledge_Activity> tbl_Profile_Priviledge_Activity { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<tbl_Profile_UserGroup> tbl_Profile_UserGroup { get; set; }

        public virtual tbl_Staff tbl_Staff { get; set; }
    }
}
