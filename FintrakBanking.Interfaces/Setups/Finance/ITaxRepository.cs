using FintrakBanking.ViewModels;
using FintrakBanking.ViewModels.Setups.Finance;
using System.Collections.Generic;

namespace FintrakBanking.Interfaces.Setups.Finance
{
    public interface ITaxRepository
    {
        TaxViewModel GetTax(int taxId);

        IEnumerable<TaxViewModel> GetAllTax();

        IEnumerable<TaxViewModel> GetAllTaxByCompanyId(int companyId);

        bool AddTax(TaxViewModel model);

        bool UpdateTax(TaxViewModel model, int taxId);

        bool DeleteTax(int taxId, UserInfo user);
    }
}
