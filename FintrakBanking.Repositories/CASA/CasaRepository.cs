using FintrakBanking.Common.Enum;
using FintrakBanking.Entities.Models;
using FintrakBanking.Interfaces.CASA;
using FintrakBanking.ViewModels.CASA;
using FintrakBanking.ViewModels.Customer;
using System;
using System.Collections.Generic;
using System.Linq;
using FintrakBanking.Interfaces.CreditLimitValidations;
using FintrakBanking.ViewModels.Finance;
using System.Text;
using FinTrakBanking.ThirdPartyIntegration.Finacle;
using System.Threading.Tasks;

namespace FintrakBanking.Repositories.CASA
{
    public class CasaRepository : ICasaRepository
    {
        private FinTrakBankingContext context;
        private ICreditLimitValidationsRepository creditLimitRepo;

        public CasaRepository(FinTrakBankingContext _context, ICreditLimitValidationsRepository _creditLimitRepo)
        {
            this.context = _context;
            this.creditLimitRepo = _creditLimitRepo;
        }

        private bool SaveAll()
        {
            return this.context.SaveChanges() > 0;
        }

        public int GetCasaAccountId(string accountNumber, int companyId)
        {
            int value = 0;
            int.TryParse(accountNumber, out value);

            var CasaAccount = (context.TBL_CASA.Where(d => d.OLDPRODUCTACCOUNTNUMBER3 == accountNumber ||
                d.OLDPRODUCTACCOUNTNUMBER2 == accountNumber || d.OLDPRODUCTACCOUNTNUMBER1 == accountNumber ||
                d.CASAACCOUNTID == (value) || d.PRODUCTACCOUNTNUMBER == accountNumber && d.COMPANYID == companyId)
                ).AsQueryable().SingleOrDefault();
            return CasaAccount.CASAACCOUNTID;
        }

        public string GetAllCASAAccount(string casaAccountNumber, int companyId)
        {
            string accno = "";
            int casaAccountId = GetCasaAccountId(casaAccountNumber, companyId);
            var accounts = context.TBL_CASA.Where(x => x.CASAACCOUNTID == casaAccountId);

            foreach (var account in accounts)
            {
                if (account == null)
                {
                    accno += account.PRODUCTACCOUNTNUMBER+" - " + account.TBL_CURRENCY.CURRENCYCODE ;
                }
                else
                {
                    accno += "," + account.PRODUCTACCOUNTNUMBER + " - " + account.TBL_CURRENCY.CURRENCYCODE;
                }
                
            }
            return accno;

        }

        public CasaBalanceViewModel GetCASABalance(string casaAccountNumber, int companyId)
        {
            int casaAccountId = GetCasaAccountId(casaAccountNumber, companyId);
            var account = context.TBL_CASA.FirstOrDefault(x => x.CASAACCOUNTID == casaAccountId);

            return new CasaBalanceViewModel {  accountName = $" {account.TBL_CUSTOMER.LASTNAME } {account.TBL_CUSTOMER.FIRSTNAME} {account.TBL_CUSTOMER.MIDDLENAME} " ,
                availableBalance = account.AVAILABLEBALANCE, ledgerBalance = account.LEDGERBALANCE, accountNo = account.PRODUCTACCOUNTNAME, productName = account.TBL_PRODUCT.PRODUCTNAME  };
        }
        /// TODO: Implement server side filtering due to large number of records that may be returned
        public IEnumerable<CasaViewModel> FindAccount(string accountNumberOrName, int companyId)
        {
            return (from data in context.TBL_CASA join cust in context.TBL_CUSTOMER on data.CUSTOMERID equals cust.CUSTOMERID
                    where data.COMPANYID == companyId && (data.PRODUCTACCOUNTNUMBER.Contains(accountNumberOrName) || 
                    cust.CUSTOMERCODE.Contains(accountNumberOrName) || cust.FIRSTNAME.Contains(accountNumberOrName) ||
                 cust.LASTNAME.Contains(accountNumberOrName)) //orderby account.AccountCode ascending, account.AccountName ascending
                    select new CasaViewModel()
                    {
                        casaAccountId = data.CASAACCOUNTID,
                        productAccountNumber = data.PRODUCTACCOUNTNUMBER,
                        productAccountName = data.PRODUCTACCOUNTNAME,
                        customerId = data.CUSTOMERID,
                        customerCode = data.TBL_CUSTOMER.CUSTOMERCODE,
                        customerName = data.TBL_CUSTOMER.FIRSTNAME +" "+ data.TBL_CUSTOMER.LASTNAME,
                        productId = data.PRODUCTID,
                        productCode = data.TBL_PRODUCT.PRODUCTCODE,
                        productName = data.TBL_PRODUCT.PRODUCTNAME,
                        companyId = data.COMPANYID,
                        branchId = data.BRANCHID,
                        currency = data.TBL_CURRENCY.CURRENCYNAME,
                        branchCode = data.TBL_BRANCH.BRANCHCODE,
                        branchName = data.TBL_BRANCH.BRANCHNAME,
                        isCurrentAccount = data.ISCURRENTACCOUNT,
                        tenor = data.TENOR ?? 0,
                        interestRate = data.INTERESTRATE ?? 0,
                        effectiveDate = data.EFFECTIVEDATE ?? General.DefaultDate,
                        terminalDate = data.TERMINALDATE ?? General.DefaultDate,
                        actionBy = data.ACTIONBY ?? 0,
                        actionDate = data.ACTIONDATE ?? General.DefaultDate,
                        accountStatusId = data.ACCOUNTSTATUSID,
                        operationId = data.OPERATIONID ?? 0,
                        availableBalance = data.AVAILABLEBALANCE,
                        ledgerBalance = data.LEDGERBALANCE,
                        relationshipOfficerId = data.RELATIONSHIPOFFICERID ?? 0,
                        misCode = data.MISCODE,
                        overdraftAmount = data.OVERDRAFTAMOUNT ?? 0,
                        overdraftInterestRate = data.OVERDRAFTINTERESTRATE ?? 0,
                        overdraftExpiryDate = data.OVERDRAFTEXPIRYDATE ?? General.DefaultDate,
                        hasOverdraft = data.HASOVERDRAFT.HasValue == true ? data.HASOVERDRAFT.Value : false,
                        lienAmount = data.LIENAMOUNT,
                        hasLien = data.HASLIEN,
                        postNoStatusId = data.POSTNOSTATUSID,
                        oldProductAccountNumber1 = data.OLDPRODUCTACCOUNTNUMBER1,
                        oldProductAccountNumber2 = data.OLDPRODUCTACCOUNTNUMBER2,
                        oldProductAccountNumber3 = data.OLDPRODUCTACCOUNTNUMBER3,
                        //aprovalStatusId = data.AprovalStatusId.HasValue == true ? (short) data.AprovalStatusId.Value : (short) 0
                        aprovalStatusId = data.APROVALSTATUSID,
                    });
        }

        public CasaViewModel GetAccount(int casaAccountId)
        {
            return (from data in context.TBL_CASA
                    where data.CASAACCOUNTID == casaAccountId //orderby account.AccountCode ascending, account.AccountName ascending
                    select new CasaViewModel()
                    {
                        casaAccountId = data.CASAACCOUNTID,
                        productAccountNumber = data.PRODUCTACCOUNTNUMBER,
                        productAccountName = data.PRODUCTACCOUNTNAME,
                        customerId = data.CUSTOMERID,
                        customerCode = data.TBL_CUSTOMER.CUSTOMERCODE,
                        productId = data.PRODUCTID,
                        productCode = data.TBL_PRODUCT.PRODUCTCODE,
                        productName = data.TBL_PRODUCT.PRODUCTNAME,
                        companyId = data.COMPANYID,
                        branchId = data.BRANCHID,
                        branchCode = data.TBL_BRANCH.BRANCHCODE,
                        branchName = data.TBL_BRANCH.BRANCHNAME,

                        isCurrentAccount = data.ISCURRENTACCOUNT,
                        tenor = data.TENOR ?? 0,
                        interestRate = data.INTERESTRATE ?? 0,
                        effectiveDate = data.EFFECTIVEDATE ?? General.DefaultDate,
                        terminalDate = data.TERMINALDATE ?? General.DefaultDate,
                        actionBy = data.ACTIONBY ?? 0,
                        actionDate = data.ACTIONDATE ?? General.DefaultDate,
                        accountStatusId = data.ACCOUNTSTATUSID,
                        operationId = data.OPERATIONID ?? 0,
                        availableBalance = data.AVAILABLEBALANCE,
                        ledgerBalance = data.LEDGERBALANCE,
                        
                        relationshipOfficerId = data.RELATIONSHIPOFFICERID ?? 0,
                        misCode = data.MISCODE,

                        overdraftAmount = data.OVERDRAFTAMOUNT ?? 0,
                        overdraftInterestRate = data.OVERDRAFTINTERESTRATE ?? 0,
                        overdraftExpiryDate = data.OVERDRAFTEXPIRYDATE ?? General.DefaultDate,
                        hasOverdraft = data.HASOVERDRAFT.HasValue == true ? data.HASOVERDRAFT.Value : false,
                        lienAmount = data.LIENAMOUNT,
                        hasLien = data.HASLIEN,
                        postNoStatusId = data.POSTNOSTATUSID,
                        oldProductAccountNumber1 = data.OLDPRODUCTACCOUNTNUMBER1,
                        oldProductAccountNumber2 = data.OLDPRODUCTACCOUNTNUMBER2,
                        oldProductAccountNumber3 = data.OLDPRODUCTACCOUNTNUMBER3,
                        //aprovalStatusId = data.AprovalStatusId.HasValue == true ? (short) data.AprovalStatusId.Value : (short) 0
                        aprovalStatusId = data.APROVALSTATUSID,

                        refreshBatchId = data.REFRESHBATCHID,
                        lastRefreshDatetime = data.LASTREFRESHDATETIME,
                        createdBy = data.CREATEDBY ?? 0,
                        lastUpdatedBy = data.LASTUPDATEDBY ?? 0,
                        dateTimeCreated = (DateTime)data.DATETIMECREATED,
                        dateTimeUpdated = data.DATETIMEUPDATED,
                        deleted = data.DELETED,
                        deletedBy = data.DELETEDBY,
                        dateTimeDeleted = data.DATETIMEDELETED
                    }).FirstOrDefault();
        }

        public IEnumerable<dynamic> GetAllCustomerAccount(int customerId,int applicationTypeId , int companyId)
        {
            IEnumerable<dynamic> data = null;
            if (applicationTypeId == (int)LoanTypeEnum.CustomerGroup)
            {
                //data = (from a in  context.TBL_CUSTOMER_GROUP_MAPPING join b in context.TBL_CASA on a.CUSTOMERID equals b.CUSTOMERID 
                //            where a.CUSTOMERID == customerId && a.TBL_CUSTOMER.COMPANYID == companyId  //orderby account.AccountCode ascending, account.AccountName ascending
                //            select new
                //            {
                //                casaAccountId = b.CASAACCOUNTID,
                //                productAccountNumber = b.PRODUCTACCOUNTNUMBER + "(" + b.PRODUCTACCOUNTNAME + " - " + b.TBL_CURRENCY.CURRENCYCODE + ")",
                //                productAccountName = b.PRODUCTACCOUNTNAME,
                //                availableBalance = b.AVAILABLEBALANCE
                //            }).Distinct();

                data = (from a in context.TBL_CUSTOMER_GROUP_MAPPING
                        join b in context.TBL_CASA on a.CUSTOMERID equals b.CUSTOMERID
                        where a.CUSTOMERGROUPID == customerId && a.TBL_CUSTOMER.COMPANYID == companyId  //orderby account.AccountCode ascending, account.AccountName ascending
                        select new
                        {
                            casaAccountId = b.CASAACCOUNTID,
                            productAccountNumber = b.PRODUCTACCOUNTNUMBER + "(" + b.PRODUCTACCOUNTNAME + " - " + b.TBL_CURRENCY.CURRENCYCODE + ")",
                            productAccountName = b.PRODUCTACCOUNTNAME,
                            availableBalance = b.AVAILABLEBALANCE
                        }).Distinct();
            }

            if (applicationTypeId == (int)LoanTypeEnum.Single)
            {
                // data = new List<CasaViewModel>();
                //var setup = context.TBL_SETUP_GLOBAL.FirstOrDefault();
                //if (setup.USE_THIRD_PARTY_INTEGRATION)
                //{
                //    var customerinfo = (from a in context.TBL_CUSTOMER
                //                        where a.CUSTOMERID == customerId
                //                        select new CasaViewModel
                //                        {
                //                            customerCode = a.CUSTOMERCODE,
                //                        }).ToList();
                //    if (customerinfo.Count > 0)
                //    {
                //        CustomerDetails customer = new CustomerDetails();
                //        Task.Run(async () => { data = await customer.GetCustomerAccountsBalance(customerinfo[0].customerCode); }).GetAwaiter().GetResult();
                //    }
                //    return data;
                //}
                //else
                //{
                    data = (from a in context.TBL_CASA
                        where a.CUSTOMERID == customerId && a.COMPANYID == companyId  //orderby account.AccountCode ascending, account.AccountName ascending
                        select new

                        {
                            casaAccountId = a.CASAACCOUNTID,
                            productAccountNumber = a.PRODUCTACCOUNTNUMBER + "(" + a.PRODUCTACCOUNTNAME + " - " + a.TBL_CURRENCY.CURRENCYCODE + ")",
                            productAccountName = a.PRODUCTACCOUNTNAME,
                            availableBalance = a.AVAILABLEBALANCE
                        }).Distinct();
                //}
                return data;
            }
          


            return data;
        }

        public IEnumerable<CasaViewModel> GetAccountByCustomerId(int customerId)
        {
            return (from data in context.TBL_CASA
                    where data.CUSTOMERID == customerId //orderby account.AccountCode ascending, account.AccountName ascending
                    select new CasaViewModel()
                    {
                        casaAccountId = data.CASAACCOUNTID,
                        productAccountNumber = data.PRODUCTACCOUNTNUMBER,
                        productAccountName = data.PRODUCTACCOUNTNAME,
                        customerId = data.CUSTOMERID,
                        customerCode = data.TBL_CUSTOMER.CUSTOMERCODE,
                        productId = data.PRODUCTID,
                        productCode = data.TBL_PRODUCT.PRODUCTCODE,
                        productName = data.TBL_PRODUCT.PRODUCTNAME,
                        companyId = data.COMPANYID,
                        branchId = data.BRANCHID,
                        branchCode = data.TBL_BRANCH.BRANCHCODE,
                        branchName = data.TBL_BRANCH.BRANCHNAME,
                        isCurrentAccount = data.ISCURRENTACCOUNT,
                        tenor = data.TENOR ?? 0,
                        interestRate = data.INTERESTRATE ?? 0,
                        effectiveDate = data.EFFECTIVEDATE ?? General.DefaultDate,
                        terminalDate = data.TERMINALDATE ?? General.DefaultDate,
                        actionBy = data.ACTIONBY ?? 0,
                        actionDate = data.ACTIONDATE ?? General.DefaultDate,
                        accountStatusId = data.ACCOUNTSTATUSID,
                        operationId = data.OPERATIONID ?? 0,
                        availableBalance = data.AVAILABLEBALANCE,
                        ledgerBalance = data.LEDGERBALANCE,

                        relationshipOfficerId = data.RELATIONSHIPOFFICERID ?? 0,
                        relationshipManagerId = data.RELATIONSHIPMANAGERID ?? 0,
                        misCode = data.MISCODE,

                        overdraftAmount = data.OVERDRAFTAMOUNT ?? 0,
                        overdraftInterestRate = data.OVERDRAFTINTERESTRATE ?? 0,
                        overdraftExpiryDate = data.OVERDRAFTEXPIRYDATE ?? General.DefaultDate,
                        hasOverdraft = data.HASOVERDRAFT.HasValue == true ? data.HASOVERDRAFT.Value : false,
                        lienAmount = data.LIENAMOUNT,
                        hasLien = data.HASLIEN,
                        postNoStatusId = data.POSTNOSTATUSID,
                        oldProductAccountNumber1 = data.OLDPRODUCTACCOUNTNUMBER1,
                        oldProductAccountNumber2 = data.OLDPRODUCTACCOUNTNUMBER2,
                        oldProductAccountNumber3 = data.OLDPRODUCTACCOUNTNUMBER3,
                        //aprovalStatusId = data.AprovalStatusId.HasValue == true ? (short) data.AprovalStatusId.Value : (short) 0
                        aprovalStatusId = data.APROVALSTATUSID,

                        //refreshBatchId = data.RefreshBatchId,
                        //lastRefreshDatetime = data.LastRefreshDatetime,
                        //createdBy = data.CreatedBy ?? 0,
                        //lastUpdatedBy = data.LastUpdatedBy ?? 0,
                        //dateTimeCreated = data.DateTimeCreated,
                        //dateTimeUpdated = data.DateTimeUpdated,
                        //deleted = data.Deleted,
                        //deletedBy = data.DeletedBy,
                        //dateTimeDeleted = data.DateTimeDeleted
                    });
        }

        public IQueryable<CustomerSearchVM> SearchCustomer(int customerTypeId, int companyId, string searchQuery)
        {
            if (customerTypeId == 0) return null;
            IQueryable<CustomerSearchVM> allCustomer = null;

            if (!string.IsNullOrWhiteSpace(searchQuery))
            {
                searchQuery = searchQuery.ToLower();
            }
            if (customerTypeId < 3)
            {
                allCustomer = from cust in context.TBL_CUSTOMER
                              join acc in context.TBL_CASA on cust.CUSTOMERID equals acc.CUSTOMERID
                              join prod in context.TBL_PRODUCT on acc.PRODUCTID equals prod.PRODUCTID
                              where cust.DELETED == false && cust.COMPANYID == companyId
                              select new CustomerSearchVM
                              {
                                  customerId = cust.CUSTOMERID,
                                  accountNumber = acc.PRODUCTACCOUNTNUMBER,
                                  customerCode = cust.CUSTOMERCODE,
                                  firstName = cust.FIRSTNAME,
                                  lastName = cust.LASTNAME,
                                  middleName = cust.MAIDENNAME,
                                  relationshipManagerId = acc.RELATIONSHIPMANAGERID ?? 0,
                                  relationshipOfficerId = acc.RELATIONSHIPOFFICERID ?? 0
                              };

                if (!string.IsNullOrWhiteSpace(searchQuery.Trim()))
                {
                    allCustomer = allCustomer
                        .Where(x => x.firstName.ToLower().Contains(searchQuery)
                        || x.middleName.ToLower().Contains(searchQuery)
                        || x.lastName.ToLower().Contains(searchQuery)
                        || x.accountNumber.Contains(searchQuery));
                }
            }
            else
            {
                allCustomer = from cg in context.TBL_CUSTOMER_GROUP
                              join gm in context.TBL_CUSTOMER_GROUP_MAPPING
                              on cg.CUSTOMERGROUPID equals gm.CUSTOMERGROUPID
                              join casa in context.TBL_CASA
                              on gm.CUSTOMERID equals casa.CUSTOMERID
                              join prod in context.TBL_PRODUCT on casa.PRODUCTID equals prod.PRODUCTID
                              where cg.DELETED == false && gm.DELETED == false && casa.DELETED == false
                              select new CustomerSearchVM
                              {
                                  customerId = cg.CUSTOMERGROUPID,
                                  customerCode = cg.GROUPCODE,
                                  firstName = cg.GROUPNAME,
                                  lastName = string.Empty,
                                  accountNumber = casa.PRODUCTACCOUNTNUMBER,
                                  relationshipManagerId = casa.RELATIONSHIPMANAGERID ?? 0,
                                  relationshipOfficerId = casa.RELATIONSHIPOFFICERID ?? 0
                              };

                if (!string.IsNullOrWhiteSpace(searchQuery.Trim()))
                {
                    allCustomer = allCustomer
                        .Where(x => x.firstName.ToLower().Contains(searchQuery)
                        || x.middleName.ToLower().Contains(searchQuery)
                        || x.lastName.ToLower().Contains(searchQuery)
                        || x.accountNumber.Contains(searchQuery));
                }
            }

            return allCustomer;
        }

        public IEnumerable<GroupCustomerMembersViewModel> GetGroupMembersByGroupId(int customerId, int companyId)
        {
            var customerGroupMapping = from b in context.TBL_CASA 
                                       where b.CUSTOMERID == customerId &&  b.DELETED == false && b.COMPANYID == companyId
                                       select new GroupCustomerMembersViewModel
                                       {
                                           customerId = b.CUSTOMERID,                                          
                                           customerCode = b.TBL_CUSTOMER.CUSTOMERCODE,
                                           lastName = b.TBL_CUSTOMER.LASTNAME,
                                           firstName = b.TBL_CUSTOMER.FIRSTNAME,
                                            customerTypeId = (short) b.TBL_CUSTOMER .CUSTOMERTYPEID,
                                             customerType = b.TBL_CUSTOMER.TBL_CUSTOMER_TYPE .NAME ,

                                       
                                       };

            return customerGroupMapping;
        }

        private IQueryable<CasaCustomerSearchViewModel> GetAllAccounts()
        {
            var data = (from casa in context.TBL_CASA
                        join cust in context.TBL_CUSTOMER on casa.CUSTOMERID equals cust.CUSTOMERID
                        //join prod in context.tbl_Product on casa.ProductId equals prod.ProductId
                        join sector in context.TBL_SUB_SECTOR on cust.SUBSECTORID equals sector.SUBSECTORID
                        join custGroup in context.TBL_CUSTOMER_GROUP_MAPPING on cust.CUSTOMERID equals custGroup.CUSTOMERID into cGroup
                        from custGroup in cGroup.DefaultIfEmpty()
                        select new CasaCustomerSearchViewModel()
                        {
                            casaAccountId = casa.CASAACCOUNTID,
                            productAccountNumber = casa.PRODUCTACCOUNTNUMBER,
                            productAccountName = casa.PRODUCTACCOUNTNAME,
                            customerId = casa.CUSTOMERID,
                            customerCode = cust.CUSTOMERCODE,
                            accountHolder = cust.FIRSTNAME + " " + cust.LASTNAME,
                            productId = casa.TBL_PRODUCT.PRODUCTID,
                            productCode = casa.TBL_PRODUCT.PRODUCTCODE,
                            productName = casa.TBL_PRODUCT.PRODUCTNAME,
                            productClassId = casa.TBL_PRODUCT.PRODUCTCLASSID,
                            productClassName = casa.TBL_PRODUCT.TBL_PRODUCT_CLASS.PRODUCTCLASSNAME,
                            companyId = casa.COMPANYID,
                            branchId = casa.BRANCHID,
                            branchCode = casa.TBL_BRANCH.BRANCHCODE,
                            branchName = casa.TBL_BRANCH.BRANCHNAME,
                            relationshipOfficerId = casa.RELATIONSHIPOFFICERID ?? 0,
                            relationshipManagerId = casa.RELATIONSHIPMANAGERID ?? 0,
                            subSectorId = sector.SUBSECTORID,
                            subSectorName = sector.NAME,
                            customerSectorId = sector.TBL_SECTOR.SECTORID,
                            customerSectorName = sector.TBL_SECTOR.NAME,
                            customerGroupId = custGroup.CUSTOMERGROUPID,
                            customerGroupName = custGroup.TBL_CUSTOMER_GROUP.GROUPNAME ?? "None",
                            taxIdentificationNumber = cust.TAXNUMBER,
                            registrationNumber = cust.TBL_CUSTOMER_COMPANYINFOMATION.FirstOrDefault(x => x.CUSTOMERID == cust.CUSTOMERID).REGISTRATIONNUMBER,
                            isBlackList = context.TBL_CUSTOMER_BLACKLIST.Any(x => x.CUSTOMERCODE == cust.CUSTOMERCODE),
                            isOnWatchList = context.TBL_LOAN_PRUDENTIALGUIDELINE.Any(x => x.TBL_LOAN.Any(l => l.CUSTOMERID == cust.CUSTOMERID && l.EXT_PRUDENT_GUIDELINE_STATUSID == (int)LoanPrudentialStatusEnum.WatchList)),
                            isCamsol = context.TBL_LOAN_CAMSOL.Any(x => context.TBL_LOAN.Any(l => l.TERMLOANID == x.LOANID && l.CUSTOMERID == cust.CUSTOMERID)),
                            customerTypeId = cust.CUSTOMERTYPEID,
                            customerTypeName = cust.TBL_CUSTOMER_TYPE.NAME,
                            completedInformation = cust.ACCOUNTCREATIONCOMPLETE,
                            customerBvnInformation = context.TBL_CUSTOMER_BVN.Where(b => b.CUSTOMERID == casa.CUSTOMERID).Select(b => new CustomerBvnViewModels()
                            {
                                bankVerificationNumber = b.BANKVERIFICATIONNUMBER,
                                customerBvnid = b.CUSTOMERBVNID,
                                firstname = b.FIRSTNAME,
                                isValidBvn = b.ISVALIDBVN,
                                isPoliticallyExposed = b.ISPOLITICALLYEXPOSED,
                                surname = b.SURNAME
                            }).ToList(),
                            customerCompanyDirectors = context.TBL_CUSTOMER_COMPANY_DIRECTOR
                            .Where(s => s.CUSTOMERID == casa.CUSTOMERID && s.COMPANYDIRECTORTYPEID == (short)CompanyDirectorTypeEnum.BoardMember)
                            .Select(s => new CustomerCompanyDirectorsViewModels()
                            {
                                bankVerificationNumber = s.CUSTOMERBVN,
                                companyDirectorTypeId = s.COMPANYDIRECTORTYPEID,
                                companyDirectorTypeName = s.TBL_CUSTOMER_COMPANY_DIREC_TYP.COMPANYDIRECTORYTYPENAME,
                                customerId = s.CUSTOMERID,
                                firstname = s.FIRSTNAME,
                                surname = s.SURNAME
                            }).ToList(),
                            customerCompanyShareholders = context.TBL_CUSTOMER_COMPANY_DIRECTOR
                            .Where(s => s.CUSTOMERID == casa.CUSTOMERID && s.COMPANYDIRECTORTYPEID == (short)CompanyDirectorTypeEnum.Shareholder)
                            .Select(s => new CustomerCompanyShareholdersViewModels()
                            {
                                bankVerificationNumber = s.CUSTOMERBVN,
                                companyDirectorTypeId = s.COMPANYDIRECTORTYPEID,
                                companyDirectorTypeName = s.TBL_CUSTOMER_COMPANY_DIREC_TYP.COMPANYDIRECTORYTYPENAME,
                                customerId = s.CUSTOMERID,
                                firstname = s.FIRSTNAME,
                                surname = s.SURNAME
                            }).ToList(),
                            customerClients = context.TBL_CUSTOMER_CLIENT_SUPPLIER.Where(cs => cs.CUSTOMERID == casa.CUSTOMERID &&
                            cs.CLIENT_SUPPLIERTYPEID == (short)CompanyClientOrSupplierTypeEnum.Client)
                            .Select(cs => new CustomerClientOrSupplierViewModels()
                            {
                                client_SupplierId = cs.CLIENT_SUPPLIERID,
                                clientOrSupplierName = cs.FIRSTNAME + " " + cs.LASTNAME,
                                firstName = cs.FIRSTNAME,
                                middleName = cs.MIDDLENAME,
                                lastName = cs.LASTNAME,
                                client_SupplierAddress = cs.ADDRESS,
                                client_SupplierPhoneNumber = cs.PHONENUMBER,
                                client_SupplierEmail = cs.EMAILADDRESS,
                                client_SupplierTypeId = cs.CLIENT_SUPPLIERTYPEID,
                                client_SupplierTypeName = cs.TBL_CUSTOMER_CLIENT_SUPPLR_TYP.CLIENT_SUPPLIERTYPENAME
                            }).ToList(),
                            customerSuppliers = context.TBL_CUSTOMER_CLIENT_SUPPLIER.Where(cs => cs.CUSTOMERID == casa.CUSTOMERID &&
                            cs.CLIENT_SUPPLIERTYPEID == (short)CompanyClientOrSupplierTypeEnum.Supplier)
                             .Select(cs => new CustomerSupplierViewModels()
                             {
                                 client_SupplierId = cs.CLIENT_SUPPLIERID,
                                 clientOrSupplierName = cs.FIRSTNAME + " " + cs.LASTNAME,
                                 firstName = cs.FIRSTNAME,
                                 middleName = cs.MIDDLENAME,
                                 lastName = cs.LASTNAME,
                                 client_SupplierAddress = cs.ADDRESS,
                                 client_SupplierPhoneNumber = cs.PHONENUMBER,
                                 client_SupplierEmail = cs.EMAILADDRESS,
                                 client_SupplierTypeId = cs.CLIENT_SUPPLIERTYPEID,
                                 client_SupplierTypeName = cs.TBL_CUSTOMER_CLIENT_SUPPLR_TYP.CLIENT_SUPPLIERTYPENAME
                             }).ToList(),
                        });

            return data;
        }

        public IQueryable<CasaCustomerSearchViewModel> SearchForCustomerAccount(int companyId, string searchQuery, int customerTypeId)
        {
            IQueryable<CasaCustomerSearchViewModel> allCustomers = null;

            if (!string.IsNullOrWhiteSpace(searchQuery))
            {
                searchQuery = searchQuery.ToLower();
            }

            if (!string.IsNullOrWhiteSpace(searchQuery.Trim()))
            {
                allCustomers = GetAllAccounts()
                    .Where(c => c.companyId == companyId)
                    .Where(ct => ct.customerTypeId == customerTypeId)
                    .Where(x => x.accountHolder.Contains(searchQuery)
               || x.customerCode.Contains(searchQuery)
               || x.productAccountNumber.Contains(searchQuery)
                ).GroupBy(c => c.customerId).Select(g => g.FirstOrDefault()).Take(10);
            }

            //foreach (var item in allCustomers)
            //{
            //    item.isBlackList = creditLimitRepo.ValidateBlackList(item.customerId) > 0;
            //    item.isOnWatchList = creditLimitRepo.ValidateWatchList(item.customerId) > 0;
            //    item.isCamsol = creditLimitRepo.ValidateCamsol(item.customerId) > 0;
            //}

            return allCustomers;
        }

        public CasaCustomerSearchViewModel GetCustomerAccountDetailsById(int customerId)
        {
            var data = GetAllAccounts().FirstOrDefault(x => x.customerId == customerId);

            if (data != null)
            {
                return data;
            }

            return new CasaCustomerSearchViewModel { };
        }

        public IEnumerable<dynamic> GetAllCustomerAccountByCustomerId(int customerId, int companyId)
        {
         var   data = (from a in context.TBL_CASA
                    where a.CUSTOMERID == customerId && a.COMPANYID == companyId  //orderby account.AccountCode ascending, account.AccountName ascending
                    select new

                    {
                        casaAccountId = a.CASAACCOUNTID,
                        productAccountNumber = a.PRODUCTACCOUNTNUMBER + "(" + a.PRODUCTACCOUNTNAME + " - " + a.TBL_CURRENCY.CURRENCYCODE + ")",
                        productAccountName = a.PRODUCTACCOUNTNAME,
                        availableBalance = a.AVAILABLEBALANCE
                    });
            return data.ToList();
        }
    }
}