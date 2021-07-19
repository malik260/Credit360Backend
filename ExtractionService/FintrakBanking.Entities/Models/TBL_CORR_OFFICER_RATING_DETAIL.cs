namespace FintrakBanking.Entities.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("TBL_CORR_OFFICER_RATING_DETAIL")]
    public partial class TBL_CORR_OFFICER_RATING_DETAIL
    {
        [Key]
        public int OFFICERRATINGDETAILID { get; set; }

        public int OFFICERRATINGID { get; set; }

        public int ASSESSMENTPARAMETERID { get; set; }

        public int SCORE { get; set; }

        public virtual TBL_CORR_OFFICER_RATING TBL_CORR_OFFICER_RATING { get; set; }

    }
}
/*

public virtual DbSet<TBL_CORR_OFFICER_RATING_DETAIL> TBL_CORR_OFFICER_RATING_DETAIL { get; set; }

    modelBuilder.Entity<TBL_CORR_OFFICER_RATING>()
        .HasMany(e => e.TBL_CORR_OFFICER_RATING_DETAIL)
        .WithRequired(e => e.TBL_CORR_OFFICER_RATING)
        .WillCascadeOnDelete(false);

*/
