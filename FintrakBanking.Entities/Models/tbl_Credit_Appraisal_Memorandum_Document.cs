namespace FintrakBanking.Entities.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("credit.tbl_Credit_Appraisal_Memorandum_Document")]
    public partial class tbl_Credit_Appraisal_Memorandum_Document
    {
        [Key]
        public int CAMDocumentationId { get; set; }

        [Required]
        public string CAMDocumentation { get; set; }

        public int AppraisalMemorandumId { get; set; }

        public int ApprovalLevelId { get; set; }

        public int CreatedBy { get; set; }

        public DateTime DateTimeCreated { get; set; }

        public int? LastUpdatedBy { get; set; }

        public DateTime? DateTimeUpdated { get; set; }

        public virtual tbl_Approval_Level tbl_Approval_Level { get; set; }

        public virtual tbl_Credit_Appraisal_Memorandum tbl_Credit_Appraisal_Memorandum { get; set; }
    }
}
