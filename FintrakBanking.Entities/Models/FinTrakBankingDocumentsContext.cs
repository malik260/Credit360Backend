namespace FintrakBanking.Entities.Models
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
        public virtual DbSet<tbl_Media_Loan_Documents> tbl_Media_Loan_Documents { get; set; }

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
        }
    }
}
