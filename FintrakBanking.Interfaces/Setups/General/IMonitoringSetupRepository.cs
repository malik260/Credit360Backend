using FintrakBanking.ViewModels.Setups.General;
using FintrakBanking.ViewModels.WorkFlow;
using System.Collections.Generic;
using System.Linq;

namespace FintrakBanking.Interfaces.Setups.General
{
    public interface IMonitoringSetupRepository
    {
        IEnumerable<MonitoringSetupViewModel> GetAllMonitoringSetup();

        bool AddMonitoringSetup(MonitoringSetupViewModel entity);

        bool UpdateMonitoringSetup(int MonitoringSetupId, MonitoringSetupViewModel entity);

        MonitoringSetupViewModel GetMonitoringSetup(int MonitoringSetupId);
        IEnumerable<MonitoringSetupViewModel> GetAllMessageType();
        IEnumerable<MonitoringSetupViewModel> GetAllProduct();
    }
}