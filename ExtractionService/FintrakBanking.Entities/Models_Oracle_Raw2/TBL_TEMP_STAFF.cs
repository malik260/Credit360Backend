namespace FintrakBanking.Entities.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("FINTRAKBANKING.TBL_TEMP_STAFF")]
    public partial class TBL_TEMP_STAFF
    {
        public TBL_TEMP_STAFF()
        {
            TBL_TEMP_PROFILE_USER = new HashSet<TBL_TEMP_PROFILE_USER>();
        }

        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public int TEMPSTAFFID { get; set; }

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

        [StringLength(100)]
        public string COMMENT_ { get; set; }

        public byte[] STAFFSIGNATURE { get; set; }

        public int? BRANCHID { get; set; }

        public int? MISINFOID { get; set; }

        public int? DEPARTMENTUNITID { get; set; }

        public int? STATEID { get; set; }

        public int? CITYID { get; set; }

        public int CUSTOMERSENSITIVITYLEVELID { get; set; }

        public int? CREATEDBY { get; set; }

        public int? LASTUPDATEDBY { get; set; }

        public DateTime? DATETIMECREATED { get; set; }

        public DateTime? DATETIMEUPDATED { get; set; }

        public int ISCURRENT { get; set; }

        public int APPROVALSTATUSID { get; set; }

        public DateTime? DATEOFBIRTH { get; set; }

        public virtual TBL_APPROVAL_STATUS TBL_APPROVAL_STATUS { get; set; }

        public virtual TBL_BRANCH TBL_BRANCH { get; set; }

        public virtual TBL_CITY TBL_CITY { get; set; }

        public virtual TBL_COMPANY TBL_COMPANY { get; set; }

        public virtual TBL_DEPARTMENT_UNIT TBL_DEPARTMENT_UNIT { get; set; }

        public virtual TBL_MIS_INFO TBL_MIS_INFO { get; set; }

        public virtual TBL_STAFF TBL_STAFF { get; set; }

        public virtual TBL_STAFF_JOBTITLE TBL_STAFF_JOBTITLE { get; set; }

        public virtual TBL_STAFF_ROLE TBL_STAFF_ROLE { get; set; }

        public virtual TBL_STATE TBL_STATE { get; set; }

        public virtual ICollection<TBL_TEMP_PROFILE_USER> TBL_TEMP_PROFILE_USER { get; set; }
    }
}
