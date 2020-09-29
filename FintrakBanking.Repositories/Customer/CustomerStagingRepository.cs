using FintrakBanking.Entities.Models;
using FintrakBanking.Entities.StagingModels;
using FintrakBanking.Interfaces.Customer;
using FintrakBanking.ViewModels.Customer;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FinTrakBanking.ThirdPartyIntegration.CustomerInfo;
using FintrakBanking.Common.CustomException;
using FintrakBanking.ViewModels.CASA;

namespace FintrakBanking.Repositories.Customer
{
   public class CustomerStagingRepository : ICustomerStagingRepository
    {
        private FinTrakBankingStagingContext context;
        private FinTrakBankingContext mainContext;
        private CustomerDetails _customer;

        public CustomerStagingRepository(FinTrakBankingStagingContext _context, FinTrakBankingContext _mainContext, CustomerDetails customer  )
        {
            context = _context;
            mainContext = _mainContext;
            this._customer = customer;
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
              //customerTypeId = (short)x.CUSTOMERTYPEID,
              dateOfBirth = (DateTime)x.DATEOFBIRTH,
              //customerBVN = x.CUSTOMERBVN,
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
                dateOfBirth = (DateTime)x.DATEOFBIRTH,
                customerBVN  = x.CUSTOMERBVN,
                
            });
        }

        public List<CustomerViewModels> GetIntegratedCustomerInformation(string searchTerm, bool isProspectConversion)
        {
            if (isProspectConversion) {
                CheckIfAccountNumberExist(searchTerm);
            }

            var data = new List<CustomerViewModels>();
            var setup = mainContext.TBL_SETUP_GLOBAL.FirstOrDefault();
            if (setup.USE_THIRD_PARTY_INTEGRATION)
            {
                var customerinfo  = (from a in mainContext.TBL_CASA
                                     join b in mainContext.TBL_CUSTOMER on a.CUSTOMERID equals b.CUSTOMERID
                                     where a.PRODUCTACCOUNTNUMBER == searchTerm && a.DELETED == false
                                     select new CustomerViewModels
                                     {
                                        customerCode = b.CUSTOMERCODE,
                                     }).ToList();

                if (customerinfo.Count > 0)
                {
                    List<CustomerViewModels> allCustomers = new List<CustomerViewModels>();
                    allCustomers = GetExistingCustomerInformation(customerinfo[0].customerCode).ToList();
                    return allCustomers.ToList();
                }
                else
                { 
                    Task.Run(async () => { data = await _customer.GetCustomerByAccountsNumber(searchTerm); }).GetAwaiter().GetResult();
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

        private void CheckIfAccountNumberExist(string accounNumber)
        {
            var account = (from a in mainContext.TBL_CASA
                           join b in mainContext.TBL_CUSTOMER on a.CUSTOMERID equals b.CUSTOMERID
                                where a.PRODUCTACCOUNTNUMBER == accounNumber && a.DELETED == false
                                select new CasaViewModel
                                {
                                    productAccountNumber = a.PRODUCTACCOUNTNUMBER,
                                    customerCode = b.CUSTOMERCODE
                                }).FirstOrDefault();

            if (account != null) { throw new SecureException($"Account Number is already attached to {account.customerCode} on Credit360!"); }
        }

        public bool MergeDuplicateCustomers(string accounNumber, string prospectiveCustomerCode, int createdBy)
        {
            var account = (from a in mainContext.TBL_CASA
                           join b in mainContext.TBL_CUSTOMER on a.CUSTOMERID equals b.CUSTOMERID
                           where a.PRODUCTACCOUNTNUMBER == accounNumber && a.DELETED == false
                           select new CasaViewModel
                           {
                               productAccountNumber = a.PRODUCTACCOUNTNUMBER,
                               customerCode = b.CUSTOMERCODE,
                               customerId = b.CUSTOMERID
                           }).FirstOrDefault();

            if (account != null) {
                var prospectiveCustomer = mainContext.TBL_CUSTOMER.Where(O => O.CUSTOMERCODE == prospectiveCustomerCode).FirstOrDefault();

                if (prospectiveCustomer != null) {
                    prospectiveCustomer.DELETED = true;
                    prospectiveCustomer.DELETEDBY = createdBy;
                    prospectiveCustomer.DATETIMEDELETED = DateTime.Now;

                    var application = mainContext.TBL_LOAN_APPLICATION.Where(O => O.CUSTOMERID == prospectiveCustomer.CUSTOMERID).FirstOrDefault();

                    if (application != null) {
                        application.CUSTOMERID = account.customerId;
                    }

                    var applicationDetails = mainContext.TBL_LOAN_APPLICATION_DETAIL.Where(O => O.CUSTOMERID == prospectiveCustomer.CUSTOMERID).ToList();

                    if (applicationDetails != null) {
                        foreach (var applicationDetail in applicationDetails)
                        {
                            applicationDetail.CUSTOMERID = account.customerId;
                        }
                    }

                    var prospectiveAccounts = mainContext.TBL_CASA.Where(O => O.CUSTOMERID == prospectiveCustomer.CUSTOMERID && O.DELETED == false).ToList();

                    if (prospectiveAccounts != null) {
                        foreach (var prospectiveAccount in prospectiveAccounts)
                        {
                            prospectiveAccount.DELETED = true;
                            prospectiveAccount.DELETEDBY = createdBy;
                            prospectiveAccount.DATETIMEDELETED = DateTime.Now;
                        }
                    }
                }

                mainContext.SaveChanges();
                return true;
            }

            return false;
        }

        
    }
}
