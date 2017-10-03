namespace FintrakBanking.Entities.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("credit.tbl_Loan_Past_Due")]
    public partial class tbl_Loan_Past_Due
    {
        [Key]
        public int PastDueId { get; set; }

        public int LoanId { get; set; }

        public short ProductTypeId { get; set; }

        [Column(TypeName = "date")]
        public DateTime Date { get; set; }

        public byte TransactionTypeId { get; set; }

        [Required]
        [StringLength(50)]
        public string PastDueCode { get; set; }

        [StringLength(50)]
        public string Parent_PastDueCode { get; set; }

        [Required]
        [StringLength(800)]
        public string Description { get; set; }

        [Column(TypeName = "money")]
        public decimal DebitAmount { get; set; }

        [Column(TypeName = "money")]
        public decimal CreditAmount { get; set; }

        public virtual tbl_Product_Type tbl_Product_Type { get; set; }

        public virtual tbl_Loan tbl_Loan { get; set; }

        public virtual tbl_Loan_Transaction_Type tbl_Loan_Transaction_Type { get; set; }
    }
}
