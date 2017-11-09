namespace FintrakBanking.Entities.DocumentModels
{
    using System;
    using System.Data.Entity;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Linq;

    public partial class FinTrakBankingDocumentsContext : DbContext
    {
        public FinTrakBankingDocumentsContext()
            : base("name=FinTrakBankingDocumentsContext")
        {
        }

        public virtual DbSet<TBL_MEDIA_CHECKLIST_DOCUMENTS> TBL_MEDIA_CHECKLIST_DOCUMENTS { get; set; }
        public virtual DbSet<TBL_MEDIA_COLLATERAL_DOCUMENTS> TBL_MEDIA_COLLATERAL_DOCUMENTS { get; set; }
        public virtual DbSet<TBL_MEDIA_JOB_REQUEST_DOCUMENTS> TBL_MEDIA_JOB_REQUEST_DOCUMENTS { get; set; }
        public virtual DbSet<TBL_MEDIA_KYC_DOCUMENTS> TBL_MEDIA_KYC_DOCUMENTS { get; set; }
        public virtual DbSet<TBL_MEDIA_LOAN_DOCUMENTS> TBL_MEDIA_LOAN_DOCUMENTS { get; set; }
        public virtual DbSet<TBL_MEDIA_STAFF_SIGNATURE> TBL_MEDIA_STAFF_SIGNATURE { get; set; }

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            modelBuilder.Entity<TBL_MEDIA_STAFF_SIGNATURE>()
                .Property(e => e.DOCUMENT_TITLE)
                .IsUnicode(false);
        }
    }
}
