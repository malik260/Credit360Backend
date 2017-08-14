namespace FintrakBanking.Entities.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("temp.tbl_Temp_Customer_Group")]
    public partial class tbl_Temp_Customer_Group
    {
        [Key]
        public int CustomerGroupId { get; set; }

        [Required]
        [StringLength(50)]
        public string GroupName { get; set; }

        [Required]
        [StringLength(10)]
        public string GroupCode { get; set; }

        [StringLength(1000)]
        public string GroupDescription { get; set; }

        public int CreatedBy { get; set; }

        public int? LastUpdatedBy { get; set; }

        public DateTime DateTimeCreated { get; set; }

        public DateTime? DateTimeUpdated { get; set; }

        public bool Deleted { get; set; }

        public int? DeletedBy { get; set; }

        public DateTime? DateTimeDeleted { get; set; }

        public bool IsCurrent { get; set; }

        public short ApprovalStatusId { get; set; }

        public int CompanyId { get; set; }

        public virtual tbl_Approval_Status tbl_Approval_Status { get; set; }

        public virtual tbl_Company tbl_Company { get; set; }
    }
}
