using FintrakBanking.ViewModels.Finance;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FintrakBanking.Interfaces.Finance
{
    public interface IEndOfDayRepository
    {
        bool RunEndOfDay(EndOfDayViewModel model);
        IEnumerable<FinanceEndofdayViewModel> GetFinanceEndofday(int companyId);
        bool ChangeApplicationDate(EndOfDayViewModel model); 
        bool RefreshLoanClassification(int companyId);
        bool GetRunningEndOfDayProcess(int companyId);
        IEnumerable<FinanceEndofdayViewModel> GetEndofdayOperationLog(DateTime eodDate, int companyId);
        IEnumerable<FinanceEndofdayViewModel> GetEndofdayOperationLogMonitoring(int companyId);
        IEnumerable<RefreshStagingMonitoringModel> RefreshStagingMonitoring(DateTime stateDate, DateTime endDate);
    }
}
