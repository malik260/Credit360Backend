using FintrakBanking.ViewModels.Finance;
using System;
using System.Collections.Generic;

namespace FintrakBanking.Interfaces.Finance
{
    public interface IFinanceTransactionRepository
    {
        bool AddCollateralSearchLien(CasaLienViewModel model);

        FinanceTransactionViewModel PostCollateralSearch(CasaLienViewModel model);

        string PostTransaction(List<FinanceTransactionViewModel> transaction);

        double GetExchangeRate(short currencyId,   int companyId);
    }
}