namespace FintrakBanking.Entities.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("TBL_CORR_RATING_METRIC_DETAIL")]
    public partial class TBL_CORR_RATING_METRIC_DETAIL
    {
        [Key]
        public int RATINGMETRICDETAILID { get; set; }

        public int RATINGINDEXDETAILID { get; set; }

        public int RATINGMETRICID { get; set; }

        public int SCORE { get; set; }

        public virtual TBL_CORR_RATING_INDEX_DETAIL TBL_CORR_RATING_INDEX_DETAIL { get; set; }

    }
}
/*

public virtual DbSet<TBL_CORR_RATING_METRIC_DETAIL> TBL_CORR_RATING_METRIC_DETAIL { get; set; }

*/
