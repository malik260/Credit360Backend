namespace FintrakBanking.Entities.SQLServerModel
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("core.TBL_CUSTOMER")]
    public partial class TBL_CUSTOMER
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public TBL_CUSTOMER()
        {
            TBL_CASA = new HashSet<TBL_CASA>();
            TBL_CUSTOM_FIELDS = new HashSet<TBL_CUSTOM_FIELDS>();
            TBL_CUSTOM_FIELDS_DATA = new HashSet<TBL_CUSTOM_FIELDS_DATA>();
            TBL_CUSTOM_FIELDS_DATA1 = new HashSet<TBL_CUSTOM_FIELDS_DATA>();
            TBL_CUSTOMER_CUSTOM_FIELD = new HashSet<TBL_CUSTOMER_CUSTOM_FIELD>();
            TBL_CUSTOMER_GROUP_MAPPING = new HashSet<TBL_CUSTOMER_GROUP_MAPPING>();
            TBL_CUSTOMER_ACCOUNT_KYC_ITEM = new HashSet<TBL_CUSTOMER_ACCOUNT_KYC_ITEM>();
            TBL_CUSTOMER_EDIT_HISTORY = new HashSet<TBL_CUSTOMER_EDIT_HISTORY>();
            TBL_LOAN = new HashSet<TBL_LOAN>();
            TBL_LOAN_ARCHIVE = new HashSet<TBL_LOAN_ARCHIVE>();
            TBL_LOAN_CONTINGENT = new HashSet<TBL_LOAN_CONTINGENT>();
            TBL_LOAN_HISTORY = new HashSet<TBL_LOAN_HISTORY>();
            TBL_LOAN_REVOLVING = new HashSet<TBL_LOAN_REVOLVING>();
            TBL_LOAN_REVOLVING_ARCHIVE = new HashSet<TBL_LOAN_REVOLVING_ARCHIVE>();
            TBL_LOAN_REVOLVING_HISTORY = new HashSet<TBL_LOAN_REVOLVING_HISTORY>();
            TBL_TEMP_CUSTOMER_GROUP_MAPPNG = new HashSet<TBL_TEMP_CUSTOMER_GROUP_MAPPNG>();
            TBL_TEMP_LOAN = new HashSet<TBL_TEMP_LOAN>();
            TBL_COLLATERAL_CUSTOMER = new HashSet<TBL_COLLATERAL_CUSTOMER>();
            TBL_TEMP_COLLATERAL_CUSTOMER = new HashSet<TBL_TEMP_COLLATERAL_CUSTOMER>();
            TBL_CUSTOMER_BVN = new HashSet<TBL_CUSTOMER_BVN>();
            TBL_CUSTOMER_CHILDREN = new HashSet<TBL_CUSTOMER_CHILDREN>();
            TBL_CUSTOMER_COMPANYINFOMATION = new HashSet<TBL_CUSTOMER_COMPANYINFOMATION>();
            TBL_CUSTOMER_EMPLOYMENTHISTORY = new HashSet<TBL_CUSTOMER_EMPLOYMENTHISTORY>();
            TBL_CUSTOMER_FS_CAPTION_DETAIL = new HashSet<TBL_CUSTOMER_FS_CAPTION_DETAIL>();
            TBL_CUSTOMER_GUARDIAN = new HashSet<TBL_CUSTOMER_GUARDIAN>();
            TBL_CUSTOMER_IDENTIFICATION = new HashSet<TBL_CUSTOMER_IDENTIFICATION>();
            TBL_CUSTOMER_MODIFICATION = new HashSet<TBL_CUSTOMER_MODIFICATION>();
            TBL_CUSTOMER_NEXTOFKIN = new HashSet<TBL_CUSTOMER_NEXTOFKIN>();
            TBL_CUSTOMER_PHONECONTACT = new HashSet<TBL_CUSTOMER_PHONECONTACT>();
            TBL_CUSTOMER_PRODUCT_FEE = new HashSet<TBL_CUSTOMER_PRODUCT_FEE>();
            TBL_CUSTOMER_RELATED_PARTY = new HashSet<TBL_CUSTOMER_RELATED_PARTY>();
            TBL_CUSTOMER_ADDRESS = new HashSet<TBL_CUSTOMER_ADDRESS>();
            TBL_TEMP_CUSTOMER_ADDRESS = new HashSet<TBL_TEMP_CUSTOMER_ADDRESS>();
            TBL_CUSTOMER_CLIENT_SUPPLIER = new HashSet<TBL_CUSTOMER_CLIENT_SUPPLIER>();
            TBL_LOAN_APPLICATION_ARCHIVE = new HashSet<TBL_LOAN_APPLICATION_ARCHIVE>();
            TBL_LOAN_APPLICATION_DETAIL = new HashSet<TBL_LOAN_APPLICATION_DETAIL>();
            TBL_LOAN_APPLICATION_DETL_ARCH = new HashSet<TBL_LOAN_APPLICATION_DETL_ARCH>();
            TBL_LOAN_APPLICATION = new HashSet<TBL_LOAN_APPLICATION>();
            TBL_CUSTOMER_CREDIT_BUREAU = new HashSet<TBL_CUSTOMER_CREDIT_BUREAU>();
            TBL_LOAN_PRELIMINARY_EVALUATN = new HashSet<TBL_LOAN_PRELIMINARY_EVALUATN>();
            TBL_TEMP_CUSTOMER_COMPANYINFO = new HashSet<TBL_TEMP_CUSTOMER_COMPANYINFO>();
            TBL_TEMP_CUSTOMER_PHONCONTACT = new HashSet<TBL_TEMP_CUSTOMER_PHONCONTACT>();
            TBL_TEMP_CUSTOMER = new HashSet<TBL_TEMP_CUSTOMER>();
        }

        [Key]
        public int CUSTOMERID { get; set; }

        [Required]
        //[StringLength(50)]
        public string CUSTOMERCODE { get; set; }

        public short BRANCHID { get; set; }

        public int COMPANYID { get; set; }

        //[StringLength(50)]
        public string TITLE { get; set; }

        [Required]
        //[StringLength(200)]
        public string FIRSTNAME { get; set; }

        //[StringLength(200)]
        public string MIDDLENAME { get; set; }

        //[StringLength(200)]
        public string LASTNAME { get; set; }

        //[StringLength(10)]
        public string GENDER { get; set; }

        [Column(TypeName = "date")]
        public DateTime? DATEOFBIRTH { get; set; }

        //[StringLength(200)]
        public string PLACEOFBIRTH { get; set; }

        //[StringLength(20)]
        public string NATIONALITY { get; set; }

        public int? MARITALSTATUS { get; set; }

        //[StringLength(100)]
        public string EMAILADDRESS { get; set; }

        //[StringLength(200)]
        public string MAIDENNAME { get; set; }

        //[StringLength(200)]
        public string SPOUSE { get; set; }

        //[StringLength(100)]
        public string OCCUPATION { get; set; }

        public short? CUSTOMERTYPEID { get; set; }

        public int? RELATIONSHIPOFFICERID { get; set; }

        public bool ISPOLITICALLYEXPOSED { get; set; }

        public bool ISINVESTMENTGRADE { get; set; }

        public bool ISREALATEDPARTY { get; set; }

        //[StringLength(200)]
        public string MISCODE { get; set; }

        //[StringLength(200)]
        public string MISSTAFF { get; set; }

        public int? APPROVALSTATUS { get; set; }

        [Column(TypeName = "date")]
        public DateTime? DATEACTEDON { get; set; }

        //[StringLength(150)]
        public string ACTEDONBY { get; set; }

        public bool ACCOUNTCREATIONCOMPLETE { get; set; }

        public bool CREATIONMAILSENT { get; set; }

        public short CUSTOMERSENSITIVITYLEVELID { get; set; }

        public short? SUBSECTORID { get; set; }

        public short? FSCAPTIONGROUPID { get; set; }

        //[StringLength(50)]
        public string TAXNUMBER { get; set; }

        //[StringLength(50)]
        public string CUSTOMERBVN { get; set; }

        //[StringLength(50)]
        public string CUSTOMERNIN { get; set; }

        public short? RISKRATINGID { get; set; }

        public bool? VALIDATED { get; set; }

        [Column(TypeName = "date")]
        public DateTime? DATEVALIDATED { get; set; }

        //[StringLength(50)]
        public string PROSPECTCUSTOMERCODE { get; set; }

        public bool? ISPROSPECT { get; set; }

        public int? CRMSLEGALSTATUSID { get; set; }

        public int? CRMSCOMPANYSIZEID { get; set; }

        public int? CRMSRELATIONSHIPTYPEID { get; set; }

        public int CREATEDBY { get; set; }

        public int? LASTUPDATEDBY { get; set; }

        public DateTime DATETIMECREATED { get; set; }

        public DateTime? DATETIMEUPDATED { get; set; }

        public bool DELETED { get; set; }

        public int? DELETEDBY { get; set; }

        public DateTime? DATETIMEDELETED { get; set; }

        public virtual TBL_BRANCH TBL_BRANCH { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<TBL_CASA> TBL_CASA { get; set; }

        public virtual TBL_COMPANY TBL_COMPANY { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<TBL_CUSTOM_FIELDS> TBL_CUSTOM_FIELDS { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<TBL_CUSTOM_FIELDS_DATA> TBL_CUSTOM_FIELDS_DATA { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<TBL_CUSTOM_FIELDS_DATA> TBL_CUSTOM_FIELDS_DATA1 { get; set; }

        public virtual TBL_CUSTOMER_TYPE TBL_CUSTOMER_TYPE { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<TBL_CUSTOMER_CUSTOM_FIELD> TBL_CUSTOMER_CUSTOM_FIELD { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<TBL_CUSTOMER_GROUP_MAPPING> TBL_CUSTOMER_GROUP_MAPPING { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<TBL_CUSTOMER_ACCOUNT_KYC_ITEM> TBL_CUSTOMER_ACCOUNT_KYC_ITEM { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<TBL_CUSTOMER_EDIT_HISTORY> TBL_CUSTOMER_EDIT_HISTORY { get; set; }

        public virtual TBL_STAFF TBL_STAFF { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<TBL_LOAN> TBL_LOAN { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<TBL_LOAN_ARCHIVE> TBL_LOAN_ARCHIVE { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<TBL_LOAN_CONTINGENT> TBL_LOAN_CONTINGENT { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<TBL_LOAN_HISTORY> TBL_LOAN_HISTORY { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<TBL_LOAN_REVOLVING> TBL_LOAN_REVOLVING { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<TBL_LOAN_REVOLVING_ARCHIVE> TBL_LOAN_REVOLVING_ARCHIVE { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<TBL_LOAN_REVOLVING_HISTORY> TBL_LOAN_REVOLVING_HISTORY { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<TBL_TEMP_CUSTOMER_GROUP_MAPPNG> TBL_TEMP_CUSTOMER_GROUP_MAPPNG { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<TBL_TEMP_LOAN> TBL_TEMP_LOAN { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<TBL_COLLATERAL_CUSTOMER> TBL_COLLATERAL_CUSTOMER { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<TBL_TEMP_COLLATERAL_CUSTOMER> TBL_TEMP_COLLATERAL_CUSTOMER { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<TBL_CUSTOMER_BVN> TBL_CUSTOMER_BVN { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<TBL_CUSTOMER_CHILDREN> TBL_CUSTOMER_CHILDREN { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<TBL_CUSTOMER_COMPANYINFOMATION> TBL_CUSTOMER_COMPANYINFOMATION { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<TBL_CUSTOMER_EMPLOYMENTHISTORY> TBL_CUSTOMER_EMPLOYMENTHISTORY { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<TBL_CUSTOMER_FS_CAPTION_DETAIL> TBL_CUSTOMER_FS_CAPTION_DETAIL { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<TBL_CUSTOMER_GUARDIAN> TBL_CUSTOMER_GUARDIAN { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<TBL_CUSTOMER_IDENTIFICATION> TBL_CUSTOMER_IDENTIFICATION { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<TBL_CUSTOMER_MODIFICATION> TBL_CUSTOMER_MODIFICATION { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<TBL_CUSTOMER_NEXTOFKIN> TBL_CUSTOMER_NEXTOFKIN { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<TBL_CUSTOMER_PHONECONTACT> TBL_CUSTOMER_PHONECONTACT { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<TBL_CUSTOMER_PRODUCT_FEE> TBL_CUSTOMER_PRODUCT_FEE { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<TBL_CUSTOMER_RELATED_PARTY> TBL_CUSTOMER_RELATED_PARTY { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<TBL_CUSTOMER_ADDRESS> TBL_CUSTOMER_ADDRESS { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<TBL_TEMP_CUSTOMER_ADDRESS> TBL_TEMP_CUSTOMER_ADDRESS { get; set; }

        public virtual TBL_CUSTOMER_FS_CAPTION_GROUP TBL_CUSTOMER_FS_CAPTION_GROUP { get; set; }

        public virtual TBL_CUSTOMER_RISK_RATING TBL_CUSTOMER_RISK_RATING { get; set; }

        public virtual TBL_CUSTOMER_SENSITIVITY_LEVEL TBL_CUSTOMER_SENSITIVITY_LEVEL { get; set; }

        public virtual TBL_SUB_SECTOR TBL_SUB_SECTOR { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<TBL_CUSTOMER_CLIENT_SUPPLIER> TBL_CUSTOMER_CLIENT_SUPPLIER { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<TBL_LOAN_APPLICATION_ARCHIVE> TBL_LOAN_APPLICATION_ARCHIVE { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<TBL_LOAN_APPLICATION_DETAIL> TBL_LOAN_APPLICATION_DETAIL { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<TBL_LOAN_APPLICATION_DETL_ARCH> TBL_LOAN_APPLICATION_DETL_ARCH { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<TBL_LOAN_APPLICATION> TBL_LOAN_APPLICATION { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<TBL_CUSTOMER_CREDIT_BUREAU> TBL_CUSTOMER_CREDIT_BUREAU { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<TBL_LOAN_PRELIMINARY_EVALUATN> TBL_LOAN_PRELIMINARY_EVALUATN { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<TBL_TEMP_CUSTOMER_COMPANYINFO> TBL_TEMP_CUSTOMER_COMPANYINFO { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<TBL_TEMP_CUSTOMER_PHONCONTACT> TBL_TEMP_CUSTOMER_PHONCONTACT { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<TBL_TEMP_CUSTOMER> TBL_TEMP_CUSTOMER { get; set; }
    }
}
