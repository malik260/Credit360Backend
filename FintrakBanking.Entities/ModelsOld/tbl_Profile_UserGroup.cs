namespace FintrakBanking.Entities.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("core.tbl_Profile_UserGroup")]
    public partial class tbl_Profile_UserGroup
    {
        [Key]
        public int UserGroupId { get; set; }

        public int UserId { get; set; }

        public short GroupId { get; set; }

        public int CreatedBy { get; set; }

        public int? LastUpdatedBy { get; set; }

        public DateTime DateTimeCreated { get; set; }

        public DateTime? DateTimeUpdated { get; set; }

        public virtual tbl_Profile_Group tbl_Profile_Group { get; set; }

        public virtual tbl_Profile_User tbl_Profile_User { get; set; }
    }
}
