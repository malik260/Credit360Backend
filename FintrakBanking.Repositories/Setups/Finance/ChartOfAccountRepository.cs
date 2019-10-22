using FintrakBanking.Common.Enum;
using FintrakBanking.Entities.Models;
using FintrakBanking.Interfaces.Admin;
using FintrakBanking.Interfaces.Credit;
using FintrakBanking.Interfaces.Setups.Approval;
using FintrakBanking.Interfaces.Setups.Finance;
using FintrakBanking.Interfaces.Setups.General;
using FintrakBanking.Interfaces.WorkFlow;
using FintrakBanking.ViewModels;
using FintrakBanking.ViewModels.Setups.Finance;
using FintrakBanking.ViewModels.ThridPartyIntegration;
using FintrakBanking.ViewModels.WorkFlow;
using System;
using System.Collections.Generic;
using FintrakBanking.Common.CustomException;
using System.Linq;
using System.Threading.Tasks;
using FintrakBanking.Common;

namespace FintrakBanking.Repositories.Setups.Finance
{
    /// <summary>
    /// TODO: Implement audit trails in these methods
    /// </summary>
    ///
    public class ChartOfAccountRepository : IChartOfAccountRepository
    {
        private FinTrakBankingContext context;
        private IAuditTrailRepository auditTrail;
        private IGeneralSetupRepository _genSetup;
        private IWorkflow workFlow;
        private IApprovalLevelStaffRepository level;
        private IIntegrationWithFinacle cwpAIP;

        public bool USE_THIRD_PARTY_INTEGRATION { get; private set; }

        public ChartOfAccountRepository(FinTrakBankingContext _context,
                                                IAuditTrailRepository _auditTrail,
                                                IGeneralSetupRepository genSetup,
                                                IWorkflow _workFlow,
                                                IApprovalLevelStaffRepository _level,
                                                     IIntegrationWithFinacle _cwpAIP)
        {
            this.context = _context;
            this._genSetup = genSetup;
            auditTrail = _auditTrail;
            this.workFlow = _workFlow;
            cwpAIP = _cwpAIP;
            level = _level;

            var globalSetting = context.TBL_SETUP_GLOBAL.FirstOrDefault();
            USE_THIRD_PARTY_INTEGRATION = globalSetting.USE_THIRD_PARTY_INTEGRATION;

        }

        private bool SaveAll()
        {
            return this.context.SaveChanges() > 0;
        }

        public GLAccountDetailsViewModel ValidateGLNumber(string glNumber)
        {
            var data = cwpAIP.ValidateGLNumber(glNumber);

            if (data  != null)
                return data;
            else
                throw new SecureException("Office Account Does Not Exist in Finaco");
        }

        public bool GoForApproval(ApprovalViewModel entity)
        {
            entity.operationId = (int)OperationsEnum.ChartOfAccountCreation;

            entity.externalInitialization = false;

            using (var trans = context.Database.BeginTransaction())
            {
                try
                {
                    workFlow.LogForApproval(entity);
                    var b = workFlow.NextLevelId ?? 0;
                    if (b == 0 && workFlow.NewState != (int)ApprovalState.Ended) // check if this is the last level
                    {
                        trans.Rollback();
                        throw new SecureException("Approval Failed");
                    }

                    if (workFlow.NewState == (int)ApprovalState.Ended)
                    {
                        var response = ApproveAccount(entity.targetId, (short)workFlow.StatusId, entity);

                        if (response)
                        {
                            trans.Commit();
                        }
                        return true;
                    }
                    else
                    {
                        trans.Commit();
                    }

                    return false;
                }
                catch (Exception ex)
                {
                    trans.Rollback();
                    throw new SecureException(ex.Message);
                }
            }
        }

        private bool ApproveAccount(int accountId, short approvalStatusId, UserInfo user)
        {
            var accountModel = context.TBL_TEMP_CHART_OF_ACCOUNT.Find(accountId);
            var accountToUpdate = context.TBL_CHART_OF_ACCOUNT.Where(x => x.ACCOUNTCODE == accountModel.ACCOUNTCODE);
            var existingAccount = accountToUpdate.FirstOrDefault();

            var currModel = context.TBL_TEMP_CHART_OF_ACCOUNT_CUR.Where(c => c.GLACCOUNTID == accountModel.GLACCOUNTID && c.DELETED == false);
            var currListToUpdate = new List<TBL_CHART_OF_ACCOUNT_CURRENCY>();

            List<TBL_CHART_OF_ACCOUNT_CURRENCY> coaCurrencies = new List<TBL_CHART_OF_ACCOUNT_CURRENCY>();

            if (existingAccount != null) //Update existing account with tempAccount record
            {
                currListToUpdate = context.TBL_CHART_OF_ACCOUNT_CURRENCY.Where(x => x.GLACCOUNTID == existingAccount.GLACCOUNTID && x.DELETED == false).ToList();

                foreach (var curr in currListToUpdate)
                {
                    context.TBL_CHART_OF_ACCOUNT_CURRENCY.Remove(curr);
                }

                foreach (var c in currModel)
                {
                    var curr = new TBL_CHART_OF_ACCOUNT_CURRENCY()
                    {
                        //GLAccountId = c.GLAccountId,
                        CURRENCYID = c.CURRENCYID,
                        DATETIMECREATED = _genSetup.GetApplicationDate(),
                        DATETIMEUPDATED = DateTime.Now,
                        DELETED = false
                    };
                    coaCurrencies.Add(curr);
                }

                if (accountModel != null)
                {
                    existingAccount.ACCOUNTCODE = accountModel.ACCOUNTCODE;
                    existingAccount.ACCOUNTNAME = accountModel.ACCOUNTNAME;
                    existingAccount.ACCOUNTTYPEID = accountModel.ACCOUNTTYPEID;
                    existingAccount.COMPANYID = accountModel.COMPANYID;
                    existingAccount.BRANCHID = accountModel.BRANCHID;
                    existingAccount.SYSTEMUSE = accountModel.SYSTEMUSE;
                    existingAccount.BRANCHSPECIFIC = accountModel.BRANCHSPECIFIC;
                    existingAccount.FSCAPTIONID = accountModel.FSCAPTIONID;
                    existingAccount.ACCOUNTSTATUSID = accountModel.ACCOUNTSTATUSID;
                    existingAccount.CREATEDBY = accountModel.CREATEDBY;
                    existingAccount.DATETIMEUPDATED = DateTime.Now;
                    existingAccount.TBL_CHART_OF_ACCOUNT_CURRENCY = coaCurrencies;
                    existingAccount.GLCLASSID = (short)accountModel.GLCLASSID;
                    existingAccount.DELETED = false;
                }
            }
            else //Insert a new account record into the real account table
            {
                foreach (var c in currModel)
                {
                    var curr = new TBL_CHART_OF_ACCOUNT_CURRENCY()
                    {
                        //GLAccountId = c.GLAccountId,
                        CURRENCYID = c.CURRENCYID,
                        DATETIMECREATED = _genSetup.GetApplicationDate(),
                        DELETED = false
                    };
                    coaCurrencies.Add(curr);
                }

                if (accountModel != null)
                {
                    var account = new TBL_CHART_OF_ACCOUNT()
                    {
                        ACCOUNTCODE = accountModel.ACCOUNTCODE,
                        ACCOUNTNAME = accountModel.ACCOUNTNAME,
                        ACCOUNTTYPEID = accountModel.ACCOUNTTYPEID,
                        COMPANYID = accountModel.COMPANYID,
                        BRANCHID = accountModel.BRANCHID,
                        SYSTEMUSE = accountModel.SYSTEMUSE,
                        BRANCHSPECIFIC = accountModel.BRANCHSPECIFIC,
                        FSCAPTIONID = accountModel.FSCAPTIONID,
                        ACCOUNTSTATUSID = accountModel.ACCOUNTSTATUSID,
                        DATETIMECREATED = _genSetup.GetApplicationDate(),
                        CREATEDBY = accountModel.CREATEDBY,
                        TBL_CHART_OF_ACCOUNT_CURRENCY = coaCurrencies,
                        GLCLASSID = (short)accountModel.GLCLASSID,
                        DELETED = false
                    };
                    context.TBL_CHART_OF_ACCOUNT.Add(account);
                }
            }

            accountModel.ISCURRENT = false;
            accountModel.APPROVALSTATUSID = approvalStatusId;
            accountModel.DATETIMEUPDATED = DateTime.Now;

            // remove the temp chart of account and currency
            //context.tbl_Temp_Chart_Of_Account.Remove(accountModel);

            //foreach (var curr in currModel)
            //{
            //    context.tbl_Temp_Chart_Of_Account_Currency.Remove(curr);
            //}

            // Audit Section ---------------------------
            var audit = new TBL_AUDIT
            {
                AUDITTYPEID = (short)AuditTypeEnum.AccountApproved,
                STAFFID = user.staffId,
                BRANCHID = (short)user.BranchId,
                DETAIL = $"Approved Account '{accountModel.ACCOUNTNAME}' with staff code'{accountModel.ACCOUNTCODE}'",
                IPADDRESS = CommonHelpers.GetLocalIpAddress(),
                URL = user.applicationUrl,
                APPLICATIONDATE = _genSetup.GetApplicationDate(),
                SYSTEMDATETIME = DateTime.Now,
                DEVICENAME = CommonHelpers.GetDeviceName(),
                OSNAME = CommonHelpers.FriendlyName()
            };

            try
            {
                auditTrail.AddAuditTrail(audit);
                // Audit Section ---------------------------

                var response = context.SaveChanges() > 0;

                if (response)
                {
                    return true;
                }
                return false;
            }
            catch (Exception ex)
            {
                throw new SecureException(ex.Message);
            }
        }

        private int AddAccount2(ChartOfAccountViewModel account)
        {
            ValidateAccountId(account.accountCode);

            if (account.currencies.Count < 1)
                throw new SecureException("Chart of Account Currency must be specified");

            List<TBL_CHART_OF_ACCOUNT_CURRENCY> currencies = new List<TBL_CHART_OF_ACCOUNT_CURRENCY>();

            //Storing the chart of account currencies
            foreach (var item in account.currencies)
            {
                var chartOfAccountCurrency = new TBL_CHART_OF_ACCOUNT_CURRENCY()
                {
                    CURRENCYID = item.currencyId,
                    //GlaccountId = chartOfAccount.GlaccountId,
                    CREATEDBY = item.createdBy,
                    DATETIMECREATED = _genSetup.GetApplicationDate()
                };

                currencies.Add(chartOfAccountCurrency);
            }
            //End of storing the chart of account currencies
            var chartOfAccount = new TBL_CHART_OF_ACCOUNT()
            {
                ACCOUNTCODE = account.accountCode,
                ACCOUNTNAME = account.accountName,
                ACCOUNTTYPEID = account.accountTypeId,
                COMPANYID = account.companyId,
                BRANCHID = account.branchId,
                SYSTEMUSE = account.systemUse,
                BRANCHSPECIFIC = account.branchSpecific,
                FSCAPTIONID = account.fsCaptionId,
                ACCOUNTSTATUSID = account.accountStatusId,

                CREATEDBY = account.createdBy,
                DATETIMECREATED = _genSetup.GetApplicationDate(),
                TBL_CHART_OF_ACCOUNT_CURRENCY = currencies,
                GLCLASSID = account.glClassId
            };

            this.context.TBL_CHART_OF_ACCOUNT.Add(chartOfAccount);
            // Audit Section ---------------------------
            var audit = new TBL_AUDIT
            {
                AUDITTYPEID = (short)AuditTypeEnum.ChartOfAccountUpdated,
                STAFFID = (int)account.createdBy,
                BRANCHID = (short)account.userBranchId,
                DETAIL = $"Added New Account: {account.accountName} with code: {account.accountCode}",
                IPADDRESS = CommonHelpers.GetLocalIpAddress(),
                URL = account.applicationUrl,
                SYSTEMDATETIME = DateTime.Now,
                APPLICATIONDATE = _genSetup.GetApplicationDate(),
                DEVICENAME = CommonHelpers.GetDeviceName(),
                OSNAME = CommonHelpers.FriendlyName()
            };
            this.auditTrail.AddAuditTrail(audit);
            //end of Audit section -------------------------------
            var status = this.SaveAll();

            if (status)
                return chartOfAccount.GLACCOUNTID;
            else
                return -1;
        }

        public bool AddTempAccount(ChartOfAccountViewModel accountModel)
        {
            //if (USE_THIRD_PARTY_INTEGRATION)
            //{
            //    if (cwpAIP.ValidateGLNumber(accountModel.accountCode) == null)
            //        throw new SecureException($"Account Number {accountModel.accountCode} does not exist on the core banking application");
            //}


            if (accountModel.currencies.Count < 1)
                throw new SecureException("Chart of Account Currency must be specified");

            List<TBL_TEMP_CHART_OF_ACCOUNT_CUR> currencies = new List<TBL_TEMP_CHART_OF_ACCOUNT_CUR>();

            bool output = false;
            var existStingTempAccount = context.TBL_TEMP_CHART_OF_ACCOUNT.Where(x => x.ACCOUNTCODE.ToLower() == accountModel.accountCode.ToLower()
                                                                  && x.ISCURRENT == true && x.COMPANYID == accountModel.companyId
                                                                  && x.APPROVALSTATUSID == (short)ApprovalStatusEnum.Pending);

            if (existStingTempAccount.Any())
            {
                throw new SecureException("Account Information already exist and is undergoing approval");
            }

            //Storing the chart of account currencies
            foreach (var item in accountModel.currencies)
            {
                var chartOfAccountCurrency = new TBL_TEMP_CHART_OF_ACCOUNT_CUR()
                {
                    //GLAccountId = item.glaccountId,
                    CURRENCYID = item.currencyId,
                    CREATEDBY = item.createdBy,
                    DATETIMECREATED = _genSetup.GetApplicationDate(),
                    DELETED = false
                };
                currencies.Add(chartOfAccountCurrency);
            }
            //End of storing the chart of account currencies

            var account = new TBL_TEMP_CHART_OF_ACCOUNT()
            {
                ACCOUNTCODE = accountModel.accountCode,
                ACCOUNTNAME = accountModel.accountName,
                ACCOUNTTYPEID = accountModel.accountTypeId,
                COMPANYID = accountModel.companyId,
                BRANCHID = accountModel.branchId,
                SYSTEMUSE = accountModel.systemUse,
                BRANCHSPECIFIC = accountModel.branchSpecific,
                FSCAPTIONID = accountModel.fsCaptionId,
                ACCOUNTSTATUSID = accountModel.accountStatusId,
                CREATEDBY = accountModel.createdBy,
                DATETIMECREATED = _genSetup.GetApplicationDate(),
                APPROVALSTATUSID = (short)ApprovalStatusEnum.Pending,
                ISCURRENT = true,
                TBL_TEMP_CHART_OF_ACCOUNT_CUR = currencies,
                GLCLASSID = accountModel.glClassId,
                DELETED = false
            };

            // Audit Section ---------------------------
            var audit = new TBL_AUDIT
            {
                AUDITTYPEID = (short)AuditTypeEnum.ChartOfAccountInitiated,
                STAFFID = accountModel.createdBy,
                BRANCHID = (short)accountModel.userBranchId,
                DETAIL = $"Initiated Chart of Account Creation for '{accountModel.accountName}' with code'{accountModel.accountCode}'",
                IPADDRESS = CommonHelpers.GetLocalIpAddress(),
                URL = accountModel.applicationUrl,
                APPLICATIONDATE = _genSetup.GetApplicationDate(),
                SYSTEMDATETIME = DateTime.Now,
                DEVICENAME = CommonHelpers.GetDeviceName(),
                OSNAME = CommonHelpers.FriendlyName()
            };

            using (var trans = context.Database.BeginTransaction())
            {
                try
                {
                    auditTrail.AddAuditTrail(audit);
                    context.TBL_TEMP_CHART_OF_ACCOUNT.Add(account);
                    output =  context.SaveChanges() > 0;

                    var entity = new ApprovalViewModel
                    {
                        staffId = accountModel.createdBy,
                        companyId = accountModel.companyId,
                        approvalStatusId = (int)ApprovalStatusEnum.Pending,
                        targetId = account.GLACCOUNTID,
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
                    throw new SecureException(ex.Message);
                }
            }
        }

        public bool IsAccountCodeAlreadyExist(string accountCode)
        {
            return context.TBL_CHART_OF_ACCOUNT.Any(x => x.ACCOUNTCODE.ToLower() == accountCode.ToLower());
        }

        public bool IsTempAccountExist(string accountCode)
        {
            return context.TBL_TEMP_CHART_OF_ACCOUNT.Any(x => x.ACCOUNTCODE.ToLower() == accountCode.ToLower() && x.APPROVALSTATUSID == (int)ApprovalStatusEnum.Pending && x.ISCURRENT == true);
        }

        private IQueryable<ChartOfAccountViewModel> GetAllAccountsDetails()
        {
            var data = (from account in context.TBL_CHART_OF_ACCOUNT
                        where account.DELETED == false
                        orderby account.TBL_ACCOUNT_TYPE.TBL_ACCOUNT_CATEGORY.ACCOUNTCATEGORYNAME ascending
                        select new ChartOfAccountViewModel()
                        {
                            accountId = account.GLACCOUNTID,
                            accountCode = account.ACCOUNTCODE,
                            accountName = account.ACCOUNTNAME,
                            accountTypeId = account.ACCOUNTTYPEID,
                            accountTypeName = account.TBL_ACCOUNT_TYPE.ACCOUNTTYPENAME,
                            accountCategoryId = account.TBL_ACCOUNT_TYPE.ACCOUNTCATEGORYID,
                            accountCategoryName = account.TBL_ACCOUNT_TYPE.TBL_ACCOUNT_CATEGORY.ACCOUNTCATEGORYNAME,
                            accountStatusId = account.ACCOUNTSTATUSID,
                            currencies = context.TBL_CHART_OF_ACCOUNT_CURRENCY.Where(curr => curr.GLACCOUNTID == account.GLACCOUNTID && curr.DELETED == false).Select(c => new ChartOfAccountCurrencyViewModel()
                            {
                                glaccountId = c.GLACCOUNTID,
                                glaccountCurrencyId = c.GLACCOUNTCURRENCYID,
                                currencyId = c.CURRENCYID,
                                currencyName = c.TBL_CURRENCY.CURRENCYCODE + " -- " + c.TBL_CURRENCY.CURRENCYNAME
                            }).ToList(),
                            companyId = account.COMPANYID,
                            branchId = account.BRANCHID,

                            systemUse = account.SYSTEMUSE,
                            branchSpecific = account.BRANCHSPECIFIC,
                            fsCaptionId = (short) account.FSCAPTIONID,

                            createdBy = account.CREATEDBY,
                            dateTimeCreated = account.DATETIMECREATED,
                            glClassId = account.GLCLASSID

                            // lastUpdatedBy = account.LastUpdatedBy.Value ,
                            // dateTimeUpdated = account.DateTimeUpdated
                        });

            return data;
        }

        public IEnumerable<ChartOfAccountViewModel> GetAllAccounts()
        {
            var data = GetAllAccountsDetails().OrderByDescending(x => x.accountCode);

            return data;
        }

        public string GetAccountNameByAccountCode(string accountCode)
        {
            return (from c in context.TBL_CHART_OF_ACCOUNT
                    where c.ACCOUNTCODE.Trim() == accountCode.Trim()
                    select c.ACCOUNTNAME).FirstOrDefault();
        }

        public IEnumerable<ChartOfAccountViewModel> GetAccountsByCategory(short accountCategoryId)
        {
            var data = GetAllAccountsDetails().Where(x => x.accountCategoryId == accountCategoryId).ToList();

            return data;
        }

        public ChartOfAccountViewModel GetAccountByAccountId(short accountId)
        {
            var account = this.context.TBL_CHART_OF_ACCOUNT.FirstOrDefault(x => x.GLACCOUNTID == accountId && x.DELETED == false); // .Find(accountId);

            if (account == null)
                return null;

            return new ChartOfAccountViewModel
            {
                accountId = account.GLACCOUNTID,
                accountCode = account.ACCOUNTCODE,
                accountName = account.ACCOUNTNAME,
                accountTypeId = account.ACCOUNTTYPEID,
                companyId = account.COMPANYID,
                branchId = account.BRANCHID,
                systemUse = account.SYSTEMUSE,
                branchSpecific = account.BRANCHSPECIFIC,
                fsCaptionId = (short) account.FSCAPTIONID,
                createdBy = account.CREATEDBY,
                dateTimeCreated = account.DATETIMECREATED,
                lastUpdatedBy = account.LASTUPDATEDBY.Value,
                dateTimeUpdated = account.DATETIMEUPDATED
            };
        }

        public int GetAccountDefaultCurrency(int glAccountId, int companyId)
        {
            int output;
            var setup = context.TBL_SETUP_GLOBAL.FirstOrDefault();
            if (setup.USE_THIRD_PARTY_INTEGRATION == false)
            {
                output = context.TBL_COMPANY.FirstOrDefault(x => x.COMPANYID == companyId).CURRENCYID;
            }
            else
            {
                 output = (from gl in context.TBL_CUSTOM_CHART_OF_ACCOUNT
                                   join gla in context.TBL_CHART_OF_ACCOUNT on gl.PLACEHOLDERID equals gla.ACCOUNTCODE
                                   join cur in context.TBL_CURRENCY on gl.CURRENCYCODE equals cur.CURRENCYCODE
                                   where gla.GLACCOUNTID == glAccountId
                                   select cur.CURRENCYID).FirstOrDefault();
            }

            return output;  
        }

        private bool UpdateAccount2(short accountId, ChartOfAccountViewModel account)
        {
            ValidateAccountId(account.accountCode);

            var accountModel = this.context.TBL_CHART_OF_ACCOUNT.Find(accountId);

            if (accountModel == null)
                return false;

            accountModel.ACCOUNTCODE = account.accountCode;
            accountModel.ACCOUNTNAME = account.accountName;
            accountModel.ACCOUNTTYPEID = account.accountTypeId;
            accountModel.COMPANYID = account.companyId;
            accountModel.BRANCHID = account.branchId;
            accountModel.SYSTEMUSE = account.systemUse;
            accountModel.BRANCHSPECIFIC = account.branchSpecific;
            accountModel.FSCAPTIONID = account.fsCaptionId;
            accountModel.ACCOUNTSTATUSID = account.accountStatusId;

            accountModel.LASTUPDATEDBY = account.lastUpdatedBy;
            accountModel.DATETIMEUPDATED = _genSetup.GetApplicationDate();

            //Account Currencies Update
            foreach (var currency in account.currencies)
            {
                var data = context.TBL_CHART_OF_ACCOUNT_CURRENCY.Where(c => c.CURRENCYID == currency.currencyId).FirstOrDefault();
                data.CURRENCYID = currency.currencyId;
                //data.GlaccountId = currency.glaccountId;
            }
            //End of account currencies update

            // Audit Section ---------------------------
            var audit = new TBL_AUDIT
            {
                AUDITTYPEID = (short)AuditTypeEnum.ChartOfAccountUpdated,
                STAFFID = (int)account.createdBy,
                BRANCHID = (short)account.userBranchId,
                DETAIL = $"Updated New Account: {accountModel.ACCOUNTNAME} with code: {accountModel.ACCOUNTCODE}",
                IPADDRESS = CommonHelpers.GetLocalIpAddress(),
                URL = account.applicationUrl,
                SYSTEMDATETIME = DateTime.Now,
                APPLICATIONDATE = _genSetup.GetApplicationDate(),
                DEVICENAME = CommonHelpers.GetDeviceName(),
                OSNAME = CommonHelpers.FriendlyName()
            };
            this.auditTrail.AddAuditTrail(audit);
            //end of Audit section -------------------------------

            return this.SaveAll();

            //throw new NotImplementedException();
        }

        private void ValidateAccountId(string accountCode)
        {
            var applicationSetup = context.TBL_SETUP_GLOBAL.FirstOrDefault();
            if (applicationSetup.USE_THIRD_PARTY_INTEGRATION == true)
            {
                var placeholder = context.TBL_CUSTOM_CHART_OF_ACCOUNT.Where(x => x.ACCOUNTID == accountCode);
                if (placeholder.Any() == false)
                {
                    throw new SecureException("The pecified account id do not exist!");
                }
            }
        }

        public bool UpdateAccount(short accountId, ChartOfAccountViewModel accountModel)
        {
            if (accountModel == null)
                return false;

            var targetAccountId = 0;

            var existingTempAccount = context.TBL_TEMP_CHART_OF_ACCOUNT
                .FirstOrDefault(x => x.ACCOUNTCODE.ToLower() == accountModel.accountCode.ToLower()
                && x.ISCURRENT == true && x.APPROVALSTATUSID == (int)ApprovalStatusEnum.Approved);

            var existingTempCurrencies = new List<TBL_TEMP_CHART_OF_ACCOUNT_CUR>();

            TBL_TEMP_CHART_OF_ACCOUNT tempAccount = new TBL_TEMP_CHART_OF_ACCOUNT();
            List<TBL_TEMP_CHART_OF_ACCOUNT_CUR> tempCurrencies = new List<TBL_TEMP_CHART_OF_ACCOUNT_CUR>();

            var unApprovedAccountEdit = context.TBL_TEMP_CHART_OF_ACCOUNT
                .Where(x => x.ISCURRENT == true && x.APPROVALSTATUSID == (int)ApprovalStatusEnum.Pending &&
            x.ACCOUNTCODE.ToLower() == accountModel.accountCode.ToLower());

            if (unApprovedAccountEdit.Any())
            {
                throw new SecureException("Chart of Account is already undergoing approval");
            }

            if (existingTempAccount != null)
            {
                existingTempCurrencies = context.TBL_TEMP_CHART_OF_ACCOUNT_CUR.Where(x => x.GLACCOUNTID == existingTempAccount.GLACCOUNTID).ToList();

                if (existingTempCurrencies.Count > 0)
                {
                    foreach (var curr in existingTempCurrencies)
                    {
                        context.TBL_TEMP_CHART_OF_ACCOUNT_CUR.Remove(curr);
                    }
                }

                foreach (var item in accountModel.currencies)
                {
                    var chartOfAccountCurrency = new TBL_TEMP_CHART_OF_ACCOUNT_CUR()
                    {
                        //GLAccountId = item.glaccountId,
                        CURRENCYID = item.currencyId,
                        CREATEDBY = accountModel.createdBy,
                        DATETIMECREATED = _genSetup.GetApplicationDate(),
                        DELETED = false
                    };
                    tempCurrencies.Add(chartOfAccountCurrency);
                }
                //End of storing the updated chart of account currencies

                var tempAccountToUpdate = existingTempAccount;

                tempAccountToUpdate.ACCOUNTCODE = accountModel.accountCode;
                tempAccountToUpdate.ACCOUNTNAME = accountModel.accountName;
                tempAccountToUpdate.ACCOUNTTYPEID = accountModel.accountTypeId;
                tempAccountToUpdate.COMPANYID = accountModel.companyId;
                tempAccountToUpdate.BRANCHID = accountModel.userBranchId;
                tempAccountToUpdate.SYSTEMUSE = accountModel.systemUse;
                tempAccountToUpdate.BRANCHSPECIFIC = accountModel.branchSpecific;
                tempAccountToUpdate.FSCAPTIONID = accountModel.fsCaptionId;
                tempAccountToUpdate.ACCOUNTSTATUSID = accountModel.accountStatusId;
                //GLAccountId = accountId,
                tempAccountToUpdate.CREATEDBY = accountModel.createdBy;
                tempAccountToUpdate.DATETIMEUPDATED = DateTime.Now;
                tempAccountToUpdate.APPROVALSTATUSID = (int)ApprovalStatusEnum.Pending;
                tempAccountToUpdate.ISCURRENT = true;
                tempAccountToUpdate.GLCLASSID = accountModel.glClassId;
                tempAccountToUpdate.DELETED = false;

                tempAccountToUpdate.TBL_TEMP_CHART_OF_ACCOUNT_CUR = tempCurrencies;
            }
            else
            {
                var targetAccount = context.TBL_CHART_OF_ACCOUNT.Find(accountId);

                //Storing the updated chart of account currencies
                foreach (var item in accountModel.currencies)
                {
                    var chartOfAccountCurrency = new TBL_TEMP_CHART_OF_ACCOUNT_CUR()
                    {
                        //GLAccountId = item.glaccountId,
                        CURRENCYID = item.currencyId,
                        CREATEDBY = accountModel.createdBy,
                        DATETIMECREATED = _genSetup.GetApplicationDate(),
                        DELETED = false
                    };
                    tempCurrencies.Add(chartOfAccountCurrency);
                }
                //End of storing the updated chart of account currencies

                tempAccount = new TBL_TEMP_CHART_OF_ACCOUNT()
                {
                    ACCOUNTCODE = targetAccount?.ACCOUNTCODE,
                    ACCOUNTNAME = accountModel.accountName,
                    ACCOUNTTYPEID = accountModel.accountTypeId,
                    COMPANYID = accountModel.companyId,
                    BRANCHID = accountModel.userBranchId,
                    SYSTEMUSE = accountModel.systemUse,
                    BRANCHSPECIFIC = accountModel.branchSpecific,
                    FSCAPTIONID = accountModel.fsCaptionId,
                    ACCOUNTSTATUSID = accountModel.accountStatusId,
                    //GLAccountId = accountId,
                    CREATEDBY = accountModel.createdBy,
                    DATETIMECREATED = _genSetup.GetApplicationDate(),
                    APPROVALSTATUSID = (int)ApprovalStatusEnum.Pending,
                    ISCURRENT = true,
                    GLCLASSID = accountModel.glClassId,
                    DELETED = false,

                    TBL_TEMP_CHART_OF_ACCOUNT_CUR = tempCurrencies,
                };

                context.TBL_TEMP_CHART_OF_ACCOUNT.Add(tempAccount);
            }

            var audit = new TBL_AUDIT
            {
                AUDITTYPEID = (short)AuditTypeEnum.ChartOfAccountUpdated,
                STAFFID = accountModel.createdBy,
                BRANCHID = (short)accountModel.userBranchId,
                DETAIL = $"Initiated updated of Chart Of Account '{accountModel.accountName}' with code'{accountModel.accountCode}'",
                IPADDRESS = CommonHelpers.GetLocalIpAddress(),
                URL = accountModel.applicationUrl,
                APPLICATIONDATE = _genSetup.GetApplicationDate(),
                SYSTEMDATETIME = DateTime.Now,
                TARGETID = accountId,
                DEVICENAME = CommonHelpers.GetDeviceName(),
                OSNAME = CommonHelpers.FriendlyName()
            };

            using (var trans = context.Database.BeginTransaction())
            {
                try
                {
                    this.auditTrail.AddAuditTrail(audit);
                    //end of Audit section -------------------------------

                    var output = this.SaveAll();

                    targetAccountId = existingTempAccount?.GLACCOUNTID ?? tempAccount.GLACCOUNTID;

                    var entity = new ApprovalViewModel
                    {
                        staffId = accountModel.createdBy,
                        companyId = accountModel.companyId,
                        approvalStatusId = (int)ApprovalStatusEnum.Pending,
                        targetId = targetAccountId,
                        operationId = (int)OperationsEnum.ChartOfAccountCreation,
                        BranchId = accountModel.userBranchId,
                        externalInitialization = true
                    };
                    bool response = workFlow.LogForApproval(entity);

                    if (response)
                    {
                        trans.Commit();

                        return output;
                    }
                    return false;
                }
                catch (Exception ex)
                {
                    trans.Rollback();
                    throw new SecureException(ex.Message);
                }
            }
        }

        public IEnumerable<ChartOfAccountViewModel> GetAccountsAwaitingApprovals(int staffId, int companyId)
        {
            var ids = _genSetup.GetStaffApprovalLevelIds(staffId, (int)OperationsEnum.ChartOfAccountCreation).ToList();

            var data = (from c in context.TBL_TEMP_CHART_OF_ACCOUNT
                        join coy in context.TBL_COMPANY on c.COMPANYID equals companyId
                        join atrail in context.TBL_APPROVAL_TRAIL on c.GLACCOUNTID equals atrail.TARGETID
                        where atrail.APPROVALSTATUSID == (int)ApprovalStatusEnum.Pending
                            && c.ISCURRENT == true && atrail.RESPONSESTAFFID == null
                              && atrail.OPERATIONID == (int)OperationsEnum.ChartOfAccountCreation 
                              && ids.Contains((int)atrail.TOAPPROVALLEVELID)
                        select new ChartOfAccountViewModel()
                        {
                            accountId = c.GLACCOUNTID,
                            accountCode = c.ACCOUNTCODE,
                            accountName = c.ACCOUNTNAME,
                            accountTypeId = c.ACCOUNTTYPEID,
                            accountTypeName = c.TBL_ACCOUNT_TYPE.ACCOUNTTYPENAME,
                            accountCategoryId = c.TBL_ACCOUNT_TYPE.ACCOUNTCATEGORYID,
                            accountCategoryName = c.TBL_ACCOUNT_TYPE.TBL_ACCOUNT_CATEGORY.ACCOUNTCATEGORYNAME,
                            accountStatusId = c.ACCOUNTSTATUSID,
                            currencies = context.TBL_TEMP_CHART_OF_ACCOUNT_CUR
                                .Where(curr => curr.GLACCOUNTID == c.GLACCOUNTID && curr.DELETED == false).Select(coa =>
                                    new ChartOfAccountCurrencyViewModel()
                                    {
                                        glaccountId = coa.GLACCOUNTID,
                                        glaccountCurrencyId = coa.GLACCOUNTCURRENCYID,
                                        currencyId = coa.CURRENCYID,
                                        currencyName = coa.TBL_CURRENCY.CURRENCYCODE + " -- " + coa.TBL_CURRENCY.CURRENCYNAME
                                    }).ToList(),
                            companyId = c.COMPANYID,
                            branchId = c.BRANCHID,
                            branchName = c.TBL_BRANCH.BRANCHNAME,
                            systemUse = c.SYSTEMUSE,
                            branchSpecific = c.BRANCHSPECIFIC,
                            fsCaptionId = (short) c.FSCAPTIONID,
                            fsCaptionName = c.TBL_FINANCIAL_STATEMENT_CAPTN.FSCAPTION,
                            operationId = atrail.OPERATIONID,
                            //approvalStatusId = c.ApprovalStatusId,
                            createdBy = c.CREATEDBY,
                            dateTimeCreated = c.DATETIMECREATED,
                            glClassId = (short)c.GLCLASSID
                        }).GroupBy(x => x.accountId).Select(g => g.FirstOrDefault());

            return data;
        }

        public ChartOfAccountViewModel GetTempAccountDetail(int accountId)
        {
            //return GetTempStaffDetails().Where(x => x.StaffId == staffId).Single();

            return (from c in context.TBL_TEMP_CHART_OF_ACCOUNT
                    join coy in context.TBL_COMPANY on c.COMPANYID equals coy.COMPANYID
                    where c.GLACCOUNTID == accountId
                    select new ChartOfAccountViewModel()
                    {
                        accountId = c.GLACCOUNTID,
                        accountCode = c.ACCOUNTCODE,
                        accountName = c.ACCOUNTNAME,
                        accountTypeId = c.ACCOUNTTYPEID,
                        accountTypeName = c.TBL_ACCOUNT_TYPE.ACCOUNTTYPENAME,
                        accountCategoryId = c.TBL_ACCOUNT_TYPE.ACCOUNTCATEGORYID,
                        accountCategoryName = c.TBL_ACCOUNT_TYPE.TBL_ACCOUNT_CATEGORY.ACCOUNTCATEGORYNAME,
                        accountStatusId = c.ACCOUNTSTATUSID,
                        currencies = context.TBL_CHART_OF_ACCOUNT_CURRENCY.Where(curr => curr.GLACCOUNTID == c.GLACCOUNTID && curr.DELETED == false).Select(coa => new ChartOfAccountCurrencyViewModel()
                        {
                            glaccountId = coa.GLACCOUNTID,
                            glaccountCurrencyId = coa.GLACCOUNTCURRENCYID,
                            currencyId = coa.CURRENCYID,
                            currencyName = coa.TBL_CURRENCY.CURRENCYCODE + " -- " + coa.TBL_CURRENCY.CURRENCYNAME
                        }).ToList(),
                        companyId = c.COMPANYID,
                        branchId = c.BRANCHID,

                        systemUse = c.SYSTEMUSE,
                        branchSpecific = c.BRANCHSPECIFIC,
                        fsCaptionId = (short)c.FSCAPTIONID,

                        createdBy = c.CREATEDBY,
                        dateTimeCreated = c.DATETIMECREATED,
                    }).FirstOrDefault();
        }

        public bool DeleteAccount(short accountId, UserInfo user)
        {
            var accountModel = this.context.TBL_CHART_OF_ACCOUNT.Find(accountId);

            if (accountModel == null)
                return false;

            accountModel.DELETED = true;
            //accountModel.DeletedBy = ;
            accountModel.DATETIMEDELETED = _genSetup.GetApplicationDate();

            var audit = new TBL_AUDIT
            {
                AUDITTYPEID = (short)AuditTypeEnum.ChartOfAccountDeleted,
                STAFFID = user.createdBy,
                BRANCHID = (short)user.BranchId,
                DETAIL = $"Deleted New Account: {accountModel.ACCOUNTNAME} with code: {accountModel.ACCOUNTCODE}",
                IPADDRESS = CommonHelpers.GetLocalIpAddress(),
                URL = user.applicationUrl,
                SYSTEMDATETIME = DateTime.Now,
                APPLICATIONDATE = _genSetup.GetApplicationDate(),
                DEVICENAME = CommonHelpers.GetDeviceName(),
                OSNAME = CommonHelpers.FriendlyName()
            };
            this.auditTrail.AddAuditTrail(audit);
            //end of Audit section -------------------------------
            return this.SaveAll();

            //throw new NotImplementedException();
        }

        public IEnumerable<LookupViewModel> GetFinancialSatementCaptionLookup()
        {
            return (from data in context.TBL_FINANCIAL_STATEMENT_CAPTN
                    where data.ISTOTALLINE == false
                    orderby data.FINTYPE, data.POSITION
                    select new LookupViewModel()
                    {
                        lookupId = (short) data.FSCAPTIONID,
                        lookupName = data.FSCAPTION + " -- " + data.TBL_ACCOUNT_CATEGORY.ACCOUNTCATEGORYNAME
                    });
            
        }

        public IEnumerable<ChartOfAccountClassViewModel> GetChartOfAccountClasses()
        {
            return (from data in context.TBL_CHART_OF_ACCOUNT_CLASS
                    select new ChartOfAccountClassViewModel()
                    {
                        glClassId = data.GLCLASSID,
                        glClassName = data.GLCLASSNAME
                    });
        }
    }
}