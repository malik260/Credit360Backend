using FintrakBanking.ViewModels.CASA;
using FintrakBanking.ViewModels.Customer;
using FintrakBanking.ViewModels.Finance;
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

        string GetAccountOwnerByAccountNumber(string accountNumber, int companyId);

        IEnumerable<CasaViewModel> GetAccountByCustomerId(int customerId);

        IEnumerable<CasaViewModel> FindAccount(string accountNumberOrName, int companyId);

        IEnumerable<CasaViewModel> GetGroupAccountNumberWithCustomerId(string accountNumberOrName, int customerId, int companyId);

        IEnumerable<CasaViewModel> GetOverdraftAccountNumberWithCustomerId(string accountNumberOrName, int customerId, int companyId);

        IQueryable<CustomerSearchVM> SearchCustomer(int customerTypeId,int companyId, string searchQuery);

        IQueryable<CasaCustomerSearchViewModel> SearchForCustomerAccount(int companyId, string searchQuery, int customerTypeId);

        IEnumerable<CasaBalanceViewModel> GetAllCustomerAccountByCustomerId(int customerId, int companyId);
        IEnumerable<CasaBalanceViewModel> GetAllCustomerAccountByCustomerIdAndCurrency(int customerId, int companyId, int currencyId);

        IEnumerable<CasaBalanceViewModel> GetBusinessAccounts( int companyId);

        CasaBalanceViewModel GetCASABalance(string casaAccountNumber, int companyId);

        string GetAllCASAAccount(string casaAccountNumber, int companyId);

        CasaCustomerSearchViewModel GetCustomerAccountDetailsById(int customerId);

        IEnumerable<CustomerCasaAcountsViewModel> GetAllCustomerAccount(int customerId, int applicationTypeId, int companyId);
        void AddCustomerAccounts(string customerCode);

        IEnumerable<CasaLienTypeViewModel> GetAllCasaLienTypes(int companyId);
        bool AddCasaLien(CasaViewModel model);
        IEnumerable<CasaLoanViewModel> GetAllCasaLoans(int companyId, int casaAccountId);
        IEnumerable<CasaViewModel> FindCustomerCasaLien(string accountNumberOrName, int companyId);
        IEnumerable<CasaAccountLienViewModel> GetAllCasaLiens(string accountNumber);
    }
}
