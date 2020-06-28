namespace FintrakBanking.Entities.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("TBL_PSR_PERFORMANCE_EVALUATION")]
    public partial class TBL_PSR_PERFORMANCE_EVALUATION
    {
        [Key]
        public int PSRPERFORMANCEEVALUATIONID { get; set; }
        public decimal? PROJECTSUM { get; set; }
        public decimal? PAYMENTTODATE { get; set; }
        public decimal? DISBURSEDTODATE { get; set; }
        public decimal? INITIALPROJECTSUM { get; set; }
        public decimal? VOWDTODATE { get; set; }
        public decimal? PMUASSESSED { get; set; }
        public decimal? CONSULTANTVOWD { get; set; }
        public decimal? COSTVARIATION { get; set; }
        public string TIMEVARIATION { get; set; }
        public decimal? APGISSUED { get; set; }
        public decimal? AMOUNTRECEIVED { get; set; }
        public decimal? PROGRESSPAYMENT { get; set; }
        public decimal? CERTIFIEDVOWD { get; set; }
        public int CREATEDBY { get; set; }
        public DateTime DATETIMECREATED { get; set; }
        public int? LASTUPDATEDBY { get; set; }
        public DateTime? DATETIMEUPDATED { get; set; }
        public bool DELETED { get; set; }
        public int? DELETEDBY { get; set; }
        public DateTime? DATETIMEDELETED { get; set; }

        public virtual TBL_PSR_PROJECT_SITE_REPORT TBL_PSR_PROJECT_SITE_REPORT { get; set; }
        public int PROJECTSITEREPORTID { get; set; }
        public int PSRREPORTTYPEID { get; set; }
        public decimal? AMORTISEDAPG { get; set; }
        public int APPROVALSTATUSID { get; set; }
    }
}
        /*

        public virtual DbSet<TBL_PSR_PERFORMANCE_EVALUATION> TBL_PSR_PERFORMANCE_EVALUATION { get; set; }

            modelBuilder.Entity<TBL_PSR_PROJECT_SITE_REPORT>()
                .HasMany(e => e.TBL_PSR_PERFORMANCE_EVALUATION)
                .WithRequired(e => e.TBL_PSR_PROJECT_SITE_REPORT)
                .WillCascadeOnDelete(false);

        */
