namespace FintrakBanking.Entities.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("FINTRAKBANKING.TBL_PROFILE_GROUP")]
    public partial class TBL_PROFILE_GROUP
    {
        public TBL_PROFILE_GROUP()
        {
            TBL_PROFILE_GROUP_ACTIVITY = new HashSet<TBL_PROFILE_GROUP_ACTIVITY>();
            TBL_PROFILE_STAFF_ROLE_GROUP = new HashSet<TBL_PROFILE_STAFF_ROLE_GROUP>();
            TBL_PROFILE_USERGROUP = new HashSet<TBL_PROFILE_USERGROUP>();
            TBL_TEMP_PROFILE_STAFF_ROL_GRP = new HashSet<TBL_TEMP_PROFILE_STAFF_ROL_GRP>();
            TBL_TEMP_PROFILE_USERGROUP = new HashSet<TBL_TEMP_PROFILE_USERGROUP>();
        }

        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public short GROUPID { get; set; }

        [Required]
        [StringLength(100)]
        public string GROUPNAME { get; set; }

        public int CREATEDBY { get; set; }

        public int? LASTUPDATEDBY { get; set; }

        public DateTime DATETIMECREATED { get; set; }

        public DateTime? DATETIMEUPDATED { get; set; }

        public virtual ICollection<TBL_PROFILE_GROUP_ACTIVITY> TBL_PROFILE_GROUP_ACTIVITY { get; set; }

        public virtual ICollection<TBL_PROFILE_STAFF_ROLE_GROUP> TBL_PROFILE_STAFF_ROLE_GROUP { get; set; }

        public virtual ICollection<TBL_PROFILE_USERGROUP> TBL_PROFILE_USERGROUP { get; set; }

        public virtual ICollection<TBL_TEMP_PROFILE_STAFF_ROL_GRP> TBL_TEMP_PROFILE_STAFF_ROL_GRP { get; set; }

        public virtual ICollection<TBL_TEMP_PROFILE_USERGROUP> TBL_TEMP_PROFILE_USERGROUP { get; set; }
    }
}
