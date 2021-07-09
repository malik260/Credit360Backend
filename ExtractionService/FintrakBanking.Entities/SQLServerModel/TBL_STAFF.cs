namespace FintrakBanking.Entities.SQLServerModel
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("core.TBL_STAFF")]
    public partial class TBL_STAFF
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public TBL_STAFF()
        {
            TBL_APPROVAL_LEVEL_STAFF = new HashSet<TBL_APPROVAL_LEVEL_STAFF>();
            TBL_APPROVAL_TRAIL = new HashSet<TBL_APPROVAL_TRAIL>();
            TBL_APPROVAL_TRAIL1 = new HashSet<TBL_APPROVAL_TRAIL>();
            TBL_APPROVAL_TRAIL2 = new HashSet<TBL_APPROVAL_TRAIL>();
            TBL_APPROVAL_TRAIL3 = new HashSet<TBL_APPROVAL_TRAIL>();
            TBL_AUDIT = new HashSet<TBL_AUDIT>();
            TBL_BRANCH_REGION = new HashSet<TBL_BRANCH_REGION>();
            TBL_CASA = new HashSet<TBL_CASA>();
            TBL_CASA1 = new HashSet<TBL_CASA>();
            TBL_CUSTOMER = new HashSet<TBL_CUSTOMER>();
            TBL_FINANCE_TRANSACTION = new HashSet<TBL_FINANCE_TRANSACTION>();
            TBL_FINANCE_TRANSACTION1 = new HashSet<TBL_FINANCE_TRANSACTION>();
            TBL_JOB_REQUEST = new HashSet<TBL_JOB_REQUEST>();
            TBL_JOB_REQUEST1 = new HashSet<TBL_JOB_REQUEST>();
            TBL_JOB_REQUEST2 = new HashSet<TBL_JOB_REQUEST>();
            TBL_JOB_REQUEST_MESSAGE = new HashSet<TBL_JOB_REQUEST_MESSAGE>();
            TBL_NOTIFICATION_LOG = new HashSet<TBL_NOTIFICATION_LOG>();
            TBL_OVERRIDE_DETAIL = new HashSet<TBL_OVERRIDE_DETAIL>();
            TBL_PROFILE_USER = new HashSet<TBL_PROFILE_USER>();
            TBL_CALL_MEMO = new HashSet<TBL_CALL_MEMO>();
            TBL_LOAN_APPLICATION_ARCHIVE = new HashSet<TBL_LOAN_APPLICATION_ARCHIVE>();
            TBL_LOAN_APPLICATION_ARCHIVE1 = new HashSet<TBL_LOAN_APPLICATION_ARCHIVE>();
            TBL_LOAN_APPLICATION = new HashSet<TBL_LOAN_APPLICATION>();
            TBL_LOAN_APPLICATION1 = new HashSet<TBL_LOAN_APPLICATION>();
            TBL_LOAN_ARCHIVE = new HashSet<TBL_LOAN_ARCHIVE>();
            TBL_LOAN_ARCHIVE1 = new HashSet<TBL_LOAN_ARCHIVE>();
            TBL_LOAN_BOOKING_REQUEST = new HashSet<TBL_LOAN_BOOKING_REQUEST>();
            TBL_LOAN_CONTINGENT = new HashSet<TBL_LOAN_CONTINGENT>();
            TBL_LOAN_CONTINGENT1 = new HashSet<TBL_LOAN_CONTINGENT>();
            TBL_LOAN_HISTORY = new HashSet<TBL_LOAN_HISTORY>();
            TBL_LOAN_HISTORY1 = new HashSet<TBL_LOAN_HISTORY>();
            TBL_LOAN = new HashSet<TBL_LOAN>();
            TBL_LOAN1 = new HashSet<TBL_LOAN>();
            TBL_LOAN_PRELIMINARY_EVALUATN = new HashSet<TBL_LOAN_PRELIMINARY_EVALUATN>();
            TBL_LOAN_PRELIMINARY_EVALUATN1 = new HashSet<TBL_LOAN_PRELIMINARY_EVALUATN>();
            TBL_LOAN_RELATIONSHIP_OFF_HIST = new HashSet<TBL_LOAN_RELATIONSHIP_OFF_HIST>();
            TBL_LOAN_REVOLVING_ARCHIVE = new HashSet<TBL_LOAN_REVOLVING_ARCHIVE>();
            TBL_LOAN_REVOLVING_ARCHIVE1 = new HashSet<TBL_LOAN_REVOLVING_ARCHIVE>();
            TBL_LOAN_REVOLVING_HISTORY = new HashSet<TBL_LOAN_REVOLVING_HISTORY>();
            TBL_LOAN_REVOLVING_HISTORY1 = new HashSet<TBL_LOAN_REVOLVING_HISTORY>();
            TBL_LOAN_REVOLVING = new HashSet<TBL_LOAN_REVOLVING>();
            TBL_LOAN_REVOLVING1 = new HashSet<TBL_LOAN_REVOLVING>();
            TBL_STAFF_ACCOUNT_HISTORY = new HashSet<TBL_STAFF_ACCOUNT_HISTORY>();
            TBL_STAFF_ACCOUNT_HISTORY1 = new HashSet<TBL_STAFF_ACCOUNT_HISTORY>();
            TBL_STAFF_EXTENDED = new HashSet<TBL_STAFF_EXTENDED>();
            TBL_STAFF_RELIEF = new HashSet<TBL_STAFF_RELIEF>();
            TBL_STAFF_RELIEF1 = new HashSet<TBL_STAFF_RELIEF>();
            TBL_STAFF1 = new HashSet<TBL_STAFF>();
            TBL_TEMP_APPROVAL_LEVEL_STAFF = new HashSet<TBL_TEMP_APPROVAL_LEVEL_STAFF>();
            TBL_TEMP_LOAN = new HashSet<TBL_TEMP_LOAN>();
            TBL_TEMP_LOAN1 = new HashSet<TBL_TEMP_LOAN>();
            TBL_TEMP_STAFF = new HashSet<TBL_TEMP_STAFF>();
        }

        [Key]
        public int STAFFID { get; set; }

        [Required]
        //[StringLength(50)]
        public string STAFFCODE { get; set; }

        public int COMPANYID { get; set; }

        public int? SUPERVISOR_STAFFID { get; set; }

        [Required]
        //[StringLength(50)]
        public string FIRSTNAME { get; set; }

        [Required]
        //[StringLength(50)]
        public string LASTNAME { get; set; }

        //[StringLength(50)]
        public string MIDDLENAME { get; set; }

        public int JOBTITLEID { get; set; }

        public int STAFFROLEID { get; set; }

        //[StringLength(100)]
        public string PHONE { get; set; }

        //[StringLength(100)]
        public string EMAIL { get; set; }

        //[StringLength(100)]
        public string ADDRESS { get; set; }

        [Column(TypeName = "date")]
        public DateTime? DATEOFBIRTH { get; set; }

        //[StringLength(1)]
        public string GENDER { get; set; }

        //[StringLength(100)]
        public string NAMEOFNOK { get; set; }

        //[StringLength(100)]
        public string PHONEOFNOK { get; set; }

        //[StringLength(100)]
        public string EMAILOFNOK { get; set; }

        //[StringLength(100)]
        public string ADDRESSOFNOK { get; set; }

        //[StringLength(1)]
        public string GENDEROFNOK { get; set; }

        //[StringLength(100)]
        public string NOKRELATIONSHIP { get; set; }

        //[StringLength(100)]
        public string COMMENT { get; set; }

        public short BRANCHID { get; set; }

        public int? MISINFOID { get; set; }

        public short? DEPARTMENTUNITID { get; set; }

        public int? STATEID { get; set; }

        public int? CITYID { get; set; }

        public short CUSTOMERSENSITIVITYLEVELID { get; set; }

        public bool NPL_LIMITEXCEEDED { get; set; }

        [Column(TypeName = "money")]
        public decimal? NPL_LIMIT { get; set; }

        [Column(TypeName = "money")]
        public decimal? LOAN_LIMIT { get; set; }

        public int? CREATEDBY { get; set; }

        public int? LASTUPDATEDBY { get; set; }

        public DateTime? DATETIMECREATED { get; set; }

        public DateTime? DATETIMEUPDATED { get; set; }

        public bool DELETED { get; set; }

        public int? DELETEDBY { get; set; }

        public DateTime? DATETIMEDELETED { get; set; }

        public decimal? WORKSTARTDURATION { get; set; }

        public decimal? WORKENDDURATION { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<TBL_APPROVAL_LEVEL_STAFF> TBL_APPROVAL_LEVEL_STAFF { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<TBL_APPROVAL_TRAIL> TBL_APPROVAL_TRAIL { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<TBL_APPROVAL_TRAIL> TBL_APPROVAL_TRAIL1 { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<TBL_APPROVAL_TRAIL> TBL_APPROVAL_TRAIL2 { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<TBL_APPROVAL_TRAIL> TBL_APPROVAL_TRAIL3 { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<TBL_AUDIT> TBL_AUDIT { get; set; }

        public virtual TBL_BRANCH TBL_BRANCH { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<TBL_BRANCH_REGION> TBL_BRANCH_REGION { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<TBL_CASA> TBL_CASA { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<TBL_CASA> TBL_CASA1 { get; set; }

        public virtual TBL_CITY TBL_CITY { get; set; }

        public virtual TBL_COMPANY TBL_COMPANY { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<TBL_CUSTOMER> TBL_CUSTOMER { get; set; }

        public virtual TBL_CUSTOMER_SENSITIVITY_LEVEL TBL_CUSTOMER_SENSITIVITY_LEVEL { get; set; }

        public virtual TBL_DEPARTMENT_UNIT TBL_DEPARTMENT_UNIT { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<TBL_FINANCE_TRANSACTION> TBL_FINANCE_TRANSACTION { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<TBL_FINANCE_TRANSACTION> TBL_FINANCE_TRANSACTION1 { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<TBL_JOB_REQUEST> TBL_JOB_REQUEST { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<TBL_JOB_REQUEST> TBL_JOB_REQUEST1 { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<TBL_JOB_REQUEST> TBL_JOB_REQUEST2 { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<TBL_JOB_REQUEST_MESSAGE> TBL_JOB_REQUEST_MESSAGE { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<TBL_NOTIFICATION_LOG> TBL_NOTIFICATION_LOG { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<TBL_OVERRIDE_DETAIL> TBL_OVERRIDE_DETAIL { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<TBL_PROFILE_USER> TBL_PROFILE_USER { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<TBL_CALL_MEMO> TBL_CALL_MEMO { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<TBL_LOAN_APPLICATION_ARCHIVE> TBL_LOAN_APPLICATION_ARCHIVE { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<TBL_LOAN_APPLICATION_ARCHIVE> TBL_LOAN_APPLICATION_ARCHIVE1 { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<TBL_LOAN_APPLICATION> TBL_LOAN_APPLICATION { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<TBL_LOAN_APPLICATION> TBL_LOAN_APPLICATION1 { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<TBL_LOAN_ARCHIVE> TBL_LOAN_ARCHIVE { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<TBL_LOAN_ARCHIVE> TBL_LOAN_ARCHIVE1 { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<TBL_LOAN_BOOKING_REQUEST> TBL_LOAN_BOOKING_REQUEST { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<TBL_LOAN_CONTINGENT> TBL_LOAN_CONTINGENT { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<TBL_LOAN_CONTINGENT> TBL_LOAN_CONTINGENT1 { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<TBL_LOAN_HISTORY> TBL_LOAN_HISTORY { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<TBL_LOAN_HISTORY> TBL_LOAN_HISTORY1 { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<TBL_LOAN> TBL_LOAN { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<TBL_LOAN> TBL_LOAN1 { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<TBL_LOAN_PRELIMINARY_EVALUATN> TBL_LOAN_PRELIMINARY_EVALUATN { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<TBL_LOAN_PRELIMINARY_EVALUATN> TBL_LOAN_PRELIMINARY_EVALUATN1 { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<TBL_LOAN_RELATIONSHIP_OFF_HIST> TBL_LOAN_RELATIONSHIP_OFF_HIST { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<TBL_LOAN_REVOLVING_ARCHIVE> TBL_LOAN_REVOLVING_ARCHIVE { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<TBL_LOAN_REVOLVING_ARCHIVE> TBL_LOAN_REVOLVING_ARCHIVE1 { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<TBL_LOAN_REVOLVING_HISTORY> TBL_LOAN_REVOLVING_HISTORY { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<TBL_LOAN_REVOLVING_HISTORY> TBL_LOAN_REVOLVING_HISTORY1 { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<TBL_LOAN_REVOLVING> TBL_LOAN_REVOLVING { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<TBL_LOAN_REVOLVING> TBL_LOAN_REVOLVING1 { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<TBL_STAFF_ACCOUNT_HISTORY> TBL_STAFF_ACCOUNT_HISTORY { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<TBL_STAFF_ACCOUNT_HISTORY> TBL_STAFF_ACCOUNT_HISTORY1 { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<TBL_STAFF_EXTENDED> TBL_STAFF_EXTENDED { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<TBL_STAFF_RELIEF> TBL_STAFF_RELIEF { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<TBL_STAFF_RELIEF> TBL_STAFF_RELIEF1 { get; set; }

        public virtual TBL_STAFF_JOBTITLE TBL_STAFF_JOBTITLE { get; set; }

        public virtual TBL_STAFF_ROLE TBL_STAFF_ROLE { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<TBL_STAFF> TBL_STAFF1 { get; set; }

        public virtual TBL_STAFF TBL_STAFF2 { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<TBL_TEMP_APPROVAL_LEVEL_STAFF> TBL_TEMP_APPROVAL_LEVEL_STAFF { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<TBL_TEMP_LOAN> TBL_TEMP_LOAN { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<TBL_TEMP_LOAN> TBL_TEMP_LOAN1 { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<TBL_TEMP_STAFF> TBL_TEMP_STAFF { get; set; }
    }
}
