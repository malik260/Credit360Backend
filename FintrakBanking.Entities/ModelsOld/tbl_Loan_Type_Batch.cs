namespace FintrakBanking.Entities.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("credit.tbl_Loan_Type_Batch")]
    public partial class tbl_Loan_Type_Batch
    {
        [Key]
        public int LoanTypeBatchId { get; set; }

        [Required]
        [StringLength(50)]
        public string LoanTypeBatchCode { get; set; }

        public int? CustomerId { get; set; }

        public int? CustomerGroupId { get; set; }

        public short LoanTypeId { get; set; }

        [Column(TypeName = "money")]
        public decimal? GroupAmount { get; set; }

        public bool IsClosed { get; set; }

        [Column(TypeName = "date")]
        public DateTime DateCreated { get; set; }

        public virtual tbl_Customer tbl_Customer { get; set; }

        public virtual tbl_Customer_Group tbl_Customer_Group { get; set; }

        public virtual tbl_Loan_Type tbl_Loan_Type { get; set; }
    }
}
