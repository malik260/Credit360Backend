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

        public virtual DbSet<TBL_APPROVAL_GROUP> TBL_APPROVAL_GROUP { get; set; }
        public virtual DbSet<TBL_APPROVAL_GROUP_MAPPING> TBL_APPROVAL_GROUP_MAPPING { get; set; }
        public virtual DbSet<TBL_APPROVAL_LEVEL> TBL_APPROVAL_LEVEL { get; set; }
        public virtual DbSet<TBL_APPROVAL_LEVEL_STAFF> TBL_APPROVAL_LEVEL_STAFF { get; set; }
        public virtual DbSet<TBL_APPROVAL_STATE> TBL_APPROVAL_STATE { get; set; }
        public virtual DbSet<TBL_BRANCH> TBL_BRANCH { get; set; }
        public virtual DbSet<TBL_CITY> TBL_CITY { get; set; }
        public virtual DbSet<TBL_CITY_CLASS> TBL_CITY_CLASS { get; set; }
        public virtual DbSet<TBL_STAFF> TBL_STAFF { get; set; }
        public virtual DbSet<TBL_STAFF_JOBTITLE> TBL_STAFF_JOBTITLE { get; set; }
        public virtual DbSet<TBL_STAFF_ORGANOGRAM> TBL_STAFF_ORGANOGRAM { get; set; }
        public virtual DbSet<TBL_STAFF_RANK> TBL_STAFF_RANK { get; set; }
        public virtual DbSet<TBL_STATE> TBL_STATE { get; set; }

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            modelBuilder.Entity<TBL_APPROVAL_GROUP>()
                .HasMany(e => e.TBL_APPROVAL_GROUP_MAPPING)
                .WithRequired(e => e.TBL_APPROVAL_GROUP)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<TBL_APPROVAL_GROUP>()
                .HasMany(e => e.TBL_APPROVAL_LEVEL)
                .WithRequired(e => e.TBL_APPROVAL_GROUP)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<TBL_APPROVAL_LEVEL>()
                .Property(e => e.MAXIMUMAMOUNT)
                .HasPrecision(19, 4);

            modelBuilder.Entity<TBL_APPROVAL_LEVEL>()
                .Property(e => e.INVESTMENTGRADEAMOUNT)
                .HasPrecision(19, 4);

            modelBuilder.Entity<TBL_APPROVAL_LEVEL>()
                .HasMany(e => e.TBL_APPROVAL_LEVEL_STAFF)
                .WithRequired(e => e.TBL_APPROVAL_LEVEL)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<TBL_APPROVAL_LEVEL_STAFF>()
                .Property(e => e.MAXIMUMAMOUNT)
                .HasPrecision(19, 4);

            modelBuilder.Entity<TBL_BRANCH>()
                .Property(e => e.NPL_LIMIT)
                .HasPrecision(19, 4);

            modelBuilder.Entity<TBL_CITY_CLASS>()
                .HasMany(e => e.TBL_CITY)
                .WithRequired(e => e.TBL_CITY_CLASS)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<TBL_STAFF>()
                .Property(e => e.GENDER)
                .IsFixedLength();

            modelBuilder.Entity<TBL_STAFF>()
                .Property(e => e.GENDEROFNOK)
                .IsFixedLength();

            modelBuilder.Entity<TBL_STAFF>()
                .Property(e => e.NPL_LIMIT)
                .HasPrecision(19, 4);

            modelBuilder.Entity<TBL_STAFF>()
                .Property(e => e.LOAN_LIMIT)
                .HasPrecision(19, 4);

            modelBuilder.Entity<TBL_STAFF>()
                .HasMany(e => e.TBL_APPROVAL_LEVEL_STAFF)
                .WithRequired(e => e.TBL_STAFF)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<TBL_STAFF>()
                .HasMany(e => e.TBL_STAFF1)
                .WithOptional(e => e.TBL_STAFF2)
                .HasForeignKey(e => e.RELIEF_STAFFID);

            modelBuilder.Entity<TBL_STAFF>()
                .HasMany(e => e.TBL_STAFF11)
                .WithOptional(e => e.TBL_STAFF3)
                .HasForeignKey(e => e.SUPERVISOR_STAFFID);

            modelBuilder.Entity<TBL_STAFF_JOBTITLE>()
                .HasMany(e => e.TBL_STAFF)
                .WithRequired(e => e.TBL_STAFF_JOBTITLE)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<TBL_STAFF_RANK>()
                .HasMany(e => e.TBL_STAFF)
                .WithRequired(e => e.TBL_STAFF_RANK)
                .WillCascadeOnDelete(false);

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
