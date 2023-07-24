

using FintrakBanking.ViewModels;
using FintrakBanking.ViewModels.Setups.Credit;
using System.Collections.Generic;

namespace FintrakBanking.Interfaces.Setups.Credit
{
    public interface ICustomerProductFeeRepository
    {
        IEnumerable<CustomerProductFeeViewModel> GetAllCustomerProductFees(int companyId);
        IEnumerable<CustomerProductFeeViewModel> GetCustomerProductFeeByCustomerId(int companyId, int customerId);
        IEnumerable<CustomerProductFeeViewModel> GetCustomerProductFeeByProductId(int companyId, int productId);
        bool AddCustomerProductFee(CustomerProductFeeViewModel model);
        bool UpdateCustomerProductFee(int customerProductFeeId, CustomerProductFeeViewModel model);
        bool DeleteCustomerProductFee(int customerProductFeeId, UserInfo user);
    }
}