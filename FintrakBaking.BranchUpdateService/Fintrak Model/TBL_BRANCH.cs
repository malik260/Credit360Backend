namespace FintrakBaking.BranchUpdateService.Fintrak_Model
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("core.TBL_BRANCH")]
    public partial class TBL_BRANCH
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public TBL_BRANCH()
        {
            TBL_DEPARTMENT = new HashSet<TBL_DEPARTMENT>();
        }

        [Key]
        public short BRANCHID { get; set; }

        public int COMPANYID { get; set; }

        [Required]
        [StringLength(250)]
        public string BRANCHNAME { get; set; }

        [Required]
        [StringLength(50)]
        public string BRANCHCODE { get; set; }

        public int REGIONID { get; set; }

        [StringLength(255)]
        public string ADDRESSLINE1 { get; set; }

        [StringLength(255)]
        public string ADDRESSLINE2 { get; set; }

        [StringLength(2000)]
        public string COMMENT { get; set; }

        public int? STATEID { get; set; }

        public int? CITYID { get; set; }

        public bool NPL_LIMITEXCEEDED { get; set; }

        [Column(TypeName = "money")]
        public decimal NPL_LIMIT { get; set; }

        public bool DELETED { get; set; }

        public int? DELETEDBY { get; set; }

        public DateTime? DATETIMEDELETED { get; set; }

        public int? CREATEDBY { get; set; }

        public int? LASTUPDATEDBY { get; set; }

        public DateTime? DATETIMECREATED { get; set; }

        public DateTime? DATETIMEUPDATED { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<TBL_DEPARTMENT> TBL_DEPARTMENT { get; set; }

        public virtual TBL_BRANCH_REGION TBL_BRANCH_REGION { get; set; }

        public virtual TBL_CITY TBL_CITY { get; set; }

        public virtual TBL_STATE TBL_STATE { get; set; }
    }
}
