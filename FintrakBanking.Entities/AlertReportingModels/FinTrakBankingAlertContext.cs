using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FintrakBanking.Entities.AlertReportingModels
{
    [DbConfigurationType(typeof(OracleDatabaseConfiguration))]
    public partial class FinTrakBankingAlertContext : DbContext
    {

       public FinTrakBankingAlertContext()
        : base("name=FinTrakBankingAlertContext")
            {
            }
public virtual DbSet<ALERT> ALERT { get; set; }

protected override void OnModelCreating(DbModelBuilder modelBuilder)
{

    var dbType = Convert.ToInt32(System.Configuration.ConfigurationManager.AppSettings["IsOracleDatabase"]);
    if (dbType == 1)
    {
        var databaseUsername =
                       System.Configuration.ConfigurationManager.AppSettings["AlertOracleDatabaseUsername"];
        modelBuilder.HasDefaultSchema(databaseUsername);
    }

    modelBuilder.Entity<ALERT>()
        .Property(e => e.ACCOUNTNAME)
        .IsUnicode(false);

    

        }
    }
}