namespace FintrakBanking.Entities.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("FINTRAKBANKING.TBL_APPROVAL_GROUP")]
    public partial class TBL_APPROVAL_GROUP
    {
        public TBL_APPROVAL_GROUP()
        {
            TBL_APPROVAL_GROUP_MAPPING = new HashSet<TBL_APPROVAL_GROUP_MAPPING>();
            TBL_APPROVAL_LEVEL = new HashSet<TBL_APPROVAL_LEVEL>();
        }

        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public short GROUPID { get; set; }

        [Required]
        [StringLength(150)]
        public string GROUPNAME { get; set; }

        public int COMPANYID { get; set; }

        public int ROLEID { get; set; }

        public int ISCOMMITTEE { get; set; }

        public int CREATEDBY { get; set; }

        public DateTime DATETIMECREATED { get; set; }

        public int? LASTUPDATEDBY { get; set; }

        public DateTime? DATETIMEUPDATED { get; set; }

        public bool DELETED { get; set; }

        public int? DELETEDBY { get; set; }

        public DateTime? DATETIMEDELETED { get; set; }

        public virtual ICollection<TBL_APPROVAL_GROUP_MAPPING> TBL_APPROVAL_GROUP_MAPPING { get; set; }

        public virtual TBL_COMPANY TBL_COMPANY { get; set; }

        public virtual ICollection<TBL_APPROVAL_LEVEL> TBL_APPROVAL_LEVEL { get; set; }
    }
}
