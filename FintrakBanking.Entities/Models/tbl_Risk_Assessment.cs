namespace FintrakBanking.Entities.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("credit.tbl_Risk_Assessment")]
    public partial class tbl_Risk_Assessment
    {
        [Key]
        public int RiskAssessmentId { get; set; }

        public int RiskIndexId { get; set; }

        public int? ParentId { get; set; }

        [Required]
        [StringLength(50)]
        public string RefCode { get; set; }

        public int LoanApplicationId { get; set; }

        public int RiskAssessmentTitleId { get; set; }

        public decimal IndexScore { get; set; }

        public int CompanyId { get; set; }

        public DateTime? DateTimeDeleted { get; set; }

        public int CreatedBy { get; set; }

        public int? LastUpdatedBy { get; set; }

        public DateTime DateTimeCreated { get; set; }

        public DateTime? DateTimeUpdated { get; set; }

        public bool Deleted { get; set; }

        public int? DeletedBy { get; set; }

        public bool Selected { get; set; }

        public virtual tbl_Loan_Application tbl_Loan_Application { get; set; }

        public virtual tbl_Risk_Assessment_Title tbl_Risk_Assessment_Title { get; set; }
    }
}
