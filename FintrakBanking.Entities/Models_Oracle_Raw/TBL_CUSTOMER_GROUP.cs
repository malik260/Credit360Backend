namespace FintrakBanking.Entities.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("FINTRAKBANKING.TBL_CUSTOMER_GROUP")]
    public partial class TBL_CUSTOMER_GROUP
    {
        public TBL_CUSTOMER_GROUP()
        {
            TBL_CUSTOMER_GROUP_MAPPING = new HashSet<TBL_CUSTOMER_GROUP_MAPPING>();
            TBL_CUSTOMER_GRP_FS_CAPTN_DET = new HashSet<TBL_CUSTOMER_GRP_FS_CAPTN_DET>();
            TBL_LOAN_APPLICATION_ARCHIVE = new HashSet<TBL_LOAN_APPLICATION_ARCHIVE>();
            TBL_LOAN_APPLICATION = new HashSet<TBL_LOAN_APPLICATION>();
            TBL_LOAN_PRELIMINARY_EVALUATN = new HashSet<TBL_LOAN_PRELIMINARY_EVALUATN>();
        }

        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public int CUSTOMERGROUPID { get; set; }

        [Required]
        [StringLength(50)]
        public string GROUPNAME { get; set; }

        [Required]
        [StringLength(10)]
        public string GROUPCODE { get; set; }

        [StringLength(1000)]
        public string GROUPDESCRIPTION { get; set; }

        public int CREATEDBY { get; set; }

        public int? LASTUPDATEDBY { get; set; }

        public DateTime DATETIMECREATED { get; set; }

        public DateTime? DATETIMEUPDATED { get; set; }

        public int DELETED { get; set; }

        public int? DELETEDBY { get; set; }

        public DateTime? DATETIMEDELETED { get; set; }

        public virtual ICollection<TBL_CUSTOMER_GROUP_MAPPING> TBL_CUSTOMER_GROUP_MAPPING { get; set; }

        public virtual ICollection<TBL_CUSTOMER_GRP_FS_CAPTN_DET> TBL_CUSTOMER_GRP_FS_CAPTN_DET { get; set; }

        public virtual ICollection<TBL_LOAN_APPLICATION_ARCHIVE> TBL_LOAN_APPLICATION_ARCHIVE { get; set; }

        public virtual ICollection<TBL_LOAN_APPLICATION> TBL_LOAN_APPLICATION { get; set; }

        public virtual ICollection<TBL_LOAN_PRELIMINARY_EVALUATN> TBL_LOAN_PRELIMINARY_EVALUATN { get; set; }
    }
}
