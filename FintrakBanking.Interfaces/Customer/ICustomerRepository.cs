using FintrakBanking.ViewModels;
using FintrakBanking.ViewModels.Customer;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace FintrakBanking.Interfaces.Customer
{
    public interface ICustomerRepository
    {
        IEnumerable<CustomerViewModels> GetCustomer(int custormerId);

        IEnumerable<CustomerViewModels> GetCustomerByBranchId(int branchId);

        IEnumerable<CustomerViewModels> GetCustomerByCompanyId(int companyId);

        IEnumerable<CustomerTypeViewModels> GetCustomerType();

        IEnumerable<CustomerViewModels> GetCustomerByTypeId(int customerTypeId);

        Task<bool> AddCustomer(CustomerViewModels entity);

        Task<bool> UpdateCustomer(int customerId, CustomerViewModels entity);

        Task<bool> DeleteCustomer(int customerId,  UserInfo user);

        IEnumerable<CustomerViewModels> CustomerSearch(int companyId, string search) ;
        IEnumerable<CustomerViewModels> CustomerSearch(int companyId, CustomerSearchItemViewModels search);

        IEnumerable<CustomerSectorViewModel> GetCustomerSectors();
        IEnumerable<CustomerSectorViewModel> GetCustomerSectorBySubSectorId(short ssId);
    }
}