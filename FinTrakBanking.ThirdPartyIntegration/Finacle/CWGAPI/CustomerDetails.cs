namespace FinTrakBanking.ThirdPartyIntegration
{
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using Newtonsoft.Json;
    using System.Net;
    using System.Net.Http;
    using System.Net.Http.Headers;
    using FintrakBanking.ViewModels.Customer;
    using FintrakBanking.ViewModels.ThridPartyIntegration;
    using FintrakBanking.ViewModels.CASA;
    using FintrakBanking.Entities.Models;
    using System.Linq;
    using FintrakBanking.ViewModels.Finance;
    using FintrakBanking.Common.Enum;
    using FintrakBanking.Common.CustomException;
    using System.Web.Script.Serialization;

    namespace CustomerInfo
    {
        public class CustomerDetails
        {
            private FinTrakBankingContext context;
            string API_KEY, API_URL = string.Empty;

            private static HttpClient httpClientInstance;
            private HttpClientHandler handler = new HttpClientHandler();

            public CustomerDetails(FinTrakBankingContext _context)
            {
                this.context = _context;
                var configdata = context.TBL_SETUP_COMPANY.FirstOrDefault();
                API_KEY = configdata.APIKEY;
                API_URL = configdata.APIURL;
            } 

            public void Run()
            {
                //httpClientInstance = new HttpClient();
                //httpClientInstance.DefaultRequestHeaders.ConnectionClose = false;

                //client.BaseAddress = new Uri("https://172.16.249.195/FbnFintrak.Api.Test/");
                //client.DefaultRequestHeaders.Accept.Clear();
                //client.DefaultRequestHeaders.Accept.Add(
                //new MediaTypeWithQualityHeaderValue("application/json"));


                //ServicePointManager.FindServicePoint(client.BaseAddress)
                //.ConnectionLeaseTimeout = (int)TimeSpan.FromMinutes(1).TotalMilliseconds;
                //ServicePointManager.DnsRefreshTimeout = (int)TimeSpan.FromMinutes(1).TotalMilliseconds;
                //ServicePointManager.FindServicePoint(client.BaseAddress).ConnectionLeaseTimeout = 60 * 1000;
            }
 
            public async Task<List<CustomerViewModels>> GetCustomerByAccountsNumber(string customerAccount)
            {

                handler.UseDefaultCredentials = true;
                DateTime requestDatetime = new DateTime(), responseDateTime = new DateTime();
                HttpResponseMessage response = null;
                ResponseMessageViewModel res = null;
                string responseMessage = "";
                HttpClient client = new HttpClient(handler);
                var token = new AuthenticationHeaderValue("Authorization", API_KEY);
                httpClientInstance = new HttpClient();
                httpClientInstance.DefaultRequestHeaders.ConnectionClose = false;
                client.Timeout = TimeSpan.FromSeconds(60);
                client.BaseAddress = new Uri(API_URL);
                client.DefaultRequestHeaders.Authorization = token;
                client.DefaultRequestHeaders.Accept.Clear();
                client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));



                CustomerTransactionViewModels customerViewModels = new CustomerTransactionViewModels();
                List<CustomerViewModels> customers = new List<CustomerViewModels>();
                ServicePointManager.ServerCertificateValidationCallback += (sender, cert, chain, sslPolicyErrors) => true;

                requestDatetime = DateTime.Now;
                //ServicePointManager.FindServicePoint(client.BaseAddress).ConnectionLeaseTimeout = 60 * 1000;
                response = await client.GetAsync($"api/Customer/GetCustomerByAccountNumber?accountNumber={customerAccount}");
                responseDateTime = DateTime.Now;
                if (response.IsSuccessStatusCode)
                { 
                    customerViewModels = await response.Content.ReadAsAsync<CustomerTransactionViewModels>();

                    customers.Add(new CustomerViewModels
                    {
                        customerCode = customerViewModels.customerCode,
                        firstName = customerViewModels.lastName,
                        lastName = customerViewModels.firstName,
                        middleName = customerViewModels.middleName,
                        customerTypeName = customerViewModels.customerType,
                        customerTypeId = (short)(customerViewModels.customerType == "CORPORATE" ? 2 : 1),
                        isPoliticallyExposed = customerViewModels.politicallyExposedPerson == "N" ? false : true,
                    });

                }
                responseMessage = await response.Content.ReadAsStringAsync();
                handler.Dispose();
                client.Dispose();

                var logs = new TBL_CUSTOM_API_LOGS
                {
                    APIURL = $"api/Customer/GetCustomerByAccountNumber?accountNumber={customerAccount}",
                    LOGTYPEID = 4,
                    REFERENCENUMBER = customerAccount,
                    REQUESTDATETIME = requestDatetime,
                    REQUESTMESSAGE = customerAccount,
                    RESPONSEDATETIME = responseDateTime,
                    RESPONSEMESSAGE = responseMessage,
                };
                FinTrakBankingContext logContext = new FinTrakBankingContext();

                logContext.TBL_CUSTOM_API_LOGS.Add(logs);

                logContext.SaveChanges();

                return customers;


            } 

            public async Task<CasaBalanceViewModel> GetCustomerAccountBalance(string customerAccount)
            {
                HttpClient client = new HttpClient(handler);
                DateTime requestDatetime = new DateTime(), responseDateTime = new DateTime();
                HttpResponseMessage response = null;
                CasaIntegrationViewModel accountAPI = new CasaIntegrationViewModel();
                ResponseMessageViewModel res = null;
                string responseMessage  = "";
                try
                {
                    handler.UseDefaultCredentials = true;
                   
                    var token = new AuthenticationHeaderValue("Authorization", API_KEY);
                    httpClientInstance = new HttpClient();
                    httpClientInstance.DefaultRequestHeaders.ConnectionClose = false;
                    client.Timeout = TimeSpan.FromSeconds(60);
                    client.BaseAddress = new Uri(API_URL);
                    client.DefaultRequestHeaders.Accept.Clear();
                    client.DefaultRequestHeaders.Authorization = token;

                    client.DefaultRequestHeaders.Accept.Add(
                    new MediaTypeWithQualityHeaderValue("application/json"));

                    CasaBalanceViewModel accountOutput = new CasaBalanceViewModel();
                    
                    ServicePointManager.ServerCertificateValidationCallback += (sender, cert, chain, sslPolicyErrors) => true;
                    requestDatetime = DateTime.Now;
                    response = await client.GetAsync($"api/Customer/GetCustomerAccountBalance?accountNumber={customerAccount}");

                    responseDateTime = DateTime.Now;
                    if (response.IsSuccessStatusCode)
                    {
                        accountAPI = await response.Content.ReadAsAsync<CasaIntegrationViewModel>();
                        

                        var currencyId = context.TBL_CURRENCY.FirstOrDefault(x => x.CURRENCYCODE == accountAPI.currencyType).CURRENCYID;
                        var account = context.TBL_CASA_ACCOUNTSTATUS.FirstOrDefault(x => x.ACCOUNTSTATUSNAME.ToLower() == accountAPI.accountStatus.ToLower());
                        var accountStatusId = account.ACCOUNTSTATUSID;

                        accountOutput.accountName = accountAPI.accountName;
                        accountOutput.accountNo = accountAPI.accountNumber;
                        accountOutput.availableBalance = accountAPI.balance;
                        accountOutput.productName = accountAPI.productName;
                        accountOutput.currencyId = currencyId;
                        accountOutput.accountStatusId = (CASAAccountStatusEnum)accountStatusId;
                        accountOutput.customerCode = accountAPI.customerCode;
                        accountOutput.product = accountAPI.product;
                        accountOutput.productType = accountAPI.productType;
                        accountOutput.currencyType = accountAPI.currencyType;
                        accountOutput.accountStatus = accountAPI.accountStatus;
                        accountOutput.freezeStatus = accountAPI.freezeStatus;
                        accountOutput.freezeReason = accountAPI.freezeReason;
                        accountOutput.lastTransactionDate = accountAPI.lastTransactionDate;
                        accountOutput.hasBalance = true;
                        accountOutput.isCasaAccountDetailAvailable = true;
                    }

                    //responseApi = await response.Content.ReadAsAsync<TransactionPostingViewModel>();

                    responseMessage = await response.Content.ReadAsStringAsync(); //.ReadAsAsync<CasaIntegrationViewModel>();

                    if (response.IsSuccessStatusCode == false)
                    {
                        accountOutput.hasBalance = false;
                        accountOutput.errorMessage = responseMessage;
                    }

                        handler.Dispose();
                    client.Dispose();

                    return accountOutput;
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
                        APIURL = $"api/Customer/GetCustomerAccountBalance?accountNumber={customerAccount}",
                        LOGTYPEID = 1,
                        REFERENCENUMBER = customerAccount,
                        REQUESTDATETIME = requestDatetime,
                        REQUESTMESSAGE = customerAccount,
                        RESPONSEDATETIME = responseDateTime,
                        RESPONSEMESSAGE = responseMessage,
                    };
                    FinTrakBankingContext logContext = new FinTrakBankingContext();

                    logContext.TBL_CUSTOM_API_LOGS.Add(logs);

                    logContext.SaveChanges();
                }
            }

            public async Task<List<CasaViewModel>> GetCustomerAccountsBalanceByCustomerCode(string customerCode)
            {
                HttpClient client = new HttpClient(handler);
                DateTime requestDatetime = new DateTime(), responseDateTime = new DateTime();
                HttpResponseMessage response = null;
                ResponseMessageViewModel res = null;
                string responseMessage = "";
                try

                {
                    handler.UseDefaultCredentials = true;
                    var token = new AuthenticationHeaderValue("Authorization", API_KEY);

                    httpClientInstance = new HttpClient();
                    httpClientInstance.DefaultRequestHeaders.ConnectionClose = false;
                    client.Timeout = TimeSpan.FromSeconds(60);
                    client.BaseAddress = new Uri(API_URL);
                    client.DefaultRequestHeaders.Authorization = token;
                    client.DefaultRequestHeaders.Accept.Clear();
                    client.DefaultRequestHeaders.Accept.Add(
                    new MediaTypeWithQualityHeaderValue("application/json"));


                    CasaViewModel casaViewModels = new CasaViewModel();
                    ServicePointManager.ServerCertificateValidationCallback += (sender, cert, chain, sslPolicyErrors) => true;
                    requestDatetime = DateTime.Now;
                    response = await client.GetAsync($"api/Customer/GetCustomerAccountsBalance?customerCode={customerCode}");

                    List<CasaViewModel> casa = new List<CasaViewModel>();
                    responseDateTime = DateTime.Now;
                    if (response.IsSuccessStatusCode)
                    {
                        var jsonString = await response.Content.ReadAsStringAsync();
                        var objData = JsonConvert.DeserializeObject<List<CasaIntegrationViewModel>>(jsonString);

                        foreach (var d in objData)
                        {
                            casa.Add(new CasaViewModel
                            {
                                productAccountNumber = d.accountNumber,
                                productAccountName = d.accountName,
                                productCode = d.productType,
                                productName = d.productName,
                                currency = d.currencyType,
                                branchCode = d.branch,
                                accountStatusName = d.accountStatus,
                                //effectiveDate = d.lastTransactionDate,
                                availableBalance = d.balance,
                                ledgerBalance = d.balance,
                            });
                        }
                    }
                    handler.Dispose();
                    client.Dispose();
                    responseMessage = await response.Content.ReadAsStringAsync();

                    return casa;
                }
                catch (Exception ex)
                {
                    throw new APIErrorException("Core Banking API Error - " +ex.Message);
                }

                finally
                {
                    handler.Dispose();
                    client.Dispose();

                    var logs = new TBL_CUSTOM_API_LOGS
                    {
                        APIURL = $"api/Customer/GetCustomerAccountsBalance?customerCode={customerCode}",
                        LOGTYPEID = 5,
                        REFERENCENUMBER = customerCode,
                        REQUESTDATETIME = requestDatetime,
                        REQUESTMESSAGE = customerCode,
                        RESPONSEDATETIME = responseDateTime,
                        RESPONSEMESSAGE = responseMessage,
                    };
                    FinTrakBankingContext logContext = new FinTrakBankingContext();

                    logContext.TBL_CUSTOM_API_LOGS.Add(logs);

                    logContext.SaveChanges();
                }
            }

            public async Task<string> CheckExposePerson(string customerCode)
            {
                HttpClient client = new HttpClient(handler);
                DateTime requestDatetime = new DateTime(), responseDateTime = new DateTime();
                HttpResponseMessage response = null;
                ResponseMessageViewModel res = null;
                string responseMessage = "";
                try
                {
                    string result = string.Empty;
                    var token = new AuthenticationHeaderValue("Authorization", API_KEY);
                    handler.UseDefaultCredentials = true;
                    //HttpClient client = new HttpClient(handler);

                    httpClientInstance = new HttpClient();
                    httpClientInstance.DefaultRequestHeaders.ConnectionClose = false;
                    client.Timeout = TimeSpan.FromSeconds(60);
                    client.DefaultRequestHeaders.Authorization = token;
                    client.BaseAddress = new Uri(API_URL);
                    client.DefaultRequestHeaders.Accept.Clear();
                    client.DefaultRequestHeaders.Accept.Add(
                    new MediaTypeWithQualityHeaderValue("application/json"));

                    ServicePointManager.ServerCertificateValidationCallback += (sender, cert, chain, sslPolicyErrors) => true;
                    requestDatetime = DateTime.Now;
                    response = await client.GetAsync($"api/ExposePerson/Get?customerCode={customerCode}");
                    responseDateTime = DateTime.Now;
                    if (response.IsSuccessStatusCode)
                    {
                        var jsonString = await response.Content.ReadAsStringAsync();
                        result = JsonConvert.DeserializeObject<string>(jsonString);
                    }
                    responseMessage = await response.Content.ReadAsStringAsync();
                    handler.Dispose();
                    client.Dispose();
                    return result;
                }
                catch (Exception ex)
                {
                    throw new APIErrorException("Core Banking API Error - " + ex.Message);
                }

                finally
                {
                    handler.Dispose();
                    client.Dispose();

                    var logs = new TBL_CUSTOM_API_LOGS
                    {
                        APIURL = $"api/ExposePerson/Get?customerCode={customerCode}",
                        LOGTYPEID = 6,
                        REFERENCENUMBER = customerCode,
                        REQUESTDATETIME = requestDatetime,
                        REQUESTMESSAGE = customerCode,
                        RESPONSEDATETIME = responseDateTime,
                        RESPONSEMESSAGE = responseMessage,
                    };
                    FinTrakBankingContext logContext = new FinTrakBankingContext();

                    logContext.TBL_CUSTOM_API_LOGS.Add(logs);

                    logContext.SaveChanges();
                }
            }

            public async Task<BVNCustomerDetailsViewModel> BVNCustomerDetails(string customerCode)
            {
                HttpClient client = new HttpClient(handler);
                DateTime requestDatetime = new DateTime(), responseDateTime = new DateTime();
                HttpResponseMessage response = null;
                ResponseMessageViewModel res = null;
                string responseMessage = "";
                string result = string.Empty;
                var token = new AuthenticationHeaderValue("Authorization", API_KEY);
                handler.UseDefaultCredentials = true;
                //HttpClient client = new HttpClient(handler);

                httpClientInstance = new HttpClient();
                httpClientInstance.DefaultRequestHeaders.ConnectionClose = false;
                client.Timeout = TimeSpan.FromSeconds(60);
                client.DefaultRequestHeaders.Authorization = token;
                client.BaseAddress = new Uri(API_URL);
                client.DefaultRequestHeaders.Accept.Clear();
                client.DefaultRequestHeaders.Accept.Add(
                new MediaTypeWithQualityHeaderValue("application/json"));

                ServicePointManager.ServerCertificateValidationCallback += (sender, cert, chain, sslPolicyErrors) => true;
                requestDatetime = DateTime.Now;
                response = await client.GetAsync($"api/OfficeAccount/GetGlAccountRecord?customerCode={customerCode}");
                responseDateTime = DateTime.Now;
                BVNCustomerDetailsViewModel data = null;
                if (response.IsSuccessStatusCode)
                {
                    var jsonString = await response.Content.ReadAsStringAsync();
                    dynamic dataObj = JsonConvert.DeserializeObject<string>(jsonString);

                    foreach (var d in dataObj)
                    {
                        data = (new BVNCustomerDetailsViewModel
                        {
                            accountNumber = d.accountNumber,
                            contactAddress = d.contactAddress,
                            dateOfBirth = d.dateOfBirth,
                            emailAddress = d.emailAddress,
                            firstName = d.firstName,
                            lastName = d.lastName,
                            middleName = d.middleName,
                            phoneNumber = d.phoneNumber

                        });
                    }

                }
                responseMessage = await response.Content.ReadAsStringAsync();
                handler.Dispose();
                client.Dispose();

                var logs = new TBL_CUSTOM_API_LOGS
                {
                    APIURL = $"api/OfficeAccount/GetGlAccountRecord?customerCode={customerCode}",
                    LOGTYPEID = 7,
                    REFERENCENUMBER = customerCode,
                    REQUESTDATETIME = requestDatetime,
                    REQUESTMESSAGE = customerCode,
                    RESPONSEDATETIME = responseDateTime,
                    RESPONSEMESSAGE = responseMessage,
                };
                FinTrakBankingContext logContext = new FinTrakBankingContext();

                logContext.TBL_CUSTOM_API_LOGS.Add(logs);

                logContext.SaveChanges();

                return data;
            }

            public async Task<InterestRateInquiryViewModel> GetInterestRateInquiry(string accountNumber, string accountType)
            {
                HttpClient client = new HttpClient(handler);
                DateTime requestDatetime = new DateTime(), responseDateTime = new DateTime();
                HttpResponseMessage response = null;
                InterestRateInquiryIntegrationViewModel accountAPI = new InterestRateInquiryIntegrationViewModel();
                ResponseMessageViewModel res = null;
                string responseMessage = "";
                try
                {
                    handler.UseDefaultCredentials = true;

                    var token = new AuthenticationHeaderValue("Authorization", API_KEY);
                    httpClientInstance = new HttpClient();
                    httpClientInstance.DefaultRequestHeaders.ConnectionClose = false;
                    client.Timeout = TimeSpan.FromSeconds(60);
                    client.BaseAddress = new Uri(API_URL);
                    client.DefaultRequestHeaders.Accept.Clear();
                    client.DefaultRequestHeaders.Authorization = token;

                    client.DefaultRequestHeaders.Accept.Add(
                    new MediaTypeWithQualityHeaderValue("application/json"));

                    InterestRateInquiryViewModel accountOutput = new InterestRateInquiryViewModel();

                    ServicePointManager.ServerCertificateValidationCallback += (sender, cert, chain, sslPolicyErrors) => true;
                    requestDatetime = DateTime.Now;
                    response = await client.GetAsync($"api/InterestRateInquiry/GetInterestRateInquiry?model.accountNumber={accountNumber}&model.accountType={accountType}");

                    responseDateTime = DateTime.Now;
                    if (response.IsSuccessStatusCode)
                    {
                        accountAPI = await response.Content.ReadAsAsync<InterestRateInquiryIntegrationViewModel>();
                        accountOutput.accountNumber = accountAPI.interestRateDetails.accountNumber;
                        accountOutput.accountType = accountAPI.interestRateDetails.accountType;
                        accountOutput.interestTableCode = accountAPI.interestRateDetails.interestTableCode;
                        accountOutput.interestSerialNumber = accountAPI.interestRateDetails.interestSerialNumber;
                        accountOutput.startDate = accountAPI.interestRateDetails.startDate;
                        accountOutput.endDate = accountAPI.interestRateDetails.endDate;
                        accountOutput.interestRateAmount = accountAPI.interestRateDetails.interestRateAmount;
                        accountOutput.lastChangedDate = accountAPI.interestRateDetails.lastChangedDate;
                    }

                    //responseApi = await response.Content.ReadAsAsync<TransactionPostingViewModel>();

                    responseMessage = await response.Content.ReadAsStringAsync(); //.ReadAsAsync<CasaIntegrationViewModel>();

                    handler.Dispose();
                    client.Dispose();

                    return accountOutput;
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
                        APIURL = $"api/InterestRateInquiry/GetInterestRateInquiry?model.accountNumber={accountNumber}&model.accountType={accountType}",
                        LOGTYPEID = 18,
                        REFERENCENUMBER = accountNumber,
                        REQUESTDATETIME = requestDatetime,
                        REQUESTMESSAGE = accountNumber + '_' + accountType,
                        RESPONSEDATETIME = responseDateTime,
                        RESPONSEMESSAGE = responseMessage,
                    };
                    FinTrakBankingContext logContext = new FinTrakBankingContext();

                    logContext.TBL_CUSTOM_API_LOGS.Add(logs);

                    logContext.SaveChanges();
                }
            }

            public async Task<List<CustomerTurnoverViewModels>> GetCustomerTransactions(string cifid, int month)
            {
                //month = 48;
                //cifid = "483008974";

                var endpointUrl = $"api/Customer/GetCustomerTransactions?Cif_Id={cifid}&Month={month}";

                httpClientInstance = new HttpClient();
                httpClientInstance.DefaultRequestHeaders.ConnectionClose = false;
                //
                handler.UseDefaultCredentials = true;
                var token = new AuthenticationHeaderValue("Authorization", API_KEY);

                HttpClient client = new HttpClient(handler);
                client.Timeout = TimeSpan.FromSeconds(60);
                client.BaseAddress = new Uri(API_URL);
                client.DefaultRequestHeaders.Authorization = token;
                client.DefaultRequestHeaders.Accept.Clear();
                client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

                ServicePointManager.ServerCertificateValidationCallback += (sender, cert, chain, sslPolicyErrors) => true;

                HttpResponseMessage response = null;
                DateTime requestTime = new DateTime();
                DateTime responseTime = new DateTime();

                requestTime = DateTime.Now;
                response = await client.GetAsync(endpointUrl);
                responseTime = DateTime.Now;

                List<CustomerTurnoverViewModels> result = null;
                // var responseMessage = await response.Content.ReadAsStringAsync();
                if (response.IsSuccessStatusCode) result = await response.Content.ReadAsAsync<List<CustomerTurnoverViewModels>>();

                handler.Dispose();
                client.Dispose();

                FintrakBankingDatabaseCustomerTurnoverOperations(
                    endpointUrl,
                    cifid,
                    requestTime,
                    responseTime,
                    "Cif_Id={cifid}&Month={month}",
                    response.Content.ReadAsStringAsync().Result,
                    result
                );

                return result;
            }

            public async Task<List<CustomerTurnoverViewModels>> GetCustomerInterestTransactions(string cifid, int month)
            {
                //month = 48;
                //cifid = "230009868";

                var endpointUrl = $"api/Customer/GetCustomerLoanInterestDetails?Cif_Id={cifid}&Month={month}";

                httpClientInstance = new HttpClient();
                httpClientInstance.DefaultRequestHeaders.ConnectionClose = false;
                //
                handler.UseDefaultCredentials = true;
                var token = new AuthenticationHeaderValue("Authorization", API_KEY);

                HttpClient client = new HttpClient(handler);
                client.Timeout = TimeSpan.FromSeconds(60);
                client.BaseAddress = new Uri(API_URL);
                client.DefaultRequestHeaders.Authorization = token;
                client.DefaultRequestHeaders.Accept.Clear();
                client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

                ServicePointManager.ServerCertificateValidationCallback += (sender, cert, chain, sslPolicyErrors) => true;

                HttpResponseMessage response = null;
                DateTime requestTime = new DateTime();
                DateTime responseTime = new DateTime();

                requestTime = DateTime.Now;
                response = await client.GetAsync(endpointUrl);
                responseTime = DateTime.Now;

                List<CustomerTurnoverViewModels> result = null;
                // var responseMessage = await response.Content.ReadAsStringAsync();
                if (response.IsSuccessStatusCode) result = await response.Content.ReadAsAsync<List<CustomerTurnoverViewModels>>();
                
                handler.Dispose();
                client.Dispose();

                FintrakBankingDatabaseCustomerTurnoverOperations(
                    endpointUrl,
                    cifid,
                    requestTime,
                    responseTime,
                    "Cif_Id={cifid}&Month={month}",
                    response.Content.ReadAsStringAsync().Result,
                    result
                );

                return result;
            }


            private void FintrakBankingDatabaseCustomerTurnoverOperations(
                string endpointUrl,
                string cifid,
                DateTime requestTime,
                DateTime responseTime,
                string requestMessage,
                string responseMessage,
                List<CustomerTurnoverViewModels> result
                )
            {

                var logs = new TBL_CUSTOM_API_LOGS
                {
                    APIURL = endpointUrl,
                    LOGTYPEID = 4,
                    REFERENCENUMBER = cifid,
                    REQUESTDATETIME = requestTime,
                    REQUESTMESSAGE = requestMessage,
                    RESPONSEDATETIME = responseTime,
                    RESPONSEMESSAGE = responseMessage,
                };

                FinTrakBankingContext logContext = new FinTrakBankingContext();
                logContext.TBL_CUSTOM_API_LOGS.Add(logs);
                logContext.SaveChanges();
            }

        }
    }

}

