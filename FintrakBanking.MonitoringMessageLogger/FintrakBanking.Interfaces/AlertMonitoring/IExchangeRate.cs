using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FintrakBanking.Interfaces.AlertMonitoring
{
  public  interface ICurrencyAndRateUpdate
    {
        bool MigrateExchangeRate();
    }
}
