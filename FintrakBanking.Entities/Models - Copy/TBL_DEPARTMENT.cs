namespace FintrakBanking.Entities.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("FINTRAKBANKING.TBL_DEPARTMENT")]
    public partial class TBL_DEPARTMENT
    {
        public TBL_DEPARTMENT()
        {
            TBL_DEPARTMENT_UNIT = new HashSet<TBL_DEPARTMENT_UNIT>();
            TBL_JOB_REQUEST = new HashSet<TBL_JOB_REQUEST>();
            TBL_JOB_TYPE_DEPARTMENT = new HashSet<TBL_JOB_TYPE_DEPARTMENT>();
        }

        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public int DEPARTMENTID { get; set; }

        public int COMPANYID { get; set; }

        [Required]
        [StringLength(50)]
        public string DEPARTMENTCODE { get; set; }

        [StringLength(50)]
        public string DEPARTMENTNAME { get; set; }

        [StringLength(250)]
        public string DESCRIPTION { get; set; }

        public int? CREATEDBY { get; set; }

        public int? LASTUPDATEDBY { get; set; }

        public DateTime? DATETIMECREATED { get; set; }

        public DateTime? DATETIMEUPDATED { get; set; }

        public int? DELETED { get; set; }

        public virtual ICollection<TBL_DEPARTMENT_UNIT> TBL_DEPARTMENT_UNIT { get; set; }

        public virtual ICollection<TBL_JOB_REQUEST> TBL_JOB_REQUEST { get; set; }

        public virtual ICollection<TBL_JOB_TYPE_DEPARTMENT> TBL_JOB_TYPE_DEPARTMENT { get; set; }
    }
}
