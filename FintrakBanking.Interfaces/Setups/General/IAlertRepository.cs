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
        bool DeleteAlertTitle(int id, UserInfo user);
        AlertViewModel GetAlertById(int id);
        bool UpdateAlertTitle(int id, AlertViewModel model, UserInfo user);
    }
}
