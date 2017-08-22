namespace FintrakBanking.Entities.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("core.tbl_Charge_Fee")]
    public partial class tbl_Charge_Fee
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public tbl_Charge_Fee()
        {
            tbl_Charge_Range = new HashSet<tbl_Charge_Range>();
            tbl_Product_Charge_Fee = new HashSet<tbl_Product_Charge_Fee>();
        }

        [Key]
        public int ChargeFeeId { get; set; }

        [Required]
        [StringLength(150)]
        public string ChargeFeeName { get; set; }

        public short AccountCategoryId { get; set; }

        public short FeeIntervalId { get; set; }

        public short ProductTypeId { get; set; }

        public short FeeTargetId { get; set; }

        public int GLAccountId { get; set; }

        public short? FeeAmortisationTypeId { get; set; }

        public bool IsIntegralFee { get; set; }

        public bool IncludeCutOffDay { get; set; }

        public short? CutOffDay { get; set; }

        public int? OperationId { get; set; }

        [Column(TypeName = "money")]
        public decimal? Amount { get; set; }

        public double? Rate { get; set; }

        public int ValueSource { get; set; }

        public bool? Recurring { get; set; }

        public int? PrimaryTaxId { get; set; }

        public int? SecondaryTaxId { get; set; }

        public int CompanyId { get; set; }

        public int CreatedBy { get; set; }

        public int? LastUpdatedBy { get; set; }

        public DateTime DateTimeCreated { get; set; }

        public DateTime? DateTimeUpdated { get; set; }

        public bool Deleted { get; set; }

        public int? DeletedBy { get; set; }

        public DateTime? DateTimeDeleted { get; set; }

        public virtual tbl_Account_Category tbl_Account_Category { get; set; }

        public virtual tbl_Chart_Of_Account tbl_Chart_Of_Account { get; set; }

        public virtual tbl_Company tbl_Company { get; set; }

        public virtual tbl_Fee_Amortisation_Type tbl_Fee_Amortisation_Type { get; set; }

        public virtual tbl_Fee_Interval tbl_Fee_Interval { get; set; }

        public virtual tbl_Fee_Target tbl_Fee_Target { get; set; }

        public virtual tbl_Operations tbl_Operations { get; set; }

        public virtual tbl_Product_Type tbl_Product_Type { get; set; }

        public virtual tbl_Tax tbl_Tax { get; set; }

        public virtual tbl_Tax tbl_Tax1 { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<tbl_Charge_Range> tbl_Charge_Range { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<tbl_Product_Charge_Fee> tbl_Product_Charge_Fee { get; set; }
    }
}
