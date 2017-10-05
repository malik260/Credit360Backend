using FintrakBanking.Common.Enum;
using FintrakBanking.Entities.Models;
using FintrakBanking.Interfaces.Admin;
using FintrakBanking.Interfaces.Setups.Approval;
using FintrakBanking.Interfaces.Setups.Finance;
using FintrakBanking.Interfaces.Setups.General;
using FintrakBanking.Interfaces.WorkFlow;
using FintrakBanking.ViewModels;
using FintrakBanking.ViewModels.Setups.Finance;
using FintrakBanking.ViewModels.WorkFlow;
using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using System.Threading.Tasks;

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
        private IGeneralSetupRepository _genSetup;
        private IWorkflow workFlow;
        private IApprovalLevelStaffRepository level;

        public ChartOfAccountRepository(FinTrakBankingContext _context,
                                                IAuditTrailRepository _auditTrail,
                                                IGeneralSetupRepository genSetup,
                                                IWorkflow _workFlow,
                                                IApprovalLevelStaffRepository _level)
        {
            this.context = _context;
            this._genSetup = genSetup;
            auditTrail = _auditTrail;
            this.workFlow = _workFlow;
            level = _level;
        }

        private bool SaveAll()
        {
            return this.context.SaveChanges() > 0;
        }

        public bool GoForApproval(ApprovalViewModel entity)
        {
            entity.operationId = (int)OperationsEnum.ChartOfAccountCreation;

            entity.externalInitialization = false;

            var response = workFlow.LogForApproval(entity);

            if (workFlow.NewState == (int)ApprovalState.Ended)
            {
                return ApproveAccount(entity.targetId, (int)ApprovalStatusEnum.Approved, entity);
            }

            return false;
        }

        private bool ApproveAccount(int accountId, short approvalStatusId, UserInfo user)
        {
            var accountModel = context.tbl_Temp_Chart_Of_Account.Find(accountId);
            var accountToUpdate = context.tbl_Chart_Of_Account.Where(x => x.AccountCode == accountModel.AccountCode);
            var existingAccount = accountToUpdate.FirstOrDefault();

            var currModel = context.tbl_Temp_Chart_Of_Account_Currency.Where(c => c.GLAccountId == accountModel.GLAccountId && c.Deleted == false);
            var currListToUpdate = context.tbl_Chart_Of_Account_Currency.Where(x => x.GLAccountId == existingAccount.GLAccountId && x.Deleted == false);

            List<tbl_Chart_Of_Account_Currency> coaCurrencies = new List<tbl_Chart_Of_Account_Currency>();

            if (accountToUpdate.Any()) //Update existing account with tempAccount record
            {
                foreach (var curr in currListToUpdate)
                {
                    context.tbl_Chart_Of_Account_Currency.Remove(curr);
                }

                foreach (var c in currModel)
                {
                    var curr = new tbl_Chart_Of_Account_Currency()
                    {
                        //GLAccountId = c.GLAccountId,
                        CurrencyId = c.CurrencyId,
                        DateTimeCreated = _genSetup.GetApplicationDate(),
                        DateTimeUpdated = DateTime.Now,
                    };
                    coaCurrencies.Add(curr);
                }

                existingAccount.AccountCode = accountModel.AccountCode;
                existingAccount.AccountName = accountModel.AccountName;
                existingAccount.AccountTypeId = accountModel.AccountTypeId;
                existingAccount.CompanyId = accountModel.CompanyId;
                existingAccount.BranchId = accountModel.BranchId;
                existingAccount.SystemUse = accountModel.SystemUse;
                existingAccount.BranchSpecific = accountModel.BranchSpecific;
                existingAccount.FSCaptionId = accountModel.FSCaptionId;
                existingAccount.AccountStatusId = accountModel.AccountStatusId;
                existingAccount.CreatedBy = accountModel.CreatedBy;
                existingAccount.DateTimeUpdated = DateTime.Now;
                existingAccount.tbl_Chart_Of_Account_Currency = coaCurrencies;
                existingAccount.GLClassId = (short)accountModel.GLClassId;
            }
            else //Insert a new account record into the real account table
            {
                foreach (var c in currModel)
                {
                    var curr = new tbl_Chart_Of_Account_Currency()
                    {
                        //GLAccountId = c.GLAccountId,
                        CurrencyId = c.CurrencyId,
                        DateTimeCreated = _genSetup.GetApplicationDate(),
                    };
                    coaCurrencies.Add(curr);
                }

                var account = new tbl_Chart_Of_Account()
                {
                    AccountCode = accountModel.AccountCode,
                    AccountName = accountModel.AccountName,
                    AccountTypeId = accountModel.AccountTypeId,
                    CompanyId = accountModel.CompanyId,
                    BranchId = accountModel.BranchId,
                    SystemUse = accountModel.SystemUse,
                    BranchSpecific = accountModel.BranchSpecific,
                    FSCaptionId = accountModel.FSCaptionId,
                    AccountStatusId = accountModel.AccountStatusId,
                    DateTimeCreated = _genSetup.GetApplicationDate(),
                    CreatedBy = accountModel.CreatedBy,
                    tbl_Chart_Of_Account_Currency = coaCurrencies,
                    GLClassId = (short)accountModel.GLClassId
                };
                context.tbl_Chart_Of_Account.Add(account);
            }

            accountModel.IsCurrent = false;
            accountModel.ApprovalStatusId = approvalStatusId;
            accountModel.DateTimeUpdated = DateTime.Now;

            // remove the temp chart of account and currency
            context.tbl_Temp_Chart_Of_Account.Remove(accountModel);

            foreach (var curr in currModel)
            {
                context.tbl_Temp_Chart_Of_Account_Currency.Remove(curr);
            }

            // Audit Section ---------------------------
            var audit = new tbl_Audit
            {
                AuditTypeId = (short)AuditTypeEnum.AccountApproved,
                StaffId = user.staffId,
                BranchId = (short)user.BranchId,
                Detail = $"Approved Account '{accountModel.AccountName}' with staff code'{accountModel.AccountCode}'",
                IPAddress = user.userIPAddress,
                Url = user.applicationUrl,
                ApplicationDate = _genSetup.GetApplicationDate(),
                SystemDateTime = DateTime.Now
            };

            this.auditTrail.AddAuditTrail(audit);
            // Audit Section ---------------------------

            try
            {
                return this.SaveAll();
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        private int AddAccount2(ChartOfAccountViewModel account)
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
                    DateTimeCreated = _genSetup.GetApplicationDate()
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
                DateTimeCreated = _genSetup.GetApplicationDate(),
                tbl_Chart_Of_Account_Currency = currencies,
                GLClassId = account.glClassId
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
                ApplicationDate = _genSetup.GetApplicationDate()
            };
            this.auditTrail.AddAuditTrail(audit);
            //end of Audit section -------------------------------
            var status = this.SaveAll();

            if (status)
                return chartOfAccount.GLAccountId;
            else
                return -1;
        }

        public async Task<bool> AddTempAccount(ChartOfAccountViewModel accountModel)
        {
            if (accountModel.currencies.Count < 1)
                throw new Exception("Chart of Account Currency must be specified");

            List<tbl_Temp_Chart_Of_Account_Currency> currencies = new List<tbl_Temp_Chart_Of_Account_Currency>();

            bool output = false;
            var existStingTempAccount = context.tbl_Temp_Chart_Of_Account.Where(x => x.AccountCode.ToLower() == accountModel.accountCode.ToLower()
                                                                  && x.IsCurrent == true && x.CompanyId == accountModel.companyId
                                                                  && x.ApprovalStatusId == (short)ApprovalStatusEnum.Pending);

            if (existStingTempAccount.Any())
            {
                throw new Exception("Account Information already exist and is undergoing approval");
            }

            //Storing the chart of account currencies
            foreach (var item in accountModel.currencies)
            {
                var chartOfAccountCurrency = new tbl_Temp_Chart_Of_Account_Currency()
                {
                    //GLAccountId = item.glaccountId,
                    CurrencyId = item.currencyId,
                    CreatedBy = item.createdBy,
                    DateTimeCreated = _genSetup.GetApplicationDate()
                };
                currencies.Add(chartOfAccountCurrency);
            }
            //End of storing the chart of account currencies

            var account = new tbl_Temp_Chart_Of_Account()
            {
                AccountCode = accountModel.accountCode,
                AccountName = accountModel.accountName,
                AccountTypeId = accountModel.accountTypeId,
                CompanyId = accountModel.companyId,
                BranchId = accountModel.branchId,
                SystemUse = accountModel.systemUse,
                BranchSpecific = accountModel.branchSpecific,
                FSCaptionId = accountModel.fsCaptionId,
                AccountStatusId = accountModel.accountStatusId,
                CreatedBy = accountModel.createdBy,
                DateTimeCreated = _genSetup.GetApplicationDate(),
                ApprovalStatusId = (short)ApprovalStatusEnum.Pending,
                IsCurrent = true,
                tbl_Temp_Chart_Of_Account_Currency = currencies,
                GLClassId = accountModel.glClassId
            };

            // Audit Section ---------------------------
            var audit = new tbl_Audit
            {
                AuditTypeId = (short)AuditTypeEnum.ChartOfAccountInitiated,
                StaffId = accountModel.createdBy,
                BranchId = (short)accountModel.userBranchId,
                Detail = $"Initiated Chart of Account Creation for '{accountModel.accountName}' with code'{accountModel.accountCode}'",
                IPAddress = accountModel.userIPAddress,
                Url = accountModel.applicationUrl,
                ApplicationDate = _genSetup.GetApplicationDate(),
                SystemDateTime = DateTime.Now
            };

            using (var trans = context.Database.BeginTransaction())
            {
                try
                {
                    auditTrail.AddAuditTrail(audit);
                    context.tbl_Temp_Chart_Of_Account.Add(account);
                    output = await context.SaveChangesAsync() > 0;

                    var entity = new ApprovalViewModel
                    {
                        staffId = accountModel.createdBy,
                        companyId = accountModel.companyId,
                        approvalStatusId = (int)ApprovalStatusEnum.Pending,
                        targetId = account.GLAccountId,
                        operationId = (int)OperationsEnum.ChartOfAccountCreation,
                        BranchId = accountModel.userBranchId,
                        externalInitialization = true
                    };
                    var response = workFlow.LogForApproval(entity);

                    if (response)
                    {
                        trans.Commit();
                    }

                    return output;
                }
                catch (Exception ex)
                {
                    trans.Rollback();
                    throw new Exception(ex.Message);
                }
            }
        }

        public bool IsAccountCodeAlreadyExist(string accountCode)
        {
            return context.tbl_Chart_Of_Account.Any(x => x.AccountCode.ToLower() == accountCode.ToLower());
        }

        public bool IsAccountExist(string accountCode)
        {
            return context.tbl_Temp_Chart_Of_Account.Any(x => x.AccountCode.ToLower() == accountCode.ToLower() && x.ApprovalStatusId == (int)ApprovalStatusEnum.Pending && x.IsCurrent == true);
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
                            currencies = context.tbl_Chart_Of_Account_Currency.Where(curr => curr.GLAccountId == account.GLAccountId && curr.Deleted == false).Select(c => new ChartOfAccountCurrencyViewModel()
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
                            glClassId = account.GLClassId

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
                createdBy = account.CreatedBy,
                dateTimeCreated = account.DateTimeCreated,
                lastUpdatedBy = account.LastUpdatedBy.Value,
                dateTimeUpdated = account.DateTimeUpdated
            };
        }

        private bool UpdateAccount2(short accountId, ChartOfAccountViewModel account)
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
            accountModel.DateTimeUpdated = _genSetup.GetApplicationDate();

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
                ApplicationDate = _genSetup.GetApplicationDate()
            };
            this.auditTrail.AddAuditTrail(audit);
            //end of Audit section -------------------------------

            return this.SaveAll();

            //throw new NotImplementedException();
        }

        public bool UpdateAccount(short accountId, ChartOfAccountViewModel accountModel)
        {
            if (accountModel == null)
                return false;

            var existStingTempAccount = context.tbl_Temp_Chart_Of_Account.Where(x => x.AccountCode.ToLower() == accountModel.accountCode.ToLower() && x.IsCurrent == true && x.ApprovalStatusId == (int)ApprovalStatusEnum.Approved);
            var existingTempCurrencies = context.tbl_Temp_Chart_Of_Account_Currency.Where(x => x.GLAccountId == accountModel.accountId).ToList();

            if (existingTempCurrencies.Count > 0)
            {
                foreach (var curr in existingTempCurrencies)
                {
                    context.tbl_Temp_Chart_Of_Account_Currency.Remove(curr);
                }
            }

            if (existStingTempAccount.Any() && existingTempCurrencies.Any())
            {
                foreach (var item in existStingTempAccount)
                {
                    item.IsCurrent = false;
                    item.DateTimeUpdated = DateTime.Now;
                }

                foreach (var item in existingTempCurrencies)
                {
                    item.IsCurrent = false;
                    item.DateTimeUpdated = DateTime.Now;
                }
            }

            var targetAccount = context.tbl_Chart_Of_Account.Find(accountId);

            var unApprovedAccountEdit = context.tbl_Temp_Chart_Of_Account.Where(x => x.IsCurrent == true && x.ApprovalStatusId == (int)ApprovalStatusEnum.Pending);
            var unApprovedCurrencyAccountEdit = context.tbl_Temp_Chart_Of_Account_Currency.Where(x => x.IsCurrent == true && x.ApprovalStatusId == (int)ApprovalStatusEnum.Pending);

            tbl_Temp_Chart_Of_Account tempAccount;
            List<tbl_Temp_Chart_Of_Account_Currency> tempCurrencies = new List<tbl_Temp_Chart_Of_Account_Currency>();

            if (unApprovedAccountEdit.Any() && unApprovedCurrencyAccountEdit.Any())
            {
                throw new Exception("Chart of Account is already undergoing approval");
            }
            else
            {
                //Storing the updated chart of account currencies
                foreach (var item in accountModel.currencies)
                {
                    var chartOfAccountCurrency = new tbl_Temp_Chart_Of_Account_Currency()
                    {
                        //GLAccountId = item.glaccountId,
                        CurrencyId = item.currencyId,
                        CreatedBy = accountModel.createdBy,
                        DateTimeCreated = _genSetup.GetApplicationDate()
                    };
                    tempCurrencies.Add(chartOfAccountCurrency);
                }
                //End of storing the updated chart of account currencies

                tempAccount = new tbl_Temp_Chart_Of_Account()
                {
                    AccountCode = targetAccount.AccountCode,
                    AccountName = accountModel.accountName,
                    AccountTypeId = accountModel.accountTypeId,
                    CompanyId = accountModel.companyId,
                    BranchId = accountModel.userBranchId,
                    SystemUse = accountModel.systemUse,
                    BranchSpecific = accountModel.branchSpecific,
                    FSCaptionId = accountModel.fsCaptionId,
                    AccountStatusId = accountModel.accountStatusId,
                    //GLAccountId = accountId,
                    CreatedBy = accountModel.createdBy,
                    DateTimeCreated = _genSetup.GetApplicationDate(),
                    ApprovalStatusId = (int)ApprovalStatusEnum.Pending,
                    IsCurrent = true,

                    tbl_Temp_Chart_Of_Account_Currency = tempCurrencies,
                    GLClassId = accountModel.glClassId
                };

                context.tbl_Temp_Chart_Of_Account.Add(tempAccount);
            }
            var audit = new tbl_Audit
            {
                AuditTypeId = (short)AuditTypeEnum.ChartOfAccountUpdated,
                StaffId = accountModel.createdBy,
                BranchId = (short)accountModel.userBranchId,
                Detail = $"Initiated updated of Chart Of Account '{accountModel.accountName}' with code'{accountModel.accountCode}'",
                IPAddress = accountModel.userIPAddress,
                Url = accountModel.applicationUrl,
                ApplicationDate = _genSetup.GetApplicationDate(),
                SystemDateTime = DateTime.Now,
                TargetId = accountId
            };

            this.auditTrail.AddAuditTrail(audit);
            //end of Audit section -------------------------------

            var output = this.SaveAll();

            var entity = new ApprovalViewModel
            {
                staffId = accountModel.createdBy,
                companyId = accountModel.companyId,
                approvalStatusId = (int)ApprovalStatusEnum.Pending,
                targetId = tempAccount.GLAccountId,
                operationId = (int)OperationsEnum.ChartOfAccountCreation,
                BranchId = accountModel.userBranchId,
                externalInitialization = true
            };
            bool response = workFlow.LogForApproval(entity);

            if (!response)
            {
                return false;
            }

            return output;
        }

        public IEnumerable<ChartOfAccountViewModel> GetAccountsAwaitingApprovals(int accountId, int companyId)
        {
            var levelResult = level.GetAllApprovalLevelStaffByStaffId(accountId, companyId, (int)OperationsEnum.ChartOfAccountCreation);
            int staffApprovalLevelId = 0;

            if (levelResult != null) staffApprovalLevelId = levelResult.approvalLevelId;

            var data = (from c in context.tbl_Temp_Chart_Of_Account
                join coy in context.tbl_Company on c.CompanyId equals companyId
                join atrail in context.tbl_Approval_Trail on c.GLAccountId equals atrail.TargetId
                where
                    atrail.ApprovalStateId == (int)ApprovalState.Processing
                    &&
                    c.IsCurrent == true
                    && atrail.ToApprovalLevelId == staffApprovalLevelId &&
                    atrail.OperationId == (int) OperationsEnum.ChartOfAccountCreation
                select new ChartOfAccountViewModel()
                {
                    accountId = c.GLAccountId,
                    accountCode = c.AccountCode,
                    accountName = c.AccountName,
                    accountTypeId = c.AccountTypeId,
                    accountTypeName = c.tbl_Account_Type.AccountTypeName,
                    accountCategoryId = c.tbl_Account_Type.AccountCategoryId,
                    accountCategoryName = c.tbl_Account_Type.tbl_Account_Category.AccountCategoryName,
                    accountStatusId = c.AccountStatusId,
                    currencies = context.tbl_Temp_Chart_Of_Account_Currency
                        .Where(curr => curr.GLAccountId == c.GLAccountId && curr.Deleted == false).Select(coa =>
                            new ChartOfAccountCurrencyViewModel()
                            {
                                glaccountId = coa.GLAccountId,
                                glaccountCurrencyId = coa.GLAccountCurrencyId,
                                currencyId = coa.CurrencyId,
                                currencyName = coa.tbl_Currency.CurrencyCode + " -- " + coa.tbl_Currency.CurrencyName
                            }).ToList(),
                    companyId = c.CompanyId,
                    branchId = c.BranchId,
                    branchName = c.tbl_Branch.BranchName,
                    systemUse = c.SystemUse,
                    branchSpecific = c.BranchSpecific,
                    fsCaptionId = c.FSCaptionId,
                    fsCaptionName = c.tbl_Financial_Statement_Caption.FSCaption,
                    operationId = atrail.OperationId,
                    approvalStatusId = c.ApprovalStatusId,
                    createdBy = c.CreatedBy,
                    dateTimeCreated = c.DateTimeCreated,
                    glClassId = (short) c.GLClassId
                }).GroupBy(x => x.accountId).Select(g => g.FirstOrDefault());

            return data;
        }

        public ChartOfAccountViewModel GetTempAccountDetail(int accountId)
        {
            //return GetTempStaffDetails().Where(x => x.StaffId == staffId).Single();

            return (from c in context.tbl_Temp_Chart_Of_Account
                    join coy in context.tbl_Company on c.CompanyId equals coy.CompanyId
                    where c.GLAccountId == accountId
                    select new ChartOfAccountViewModel()
                    {
                        accountId = c.GLAccountId,
                        accountCode = c.AccountCode,
                        accountName = c.AccountName,
                        accountTypeId = c.AccountTypeId,
                        accountTypeName = c.tbl_Account_Type.AccountTypeName,
                        accountCategoryId = c.tbl_Account_Type.AccountCategoryId,
                        accountCategoryName = c.tbl_Account_Type.tbl_Account_Category.AccountCategoryName,
                        accountStatusId = c.AccountStatusId,
                        currencies = context.tbl_Chart_Of_Account_Currency.Where(curr => curr.GLAccountId == c.GLAccountId && curr.Deleted == false).Select(coa => new ChartOfAccountCurrencyViewModel()
                        {
                            glaccountId = coa.GLAccountId,
                            glaccountCurrencyId = coa.GLAccountCurrencyId,
                            currencyId = coa.CurrencyId,
                            currencyName = coa.tbl_Currency.CurrencyCode + " -- " + coa.tbl_Currency.CurrencyName
                        }).ToList(),
                        companyId = c.CompanyId,
                        branchId = c.BranchId,

                        systemUse = c.SystemUse,
                        branchSpecific = c.BranchSpecific,
                        fsCaptionId = c.FSCaptionId,

                        createdBy = c.CreatedBy,
                        dateTimeCreated = c.DateTimeCreated,
                    }).FirstOrDefault();
        }

        public bool DeleteAccount(short accountId, UserInfo user)
        {
            var accountModel = this.context.tbl_Chart_Of_Account.Find(accountId);

            if (accountModel == null)
                return false;

            accountModel.Deleted = true;
            //accountModel.DeletedBy = ;
            accountModel.DateTimeDeleted = _genSetup.GetApplicationDate();

            var audit = new tbl_Audit
            {
                AuditTypeId = (short)AuditTypeEnum.ChartOfAccountDeleted,
                StaffId = (int)user.createdBy,
                BranchId = (short)user.BranchId,
                Detail = $"Deleted New Account: {accountModel.AccountName} with code: {accountModel.AccountCode}",
                IPAddress = user.userIPAddress,
                Url = user.applicationUrl,
                SystemDateTime = DateTime.Now,
                ApplicationDate = _genSetup.GetApplicationDate()
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

        public IEnumerable<ChartOfAccountClassViewModel> GetChartOfAccountClasses()
        {
            return (from data in context.tbl_Chart_Of_Account_Class
                    select new ChartOfAccountClassViewModel()
                    {
                        glClassId = data.GLClassId,
                        glClassName = data.GLClassName
                    });
        }
    }
}