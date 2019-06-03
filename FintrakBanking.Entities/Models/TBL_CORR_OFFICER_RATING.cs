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
            TBL_CORR_RATING_INDEX_DETAIL = new HashSet<TBL_CORR_RATING_INDEX_DETAIL>();
        }

        [Key]
        public int OFFICERRATINGID { get; set; }

        public int STAFFID { get; set; }

        public int BRANCHID { get; set; }

        public int BUSINESSUNITID { get; set; }

        public int BORROWINGCUSTOMERS { get; set; }

        public decimal EXPOSURE { get; set; }

        [Required]
        [StringLength(20)]
        public string COMMENT { get; set; }

        public DateTime DATERATED { get; set; }

        public DateTime FROMDATE { get; set; }

        public DateTime TODATE { get; set; }

        public virtual ICollection<TBL_CORR_RATING_INDEX_DETAIL> TBL_CORR_RATING_INDEX_DETAIL { get; set; }

    }
}
/*

public virtual DbSet<TBL_CORR_OFFICER_RATING> TBL_CORR_OFFICER_RATING { get; set; }

    modelBuilder.Entity<TBL_CORR_OFFICER_RATING>()
        .HasMany(e => e.TBL_CORR_RATING_INDEX_DETAIL)
        .WithRequired(e => e.TBL_CORR_OFFICER_RATING)
        .WillCascadeOnDelete(false);

*/
