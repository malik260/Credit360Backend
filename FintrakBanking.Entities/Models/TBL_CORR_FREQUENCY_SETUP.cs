namespace FintrakBanking.Entities.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("TBL_CORR_FREQUENCY_SETUP")]
    public partial class TBL_CORR_FREQUENCY_SETUP
    {
        [Key]
        public int FREQUENCYSETUPID { get; set; }

        public int RATINGPERIOD { get; set; }

        public DateTime LASTRATINGDATE { get; set; }

        public DateTime NEXTRATINGDATE { get; set; }

    }
}
/*

public virtual DbSet<TBL_CORR_FREQUENCY_SETUP> TBL_CORR_FREQUENCY_SETUP { get; set; }

*/
