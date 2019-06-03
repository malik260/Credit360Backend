namespace FintrakBanking.Entities.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("TBL_CORR_RATING_INDEX_SETUP")]
    public partial class TBL_CORR_RATING_INDEX_SETUP
    {
        //public TBL_CORR_RATING_INDEX_SETUP()
        //{
        //    TBL_CORR_RATING_METRIC_SETUP = new HashSet<TBL_CORR_RATING_METRIC_SETUP>();
        //}

        [Key]
        public int RATINGINDEXSETUPID { get; set; }

        [Required]
        [StringLength(200)]
        public string NAME { get; set; }

        public int PERCENTAGEWEIGHT { get; set; }

        public int KEYINDICATORID { get; set; }

        public bool ISACTIVE { get; set; }

        public int? DEFINEDFUNCTIONID { get; set; }

        //public virtual TBL_CORR_KEY_INDICATOR TBL_CORR_KEY_INDICATOR { get; set; }

        //public virtual ICollection<TBL_CORR_RATING_METRIC_SETUP> TBL_CORR_RATING_METRIC_SETUP { get; set; }
    }
}
        /*

        public virtual DbSet<TBL_CORR_RATING_INDEX_SETUP> TBL_CORR_RATING_INDEX_SETUP { get; set; }

            modelBuilder.Entity<TBL_CORR_KEY_INDICATOR>()
                .HasMany(e => e.TBL_CORR_RATING_INDEX_SETUP)
                .WithRequired(e => e.TBL_CORR_KEY_INDICATOR)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<TBL_CORR_RATING_INDEX_SETUP>()
                .HasMany(e => e.TBL_CORR_RATING_METRIC_SETUP)
                .WithRequired(e => e.TBL_CORR_RATING_INDEX_SETUP)
                .WillCascadeOnDelete(false);

        */
