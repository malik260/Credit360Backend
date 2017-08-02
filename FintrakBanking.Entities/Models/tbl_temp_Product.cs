namespace FintrakBanking.Entities.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("temp.tbl_temp_Product")]
    public partial class tbl_temp_Product
    {
        [Key]
        public short ProductId { get; set; }

        public int CompanyId { get; set; }

        public short ProductTypeId { get; set; }

        public short ProductCategoryId { get; set; }

        public bool IsMultipleCurency { get; set; }

        public short ProductClassId { get; set; }

        [Required]
        [StringLength(50)]
        public string ProductCode { get; set; }

        [Required]
        [StringLength(50)]
        public string ProductName { get; set; }

        [StringLength(100)]
        public string ProductDescription { get; set; }

        public int? PrincipalBalanceGL { get; set; }

        public int? InterestIncomeExpenseGL { get; set; }

        public int? InterestReceivablePayableGL { get; set; }

        public int? DormantGL { get; set; }

        public int? PremiumDiscountGL { get; set; }

        public short? DealTypeId { get; set; }

        public short? DealClassificationId { get; set; }

        public short? DayCountId { get; set; }

        public short? ScheduleTypeId { get; set; }

        public bool AllowScheduleTypeOverride { get; set; }

        public int MaximumTenor { get; set; }

        public int MinimumTenor { get; set; }

        public decimal? MaximumRate { get; set; }

        public decimal? MinimumRate { get; set; }

        public decimal? MinimumBalance { get; set; }

        public short? ProductPriceIndexId { get; set; }

        public double? ProductPriceIndexSpread { get; set; }

        public bool? AllowOverdrawn { get; set; }

        public int? OverdrawnGL { get; set; }

        public bool? AllowRate { get; set; }

        public bool? AllowTenor { get; set; }

        public int? ApprovedBy { get; set; }

        public bool? Completed { get; set; }

        public bool? Approved { get; set; }

        public int? CreatedBy { get; set; }

        public int? LastUpdatedBy { get; set; }

        public DateTime? DateTimeCreated { get; set; }

        public DateTime? DateTimeUpdated { get; set; }

        public bool Deleted { get; set; }

        public int? DeletedBy { get; set; }

        public DateTime? DateTimeDeleted { get; set; }

        public bool IsCurrent { get; set; }

        public short ApprovalStatusId { get; set; }

        public virtual tbl_Company tbl_Company { get; set; }

        public virtual tbl_Day_Count tbl_Day_Count { get; set; }

        public virtual tbl_Product_Category tbl_Product_Category { get; set; }

        public virtual tbl_Product_Class tbl_Product_Class { get; set; }

        public virtual tbl_Product_Price_Index tbl_Product_Price_Index { get; set; }

        public virtual tbl_Product_Type tbl_Product_Type { get; set; }

        public virtual tbl_Loan_Schedule_Type tbl_Loan_Schedule_Type { get; set; }

        public virtual tbl_Chart_Of_Account tbl_Chart_Of_Account { get; set; }

        public virtual tbl_Chart_Of_Account tbl_Chart_Of_Account1 { get; set; }

        public virtual tbl_Chart_Of_Account tbl_Chart_Of_Account2 { get; set; }

        public virtual tbl_Chart_Of_Account tbl_Chart_Of_Account3 { get; set; }

        public virtual tbl_Chart_Of_Account tbl_Chart_Of_Account4 { get; set; }

        public virtual tbl_Chart_Of_Account tbl_Chart_Of_Account5 { get; set; }

        public virtual tbl_Deal_Classification tbl_Deal_Classification { get; set; }

        public virtual tbl_Deal_Type tbl_Deal_Type { get; set; }
    }
}
