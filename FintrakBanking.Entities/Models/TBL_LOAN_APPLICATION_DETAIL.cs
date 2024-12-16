namespace FintrakBanking.Entities.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("TBL_LOAN_APPLICATION_DETAIL")]
    public partial class TBL_LOAN_APPLICATION_DETAIL
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public TBL_LOAN_APPLICATION_DETAIL()
        {
            TBL_LOAN = new HashSet<TBL_LOAN>();
            TBL_LOAN_APPLICATION_COLLATRL2 = new HashSet<TBL_LOAN_APPLICATION_COLLATRL2>();
            TBL_LOAN_APPLICATION_COVENANT = new HashSet<TBL_LOAN_APPLICATION_COVENANT>();
            TBL_LOAN_TRANSACTION_DYNAMICS = new HashSet<TBL_LOAN_TRANSACTION_DYNAMICS>();
            TBL_LOAN_APPLICATION_DETL_EDU = new HashSet<TBL_LOAN_APPLICATION_DETL_EDU>();
            TBL_LOAN_APPLICATION_DETL_INV = new HashSet<TBL_LOAN_APPLICATION_DETL_INV>();
            TBL_LOAN_APPLICATION_DETL_TRA = new HashSet<TBL_LOAN_APPLICATION_DETL_TRA>();
            TBL_LOAN_APPLICATION_DETL_CON = new HashSet<TBL_LOAN_APPLICATION_DETL_CON>();
            TBL_LOAN_APPLICATION_DETL_ARCH = new HashSet<TBL_LOAN_APPLICATION_DETL_ARCH>();
            TBL_LOAN_APPLICATION_DETL_BG = new HashSet<TBL_LOAN_APPLICATION_DETL_BG>();
            TBL_LOAN_APPLICATION_DETL_FEE = new HashSet<TBL_LOAN_APPLICATION_DETL_FEE>();
            TBL_LOAN_APPLICATION_DETL_LOG = new HashSet<TBL_LOAN_APPLICATION_DETL_LOG>();
            TBL_LOAN_APPLICATN_DETL_MTRIG = new HashSet<TBL_LOAN_APPLICATN_DETL_MTRIG>();
            TBL_LOAN_ARCHIVE = new HashSet<TBL_LOAN_ARCHIVE>();
            TBL_LOAN_BOOKING_REQUEST = new HashSet<TBL_LOAN_BOOKING_REQUEST>();
            TBL_LOAN_CONDITION_PRECEDENT = new HashSet<TBL_LOAN_CONDITION_PRECEDENT>();
            TBL_LOAN_CONTINGENT = new HashSet<TBL_LOAN_CONTINGENT>();
            TBL_LOAN_RATE_FEE_CONCESSION = new HashSet<TBL_LOAN_RATE_FEE_CONCESSION>();
            TBL_LOAN_REVOLVING_ARCHIVE = new HashSet<TBL_LOAN_REVOLVING_ARCHIVE>();
            TBL_LOAN_REVOLVING = new HashSet<TBL_LOAN_REVOLVING>();
            TBL_TEMP_LOAN = new HashSet<TBL_TEMP_LOAN>();
        }

        [Key]
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

        //[Column(TypeName = "money")]
        public decimal? EQUITYAMOUNT { get; set; }

        public int? EQUITYCASAACCOUNTID { get; set; }

        public short CONSESSIONAPPROVALSTATUSID { get; set; }

        //[StringLength(3000)]
        public string CONSESSIONREASON { get; set; }
        public bool? ISLINEFACILITY { get; set; }
        public bool HASDONECHECKLIST { get; set; }

        public bool ISPOLITICALLYEXPOSED { get; set; }

        //[StringLength(2000)]
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
        public string OLDAPPLICATIONREFFORRENEWAL { get; set; }
        public bool ISSPECIALISED { get; set; }
        
        //public int? TENORFREQUENCYTYPEID { get; set; }
        public int? TENORFREQUENCYTYPEID { get; set; }
        // public int? OPERATINGCASAACCOUNTID { get; set; }
        public bool? ISFACILITYCREATED { get; set; }
        [ForeignKey("TBL_LOAN_DETAIL_REVIEW_TYPE")]
        public int LOANDETAILREVIEWTYPEID { get; set; }
        public bool? ISFEETAKEN { get; set; }
        public short? TAKEFEETYPEID { get; set; }

        public short? APPROVEDLINESTATUSID { get; set; }

        public string INTERESTREPAYMENT { get; set; }
        public int? INTERESTREPAYMENTID { get; set; }
        public string MORATORIUM { get; set; }
        public bool? ISMORATORIUM { get; set; }
        public decimal? APPROVEDLINELIMIT { get; set; }
        public int? APPROVEDTRADECYCLEID { get; set; }
        public short? PROPERTYTYPEID { get; set; }
        public string PROPERTYTITLE { get; set; }
        public decimal? PROPERTYPRICE { get; set; }
        public decimal? DOWNPAYMENT { get; set; }
        //[StringLength(3999)]
        //public string REPAYMENTDATE { get; set; }
        //public decimal? REQUESTEDAMOUNT { get; set; }
        //public decimal? CREDITSCORE { get; set; }
        //public string CREDITRATING { get; set; }



        public virtual TBL_APPROVAL_STATUS TBL_APPROVAL_STATUS { get; set; }
        public virtual TBL_LOAN_DETAIL_REVIEW_TYPE TBL_LOAN_DETAIL_REVIEW_TYPE { get; set; }

        public virtual TBL_CASA TBL_CASA { get; set; }

        public virtual TBL_CASA TBL_CASA1 { get; set; }

        public virtual TBL_CURRENCY TBL_CURRENCY { get; set; }

        public virtual TBL_CUSTOMER TBL_CUSTOMER { get; set; }

        public virtual TBL_PRODUCT TBL_PRODUCT { get; set; }

        public virtual TBL_PRODUCT TBL_PRODUCT1 { get; set; }

        public virtual TBL_SUB_SECTOR TBL_SUB_SECTOR { get; set; }

        public virtual TBL_REPAYMENT_TERM TBL_REPAYMENT_TERM { get; set; }

        public virtual TBL_PRODUCT_PRICE_INDEX TBL_PRODUCT_PRICE_INDEX { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<TBL_LOAN> TBL_LOAN { get; set; }

        public virtual TBL_LOAN_APPLICATION TBL_LOAN_APPLICATION { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<TBL_LOAN_APPLICATION_COLLATRL2> TBL_LOAN_APPLICATION_COLLATRL2 { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<TBL_LOAN_APPLICATION_COVENANT> TBL_LOAN_APPLICATION_COVENANT { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<TBL_LOAN_TRANSACTION_DYNAMICS> TBL_LOAN_TRANSACTION_DYNAMICS { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<TBL_LOAN_APPLICATION_DETL_EDU> TBL_LOAN_APPLICATION_DETL_EDU { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<TBL_LOAN_APPLICATION_DETL_INV> TBL_LOAN_APPLICATION_DETL_INV { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<TBL_LOAN_APPLICATION_DETL_TRA> TBL_LOAN_APPLICATION_DETL_TRA { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<TBL_LOAN_APPLICATION_DETL_ARCH> TBL_LOAN_APPLICATION_DETL_ARCH { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<TBL_LOAN_APPLICATION_DETL_BG> TBL_LOAN_APPLICATION_DETL_BG { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<TBL_LOAN_APPLICATION_DETL_FEE> TBL_LOAN_APPLICATION_DETL_FEE { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<TBL_LOAN_APPLICATION_DETL_LOG> TBL_LOAN_APPLICATION_DETL_LOG { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<TBL_LOAN_APPLICATION_DETL_CON> TBL_LOAN_APPLICATION_DETL_CON { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<TBL_LOAN_APPLICATN_DETL_MTRIG> TBL_LOAN_APPLICATN_DETL_MTRIG { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<TBL_LOAN_ARCHIVE> TBL_LOAN_ARCHIVE { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<TBL_LOAN_BOOKING_REQUEST> TBL_LOAN_BOOKING_REQUEST { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<TBL_LOAN_CONDITION_PRECEDENT> TBL_LOAN_CONDITION_PRECEDENT { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<TBL_LOAN_CONTINGENT> TBL_LOAN_CONTINGENT { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<TBL_LOAN_RATE_FEE_CONCESSION> TBL_LOAN_RATE_FEE_CONCESSION { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<TBL_LOAN_REVOLVING_ARCHIVE> TBL_LOAN_REVOLVING_ARCHIVE { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<TBL_LOAN_REVOLVING> TBL_LOAN_REVOLVING { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<TBL_TEMP_LOAN> TBL_TEMP_LOAN { get; set; }
        public bool STAMPDUTYAPPLICABLE { get; set; }
    }
}
