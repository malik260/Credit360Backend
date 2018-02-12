using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FintrakBanking.Interfaces.ThridPartyIntegration
{
  public   interface IXDSServiceRepository
    {
        bool IsticketActive(string userName, string password);
        string Login(string userName, string password);
        List<dynamic> GetApprovedReasons();
    }
}
