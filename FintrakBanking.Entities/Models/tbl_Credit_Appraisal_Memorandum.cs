namespace FintrakBanking.Entities.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("credit.tbl_Credit_Appraisal_Memorandum")]
    public partial class tbl_Credit_Appraisal_Memorandum
    {
        [Key]
        public int AppraisalMemorandumId { get; set; }

        public int LoanApplicationId { get; set; }

        //[Required]
        [StringLength(50)]
        public string CAMRef { get; set; }

        public bool IsCompleted { get; set; }

        public bool RiskRated { get; set; }

        //[Required]
        public string CAMDocumentation { get; set; }

        //[Column(TypeName = "xml")]
        //[Required]
        public string LoanDetails { get; set; }

        public int CreatedBy { get; set; }

        public DateTime DateTimeCreated { get; set; }

        public int? LastUpdatedBy { get; set; }

        public DateTime? DateTimeUpdated { get; set; }

        public bool Deleted { get; set; }

        public int? DeletedBy { get; set; }

        public DateTime? DateTimeDeleted { get; set; }

        public virtual tbl_Loan_Application tbl_Loan_Application { get; set; }
    }
}
