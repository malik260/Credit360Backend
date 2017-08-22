using FintrakBanking.ViewModels.CASA;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace FintrakBanking.Interfaces.CASA
{
    public interface ICasaRepository
    {
        CasaViewModel GetAccount(int accountId);

        IEnumerable<CasaViewModel> GetAccountByCustomerId(int customerId);

        IEnumerable<CasaViewModel> FindAccount(string accountNumberOrName, int companyId);

        IQueryable<CustomerSearchVM> SearchCustomer(int customerTypeId,int companyId, string searchQuery);

        IQueryable<CasaCustomerSearchViewModel> SearchForCustomerAccount(int companyId, string searchQuery);

    }
}
