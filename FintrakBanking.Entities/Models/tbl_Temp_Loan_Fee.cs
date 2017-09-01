namespace FintrakBanking.Entities.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("temp.tbl_Temp_Loan_Fee")]
    public partial class tbl_Temp_Loan_Fee
    {
        [Key]
        public short LoanChargeFeeId { get; set; }

        public int LoanApplicationId { get; set; }

        public int CustomerId { get; set; }

        public int ChargeFeeId { get; set; }

        [Required]
        [StringLength(10)]
        public string FeeRateValue { get; set; }

        [Required]
        [StringLength(10)]
        public string FeeDependentAmount { get; set; }

        [Required]
        [StringLength(10)]
        public string FeeAmount { get; set; }

        [StringLength(10)]
        public string IsIntegralFee { get; set; }

        public int CreatedBy { get; set; }

        public DateTime DateTimeCreated { get; set; }

        public virtual tbl_Charge_Fee tbl_Charge_Fee { get; set; }

        public virtual tbl_Customer tbl_Customer { get; set; }

        public virtual tbl_Loan_Application tbl_Loan_Application { get; set; }
    }
}
