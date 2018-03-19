namespace FintrakBaking.BranchUpdateService.Fintrak_Model
{
    using System;
    using System.Data.Entity;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Linq;

    public partial class FinTrakBankingContext : DbContext
    {
        public FinTrakBankingContext()
            : base("name=FinTrakBankingContext")
        {
        }

        public virtual DbSet<TBL_BRANCH> TBL_BRANCH { get; set; }
        public virtual DbSet<TBL_CITY> TBL_CITY { get; set; }
        public virtual DbSet<TBL_STATE> TBL_STATE { get; set; }

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            modelBuilder.Entity<TBL_BRANCH>()
                .Property(e => e.NPL_LIMIT)
                .HasPrecision(19, 4);

            modelBuilder.Entity<TBL_STATE>()
                .Property(e => e.COLLATERALSEARCHCHARGEAMOUNT)
                .HasPrecision(19, 4);

            modelBuilder.Entity<TBL_STATE>()
                .Property(e => e.CHARTINGAMOUNT)
                .HasPrecision(19, 4);

            modelBuilder.Entity<TBL_STATE>()
                .Property(e => e.VERIFICATIONAMOUNT)
                .HasPrecision(19, 4);

            modelBuilder.Entity<TBL_STATE>()
                .HasMany(e => e.TBL_CITY)
                .WithRequired(e => e.TBL_STATE)
                .WillCascadeOnDelete(false);
        }
    }
}
