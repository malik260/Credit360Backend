namespace FintrakBanking.Entities.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("TBL_CORR_OFFICER_RATING")]
    public partial class TBL_CORR_OFFICER_RATING
    {
        public TBL_CORR_OFFICER_RATING()
        {
            TBL_CORR_OFFICER_RATING_DETAIL = new HashSet<TBL_CORR_OFFICER_RATING_DETAIL>();
        }

        [Key]
        public int OFFICERRATINGID { get; set; }

        public int STAFFID { get; set; }

        public int RATINGPERIODID { get; set; }

        public int CORRSCORE { get; set; }

        [Required]
        //[StringLength(20)]
        public string CORRCOMMENT { get; set; }

        public int CREATEDBY { get; set; }
        public DateTime DATETIMECREATED { get; set; }
        public int? LASTUPDATEDBY { get; set; }
        public DateTime? DATETIMEUPDATED { get; set; }
        public bool DELETED { get; set; }
        public int? DELETEDBY { get; set; }
        public DateTime? DATETIMEDELETED { get; set; }

        public virtual ICollection<TBL_CORR_OFFICER_RATING_DETAIL> TBL_CORR_OFFICER_RATING_DETAIL { get; set; }

    }
}