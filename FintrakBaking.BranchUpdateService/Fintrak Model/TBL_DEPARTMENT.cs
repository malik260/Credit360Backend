namespace FintrakBaking.BranchUpdateService.Fintrak_Model
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("core.TBL_DEPARTMENT")]
    public partial class TBL_DEPARTMENT
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public TBL_DEPARTMENT()
        {
            TBL_DEPARTMENT_UNIT = new HashSet<TBL_DEPARTMENT_UNIT>();
        }

        [Key]
        public short DEPARTMENTID { get; set; }

        public short? BRANCHID { get; set; }

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

        public bool? DELETED { get; set; }

        public virtual TBL_BRANCH TBL_BRANCH { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<TBL_DEPARTMENT_UNIT> TBL_DEPARTMENT_UNIT { get; set; }
    }
}
