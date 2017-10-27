namespace FintrakBanking.Entities.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("core.tbl_Checklist_Definition")]
    public partial class tbl_Checklist_Definition
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public tbl_Checklist_Definition()
        {
            tbl_Checklist_Detail = new HashSet<tbl_Checklist_Detail>();
        }

        [Key]
        public int CheckListDefinitionId { get; set; }

        public short? ProductId { get; set; }

        public int? ApprovalLevelId { get; set; }

        public int CheckListItemId { get; set; }

        [StringLength(2000)]
        public string ItemDescription { get; set; }

        public bool IsRequired { get; set; }

        public int CompanyId { get; set; }

        public bool IsActive { get; set; }

        public int CreatedBy { get; set; }

        public int? LastUpdatedBy { get; set; }

        public DateTime DateTimeCreated { get; set; }

        public DateTime? DateTimeUpdated { get; set; }

        public bool Deleted { get; set; }

        public int? DeletedBy { get; set; }

        public DateTime? DateTimeDeleted { get; set; }

        public virtual tbl_Approval_Level tbl_Approval_Level { get; set; }

        public virtual tbl_Company tbl_Company { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<tbl_Checklist_Detail> tbl_Checklist_Detail { get; set; }

        public virtual tbl_Product tbl_Product { get; set; }

        public virtual tbl_CheckList_Item tbl_CheckList_Item { get; set; }
    }
}
