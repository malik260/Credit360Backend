namespace FintrakBanking.Entities.StagingModels
{
    using System;
    using System.Data.Entity;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Linq;

    public partial class FinTrakBankingStagingContext : DbContext
    {
        public FinTrakBankingStagingContext()
            : base("name=FinTrakBankingStagingContext")
        {
        }

        public virtual DbSet<STG_BRANCH> STG_BRANCH { get; set; }
        public virtual DbSet<STG_CURRENCY_EXCHANGERATE> STG_CURRENCY_EXCHANGERATE { get; set; }
        public virtual DbSet<STG_CUSTOMER> STG_CUSTOMER { get; set; }
        public virtual DbSet<STG_LOAN_MART> STG_LOAN_MART { get; set; }
        public virtual DbSet<STG_STAFF> STG_STAFF { get; set; }
        public virtual DbSet<STG_PRICE_INDEX_RATE> STG_PRICE_INDEX_RATE { get; set; }
        public virtual DbSet<STG_STAFF_RAW2> STG_STAFF_RAW2 { get; set; }

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            modelBuilder.Entity<STG_LOAN_MART>()
                .Property(e => e.PRINCIPALAMOUNT)
                .HasPrecision(19, 4);

            modelBuilder.Entity<STG_LOAN_MART>()
                .Property(e => e.OUTSTANDINGPRINCIPAL)
                .HasPrecision(19, 4);

            modelBuilder.Entity<STG_LOAN_MART>()
                .Property(e => e.OUTSTANDINGINTEREST)
                .HasPrecision(19, 4);

            modelBuilder.Entity<STG_LOAN_MART>()
                .Property(e => e.CASABALANCE)
                .HasPrecision(19, 4);
        }
    }
}
