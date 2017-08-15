namespace FintrakBanking.Entities.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("core.tbl_Approval_Level_Staff")]
    public partial class tbl_Approval_Level_Staff
    {
        [Key]
        public int StaffLevelId { get; set; }

        public int StaffId { get; set; }

        public int ApprovalLevelId { get; set; }

        [Column(TypeName = "money")]
        public decimal MaximumAmount { get; set; }

        public int CreatedBy { get; set; }

        public DateTime DateTimeCreated { get; set; }

        public int? LastUpdatedBy { get; set; }

        public DateTime? DateTimeUpdated { get; set; }

        public bool Deleted { get; set; }

        public int? DeletedBy { get; set; }

        public DateTime? DateTimeDeleted { get; set; }

        public virtual tbl_Approval_Level tbl_Approval_Level { get; set; }

        public virtual tbl_Staff tbl_Staff { get; set; }
    }
}
