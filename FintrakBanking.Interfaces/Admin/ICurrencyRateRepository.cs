using FintrakBanking.ViewModels.Admin;
using System;
using System.Collections.Generic;
using System.Text;

namespace FintrakBanking.Interfaces.Admin
{
    public interface ICurrencyRateRepository
    {
        #region Currency Rate
        CurrencyViewModel GetBaseCurrency(int companyId);
        IEnumerable<CurrencyViewModel> GetCurrency();
        IEnumerable<CurrencyRateViewModel> GetCurrencyRate();
        List<CurrencyRateViewModel> GetCurrencyRateById(short currencyRateId);
        bool AddCurrencyRate( CurrencyRateViewModel model);
        bool UpdateCurrencyRate(short currencyRateId, CurrencyRateViewModel model);
        
        #endregion
    }
}
