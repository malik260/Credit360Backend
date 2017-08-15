namespace FintrakBanking.Entities.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("core.tbl_Profile_Priviledge_Activity")]
    public partial class tbl_Profile_Priviledge_Activity
    {
        [Key]
        public int UserActivityPriviledgeId { get; set; }

        public int UserId { get; set; }

        public int ActivityId { get; set; }

        public short PriviledgeId { get; set; }

        public int CreatedBy { get; set; }

        public int? LastUpdatedBy { get; set; }

        public DateTime DateTimeCreated { get; set; }

        public DateTime? DateTimeUpdated { get; set; }

        public virtual tbl_Profile_Activity tbl_Profile_Activity { get; set; }

        public virtual tbl_Profile_Priviledge tbl_Profile_Priviledge { get; set; }

        public virtual tbl_Profile_User tbl_Profile_User { get; set; }
    }
}
