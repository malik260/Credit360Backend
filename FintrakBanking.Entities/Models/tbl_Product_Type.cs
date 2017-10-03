namespace FintrakBanking.Entities.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("core.tbl_Product_Type")]
    public partial class tbl_Product_Type
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public tbl_Product_Type()
        {
            tbl_Charge_Fee = new HashSet<tbl_Charge_Fee>();
            tbl_Fee = new HashSet<tbl_Fee>();
            tbl_Product = new HashSet<tbl_Product>();
            tbl_Temp_Product = new HashSet<tbl_Temp_Product>();
            tbl_Loan_Collateral_Mapping = new HashSet<tbl_Loan_Collateral_Mapping>();
            tbl_Loan_Covenant_Detail = new HashSet<tbl_Loan_Covenant_Detail>();
            tbl_Loan_Fee = new HashSet<tbl_Loan_Fee>();
            tbl_Loan_Force_Debit = new HashSet<tbl_Loan_Force_Debit>();
            tbl_Loan_Guarantor = new HashSet<tbl_Loan_Guarantor>();
            tbl_Loan_Past_Due = new HashSet<tbl_Loan_Past_Due>();
            tbl_Loan_Schedule_Type_Product_Type_Mapping = new HashSet<tbl_Loan_Schedule_Type_Product_Type_Mapping>();
            tbl_Temp_Charge_Fee = new HashSet<tbl_Temp_Charge_Fee>();
            tbl_Temp_Fee = new HashSet<tbl_Temp_Fee>();
        }

        [Key]
        public short ProductTypeId { get; set; }

        [Required]
        [StringLength(50)]
        public string ProductTypeName { get; set; }

        public short ProductGroupId { get; set; }

        public bool RequirePrincipalGL { get; set; }

        public bool RequireInterestIncomeExpenseGL { get; set; }

        public bool RequireInterestReceivablePayableGL { get; set; }

        public bool RequireDormantGL { get; set; }

        public bool RequirePremiumDiscountGL { get; set; }

        public bool RequireOverdrawnGL { get; set; }

        public short DealClassificationId { get; set; }

        public bool RequireRate { get; set; }

        public bool RequireTenor { get; set; }

        public bool RequireScheduleType { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<tbl_Charge_Fee> tbl_Charge_Fee { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<tbl_Fee> tbl_Fee { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<tbl_Product> tbl_Product { get; set; }

        public virtual tbl_Product_Group tbl_Product_Group { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<tbl_Temp_Product> tbl_Temp_Product { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<tbl_Loan_Collateral_Mapping> tbl_Loan_Collateral_Mapping { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<tbl_Loan_Covenant_Detail> tbl_Loan_Covenant_Detail { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<tbl_Loan_Fee> tbl_Loan_Fee { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<tbl_Loan_Force_Debit> tbl_Loan_Force_Debit { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<tbl_Loan_Guarantor> tbl_Loan_Guarantor { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<tbl_Loan_Past_Due> tbl_Loan_Past_Due { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<tbl_Loan_Schedule_Type_Product_Type_Mapping> tbl_Loan_Schedule_Type_Product_Type_Mapping { get; set; }

        public virtual tbl_Deal_Classification tbl_Deal_Classification { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<tbl_Temp_Charge_Fee> tbl_Temp_Charge_Fee { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<tbl_Temp_Fee> tbl_Temp_Fee { get; set; }
    }
}
