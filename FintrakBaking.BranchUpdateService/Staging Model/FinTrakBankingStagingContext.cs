namespace FintrakBaking.BranchUpdateService.Staging_Model
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

        public virtual DbSet<STG_CUSTOMER> STG_CUSTOMER { get; set; }
        public virtual DbSet<STG_BRANCH> STG_BRANCH { get; set; }
        public virtual DbSet<STG_STAFF> STG_STAFF { get; set; }

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
        }
    }
}
