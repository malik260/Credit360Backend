using FintrakBanking.Common.Enum;
using FintrakBanking.Entities.Models;
using FintrakBanking.Interfaces.Admin;
using FintrakBanking.Interfaces.Setups.Finance;
using FintrakBanking.Interfaces.Setups.General;
using FintrakBanking.ViewModels;
using FintrakBanking.ViewModels.Setups.Finance;
using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;

namespace FintrakBanking.Repositories.Setups.Finance
{
    /// <summary>
    /// TODO: Implement audit trails in these methods
    /// </summary>
    /// 
    [Export(typeof(IChartOfAccountRepository))]
    [PartCreationPolicy(CreationPolicy.NonShared)]
    public class ChartOfAccountRepository : IChartOfAccountRepository
    {
        private FinTrakBankingContext context;
        private IAuditTrailRepository auditTrail;
        IGeneralSetupRepository _genSetup;

        public ChartOfAccountRepository(FinTrakBankingContext _context,
                                                IAuditTrailRepository _auditTrail,
                                                IGeneralSetupRepository genSetup)
        {
            this.context = _context;
            this._genSetup = genSetup;
            auditTrail = _auditTrail;
        }

        private bool SaveAll()
        {
            return this.context.SaveChanges() > 0;
        }

        public int AddAccount(ChartOfAccountViewModel account)
        {
            if (account.currencies.Count < 1)
                throw new Exception("Chart of Account Currency must be specified");

            List<tbl_Chart_Of_Account_Currency> currencies = new List<tbl_Chart_Of_Account_Currency>();

            //Storing the chart of account currencies
            foreach (var item in account.currencies)
            {
                var chartOfAccountCurrency = new tbl_Chart_Of_Account_Currency()
                {
                    CurrencyId = item.currencyId,
                    //GlaccountId = chartOfAccount.GlaccountId,
                    CreatedBy = item.createdBy,
                    DateTimeCreated = _genSetup.GetApplicaionDate()
                };

                currencies.Add(chartOfAccountCurrency);
            }
            //End of storing the chart of account currencies
            var chartOfAccount = new tbl_Chart_Of_Account()
            {
                AccountCode = account.accountCode,
                AccountName = account.accountName,
                AccountTypeId = account.accountTypeId,
                CompanyId = account.companyId,
                BranchId = account.branchId, 
                SystemUse = account.systemUse,
                BranchSpecific = account.branchSpecific,
                FSCaptionId = account.fsCaptionId,
                AccountStatusId = account.accountStatusId,

                CreatedBy = account.createdBy,
                DateTimeCreated = _genSetup.GetApplicaionDate()
                //OldAccountId
            };

            this.context.tbl_Chart_Of_Account.Add(chartOfAccount);
            // Audit Section ---------------------------
            var audit = new tbl_Audit
            {
                AuditTypeId = (short)AuditTypeEnum.ChartOfAccountUpdated,
                StaffId = (int)account.createdBy,
                BranchId = (short)account.userBranchId,
                Detail = $"Added New Account: {account.accountName} with code: {account.accountCode}",
                IPAddress = account.userIPAddress,
                Url = account.applicationUrl,
                SystemDateTime = DateTime.Now,
                ApplicationDate = _genSetup.GetApplicaionDate()
            };
            this.auditTrail.AddAuditTrail(audit);
            //end of Audit section -------------------------------
            var status = this.SaveAll();

            if (status)
                return chartOfAccount.GLAccountId;
            else
                return -1;
        }

        private IQueryable<ChartOfAccountViewModel> GetAllAccountsDetails()
        {
            var data = (from account in context.tbl_Chart_Of_Account
                        where account.Deleted == false
                        orderby account.tbl_Account_Type.tbl_Account_Category.AccountCategoryName ascending
                        select new ChartOfAccountViewModel()
                        {
                            accountId = account.GLAccountId,
                            accountCode = account.AccountCode,
                            accountName = account.AccountName,
                            accountTypeId = account.AccountTypeId,
                            accountTypeName = account.tbl_Account_Type.AccountTypeName,
                            accountCategoryId = account.tbl_Account_Type.AccountCategoryId,
                            accountCategoryName = account.tbl_Account_Type.tbl_Account_Category.AccountCategoryName,
                            accountStatusId = account.AccountStatusId,
                            currencies = context.tbl_Chart_Of_Account_Currency.Where(curr => curr.GLAccountId == account.GLAccountId).Select(c => new ChartOfAccountCurrencyViewModel()
                            {
                                glaccountId = c.GLAccountId,
                                glaccountCurrencyId = c.GLAccountCurrencyId,
                                currencyId = c.CurrencyId,
                                currencyName = c.tbl_Currency.CurrencyCode + " -- " + c.tbl_Currency.CurrencyName

                            }).ToList(),
                            companyId = account.CompanyId,
                            branchId = account.BranchId,

                            systemUse = account.SystemUse,
                            branchSpecific = account.BranchSpecific,
                            fsCaptionId = account.FSCaptionId,

                            createdBy = account.CreatedBy,
                            dateTimeCreated = account.DateTimeCreated,

                            // lastUpdatedBy = account.LastUpdatedBy.Value ,
                            // dateTimeUpdated = account.DateTimeUpdated
                        });

            return data;
        }

        public IEnumerable<ChartOfAccountViewModel> GetAllAccounts()
        {
            var data = GetAllAccountsDetails();

            return data;
        }

        public IEnumerable<ChartOfAccountViewModel> GetAccountsByCategory(short accountCategoryId)
        {
            var data = GetAllAccountsDetails().Where(x => x.accountCategoryId == accountCategoryId).ToList();

            return data;
        }

        public ChartOfAccountViewModel GetAccountViewModel(short accountId)
        {
            var account = this.context.tbl_Chart_Of_Account.FirstOrDefault(x => x.GLAccountId == accountId && x.Deleted == false); // .Find(accountId);

            if (account == null)
                return null;

            return new ChartOfAccountViewModel
            {
                accountId = account.GLAccountId,
                accountCode = account.AccountCode,
                accountName = account.AccountName,
                accountTypeId = account.AccountTypeId,
                companyId = account.CompanyId,
                branchId = account.BranchId, 
                systemUse = account.SystemUse,
                branchSpecific = account.BranchSpecific,
                fsCaptionId = account.FSCaptionId,
                createdBy = account.CreatedBy ,
                dateTimeCreated = account.DateTimeCreated ,
                lastUpdatedBy = account.LastUpdatedBy.Value ,
                dateTimeUpdated = account.DateTimeUpdated
            };
        }

        public bool UpdateAccount(short accountId, ChartOfAccountViewModel account)
        {
            var accountModel = this.context.tbl_Chart_Of_Account.Find(accountId);

            if (accountModel == null)
                return false;

            accountModel.AccountCode = account.accountCode;
            accountModel.AccountName = account.accountName;
            accountModel.AccountTypeId = account.accountTypeId;
            accountModel.CompanyId = account.companyId;
            accountModel.BranchId = account.branchId; 
            accountModel.SystemUse = account.systemUse;
            accountModel.BranchSpecific = account.branchSpecific;
            accountModel.FSCaptionId = account.fsCaptionId;
            accountModel.AccountStatusId = account.accountStatusId;

            accountModel.LastUpdatedBy = account.lastUpdatedBy;
            accountModel.DateTimeUpdated = _genSetup.GetApplicaionDate();

            //Account Currencies Update
            foreach (var currency in account.currencies)
            {
                var data = context.tbl_Chart_Of_Account_Currency.Where(c => c.CurrencyId == currency.currencyId).FirstOrDefault();
                data.CurrencyId = currency.currencyId;
                //data.GlaccountId = currency.glaccountId;
            }
            //End of account currencies update

            // Audit Section ---------------------------
            var audit = new tbl_Audit
            {
                AuditTypeId = (short)AuditTypeEnum.ChartOfAccountUpdated,
                StaffId = (int)account.createdBy,
                BranchId = (short)account.userBranchId,
                Detail = $"Updated New Account: {accountModel.AccountName} with code: {accountModel.AccountCode}",
                IPAddress = account.userIPAddress,
                Url = account.applicationUrl,
                SystemDateTime = DateTime.Now,
                ApplicationDate = _genSetup.GetApplicaionDate()
            };
            this.auditTrail.AddAuditTrail(audit);
            //end of Audit section -------------------------------

            return this.SaveAll();

            //throw new NotImplementedException();
        }

        public bool DeleteAccount(short accountId, UserInfo user)
        {
            var accountModel = this.context.tbl_Chart_Of_Account.Find(accountId);

            if (accountModel == null)
                return false;

            accountModel.Deleted = true;
            //accountModel.DeletedBy = ;
            accountModel.DateTimeDeleted = _genSetup.GetApplicaionDate();

            var audit = new tbl_Audit
            {
                AuditTypeId = (short)AuditTypeEnum.ChartOfAccountDeleted,
                StaffId = (int)user.createdBy,
                BranchId = (short)user.BranchId,
                Detail = $"Deleted New Account: {accountModel.AccountName} with code: {accountModel.AccountCode}",
                IPAddress = user.userIPAddress,
                Url = user.applicationUrl,
                SystemDateTime = DateTime.Now,
                ApplicationDate = _genSetup.GetApplicaionDate()
            };
            this.auditTrail.AddAuditTrail(audit);
            //end of Audit section -------------------------------
            return this.SaveAll();

            //throw new NotImplementedException();
        }

        public IEnumerable<LookupViewModel> GetFinancialSatementCaptionLookup()
        {
            return (from data in context.tbl_Financial_Statement_Caption
                    where data.IsTotalLine == false
                    orderby data.FinType, data.Position
                    select new LookupViewModel()
                    {
                        lookupId = data.FSCaptionId,
                        lookupName = data.FSCaption + " -- " + data.tbl_Account_Category.AccountCategoryName
                    });
        }
    }
}