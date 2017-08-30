using FintrakBanking.ViewModels;
using FintrakBanking.ViewModels.Customer;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace FintrakBanking.Interfaces.Customer
{
    public interface ICustomerRepository
    {
         CustomerViewModels GetCustomer(int custormerId);
        IEnumerable<CustomerViewModels> GetCustomerInGroupByGroupId(int groupId);

        IEnumerable<CustomerViewModels> GetCustomerByBranchId(int branchId);

        IEnumerable<CustomerViewModels> GetCustomerByCompanyId(int companyId);

        IEnumerable<CustomerTypeViewModels> GetCustomerType();

        IEnumerable<CustomerViewModels> GetCustomerByTypeId(int customerTypeId);

        Task<bool> AddCustomer(CustomerViewModels entity);

        Task<bool> UpdateCustomer(int customerId, CustomerViewModels entity);

        Task<bool> DeleteCustomer(int customerId,  UserInfo user);

        IEnumerable<CustomerViewModels> CustomerSearch(int companyId, string search) ;
        IEnumerable<CustomerViewModels> CustomerSearch(int companyId, CustomerSearchItemViewModels search);
        IQueryable<CustomerSearchItemViewModels> CustomerSearchRealTime(int companyId, string search);

        IEnumerable<CustomerSectorViewModel> GetCustomerSectors();
        IEnumerable<CustomerSectorViewModel> GetCustomerSectorBySubSectorId(short ssId);
    }
}