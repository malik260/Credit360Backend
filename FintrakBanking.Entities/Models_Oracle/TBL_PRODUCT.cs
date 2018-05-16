namespace FintrakBanking.Entities.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("TBL_PRODUCT")]
    public partial class TBL_PRODUCT
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public TBL_PRODUCT()
        {
            TBL_APPROVAL_GROUP_MAPPING = new HashSet<TBL_APPROVAL_GROUP_MAPPING>();
            TBL_CASA = new HashSet<TBL_CASA>();
            TBL_CHECKLIST_DEFINITION = new HashSet<TBL_CHECKLIST_DEFINITION>();
            TBL_CUSTOMER_PRODUCT_FEE = new HashSet<TBL_CUSTOMER_PRODUCT_FEE>();
            TBL_DAILY_ACCRUAL = new HashSet<TBL_DAILY_ACCRUAL>();
            TBL_KYC_ITEM = new HashSet<TBL_KYC_ITEM>();
            TBL_LOAN = new HashSet<TBL_LOAN>();
            TBL_LOAN_ARCHIVE = new HashSet<TBL_LOAN_ARCHIVE>();
            TBL_LOAN_CONTINGENT = new HashSet<TBL_LOAN_CONTINGENT>();
            TBL_LOAN_REVOLVING = new HashSet<TBL_LOAN_REVOLVING>();
            TBL_LOAN_REVOLVING_ARCHIVE = new HashSet<TBL_LOAN_REVOLVING_ARCHIVE>();
            TBL_TEMP_LOAN = new HashSet<TBL_TEMP_LOAN>();
            TBL_PRODUCT_COLLATERALTYPE = new HashSet<TBL_PRODUCT_COLLATERALTYPE>();
            TBL_CONDITION_PRECEDENT = new HashSet<TBL_CONDITION_PRECEDENT>();
            TBL_LOAN_APPLICATION_DETAIL = new HashSet<TBL_LOAN_APPLICATION_DETAIL>();
            TBL_LOAN_APPLICATION_DETAIL1 = new HashSet<TBL_LOAN_APPLICATION_DETAIL>();
            TBL_LOAN_APPLICATION_DETL_ARCH = new HashSet<TBL_LOAN_APPLICATION_DETL_ARCH>();
            TBL_LOAN_APPLICATION_DETL_ARCH1 = new HashSet<TBL_LOAN_APPLICATION_DETL_ARCH>();
            TBL_LOAN_APPLICATION_DETL_LOG = new HashSet<TBL_LOAN_APPLICATION_DETL_LOG>();
            TBL_OFFERLETTER = new HashSet<TBL_OFFERLETTER>();
            TBL_PRODUCT_BEHAVIOUR = new HashSet<TBL_PRODUCT_BEHAVIOUR>();
            TBL_PRODUCT_CURRENCY = new HashSet<TBL_PRODUCT_CURRENCY>();
            TBL_PRODUCT_CHARGE_FEE = new HashSet<TBL_PRODUCT_CHARGE_FEE>();
            TBL_RISK_RATING = new HashSet<TBL_RISK_RATING>();
            TBL_TEMP_OFFERLETTER = new HashSet<TBL_TEMP_OFFERLETTER>();
            TBL_TRANSACTION_DYNAMICS = new HashSet<TBL_TRANSACTION_DYNAMICS>();
        }

        [Key]
        public short PRODUCTID { get; set; }

        public int COMPANYID { get; set; }

        public short PRODUCTTYPEID { get; set; }

        public short PRODUCTCATEGORYID { get; set; }

        public bool ISMULTIPLECURENCY { get; set; }

        public short PRODUCTCLASSID { get; set; }

        [Required]
        [StringLength(50)]
        public string PRODUCTCODE { get; set; }

        [Required]
        [StringLength(200)]
        public string PRODUCTNAME { get; set; }

        [StringLength(200)]
        public string PRODUCTDESCRIPTION { get; set; }

        public int? PRINCIPALBALANCEGL { get; set; }

        public int? PRINCIPALBALANCEGL2 { get; set; }

        public int? INTERESTINCOMEEXPENSEGL { get; set; }

        public int? INTERESTRECEIVABLEPAYABLEGL { get; set; }

        public int? DORMANTGL { get; set; }

        public int? PREMIUMDISCOUNTGL { get; set; }

        public short? DEALTYPEID { get; set; }

        public short? DEALCLASSIFICATIONID { get; set; }

        public short? DAYCOUNTCONVENTIONID { get; set; }

        public short? SCHEDULETYPEID { get; set; }

        public bool ALLOWSCHEDULETYPEOVERRIDE { get; set; }

        public int MAXIMUMTENOR { get; set; }

        public int MINIMUMTENOR { get; set; }

        public decimal? MAXIMUMRATE { get; set; }

        public decimal? MINIMUMRATE { get; set; }

        public decimal? MINIMUMBALANCE { get; set; }

        public short? PRODUCTPRICEINDEXID { get; set; }

        public double? PRODUCTPRICEINDEXSPREAD { get; set; }

        public bool? ALLOWOVERDRAWN { get; set; }

        public int? OVERDRAWNGL { get; set; }

        public bool? ALLOWRATE { get; set; }

        public bool? ALLOWTENOR { get; set; }

        public bool? ALLOWMORATORIUM { get; set; }

        public bool? ALLOWCUSTOMERACCOUNTFORCEDEBIT { get; set; }

        public int? DEFAULTGRACEPERIOD { get; set; }

        public int? CLEANUPPERIOD { get; set; }

        public int? EXPIRYPERIOD { get; set; }

        public double? EQUITYCONTRIBUTION { get; set; }

        public int? MAXIMUMDRAWDOWNDURATION { get; set; }

        public int? APPROVEDBY { get; set; }

        public bool? COMPLETED { get; set; }

        public bool? APPROVED { get; set; }

        public int? CREATEDBY { get; set; }

        public int? LASTUPDATEDBY { get; set; }

        public DateTime? DATETIMECREATED { get; set; }

        public DateTime? DATETIMEUPDATED { get; set; }

        public bool DELETED { get; set; }

        public int? DELETEDBY { get; set; }

        public DateTime? DATETIMEDELETED { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<TBL_APPROVAL_GROUP_MAPPING> TBL_APPROVAL_GROUP_MAPPING { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<TBL_CASA> TBL_CASA { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<TBL_CHECKLIST_DEFINITION> TBL_CHECKLIST_DEFINITION { get; set; }

        public virtual TBL_COMPANY TBL_COMPANY { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<TBL_CUSTOMER_PRODUCT_FEE> TBL_CUSTOMER_PRODUCT_FEE { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<TBL_DAILY_ACCRUAL> TBL_DAILY_ACCRUAL { get; set; }

        public virtual TBL_DAY_COUNT_CONVENTION TBL_DAY_COUNT_CONVENTION { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<TBL_KYC_ITEM> TBL_KYC_ITEM { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<TBL_LOAN> TBL_LOAN { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<TBL_LOAN_ARCHIVE> TBL_LOAN_ARCHIVE { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<TBL_LOAN_CONTINGENT> TBL_LOAN_CONTINGENT { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<TBL_LOAN_REVOLVING> TBL_LOAN_REVOLVING { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<TBL_LOAN_REVOLVING_ARCHIVE> TBL_LOAN_REVOLVING_ARCHIVE { get; set; }

        public virtual TBL_DEAL_CLASSIFICATION TBL_DEAL_CLASSIFICATION { get; set; }

        public virtual TBL_PRODUCT_TYPE TBL_PRODUCT_TYPE { get; set; }

        public virtual TBL_PRODUCT_CATEGORY TBL_PRODUCT_CATEGORY { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<TBL_TEMP_LOAN> TBL_TEMP_LOAN { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<TBL_PRODUCT_COLLATERALTYPE> TBL_PRODUCT_COLLATERALTYPE { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<TBL_CONDITION_PRECEDENT> TBL_CONDITION_PRECEDENT { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<TBL_LOAN_APPLICATION_DETAIL> TBL_LOAN_APPLICATION_DETAIL { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<TBL_LOAN_APPLICATION_DETAIL> TBL_LOAN_APPLICATION_DETAIL1 { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<TBL_LOAN_APPLICATION_DETL_ARCH> TBL_LOAN_APPLICATION_DETL_ARCH { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<TBL_LOAN_APPLICATION_DETL_ARCH> TBL_LOAN_APPLICATION_DETL_ARCH1 { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<TBL_LOAN_APPLICATION_DETL_LOG> TBL_LOAN_APPLICATION_DETL_LOG { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<TBL_OFFERLETTER> TBL_OFFERLETTER { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<TBL_PRODUCT_BEHAVIOUR> TBL_PRODUCT_BEHAVIOUR { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<TBL_PRODUCT_CURRENCY> TBL_PRODUCT_CURRENCY { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<TBL_PRODUCT_CHARGE_FEE> TBL_PRODUCT_CHARGE_FEE { get; set; }

        public virtual TBL_CHART_OF_ACCOUNT TBL_CHART_OF_ACCOUNT { get; set; }

        public virtual TBL_CHART_OF_ACCOUNT TBL_CHART_OF_ACCOUNT1 { get; set; }

        public virtual TBL_CHART_OF_ACCOUNT TBL_CHART_OF_ACCOUNT2 { get; set; }

        public virtual TBL_CHART_OF_ACCOUNT TBL_CHART_OF_ACCOUNT3 { get; set; }

        public virtual TBL_CHART_OF_ACCOUNT TBL_CHART_OF_ACCOUNT4 { get; set; }

        public virtual TBL_CHART_OF_ACCOUNT TBL_CHART_OF_ACCOUNT5 { get; set; }

        public virtual TBL_CHART_OF_ACCOUNT TBL_CHART_OF_ACCOUNT6 { get; set; }

        public virtual TBL_DEAL_TYPE TBL_DEAL_TYPE { get; set; }

        public virtual TBL_LOAN_SCHEDULE_TYPE TBL_LOAN_SCHEDULE_TYPE { get; set; }

        public virtual TBL_PRODUCT_CLASS TBL_PRODUCT_CLASS { get; set; }

        public virtual TBL_PRODUCT_PRICE_INDEX TBL_PRODUCT_PRICE_INDEX { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<TBL_RISK_RATING> TBL_RISK_RATING { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<TBL_TEMP_OFFERLETTER> TBL_TEMP_OFFERLETTER { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<TBL_TRANSACTION_DYNAMICS> TBL_TRANSACTION_DYNAMICS { get; set; }
    }
}
