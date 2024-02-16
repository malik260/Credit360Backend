namespace FintrakBanking.Entities.DocumentModels
{
    using System;
    using System.Data.Entity;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Linq;
    using FintrakBanking.Entities.Models;

    //[DbConfigurationType(typeof(OracleDatabaseConfiguration))]
    [DbConfigurationType(typeof(SqlDatabaseConfiguration))]
    public partial class FinTrakBankingDocumentsContext : DbContext
    {
        
        public FinTrakBankingDocumentsContext()
            : base("name=FinTrakBankingDocumentsContext")
        {
        }
        public virtual DbSet<TBL_LOAN_RECOVERY_REPORTING_DOCUMENT> TBL_LOAN_RECOVERY_REPORTING_DOCUMENT { get; set; }
        public virtual DbSet<TBL_CUSTOMER_CREDIT_BUREAU> TBL_CUSTOMER_CREDIT_BUREAU { get; set; }
        public virtual DbSet<TBL_DOC_COLLATERAL_VISITATION> TBL_DOC_COLLATERAL_VISITATION { get; set; }
        public virtual DbSet<TBL_DOC_COLLATERAL_RELEASE> TBL_DOC_COLLATERAL_RELEASE { get; set; }

        public virtual DbSet<TBL_LOAN_COMMITTEE_MINUTES> TBL_LOAN_COMMITTEE_MINUTES { get; set; }
        public virtual DbSet<TBL_LOAN_CONDITION_DOCUMENTS> TBL_LOAN_CONDITION_DOCUMENTS { get; set; }
        public virtual DbSet<TBL_MEDIA_CHECKLIST_DOCUMENTS> TBL_MEDIA_CHECKLIST_DOCUMENTS { get; set; }
        public virtual DbSet<TBL_MEDIA_COLLATERAL_DOCUMENTS> TBL_MEDIA_COLLATERAL_DOCUMENTS { get; set; }
        public virtual DbSet<TBL_MEDIA_COMPANY> TBL_MEDIA_COMPANY { get; set; }
        public virtual DbSet<TBL_MEDIA_JOB_REQUEST_DOCUMENT> TBL_MEDIA_JOB_REQUEST_DOCUMENT { get; set; }
        public virtual DbSet<TBL_MEDIA_KYC_DOCUMENTS> TBL_MEDIA_KYC_DOCUMENTS { get; set; }
        public virtual DbSet<TBL_MEDIA_LOAN_DOCUMENTS> TBL_MEDIA_LOAN_DOCUMENTS { get; set; }
        public virtual DbSet<TBL_TEMP_MEDIA_LOAN_DOCUMENTS> TBL_TEMP_MEDIA_LOAN_DOCUMENTS { get; set; }

        public virtual DbSet<TBL_MEDIA_STAFF_PICTURE> TBL_MEDIA_STAFF_PICTURE { get; set; }
        public virtual DbSet<TBL_MEDIA_STAFF_SIGNATURE> TBL_MEDIA_STAFF_SIGNATURE { get; set; }
        public virtual DbSet<TBL_TEMP_MEDIA_COLLATERAL_DOCS> TBL_TEMP_MEDIA_COLLATERAL_DOCS { get; set; }
        public virtual DbSet<TBL_MEDIA_LOAN_MATURITY_INSTR> TBL_MEDIA_LOAN_MATURITY_INSTR { get; set; }
        public virtual DbSet<TBL_LOAN_CONTINGENT_USAGE_DOCS> TBL_LOAN_CONTINGENT_USAGE_DOCS { get; set; }
        public virtual DbSet<TBL_DOC_INVOICE> TBL_DOC_INVOICE { get; set; }
        public virtual DbSet<TBL_TEMP_DOC_INVOICE> TBL_TEMP_DOC_INVOICE { get; set; }

        public virtual DbSet<TBL_DOCUMENT_UPLOAD> TBL_DOCUMENT_UPLOAD { get; set; }
        public virtual DbSet<TBL_DOCUMENT_USAGE> TBL_DOCUMENT_USAGE { get; set; }
        public virtual DbSet<TBL_DOCUMENT_USAGE_FINAL_ARCHIVE> TBL_DOCUMENT_USAGE_FINAL_ARCHIVE { get; set; }
        public virtual DbSet<TBL_DOCUMENT_CATEGORY_TYPE> TBL_DOCUMENT_CATEGORY_TYPE { get; set; }
        public virtual DbSet<TBL_DOCUMENT_CATEGORY> TBL_DOCUMENT_CATEGORY { get; set; }
        public virtual DbSet<TBL_DOCUMENT_TYPE> TBL_DOCUMENT_TYPE { get; set; }
        public virtual DbSet<TBL_DOC_MAPPING> TBL_DOC_MAPPING { get; set; }
        public virtual DbSet<TBL_DEFERRED_DOC_TRACKER> TBL_DEFERRED_DOC_TRACKER { get; set; }

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {

            var dbType = Convert.ToInt32(System.Configuration.ConfigurationManager.AppSettings["IsOracleDatabase"]);
            if (dbType == 1)
            {
                var databaseUsername =
                               System.Configuration.ConfigurationManager.AppSettings["DocumentOracleDatabaseUsername"];
                modelBuilder.HasDefaultSchema(databaseUsername);
            }

            //var databaseUsername = System.Configuration.ConfigurationManager.AppSettings["DocumentOracleDatabaseUsername"];

            //modelBuilder.HasDefaultSchema(databaseUsername); 


            modelBuilder.Entity<TBL_CUSTOMER_CREDIT_BUREAU>()
                .Property(e => e.DOCUMENT_TITLE)
                .IsUnicode(false);

            //modelBuilder.Entity<TBL_MEDIA_COLLATERAL_DOCUMENTS>()
            //    .Property(e => e.ISPRIMARYDOCUMENT)
            //    .HasPrecision(38, 0);

            modelBuilder.Entity<TBL_MEDIA_STAFF_PICTURE>()
                .Property(e => e.DOCUMENT_TITLE)
                .IsUnicode(false);

            //modelBuilder.Entity<TBL_TEMP_MEDIA_COLLATERAL_DOCS>()
            //    .Property(e => e.ISPRIMARYDOCUMENT)
            //    .HasPrecision(38, 0);

            modelBuilder.Entity<TBL_MEDIA_LOAN_MATURITY_INSTR>()
                .Property(e => e.DOCUMENT_TITLE)
                .IsUnicode(false);

            modelBuilder.Entity<TBL_MEDIA_LOAN_MATURITY_INSTR>()
                .Property(e => e.FILENAME)
                .IsUnicode(false);

            modelBuilder.Entity<TBL_MEDIA_LOAN_MATURITY_INSTR>()
                .Property(e => e.FILEEXTENSION)
                .IsUnicode(false);


            modelBuilder.Entity<TBL_DOCUMENT_CATEGORY>()
                .HasMany(e => e.TBL_DOCUMENT_TYPE)
                .WithRequired(e => e.TBL_DOCUMENT_CATEGORY)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<TBL_DOCUMENT_TYPE>()
                .HasMany(e => e.TBL_DOCUMENT_UPLOAD)
                .WithRequired(e => e.TBL_DOCUMENT_TYPE)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<TBL_DOCUMENT_TYPE>()
                .HasMany(e => e.TBL_DOC_MAPPING)
                .WithRequired(e => e.TBL_DOCUMENT_TYPE)
                .WillCascadeOnDelete(false);
        }
    }
}
