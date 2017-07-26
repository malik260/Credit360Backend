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

        public IEnumerable<ChartOfAccountViewModel> GetAllAccounts()
        {
            //return this.context.TblChartOfAccount;
            return (from account in context.tbl_Chart_Of_Account
                    where account.Deleted == false //orderby account.AccountCode ascending, account.AccountName ascending
                    select new ChartOfAccountViewModel()
                    {
                        accountId = account.GLAccountId,
                        accountCode = account.AccountCode,
                        accountName = account.AccountName,
                        accountTypeId = account.AccountTypeId,
                        accountTypeName = account.tbl_Account_Type.AccountTypeName,
                        accountCategoryId = account.tbl_Account_Type.AccountCategoryId,
                        accountCategoryName = account.tbl_Account_Type.tbl_Account_Category.AccountCategoryName,
                        companyId = account.CompanyId,
                        branchId = account.BranchId, 
                        systemUse = account.SystemUse,
                        branchSpecific = account.BranchSpecific,
                        fsCaptionId = account.FSCaptionId,

                        // createdBy = account.CreatedBy.Value ,
                        // dateTimeCreated =  account.DateTimeCreated.Value ,

                        // lastUpdatedBy = account.LastUpdatedBy.Value ,
                        // dateTimeUpdated = account.DateTimeUpdated
                    });
        }

        public IEnumerable<ChartOfAccountViewModel> GetAccountsByCategory(short accountCategoryId)
        {
            //return this.context.TblChartOfAccount;
            return (from account in context.tbl_Chart_Of_Account
                    where account.tbl_Account_Type.AccountCategoryId == accountCategoryId && account.Deleted == false //orderby account.AccountCode ascending, account.AccountName ascending
                    select new ChartOfAccountViewModel()
                    {
                        accountId = account.GLAccountId,
                        accountCode = account.AccountCode,
                        accountName = account.AccountName,
                        accountTypeId = account.AccountTypeId,
                        accountTypeName = account.tbl_Account_Type.AccountTypeName,
                        accountCategoryId = account.tbl_Account_Type.AccountCategoryId,
                        accountCategoryName = account.tbl_Account_Type.tbl_Account_Category.AccountCategoryName,
                        companyId = account.CompanyId,
                        branchId = account.BranchId, 
                        systemUse = account.SystemUse,
                        branchSpecific = account.BranchSpecific,
                        fsCaptionId = account.FSCaptionId,
                        createdBy = account.CreatedBy,
                        dateTimeCreated = account.DateTimeCreated ,

                        lastUpdatedBy = account.LastUpdatedBy.Value ,
                        dateTimeUpdated = account.DateTimeUpdated
                    });
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