using FintrakBanking.Entities.Models;
using FintrakBanking.Interfaces.Credit;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FintrakBanking.ViewModels.CASA;
using FintrakBanking.ViewModels.Customer;
using FintrakBanking.ViewModels.Finance;
using FintrakBanking.ViewModels.ThridPartyIntegration;
using Newtonsoft.Json;
namespace FinTrakBanking.ThirdPartyIntegration
{
    using CustomerInfo;
    using Finacle;
    using OverDraftTransactions;
    using ForeignCurrencyAccountCreation;
    using AccountInformation;
    using FintrakBanking.Common.CustomException;
    using static FinTrakBanking.ThirdPartyIntegration.TwoFactorAuthIntegration.TwoFactorAuthIntegrationService;
    using FintrakBanking.Common.Enum;
    using FintrakBanking.ViewModels.Admin;
    using FinTrakBanking.ThirdPartyIntegration.StaffInfo;

    public class IntegrationWithFinacle : IIntegrationWithFinacle
    {
        private FinTrakBankingContext context;
        private TransactionPosting transaction;
        private OverDraft overDraft;
        private ForeignCurrencyAccount account;
        private CustomerDetails customer;
        private StaffDetails staff;
        private AccountDetail accountDetail;
        private ITwoFactorAuthIntegrationService twoFactorAuth;
        bool USE_TWO_FACTOR_AUTHENTICATION = false;

        public IntegrationWithFinacle(FinTrakBankingContext context, TransactionPosting transaction,
            CustomerDetails customer, StaffDetails staff, OverDraft overDraft, ForeignCurrencyAccount account, AccountDetail accountDetail,
            ITwoFactorAuthIntegrationService _twoFactorAuth)
        {
            this.context = context;
            this.transaction = transaction;
            this.customer = customer;
            this.staff = staff;
            this.overDraft = overDraft;
            this.account = account;
            this.accountDetail = accountDetail;
            this.twoFactorAuth = _twoFactorAuth;

            var globalSetting = context.TBL_SETUP_GLOBAL.FirstOrDefault();
            USE_TWO_FACTOR_AUTHENTICATION = globalSetting.USE_TWO_FACTOR_AUTHENTICATION;
        }

        #region Overdraft

        public ResponseMessageViewModel OverDraftExtend(OverDraftExtendViewModel model, TwoFactorAutheticationViewModel twoFADetails = null)
        {
            if (USE_TWO_FACTOR_AUTHENTICATION)
            {
                if (twoFADetails == null)
                    throw new TwoFactorAuthenticationException("Authentication token not specified. Specify the second factor authentication token");
                if(twoFADetails.skipAuthentication == false)
                {
                    var authenticated = twoFactorAuth.Authenticate(twoFADetails.username, twoFADetails.passcode);

                    if (authenticated.authenticated == false)
                        throw new TwoFactorAuthenticationException("Two factor authentication failed. Input the token and try again");
                }
            }

            ResponseMessage result = null;
                Task.Run(async () => result = await overDraft.APIOverDraftExtend(model)).GetAwaiter().GetResult();

            if (result.Message.IsSuccessStatusCode)
            {
                if (result.APIResponse.webRequestStatus.Replace(":", "") == "FAILURE")
                {
                    throw new SecureException(result.APIResponse.message);
                }
                else
                {
                    LogOverDraftExtend(model);
                    return result.APIResponse;
                }
            }
            else
            {
                throw new SecureException(result.Message.StatusCode + " " + result.Message.ReasonPhrase);
            }

        }

        public ResponseMessageViewModel OverDraftNormal(OverDraftNormalViewModel model, TwoFactorAutheticationViewModel twoFADetails = null)
        {
            if (USE_TWO_FACTOR_AUTHENTICATION && twoFADetails.skipAuthentication == false)
            {
                if (twoFADetails == null)
                    throw new TwoFactorAuthenticationException("Authentication token not specified. Specify the second factor authentication token");

                if(twoFADetails.skipAuthentication == false)
                {
                    var authenticated = twoFactorAuth.Authenticate(twoFADetails.username, twoFADetails.passcode);


                    if (authenticated.authenticated == false)
                        throw new TwoFactorAuthenticationException("Two factor authentication failed. Input the token and try again");
                }
              
            }

            ResponseMessage result = null;
            // if( LogOverDraftNormal(model))
            Task.Run(async () => result = await overDraft.APIOverDraftNormal(model)).GetAwaiter().GetResult();

            if (result.Message.IsSuccessStatusCode)
            {
                if (result.APIResponse.webRequestStatus.Replace(":", "") == "FAILURE")
                {
                    throw new SecureException(result.APIResponse.message);
                }
                else
                {
                    LogOverDraftNormal(model);
                    return result.APIResponse;
                }
            }
            else
            {
                throw new SecureException(result.Message.StatusCode + "" + result.Message.ReasonPhrase);
            }


        }

        public ResponseMessageViewModel OverDraftTopUp(OverDraftTopUpAndRenewViewModel model, TwoFactorAutheticationViewModel twoFADetails = null)
        {

            if (USE_TWO_FACTOR_AUTHENTICATION)
            {
                if (twoFADetails == null)
                    throw new TwoFactorAuthenticationException("Authentication token not specified. Specify the second factor authentication token");

                if(twoFADetails.skipAuthentication == false)
                {
                    var authenticated = twoFactorAuth.Authenticate(twoFADetails.username, twoFADetails.passcode);

                    if (authenticated.authenticated == false)
                        throw new TwoFactorAuthenticationException("Two factor authentication failed. Input the token and try again");
                }
               
            }

            ResponseMessage result = null;

            model.apiUrl = @"api/OverDraft/TopUp";

            //if (LogOverDraftTopUpAndRenew(model))
            Task.Run(async () => result = await overDraft.APIOverDraftTopUp(model)).GetAwaiter().GetResult();

            if (result.Message.IsSuccessStatusCode)
            {
                if (result.APIResponse.webRequestStatus.Replace(":", "") == "FAILURE")
                {
                    throw new SecureException(result.APIResponse.message);
                }
                else
                {
                    LogOverDraftTopUpAndRenew(model);
                    return result.APIResponse;
                }
            }
            else
            {
                throw new SecureException(result.Message.StatusCode + "" + result.Message.ReasonPhrase);
            }
        }

        public ResponseMessageViewModel OverDraftRenew(OverDraftTopUpAndRenewViewModel model, TwoFactorAutheticationViewModel twoFADetails = null)
        {
            if (USE_TWO_FACTOR_AUTHENTICATION)
            {
                if (twoFADetails == null)
                    throw new TwoFactorAuthenticationException("Authentication token not specified. Specify the second factor authentication token");

                if (twoFADetails.skipAuthentication == false)
                {
                    var authenticated = twoFactorAuth.Authenticate(twoFADetails.username, twoFADetails.passcode);

                    if (authenticated.authenticated == false)
                        throw new TwoFactorAuthenticationException("Two factor authentication failed. Input the token and try again");
                }
                    
            }

            ResponseMessage result = null;

            model.apiUrl = @"api/OverDraft/Renew ";

                Task.Run(async () => result = await overDraft.APIOverDraftRenew(model)).GetAwaiter().GetResult();

            if (result.Message.IsSuccessStatusCode)
            {
                if (result.APIResponse.webRequestStatus.Replace(":", "") == "FAILURE")
                {
                    throw new SecureException(result.APIResponse.message);
                }
                else
                {
                    LogOverDraftTopUpAndRenew(model);
                    return result.APIResponse;
                }
            }
            else
            {
                throw new SecureException(result.Message.StatusCode + "" + result.Message.ReasonPhrase);
            }
        }

        public ResponseMessageViewModel TemporaryOverDraftNormal(TemporaryOverDraftViewModel model, TwoFactorAutheticationViewModel twoFADetails = null)
        {

            if (USE_TWO_FACTOR_AUTHENTICATION)
            {
                if (twoFADetails == null)
                    throw new TwoFactorAuthenticationException("Authentication token not specified. Specify the second factor authentication token");

                if(twoFADetails.skipAuthentication == false)
                {
                    var authenticated = twoFactorAuth.Authenticate(twoFADetails.username, twoFADetails.passcode);

                    if (authenticated.authenticated == false)
                        throw new TwoFactorAuthenticationException("Two factor authentication failed. Input the token and try again");
                }

            }

            ResponseMessage result = null;
            model.APIUrl = @"api/TemporaryOverDraft/Normal";


            Task.Run(async () => result = await overDraft.APITemporaryOverDraftNormal(model)).GetAwaiter().GetResult();

            if (result.Message.IsSuccessStatusCode)
            {
                if (result.APIResponse.webRequestStatus.Replace(":", "") == "FAILURE")
                {
                    throw new SecureException(result.APIResponse.message);
                }
                else
                {
                    LogTemporaryOverDraft(model);
                    return result.APIResponse;
                }
            }
            else
            {
                throw new SecureException(result.Message.StatusCode + "" + result.Message.ReasonPhrase);
            }
        }

        public ResponseMessageViewModel TemporaryOverDraftRunning(TemporaryOverDraftViewModel model, TwoFactorAutheticationViewModel twoFADetails = null)
        {
            if (USE_TWO_FACTOR_AUTHENTICATION)
            {
                if (twoFADetails == null)
                    throw new TwoFactorAuthenticationException("Authentication token not specified. Specify the second factor authentication token");

                if (twoFADetails.skipAuthentication == false)
                {
                    var authenticated = twoFactorAuth.Authenticate(twoFADetails.username, twoFADetails.passcode);

                    if (authenticated.authenticated == false)
                        throw new TwoFactorAuthenticationException("Two factor authentication failed. Input the token and try again");
                }
                   
            }
            model.APIUrl = @"api/TemporaryOverDraft/Running";
            ResponseMessage result = null;

                Task.Run(async () => result = await overDraft.APITemporaryOverDraftRunning(model)).GetAwaiter().GetResult();



            if (result.Message.IsSuccessStatusCode)
            {
                if (result.APIResponse.webRequestStatus.Replace(":", "") == "FAILURE")
                {
                    throw new SecureException(result.APIResponse.message);
                }
                else
                {
                    LogTemporaryOverDraft(model);
                    return result.APIResponse;
                }
            }
            else
            {
                throw new SecureException(result.Message.StatusCode + "" + result.Message.ReasonPhrase);
            }
            //}
            //else
            //{
            //    throw new SecureException("Logging Finaco transaction failed, operation is truncated.");
            //}

        }

        public ResponseMessageViewModel TemporaryOverDraftSingle(TemporaryOverDraftViewModel model, TwoFactorAutheticationViewModel twoFADetails = null)
        {
            if (USE_TWO_FACTOR_AUTHENTICATION)
            {
                if (twoFADetails == null)
                    throw new TwoFactorAuthenticationException("Authentication token not specified. Specify the second factor authentication token");

                if(twoFADetails.skipAuthentication == false)
                {
                    var authenticated = twoFactorAuth.Authenticate(twoFADetails.username, twoFADetails.passcode);
                    if (authenticated.authenticated == false)
                        throw new TwoFactorAuthenticationException("Two factor authentication failed. Input the token and try again");
                }
            }

            model.APIUrl = @"api/TemporaryOverDraft/Single";
            ResponseMessage result = null;
                Task.Run(async () => result = await overDraft.APITemporaryOverDraftSingle(model)).GetAwaiter().GetResult();

            if (result.Message.IsSuccessStatusCode)
            {
                if (result.APIResponse.webRequestStatus.Replace(":", "") == "FAILURE")
                {
                    throw new SecureException(result.APIResponse.message);
                }
                else
                {
                    LogTemporaryOverDraft(model);
                    return result.APIResponse;
                }
            }
            else
            {
                throw new SecureException("Core Banking API error - Response Code: " + result.Message.StatusCode + ". Response Message: " + result.Message.ReasonPhrase); 
               // throw new APIErrorException("Cresult.Message.StatusCode + " " + result.Message.ReasonPhrase);
            }
        }


        #endregion


        public CurrencyExchangeRateViewModel GetExchangeRate(string fromCurrencyCode, string toCurrencyCode, string rateCode)
        {
            var data = new CurrencyExchangeRateViewModel();
            Task.Run(async () => { data = await transaction.GetExchangeRate(fromCurrencyCode, toCurrencyCode, rateCode); }).GetAwaiter().GetResult();
            return new CurrencyExchangeRateViewModel
            {
                // baseCurrencyId = baseCurrency,
                currencyId = data.currencyId,
                buyingRate = data.buyingRate,
                sellingRate = data.sellingRate,
                date = data.date,
                isBaseCurrency = false
            };
        }

        public bool GetExposePersonStatus(string customerCode)
        {
            bool result = false;
            string data = string.Empty;

            Task.Run(async () => data = await customer.CheckExposePerson(customerCode)).GetAwaiter().GetResult();

            if (data == "" || data == "NO-MATCH" || data == "NOT AVAIABLE")
                result = false;
            if (data == "Match")
                result = true;

            return result;

        }

        public BVNCustomerDetailsViewModel BVNCustomerDetails(string customerCode)
        {
            BVNCustomerDetailsViewModel data = null;
            Task.Run(async () => data = await customer.BVNCustomerDetails(customerCode)).GetAwaiter().GetResult();
            if (data != null)
                return data;
            throw new SecureException("Not Found ");

        }

        public GLAccountDetailsViewModel ValidateGLNumber(string glNumber)
        {
            GLAccountDetailsViewModel result = null;
            Task.Run(async () => result = await accountDetail.APIOfficeAccountGetGeneralLedgerAccountRecord(glNumber)).GetAwaiter().GetResult();
            if (result.response.ReasonPhrase == "OK")
            {
                return result;
            }
            return result;
        }

        public TDAccountRecordViewModel ValidateTDAccountNumber(string teamDepositAccountNumber)
        {
            TDAccountRecordViewModel result = null;
            try
            {
                Task.Run(async () => result = await accountDetail.APIOfficeAccountGetTermDepositAccountRecord(teamDepositAccountNumber)).GetAwaiter().GetResult();
                if (result.response.ReasonPhrase == "OK")
                {
                    return result;
                }
            }
            catch  {
                throw new APIErrorException(result.errorDesc);
            }
            return result;
        }

        public AccountCreationResponseMessageViewModel CreateForeignAccount(CreateAccountViewModel entity)
        {
            if (USE_TWO_FACTOR_AUTHENTICATION)
            {
                if (entity.username == null || entity.passCode == null)
                    throw new TwoFactorAuthenticationException("Authentication token not specified. Specify the second factor authentication token");

                var authenticated = twoFactorAuth.Authenticate(entity.username, entity.passCode);

                if (authenticated.authenticated == false)
                    throw new TwoFactorAuthenticationException("Two factor authentication failed. Input the token and try again");
            }

            AccountCreationResponseMessageViewModel module = null;
            AccountCreationRespones result = null;
            Task.Run(async () => result = await account.CreateAccount(entity)).GetAwaiter().GetResult();
            if (result.Message.ReasonPhrase == "OK")
                if (result.APIResponse.webRequestStatus == "SUCCESS")
                    module = result.APIResponse;
            if (result.APIResponse.webRequestStatus == "FAILURE")
                throw new SecureException(result.APIResponse.errorMessage);
            return module;
        }

        public PostingResult PostTransactions(List<FinanceTransactionViewModel> model)
        { 
            ResponseMessage result = null;
            
            List<TransactionPostingViewModel> transactionList = TransactionData(model);

            var curencyTypeCount = model.Select(x => x.currencyId).Distinct().Count();

            if (curencyTypeCount <= 1)
            {
                Task.Run(async () => result = await transaction.ApiTransactionPosting(transactionList)).GetAwaiter().GetResult();
            }
            else
            {
                Task.Run(async () => result = await transaction.ApiTransactionPosting(transactionList, true)).GetAwaiter().GetResult();
            }

            if (result.APIResponse != null)
            {
                if (result.APIResponse.responseCode == "0")
                {
                    AddCustomTransactions(transactionList);

                    string str = result.APIResponse.webRequestStatus;
                    str = str.Replace(":", "");
                    str = str.Replace("FAILURE", "");
                    str = str.Replace("SUCCESS+", "");

                    return new PostingResult {posted = true, responseCode = str.Trim() };
                }
                //if (result.APIResponse.webRequestStatus == "SUCCESS+      M18")
                //{
                //    AddCustomTransactions(transactionList);
                //    return true;
                //}
                else
                {
                    var message = result.responseMessage.Replace("[", "").Replace("]", "").Replace("{", "").Replace("}", "").Replace(@"""", "");

                    throw new ConditionNotMetException("Core Banking API error - Response Code:" + result.APIResponse.responseCode + ". Response Message:" + result.APIResponse.webRequestStatus); //message result.APIResponse.webRequestStatus
                }
            }
            else
            {
                var message = result.responseMessage.Replace("[", "").Replace("]", "").Replace("{","").Replace("}", "").Replace(@"""", "");
                //throw new APIErrorException("Core Banking API Error - Kindly contact the administrator. See error log below :" + "/n" + message); // .Message.ReasonPhrase);
                throw new APIErrorException("Core Banking API Error - Kindly contact the administrator.");
            }

            //return result.APIStatus;

        }

        //public bool PostCrossCurrencyTransactions(List<FinanceTransactionViewModel> model)
        //{
        //    ResponseMessage result = null;
        //    List<TransactionPostingViewModel> transactionLst = TransactionData(model);

        //    Task.Run(async () => result = await transaction.ApiPostCrossCurrencyTransactions(transactionLst)).GetAwaiter().GetResult();


        //    if (result.APIResponse.responseCode == "0")
        //    {
        //        AddCustomTransactions(transactionLst);
        //    }

        //    return result.APIStatus;
        //}

        //public bool PostCrossCurrencyTransactions(List<FinanceTransactionViewModel> model)
        //{
        //    ResponseMessage result = null;

        //    List<TransactionPostingViewModel> transactionLst = TransactionData(model);

        //    Task.Run(async () => result = await transaction.ApiPostCrossCurrencyTransactions(transactionLst)).GetAwaiter().GetResult();

        //    if (result.APIResponse != null)
        //    {
        //        if (result.APIResponse.responseCode == "0")
        //        {
        //            AddCustomTransactions(transactionLst);
        //            return true;
        //        }
        //        else
        //        {
        //            throw new ConditionNotMetException(result.APIResponse.webRequestStatus);
        //        }
        //    }
        //    else
        //    {
        //        throw new APIErrorException("Core Banking API Error - " + result.Message.ReasonPhrase);
        //    }

        //    //return result.APIStatus;

        //}

        public bool AddCustomerAccounts(int customerId,string customerCode)
        {
            //var customerId = context.TBL_CUSTOMER.Where(a => a.CUSTOMERCODE == customerCode).Select(b => b.CUSTOMERID).FirstOrDefault();
            //var customerId = this.context.TBL_CUSTOMER.FirstOrDefault(a => a.CUSTOMERCODE == customerCode).CUSTOMERID;
            bool output = false;
            var data = new List<CasaViewModel>();
            List<TBL_CASA> customerAcct = new List<TBL_CASA>();

            //Task.Run(async () => { data = await customer.GetCustomerAccountsBalanceByCustomerCode(customerCode); })
            //    .GetAwaiter().GetResult();


            Task.Run(async () => data = await customer.GetCustomerAccountsBalanceByCustomerCode(customerCode)).GetAwaiter().GetResult();

            foreach (var item in data)
            {
                var currencyId = context.TBL_CURRENCY.FirstOrDefault(x => x.CURRENCYCODE == item.currency).CURRENCYID;
                var accountStatusId = context.TBL_CASA_ACCOUNTSTATUS
                    .FirstOrDefault(x => x.ACCOUNTSTATUSNAME.ToLower() == item.accountStatusName.ToLower())
                    .ACCOUNTSTATUSID;
                TBL_CASA addCustomerAcct = new TBL_CASA();
                addCustomerAcct.CUSTOMERID = customerId;
                addCustomerAcct.AVAILABLEBALANCE = item.availableBalance;
                addCustomerAcct.LEDGERBALANCE = item.ledgerBalance;
                addCustomerAcct.PRODUCTACCOUNTNAME = item.productName;//item.productAccountName;
                addCustomerAcct.PRODUCTACCOUNTNUMBER = item.productAccountNumber;
                addCustomerAcct.PRODUCTID = (short)DefaultProductEnum.CASA; //(short)(item.productCode != "" ? 8 : 8);
                addCustomerAcct.COMPANYID = 1;
                addCustomerAcct.BRANCHID = (short)(item.branchCode != "" ? context.TBL_BRANCH.FirstOrDefault(x => x.BRANCHCODE == item.branchCode).BRANCHID : 94);
                addCustomerAcct.CURRENCYID = currencyId;//(short)(item.currency == "NGN" ? 1 : 0);
                addCustomerAcct.ISCURRENTACCOUNT = true;
                addCustomerAcct.ACCOUNTSTATUSID = (short)accountStatusId;//(short)(item.accountStatusName == "Active" ? 1 : 3);
                addCustomerAcct.LIENAMOUNT = 0;
                addCustomerAcct.HASLIEN = false;
                addCustomerAcct.POSTNOSTATUSID = 1;
                addCustomerAcct.DELETED = false;

                customerAcct.Add(addCustomerAcct);
            }
            var customerExist = this.context.TBL_CASA.FirstOrDefault(a => a.CUSTOMERID == customerId);
            if (customerExist == null)
            {
                this.context.TBL_CASA.AddRange(customerAcct);
                context.SaveChanges();
            }
            else
            {
                foreach (var a in customerAcct)
                {

                    TBL_CASA result = (from p in context.TBL_CASA
                                       where p.CUSTOMERID == a.CUSTOMERID && p.PRODUCTACCOUNTNUMBER == a.PRODUCTACCOUNTNUMBER
                                       select p).SingleOrDefault();

                    if (result == null)
                    {
                        this.context.TBL_CASA.Add(a);
                        context.SaveChanges();
                    }
                    else
                    {
                        result.AVAILABLEBALANCE = a.AVAILABLEBALANCE;
                        result.ACCOUNTSTATUSID = a.ACCOUNTSTATUSID;
                        result.LEDGERBALANCE = a.LEDGERBALANCE;
                        context.SaveChanges();
                    }
                }

            }

            //context.SaveChanges();
            //context.SaveChangesAsync();

            output = true;

            return output;
        }
        public bool AddCustomerAccounts(string customerCode)
        {
            if (customerCode == null)
                return false;

            var customerId = context.TBL_CUSTOMER.Where(a => a.CUSTOMERCODE == customerCode).Select(b => b.CUSTOMERID).FirstOrDefault();
            //var customerId = this.context.TBL_CUSTOMER.FirstOrDefault(a => a.CUSTOMERCODE == customerCode).CUSTOMERID;
            bool output = false;
            var data = new List<CasaViewModel>();
            List<TBL_CASA> customerAcct = new List<TBL_CASA>();

            //Task.Run(async () => { data = await customer.GetCustomerAccountsBalanceByCustomerCode(customerCode); })
            //    .GetAwaiter().GetResult();


            Task.Run(async () => data = await customer.GetCustomerAccountsBalanceByCustomerCode(customerCode)).GetAwaiter().GetResult();

            foreach (var item in data)
            {
                var currencyId = context.TBL_CURRENCY.FirstOrDefault(x => x.CURRENCYCODE == item.currency).CURRENCYID;
                var accountStatusId = context.TBL_CASA_ACCOUNTSTATUS
                    .FirstOrDefault(x => x.ACCOUNTSTATUSNAME.ToLower() == item.accountStatusName.ToLower())
                    .ACCOUNTSTATUSID;
                TBL_CASA addCustomerAcct = new TBL_CASA();
                addCustomerAcct.CUSTOMERID = customerId;
                addCustomerAcct.AVAILABLEBALANCE = item.availableBalance;
                addCustomerAcct.LEDGERBALANCE = item.ledgerBalance;
                addCustomerAcct.PRODUCTACCOUNTNAME = item.productName;//item.productAccountName;
                addCustomerAcct.PRODUCTACCOUNTNUMBER = item.productAccountNumber;
                addCustomerAcct.PRODUCTID = (short)DefaultProductEnum.CASA; //(short)(item.productCode != "" ? 8 : 8);
                addCustomerAcct.COMPANYID = 1;
                addCustomerAcct.BRANCHID = (short)(item.branchCode != "" ? context.TBL_BRANCH.FirstOrDefault(x => x.BRANCHCODE == item.branchCode).BRANCHID : 94);
                addCustomerAcct.CURRENCYID = currencyId;//(short)(item.currency == "NGN" ? 1 : 0);
                addCustomerAcct.ISCURRENTACCOUNT = true;
                addCustomerAcct.ACCOUNTSTATUSID = (short)accountStatusId;//(short)(item.accountStatusName == "Active" ? 1 : 3);
                addCustomerAcct.LIENAMOUNT = 0;
                addCustomerAcct.HASLIEN = false;
                addCustomerAcct.POSTNOSTATUSID = 1;
                addCustomerAcct.DELETED = false;

                customerAcct.Add(addCustomerAcct);
            }
            var customerExist = this.context.TBL_CASA.FirstOrDefault(a => a.CUSTOMERID == customerId);
            if (customerExist == null)
            {
                this.context.TBL_CASA.AddRange(customerAcct);
                context.SaveChanges();
            }
            else
            {
                foreach (var a in customerAcct)
                {

                    TBL_CASA result = (from p in context.TBL_CASA
                                       where p.CUSTOMERID == a.CUSTOMERID && p.PRODUCTACCOUNTNUMBER == a.PRODUCTACCOUNTNUMBER
                                       select p).SingleOrDefault();

                    if (result == null)
                    {
                        this.context.TBL_CASA.Add(a);
                        context.SaveChanges();
                    }
                    else
                    {
                        result.AVAILABLEBALANCE = a.AVAILABLEBALANCE;
                        result.ACCOUNTSTATUSID = a.ACCOUNTSTATUSID;
                        result.LEDGERBALANCE = a.LEDGERBALANCE;
                        context.SaveChanges();
                    }
                }

            }

            //context.SaveChanges();
            //context.SaveChangesAsync();

            output = true;

            return output;
        }

        public List<CustomerViewModels> GetCustomerByAccountsNumber(string customerAccount)
        {
            List<CustomerViewModels> cust = new List<CustomerViewModels>();

            Task.Run(async () => cust = await customer.GetCustomerByAccountsNumber(customerAccount)).GetAwaiter()
                .GetResult();
            return cust;
        }

        public CasaBalanceViewModel GetCustomerAccountBalance(string customerAccount)
        {
            CasaBalanceViewModel accountOutput = null;

            Task.Run(async () => accountOutput = await customer.GetCustomerAccountBalance(customerAccount)).GetAwaiter()
                .GetResult();

            return accountOutput;
        }

        public List<CasaViewModel> GetCustomerAccountsBalanceByCustomerCode(string customerCode)
        {
            List<CasaViewModel> casa = new List<CasaViewModel>();
            Task.Run(async () => casa = await customer.GetCustomerAccountsBalanceByCustomerCode(customerCode))
                .GetAwaiter().GetResult();
            return casa;
        }

        public List<CustomerTurnoverViewModel> GetCustomerAccountTurnover(string customerCode, int durationInMonths)
        {
            List<CustomerTurnoverViewModel> accounts = new List<CustomerTurnoverViewModel>();

            Task.Run(async () => accounts = await customer.GetCustomerTransactions(customerCode, durationInMonths)).GetAwaiter()
                .GetResult();

            return accounts;
        }

        public List<CustomerTurnoverViewModel> GetCustomerAccountInterestTransactions(string customerCode, int durationInMonths)
        {
            List<CustomerTurnoverViewModel> accounts = new List<CustomerTurnoverViewModel>();

            Task.Run(async () => accounts = await customer.GetCustomerInterestTransactions(customerCode, durationInMonths)).GetAwaiter()
                .GetResult();

            return accounts;
        }

        public string GetGlAccountCode(int glAccountId, int currencyId, int branchId)
        {
            var account = context.TBL_CHART_OF_ACCOUNT.FirstOrDefault(x => x.GLACCOUNTID == glAccountId);

            var currency = context.TBL_CURRENCY.FirstOrDefault(x => x.CURRENCYID == currencyId);

            var accountInfo = account.ACCOUNTCODE + " - " + account.ACCOUNTNAME + " for currency " + currency.CURRENCYCODE;

            var nonBranchSpecificAccount = (from gl in context.TBL_CUSTOM_CHART_OF_ACCOUNT
                               join gla in context.TBL_CHART_OF_ACCOUNT on gl.PLACEHOLDERID equals gla.ACCOUNTCODE                               
                               where gla.GLACCOUNTID == glAccountId
                               select new { gl.ACCOUNTID, gl.ISBRANCHSPECIFIC }).FirstOrDefault();

            if (nonBranchSpecificAccount == null)
                throw new ConditionNotMetException("There is no custom GL setup for GL " + accountInfo +  ". Check the custom GL setup");

            if (nonBranchSpecificAccount.ISBRANCHSPECIFIC == false)
            {
                return nonBranchSpecificAccount.ACCOUNTID;
            }
            else
            {
                var branchCode = (from br in context.TBL_BRANCH where br.BRANCHID == branchId select br.BRANCHCODE).FirstOrDefault();

                var accountCode = (from gl in context.TBL_CUSTOM_CHART_OF_ACCOUNT
                                   join gla in context.TBL_CHART_OF_ACCOUNT on gl.PLACEHOLDERID equals gla.ACCOUNTCODE
                                   join cur in context.TBL_CURRENCY on gl.CURRENCYCODE equals cur.CURRENCYCODE
                                   where cur.CURRENCYID == currencyId && gla.GLACCOUNTID == glAccountId
                                   select gl.ACCOUNTID).FirstOrDefault();

                if (accountCode == null)
                    throw new ConditionNotMetException("There is no custom GL setup for GL " + accountInfo + ". Check the custom GL setup");

                var glAccountCode = branchCode + accountCode; //"100" + accountCode;

                return glAccountCode;
            }
        }

        public bool ChangeOverDraftInterestRate(InterestRateInquiryViewModel model, string accountType,TwoFactorAutheticationViewModel twoFADetails = null)
        {
            if (USE_TWO_FACTOR_AUTHENTICATION)
            {
                if (twoFADetails == null)
                    throw new TwoFactorAuthenticationException("Authentication token not specified. Specify the second factor authentication token");

                if (twoFADetails.skipAuthentication == false)
                {
                    var authenticated = twoFactorAuth.Authenticate(twoFADetails.username, twoFADetails.passcode);

                    if (authenticated.authenticated == false)
                        throw new TwoFactorAuthenticationException("Two factor authentication failed. Input the token and try again");
                }
            }

            model.APIUrl = @"api/InterestRateInquiry/PostInterestRate";
            ResponseMessage result = null;
            Task.Run(async () => result = await overDraft.APIOverDraftInterestRate(model, accountType)).GetAwaiter().GetResult();

            if (result.Message.IsSuccessStatusCode)
            {
                if (result.APIResponse.webRequestStatus.Replace(":", "") == "FAILURE")
                {
                    //throw new SecureException(result.APIResponse.message);
                    var message = result.responseMessage.Replace("[", "").Replace("]", "").Replace("{", "").Replace("}", "").Replace(@"""", "");
                    throw new SecureException("Core Banking API Error - " + message);
                }
                else
                {
                    //LogTemporaryOverDraft(model);
                    result.APIResponse.responseStatus = true;
                    return true; // result.APIResponse;
                }
            }
            else
            {

                //throw new APIErrorException("Core Banking API Error - " + result.Message.StatusCode + "" + result.Message.ReasonPhrase);
                var message = result.responseMessage.Replace("[", "").Replace("]", "").Replace("{", "").Replace("}", "").Replace(@"""", "");
                throw new APIErrorException("Core Banking API Error - " + result.Message.StatusCode + "" + message);
            }
        }


        #region  private
        private bool LogOverDraftExtend(OverDraftExtendViewModel model)
        {
            bool result = false;
            var modify = context.TBL_CUSTOM_OVERDRAFTEXTEND.Find(model.overdraftExtendId);
            if (modify != null)
            {
                modify.CONSUMED = true;
                modify.DATETIMECONSUMED = DateTime.Now;
                result = context.SaveChanges() > 0;
            }
            else
            {
                var data = new TBL_CUSTOM_OVERDRAFTEXTEND
                {
                    ACCOUNTNUMBER = model.accountNumber,
                    APIURL = @"api/OverDraft/Extend",
                    DATETIMECREATED = DateTime.Now,
                    EXPIRYDATE = model.expiryDate,
                    SANCTIONLIMIT = model.sanctionLimit,
                    SANCTIONREFERENCENUMBER = model.sanctionReferenceNumber
                };
                context.TBL_CUSTOM_OVERDRAFTEXTEND.Add(data);
                result = context.SaveChanges() > 0;
                model.overdraftExtendId = data.OVERDRAFTEXTENDID;

            }
            return result;
        }

        private bool LogOverDraftNormal(OverDraftNormalViewModel model)
        {
            bool result = false;
            var modify = context.TBL_CUSTOM_OVERDRAFTNORMAL.Find(model.overdraftNormalId);
            if (modify != null)
            {
                modify.CONSUMED = true;
                modify.DATETIMECONSUMED = DateTime.Now;
                result = context.SaveChanges() > 0;
            }
            else
            {
                //var data = new TBL_CUSTOM_OVERDRAFTNORMAL
                //{
                //    ACCOUNTNUMBER = model.accountNumber,
                //    APIURL = @"api/OverDraft/Normal",
                //    DATETIMECREATED = DateTime.Now,
                //    EXPIRYDATE = model.expiryDate,
                //    SANCTIONLIMIT = model.sanctionLimit,
                //    SANCTIONREFERENCENUMBER = model.sanctionReferenceNumber,
                //    APPLICATIONDATE = model.applicationDate,
                //    DOCUMENTDATE = model.documentDate,
                //    REVIEWEDDATE = model.reviewedDate,
                //    SANCTIONAUTHORIZER = model.sanctionAuthorizer,
                //    SANCTIONDATE = model.sanctionDate,
                //    SANCTIONLEVEL = model.sanctionLevel,

                //};
                //context.TBL_CUSTOM_OVERDRAFTNORMAL.Add(data);
                //result = context.SaveChanges() > 0;
                // model.overdraftNormalId = data.OVERDRAFTNORMALID;
            }
            return result;
        }

        private bool LogOverDraftTopUpAndRenew(OverDraftTopUpAndRenewViewModel model)
        {
            bool result = false;
            var modify = context.TBL_CUSTOM_OVERDRAFTEXTEND.Find(model.overdraftExtendId);
            if (modify != null)
            {
                modify.CONSUMED = true;
                modify.DATETIMECONSUMED = DateTime.Now;
                result = context.SaveChanges() > 0;
            }
            else
            {
                var data = new TBL_CUSTOM_OVERDRAFTEXTEND
                {

                    ACCOUNTNUMBER = model.accountNumber,
                    APIURL = model.apiUrl,
                    DATETIMECREATED = model.createdDate,
                    EXPIRYDATE = model.expiryDate,
                    SANCTIONLIMIT = model.sanctionLimit,
                    SANCTIONREFERENCENUMBER = model.sanctionReferenceNumber,
                    CONSUMED = true,
                    DATETIMECONSUMED = DateTime.Now,

                };
                context.TBL_CUSTOM_OVERDRAFTEXTEND.Add(data);
                result = context.SaveChanges() > 0;
                model.overdraftExtendId = data.OVERDRAFTEXTENDID;
            }
            return result;

        }

        private bool LogTemporaryOverDraft(TemporaryOverDraftViewModel model)
        {
            bool result = false;
            var modify = context.TBL_CUSTOM_TEMPORARYOVERDRAFT.Find(model.TemporaryOverDraftId);
            if (modify != null)
            {
                modify.CONSUMED = true;
                modify.DATETIMECONSUMED = DateTime.Now;
                result = context.SaveChanges() > 0;
            }
            else
            {
                var data = new TBL_CUSTOM_TEMPORARYOVERDRAFT
                {
                    APIURL = model.APIUrl,
                    DATETIMECREATED = DateTime.Now,
                    TEMPORARYOVERDRAFTAMOUNT = model.TemporaryOverDraftAmount,
                    TEMPORARYOVERDRAFTDATE = model.TemporaryOverDraftDate,
                    TEMPORARYOVERDRAFTFLAG = model.TemporaryOverDraftFlag,
                    TEMPORARYOVERDRAFTNARATION = model.TemporaryOverDraftNaration
                };
                context.TBL_CUSTOM_TEMPORARYOVERDRAFT.Add(data);
                result = context.SaveChanges() > 0;
                model.TemporaryOverDraftId = data.TEMPORARYOVERDRAFTID;
            }
            return result;
        }


        private List<TransactionPostingViewModel> TransactionData(List<FinanceTransactionViewModel> model )
        {
            List<TransactionPostingViewModel> transactionLst = new List<TransactionPostingViewModel>();
            foreach (var item in model)
            {

                //    var account ;

                var transPosting = new TransactionPostingViewModel();
                //accounts = item.casaAccountId != null ? context.TBL_CASA.FirstOrDefault(x => x.CASAACCOUNTID == item.casaAccountId).PRODUCTACCOUNTNUMBER : context.TBL_CHART_OF_ACCOUNT.FirstOrDefault(x => x.GLACCOUNTID == item.glAccountId).ACCOUNTCODE,
                transPosting.currencyType = context.TBL_CURRENCY
                   .FirstOrDefault(x => x.CURRENCYID == item.currencyId)
                   ?.CURRENCYCODE;

                //transPosting.accounts = item.casaAccountId != null
                //? context.TBL_CASA.FirstOrDefault(x => x.CASAACCOUNTID == item.casaAccountId)?
                //.PRODUCTACCOUNTNUMBER : GetGlAccountCode(item.glAccountId, item.currencyId, item.sourceBranchId);

                if (item.useDirectAccount == false)
                {
                    if (item.casaAccountId != null)
                        transPosting.accounts = context.TBL_CASA.FirstOrDefault(x => x.CASAACCOUNTID == item.casaAccountId)?.PRODUCTACCOUNTNUMBER.Trim();

                    else
                        transPosting.accounts = GetGlAccountCode(item.glAccountId, item.currencyId, item.sourceBranchId).Trim();
                }
                else
                {
                    transPosting.accounts = item.accountNumber.Trim();
                }

                transPosting.amounts = item.creditAmount > 0
                    ? "C" + String.Format("{0:0.00}", item.creditAmount)
                    : "D" + String.Format("{0:0.00}", item.debitAmount);
                //amounts = item.sourceReferenceNumber,
                transPosting.narration = item.description;
                if(item.batchCode == null)
                {
                    transPosting.referenceNumber =  "1222333444";  // to be change  transPosting.referenceNumber = item.sourceReferenceNumber                    
                }
                else
                {
                    transPosting.referenceNumber = item.batchCode;  
                }

                transPosting.sourceReferenceNumber = item.sourceReferenceNumber.Trim();
                transPosting.valueDate = item.valueDate.ToString("dd-MMM-yyyy", null);
                transPosting.operationId = item.operationId; // != null ? context.TBL_CASA.FirstOrDefault(x => x.CASAACCOUNTID == item.operationId).PRODUCTACCOUNTNUMBER : context.TBL_CHART_OF_ACCOUNT.FirstOrDefault(x => x.GLACCOUNTID == item.glAccountId).ACCOUNTCODE,

                //cross currency attributes
                transPosting.rateCode = item.rateCode;
                transPosting.rateUnit = item.rateUnit;
                transPosting.currencycrosscode = item.currencyCrossCode;
                //-------------------

                transactionLst.Add(transPosting);
            }

            return transactionLst;
        }

        private bool AddCustomTransactions(List<TransactionPostingViewModel> entity)
        {
            List<TBL_CUSTOM_FIANCE_TRANSACTION> lstData = new List<TBL_CUSTOM_FIANCE_TRANSACTION>();
            bool output = false;
            foreach (var item in entity)
            {
                var data = new TBL_CUSTOM_FIANCE_TRANSACTION();
                {
                    data.ACCOUNTID = item.accounts;
                    data.AMOUNT = item.amounts;
                    data.BATCHCODE = item.referenceNumber;
                    data.CONSUMED = false;
                    data.CURRENCYCODE = item.currencyType;
                    data.DATETIMECONSUMED = null;
                    data.DATETIMECREATED = DateTime.Now;
                    data.NARRATION = item.narration;
                    data.OPERATIONID = item.operationId;
                }
                lstData.Add(data);
            };
            context.TBL_CUSTOM_FIANCE_TRANSACTION.AddRange(lstData);
            context.SaveChanges();
            output = true;
            return output;

        }

        public InterestRateInquiryViewModel GetInterestRateInquiry(string accountNumber, string accountType)
        {
            InterestRateInquiryViewModel accountOutput = null;

            Task.Run(async () => accountOutput = await customer.GetInterestRateInquiry(accountNumber, accountType)).GetAwaiter()
                .GetResult();

            return accountOutput;
        }

        public Users GetUserRoleFinacle(string staffCode)
        {

            Users module = null;
            Task.Run(async () => module = await staff.GetStaffRoleByStaffCode(staffCode.ToUpper())).GetAwaiter()
               .GetResult();

            return module;
        }



        //private bool LogTemporaryOverDraft(InterestRateInquiryViewModel model)
        //{
        //    bool result = false;
        //    {
        //        var data = new TBL_CUSTOM_TEMPORARYOVERDRAFT
        //        {
        //            APIURL = model.APIUrl,
        //            DATETIMECREATED = DateTime.Now,
        //            TEMPORARYOVERDRAFTAMOUNT = model.interestRateAmount,
        //            TEMPORARYOVERDRAFTDATE = model.TemporaryOverDraftDate,
        //            TEMPORARYOVERDRAFTFLAG = model.TemporaryOverDraftFlag,
        //            TEMPORARYOVERDRAFTNARATION = model.TemporaryOverDraftNaration
        //        };
        //        context.TBL_CUSTOM_TEMPORARYOVERDRAFT.Add(data);
        //        result = context.SaveChanges() > 0;
        //    }
        //    return result;
        //}
        #endregion

    }
}