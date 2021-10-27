namespace FintrakBanking.Entities.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("FINTRAKBANKING.TBL_COMPANY")]
    public partial class TBL_COMPANY
    {
        public TBL_COMPANY()
        {
            TBL_APPROVAL_GROUP = new HashSet<TBL_APPROVAL_GROUP>();
            TBL_APPROVAL_TRAIL = new HashSet<TBL_APPROVAL_TRAIL>();
            TBL_BRANCH = new HashSet<TBL_BRANCH>();
            TBL_BRANCH_REGION = new HashSet<TBL_BRANCH_REGION>();
            TBL_CALL_MEMO_LIMIT = new HashSet<TBL_CALL_MEMO_LIMIT>();
            TBL_CASA = new HashSet<TBL_CASA>();
            TBL_CASA_LIEN = new HashSet<TBL_CASA_LIEN>();
            TBL_CHARGE_FEE = new HashSet<TBL_CHARGE_FEE>();
            TBL_CHART_OF_ACCOUNT = new HashSet<TBL_CHART_OF_ACCOUNT>();
            TBL_CHECKLIST_DEFINITION = new HashSet<TBL_CHECKLIST_DEFINITION>();
            TBL_COLLATERAL_CUSTOMER = new HashSet<TBL_COLLATERAL_CUSTOMER>();
            TBL_COMPANY1 = new HashSet<TBL_COMPANY>();
            TBL_CREDIT_APPRAISAL_MEMORANDM = new HashSet<TBL_CREDIT_APPRAISAL_MEMORANDM>();
            TBL_CREDIT_TEMPLATE = new HashSet<TBL_CREDIT_TEMPLATE>();
            TBL_CUSTOMER = new HashSet<TBL_CUSTOMER>();
            TBL_CUSTOMER_BLACKLIST = new HashSet<TBL_CUSTOMER_BLACKLIST>();
            TBL_CUSTOMER_EMPLOYER = new HashSet<TBL_CUSTOMER_EMPLOYER>();
            TBL_CUSTOMER_PRODUCT_FEE = new HashSet<TBL_CUSTOMER_PRODUCT_FEE>();
            TBL_CUSTOMER_RISK_RATING = new HashSet<TBL_CUSTOMER_RISK_RATING>();
            TBL_DAILY_ACCRUAL = new HashSet<TBL_DAILY_ACCRUAL>();
            TBL_FINANCE_ENDOFDAY = new HashSet<TBL_FINANCE_ENDOFDAY>();
            TBL_FINANCE_TRANSACTION = new HashSet<TBL_FINANCE_TRANSACTION>();
            TBL_FINANCECURRENTDATE = new HashSet<TBL_FINANCECURRENTDATE>();
            TBL_LOAN = new HashSet<TBL_LOAN>();
            TBL_LOAN_APPLICATION_ARCHIVE = new HashSet<TBL_LOAN_APPLICATION_ARCHIVE>();
            TBL_LOAN_APPLICATION = new HashSet<TBL_LOAN_APPLICATION>();
            TBL_LOAN_ARCHIVE = new HashSet<TBL_LOAN_ARCHIVE>();
            TBL_LOAN_BULK_INTEREST_REVIEW = new HashSet<TBL_LOAN_BULK_INTEREST_REVIEW>();
            TBL_LOAN_CAMSOL = new HashSet<TBL_LOAN_CAMSOL>();
            TBL_LOAN_CONTINGENT = new HashSet<TBL_LOAN_CONTINGENT>();
            TBL_LOAN_MARKET = new HashSet<TBL_LOAN_MARKET>();
            TBL_LOAN_PRELIMINARY_EVALUATN = new HashSet<TBL_LOAN_PRELIMINARY_EVALUATN>();
            TBL_LOAN_PRINCIPAL = new HashSet<TBL_LOAN_PRINCIPAL>();
            TBL_LOAN_REVIEW_APPLICATN_CAM = new HashSet<TBL_LOAN_REVIEW_APPLICATN_CAM>();
            TBL_LOAN_REVOLVING = new HashSet<TBL_LOAN_REVOLVING>();
            TBL_LOAN_REVOLVING_ARCHIVE = new HashSet<TBL_LOAN_REVOLVING_ARCHIVE>();
            TBL_MIS_INFO = new HashSet<TBL_MIS_INFO>();
            TBL_PRODUCT = new HashSet<TBL_PRODUCT>();
            TBL_PRODUCT_CHARGE_FEE = new HashSet<TBL_PRODUCT_CHARGE_FEE>();
            TBL_PRODUCT_COLLATERALTYPE = new HashSet<TBL_PRODUCT_COLLATERALTYPE>();
            TBL_PRODUCT_PRICE_INDEX = new HashSet<TBL_PRODUCT_PRICE_INDEX>();
            TBL_SETUP_COMPANY = new HashSet<TBL_SETUP_COMPANY>();
            TBL_SIGNATURE_DOCUMENT_STAFF = new HashSet<TBL_SIGNATURE_DOCUMENT_STAFF>();
            TBL_STAFF = new HashSet<TBL_STAFF>();
            TBL_TAX = new HashSet<TBL_TAX>();
            TBL_TEMP_CHARGE_FEE = new HashSet<TBL_TEMP_CHARGE_FEE>();
            TBL_TEMP_CHART_OF_ACCOUNT = new HashSet<TBL_TEMP_CHART_OF_ACCOUNT>();
            TBL_TEMP_COLLATERAL_CUSTOMER = new HashSet<TBL_TEMP_COLLATERAL_CUSTOMER>();
            TBL_TEMP_CUSTOMER_EMPLOYER = new HashSet<TBL_TEMP_CUSTOMER_EMPLOYER>();
            TBL_TEMP_CUSTOMER_GROUP = new HashSet<TBL_TEMP_CUSTOMER_GROUP>();
            TBL_TEMP_CUSTOMER_GROUP_MAPPNG = new HashSet<TBL_TEMP_CUSTOMER_GROUP_MAPPNG>();
            TBL_TEMP_LOAN = new HashSet<TBL_TEMP_LOAN>();
            TBL_TEMP_PRODUCT = new HashSet<TBL_TEMP_PRODUCT>();
            TBL_TEMP_PRODUCT_CHARGE_FEE = new HashSet<TBL_TEMP_PRODUCT_CHARGE_FEE>();
            TBL_TEMP_PRODUCT_COLLATERALTYP = new HashSet<TBL_TEMP_PRODUCT_COLLATERALTYP>();
            TBL_TEMP_STAFF = new HashSet<TBL_TEMP_STAFF>();
        }

        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public int COMPANYID { get; set; }

        [Required]
        [StringLength(250)]
        public string NAME { get; set; }

        [StringLength(250)]
        public string ADDRESS { get; set; }

        [StringLength(100)]
        public string TELEPHONE { get; set; }

        [StringLength(100)]
        public string EMAIL { get; set; }

        public decimal? SHAREHOLDERSFUND { get; set; }

        public decimal? PRELIMINARYEVALUATION_LIMIT { get; set; }

        public int COUNTRYID { get; set; }

        public int CURRENCYID { get; set; }

        public int LANGUAGEID { get; set; }

        public int? NATUREOFBUSINESSID { get; set; }

        [StringLength(50)]
        public string NAMEOFSCHEME { get; set; }

        [StringLength(50)]
        public string FUNCTIONSREGISTERED { get; set; }

        public decimal? AUTHORISEDSHARECAPITAL { get; set; }

        [StringLength(100)]
        public string NAMEOFREGISTRAR { get; set; }

        [StringLength(100)]
        public string NAMEOFTRUSTEES { get; set; }

        [StringLength(50)]
        public string FORMERMANAGERSTRUSTEES { get; set; }

        public int? INITIALFLOATATION { get; set; }

        public int? INITIALSUBSCRIPTION { get; set; }

        [StringLength(100)]
        public string REGISTEREDBY { get; set; }

        [StringLength(1000)]
        public string TRUSTEESADDRESS { get; set; }

        [StringLength(1000)]
        public string INVESTMENTOBJECTIVE { get; set; }

        [StringLength(100)]
        public string WEBSITE { get; set; }

        [StringLength(10)]
        public string EBUSINESSCODE { get; set; }

        [StringLength(50)]
        public string EOYPROFITANDLOSSGL { get; set; }

        public int? COMPANYCLASSID { get; set; }

        public int? COMPANYTYPEID { get; set; }

        public int? ACCOUNTINGSTANDARDID { get; set; }

        public int? MANAGEMENTTYPEID { get; set; }

        public int? PARENTID { get; set; }

        [StringLength(100)]
        public string LOGOPATH { get; set; }

        public int? CREATEDBY { get; set; }

        public int? LASTUPDATEDBY { get; set; }

        public DateTime? DATETIMECREATED { get; set; }

        public DateTime? DATETIMEUPDATED { get; set; }

        public byte[] COMPANYLOGO { get; set; }

        public DateTime? DATEOFCOMMENCEMENT { get; set; }

        public DateTime? DATEOFRENEWALOFREGISTRATION { get; set; }

        public DateTime? DATEOFINCORPORATION { get; set; }

        public virtual TBL_ACCOUNTING_STANDARD TBL_ACCOUNTING_STANDARD { get; set; }

        public virtual ICollection<TBL_APPROVAL_GROUP> TBL_APPROVAL_GROUP { get; set; }

        public virtual ICollection<TBL_APPROVAL_TRAIL> TBL_APPROVAL_TRAIL { get; set; }

        public virtual ICollection<TBL_BRANCH> TBL_BRANCH { get; set; }

        public virtual ICollection<TBL_BRANCH_REGION> TBL_BRANCH_REGION { get; set; }

        public virtual ICollection<TBL_CALL_MEMO_LIMIT> TBL_CALL_MEMO_LIMIT { get; set; }

        public virtual ICollection<TBL_CASA> TBL_CASA { get; set; }

        public virtual ICollection<TBL_CASA_LIEN> TBL_CASA_LIEN { get; set; }

        public virtual ICollection<TBL_CHARGE_FEE> TBL_CHARGE_FEE { get; set; }

        public virtual ICollection<TBL_CHART_OF_ACCOUNT> TBL_CHART_OF_ACCOUNT { get; set; }

        public virtual ICollection<TBL_CHECKLIST_DEFINITION> TBL_CHECKLIST_DEFINITION { get; set; }

        public virtual ICollection<TBL_COLLATERAL_CUSTOMER> TBL_COLLATERAL_CUSTOMER { get; set; }

        public virtual TBL_COUNTRY TBL_COUNTRY { get; set; }

        public virtual ICollection<TBL_COMPANY> TBL_COMPANY1 { get; set; }

        public virtual TBL_COMPANY TBL_COMPANY2 { get; set; }

        public virtual TBL_COMPANY_TYPE TBL_COMPANY_TYPE { get; set; }

        public virtual TBL_NATURE_OF_BUSINESS TBL_NATURE_OF_BUSINESS { get; set; }

        public virtual TBL_MANAGEMENT_TYPE TBL_MANAGEMENT_TYPE { get; set; }

        public virtual TBL_COMPANY_CLASS TBL_COMPANY_CLASS { get; set; }

        public virtual TBL_CURRENCY TBL_CURRENCY { get; set; }

        public virtual TBL_LANGUAGE TBL_LANGUAGE { get; set; }

        public virtual ICollection<TBL_CREDIT_APPRAISAL_MEMORANDM> TBL_CREDIT_APPRAISAL_MEMORANDM { get; set; }

        public virtual ICollection<TBL_CREDIT_TEMPLATE> TBL_CREDIT_TEMPLATE { get; set; }

        public virtual ICollection<TBL_CUSTOMER> TBL_CUSTOMER { get; set; }

        public virtual ICollection<TBL_CUSTOMER_BLACKLIST> TBL_CUSTOMER_BLACKLIST { get; set; }

        public virtual ICollection<TBL_CUSTOMER_EMPLOYER> TBL_CUSTOMER_EMPLOYER { get; set; }

        public virtual ICollection<TBL_CUSTOMER_PRODUCT_FEE> TBL_CUSTOMER_PRODUCT_FEE { get; set; }

        public virtual ICollection<TBL_CUSTOMER_RISK_RATING> TBL_CUSTOMER_RISK_RATING { get; set; }

        public virtual ICollection<TBL_DAILY_ACCRUAL> TBL_DAILY_ACCRUAL { get; set; }

        public virtual ICollection<TBL_FINANCE_ENDOFDAY> TBL_FINANCE_ENDOFDAY { get; set; }

        public virtual ICollection<TBL_FINANCE_TRANSACTION> TBL_FINANCE_TRANSACTION { get; set; }

        public virtual ICollection<TBL_FINANCECURRENTDATE> TBL_FINANCECURRENTDATE { get; set; }

        public virtual ICollection<TBL_LOAN> TBL_LOAN { get; set; }

        public virtual ICollection<TBL_LOAN_APPLICATION_ARCHIVE> TBL_LOAN_APPLICATION_ARCHIVE { get; set; }

        public virtual ICollection<TBL_LOAN_APPLICATION> TBL_LOAN_APPLICATION { get; set; }

        public virtual ICollection<TBL_LOAN_ARCHIVE> TBL_LOAN_ARCHIVE { get; set; }

        public virtual ICollection<TBL_LOAN_BULK_INTEREST_REVIEW> TBL_LOAN_BULK_INTEREST_REVIEW { get; set; }

        public virtual ICollection<TBL_LOAN_CAMSOL> TBL_LOAN_CAMSOL { get; set; }

        public virtual ICollection<TBL_LOAN_CONTINGENT> TBL_LOAN_CONTINGENT { get; set; }

        public virtual ICollection<TBL_LOAN_MARKET> TBL_LOAN_MARKET { get; set; }

        public virtual ICollection<TBL_LOAN_PRELIMINARY_EVALUATN> TBL_LOAN_PRELIMINARY_EVALUATN { get; set; }

        public virtual ICollection<TBL_LOAN_PRINCIPAL> TBL_LOAN_PRINCIPAL { get; set; }

        public virtual ICollection<TBL_LOAN_REVIEW_APPLICATN_CAM> TBL_LOAN_REVIEW_APPLICATN_CAM { get; set; }

        public virtual ICollection<TBL_LOAN_REVOLVING> TBL_LOAN_REVOLVING { get; set; }

        public virtual ICollection<TBL_LOAN_REVOLVING_ARCHIVE> TBL_LOAN_REVOLVING_ARCHIVE { get; set; }

        public virtual ICollection<TBL_MIS_INFO> TBL_MIS_INFO { get; set; }

        public virtual ICollection<TBL_PRODUCT> TBL_PRODUCT { get; set; }

        public virtual ICollection<TBL_PRODUCT_CHARGE_FEE> TBL_PRODUCT_CHARGE_FEE { get; set; }

        public virtual ICollection<TBL_PRODUCT_COLLATERALTYPE> TBL_PRODUCT_COLLATERALTYPE { get; set; }

        public virtual ICollection<TBL_PRODUCT_PRICE_INDEX> TBL_PRODUCT_PRICE_INDEX { get; set; }

        public virtual ICollection<TBL_SETUP_COMPANY> TBL_SETUP_COMPANY { get; set; }

        public virtual ICollection<TBL_SIGNATURE_DOCUMENT_STAFF> TBL_SIGNATURE_DOCUMENT_STAFF { get; set; }

        public virtual ICollection<TBL_STAFF> TBL_STAFF { get; set; }

        public virtual ICollection<TBL_TAX> TBL_TAX { get; set; }

        public virtual ICollection<TBL_TEMP_CHARGE_FEE> TBL_TEMP_CHARGE_FEE { get; set; }

        public virtual ICollection<TBL_TEMP_CHART_OF_ACCOUNT> TBL_TEMP_CHART_OF_ACCOUNT { get; set; }

        public virtual ICollection<TBL_TEMP_COLLATERAL_CUSTOMER> TBL_TEMP_COLLATERAL_CUSTOMER { get; set; }

        public virtual ICollection<TBL_TEMP_CUSTOMER_EMPLOYER> TBL_TEMP_CUSTOMER_EMPLOYER { get; set; }

        public virtual ICollection<TBL_TEMP_CUSTOMER_GROUP> TBL_TEMP_CUSTOMER_GROUP { get; set; }

        public virtual ICollection<TBL_TEMP_CUSTOMER_GROUP_MAPPNG> TBL_TEMP_CUSTOMER_GROUP_MAPPNG { get; set; }

        public virtual ICollection<TBL_TEMP_LOAN> TBL_TEMP_LOAN { get; set; }

        public virtual ICollection<TBL_TEMP_PRODUCT> TBL_TEMP_PRODUCT { get; set; }

        public virtual ICollection<TBL_TEMP_PRODUCT_CHARGE_FEE> TBL_TEMP_PRODUCT_CHARGE_FEE { get; set; }

        public virtual ICollection<TBL_TEMP_PRODUCT_COLLATERALTYP> TBL_TEMP_PRODUCT_COLLATERALTYP { get; set; }

        public virtual ICollection<TBL_TEMP_STAFF> TBL_TEMP_STAFF { get; set; }
    }
}
