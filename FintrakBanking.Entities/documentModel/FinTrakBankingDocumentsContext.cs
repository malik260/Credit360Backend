namespace FintrakBanking.Entities.documentModel
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

        public virtual DbSet<tbl_Media_Collateral_Documents> tbl_Media_Collateral_Documents { get; set; }

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
        }
    }
}
