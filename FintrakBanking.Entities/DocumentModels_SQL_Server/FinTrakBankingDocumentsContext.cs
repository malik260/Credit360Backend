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

        public virtual DbSet<TBL_CUSTOMER_CREDIT_BUREAU> TBL_CUSTOMER_CREDIT_BUREAU { get; set; }
        public virtual DbSet<TBL_DOC_COLLATERAL_VISITATION> TBL_DOC_COLLATERAL_VISITATION { get; set; }
        public virtual DbSet<TBL_LOAN_COMMITTEE_MINUTES> TBL_LOAN_COMMITTEE_MINUTES { get; set; }
        public virtual DbSet<TBL_LOAN_CONDITION_DOCUMENTS> TBL_LOAN_CONDITION_DOCUMENTS { get; set; }
        public virtual DbSet<TBL_MEDIA_CHECKLIST_DOCUMENTS> TBL_MEDIA_CHECKLIST_DOCUMENTS { get; set; }
        public virtual DbSet<TBL_MEDIA_COLLATERAL_DOCUMENTS> TBL_MEDIA_COLLATERAL_DOCUMENTS { get; set; }
        public virtual DbSet<TBL_MEDIA_COMPANY> TBL_MEDIA_COMPANY { get; set; }
        public virtual DbSet<TBL_MEDIA_JOB_REQUEST_DOCUMENT> TBL_MEDIA_JOB_REQUEST_DOCUMENT { get; set; }
        public virtual DbSet<TBL_MEDIA_KYC_DOCUMENTS> TBL_MEDIA_KYC_DOCUMENTS { get; set; }
        public virtual DbSet<TBL_MEDIA_LOAN_DOCUMENTS> TBL_MEDIA_LOAN_DOCUMENTS { get; set; }
        public virtual DbSet<TBL_MEDIA_STAFF_PICTURE> TBL_MEDIA_STAFF_PICTURE { get; set; }
        public virtual DbSet<TBL_MEDIA_STAFF_SIGNATURE> TBL_MEDIA_STAFF_SIGNATURE { get; set; }
        public virtual DbSet<TBL_TEMP_MEDIA_COLLATERAL_DOCS> TBL_TEMP_MEDIA_COLLATERAL_DOCS { get; set; }
        public virtual DbSet<TBL_MEDIA_LOAN_MATURITY_INSTR> TBL_MEDIA_LOAN_MATURITY_INSTR { get; set; }

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            modelBuilder.Entity<TBL_CUSTOMER_CREDIT_BUREAU>()
                .Property(e => e.DOCUMENT_TITLE)
                .IsUnicode(false);

            modelBuilder.Entity<TBL_MEDIA_STAFF_PICTURE>()
                .Property(e => e.DOCUMENT_TITLE)
                .IsUnicode(false);

            modelBuilder.Entity<TBL_MEDIA_LOAN_MATURITY_INSTR>()
                .Property(e => e.DOCUMENT_TITLE)
                .IsUnicode(false);
        }
    }
}
