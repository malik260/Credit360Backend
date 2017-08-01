namespace FintrakBanking.Entities.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("temp.tbl_Temp_Chart_Of_Account")]
    public partial class tbl_Temp_Chart_Of_Account
    {
        [Key]
        public int GLAccountId { get; set; }

        public int AccountTypeId { get; set; }

        [Required]
        [StringLength(20)]
        public string AccountCode { get; set; }

        [Required]
        [StringLength(500)]
        public string AccountName { get; set; }

        public int CompanyId { get; set; }

        public short BranchId { get; set; }

        public bool SystemUse { get; set; }

        public int? AccountStatusId { get; set; }

        public bool BranchSpecific { get; set; }

        [StringLength(20)]
        public string OldAccountId { get; set; }

        public short FSCaptionId { get; set; }

        public bool Deleted { get; set; }

        public int? DeletedBy { get; set; }

        public DateTime? DateTimeDeleted { get; set; }

        public int CreatedBy { get; set; }

        public int? LastUpdatedBy { get; set; }

        public DateTime DateTimeCreated { get; set; }

        public DateTime? DateTimeUpdated { get; set; }

        public bool IsCurrent { get; set; }

        public short ApprovalStatusId { get; set; }

        public virtual tbl_Branch tbl_Branch { get; set; }

        public virtual tbl_Company tbl_Company { get; set; }

        public virtual tbl_Account_Type tbl_Account_Type { get; set; }

        public virtual tbl_Financial_Statement_Caption tbl_Financial_Statement_Caption { get; set; }
    }
}
