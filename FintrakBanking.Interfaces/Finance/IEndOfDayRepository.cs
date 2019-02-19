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
        bool RefreshLoanClassification();
        bool GetRunningEndOfDayProcess(int companyId);
        IEnumerable<FinanceEndofdayViewModel> GetEndofdayOperationLog(DateTime oedDate, int companyId);
    }
}
