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

        public virtual DbSet<STG_BRANCH> STG_BRANCH { get; set; }

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
        }
    }
}
