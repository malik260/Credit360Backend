using FintrakBanking.ViewModels.Credit;
using FintrakBanking.ViewModels.Setups.Credit;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FintrakBanking.Interfaces.Credit
{
   public interface ILoanPerformanceRepository
    {
        IEnumerable<PrudGuildlineTypeViewModel> GetPrudGuildlineType();
        IQueryable<LoanViewModel> GetAllLoan();
        IEnumerable<LoanViewModel> GetAllLoans();

        IEnumerable<PrudentialGuidelineViewModel> GetPrudGuildlineStatus();
        bool LoanPerformanceStatusChange(PrudGuidelineStatusChangeViewModel entity);
    }
}
