using FintrakBanking.Entities.Models;
using FintrakBanking.Entities.StagingModels;
using FintrakBanking.ViewModels.Customer;
using FintrakBanking.ViewModels.ThridPartyIntegration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FintrakBanking.Interfaces.Customer
{
   public interface ICustomerStagingRepository
    {
        //IEnumerable<CustomerInformationStagingViewModels> GetIntegratedCustomerInformation(string searchTerm);
        // CustomerStagingRepository(FinTrakBankingStagingContext _context, FinTrakBankingContext _mainContext);
        // Task<List<CustomerViewModels>> GetIntegratedCustomerInformation(string searchTerm);
        List<CustomerViewModels> GetIntegratedCustomerInformation(string searchTerm);
        //Task<CustomerIntegrationViewModels> GetIntegratedCustomerInformation(string searchTerm);
        //Task<List<CustomerIntegrationViewModels>> GetIntegratedCustomerInformation(string searchTerm);


    }
}
