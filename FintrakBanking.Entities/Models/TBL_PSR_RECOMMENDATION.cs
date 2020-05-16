namespace FintrakBanking.Entities.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("TBL_PSR_RECOMMENDATION")]
    public partial class TBL_PSR_RECOMMENDATION
    {
        [Key]
        public int PSRRECOMMENDATIONID { get; set; }

        [Required]
       // //[StringLength(1000)]
        public string COMMENTS { get; set; }

        //public string CUSTOMERRATING { get; set; }
        //public string PROJECTRISKRATING { get; set; }

        public int CREATEDBY { get; set; }
        public DateTime DATETIMECREATED { get; set; }
        public int? LASTUPDATEDBY { get; set; }
        public DateTime? DATETIMEUPDATED { get; set; }
        public bool DELETED { get; set; }
        public int? DELETEDBY { get; set; }
        public DateTime? DATETIMEDELETED { get; set; }

        public virtual TBL_PSR_PROJECT_SITE_REPORT TBL_PSR_PROJECT_SITE_REPORT { get; set; }
        public int PROJECTSITEREPORTID { get; set; }
    }
}
        /*

        public virtual DbSet<TBL_PSR_RECOMMENDATION> TBL_PSR_RECOMMENDATION { get; set; }

            modelBuilder.Entity<TBL_PSR_PROJECT_SITE_REPORT>()
                .HasMany(e => e.TBL_PSR_RECOMMENDATION)
                .WithRequired(e => e.TBL_PSR_PROJECT_SITE_REPORT)
                .WillCascadeOnDelete(false);

        */
