namespace FintrakBanking.Entities.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("TBL_ATC_TYPE")]
    public partial class TBL_ATC_TYPE
    {
        [Key]
        public int ATCTYPEID { get; set; }

        [Required]
        //[StringLength(100)]
        public string ACTTYPENAME { get; set; }

       // public virtual TBL_ATC_LODGMENT TBL_ATC_LODGMENT { get; set; }

    }
}
        /*

        public virtual DbSet<TBL_ATC_TYPE> TBL_ATC_TYPE { get; set; }

            modelBuilder.Entity<TBL_ATC_LODGMENT>()
                .HasMany(e => e.TBL_ATC_TYPE)
                .WithRequired(e => e.TBL_ATC_LODGMENT)
                .WillCascadeOnDelete(false);

        */
