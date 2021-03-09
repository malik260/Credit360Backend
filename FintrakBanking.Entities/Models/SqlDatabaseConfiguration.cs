using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FintrakBanking.Entities.Models
{
    public class SqlDatabaseConfiguration : DbConfiguration
    {
        public SqlDatabaseConfiguration()
        { 
            this.SetDefaultConnectionFactory(new System.Data.Entity.Infrastructure.SqlConnectionFactory());
        }
    }
}
