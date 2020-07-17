namespace FintrakBanking.Entities.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("TBL_PSR_PROJECT_SITE_REPORT")]
    public partial class TBL_PSR_PROJECT_SITE_REPORT
    {
        public TBL_PSR_PROJECT_SITE_REPORT()
        {
            //TBL_PSR_PSR_PERFORMANCE_EVALUATION = new HashSet<TBL_PSR_PSR_PERFORMANCE_EVALUATION>();
            //TBL_PSR_PSR_COMMENT = new HashSet<TBL_PSR_PSR_COMMENT>();
            //TBL_PSR_PSR_OBSERVATION = new HashSet<TBL_PSR_PSR_OBSERVATION>();
            //TBL_PSR_PSR_RECOMMENDATION = new HashSet<TBL_PSR_PSR_RECOMMENDATION>();
            //TBL_PSR_PSR_NEXT_INSPECTION_TASK = new HashSet<TBL_PSR_PSR_NEXT_INSPECTION_TASK>();
            //TBL_PSR_PSR_REPORT_TYPE = new HashSet<TBL_PSR_PSR_REPORT_TYPE>();
        }

        [Key]
        public int PROJECTSITEREPORTID { get; set; }

        public int PSRREPORTTYPEID { get; set; }

        public int LOANAPPLICATIONID { get; set; }
        public int? LOANAPPLICATIONDETAILID { get; set; }
        public string PROJECTLOCATION { get; set; }

        [Required]
        ////[StringLength(2)]
        public string CLIENTNAME { get; set; }

        [Required]
        ////[StringLength(2)]
        public string CONTRACTORNAME { get; set; }

        [Required]
        ////[StringLength(2)]
        public string CONSULTANTNAME { get; set; }

        public decimal PROJECTAMOUNT { get; set; }

        [Required]
        //[StringLength(1000)]
        public string PROJECTDESCRIPTION { get; set; }

        public DateTime COMMENCEMENTDATE { get; set; }

        public DateTime COMPLETIONDATE { get; set; }

        //public DateTime NEXTVISITATIONDATE { get; set; }
        public DateTime? INSPECTIONDATE { get; set; }

        public int CREATEDBY { get; set; }
        public DateTime DATETIMECREATED { get; set; }
        public int? LASTUPDATEDBY { get; set; }
        public DateTime? DATETIMEUPDATED { get; set; }
        public bool DELETED { get; set; }
        public bool ACCEPTANCE { get; set; }
        public int? DELETEDBY { get; set; }
        public DateTime? DATETIMEDELETED { get; set; }
        public int APPROVALSTATUSID { get; set; }
        public int? CURRENCYID { get; set; }

        //public virtual ICollection<TBL_PSR_PSR_PERFORMANCE_EVALUATION> TBL_PSR_PSR_PERFORMANCE_EVALUATION { get; set; }

        //public virtual ICollection<TBL_PSR_PSR_COMMENT> TBL_PSR_PSR_COMMENT { get; set; }

        //public virtual ICollection<TBL_PSR_PSR_OBSERVATION> TBL_PSR_PSR_OBSERVATION { get; set; }

        //public virtual ICollection<TBL_PSR_PSR_RECOMMENDATION> TBL_PSR_PSR_RECOMMENDATION { get; set; }

        //public virtual ICollection<TBL_PSR_PSR_NEXT_INSPECTION_TASK> TBL_PSR_PSR_NEXT_INSPECTION_TASK { get; set; }

        //public virtual ICollection<TBL_PSR_PSR_REPORT_TYPE> TBL_PSR_PSR_REPORT_TYPE { get; set; }

    }
}
        /*

        public virtual DbSet<TBL_PSR_PROJECT_SITE_REPORT> TBL_PSR_PROJECT_SITE_REPORT { get; set; }

            modelBuilder.Entity<TBL_PSR_PROJECT_SITE_REPORT>()
                .HasMany(e => e.TBL_PSR_PSR_PERFORMANCE_EVALUATION)
                .WithRequired(e => e.TBL_PSR_PROJECT_SITE_REPORT)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<TBL_PSR_PROJECT_SITE_REPORT>()
                .HasMany(e => e.TBL_PSR_PSR_COMMENT)
                .WithRequired(e => e.TBL_PSR_PROJECT_SITE_REPORT)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<TBL_PSR_PROJECT_SITE_REPORT>()
                .HasMany(e => e.TBL_PSR_PSR_OBSERVATION)
                .WithRequired(e => e.TBL_PSR_PROJECT_SITE_REPORT)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<TBL_PSR_PROJECT_SITE_REPORT>()
                .HasMany(e => e.TBL_PSR_PSR_RECOMMENDATION)
                .WithRequired(e => e.TBL_PSR_PROJECT_SITE_REPORT)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<TBL_PSR_PROJECT_SITE_REPORT>()
                .HasMany(e => e.TBL_PSR_PSR_NEXT_INSPECTION_TASK)
                .WithRequired(e => e.TBL_PSR_PROJECT_SITE_REPORT)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<TBL_PSR_PROJECT_SITE_REPORT>()
                .HasMany(e => e.TBL_PSR_PSR_REPORT_TYPE)
                .WithRequired(e => e.TBL_PSR_PROJECT_SITE_REPORT)
                .WillCascadeOnDelete(false);

        */
