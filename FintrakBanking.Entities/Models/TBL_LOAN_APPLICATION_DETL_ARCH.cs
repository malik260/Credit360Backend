namespace FintrakBanking.Entities.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("TBL_LOAN_APPLICATION_DETL_ARCH")]
    public partial class TBL_LOAN_APPLICATION_DETL_ARCH
    {
        [Key]
        public int APPLICATIONDETAIL_ARCHIVE_ID { get; set; }
        //[Column(TypeName = "date")]
        public DateTime ARCHIVEDATE { get; set; }
        public int LOANAPPLICATIONDETAILID { get; set; }
        public int LOANAPPLICATIONID { get; set; }
        public int CUSTOMERID { get; set; }
        public short PROPOSEDPRODUCTID { get; set; }
        public int PROPOSEDTENOR { get; set; }
        public double PROPOSEDINTERESTRATE { get; set; }
        //[Column(TypeName = "money")]
        public decimal PROPOSEDAMOUNT { get; set; }
        public short APPROVEDPRODUCTID { get; set; }
        public int APPROVEDTENOR { get; set; }
        public double APPROVEDINTERESTRATE { get; set; }
        //[Column(TypeName = "money")]
        public decimal APPROVEDAMOUNT { get; set; }
        public short CURRENCYID { get; set; }
        public double EXCHANGERATE { get; set; }
        public short SUBSECTORID { get; set; }
        public short STATUSID { get; set; }
        [Required]
        //[StringLength(500)]
        public string LOANPURPOSE { get; set; }
        public decimal? EQUITYAMOUNT { get; set; }
        public bool HASDONECHECKLIST { get; set; }
        public int? EQUITYCASAACCOUNTID { get; set; }
        public short CONSESSIONAPPROVALSTATUSID { get; set; }
        //[StringLength(3000)]
        public string CONSESSIONREASON { get; set; }
        public bool ISPOLITICALLYEXPOSED { get; set; }
        public string REPAYMENTTERMS { get; set; }
        public int? REPAYMENTSCHEDULEID { get; set; }
        //[Column(TypeName = "date")]
        public DateTime? EFFECTIVEDATE { get; set; }
        public bool? ISTAKEOVERAPPLICATION { get; set; }
        //[Column(TypeName = "date")]
        public DateTime? EXPIRYDATE { get; set; }
        public int CREATEDBY { get; set; }
        public DateTime DATETIMECREATED { get; set; }
        public int? LASTUPDATEDBY { get; set; }
        public DateTime? DATETIMEUPDATED { get; set; }
        public bool DELETED { get; set; }
        public int? DELETEDBY { get; set; }
        public DateTime? DATETIMEDELETED { get; set; }
        public int? CASAACCOUNTID { get; set; }
        public int? OPERATINGCASAACCOUNTID { get; set; }
        public bool SECUREDBYCOLLATERAL { get; set; }
        public int? CRMSCOLLATERALTYPEID { get; set; }
        public int? MORATORIUMDURATION { get; set; }
        public int? CRMSFUNDINGSOURCEID { get; set; }
        public int? CRMSREPAYMENTSOURCEID { get; set; }
        //[StringLength(50)]
        public string CRMSFUNDINGSOURCECATEGORY { get; set; }
        //[StringLength(50)]
        public string CRMS_ECCI_NUMBER { get; set; }
        //[StringLength(50)]
        public string CRMSCODE { get; set; }
        public short? CRMSREPAYMENTAGREEMENTID { get; set; }
        public bool? CRMSVALIDATED { get; set; }
        public DateTime? CRMSDATE { get; set; }
        public string TRANSACTIONDYNAMICS { get; set; }
        public string CONDITIONPRECIDENT { get; set; }
        public string CONDITIONSUBSEQUENT { get; set; }
        //[StringLength(2000)]
        public string FIELD1 { get; set; }
        public double? PRODUCTPRICEINDEXRATE { get; set; }
        public short? PRODUCTPRICEINDEXID { get; set; }
        //[StringLength(1000)]
        public string FIELD2 { get; set; }
        public decimal? FIELD3 { get; set; }
        public bool ISSPECIALISED { get; set; }
        //public int? TENORFREQUENCYTYPEID { get; set; }
        public int? TENORFREQUENCYTYPEID { get; set; }
        public bool? ISFACILITYCREATED { get; set; }
        public int LOANDETAILREVIEWTYPEID { get; set; }
        public bool? ISFEETAKEN { get; set; }
        public short? TAKEFEETYPEID { get; set; }
        public short? APPROVEDLINESTATUSID { get; set; }
        public string INTERESTREPAYMENT { get; set; }
        public int? INTERESTREPAYMENTID { get; set; }
        public string MORATORIUM { get; set; }
        public bool? ISMORATORIUM { get; set; }
        public decimal? APPROVEDLINELIMIT { get; set; }

        public virtual TBL_APPROVAL_STATUS TBL_APPROVAL_STATUS { get; set; }

        public virtual TBL_CURRENCY TBL_CURRENCY { get; set; }

        public virtual TBL_CUSTOMER TBL_CUSTOMER { get; set; }

        public virtual TBL_PRODUCT TBL_PRODUCT { get; set; }

        public virtual TBL_PRODUCT TBL_PRODUCT1 { get; set; }

        public virtual TBL_SUB_SECTOR TBL_SUB_SECTOR { get; set; }

        public virtual TBL_LOAN_APPLICATION TBL_LOAN_APPLICATION { get; set; }

        public virtual TBL_LOAN_APPLICATION_DETAIL TBL_LOAN_APPLICATION_DETAIL { get; set; }
    }
}
