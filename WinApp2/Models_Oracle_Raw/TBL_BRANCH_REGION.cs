namespace FintrakBanking.Entities.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("FINTRAKBANKING.TBL_BRANCH_REGION")]
    public partial class TBL_BRANCH_REGION
    {
        public TBL_BRANCH_REGION()
        {
            TBL_BRANCH = new HashSet<TBL_BRANCH>();
        }

        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public int REGIONID { get; set; }

        [Required]
        [StringLength(200)]
        public string REGION_NAME { get; set; }

        public int COMPANYID { get; set; }

        public int? CAM_HOU_STAFFID { get; set; }

        public int CREATEDBY { get; set; }

        public DateTime DATETIMECREATED { get; set; }

        public int? LASTUPDATEDBY { get; set; }

        public DateTime? DATETIMEUPDATED { get; set; }

        public int DELETED { get; set; }

        public int? DELETEDBY { get; set; }

        public DateTime? DATETIMEDELETED { get; set; }

        public virtual ICollection<TBL_BRANCH> TBL_BRANCH { get; set; }

        public virtual TBL_COMPANY TBL_COMPANY { get; set; }

        public virtual TBL_STAFF TBL_STAFF { get; set; }
    }
}
