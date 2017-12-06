using FintrakBanking.ViewModels.Customer;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FintrakBanking.Interfaces.Customer
{
   public interface ICustomerStagingRepository
    {
        IEnumerable<CustomerInformationStagingViewModels> GetIntegratedCustomerInformation(string searchTerm);
    }
}
