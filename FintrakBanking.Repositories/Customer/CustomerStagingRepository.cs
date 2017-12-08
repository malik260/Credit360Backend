using FintrakBanking.Entities.StagingModels;
using FintrakBanking.Interfaces.Customer;
using FintrakBanking.ViewModels.Customer;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FintrakBanking.Repositories.Customer
{
   public class CustomerStagingRepository : ICustomerStagingRepository
    {
        private FinTrakBankingStagingContext context;
       public CustomerStagingRepository(FinTrakBankingStagingContext _context)
        {
            context = _context;
        }
        public IQueryable<CustomerInformationStagingViewModels> GetIntegratedCustomerInformation()
        {
            return this.context.STG_CUSTOMER.Select(x => new CustomerInformationStagingViewModels
            {
              customerCode = x.CUSTOMERCODE,
              firstName = x.FIRSTNAME,
              middleName = x.MIDDLENAME,
              lastName = x.LASTNAME,
              customerTypeId = (short)x.CUSTOMERTYPEID,
            dateOfBirth = (DateTime)x.DATEOFBIRTH
            });
        }
        public IEnumerable<CustomerInformationStagingViewModels> GetIntegratedCustomerInformation(string searchTerm)
        {
            IQueryable<CustomerInformationStagingViewModels> allCustomers = null;
            allCustomers = GetIntegratedCustomerInformation();
            if(allCustomers.ToList().Count() > 0)
            {
                if (!String.IsNullOrEmpty(searchTerm))
                {
                    allCustomers = allCustomers.Where(x =>
                    x.firstName.ToLower().Contains(searchTerm.ToLower())
                    || x.lastName.ToLower().Contains(searchTerm.ToLower())
                    || x.middleName.ToLower().Contains(searchTerm.ToLower())
                    || x.customerCode.Contains(searchTerm)
                    );
                }
                return allCustomers;
            }
            return null;
        }
    }
}
