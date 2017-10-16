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

            var CasaAccount = (context.tbl_CASA.Where(d => d.OldProductAccountNumber3 == accountNumber ||
                d.OldProductAccountNumber2 == accountNumber || d.OldProductAccountNumber1 == accountNumber ||
                d.CasaAccountId == (value) || d.ProductAccountNumber == accountNumber && d.CompanyId == companyId)
                ).AsQueryable().SingleOrDefault();
            return CasaAccount.CasaAccountId;
        }

        public CasaBalanceViewModel GetCASABalance(string casaAccountNumber, int companyId)
        {
            int casaAccountId = GetCasaAccountId(casaAccountNumber, companyId);
            var account = context.tbl_CASA.FirstOrDefault(x => x.CasaAccountId == casaAccountId);

            return new CasaBalanceViewModel {  accountName = $" {account.tbl_Customer.LastName } {account.tbl_Customer.FirstName} {account.tbl_Customer.MiddleName} " ,
                availableBalance = account.AvailableBalance, ledgerBalance = account.LedgerBalance, accountNo = account.ProductAccountName, productName = account.tbl_Product.ProductName  };
        }
        /// TODO: Implement server side filtering due to large number of records that may be returned
        public IEnumerable<CasaViewModel> FindAccount(string accountNumberOrName, int companyId)
        {
            return (from data in context.tbl_CASA join cust in context.tbl_Customer on data.CustomerId equals cust.CustomerId
                    where data.CompanyId == companyId && (data.ProductAccountNumber.Contains(accountNumberOrName) || 
                    cust.CustomerCode.Contains(accountNumberOrName) || cust.FirstName.Contains(accountNumberOrName) ||
                 cust.LastName.Contains(accountNumberOrName)) //orderby account.AccountCode ascending, account.AccountName ascending
                    select new CasaViewModel()
                    {
                        casaAccountId = data.CasaAccountId,
                        productAccountNumber = data.ProductAccountNumber,
                        productAccountName = data.ProductAccountName,
                        customerId = data.CustomerId,
                        customerCode = data.tbl_Customer.CustomerCode,
                        customerName = data.tbl_Customer.FirstName +" "+ data.tbl_Customer.LastName,
                        productId = data.ProductId,
                        productCode = data.tbl_Product.ProductCode,
                        productName = data.tbl_Product.ProductName,
                        companyId = data.CompanyId,
                        branchId = data.BranchId,
                        currency = data.tbl_Currency.CurrencyName,
                        branchCode = data.tbl_Branch.BranchCode,
                        branchName = data.tbl_Branch.BranchName,
                        isCurrentAccount = data.IsCurrentAccount,
                        tenor = data.Tenor ?? 0,
                        interestRate = data.InterestRate ?? 0,
                        effectiveDate = data.EffectiveDate ?? General.DefaultDate,
                        terminalDate = data.TerminalDate ?? General.DefaultDate,
                        actionBy = data.ActionBy ?? 0,
                        actionDate = data.ActionDate ?? General.DefaultDate,
                        accountStatusId = data.AccountStatusId,
                        operationId = data.OperationId ?? 0,
                        availableBalance = data.AvailableBalance,
                        ledgerBalance = data.LedgerBalance,
                        relationshipOfficerId = data.RelationshipOfficerId ?? 0,
                        misCode = data.MISCode,
                        overdraftAmount = data.OverdraftAmount ?? 0,
                        overdraftInterestRate = data.OverdraftInterestRate ?? 0,
                        overdraftExpiryDate = data.OverdraftExpiryDate ?? General.DefaultDate,
                        hasOverdraft = data.HasOverdraft.HasValue == true ? data.HasOverdraft.Value : false,
                        lienAmount = data.LienAmount,
                        hasLien = data.HasLien,
                        postNoStatusId = data.PostNoStatusId,
                        oldProductAccountNumber1 = data.OldProductAccountNumber1,
                        oldProductAccountNumber2 = data.OldProductAccountNumber2,
                        oldProductAccountNumber3 = data.OldProductAccountNumber3,
                        //aprovalStatusId = data.AprovalStatusId.HasValue == true ? (short) data.AprovalStatusId.Value : (short) 0
                        aprovalStatusId = data.AprovalStatusId,
                    });
        }

        public CasaViewModel GetAccount(int casaAccountId)
        {
            return (from data in context.tbl_CASA
                    where data.CasaAccountId == casaAccountId //orderby account.AccountCode ascending, account.AccountName ascending
                    select new CasaViewModel()
                    {
                        casaAccountId = data.CasaAccountId,
                        productAccountNumber = data.ProductAccountNumber,
                        productAccountName = data.ProductAccountName,
                        customerId = data.CustomerId,
                        customerCode = data.tbl_Customer.CustomerCode,
                        productId = data.ProductId,
                        productCode = data.tbl_Product.ProductCode,
                        productName = data.tbl_Product.ProductName,
                        companyId = data.CompanyId,
                        branchId = data.BranchId,
                        branchCode = data.tbl_Branch.BranchCode,
                        branchName = data.tbl_Branch.BranchName,

                        isCurrentAccount = data.IsCurrentAccount,
                        tenor = data.Tenor ?? 0,
                        interestRate = data.InterestRate ?? 0,
                        effectiveDate = data.EffectiveDate ?? General.DefaultDate,
                        terminalDate = data.TerminalDate ?? General.DefaultDate,
                        actionBy = data.ActionBy ?? 0,
                        actionDate = data.ActionDate ?? General.DefaultDate,
                        accountStatusId = data.AccountStatusId,
                        operationId = data.OperationId ?? 0,
                        availableBalance = data.AvailableBalance,
                        ledgerBalance = data.LedgerBalance,

                        relationshipOfficerId = data.RelationshipOfficerId ?? 0,
                        misCode = data.MISCode,

                        overdraftAmount = data.OverdraftAmount ?? 0,
                        overdraftInterestRate = data.OverdraftInterestRate ?? 0,
                        overdraftExpiryDate = data.OverdraftExpiryDate ?? General.DefaultDate,
                        hasOverdraft = data.HasOverdraft.HasValue == true ? data.HasOverdraft.Value : false,
                        lienAmount = data.LienAmount,
                        hasLien = data.HasLien,
                        postNoStatusId = data.PostNoStatusId,
                        oldProductAccountNumber1 = data.OldProductAccountNumber1,
                        oldProductAccountNumber2 = data.OldProductAccountNumber2,
                        oldProductAccountNumber3 = data.OldProductAccountNumber3,
                        //aprovalStatusId = data.AprovalStatusId.HasValue == true ? (short) data.AprovalStatusId.Value : (short) 0
                        aprovalStatusId = data.AprovalStatusId,

                        refreshBatchId = data.RefreshBatchId,
                        lastRefreshDatetime = data.LastRefreshDatetime,
                        createdBy = data.CreatedBy ?? 0,
                        lastUpdatedBy = data.LastUpdatedBy ?? 0,
                        dateTimeCreated = (DateTime)data.DateTimeCreated,
                        dateTimeUpdated = data.DateTimeUpdated,
                        deleted = data.Deleted,
                        deletedBy = data.DeletedBy,
                        dateTimeDeleted = data.DateTimeDeleted
                    }).FirstOrDefault();
        }

        public IEnumerable<dynamic> GetAllCustomerAccountByCustomerId(int customerId, int companyId)
        {
            var data = (from a in context.tbl_CASA
                        where a.CustomerId == customerId && a.CompanyId == companyId //orderby account.AccountCode ascending, account.AccountName ascending
                        select new

                        {
                            casaAccountId = a.CasaAccountId,
                            productAccountNumber = a.ProductAccountNumber + "(" + a.ProductAccountName + ")",
                            productAccountName = a.ProductAccountName,
                            availableBalance = a.AvailableBalance
                        });
            return data;
        }

        public IEnumerable<CasaViewModel> GetAccountByCustomerId(int customerId)
        {
            return (from data in context.tbl_CASA
                    where data.CustomerId == customerId //orderby account.AccountCode ascending, account.AccountName ascending
                    select new CasaViewModel()
                    {
                        casaAccountId = data.CasaAccountId,
                        productAccountNumber = data.ProductAccountNumber,
                        productAccountName = data.ProductAccountName,
                        customerId = data.CustomerId,
                        customerCode = data.tbl_Customer.CustomerCode,
                        productId = data.ProductId,
                        productCode = data.tbl_Product.ProductCode,
                        productName = data.tbl_Product.ProductName,
                        companyId = data.CompanyId,
                        branchId = data.BranchId,
                        branchCode = data.tbl_Branch.BranchCode,
                        branchName = data.tbl_Branch.BranchName,
                        isCurrentAccount = data.IsCurrentAccount,
                        tenor = data.Tenor ?? 0,
                        interestRate = data.InterestRate ?? 0,
                        effectiveDate = data.EffectiveDate ?? General.DefaultDate,
                        terminalDate = data.TerminalDate ?? General.DefaultDate,
                        actionBy = data.ActionBy ?? 0,
                        actionDate = data.ActionDate ?? General.DefaultDate,
                        accountStatusId = data.AccountStatusId,
                        operationId = data.OperationId ?? 0,
                        availableBalance = data.AvailableBalance,
                        ledgerBalance = data.LedgerBalance,

                        relationshipOfficerId = data.RelationshipOfficerId ?? 0,
                        relationshipManagerId = data.RelationshipManagerId ?? 0,
                        misCode = data.MISCode,

                        overdraftAmount = data.OverdraftAmount ?? 0,
                        overdraftInterestRate = data.OverdraftInterestRate ?? 0,
                        overdraftExpiryDate = data.OverdraftExpiryDate ?? General.DefaultDate,
                        hasOverdraft = data.HasOverdraft.HasValue == true ? data.HasOverdraft.Value : false,
                        lienAmount = data.LienAmount,
                        hasLien = data.HasLien,
                        postNoStatusId = data.PostNoStatusId,
                        oldProductAccountNumber1 = data.OldProductAccountNumber1,
                        oldProductAccountNumber2 = data.OldProductAccountNumber2,
                        oldProductAccountNumber3 = data.OldProductAccountNumber3,
                        //aprovalStatusId = data.AprovalStatusId.HasValue == true ? (short) data.AprovalStatusId.Value : (short) 0
                        aprovalStatusId = data.AprovalStatusId,

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
                allCustomer = from cust in context.tbl_Customer
                              join acc in context.tbl_CASA on cust.CustomerId equals acc.CustomerId
                              join prod in context.tbl_Product on acc.ProductId equals prod.ProductId
                              where cust.Deleted == false && cust.CompanyId == companyId
                              select new CustomerSearchVM
                              {
                                  customerId = cust.CustomerId,
                                  accountNumber = acc.ProductAccountNumber,
                                  customerCode = cust.CustomerCode,
                                  firstName = cust.FirstName,
                                  lastName = cust.LastName,
                                  middleName = cust.MaidenName,
                                  relationshipManagerId = acc.RelationshipManagerId ?? 0,
                                  relationshipOfficerId = acc.RelationshipOfficerId ?? 0
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
                allCustomer = from cg in context.tbl_Customer_Group
                              join gm in context.tbl_Customer_Group_Mapping
                              on cg.CustomerGroupId equals gm.CustomerGroupId
                              join casa in context.tbl_CASA
                              on gm.CustomerId equals casa.CustomerId
                              join prod in context.tbl_Product on casa.ProductId equals prod.ProductId
                              where cg.Deleted == false && gm.Deleted == false && casa.Deleted == false
                              select new CustomerSearchVM
                              {
                                  customerId = cg.CustomerGroupId,
                                  customerCode = cg.GroupCode,
                                  firstName = cg.GroupName,
                                  lastName = string.Empty,
                                  accountNumber = casa.ProductAccountNumber,
                                  relationshipManagerId = casa.RelationshipManagerId ?? 0,
                                  relationshipOfficerId = casa.RelationshipOfficerId ?? 0
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
            var customerGroupMapping = from b in context.tbl_CASA 
                                       where b.CustomerId == customerId &&  b.Deleted == false && b.CompanyId == companyId
                                       select new GroupCustomerMembersViewModel
                                       {
                                           customerId = b.CustomerId,                                          
                                           customerCode = b.tbl_Customer.CustomerCode,
                                           lastName = b.tbl_Customer.LastName,
                                           firstName = b.tbl_Customer.FirstName,
                                       
                                       };

            return customerGroupMapping;
        }

        private IQueryable<CasaCustomerSearchViewModel> GetAllAccounts()
        {
            var data = (from casa in context.tbl_CASA
                        join cust in context.tbl_Customer on casa.CustomerId equals cust.CustomerId
                        join prod in context.tbl_Product on casa.ProductId equals prod.ProductId
                        join sector in context.tbl_Sub_Sector on cust.SubSectorId equals sector.SubSectorId
                        join custGroup in context.tbl_Customer_Group_Mapping on cust.CustomerId equals custGroup.CustomerId into cGroup
                        from custGroup in cGroup.DefaultIfEmpty()
                        select new CasaCustomerSearchViewModel()
                        {
                            casaAccountId = casa.CasaAccountId,
                            productAccountNumber = casa.ProductAccountNumber,
                            productAccountName = casa.ProductAccountName,
                            customerId = casa.CustomerId,
                            customerCode = cust.CustomerCode,
                            accountHolder = cust.FirstName + " " + cust.LastName,
                            productId = prod.ProductId,
                            productCode = prod.ProductCode,
                            productName = prod.ProductName,
                            productClassId = prod.ProductClassId,
                            productClassName = prod.tbl_Product_Class.ProductClassName,
                            companyId = casa.CompanyId,
                            branchId = casa.BranchId,
                            branchCode = casa.tbl_Branch.BranchCode,
                            branchName = casa.tbl_Branch.BranchName,
                            relationshipOfficerId = casa.RelationshipOfficerId ?? 0,
                            relationshipManagerId = casa.RelationshipManagerId ?? 0,
                            subSectorId = sector.SubSectorId,
                            subSectorName = sector.Name,
                            customerSectorId = sector.tbl_Sector.SectorId,
                            customerSectorName = sector.tbl_Sector.Name,
                            customerGroupId = custGroup.CustomerGroupId,
                            customerGroupName = custGroup.tbl_Customer_Group.GroupName ?? "None",
                            taxIdentificationNumber = cust.TaxNumber,
                            registrationNumber = cust.tbl_Customer_CompanyInfomation.FirstOrDefault(x => x.CustomerId == cust.CustomerId).RegistrationNumber,
                            isBlackList = context.tbl_Customer_Blacklist.Any(x => x.CustomerId == cust.CustomerId),
                            isOnWatchList = context.tbl_Loan_PrudentialGuideline.Any(x => x.tbl_Loan.Any(l => l.CustomerId == cust.CustomerId) && x.PrudentialGuidelineStatusId == (int)LoanPrudentialStatusEnum.WatchList),
                            isCamsol = context.tbl_Loan_Camsol.Any(x => context.tbl_Loan.Any(l => l.TermLoanId == x.LoanId && l.CustomerId == cust.CustomerId)),
                            customerTypeId = cust.CustomerTypeId,
                            customerTypeName = cust.tbl_Customer_Type.Name,
                            customerBvnInformation = context.tbl_Customer_BVN.Where(b => b.CustomerId == casa.CustomerId).Select(b => new CustomerBvnViewModels()
                            {
                                bankVerificationNumber = b.BankVerificationNumber,
                                customerBvnid = b.CustomerBVNId,
                                firstname = b.Firstname,
                                isValidBvn = b.IsValidBVN,
                                isPoliticallyExposed = b.IsPoliticallyExposed,
                                surname = b.Surname
                            }).ToList(),
                            customerCompanyDirectors = context.tbl_Customer_Company_Director
                            .Where(s => s.CustomerId == casa.CustomerId && s.CompanyDirectorTypeId == (short)CompanyDirectorTypeEnum.BoardMember)
                            .Select(s => new CustomerCompanyDirectorsViewModels()
                            {
                                bankVerificationNumber = s.CustomerBVN,
                                companyDirectorTypeId = s.CompanyDirectorTypeId,
                                companyDirectorTypeName = s.tbl_Customer_Company_DirectorType.CompanyDirectoryTypeName,
                                customerId = s.CustomerId,
                                firstname = s.Firstname,
                                surname = s.Surname
                            }).ToList(),
                            customerCompanyShareholders = context.tbl_Customer_Company_Director
                            .Where(s => s.CustomerId == casa.CustomerId && s.CompanyDirectorTypeId == (short)CompanyDirectorTypeEnum.Shareholder)
                            .Select(s => new CustomerCompanyShareholdersViewModels()
                            {
                                bankVerificationNumber = s.CustomerBVN,
                                companyDirectorTypeId = s.CompanyDirectorTypeId,
                                companyDirectorTypeName = s.tbl_Customer_Company_DirectorType.CompanyDirectoryTypeName,
                                customerId = s.CustomerId,
                                firstname = s.Firstname,
                                surname = s.Surname
                            }).ToList(),
                            customerClients = context.tbl_Customer_Client_Supplier.Where(cs => cs.CustomerId == casa.CustomerId &&
                            cs.Client_SupplierTypeId == (short)CompanyClientOrSupplierTypeEnum.Client)
                            .Select(cs => new CustomerClientOrSupplierViewModels()
                            {
                                client_SupplierId = cs.Client_SupplierId,
                                clientOrSupplierName = cs.FirstName + " " + cs.LastName,
                                firstName = cs.FirstName,
                                middleName = cs.MiddleName,
                                lastName = cs.LastName,
                                client_SupplierAddress = cs.Address,
                                client_SupplierPhoneNumber = cs.PhoneNumber,
                                client_SupplierEmail = cs.EmailAddress,
                                client_SupplierTypeId = cs.Client_SupplierTypeId,
                                client_SupplierTypeName = cs.tbl_Customer_Client_Supplier_Type.Client_SupplierTypeName
                            }).ToList(),
                            customerSuppliers = context.tbl_Customer_Client_Supplier.Where(cs => cs.CustomerId == casa.CustomerId &&
                            cs.Client_SupplierTypeId == (short)CompanyClientOrSupplierTypeEnum.Supplier)
                             .Select(cs => new CustomerSupplierViewModels()
                             {
                                 client_SupplierId = cs.Client_SupplierId,
                                 clientOrSupplierName = cs.FirstName + " " + cs.LastName,
                                 firstName = cs.FirstName,
                                 middleName = cs.MiddleName,
                                 lastName = cs.LastName,
                                 client_SupplierAddress = cs.Address,
                                 client_SupplierPhoneNumber = cs.PhoneNumber,
                                 client_SupplierEmail = cs.EmailAddress,
                                 client_SupplierTypeId = cs.Client_SupplierTypeId,
                                 client_SupplierTypeName = cs.tbl_Customer_Client_Supplier_Type.Client_SupplierTypeName
                             }).ToList(),
                        });

            return data;
        }

        public IQueryable<CasaCustomerSearchViewModel> SearchForCustomerAccount(int companyId, string searchQuery)
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
                    .Where(x => x.accountHolder.Contains(searchQuery)
               || x.customerCode.Contains(searchQuery)
               || x.productAccountNumber.Contains(searchQuery)
                ).Take(10);
            }

            //foreach (var item in allCustomers)
            //{
            //    item.isBlackList = creditLimitRepo.ValidateBlackList(item.customerId) > 0;
            //    item.isOnWatchList = creditLimitRepo.ValidateWatchList(item.customerId) > 0;
            //    item.isCamsol = creditLimitRepo.ValidateCamsol(item.customerId) > 0;
            //}

            return allCustomers;
        }

    }
}