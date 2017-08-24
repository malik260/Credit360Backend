namespace FintrakBanking.Entities.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("credit.tbl_Loan_Fee")]
    public partial class tbl_Loan_Fee
    {
        [Key]
        public int LoanChargeFeeId { get; set; }

        public int LoanId { get; set; }

        public int ChargeFeeId { get; set; }

        [Column(TypeName = "money")]
        public decimal FeeRateValue { get; set; }

        [Column(TypeName = "money")]
        public decimal FeeDependentAmount { get; set; }

        [Column(TypeName = "money")]
        public decimal FeeAmount { get; set; }

        public bool IsIntegralFee { get; set; }

        public virtual tbl_Charge_Fee tbl_Charge_Fee { get; set; }

        public virtual tbl_Loan tbl_Loan { get; set; }
    }
}
