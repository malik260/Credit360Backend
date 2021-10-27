namespace FintrakBanking.Entities.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("FINTRAKBANKING.TBL_DEPARTMENT_UNIT")]
    public partial class TBL_DEPARTMENT_UNIT
    {
        public TBL_DEPARTMENT_UNIT()
        {
            TBL_JOB_REQUEST = new HashSet<TBL_JOB_REQUEST>();
            TBL_STAFF = new HashSet<TBL_STAFF>();
            TBL_TEMP_STAFF = new HashSet<TBL_TEMP_STAFF>();
        }

        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public int DEPARTMENTUNITID { get; set; }

        [Required]
        [StringLength(100)]
        public string DEPARTMENTUNITNAME { get; set; }

        public int DEPARTMENTID { get; set; }

        [StringLength(100)]
        public string EMAIL { get; set; }

        public virtual TBL_DEPARTMENT TBL_DEPARTMENT { get; set; }

        public virtual ICollection<TBL_JOB_REQUEST> TBL_JOB_REQUEST { get; set; }

        public virtual ICollection<TBL_STAFF> TBL_STAFF { get; set; }

        public virtual ICollection<TBL_TEMP_STAFF> TBL_TEMP_STAFF { get; set; }
    }
}
