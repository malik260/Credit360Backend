namespace FintrakBanking.Entities.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("TBL_CORR_KEY_INDICATOR")]
    public partial class TBL_CORR_KEY_INDICATOR
    {
        public TBL_CORR_KEY_INDICATOR()
        {
            TBL_CORR_ASSESSMENT_PARAMETER = new HashSet<TBL_CORR_ASSESSMENT_PARAMETER>();
        }

        [Key]
        public int KEYINDICATORID { get; set; }

        [Required]
        //[StringLength(100)]
        public string INDICATORNAME { get; set; }

        //[StringLength(1000)]
        public string DESCRIPTION { get; set; }

        public int PERCENTAGEWEIGHT { get; set; }

        public bool ISACTIVE { get; set; }

        public virtual ICollection<TBL_CORR_ASSESSMENT_PARAMETER> TBL_CORR_ASSESSMENT_PARAMETER { get; set; }

    }
}
/*

public virtual DbSet<TBL_CORR_KEY_INDICATOR> TBL_CORR_KEY_INDICATOR { get; set; }

    modelBuilder.Entity<TBL_CORR_KEY_INDICATOR>()
        .HasMany(e => e.TBL_CORR_ASSESSMENT_PARAMETER)
        .WithRequired(e => e.TBL_CORR_KEY_INDICATOR)
        .WillCascadeOnDelete(false);

*/
