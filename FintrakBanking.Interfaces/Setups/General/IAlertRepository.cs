using FintrakBanking.ViewModels;
using FintrakBanking.ViewModels.Setups.General;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FintrakBanking.Interfaces.Setups.General
{
   public interface IAlertRepository
    {
        IEnumerable<AlertViewModel> GetAllAlerts();
        bool AddAlertTitle(AlertViewModel model);
        bool UpdateLcCondition(AlertViewModel model, int id, UserInfo user);
        bool DeleteLcCondition(int id, UserInfo user);
    }
}
