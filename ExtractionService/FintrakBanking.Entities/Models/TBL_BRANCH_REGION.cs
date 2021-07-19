namespace FintrakBanking.Entities.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("TBL_BRANCH_REGION")]
    public partial class TBL_BRANCH_REGION
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public TBL_BRANCH_REGION()
        {
            TBL_BRANCH = new HashSet<TBL_BRANCH>();
            TBL_BRANCH_REGION_STAFF = new HashSet<TBL_BRANCH_REGION_STAFF>();
        }

        [Key]
        public int REGIONID { get; set; }
        public int? REGIONID2 { get; set; }

        [Required]
        //[StringLength(200)]
        public string REGION_NAME { get; set; }

        public int COMPANYID { get; set; }

        public int? CAM_HOU_STAFFID { get; set; }

        public int? REGIONTYPEID { get; set; }

        public int CREATEDBY { get; set; }

        public DateTime DATETIMECREATED { get; set; }

        public int? LASTUPDATEDBY { get; set; }

        public DateTime? DATETIMEUPDATED { get; set; }

        public bool DELETED { get; set; }

        public int? DELETEDBY { get; set; }

        public DateTime? DATETIMEDELETED { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<TBL_BRANCH> TBL_BRANCH { get; set; }
        public virtual ICollection<TBL_BRANCH_REGION_STAFF> TBL_BRANCH_REGION_STAFF { get; set; }

        public virtual TBL_COMPANY TBL_COMPANY { get; set; }

        public virtual TBL_STAFF TBL_STAFF { get; set; }
    }
}
