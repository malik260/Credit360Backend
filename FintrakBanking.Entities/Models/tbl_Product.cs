namespace FintrakBanking.Entities.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("core.tbl_Product")]
    public partial class tbl_Product
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public tbl_Product()
        {
            tbl_CASA = new HashSet<tbl_CASA>();
            tbl_KYC_Item = new HashSet<tbl_KYC_Item>();
            tbl_Loan_Guarantor = new HashSet<tbl_Loan_Guarantor>();
            tbl_Loan = new HashSet<tbl_Loan>();
            tbl_Product_CollateralType = new HashSet<tbl_Product_CollateralType>();
            tbl_Product_Currency = new HashSet<tbl_Product_Currency>();
            tbl_Temp_Product_Currency = new HashSet<tbl_Temp_Product_Currency>();
            tbl_Product_Fee = new HashSet<tbl_Product_Fee>();
            tbl_Temp_Product_Fee = new HashSet<tbl_Temp_Product_Fee>();
            tbl_Risk_Rating = new HashSet<tbl_Risk_Rating>();
        }

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

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<tbl_CASA> tbl_CASA { get; set; }

        public virtual tbl_Company tbl_Company { get; set; }

        public virtual tbl_Day_Count tbl_Day_Count { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<tbl_KYC_Item> tbl_KYC_Item { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<tbl_Loan_Guarantor> tbl_Loan_Guarantor { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<tbl_Loan> tbl_Loan { get; set; }

        public virtual tbl_Deal_Classification tbl_Deal_Classification { get; set; }

        public virtual tbl_Product_Type tbl_Product_Type { get; set; }

        public virtual tbl_Product_Category tbl_Product_Category { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<tbl_Product_CollateralType> tbl_Product_CollateralType { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<tbl_Product_Currency> tbl_Product_Currency { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<tbl_Temp_Product_Currency> tbl_Temp_Product_Currency { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<tbl_Product_Fee> tbl_Product_Fee { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<tbl_Temp_Product_Fee> tbl_Temp_Product_Fee { get; set; }

        public virtual tbl_Chart_Of_Account tbl_Chart_Of_Account { get; set; }

        public virtual tbl_Chart_Of_Account tbl_Chart_Of_Account1 { get; set; }

        public virtual tbl_Chart_Of_Account tbl_Chart_Of_Account2 { get; set; }

        public virtual tbl_Chart_Of_Account tbl_Chart_Of_Account3 { get; set; }

        public virtual tbl_Chart_Of_Account tbl_Chart_Of_Account4 { get; set; }

        public virtual tbl_Chart_Of_Account tbl_Chart_Of_Account5 { get; set; }

        public virtual tbl_Deal_Type tbl_Deal_Type { get; set; }

        public virtual tbl_Loan_Schedule_Type tbl_Loan_Schedule_Type { get; set; }

        public virtual tbl_Product_Class tbl_Product_Class { get; set; }

        public virtual tbl_Product_Price_Index tbl_Product_Price_Index { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<tbl_Risk_Rating> tbl_Risk_Rating { get; set; }
    }
}
