namespace FintrakBanking.Entities.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("core.tbl_Approval_Trail")]
    public partial class tbl_Approval_Trail
    {
        [Key]
        public int ApprovalTrailId { get; set; }

        public int TargetId { get; set; }

        public DateTime ArrivalDateTime { get; set; }

        public DateTime SystemArrivalDateTime { get; set; }

        public DateTime? ResponseDateTime { get; set; }

        public DateTime? SystemResponseDateTime { get; set; }

        public int? ResponseByStaffId { get; set; }

        public int CompanyId { get; set; }

        public int StaffId { get; set; }

        public int FromApprovalLevelId { get; set; }

        public int? ToApprovalLevelId { get; set; }

        public short ApprovalStateId { get; set; }

        public short ApprovalStatusId { get; set; }

        public int OperationId { get; set; }

        [StringLength(500)]
        public string Comment { get; set; }

        public virtual tbl_Approval_Level tbl_Approval_Level { get; set; }

        public virtual tbl_Approval_Level tbl_Approval_Level1 { get; set; }

        public virtual tbl_Approval_State tbl_Approval_State { get; set; }

        public virtual tbl_Approval_Status tbl_Approval_Status { get; set; }

        public virtual tbl_Company tbl_Company { get; set; }

        public virtual tbl_Operations tbl_Operations { get; set; }

        public virtual tbl_Staff tbl_Staff { get; set; }

        public virtual tbl_Staff tbl_Staff1 { get; set; }
    }
}
