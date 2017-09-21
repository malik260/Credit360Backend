namespace FintrakBanking.Entities.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("credit.tbl_Loan_Fee_Schedule")]
    public partial class tbl_Loan_Fee_Schedule
    {
        [Key]
        public int PeriodicLoanChargeFeeId { get; set; }

        public int LoanChargeFeeId { get; set; }

        public int FeeNumber { get; set; }

        [Column(TypeName = "date")]
        public DateTime FeeDate { get; set; }

        [Column(TypeName = "money")]
        public decimal FeeAmount { get; set; }

        public virtual tbl_Loan_Fee tbl_Loan_Fee { get; set; }
    }
}
