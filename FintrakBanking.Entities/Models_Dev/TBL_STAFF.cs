namespace FintrakBanking.Entities.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("FINTRAKBANKING.TBL_STAFF")]
    public partial class TBL_STAFF
    {
        public TBL_STAFF()
        {
            TBL_APPROVAL_LEVEL_STAFF = new HashSet<TBL_APPROVAL_LEVEL_STAFF>();
            TBL_APPROVAL_TRAIL = new HashSet<TBL_APPROVAL_TRAIL>();
            TBL_AUDIT = new HashSet<TBL_AUDIT>();
            TBL_BRANCH_REGION = new HashSet<TBL_BRANCH_REGION>();
            TBL_CALL_MEMO = new HashSet<TBL_CALL_MEMO>();
            TBL_CASA = new HashSet<TBL_CASA>();
            TBL_CUSTOMER = new HashSet<TBL_CUSTOMER>();
            TBL_FINANCE_TRANSACTION = new HashSet<TBL_FINANCE_TRANSACTION>();
            TBL_JOB_REQUEST = new HashSet<TBL_JOB_REQUEST>();
            TBL_JOB_REQUEST_MESSAGE = new HashSet<TBL_JOB_REQUEST_MESSAGE>();
            TBL_LOAN = new HashSet<TBL_LOAN>();
            TBL_LOAN_APPLICATION = new HashSet<TBL_LOAN_APPLICATION>();
            TBL_LOAN_APPLICATION_ARCHIVE = new HashSet<TBL_LOAN_APPLICATION_ARCHIVE>();
            TBL_LOAN_ARCHIVE = new HashSet<TBL_LOAN_ARCHIVE>();
            TBL_LOAN_BOOKING_REQUEST = new HashSet<TBL_LOAN_BOOKING_REQUEST>();
            TBL_LOAN_CONTINGENT = new HashSet<TBL_LOAN_CONTINGENT>();
            TBL_LOAN_PRELIMINARY_EVALUATN = new HashSet<TBL_LOAN_PRELIMINARY_EVALUATN>();
            TBL_LOAN_RELATIONSHIP_OFF_HIST = new HashSet<TBL_LOAN_RELATIONSHIP_OFF_HIST>();
            TBL_LOAN_REVOLVING = new HashSet<TBL_LOAN_REVOLVING>();
            TBL_LOAN_REVOLVING_ARCHIVE = new HashSet<TBL_LOAN_REVOLVING_ARCHIVE>();
            TBL_NOTIFICATION_LOG = new HashSet<TBL_NOTIFICATION_LOG>();
            TBL_OVERRIDE_DETAIL = new HashSet<TBL_OVERRIDE_DETAIL>();
            TBL_PROFILE_USER = new HashSet<TBL_PROFILE_USER>();
            TBL_STAFF_ACCOUNT_HISTORY = new HashSet<TBL_STAFF_ACCOUNT_HISTORY>();
            TBL_STAFF_RELIEF = new HashSet<TBL_STAFF_RELIEF>();
            TBL_STAFF1 = new HashSet<TBL_STAFF>();
            TBL_TEMP_LOAN = new HashSet<TBL_TEMP_LOAN>();
            TBL_TEMP_STAFF = new HashSet<TBL_TEMP_STAFF>();
        }

        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public int STAFFID { get; set; }

        [Required]
        [StringLength(50)]
        public string STAFFCODE { get; set; }

        public int COMPANYID { get; set; }

        public int? SUPERVISOR_STAFFID { get; set; }

        [Required]
        [StringLength(50)]
        public string FIRSTNAME { get; set; }

        [Required]
        [StringLength(50)]
        public string LASTNAME { get; set; }

        [StringLength(50)]
        public string MIDDLENAME { get; set; }

        public int JOBTITLEID { get; set; }

        public int STAFFROLEID { get; set; }

        [StringLength(100)]
        public string PHONE { get; set; }

        [StringLength(100)]
        public string EMAIL { get; set; }

        [StringLength(100)]
        public string ADDRESS { get; set; }

        [StringLength(1)]
        public string GENDER { get; set; }

        [StringLength(100)]
        public string NAMEOFNOK { get; set; }

        [StringLength(100)]
        public string PHONEOFNOK { get; set; }

        [StringLength(100)]
        public string EMAILOFNOK { get; set; }

        [StringLength(100)]
        public string ADDRESSOFNOK { get; set; }

        [StringLength(1)]
        public string GENDEROFNOK { get; set; }

        [StringLength(100)]
        public string NOKRELATIONSHIP { get; set; }

        [StringLength(100), Column(name: "COMMENT_")]
        public string COMMENT { get; set; }

        public short? BRANCHID { get; set; }

        public int? MISINFOID { get; set; }

        public short? DEPARTMENTUNITID { get; set; }

        public int? STATEID { get; set; }

        public int? CITYID { get; set; }

        public short CUSTOMERSENSITIVITYLEVELID { get; set; }

        public bool NPL_LIMITEXCEEDED { get; set; }

        public decimal? NPL_LIMIT { get; set; }

        public decimal? LOAN_LIMIT { get; set; }

        public int? CREATEDBY { get; set; }

        public int? LASTUPDATEDBY { get; set; }

        public DateTime? DATETIMECREATED { get; set; }

        public DateTime? DATETIMEUPDATED { get; set; }

        public bool DELETED { get; set; }

        public int? DELETEDBY { get; set; }

        public DateTime? DATETIMEDELETED { get; set; }

        public DateTime? DATEOFBIRTH { get; set; }

        public virtual ICollection<TBL_APPROVAL_LEVEL_STAFF> TBL_APPROVAL_LEVEL_STAFF { get; set; }

        public virtual ICollection<TBL_APPROVAL_TRAIL> TBL_APPROVAL_TRAIL { get; set; }

        public virtual ICollection<TBL_AUDIT> TBL_AUDIT { get; set; }

        public virtual ICollection<TBL_BRANCH_REGION> TBL_BRANCH_REGION { get; set; }

        public virtual ICollection<TBL_CALL_MEMO> TBL_CALL_MEMO { get; set; }

        public virtual ICollection<TBL_CASA> TBL_CASA { get; set; }

        public virtual TBL_CITY TBL_CITY { get; set; }

        public virtual TBL_COMPANY TBL_COMPANY { get; set; }

        public virtual ICollection<TBL_CUSTOMER> TBL_CUSTOMER { get; set; }

        public virtual TBL_CUSTOMER_SENSITIVITY_LEVEL TBL_CUSTOMER_SENSITIVITY_LEVEL { get; set; }

        public virtual TBL_DEPARTMENT_UNIT TBL_DEPARTMENT_UNIT { get; set; }

        public virtual ICollection<TBL_FINANCE_TRANSACTION> TBL_FINANCE_TRANSACTION { get; set; }

        public virtual ICollection<TBL_JOB_REQUEST> TBL_JOB_REQUEST { get; set; }

        public virtual ICollection<TBL_JOB_REQUEST_MESSAGE> TBL_JOB_REQUEST_MESSAGE { get; set; }

        public virtual ICollection<TBL_LOAN> TBL_LOAN { get; set; }

        public virtual ICollection<TBL_LOAN_APPLICATION> TBL_LOAN_APPLICATION { get; set; }

        public virtual ICollection<TBL_LOAN_APPLICATION_ARCHIVE> TBL_LOAN_APPLICATION_ARCHIVE { get; set; }

        public virtual ICollection<TBL_LOAN_ARCHIVE> TBL_LOAN_ARCHIVE { get; set; }

        public virtual ICollection<TBL_LOAN_BOOKING_REQUEST> TBL_LOAN_BOOKING_REQUEST { get; set; }

        public virtual ICollection<TBL_LOAN_CONTINGENT> TBL_LOAN_CONTINGENT { get; set; }

        public virtual ICollection<TBL_LOAN_PRELIMINARY_EVALUATN> TBL_LOAN_PRELIMINARY_EVALUATN { get; set; }

        public virtual ICollection<TBL_LOAN_RELATIONSHIP_OFF_HIST> TBL_LOAN_RELATIONSHIP_OFF_HIST { get; set; }

        public virtual ICollection<TBL_LOAN_REVOLVING> TBL_LOAN_REVOLVING { get; set; }

        public virtual ICollection<TBL_LOAN_REVOLVING_ARCHIVE> TBL_LOAN_REVOLVING_ARCHIVE { get; set; }

        public virtual ICollection<TBL_NOTIFICATION_LOG> TBL_NOTIFICATION_LOG { get; set; }

        public virtual ICollection<TBL_OVERRIDE_DETAIL> TBL_OVERRIDE_DETAIL { get; set; }

        public virtual ICollection<TBL_PROFILE_USER> TBL_PROFILE_USER { get; set; }

        public virtual ICollection<TBL_STAFF_ACCOUNT_HISTORY> TBL_STAFF_ACCOUNT_HISTORY { get; set; }

        public virtual ICollection<TBL_STAFF_RELIEF> TBL_STAFF_RELIEF { get; set; }

        public virtual TBL_STAFF_JOBTITLE TBL_STAFF_JOBTITLE { get; set; }

        public virtual TBL_STAFF_ROLE TBL_STAFF_ROLE { get; set; }

        public virtual ICollection<TBL_STAFF> TBL_STAFF1 { get; set; }

        public virtual TBL_STAFF TBL_STAFF2 { get; set; }

        public virtual ICollection<TBL_TEMP_LOAN> TBL_TEMP_LOAN { get; set; }

        public virtual ICollection<TBL_TEMP_STAFF> TBL_TEMP_STAFF { get; set; }
    }
}
