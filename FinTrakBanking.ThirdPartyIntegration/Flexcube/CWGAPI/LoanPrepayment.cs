using FintrakBanking.Common;
using FintrakBanking.Common.CustomException;
using FintrakBanking.Common.Enum;
using FintrakBanking.Entities.Models;
using FintrakBanking.Entities.StagingModels;
using FintrakBanking.Interfaces.Credit;
using FintrakBanking.Interfaces.Setups.General;
using FintrakBanking.ViewModels.Finance;
using FintrakBanking.ViewModels.ThridPartyIntegration;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using System.Web.Script.Serialization;

namespace ThirdPartyIntegration
{
    public class LoanPrepayment 
    {
        private FinTrakBankingContext context;
        private FinTrakBankingStagingContext staging;
        string API_KEY, API_URL = string.Empty;
        private IEnumerable<TBL_API_URL> APIUrlConfig;
        private IGeneralSetupRepository genSetup;
        
        public LoanPrepayment(FinTrakBankingContext _context, FinTrakBankingStagingContext _staging, IGeneralSetupRepository _genSetup)
        {
            this.context = _context;
            this.staging = _staging;
            var configdata = context.TBL_SETUP_COMPANY.FirstOrDefault();
            APIUrlConfig = context.TBL_API_URL;
            API_KEY = "FTK05202023"; //configdata.APIKEY;
            API_URL = configdata.APIURL;
            this.genSetup = _genSetup;

        }
        
        private void getAPIURLSettings(string typeName = null)
        {
            var apiConfig = APIUrlConfig.Where(x => x.TYPENAME.ToLower() == typeName.ToLower()).FirstOrDefault();
            if (apiConfig != null)
            {
                API_URL = apiConfig.URL.Trim();
                API_KEY = apiConfig.APIKEY;
            }
            if (apiConfig == null)
            {
                apiConfig = APIUrlConfig.Where(x => x.TYPENAME.ToUpper() == "DEFAULT").FirstOrDefault();
                API_URL = apiConfig.URL.Trim();
                API_KEY = apiConfig.APIKEY;
            }
        }

        public async Task<MainResponseLoanPrepaymentViewModel> GetTodayRepaymentLoans(LoanPrepaymentViewModel model)
        {
            HttpClientHandler handler = new HttpClientHandler();
            HttpClient httpClientInstance;

            HttpClient client = new HttpClient(handler);
            var inputJson = new JavaScriptSerializer().Serialize(model);
            DateTime requestDatetime = new DateTime(), responseDateTime = new DateTime();
            HttpResponseMessage response = null;

            MainResponseLoanPrepaymentViewModel responseApi = new MainResponseLoanPrepaymentViewModel();
            ResponseMessage responseMsg = null;
            string responseJson = "";

            getAPIURLSettings("LoanPrepayment");
            string apiUrl = "GetTodayRepaymentLoans";

            try
            {
                var token = new AuthenticationHeaderValue("Authorization", API_KEY);
                var dta = context.TBL_SETUP_GLOBAL.ToList();
                handler.UseDefaultCredentials = true;
                httpClientInstance = new HttpClient();
                httpClientInstance.DefaultRequestHeaders.ConnectionClose = false;
                client.Timeout = TimeSpan.FromSeconds(180);
                client.DefaultRequestHeaders.Authorization = token;
                client.BaseAddress = new Uri(API_URL);
                client.DefaultRequestHeaders.Accept.Clear();
                client.DefaultRequestHeaders.Accept.Add(
                new MediaTypeWithQualityHeaderValue("application/json"));

                ServicePointManager.ServerCertificateValidationCallback += (sender, cert, chain, sslPolicyErrors) => true;
                requestDatetime = DateTime.Now;
                response = client.PostAsync(apiUrl, new StringContent(
                                                new JavaScriptSerializer().Serialize(model), Encoding.UTF8, "application/json")).Result;
                responseDateTime = DateTime.Now;

                if (response.IsSuccessStatusCode)
                {
                    responseApi = await response.Content.ReadAsAsync<MainResponseLoanPrepaymentViewModel>();

                    var res = new ResponseMessageViewModel
                    {
                        responseCode = responseApi.response_code,
                        responseStatus = responseApi.response_message == "Successful" ? true : false,
                    };

                    responseMsg = new ResponseMessage
                    {
                        APIResponse = res,
                        APIStatus = response.IsSuccessStatusCode,
                        Message = response
                    };
                }
                else
                {
                    responseMsg = new ResponseMessage
                    {
                        APIResponse = null,
                        APIStatus = response.IsSuccessStatusCode,
                        Message = response
                    };
                }

                responseJson = await response.Content.ReadAsStringAsync();
                responseMsg.responseMessage = responseJson;
                return responseApi;
            }
            catch (Exception ex)
            {
                var innerExceptionMessage = "";
                if (ex.InnerException != null)
                    innerExceptionMessage = ex.InnerException.Message;

                throw new APIErrorException($"Core Banking API Error - {ex.Message} - inner exception - {innerExceptionMessage}");
            }

            finally
            {
                handler.Dispose();
                client.Dispose();

                var logs = new TBL_CUSTOM_API_LOGS
                {
                    APIURL = API_URL + apiUrl,
                    LOGTYPEID = 5,
                    REFERENCENUMBER = model.user_ref_no,
                    REQUESTDATETIME = requestDatetime,
                    REQUESTMESSAGE = inputJson,
                    RESPONSEDATETIME = responseDateTime,
                    RESPONSEMESSAGE = responseJson,
                };

                FinTrakBankingContext logContext = new FinTrakBankingContext();
                logContext.TBL_CUSTOM_API_LOGS.Add(logs);
                logContext.SaveChanges();
            }

        }

        public async Task<MainResponseLoanPrepaymentViewModel> GetTodayLoanRepaymentByRefNo(LoanPrepaymentViewModel model)
        {
            HttpClientHandler handler = new HttpClientHandler();
            HttpClient httpClientInstance;

            HttpClient client = new HttpClient(handler);
            var inputJson = new JavaScriptSerializer().Serialize(model);
            DateTime requestDatetime = new DateTime(), responseDateTime = new DateTime();
            HttpResponseMessage response = null;

            MainResponseLoanPrepaymentViewModel responseApi = new MainResponseLoanPrepaymentViewModel();
            ResponseMessage responseMsg = null;
            string responseJson = "";

            getAPIURLSettings("LoanPrepayment");
            string apiUrl = "GetTodayLoanRepaymentByRefNo";

            try
            {
                var token = new AuthenticationHeaderValue("Authorization", API_KEY);
                var dta = context.TBL_SETUP_GLOBAL.ToList();
                handler.UseDefaultCredentials = true;
                httpClientInstance = new HttpClient();
                httpClientInstance.DefaultRequestHeaders.ConnectionClose = false;
                client.Timeout = TimeSpan.FromSeconds(180);
                client.DefaultRequestHeaders.Authorization = token;
                client.BaseAddress = new Uri(API_URL);
                client.DefaultRequestHeaders.Accept.Clear();
                client.DefaultRequestHeaders.Accept.Add(
                new MediaTypeWithQualityHeaderValue("application/json"));

                ServicePointManager.ServerCertificateValidationCallback += (sender, cert, chain, sslPolicyErrors) => true;
                requestDatetime = DateTime.Now;
                response = client.PostAsync(apiUrl, new StringContent(
                                                new JavaScriptSerializer().Serialize(model), Encoding.UTF8, "application/json")).Result;
                responseDateTime = DateTime.Now;

                if (response.IsSuccessStatusCode)
                {
                    responseApi = await response.Content.ReadAsAsync<MainResponseLoanPrepaymentViewModel>();

                    var res = new ResponseMessageViewModel
                    {
                        responseCode = responseApi.response_code,
                        responseStatus = responseApi.response_message == "Successful" ? true : false,
                    };

                    responseMsg = new ResponseMessage
                    {
                        APIResponse = res,
                        APIStatus = response.IsSuccessStatusCode,
                        Message = response
                    };
                }
                else
                {
                    responseMsg = new ResponseMessage
                    {
                        APIResponse = null,
                        APIStatus = response.IsSuccessStatusCode,
                        Message = response
                    };
                }

                responseJson = await response.Content.ReadAsStringAsync();
                responseMsg.responseMessage = responseJson;
                return responseApi;
            }
            catch (Exception ex)
            {
                var innerExceptionMessage = "";
                if (ex.InnerException != null)
                    innerExceptionMessage = ex.InnerException.Message;

                throw new APIErrorException($"Core Banking API Error - {ex.Message} - inner exception - {innerExceptionMessage}");
            }

            finally
            {
                handler.Dispose();
                client.Dispose();

                var logs = new TBL_CUSTOM_API_LOGS
                {
                    APIURL = API_URL + apiUrl,
                    LOGTYPEID = 5,
                    REFERENCENUMBER = model.user_ref_no,
                    REQUESTDATETIME = requestDatetime,
                    REQUESTMESSAGE = inputJson,
                    RESPONSEDATETIME = responseDateTime,
                    RESPONSEMESSAGE = responseJson,
                };

                FinTrakBankingContext logContext = new FinTrakBankingContext();
                logContext.TBL_CUSTOM_API_LOGS.Add(logs);
                logContext.SaveChanges();
            }

        }

        public async Task<MainResponseLoanPrepaymentViewModel> GetTodayLoanSumRepaymentByRefNo(LoanPrepaymentViewModel model)
        {
            HttpClientHandler handler = new HttpClientHandler();
            HttpClient httpClientInstance;

            HttpClient client = new HttpClient(handler);
            var inputJson = new JavaScriptSerializer().Serialize(model);
            DateTime requestDatetime = new DateTime(), responseDateTime = new DateTime();
            HttpResponseMessage response = null;

            MainResponseLoanPrepaymentViewModel responseApi = new MainResponseLoanPrepaymentViewModel();
            ResponseMessage responseMsg = null;
            string responseJson = "";

            getAPIURLSettings("LoanPrepayment");
            string apiUrl = "GetTodayLoanSumRepaymentByRefNo";

            try
            {
                var token = new AuthenticationHeaderValue("Authorization", API_KEY);
                var dta = context.TBL_SETUP_GLOBAL.ToList();
                handler.UseDefaultCredentials = true;
                httpClientInstance = new HttpClient();
                httpClientInstance.DefaultRequestHeaders.ConnectionClose = false;
                client.Timeout = TimeSpan.FromSeconds(180);
                client.DefaultRequestHeaders.Authorization = token;
                client.BaseAddress = new Uri(API_URL);
                client.DefaultRequestHeaders.Accept.Clear();
                client.DefaultRequestHeaders.Accept.Add(
                new MediaTypeWithQualityHeaderValue("application/json"));

                ServicePointManager.ServerCertificateValidationCallback += (sender, cert, chain, sslPolicyErrors) => true;
                requestDatetime = DateTime.Now;
                response = client.PostAsync(apiUrl, new StringContent(
                                                new JavaScriptSerializer().Serialize(model), Encoding.UTF8, "application/json")).Result;
                responseDateTime = DateTime.Now;

                if (response.IsSuccessStatusCode)
                {
                    responseApi = await response.Content.ReadAsAsync<MainResponseLoanPrepaymentViewModel>();

                    var res = new ResponseMessageViewModel
                    {
                        responseCode = responseApi.response_code,
                        responseStatus = responseApi.response_message == "Successful" ? true : false,
                    };

                    responseMsg = new ResponseMessage
                    {
                        APIResponse = res,
                        APIStatus = response.IsSuccessStatusCode,
                        Message = response
                    };
                }
                else
                {
                    responseMsg = new ResponseMessage
                    {
                        APIResponse = null,
                        APIStatus = response.IsSuccessStatusCode,
                        Message = response
                    };
                }

                responseJson = await response.Content.ReadAsStringAsync();
                responseMsg.responseMessage = responseJson;
                return responseApi;
            }
            catch (Exception ex)
            {
                var innerExceptionMessage = "";
                if (ex.InnerException != null)
                    innerExceptionMessage = ex.InnerException.Message;

                throw new APIErrorException($"Core Banking API Error - {ex.Message} - inner exception - {innerExceptionMessage}");
            }

            finally
            {
                handler.Dispose();
                client.Dispose();

                var logs = new TBL_CUSTOM_API_LOGS
                {
                    APIURL = API_URL + apiUrl,
                    LOGTYPEID = 5,
                    REFERENCENUMBER = model.user_ref_no,
                    REQUESTDATETIME = requestDatetime,
                    REQUESTMESSAGE = inputJson,
                    RESPONSEDATETIME = responseDateTime,
                    RESPONSEMESSAGE = responseJson,
                };

                FinTrakBankingContext logContext = new FinTrakBankingContext();
                logContext.TBL_CUSTOM_API_LOGS.Add(logs);
                logContext.SaveChanges();
            }

        }

        public async Task<ResponseLoanPrepaymentViewModel> GetOverdraftRepayment(LoanPrepaymentViewModel model)
        {
            HttpClientHandler handler = new HttpClientHandler();
            HttpClient httpClientInstance;

            HttpClient client = new HttpClient(handler);
            var inputJson = new JavaScriptSerializer().Serialize(model);
            DateTime requestDatetime = new DateTime(), responseDateTime = new DateTime();
            HttpResponseMessage response = null;

            ResponseLoanPrepaymentViewModel responseApi = new ResponseLoanPrepaymentViewModel();
            ResponseMessage responseMsg = null;
            string responseJson = "";

            getAPIURLSettings("LoanPrepayment");
            string apiUrl = "GetOverdraftRepayment";

            try
            {
                var token = new AuthenticationHeaderValue("Authorization", API_KEY);
                var dta = context.TBL_SETUP_GLOBAL.ToList();
                handler.UseDefaultCredentials = true;
                httpClientInstance = new HttpClient();
                httpClientInstance.DefaultRequestHeaders.ConnectionClose = false;
                client.Timeout = TimeSpan.FromSeconds(180);
                client.DefaultRequestHeaders.Authorization = token;
                client.BaseAddress = new Uri(API_URL);
                client.DefaultRequestHeaders.Accept.Clear();
                client.DefaultRequestHeaders.Accept.Add(
                new MediaTypeWithQualityHeaderValue("application/json"));

                ServicePointManager.ServerCertificateValidationCallback += (sender, cert, chain, sslPolicyErrors) => true;
                requestDatetime = DateTime.Now;
                response = client.PostAsync(apiUrl, new StringContent(
                                                new JavaScriptSerializer().Serialize(model), Encoding.UTF8, "application/json")).Result;
                responseDateTime = DateTime.Now;

                if (response.IsSuccessStatusCode)
                {
                    responseApi = await response.Content.ReadAsAsync<ResponseLoanPrepaymentViewModel>();

                    var res = new ResponseMessageViewModel
                    {
                        responseCode = responseApi.response_code,
                        responseStatus = responseApi.response_message == "Successful" ? true : false,
                    };

                    responseMsg = new ResponseMessage
                    {
                        APIResponse = res,
                        APIStatus = response.IsSuccessStatusCode,
                        Message = response
                    };
                }
                else
                {
                    responseMsg = new ResponseMessage
                    {
                        APIResponse = null,
                        APIStatus = response.IsSuccessStatusCode,
                        Message = response
                    };
                }

                responseJson = await response.Content.ReadAsStringAsync();
                responseMsg.responseMessage = responseJson;
                return responseApi;
            }
            catch (Exception ex)
            {
                var innerExceptionMessage = "";
                if (ex.InnerException != null)
                    innerExceptionMessage = ex.InnerException.Message;

                throw new APIErrorException($"Core Banking API Error - {ex.Message} - inner exception - {innerExceptionMessage}");
            }

            finally
            {
                handler.Dispose();
                client.Dispose();

                var logs = new TBL_CUSTOM_API_LOGS
                {
                    APIURL = API_URL + apiUrl,
                    LOGTYPEID = 5,
                    REFERENCENUMBER = model.user_ref_no,
                    REQUESTDATETIME = requestDatetime,
                    REQUESTMESSAGE = inputJson,
                    RESPONSEDATETIME = responseDateTime,
                    RESPONSEMESSAGE = responseJson,
                };

                FinTrakBankingContext logContext = new FinTrakBankingContext();
                logContext.TBL_CUSTOM_API_LOGS.Add(logs);
                logContext.SaveChanges();
            }

        }


        public bool GetLoanRepaymentToStaging()
        {
            try
            {
                MainResponseLoanPrepaymentViewModel response = new MainResponseLoanPrepaymentViewModel();
                LoanPrepaymentViewModel model = new LoanPrepaymentViewModel();
                model.auth_key = API_KEY;
                model.channel_code = "FINTRAK";
                model.review_date = DateTime.Now.Date.ToString("dd-MMM-yyyy"); 

                Task.Run(async () => response = await GetTodayRepaymentLoans(model)).GetAwaiter().GetResult();
                if (response.response_code == "00")
                {
                    //var existingRecords = staging.STG_CONTRACT_DAILY_REPAY.Where(x => x.AMOUNTPAID > 0 && DbFunctions.TruncateTime(x.PAYMENTDATE) == DbFunctions.TruncateTime(DateTime.Now)).Select(x => x.CONTRACTREFERENCENUMBER).ToList();
                    //var repaymentDataReceived = response.getrepaymentdetailsresp.Where(x=> !existingRecords.Contains(x.account_number)).ToList();
                    var repaymentDataReceived = response.getrepaymentdetailsresp.ToList();

                    var stagingdata = new List<STG_CONTRACT_DAILY_REPAY>();
                    foreach (var itemReceived in repaymentDataReceived)
                    {
                        var data = new STG_CONTRACT_DAILY_REPAY
                        {
                            CONTRACTREFERENCENUMBER = itemReceived.account_number,
                            LOANSYSTEMTYPEID = (short)LoanSystemTypeEnum.TermDisbursedFacility,
                            CUSTOMERACCOUNTNUMBER = itemReceived.customer_acct,
                            BRANCHCODE = itemReceived.branch_code,
                            PAYMENTDESCRIPTION = itemReceived.component_name,
                            DUEDATE = itemReceived.due_date,
                            PAYMENTDATE = itemReceived.paid_date,
                            AMOUNTPAID = itemReceived.amount_paid,
                            STATUS = false
                        };
                        stagingdata.Add(data);
                    }
                    staging.STG_CONTRACT_DAILY_REPAY.AddRange(stagingdata);

                    var saved = staging.SaveChanges() > 0;
                    if (saved)
                    {
                        return true;
                    }

                };
                
                return false;
            }
            catch(Exception e)
            {
                throw e;
            }
        }

        public bool GetOverdraftRepaymentToStaging()
        {
            try
            {
                ResponseLoanPrepaymentViewModel response = new ResponseLoanPrepaymentViewModel();
                LoanPrepaymentViewModel model = new LoanPrepaymentViewModel();
                model.auth_key = API_KEY;
                model.channel_code = "FINTRAK";
                model.review_date = DateTime.Now.Date.ToString("dd-MMM-yyyy"); 

                var loans = (from x in context.TBL_LOAN_REVOLVING
                             join c in context.TBL_CASA on x.CASAACCOUNTID equals c.CASAACCOUNTID
                             join cust in context.TBL_CUSTOMER on c.CUSTOMERID equals cust.CUSTOMERID
                             where x.LOANSTATUSID != (short)LoanStatusEnum.Inactive
                             && x.LOANSTATUSID != (short)LoanStatusEnum.Cancelled
                             && x.LOANSTATUSID != (short)LoanStatusEnum.Completed
                              && x.LOANSTATUSID != (short)LoanStatusEnum.Terminated

                             select new SubResponseLoanPrepaymentViewModel()
                             {
                                 account_number = x.LOANREFERENCENUMBER,
                                 customer_acct = c.PRODUCTACCOUNTNUMBER,
                                 user_ref_no = cust.CUSTOMERCODE,
                                 account_balance = 0,
                                 creditTurnover = 0,
                                 debitTurnover = 0,
                                 transactionDate = DateTime.Now,
                             }).ToList();

                foreach (var item in loans)
                {

                    Task.Run(async () => response = await GetOverdraftRepayment(model)).GetAwaiter().GetResult();
                    if (response.response_code == "00")
                    {
                        var repaymentDataReceived = response;

                        var data = new STG_OVERDRAFT_DAILY_REPAY
                        {
                            LOANSYSTEMTYPEID = (short)LoanSystemTypeEnum.OverdraftFacility,
                            CUSTOMERACCOUNTNUMBER = item.customer_acct,
                            ACCOUNTBALANCE = item.account_balance,
                            CREDITTURNOVER = item.creditTurnover,
                            DEBITTURNOVER = item.debitTurnover,
                            TRANSACTIONDATE = item.transactionDate,
                            STATUS = false,
                        };

                        staging.STG_OVERDRAFT_DAILY_REPAY.Add(data);
                    };
                    return staging.SaveChanges() > 0;
                };

                return true;
            }catch(Exception e)
            {
                throw e;
            }
        }

        public void GetRepaymentEntriesToStaging()
        {
            GetLoanRepaymentToStaging();
            GetOverdraftRepaymentToStaging(); 
        }

        public bool postPaymentEntries()
        {
            var unReconciledPayLog = staging.STG_CONTRACT_DAILY_REPAY.Where(x => x.STATUS == false && x.LOANSYSTEMTYPEID == (short)LoanSystemTypeEnum.TermDisbursedFacility).ToList();
            var BatchCode = CommonHelpers.GenerateRandomDigitCode(10);
            foreach (var item in unReconciledPayLog)
            {
                var loanAccount = context.TBL_LOAN.Where(x => x.COREBANKINGREF == item.CONTRACTREFERENCENUMBER).FirstOrDefault();
                var casa = this.context.TBL_CASA.FirstOrDefault(x => x.CASAACCOUNTID == loanAccount.CASAACCOUNTID && x.COMPANYID == loanAccount.COMPANYID);
                var product = this.context.TBL_PRODUCT.FirstOrDefault(x => x.PRODUCTID == loanAccount.PRODUCTID && x.COMPANYID == loanAccount.COMPANYID);

                var repaymentAccountGL = context.TBL_PRODUCT.FirstOrDefault(x => x.PRODUCTID == casa.PRODUCTID).PRINCIPALBALANCEGL.Value;

                if (loanAccount == null) continue;

                FinanceTransactionStagingViewModel newFinancialReturn = new FinanceTransactionStagingViewModel()
                {
                     creditGlAccountId = repaymentAccountGL,
                     sourceReferenceNumber = loanAccount.LOANREFERENCENUMBER,
                     creditCasaAccountId = loanAccount.CASAACCOUNTID2,
                     debitCasaAccountId = loanAccount.CASAACCOUNTID,
                     description = item.PAYMENTDESCRIPTION,
                     amount = item.AMOUNTPAID,
                     valueDate = item.DUEDATE,
                     currencyId = loanAccount.CURRENCYID,
                     destinationBranchId = loanAccount.BRANCHID,
                    // sourceApplicationId = 0,
                };

                if (item.PAYMENTDESCRIPTION == "MAIN_INT") newFinancialReturn.operationId = (short)OperationsEnum.InterestLoanRepayment;
                //else if (item.PAYMENTDESCRIPTION == "") newFinancialReturn.operationId = (short)OperationsEnum.PrincipalLoanRepayment;


                //PAYMENT DESCRIPTION IS UNKOWN
                if (newFinancialReturn.operationId <= 0) continue;

                TBL_FINANCE_TRANSACTION financePosting = new TBL_FINANCE_TRANSACTION();
                financePosting.CURRENCYID = (short)newFinancialReturn.currencyId;
                financePosting.CURRENCYRATE = loanAccount.EXCHANGERATE;
                financePosting.DEBITAMOUNT = newFinancialReturn.amount;
                financePosting.CREDITAMOUNT = newFinancialReturn.amount;
                financePosting.SOURCEREFERENCENUMBER = newFinancialReturn.sourceReferenceNumber;
                financePosting.SOURCEBRANCHID = (short)loanAccount.TERMLOANID;
                financePosting.SOURCEAPPLICATIONID = newFinancialReturn.sourceApplicationId;
                financePosting.GLACCOUNTID = newFinancialReturn.creditGlAccountId;
                financePosting.CASAACCOUNTID = newFinancialReturn.creditCasaAccountId;
                financePosting.OPERATIONID = newFinancialReturn.operationId;
                financePosting.DESCRIPTION = newFinancialReturn.description;
                financePosting.BATCHCODE = BatchCode;
                financePosting.BATCHCODE2 = "";
                financePosting.COMPANYID = loanAccount.COMPANYID;
                financePosting.APPROVEDDATETIME = item.PAYMENTDATE;
                // financePosting.APPROVEDBY = item.
                context.TBL_FINANCE_TRANSACTION.Add(financePosting);

                item.STATUS = true;
            }
            return context.SaveChanges() > 0;
        }
    }
}
