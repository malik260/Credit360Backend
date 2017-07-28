namespace FintrakBanking.Entities.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("credit.tbl_Credit_Template")]
    public partial class tbl_Credit_Template
    {
        [Key]
        public int CreditTemplateId { get; set; }

        [Column(TypeName = "ntext")]
        [Required]
        public string CreditTemplate { get; set; }

        public int ApprovalLevelId { get; set; }

        [Required]
        [StringLength(250)]
        public string TemplateTitle { get; set; }

        public short ProductClassId { get; set; }

        public int? LastUpdatedBy { get; set; }

        public int CompanyId { get; set; }

        public DateTime DateTimeCreated { get; set; }

        public DateTime? DateTimeUpdated { get; set; }

        public bool Deleted { get; set; }

        public int? DeletedBy { get; set; }

        public DateTime? DateTimeDeleted { get; set; }

        public int CreatedBy { get; set; }

        public virtual tbl_Approval_Level tbl_Approval_Level { get; set; }

        public virtual tbl_Product_Class tbl_Product_Class { get; set; }
    }
}
