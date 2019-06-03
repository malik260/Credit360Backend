namespace FintrakBanking.Entities.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("TBL_CORR_RATING_INDEX_DETAIL")]
    public partial class TBL_CORR_RATING_INDEX_DETAIL
    {
        public TBL_CORR_RATING_INDEX_DETAIL()
        {
            TBL_CORR_RATING_METRIC_DETAIL = new HashSet<TBL_CORR_RATING_METRIC_DETAIL>();
        }

        [Key]
        public int RATINGINDEXDETAILID { get; set; }

        public int OFFICERRATINGID { get; set; }

        public int RATINGINDEXSETUPID { get; set; }

        public int SCORE { get; set; }

        public virtual TBL_CORR_OFFICER_RATING TBL_CORR_OFFICER_RATING { get; set; }

        public virtual ICollection<TBL_CORR_RATING_METRIC_DETAIL> TBL_CORR_RATING_METRIC_DETAIL { get; set; }
        public int PERCENTAGEWEIGHT { get; set; }
    }
}
/*

public virtual DbSet<TBL_CORR_RATING_INDEX_DETAIL> TBL_CORR_RATING_INDEX_DETAIL { get; set; }


*/
