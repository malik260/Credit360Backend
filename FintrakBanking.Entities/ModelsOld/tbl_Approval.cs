namespace FintrakBanking.Entities.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    public partial class tbl_Approval
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public int ApprovalId { get; set; }

        public int StaffId { get; set; }

        public int OperationId { get; set; }

        public int CompanyId { get; set; }

        public int TargetId { get; set; }

        public int? ApprovalStatusId { get; set; }

        [Column(TypeName = "money")]
        public decimal? Amount { get; set; }
    }
}
