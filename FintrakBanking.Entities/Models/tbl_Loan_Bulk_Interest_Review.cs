namespace FintrakBanking.Entities.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("credit.tbl_Loan_Bulk_Interest_Review")]
    public partial class tbl_Loan_Bulk_Interest_Review
    {
        [Key]
        public int BulkInterestRateReviewId { get; set; }

        public int CompanyId { get; set; }

        [Column(TypeName = "date")]
        public DateTime EffectiveDate { get; set; }

        public short ProductPriceIndexId { get; set; }

        public double OldInterestRate { get; set; }

        public double NewInterestRate { get; set; }

        public bool IsProcessed { get; set; }

        public DateTime? ProcessStartTime { get; set; }

        public DateTime? ProcessEndTime { get; set; }

        public int CreatedBy { get; set; }

        [Column(TypeName = "date")]
        public DateTime DateCreated { get; set; }

        public virtual tbl_Company tbl_Company { get; set; }

        public virtual tbl_Product_Price_Index tbl_Product_Price_Index { get; set; }
    }
}
