using FintrakBanking.ViewModels.CASA;
using FintrakBanking.ViewModels.Customer;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace FintrakBanking.Interfaces.CASA
{
    public interface ICasaRepository
    {
        CasaViewModel GetAccount(int accountId);

        int GetCasaAccountId(string accountNumber, int companyId);

        IEnumerable<CasaViewModel> GetAccountByCustomerId(int customerId);

        IEnumerable<CasaViewModel> FindAccount(string accountNumberOrName, int companyId);

        IQueryable<CustomerSearchVM> SearchCustomer(int customerTypeId,int companyId, string searchQuery);

        IQueryable<CasaCustomerSearchViewModel> SearchForCustomerAccount(int companyId, string searchQuery);

        IEnumerable<dynamic> GetAllCustomerAccountByCustomerId(int customerId, int companyId);

       
    }
}
