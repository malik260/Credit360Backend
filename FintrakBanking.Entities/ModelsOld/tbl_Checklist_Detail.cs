namespace FintrakBanking.Entities.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("core.tbl_Checklist_Detail")]
    public partial class tbl_Checklist_Detail
    {
        [Key]
        public long ChecklistId { get; set; }

        public int CheckListDefinitionId { get; set; }

        [StringLength(500)]
        public string Remark { get; set; }

        public int CheckedBy { get; set; }

        public short TargetTypeId { get; set; }

        public int TargetId { get; set; }

        public short CheckListStatusId { get; set; }

        public DateTime? DeferedDate { get; set; }

        public int? CreatedBy { get; set; }

        public int? LastUpdatedBy { get; set; }

        public DateTime DateTimeCreated { get; set; }

        public DateTime? DateTimeUpdated { get; set; }

        public bool Deleted { get; set; }

        public int? DeletedBy { get; set; }

        public DateTime? DateTimeDeleted { get; set; }

        public virtual tbl_Checklist_Definition tbl_Checklist_Definition { get; set; }

        public virtual tbl_Checklist_Status tbl_Checklist_Status { get; set; }

        public virtual tbl_Checklist_TargetType tbl_Checklist_TargetType { get; set; }
    }
}
