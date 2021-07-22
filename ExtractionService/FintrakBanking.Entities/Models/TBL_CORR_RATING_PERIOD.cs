namespace FintrakBanking.Entities.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("TBL_CORR_RATING_PERIOD")]
    public partial class TBL_CORR_RATING_PERIOD
    {
        [Key]
        public int RATINGPERIODID { get; set; }

        public DateTime STARTDATE { get; set; }

        public DateTime ENDDATE { get; set; }

        public int CREATEDBY { get; set; }
        public DateTime DATETIMECREATED { get; set; }
        public int? LASTUPDATEDBY { get; set; }
        public DateTime? DATETIMEUPDATED { get; set; }
        public bool DELETED { get; set; }
        public int? DELETEDBY { get; set; }
        public DateTime? DATETIMEDELETED { get; set; }

    }
}
/*

public virtual DbSet<TBL_CORR_RATING_PERIOD> TBL_CORR_RATING_PERIOD { get; set; }

*/
