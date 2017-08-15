namespace FintrakBanking.Entities.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("core.tbl_Profile_Group_Activity")]
    public partial class tbl_Profile_Group_Activity
    {
        [Key]
        public int GroupActivityId { get; set; }

        public short GroupId { get; set; }

        public int ActivityId { get; set; }

        public bool? CanEdit { get; set; }

        public bool? CanAdd { get; set; }

        public bool? CanView { get; set; }

        public bool? CanDelete { get; set; }

        public bool? CanApprove { get; set; }

        public int CreatedBy { get; set; }

        public int? LastUpdatedBy { get; set; }

        public DateTime DateTimeCreated { get; set; }

        public DateTime? DateTimeUpdated { get; set; }

        public virtual tbl_Profile_Activity tbl_Profile_Activity { get; set; }

        public virtual tbl_Profile_Group tbl_Profile_Group { get; set; }
    }
}
