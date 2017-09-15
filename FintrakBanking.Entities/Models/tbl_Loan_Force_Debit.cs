namespace FintrakBanking.Entities.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("credit.tbl_Loan_Force_Debit")]
    public partial class tbl_Loan_Force_Debit
    {
        [Key]
        public int ForceDebitId { get; set; }

        public int LoanId { get; set; }

        [Column(TypeName = "date")]
        public DateTime Date { get; set; }

        public byte TransactionTypeId { get; set; }

        [Required]
        [StringLength(50)]
        public string ForceDebitCode { get; set; }

        [StringLength(50)]
        public string Parent_ForceDebitCode { get; set; }

        [Required]
        [StringLength(800)]
        public string Description { get; set; }

        [Column(TypeName = "money")]
        public decimal DebitAmount { get; set; }

        [Column(TypeName = "money")]
        public decimal CreditAmount { get; set; }

        public virtual tbl_Loan tbl_Loan { get; set; }

        public virtual tbl_Loan_Transaction_Type tbl_Loan_Transaction_Type { get; set; }
    }
}
