namespace FintrakBanking.Entities.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("TBL_PSR_NEXT_INSPECTION_TASK")]
    public partial class TBL_PSR_NEXT_INSPECTION_TASK
    {
        [Key]
        public int PSRNEXTINSPECTIONTASKID { get; set; }

        [Required]
        ////[StringLength(1000)]
        public string COMMENTS { get; set; }

        public bool ISDONE { get; set; }

        public int CREATEDBY { get; set; }
        public DateTime DATETIMECREATED { get; set; }
        public int? LASTUPDATEDBY { get; set; }
        public DateTime? DATETIMEUPDATED { get; set; }
        public bool DELETED { get; set; }
        public int? DELETEDBY { get; set; }
        public DateTime? DATETIMEDELETED { get; set; }

        public virtual TBL_PSR_PROJECT_SITE_REPORT TBL_PSR_PROJECT_SITE_REPORT { get; set; }
        public int PROJECTSITEREPORTID { get; set; }
        public DateTime NEXTINSPECTIONDATE { get; set; }
    }
}
        /*

        public virtual DbSet<TBL_PSR_NEXT_INSPECTION_TASK> TBL_PSR_NEXT_INSPECTION_TASK { get; set; }

            modelBuilder.Entity<TBL_PSR_PROJECT_SITE_REPORT>()
                .HasMany(e => e.TBL_PSR_NEXT_INSPECTION_TASK)
                .WithRequired(e => e.TBL_PSR_PROJECT_SITE_REPORT)
                .WillCascadeOnDelete(false);

        */
