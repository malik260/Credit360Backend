namespace FintrakBaking.BranchUpdateService.Fintrak_Model
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
            TBL_STAFF1 = new HashSet<TBL_STAFF>();
            TBL_STAFF11 = new HashSet<TBL_STAFF>();
        }

        [Key]
        public int STAFFID { get; set; }

        [Required]
        [StringLength(50)]
        public string STAFFCODE { get; set; }

        public int COMPANYID { get; set; }

        public int? SUPERVISOR_STAFFID { get; set; }

        public int? RELIEF_STAFFID { get; set; }

        [Required]
        [StringLength(50)]
        public string FIRSTNAME { get; set; }

        [Required]
        [StringLength(50)]
        public string LASTNAME { get; set; }

        [StringLength(50)]
        public string MIDDLENAME { get; set; }

        public int JOBTITLEID { get; set; }

        public int RANKID { get; set; }

        [StringLength(100)]
        public string PHONE { get; set; }

        [StringLength(100)]
        public string EMAIL { get; set; }

        [StringLength(100)]
        public string ADDRESS { get; set; }

        [Column(TypeName = "date")]
        public DateTime? DATEOFBIRTH { get; set; }

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
        public string COMMENT { get; set; }

        public short? BRANCHID { get; set; }

        public int? MISINFOID { get; set; }

        public short? DEPARTMENTID { get; set; }

        public short? DEPARTMENT_UNITID { get; set; }

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

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<TBL_APPROVAL_LEVEL_STAFF> TBL_APPROVAL_LEVEL_STAFF { get; set; }

        public virtual TBL_CITY TBL_CITY { get; set; }

        public virtual TBL_STAFF_JOBTITLE TBL_STAFF_JOBTITLE { get; set; }

        public virtual TBL_STAFF_RANK TBL_STAFF_RANK { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<TBL_STAFF> TBL_STAFF1 { get; set; }

        public virtual TBL_STAFF TBL_STAFF2 { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<TBL_STAFF> TBL_STAFF11 { get; set; }

        public virtual TBL_STAFF TBL_STAFF3 { get; set; }
    }
}
