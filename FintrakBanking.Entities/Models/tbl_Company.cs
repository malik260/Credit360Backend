namespace FintrakBanking.Entities.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("core.tbl_Company")]
    public partial class tbl_Company
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public tbl_Company()
        {
            tbl_Approval_Group = new HashSet<tbl_Approval_Group>();
            tbl_Approval_Trail = new HashSet<tbl_Approval_Trail>();
            tbl_Branch = new HashSet<tbl_Branch>();
            tbl_CASA = new HashSet<tbl_CASA>();
            tbl_Charge_Fee = new HashSet<tbl_Charge_Fee>();
            tbl_Checklist_Definition = new HashSet<tbl_Checklist_Definition>();
            tbl_MIS_Info = new HashSet<tbl_MIS_Info>();
            tbl_Product = new HashSet<tbl_Product>();
            tbl_Temp_Product = new HashSet<tbl_Temp_Product>();
            tbl_CASA_Lien = new HashSet<tbl_CASA_Lien>();
            tbl_Chart_Of_Account = new HashSet<tbl_Chart_Of_Account>();
            tbl_Temp_Chart_Of_Account = new HashSet<tbl_Temp_Chart_Of_Account>();
            tbl_Collateral_Customer = new HashSet<tbl_Collateral_Customer>();
            tbl_Temp_Collateral_Customer = new HashSet<tbl_Temp_Collateral_Customer>();
            tbl_Company1 = new HashSet<tbl_Company>();
            tbl_Credit_Appraisal_Memorandum = new HashSet<tbl_Credit_Appraisal_Memorandum>();
            tbl_Credit_Template = new HashSet<tbl_Credit_Template>();
            tbl_Customer_Blacklist = new HashSet<tbl_Customer_Blacklist>();
            tbl_Customer_FS_Caption_Group = new HashSet<tbl_Customer_FS_Caption_Group>();
            tbl_Customer_FS_Ratio_Caption = new HashSet<tbl_Customer_FS_Ratio_Caption>();
            tbl_Customer = new HashSet<tbl_Customer>();
            tbl_Daily_Accural = new HashSet<tbl_Daily_Accural>();
            tbl_Finance_Transaction = new HashSet<tbl_Finance_Transaction>();
            tbl_Limit = new HashSet<tbl_Limit>();
            tbl_Loan_Application = new HashSet<tbl_Loan_Application>();
            tbl_Loan_Archive = new HashSet<tbl_Loan_Archive>();
            tbl_Loan_Camsol = new HashSet<tbl_Loan_Camsol>();
            tbl_Loan_Contingent = new HashSet<tbl_Loan_Contingent>();
            tbl_Loan_Preliminary_Evaluation = new HashSet<tbl_Loan_Preliminary_Evaluation>();
            tbl_Loan_Revolving = new HashSet<tbl_Loan_Revolving>();
            tbl_Loan = new HashSet<tbl_Loan>();
            tbl_Product_CollateralType = new HashSet<tbl_Product_CollateralType>();
            tbl_Temp_Product_CollateralType = new HashSet<tbl_Temp_Product_CollateralType>();
            tbl_Fee = new HashSet<tbl_Fee>();
            tbl_Product_Charge_Fee = new HashSet<tbl_Product_Charge_Fee>();
            tbl_Product_Price_Index = new HashSet<tbl_Product_Price_Index>();
            tbl_Solicitor = new HashSet<tbl_Solicitor>();
            tbl_Staff_Organogram = new HashSet<tbl_Staff_Organogram>();
            tbl_Staff = new HashSet<tbl_Staff>();
            tbl_Stock = new HashSet<tbl_Stock>();
            tbl_Tax = new HashSet<tbl_Tax>();
            tbl_Temp_Charge_Fee = new HashSet<tbl_Temp_Charge_Fee>();
            tbl_Temp_Customer_Group_Mapping = new HashSet<tbl_Temp_Customer_Group_Mapping>();
            tbl_Temp_Customer_Group = new HashSet<tbl_Temp_Customer_Group>();
            tbl_Temp_Product_CollateralType1 = new HashSet<tbl_Temp_Product_CollateralType>();
            tbl_Temp_Fee = new HashSet<tbl_Temp_Fee>();
            tbl_Temp_Product_Fee = new HashSet<tbl_Temp_Product_Fee>();
            tbl_Temp_Staff = new HashSet<tbl_Temp_Staff>();
        }

        [Key]
        public int CompanyId { get; set; }

        [Required]
        [StringLength(250)]
        public string Name { get; set; }

        [StringLength(250)]
        public string Address { get; set; }

        [StringLength(100)]
        public string Telephone { get; set; }

        [StringLength(100)]
        public string Email { get; set; }

        [Column(TypeName = "date")]
        public DateTime? DateOfIncorporation { get; set; }

        public int CountryId { get; set; }

        public short CurrencyId { get; set; }

        public short? NatureOfBusinessId { get; set; }

        [StringLength(50)]
        public string NameOfScheme { get; set; }

        [StringLength(50)]
        public string FunctionsRegistered { get; set; }

        [Column(TypeName = "money")]
        public decimal? AuthorisedShareCapital { get; set; }

        [StringLength(100)]
        public string NameOfRegistrar { get; set; }

        [StringLength(100)]
        public string NameOfTrustees { get; set; }

        [StringLength(50)]
        public string FormerManagersTrustees { get; set; }

        [Column(TypeName = "date")]
        public DateTime? DateOfRenewalOfRegistration { get; set; }

        [Column(TypeName = "date")]
        public DateTime? DateOfCommencement { get; set; }

        public int? InitialFloatation { get; set; }

        public int? InitialSubscription { get; set; }

        [StringLength(100)]
        public string RegisteredBy { get; set; }

        [StringLength(1000)]
        public string TrusteesAddress { get; set; }

        [StringLength(1000)]
        public string InvestmentObjective { get; set; }

        [StringLength(100)]
        public string Website { get; set; }

        [StringLength(10)]
        public string EBusinessCode { get; set; }

        [StringLength(50)]
        public string EOYProfitAndLossGL { get; set; }

        public short? CompanyClassId { get; set; }

        public short? CompanyTypeId { get; set; }

        public short? AccountingStandardId { get; set; }

        public short? ManagementTypeId { get; set; }

        public int? ParentId { get; set; }

        public int? CreatedBy { get; set; }

        public int? LastUpdatedBy { get; set; }

        public DateTime? DateTimeCreated { get; set; }

        public DateTime? DateTimeUpdated { get; set; }

        public byte[] CompanyLogo { get; set; }

        public virtual tbl_Accounting_Standard tbl_Accounting_Standard { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<tbl_Approval_Group> tbl_Approval_Group { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<tbl_Approval_Trail> tbl_Approval_Trail { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<tbl_Branch> tbl_Branch { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<tbl_CASA> tbl_CASA { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<tbl_Charge_Fee> tbl_Charge_Fee { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<tbl_Checklist_Definition> tbl_Checklist_Definition { get; set; }

        public virtual tbl_Company_Class tbl_Company_Class { get; set; }

        public virtual tbl_Company_Type tbl_Company_Type { get; set; }

        public virtual tbl_Country tbl_Country { get; set; }

        public virtual tbl_Management_Type tbl_Management_Type { get; set; }

        public virtual tbl_Nature_Of_Business tbl_Nature_Of_Business { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<tbl_MIS_Info> tbl_MIS_Info { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<tbl_Product> tbl_Product { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<tbl_Temp_Product> tbl_Temp_Product { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<tbl_CASA_Lien> tbl_CASA_Lien { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<tbl_Chart_Of_Account> tbl_Chart_Of_Account { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<tbl_Temp_Chart_Of_Account> tbl_Temp_Chart_Of_Account { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<tbl_Collateral_Customer> tbl_Collateral_Customer { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<tbl_Temp_Collateral_Customer> tbl_Temp_Collateral_Customer { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<tbl_Company> tbl_Company1 { get; set; }

        public virtual tbl_Company tbl_Company2 { get; set; }

        public virtual tbl_Currency tbl_Currency { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<tbl_Credit_Appraisal_Memorandum> tbl_Credit_Appraisal_Memorandum { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<tbl_Credit_Template> tbl_Credit_Template { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<tbl_Customer_Blacklist> tbl_Customer_Blacklist { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<tbl_Customer_FS_Caption_Group> tbl_Customer_FS_Caption_Group { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<tbl_Customer_FS_Ratio_Caption> tbl_Customer_FS_Ratio_Caption { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<tbl_Customer> tbl_Customer { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<tbl_Daily_Accural> tbl_Daily_Accural { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<tbl_Finance_Transaction> tbl_Finance_Transaction { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<tbl_Limit> tbl_Limit { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<tbl_Loan_Application> tbl_Loan_Application { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<tbl_Loan_Archive> tbl_Loan_Archive { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<tbl_Loan_Camsol> tbl_Loan_Camsol { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<tbl_Loan_Contingent> tbl_Loan_Contingent { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<tbl_Loan_Preliminary_Evaluation> tbl_Loan_Preliminary_Evaluation { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<tbl_Loan_Revolving> tbl_Loan_Revolving { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<tbl_Loan> tbl_Loan { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<tbl_Product_CollateralType> tbl_Product_CollateralType { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<tbl_Temp_Product_CollateralType> tbl_Temp_Product_CollateralType { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<tbl_Fee> tbl_Fee { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<tbl_Product_Charge_Fee> tbl_Product_Charge_Fee { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<tbl_Product_Price_Index> tbl_Product_Price_Index { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<tbl_Solicitor> tbl_Solicitor { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<tbl_Staff_Organogram> tbl_Staff_Organogram { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<tbl_Staff> tbl_Staff { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<tbl_Stock> tbl_Stock { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<tbl_Tax> tbl_Tax { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<tbl_Temp_Charge_Fee> tbl_Temp_Charge_Fee { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<tbl_Temp_Customer_Group_Mapping> tbl_Temp_Customer_Group_Mapping { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<tbl_Temp_Customer_Group> tbl_Temp_Customer_Group { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<tbl_Temp_Product_CollateralType> tbl_Temp_Product_CollateralType1 { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<tbl_Temp_Fee> tbl_Temp_Fee { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<tbl_Temp_Product_Fee> tbl_Temp_Product_Fee { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<tbl_Temp_Staff> tbl_Temp_Staff { get; set; }
    }
}
