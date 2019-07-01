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
        public string PROJECTSUM { get; set; }
        public string PAYMENTTODATE { get; set; }
        public string DISBURSEDTODATE { get; set; }
        public string INITIALPROJECTSUM { get; set; }
        public string VOWDTODATE { get; set; }
        public string PMUASSESSED { get; set; }
        public string CONSULTANTVOWD { get; set; }
        public string COSTVARIATION { get; set; }
        public string TIMEVARIATION { get; set; }
        public string APGISSUED { get; set; }
        public string AMOUNTRECEIVED { get; set; }
        public string PROGRESSPAYMENT { get; set; }
        public string CERTIFIEDVOWD { get; set; }
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
        public string AMORTISEDAPG { get; set; }
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
