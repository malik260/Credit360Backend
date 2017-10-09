namespace FintrakBanking.Entities.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("credit.tbl_Loan_Camsol")]
    public partial class tbl_Loan_Camsol
    {
        [Key]
        public int Loan_CamsolId { get; set; }

        public int CompanyId { get; set; }

        public int LoanId { get; set; }

        [Column(TypeName = "money")]
        public decimal AmountAffected { get; set; }

        public DateTime Date { get; set; }

        [Required]
        [StringLength(50)]
        public string Type { get; set; }

        public virtual tbl_Company tbl_Company { get; set; }
    }
}
