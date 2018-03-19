using FintrakBanking.Entities.Models;
using FintrakBanking.Entities.StagingModels;
using FintrakBanking.Interfaces.Customer;
using FintrakBanking.ViewModels.Customer;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FinTrakBanking.ThirdPartyIntegration.Finacle;
using FintrakBanking.ViewModels.ThridPartyIntegration;

namespace FintrakBanking.Repositories.Customer
{
   public class CustomerStagingRepository : ICustomerStagingRepository
    {
        private FinTrakBankingStagingContext context;
        private FinTrakBankingContext mainContext;

       public CustomerStagingRepository(FinTrakBankingStagingContext _context, FinTrakBankingContext _mainContext)
        {
            context = _context;
            mainContext = _mainContext;
        }

        //CustomerInformationStagingViewModels
        public IQueryable<CustomerViewModels> GetIntegratedCustomerInformation()
        {
            return this.context.STG_CUSTOMER.Select(x => new CustomerViewModels
            {
              customerCode = x.CUSTOMERCODE,
              firstName = x.FIRSTNAME,
              middleName = x.MIDDLENAME,
              lastName = x.LASTNAME,
              customerTypeId = (short)x.CUSTOMERTYPEID,
            dateOfBirth = (DateTime)x.DATEOFBIRTH
            });
        }

        public IQueryable<CustomerViewModels> GetExistingCustomerInformation(string customerCode)
        {
            return this.mainContext.TBL_CUSTOMER.Where (x => x.CUSTOMERCODE == customerCode).Select(x => new CustomerViewModels
            {
                customerCode = x.CUSTOMERCODE,
                firstName = x.FIRSTNAME,
                middleName = x.MIDDLENAME,
                lastName = x.LASTNAME,
                customerTypeId = (short)x.CUSTOMERTYPEID,
                dateOfBirth = (DateTime)x.DATEOFBIRTH
            });
        }
        public List<CustomerViewModels> GetIntegratedCustomerInformation(string searchTerm)
        {
            var data = new List<CustomerViewModels>();
            var setup = mainContext.TBL_SETUP_GLOBAL.FirstOrDefault();
            if (setup.USE_THIRD_PARTY_INTEGRATION)
            {
                //var casa = (from a in mainContext.TBL_CASA
                //            where a.PRODUCTACCOUNTNUMBER == searchTerm
                //           //select a.TBL_CUSTOMER.CUSTOMERCODE;
                //           select a.TBL_CUSTOMER.CUSTOMERCODE).ToList();
                //var customerCode  = mainContext.TBL_CASA.Where(a => a.PRODUCTACCOUNTNUMBER == searchTerm);

                var customerinfo  = (from a in mainContext.TBL_CASA join b in mainContext.TBL_CUSTOMER
                                    on a.CUSTOMERID equals b.CUSTOMERID where a.PRODUCTACCOUNTNUMBER == searchTerm
                                   select new CustomerViewModels
                                   {
                                       customerCode = b.CUSTOMERCODE,

                                   }).ToList();
                //var casa = mainContext.TBL_CASA.Where(a => a.PRODUCTACCOUNTNUMBER == searchTerm);
                //var existingCustomer = mainContext.TBL_CUSTOMER.Where(a => a.CUSTOMERCODE == customerCode).;

                if (customerinfo.Count > 0)
                {
                    //IQueryable<CustomerViewModels> allCustomers = null;

                    List<CustomerViewModels> allCustomers = new List<CustomerViewModels>();
                    allCustomers = GetExistingCustomerInformation(customerinfo[0].customerCode).ToList();
                    return allCustomers.ToList();
                }
                else
                { 
                //mainContext.TBL_CASA.Where(a => a.PRODUCTACCOUNTNUMBER == searchTerm);
                //var existingCustomer = mainContext.TBL_CUSTOMER.Where(a => a.CUSTOMERCODE == searchTerm);
                CustomerDetails customer = new CustomerDetails(mainContext);
                //customer.RunAsync().GetAwaiter().GetResult();
                //return await customer.GetCustomerByAccountNumber(searchTerm);//GetAllCustomers
                //return customer.GetAllCustomers(searchTerm).GetAwaiter().GetResult();

                //return  customer.GetCustomerByAccountNumber(searchTerm).GetAwaiter().GetResult();
                Task.Run(async () => { data = await customer.GetCustomerByAccountsNumber(searchTerm); }).GetAwaiter().GetResult();

                return data;
                }
                //return customer.GetCustomerByAccountNumber(searchTerm).GetAwaiter().GetResult();
            }
            else
            {
                IQueryable<CustomerViewModels> allCustomers = null;
                allCustomers = GetIntegratedCustomerInformation();
                if (allCustomers.ToList().Count() > 0)
                {
                    if (!String.IsNullOrEmpty(searchTerm))
                    {
                        allCustomers = allCustomers.Where(x =>
                        x.firstName.ToLower().Contains(searchTerm.ToLower())
                        || x.lastName.ToLower().Contains(searchTerm.ToLower())
                        || x.middleName.ToLower().Contains(searchTerm.ToLower())
                        || x.customerCode.ToLower().Contains(searchTerm.ToLower())
                        //|| x.customerAccountNo.ToLower().Contains(searchTerm.ToLower())
                        );
                    }
                    return allCustomers.ToList();
                }
                else
                    return null;
            }


        }

        //public async Task<CustomerIntegrationViewModels> GetIntegratedCustomerInformation(string searchTerm)
        //{
        //    var data = new CustomerIntegrationViewModels();
        //    var setup = mainContext.TBL_SETUP_GLOBAL.FirstOrDefault();
        //    if (setup.USE_THIRD_PARTY_INTEGRATION)
        //    {
        //        CustomerDetails customer = new CustomerDetails();
        //        //return await customer.GetCustomerByAccountNumber(searchTerm);GetAllCustomers
        //        //return customer.GetAllCustomers(searchTerm).GetAwaiter().GetResult();
        //        customer.RunAsync().GetAwaiter().GetResult();
        //        //return  customer.GetCustomerByAccountNumber(searchTerm).GetAwaiter().GetResult();
        //        Task.Run(async () => { data = await customer.GetAllCustomers(searchTerm); }).GetAwaiter().GetResult();

        //        return data;

        //        //return customer.GetCustomerByAccountNumber(searchTerm).GetAwaiter().GetResult();
        //    }
        //    else
        //    {
        //        IQueryable<CustomerViewModels> allCustomers = null;
        //        allCustomers = GetIntegratedCustomerInformation();
        //        if (allCustomers.ToList().Count() > 0)
        //        {
        //            if (!String.IsNullOrEmpty(searchTerm))
        //            {
        //                allCustomers = allCustomers.Where(x =>
        //                x.firstName.ToLower().Contains(searchTerm.ToLower())
        //                || x.lastName.ToLower().Contains(searchTerm.ToLower())
        //                || x.middleName.ToLower().Contains(searchTerm.ToLower())
        //                || x.customerCode.ToLower().Contains(searchTerm.ToLower())
        //                || x.customerAccountNo.ToLower().Contains(searchTerm.ToLower())
        //                );
        //            }
        //            //return allCustomers.ToList();
        //            return null;
        //        }
        //        else
        //            return null;
        //    }


        //}

        //public async Task <CustomerViewModels> GetIntegratedCustomerInformation(string searchTerm)
        //{
        //    var setup = mainContext.TBL_SETUP_GLOBAL.FirstOrDefault();
        //    if (setup.USE_THIRD_PARTY_INTEGRATION)
        //    {
        //        CustomerDetails customer = new CustomerDetails();
        //        return await customer.GetCustomerByAccountNumber2(searchTerm);
        //        //RunAsync().GetAwaiter().GetResult();
        //        //return  customer.GetCustomerByAccountNumber(searchTerm).GetAwaiter().GetResult();
        //    }
        //    else
        //    {
        //        IQueryable<CustomerViewModels> allCustomers = null;
        //        allCustomers = GetIntegratedCustomerInformation();
        //        if (allCustomers.ToList().Count() > 0)
        //        {
        //            if (!String.IsNullOrEmpty(searchTerm))
        //            {
        //                allCustomers = allCustomers.Where(x =>
        //                x.firstName.ToLower().Contains(searchTerm.ToLower())
        //                || x.lastName.ToLower().Contains(searchTerm.ToLower())
        //                || x.middleName.ToLower().Contains(searchTerm.ToLower())
        //                || x.customerCode.ToLower().Contains(searchTerm.ToLower())
        //                || x.customerAccountNo.ToLower().Contains(searchTerm.ToLower())
        //                );
        //            }
        //            return null;
        //        }
        //        else
        //            return null;
        //    }


        //}
    }
}
