namespace FintrakBanking.Entities.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("core.tbl_Daily_Accrual")]
    public partial class tbl_Daily_Accrual
    {
        [Key]
        public int DailyAccuralId { get; set; }

        [Required]
        [StringLength(50)]
        public string ReferenceNumber { get; set; }

        [StringLength(50)]
        public string BaseReferenceNumber { get; set; }

        public short CategoryId { get; set; }

        public byte TransactionTypeId { get; set; }

        public short ProductId { get; set; }

        public int CompanyId { get; set; }

        public short BranchId { get; set; }

        public short CurrencyId { get; set; }

        public double ExchangeRate { get; set; }

        [Column(TypeName = "money")]
        public decimal MainAmount { get; set; }

        public double InterestRate { get; set; }

        [Column(TypeName = "date")]
        public DateTime Date { get; set; }

        public short DayCountConventionId { get; set; }

        [Column(TypeName = "money")]
        public decimal DailyAccuralAmount { get; set; }

        [Column(TypeName = "money")]
        public decimal SystemDateTime { get; set; }

        public virtual tbl_Branch tbl_Branch { get; set; }

        public virtual tbl_Company tbl_Company { get; set; }

        public virtual tbl_Currency tbl_Currency { get; set; }

        public virtual tbl_Daily_Accrual_Category tbl_Daily_Accrual_Category { get; set; }

        public virtual tbl_Day_Count_Convention tbl_Day_Count_Convention { get; set; }

        public virtual tbl_Loan_Transaction_Type tbl_Loan_Transaction_Type { get; set; }

        public virtual tbl_Product tbl_Product { get; set; }
    }
}
