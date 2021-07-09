namespace FintrakBanking.Entities.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("FINTRAKBANKING.TBL_TEMP_PRODUCT")]
    public partial class TBL_TEMP_PRODUCT
    {
        public TBL_TEMP_PRODUCT()
        {
            TBL_TEMP_PRODUCT_CHARGE_FEE = new HashSet<TBL_TEMP_PRODUCT_CHARGE_FEE>();
            TBL_TEMP_PRODUCT_COLLATERALTYP = new HashSet<TBL_TEMP_PRODUCT_COLLATERALTYP>();
            TBL_TEMP_PRODUCT_CURRENCY = new HashSet<TBL_TEMP_PRODUCT_CURRENCY>();
        }

        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public int PRODUCTID { get; set; }

        public int COMPANYID { get; set; }

        public int PRODUCTTYPEID { get; set; }

        public int PRODUCTCATEGORYID { get; set; }

        public int ISMULTIPLECURENCY { get; set; }

        public int PRODUCTCLASSID { get; set; }

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

        public int? DEALTYPEID { get; set; }

        public int? DEALCLASSIFICATIONID { get; set; }

        public int? DAYCOUNTCONVENTIONID { get; set; }

        public int? SCHEDULETYPEID { get; set; }

        public int ALLOWSCHEDULETYPEOVERRIDE { get; set; }

        public int MAXIMUMTENOR { get; set; }

        public int MINIMUMTENOR { get; set; }

        public decimal? MAXIMUMRATE { get; set; }

        public decimal? MINIMUMRATE { get; set; }

        public decimal? MINIMUMBALANCE { get; set; }

        public int? PRODUCTPRICEINDEXID { get; set; }

        public decimal? PRODUCTPRICEINDEXSPREAD { get; set; }

        public int? ALLOWOVERDRAWN { get; set; }

        public int? OVERDRAWNGL { get; set; }

        public int? ALLOWRATE { get; set; }

        public int? ALLOWTENOR { get; set; }

        public int? ALLOWMORATORIUM { get; set; }

        public int? APPROVEDBY { get; set; }

        public int? ALLOWCUSTOMERACCOUNTFORCEDEBIT { get; set; }

        public int? DEFAULTGRACEPERIOD { get; set; }

        public int? CLEANUPPERIOD { get; set; }

        public int? EXPIRYPERIOD { get; set; }

        public decimal? EQUITYCONTRIBUTION { get; set; }

        public int? COMPLETED { get; set; }

        public int? APPROVED { get; set; }

        public int ISCURRENT { get; set; }

        public int APPROVALSTATUSID { get; set; }

        public int? CREATEDBY { get; set; }

        public int? LASTUPDATEDBY { get; set; }

        public DateTime? DATETIMECREATED { get; set; }

        public DateTime? DATETIMEUPDATED { get; set; }

        public int DELETED { get; set; }

        public int? DELETEDBY { get; set; }

        public DateTime? DATETIMEDELETED { get; set; }

        public virtual TBL_APPROVAL_STATUS TBL_APPROVAL_STATUS { get; set; }

        public virtual TBL_CHART_OF_ACCOUNT TBL_CHART_OF_ACCOUNT { get; set; }

        public virtual TBL_CHART_OF_ACCOUNT TBL_CHART_OF_ACCOUNT1 { get; set; }

        public virtual TBL_CHART_OF_ACCOUNT TBL_CHART_OF_ACCOUNT2 { get; set; }

        public virtual TBL_CHART_OF_ACCOUNT TBL_CHART_OF_ACCOUNT3 { get; set; }

        public virtual TBL_CHART_OF_ACCOUNT TBL_CHART_OF_ACCOUNT4 { get; set; }

        public virtual TBL_CHART_OF_ACCOUNT TBL_CHART_OF_ACCOUNT5 { get; set; }

        public virtual TBL_CHART_OF_ACCOUNT TBL_CHART_OF_ACCOUNT6 { get; set; }

        public virtual TBL_COMPANY TBL_COMPANY { get; set; }

        public virtual TBL_DAY_COUNT_CONVENTION TBL_DAY_COUNT_CONVENTION { get; set; }

        public virtual TBL_DEAL_CLASSIFICATION TBL_DEAL_CLASSIFICATION { get; set; }

        public virtual TBL_DEAL_TYPE TBL_DEAL_TYPE { get; set; }

        public virtual TBL_LOAN_SCHEDULE_TYPE TBL_LOAN_SCHEDULE_TYPE { get; set; }

        public virtual TBL_PRODUCT_CATEGORY TBL_PRODUCT_CATEGORY { get; set; }

        public virtual TBL_PRODUCT_CLASS TBL_PRODUCT_CLASS { get; set; }

        public virtual TBL_PRODUCT_PRICE_INDEX TBL_PRODUCT_PRICE_INDEX { get; set; }

        public virtual TBL_PRODUCT_TYPE TBL_PRODUCT_TYPE { get; set; }

        public virtual ICollection<TBL_TEMP_PRODUCT_CHARGE_FEE> TBL_TEMP_PRODUCT_CHARGE_FEE { get; set; }

        public virtual ICollection<TBL_TEMP_PRODUCT_COLLATERALTYP> TBL_TEMP_PRODUCT_COLLATERALTYP { get; set; }

        public virtual ICollection<TBL_TEMP_PRODUCT_CURRENCY> TBL_TEMP_PRODUCT_CURRENCY { get; set; }
    }
}
