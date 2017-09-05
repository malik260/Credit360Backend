namespace FintrakBanking.Entities.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("credit.tbl_Credit_Appraisal_Memorandum_Loan_Detail")]
    public partial class tbl_Credit_Appraisal_Memorandum_Loan_Detail
    {
        [Key]
        public int AppraisalMemorandumLoanDetailId { get; set; }

        public int AppraisalMemorandumId { get; set; }

        [Column(TypeName = "money")]
        public decimal PrincipalAmount { get; set; }

        public double InterestRate { get; set; }

        public int Tenor { get; set; }

        public int CreatedBy { get; set; }

        public DateTime DateTimeCreated { get; set; }

        public DateTime SystemDateTime { get; set; }

        public virtual tbl_Credit_Appraisal_Memorandum tbl_Credit_Appraisal_Memorandum { get; set; }
    }
}
