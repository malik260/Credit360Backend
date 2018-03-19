using FintrakBanking.ViewModels;
using FintrakBanking.ViewModels.Report;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FintrakBanking.Interfaces.Reports
{
    public interface ILimitAndMonitoringRepository 
    {
        IEnumerable<LimitAndMonitoringViewModel> GetAllEmailAlertMessages();

        IEnumerable<LimitAndMonitoringViewModel> GetAllSetEmailAlertMessages();

        LimitAndMonitoringViewModel GetAllSetEmailAlertMessages(int alertId);

        string AddNewEmailMessageSeeting(LimitAndMonitoringViewModel alertSetting);

        string UpdateEmailMessageSeeting(LimitAndMonitoringViewModel alertSetting);

       string  RemoveEmailMessageSeeting(LimitAndMonitoringViewModel alertSetting);

        string UpdateEmailAlertMessages(LimitAndMonitoringViewModel data);

    }
}
