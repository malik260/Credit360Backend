using FintrakBanking.ViewModels.Customer;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FintrakBanking.Interfaces.Credit
{
    public interface ICashFlowLendingRepository
    {
        APIResponse AddCustomer(IncomingCustomerViewModels model);
        APIResponse submitRequest(CflLoanApplication model);
    }
}
