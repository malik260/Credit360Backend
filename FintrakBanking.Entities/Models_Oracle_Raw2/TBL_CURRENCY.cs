namespace FintrakBanking.Entities.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("FINTRAKBANKING.TBL_CURRENCY")]
    public partial class TBL_CURRENCY
    {
        public TBL_CURRENCY()
        {
            TBL_CASA = new HashSet<TBL_CASA>();
            TBL_CHART_OF_ACCOUNT_CURRENCY = new HashSet<TBL_CHART_OF_ACCOUNT_CURRENCY>();
            TBL_COLLATERAL_CUSTOMER = new HashSet<TBL_COLLATERAL_CUSTOMER>();
            TBL_COMPANY = new HashSet<TBL_COMPANY>();
            TBL_CURRENCY_EXCHANGERATE = new HashSet<TBL_CURRENCY_EXCHANGERATE>();
            TBL_CURRENCY_EXCHANGERATE1 = new HashSet<TBL_CURRENCY_EXCHANGERATE>();
            TBL_DAILY_ACCRUAL = new HashSet<TBL_DAILY_ACCRUAL>();
            TBL_FINANCE_TRANSACTION = new HashSet<TBL_FINANCE_TRANSACTION>();
            TBL_LOAN = new HashSet<TBL_LOAN>();
            TBL_LOAN_APPLICATION_DETL_INV = new HashSet<TBL_LOAN_APPLICATION_DETL_INV>();
            TBL_LOAN_APPLICATION_DETAIL = new HashSet<TBL_LOAN_APPLICATION_DETAIL>();
            TBL_LOAN_APPLICATION_DETL_BG = new HashSet<TBL_LOAN_APPLICATION_DETL_BG>();
            TBL_LOAN_APPLICATION_DETL_ARCH = new HashSet<TBL_LOAN_APPLICATION_DETL_ARCH>();
            TBL_LOAN_ARCHIVE = new HashSet<TBL_LOAN_ARCHIVE>();
            TBL_LOAN_CONTINGENT = new HashSet<TBL_LOAN_CONTINGENT>();
            TBL_LOAN_REVOLVING = new HashSet<TBL_LOAN_REVOLVING>();
            TBL_LOAN_REVOLVING_ARCHIVE = new HashSet<TBL_LOAN_REVOLVING_ARCHIVE>();
            TBL_PRODUCT_CURRENCY = new HashSet<TBL_PRODUCT_CURRENCY>();
            TBL_TEMP_CHART_OF_ACCOUNT_CUR = new HashSet<TBL_TEMP_CHART_OF_ACCOUNT_CUR>();
            TBL_TEMP_COLLATERAL_CUSTOMER = new HashSet<TBL_TEMP_COLLATERAL_CUSTOMER>();
            TBL_TEMP_LOAN = new HashSet<TBL_TEMP_LOAN>();
            TBL_TEMP_PRODUCT_CURRENCY = new HashSet<TBL_TEMP_PRODUCT_CURRENCY>();
        }

        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public int CURRENCYID { get; set; }

        [Required]
        [StringLength(50)]
        public string CURRENCYCODE { get; set; }

        [Required]
        [StringLength(250)]
        public string CURRENCYNAME { get; set; }

        public int INUSE { get; set; }

        public int? CREATEDBY { get; set; }

        public int? LASTUPDATEDBY { get; set; }

        public DateTime DATETIMECREATED { get; set; }

        public DateTime? DATETIMEUPDATED { get; set; }

        public virtual ICollection<TBL_CASA> TBL_CASA { get; set; }

        public virtual ICollection<TBL_CHART_OF_ACCOUNT_CURRENCY> TBL_CHART_OF_ACCOUNT_CURRENCY { get; set; }

        public virtual ICollection<TBL_COLLATERAL_CUSTOMER> TBL_COLLATERAL_CUSTOMER { get; set; }

        public virtual ICollection<TBL_COMPANY> TBL_COMPANY { get; set; }

        public virtual ICollection<TBL_CURRENCY_EXCHANGERATE> TBL_CURRENCY_EXCHANGERATE { get; set; }

        public virtual ICollection<TBL_CURRENCY_EXCHANGERATE> TBL_CURRENCY_EXCHANGERATE1 { get; set; }

        public virtual ICollection<TBL_DAILY_ACCRUAL> TBL_DAILY_ACCRUAL { get; set; }

        public virtual ICollection<TBL_FINANCE_TRANSACTION> TBL_FINANCE_TRANSACTION { get; set; }

        public virtual ICollection<TBL_LOAN> TBL_LOAN { get; set; }

        public virtual ICollection<TBL_LOAN_APPLICATION_DETL_INV> TBL_LOAN_APPLICATION_DETL_INV { get; set; }

        public virtual ICollection<TBL_LOAN_APPLICATION_DETAIL> TBL_LOAN_APPLICATION_DETAIL { get; set; }

        public virtual ICollection<TBL_LOAN_APPLICATION_DETL_BG> TBL_LOAN_APPLICATION_DETL_BG { get; set; }

        public virtual ICollection<TBL_LOAN_APPLICATION_DETL_ARCH> TBL_LOAN_APPLICATION_DETL_ARCH { get; set; }

        public virtual ICollection<TBL_LOAN_ARCHIVE> TBL_LOAN_ARCHIVE { get; set; }

        public virtual ICollection<TBL_LOAN_CONTINGENT> TBL_LOAN_CONTINGENT { get; set; }

        public virtual ICollection<TBL_LOAN_REVOLVING> TBL_LOAN_REVOLVING { get; set; }

        public virtual ICollection<TBL_LOAN_REVOLVING_ARCHIVE> TBL_LOAN_REVOLVING_ARCHIVE { get; set; }

        public virtual ICollection<TBL_PRODUCT_CURRENCY> TBL_PRODUCT_CURRENCY { get; set; }

        public virtual ICollection<TBL_TEMP_CHART_OF_ACCOUNT_CUR> TBL_TEMP_CHART_OF_ACCOUNT_CUR { get; set; }

        public virtual ICollection<TBL_TEMP_COLLATERAL_CUSTOMER> TBL_TEMP_COLLATERAL_CUSTOMER { get; set; }

        public virtual ICollection<TBL_TEMP_LOAN> TBL_TEMP_LOAN { get; set; }

        public virtual ICollection<TBL_TEMP_PRODUCT_CURRENCY> TBL_TEMP_PRODUCT_CURRENCY { get; set; }
    }
}
