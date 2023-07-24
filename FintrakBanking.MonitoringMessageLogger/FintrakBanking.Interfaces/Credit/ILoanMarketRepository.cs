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
        string AddExposure(ExposureViewModel expo, int staffId);
        string UpdateLoanMarket(int markeetId, LoanMarketViewModel loanMarket);
        string updateExposure(int exposureId, ExposureViewModel expo);
        string DeleteLoanMarket(int markeetId, LoanMarketViewModel loanMarket);
        IEnumerable<ExposureViewModel> GetExposureManual(); 
        bool DeleteExposure(int exposureId, int staffId);
    }
}
