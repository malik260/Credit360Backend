namespace FintrakBanking.Entities.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("TBL_CHART_OF_ACCOUNT")]
    public partial class TBL_CHART_OF_ACCOUNT
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public TBL_CHART_OF_ACCOUNT()
        {
            TBL_CHARGE_FEE_DETAIL = new HashSet<TBL_CHARGE_FEE_DETAIL>();
            TBL_CHARGE_FEE_DETAIL1 = new HashSet<TBL_CHARGE_FEE_DETAIL>();
            
            TBL_FINANCE_TRANSACTION = new HashSet<TBL_FINANCE_TRANSACTION>();
            TBL_PRODUCT = new HashSet<TBL_PRODUCT>();
            TBL_PRODUCT1 = new HashSet<TBL_PRODUCT>();
            TBL_PRODUCT2 = new HashSet<TBL_PRODUCT>();
            TBL_PRODUCT3 = new HashSet<TBL_PRODUCT>();
            TBL_PRODUCT4 = new HashSet<TBL_PRODUCT>();
            TBL_PRODUCT5 = new HashSet<TBL_PRODUCT>();
            TBL_PRODUCT6 = new HashSet<TBL_PRODUCT>();
            TBL_SETUP_COMPANY = new HashSet<TBL_SETUP_COMPANY>();
            TBL_TAX = new HashSet<TBL_TAX>();
            TBL_COLLATERAL_TYPE = new HashSet<TBL_COLLATERAL_TYPE>();
            TBL_CREDIT_BUREAU = new HashSet<TBL_CREDIT_BUREAU>();
            TBL_CHARGES = new HashSet<TBL_CHARGES>();
            TBL_CHART_OF_ACCOUNT_CURRENCY = new HashSet<TBL_CHART_OF_ACCOUNT_CURRENCY>();
            TBL_TEMP_PRODUCT = new HashSet<TBL_TEMP_PRODUCT>();
            TBL_TEMP_PRODUCT1 = new HashSet<TBL_TEMP_PRODUCT>();
            TBL_TEMP_PRODUCT2 = new HashSet<TBL_TEMP_PRODUCT>();
            TBL_TEMP_PRODUCT3 = new HashSet<TBL_TEMP_PRODUCT>();
            TBL_TEMP_PRODUCT4 = new HashSet<TBL_TEMP_PRODUCT>();
            TBL_TEMP_PRODUCT5 = new HashSet<TBL_TEMP_PRODUCT>();
            TBL_TEMP_CHARGE_FEE_DETAIL = new HashSet<TBL_TEMP_CHARGE_FEE_DETAIL>();
            TBL_TEMP_CHARGE_FEE_DETAIL1 = new HashSet<TBL_TEMP_CHARGE_FEE_DETAIL>();
            
            TBL_TEMP_PRODUCT6 = new HashSet<TBL_TEMP_PRODUCT>();
        }

        [Key]
        public int GLACCOUNTID { get; set; }

        public int ACCOUNTTYPEID { get; set; }

        [Required]
        [StringLength(20)]
        public string ACCOUNTCODE { get; set; }

        [Required]
        [StringLength(500)]
        public string ACCOUNTNAME { get; set; }

        public int COMPANYID { get; set; }

        public short BRANCHID { get; set; }

        public short GLCLASSID { get; set; }

        public bool SYSTEMUSE { get; set; }

        public int? ACCOUNTSTATUSID { get; set; }

        public bool BRANCHSPECIFIC { get; set; }

        [StringLength(20)]
        public string OLDACCOUNTID { get; set; }

        public short FSCAPTIONID { get; set; }

        public bool DELETED { get; set; }

        public int? DELETEDBY { get; set; }

        public DateTime? DATETIMEDELETED { get; set; }

        public int CREATEDBY { get; set; }

        public int? LASTUPDATEDBY { get; set; }

        public DateTime DATETIMECREATED { get; set; }

        public DateTime? DATETIMEUPDATED { get; set; }

        public virtual TBL_BRANCH TBL_BRANCH { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<TBL_CHARGE_FEE_DETAIL> TBL_CHARGE_FEE_DETAIL { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<TBL_CHARGE_FEE_DETAIL> TBL_CHARGE_FEE_DETAIL1 { get; set; }

        public virtual TBL_COMPANY TBL_COMPANY { get; set; }

 

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<TBL_FINANCE_TRANSACTION> TBL_FINANCE_TRANSACTION { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<TBL_PRODUCT> TBL_PRODUCT { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<TBL_PRODUCT> TBL_PRODUCT1 { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<TBL_PRODUCT> TBL_PRODUCT2 { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<TBL_PRODUCT> TBL_PRODUCT3 { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<TBL_PRODUCT> TBL_PRODUCT4 { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<TBL_PRODUCT> TBL_PRODUCT5 { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<TBL_PRODUCT> TBL_PRODUCT6 { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<TBL_SETUP_COMPANY> TBL_SETUP_COMPANY { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<TBL_TAX> TBL_TAX { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<TBL_COLLATERAL_TYPE> TBL_COLLATERAL_TYPE { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<TBL_CREDIT_BUREAU> TBL_CREDIT_BUREAU { get; set; }

        public virtual TBL_ACCOUNT_TYPE TBL_ACCOUNT_TYPE { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<TBL_CHARGES> TBL_CHARGES { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<TBL_CHART_OF_ACCOUNT_CURRENCY> TBL_CHART_OF_ACCOUNT_CURRENCY { get; set; }

        public virtual TBL_CHART_OF_ACCOUNT_CLASS TBL_CHART_OF_ACCOUNT_CLASS { get; set; }

        public virtual TBL_FINANCIAL_STATEMENT_CAPTN TBL_FINANCIAL_STATEMENT_CAPTN { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<TBL_TEMP_PRODUCT> TBL_TEMP_PRODUCT { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<TBL_TEMP_PRODUCT> TBL_TEMP_PRODUCT1 { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<TBL_TEMP_PRODUCT> TBL_TEMP_PRODUCT2 { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<TBL_TEMP_PRODUCT> TBL_TEMP_PRODUCT3 { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<TBL_TEMP_PRODUCT> TBL_TEMP_PRODUCT4 { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<TBL_TEMP_PRODUCT> TBL_TEMP_PRODUCT5 { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<TBL_TEMP_CHARGE_FEE_DETAIL> TBL_TEMP_CHARGE_FEE_DETAIL { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<TBL_TEMP_CHARGE_FEE_DETAIL> TBL_TEMP_CHARGE_FEE_DETAIL1 { get; set; }        

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<TBL_TEMP_PRODUCT> TBL_TEMP_PRODUCT6 { get; set; }
    }
}
