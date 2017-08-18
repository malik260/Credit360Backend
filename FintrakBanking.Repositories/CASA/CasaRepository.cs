using FintrakBanking.Entities.Models;
using FintrakBanking.Interfaces.CASA;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using FintrakBanking.ViewModels.CASA;
using FintrakBanking.Common.Enum;
using System.ComponentModel.Composition;
using FintrakBanking.ViewModels.Customer;

namespace FintrakBanking.Repositories.CASA
{
    [Export(typeof(ICasaRepository))]
    [PartCreationPolicy(CreationPolicy.NonShared)]
    public class CasaRepository : ICasaRepository
    {
        private FinTrakBankingContext context;

        public CasaRepository(FinTrakBankingContext _context)
        {
            this.context = _context;
        }

        private bool SaveAll()
        {
            return this.context.SaveChanges() > 0;
        }


        /// TODO: Implement server side filtering due to large number of records that may be returned
        public IEnumerable<CasaViewModel> FindAccount(string accountNumberOrName, int companyId)
        {
            return (from data in context.tbl_CASA
                    where data.CompanyId == companyId && (data.ProductAccountNumber == accountNumberOrName || data.ProductAccountName.Contains(accountNumberOrName)) //orderby account.AccountCode ascending, account.AccountName ascending
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

        private IQueryable<CasaCustomerSearchViewModel> GetAllAccounts()
        {
            var data = (from casa in context.tbl_CASA
                        join cust in context.tbl_Customer on casa.CustomerId equals cust.CustomerId
                        join prod in context.tbl_Product on casa.ProductId equals prod.ProductId
                        join sector in context.tbl_Sub_Sector on cust.SubSectorId equals sector.SubSectorId
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
                            customerCompanyDirectors = context.tbl_Customer_Company_Director.Where(s => s.CustomerId == casa.CustomerId).Select(s => new CustomerCompanyDirectorsViewModels()
                            {
                                bankVerificationNumber = s.CustomerBVN,
                                companyDirectorTypeId = s.CompanyDirectorTypeId,
                                companyDirectorTypeName = s.tbl_Customer_Company_DirectorType.CompanyDirectoryTypeName,
                                customerId = s.CustomerId,
                                customerName = s.Firstname + " " + s.Surname,
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
                );
            }

            return allCustomers;
        }
    }
}
