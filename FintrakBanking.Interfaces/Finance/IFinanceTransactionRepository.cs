using FintrakBanking.ViewModels.Finance;
using System;

namespace FintrakBanking.Interfaces.Finance
{
    public interface IFinanceTransactionRepository
    {
        CasaLienViewModel AddCollateralSearchLien(CasaLienViewModel model);

        CasaLienViewModel PostCollateralSearch(CasaLienViewModel model);

        string PostTransaction(FinanceTransactionViewModel transaction);

        double GetExchangeRate(short currencyId,   int companyId);
    }
}