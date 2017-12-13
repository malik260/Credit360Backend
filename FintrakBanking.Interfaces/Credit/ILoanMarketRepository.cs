using FintrakBanking.ViewModels.Credit;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FintrakBanking.Interfaces.Credit
{
  public interface ILoanMarketRepository 
    {
        IEnumerable<LoanMarketViewModel> GetLoanMarket(int companyId);

        LoanMarketViewModel GetLoanMarket(int markeetId, int companyId);

        string AddLoanMarket(LoanMarketViewModel loanMarket);

        string UpdateLoanMarket(LoanMarketViewModel loanMarket);

        string DeleteLoanMarket(LoanMarketViewModel loanMarket);
    }
}
