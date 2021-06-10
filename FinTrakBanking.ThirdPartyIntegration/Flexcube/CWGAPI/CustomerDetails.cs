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
    using Newtonsoft.Json.Linq;


    namespace CustomerInfo
    {
        public class CustomerDetails
        {
            private FinTrakBankingContext context;
            string API_KEY, API_URL = string.Empty;
            private List<TBL_API_URL> APIUrlConfig;


            public CustomerDetails(FinTrakBankingContext _context)
            {
                this.context = _context;
                APIUrlConfig = new List<TBL_API_URL>();

            }

            private void getAPIURLSettings(string typeName = null)
            {
                APIUrlConfig = context.TBL_API_URL.ToList();
                var apiConfig = APIUrlConfig.Where(x => x.TYPENAME.ToLower() == typeName.ToLower()).FirstOrDefault();
                if (apiConfig != null && !String.IsNullOrEmpty(apiConfig.URL))
                {
                    API_URL = apiConfig.URL.Trim();
                    API_KEY = apiConfig.APIKEY;
                }
                if (apiConfig == null || String.IsNullOrEmpty(apiConfig.URL))
                {
                    apiConfig = APIUrlConfig.Where(x => x.TYPENAME.ToUpper() == "DEFAULT").FirstOrDefault();

                    if (apiConfig != null) {
                        API_URL = apiConfig.URL.Trim();
                        API_KEY = apiConfig.APIKEY;
                    }
                }
            }

            public async Task<List<CustomerViewModels>> GetCustomerByAccountsNumber(string customerAccount)
            {
                HttpClientHandler handler = new HttpClientHandler();
                HttpClient httpClientInstance;
                handler.UseDefaultCredentials = true;
                DateTime requestDatetime = new DateTime(), responseDateTime = new DateTime();
                HttpResponseMessage response = null;
                string responseMessage = "";
                getAPIURLSettings("Customer");

                try
                {
                    HttpClient client = new HttpClient(handler);
                    var token = new AuthenticationHeaderValue("Authorization", API_KEY);
                    httpClientInstance = new HttpClient();
                    httpClientInstance.DefaultRequestHeaders.ConnectionClose = false;
                    client.Timeout = TimeSpan.FromSeconds(180);
                    client.BaseAddress = new Uri(API_URL);
                    client.DefaultRequestHeaders.Authorization = token;
                    client.DefaultRequestHeaders.Accept.Clear();
                    client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

                    List<CustomerViewModels> customers = new List<CustomerViewModels>();
                    ServicePointManager.ServerCertificateValidationCallback += (sender, cert, chain, sslPolicyErrors) => true;

                    requestDatetime = DateTime.Now;
                    response = await client.GetAsync($"GetCustomerByAccountNumber/{customerAccount}");
                    responseDateTime = DateTime.Now;

                    if (response.IsSuccessStatusCode)
                    {
                        responseMessage = await response.Content.ReadAsStringAsync();
                        //responseMessage = GetLatestEntryFromCustomApiLogs(customerAccount, "GetCustomerByAccountNumber");

                        if (responseMessage.Contains("data"))
                        {
                            JObject jsonString = JObject.Parse(responseMessage);
                            var data = jsonString["data"].ToString();
                            var objData = JsonConvert.DeserializeObject<List<CustomerViewModels>>(data);

                            foreach (var customerModel in objData)
                            {
                                if (customerModel.customerType == "C") { customerModel.customerTypeId = 2; }
                                else { customerModel.customerTypeId = 1; }

                                if (customerModel.gender == "M") { customerModel.gender = "Male"; }
                                if (customerModel.gender == "F") { customerModel.gender = "Female"; }

                                if (customerModel.customerTypeId == (short)CustomerTypeEnum.Corporate || customerModel.customerType == "C")
                                {
                                    customerModel.firstName = customerModel.companyName == null ? customerModel.company_name : customerModel.companyName;
                                    customerModel.companyName = customerModel.company_name;
                                }

                                customerModel.isPoliticallyExposed = customerModel.politicallyExposedPerson > 0;
                                customers.Add(customerModel);
                            }
                        }
                        else
                        {
                            throw new APIErrorException($"Core Banking API Error - GetCustomerByAccountNumber API is Currently Unavailable. Contact IT Admin or ESB Team for Support!");
                        }

                    }

                    handler.Dispose();
                    client.Dispose();
                    return customers;
                }
                catch (APIErrorException ex)
                {
                    throw new APIErrorException($"{ex.Message}");
                }
                catch (Exception ex)
                {
                    var innerExceptionMessage = "";
                    if (ex.InnerException != null)
                        innerExceptionMessage = ex.InnerException.Message;

                    throw new Exception($"Core Banking API Error - {ex.Message} - inner exception - {innerExceptionMessage}");
                }
                finally
                {
                    var logs = new TBL_CUSTOM_API_LOGS
                    {
                        APIURL = $"{API_URL}GetCustomerByAccountNumber/{customerAccount}",
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

            //private string GetLatestEntryFromCustomApiLogs(string customerCode, string apiUrl)
            //{
            //    FinTrakBankingContext logContext = new FinTrakBankingContext();
            //    var result = logContext.TBL_CUSTOM_API_LOGS.Where(O => O.REFERENCENUMBER == customerCode && O.APIURL.Contains(apiUrl)).OrderByDescending(O => O.APILOGID).Select(O => O.RESPONSEMESSAGE).FirstOrDefault();
            //    return result;
            //}


            public async Task<CasaBalanceViewModel> GetCustomerAccountBalance(string customerAccount)
            {
                HttpClientHandler handler = new HttpClientHandler();
                HttpClient httpClientInstance;

                HttpClient client = new HttpClient(handler);
                DateTime requestDatetime = new DateTime(), responseDateTime = new DateTime();
                HttpResponseMessage response = null;
                CasaIntegrationViewModel accountAPI = new CasaIntegrationViewModel();
               // ResponseMessageViewModel res = null;
                string responseMessage  = "";
                getAPIURLSettings("Customer");
                try
                {
                    handler.UseDefaultCredentials = true;
                   
                    var token = new AuthenticationHeaderValue("Authorization", API_KEY);
                    httpClientInstance = new HttpClient();
                    httpClientInstance.DefaultRequestHeaders.ConnectionClose = false;
                    client.Timeout = TimeSpan.FromSeconds(180);
                    client.BaseAddress = new Uri(API_URL);
                    client.DefaultRequestHeaders.Accept.Clear();
                    client.DefaultRequestHeaders.Authorization = token;

                    client.DefaultRequestHeaders.Accept.Add(
                    new MediaTypeWithQualityHeaderValue("application/json"));

                    CasaBalanceViewModel accountOutput = new CasaBalanceViewModel();
                    
                    ServicePointManager.ServerCertificateValidationCallback += (sender, cert, chain, sslPolicyErrors) => true;
                    requestDatetime = DateTime.Now;
                    //response = await client.GetAsync($"api/Customer/GetCustomerAccountBalance?accountNumber={customerAccount}");
                    response = await client.GetAsync($"GetCustomerAccountBalance/{customerAccount}"); 

                    responseDateTime = DateTime.Now;
                    if (response.IsSuccessStatusCode)
                    {
                        // = await response.Content.ReadAsAsync<CasaBalanceViewModel>();
                        string responseData = await response.Content.ReadAsStringAsync();
                        //customerViewModels = JsonConvert.DeserializeObject<CustomerTransactionViewModels>(responseData);
                        
                        JObject responseDataJsonString = JObject.Parse(responseData);
                        var data = responseDataJsonString["data"].ToString();
                        if(data != "{}") { accountOutput = JsonConvert.DeserializeObject<CasaBalanceViewModel>(data); }
                        if (data == "{}") { accountOutput = null; }


                        if (accountOutput != null)
                        {
                            var currencyId = context.TBL_CURRENCY.FirstOrDefault(x => x.CURRENCYCODE == accountOutput.currencyType).CURRENCYID;
                            if (accountOutput.accountStatus.ToLower() == "open") accountOutput.accountStatus = "Active";

                             var account = context.TBL_CASA_ACCOUNTSTATUS.FirstOrDefault(x => x.ACCOUNTSTATUSNAME.ToLower() == accountOutput.accountStatus.ToLower());
                            var accountStatusId = account != null ? account.ACCOUNTSTATUSID : 0;

                            var product = context.TBL_PRODUCT.Where(x => x.PRODUCTCODE == accountOutput.product).FirstOrDefault();

                            //accountOutput.accountName = accountAPI.accountName;
                            //accountOutput.accountNo = accountAPI.accountNumber;
                            //accountOutput.availableBalance = accountAPI.balance;
                            accountOutput.currencyId = currencyId;
                            accountOutput.accountStatusId = (CASAAccountStatusEnum)accountStatusId;
                            //accountOutput.customerCode = accountAPI.customerCode;
                            //accountOutput.product = accountAPI.product;
                            //accountOutput.currencyType = accountAPI.currencyType;
                            //accountOutput.accountStatus = accountAPI.accountStatus;
                            //accountOutput.freezeStatus = accountAPI.freezeStatus;
                            //accountOutput.freezeReason = accountAPI.freezeReason;
                            //accountOutput.lastTransactionDate = accountAPI.lastTransactionDate;
                            accountOutput.hasBalance = true;
                            accountOutput.isCasaAccountDetailAvailable = true;
                            if(product != null)
                            {
                                //accountOutput.productType = accountAPI.productType;
                                //accountOutput.productName = accountAPI.productName;
                            }


                        }
                        else { accountOutput = new CasaBalanceViewModel(); }
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
                        APIURL = $"{API_URL}GetCustomerAccountBalance/{customerAccount}",
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
                HttpClientHandler handler = new HttpClientHandler();
                HttpClient httpClientInstance;

                HttpClient client = new HttpClient(handler);
                DateTime requestDatetime = new DateTime(), responseDateTime = new DateTime();
                HttpResponseMessage response = null;
                //ResponseMessageViewModel res = null;
                string responseMessage = "";
                getAPIURLSettings("CustomerAccountBalance");

                try
                {
                    handler.UseDefaultCredentials = true;
                    var token = new AuthenticationHeaderValue("Authorization", API_KEY);

                    httpClientInstance = new HttpClient();
                    httpClientInstance.DefaultRequestHeaders.ConnectionClose = false;
                    client.Timeout = TimeSpan.FromSeconds(180);
                    client.BaseAddress = new Uri(API_URL);
                    client.DefaultRequestHeaders.Authorization = token;
                    client.DefaultRequestHeaders.Accept.Clear();
                    client.DefaultRequestHeaders.Accept.Add(
                    new MediaTypeWithQualityHeaderValue("application/json"));

                    CasaViewModel casaViewModels = new CasaViewModel();
                    ServicePointManager.ServerCertificateValidationCallback += (sender, cert, chain, sslPolicyErrors) => true;
                    requestDatetime = DateTime.Now;
                    response = await client.GetAsync($"GetCustomerAccountBalances/{customerCode}");

                    List<CasaViewModel> casa = new List<CasaViewModel>();
                    responseDateTime = DateTime.Now;

                    if (response.IsSuccessStatusCode && response.Content != null)
                    {
                        var jsonString = await response.Content.ReadAsStringAsync();
                        JObject responseDataJsonString = JObject.Parse(jsonString);
                        var jsonDataString = responseDataJsonString["data"].ToString();
                        var objData = JsonConvert.DeserializeObject<List<CasaIntegrationViewModel>>(jsonDataString);

                        foreach (var d in objData)
                        {
                            casa.Add(new CasaViewModel
                            {
                                productAccountNumber = d.accountNumber,
                                productAccountName = d.accountName,
                                productCode = d.product,
                                productName = d.productType ,
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
                catch (APIErrorException ex)
                {
                    throw new APIErrorException("Core Banking API Error - " + response.RequestMessage);
                }

                finally
                {
                    handler.Dispose();
                    client.Dispose();
                    responseDateTime = DateTime.Now;
                    var logs = new TBL_CUSTOM_API_LOGS
                    {
                        APIURL = $"{API_URL}GetCustomerAccountBalances/{customerCode}",
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

            //private string GetLatestEntryFromCustomApiLogs(string customerCode, string apiUrl)
            //{
            //    FinTrakBankingContext logContext = new FinTrakBankingContext();
            //    var result = logContext.TBL_CUSTOM_API_LOGS.Where(O => O.REFERENCENUMBER == customerCode && O.APIURL.Contains(apiUrl)).OrderByDescending(O => O.APILOGID).Select(O => O.RESPONSEMESSAGE).FirstOrDefault();
            //    return result;
            //}

            public async Task<string> CheckExposePerson(string customerCode)
            {
                HttpClientHandler handler = new HttpClientHandler();
                HttpClient httpClientInstance;

                HttpClient client = new HttpClient(handler);
                DateTime requestDatetime = new DateTime(), responseDateTime = new DateTime();
                HttpResponseMessage response = null;
              //  ResponseMessageViewModel res = null;
                string responseMessage = "";
                getAPIURLSettings("ExposedPerson");
                try
                {
                    string result = string.Empty;
                    var token = new AuthenticationHeaderValue("Authorization", API_KEY);
                    handler.UseDefaultCredentials = true;
                    //HttpClient client = new HttpClient(handler);

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
                    try { response = await client.GetAsync($"GetExposedPerson/{customerCode}"); } catch (Exception ex) { }
                    responseDateTime = DateTime.Now;
                    if (response.IsSuccessStatusCode)
                    {
                        var responseData = await response.Content.ReadAsStringAsync();
                        JObject responseDataJsonString = JObject.Parse(responseData);
                        
                        //result = responseDataJsonString["responseMessage"].ToString();
                        //result = JsonConvert.DeserializeObject<string>(responseMessageString);
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
                    responseDateTime = DateTime.Now;
                    var logs = new TBL_CUSTOM_API_LOGS
                    {
                        APIURL = $"{API_URL}GetExposedPerson/{customerCode}",
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
                HttpClientHandler handler = new HttpClientHandler();
                HttpClient httpClientInstance;


                HttpClient client = new HttpClient(handler);
                DateTime requestDatetime = new DateTime(), responseDateTime = new DateTime();
                HttpResponseMessage response = null;
                //ResponseMessageViewModel res = null;
                string responseMessage = "";
                getAPIURLSettings("BVN");

                string result = string.Empty;
                var token = new AuthenticationHeaderValue("Authorization", API_KEY);
                handler.UseDefaultCredentials = true;
                //HttpClient client = new HttpClient(handler);

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
                response = await client.GetAsync($"api/OfficeAccount/GetGeneralLedgerAccountRecord/{customerCode}");

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
                    APIURL = $"{API_URL}api/OfficeAccount/GetGlAccountRecord?customerCode={customerCode}",
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
                HttpClientHandler handler = new HttpClientHandler();
                HttpClient httpClientInstance;

                HttpClient client = new HttpClient(handler);
                DateTime requestDatetime = new DateTime(), responseDateTime = new DateTime();
                HttpResponseMessage response = null;
                InterestRateInquiryIntegrationViewModel accountAPI = new InterestRateInquiryIntegrationViewModel();
                //ResponseMessageViewModel res = null;
                string responseMessage = "";
                getAPIURLSettings("RateEnquiry");
                try
                {
                    handler.UseDefaultCredentials = true;

                    var token = new AuthenticationHeaderValue("Authorization", API_KEY);
                    httpClientInstance = new HttpClient();
                    httpClientInstance.DefaultRequestHeaders.ConnectionClose = false;
                    client.Timeout = TimeSpan.FromSeconds(180);
                    client.BaseAddress = new Uri(API_URL);
                    client.DefaultRequestHeaders.Accept.Clear();
                    client.DefaultRequestHeaders.Authorization = token;

                    client.DefaultRequestHeaders.Accept.Add(
                    new MediaTypeWithQualityHeaderValue("application/json"));

                    InterestRateInquiryViewModel accountOutput = new InterestRateInquiryViewModel();

                    ServicePointManager.ServerCertificateValidationCallback += (sender, cert, chain, sslPolicyErrors) => true;
                    requestDatetime = DateTime.Now;
                    //response = await client.GetAsync($"api/InterestRateInquiry/GetInterestRateInquiry?model.accountNumber={accountNumber}&model.accountType={accountType}",
                    response = await client.GetAsync($"GetInterestRateInquiry/{accountNumber}");

                   
                    responseDateTime = DateTime.Now;
                    if (response.IsSuccessStatusCode)
                    {
                        //accountAPI = await response.Content.ReadAsAsync<InterestRateInquiryIntegrationViewModel>();

                        var responseData = await response.Content.ReadAsStringAsync();
                        JObject jsonString = JObject.Parse(responseData);
                        var data = jsonString["data"].ToString();

                        accountAPI = JsonConvert.DeserializeObject<InterestRateInquiryIntegrationViewModel>(data);

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
                        //APIURL = $"api/InterestRateInquiry/GetInterestRateInquiry?model.accountNumber={accountNumber}&model.accountType={accountType}",
                        APIURL = $"{API_URL}GetInterestRateInquiry/{ accountNumber }",
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

            //public async Task<List<CustomerTurnoverViewModel>> GetCustomerTransactions(string customerCode, int durationInMonths)
            //{
            //    //month = 48;
            //    //cifid = "483008974";
            //    HttpClientHandler handler = new HttpClientHandler();
            //    HttpClient httpClientInstance;

            //    //var endpointUrl = $"api/Customer/GetCustomerTransactions?Cif_Id={customerCode}&Month={durationInMonths}";
            //    //var endpointUrl = $"api/Customer/GetCustomerTransactions/{customerCode}/{durationInMonths}";
            //    var endpointUrl = $"api/Customer/GetCustomerTransactions/{customerCode}/07-2019";

            //    httpClientInstance = new HttpClient();
            //    httpClientInstance.DefaultRequestHeaders.ConnectionClose = false;
            //    //
            //    handler.UseDefaultCredentials = true;
            //    var token = new AuthenticationHeaderValue("Authorization", API_KEY);

            //    HttpClient client = new HttpClient(handler);
            //    client.Timeout = TimeSpan.FromSeconds(180);
            //    client.BaseAddress = new Uri(API_URL);
            //    client.DefaultRequestHeaders.Authorization = token;
            //    client.DefaultRequestHeaders.Accept.Clear();
            //    client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

            //    ServicePointManager.ServerCertificateValidationCallback += (sender, cert, chain, sslPolicyErrors) => true;

            //    HttpResponseMessage response = null;
            //    DateTime requestTime = new DateTime();
            //    DateTime responseTime = new DateTime();

            //    requestTime = DateTime.Now;
            //    response = await client.GetAsync(endpointUrl);
            //    responseTime = DateTime.Now;

            //    List<CustomerTurnoverViewModelAPI> result = null;

            //    List<CustomerTurnoverViewModel> accounts = new List<CustomerTurnoverViewModel>();

            //    var responseMessage = await response.Content.ReadAsStringAsync();

            //    if (response.IsSuccessStatusCode)
            //    {
            //        //result = await response.Content.ReadAsAsync<List<CustomerTurnoverViewModelAPI>>();

            //        var responseData = await response.Content.ReadAsStringAsync();
            //        JObject responseDataJsonString = JObject.Parse(responseData);

            //        var data = responseDataJsonString["data"].ToString();
            //        var apiData = JsonConvert.DeserializeObject<List<CustomerTurnoverViewModelAPI>>(data);


            //        //var jsonString = await response.Content.ReadAsStringAsync();

            //        //var apiData = JsonConvert.DeserializeObject<List<CustomerTurnoverViewModelAPI>>(jsonString);

            //        foreach (var item in apiData)
            //        {

            //            decimal amc = 0;
            //            Decimal.TryParse(item.amc.Replace(",", ""), out amc);

            //            decimal vat = 0;
            //            Decimal.TryParse(item.vat.Replace(",", ""), out vat);

            //            decimal management_Fee = 0;
            //            Decimal.TryParse(item.management_Fee.Replace(",", ""), out management_Fee);

            //            decimal commitment_Fees = 0;
            //            Decimal.TryParse(item.commitment_Fees.Replace(",", ""), out commitment_Fees);

            //            decimal com_Contigent_Liab = 0;
            //            Decimal.TryParse(item.com_Contigent_Liab.Replace(",", ""), out com_Contigent_Liab);

            //            decimal lc_Commission = 0;
            //            Decimal.TryParse(item.lc_Commission.Replace(",", ""), out lc_Commission);

            //            decimal sms_Alert = 0;
            //            Decimal.TryParse(item.sms_Alert.Replace(",", ""), out lc_Commission);

            //            accounts.Add(new CustomerTurnoverViewModel
            //            {
            //                accountNumber = item.foracid,
            //                customerCode = item.cust_Id,
            //                period = item.period,
            //                productName = item.schm_Type,
            //                max_Credit_Balance = item.max_Credit_Balance,
            //                max_Debit_Balance = item.max_Debit_Balance,
            //                min_Credit_Balance = item.min_Credit_Balance,
            //                min_Debit_Balance = item.min_Debit_Balance,
            //                credit_Turnover = item.credit_Turnover,
            //                debit_Turnover = item.debit_Turnover,
            //                amc = amc,
            //                vat = vat,
            //                management_Fee = management_Fee,
            //                commitment_Fees = commitment_Fees,
            //                com_Contigent_Liab = com_Contigent_Liab,
            //                lc_Commission = lc_Commission,
            //                sms_Alert = sms_Alert,
            //                month=item.month,
            //                year = item.year,

            //            });
            //        }

            //    }


            //    handler.Dispose();
            //    client.Dispose();

            //    FintrakBankingDatabaseCustomerTurnoverOperations(
            //        endpointUrl,
            //        customerCode,
            //        requestTime,
            //        responseTime,
            //        "Cif_Id={cifid}&Month={month}",
            //        responseMessage
            //    );

            //    return accounts;
            //}


            public async Task<List<CustomerTurnoverViewModel>> GetCustomerTransactions(string accountNumber, int durationInMonths)
            {
                var currentDate = DateTime.Now;
                var startDate = DateTime.Now.AddMonths(-durationInMonths);
                getAPIURLSettings("CustomerTransactions");
                HttpClientHandler handler = new HttpClientHandler();
                HttpClient httpClientInstance;

                var endpointUrl = "";

                if (startDate.Month > 9 && currentDate.Month > 9)
                    endpointUrl = $"GetCustomerTransactions/{accountNumber}/{startDate.Month}/{currentDate.Month}/{startDate.Year}/{currentDate.Year}";
                else if (startDate.Month > 9 && currentDate.Month < 9)
                    endpointUrl = $"GetCustomerTransactions/{accountNumber}/{startDate.Month}/0{currentDate.Month}/{startDate.Year}/{currentDate.Year}";
                else if (startDate.Month < 9 && currentDate.Month > 9)
                    endpointUrl = $"GetCustomerTransactions/{accountNumber}/0{startDate.Month}/{currentDate.Month}/{startDate.Year}/{currentDate.Year}";
                else
                    endpointUrl = $"GetCustomerTransactions/{accountNumber}/0{startDate.Month}/0{currentDate.Month}/{startDate.Year}/{currentDate.Year}";

                httpClientInstance = new HttpClient();
                httpClientInstance.DefaultRequestHeaders.ConnectionClose = false;
                handler.UseDefaultCredentials = true;
                var token = new AuthenticationHeaderValue("Authorization", API_KEY);

                HttpClient client = new HttpClient(handler);
                client.Timeout = TimeSpan.FromSeconds(180);
                client.BaseAddress = new Uri(API_URL);
                client.DefaultRequestHeaders.Authorization = token;
                client.DefaultRequestHeaders.Accept.Clear();
                client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

                ServicePointManager.ServerCertificateValidationCallback += (sender, cert, chain, sslPolicyErrors) => true;

                HttpResponseMessage response = null;
                DateTime requestTime = new DateTime();
                DateTime responseTime = new DateTime();

                requestTime = DateTime.Now;
                try { response = await client.GetAsync(endpointUrl); } catch (Exception e) { throw new ConditionNotMetException(e.Message); }

                responseTime = DateTime.Now;

                List<CustomerTurnoverGroupViewModel> result = null;
                List<CustomerTurnoverViewModel> accounts = new List<CustomerTurnoverViewModel>();

                var responseMessage = await response.Content.ReadAsStringAsync();

                if (response.IsSuccessStatusCode)
                {
                    var responseData = await response.Content.ReadAsStringAsync();
                    JObject responseDataJsonString = JObject.Parse(responseData);

                    var data = responseDataJsonString["data"].ToString();
                    var apiData = JsonConvert.DeserializeObject<List<CustomerTurnoverGroupViewModel>>(data);

                    //var jsonString = await response.Content.ReadAsStringAsync();
                    //var apiData = JsonConvert.DeserializeObject<List<CustomerTurnoverViewModelAPI>>(jsonString);

                    foreach (var item in apiData)
                    {
                        for (int i = 0; i <= item.Account.Count - 1; i++)
                        {
                            if (item.Account[i].cust_Id != null)
                            {
                                decimal amc = 0;
                                Decimal.TryParse(item.Account[i].amc.Replace(",", ""), out amc);

                                decimal vat = 0;
                                Decimal.TryParse(item.Account[i].vat.Replace(",", ""), out vat);

                                decimal management_Fee = 0;
                                Decimal.TryParse(item.Account[i].management_Fee.Replace(",", ""), out management_Fee);

                                decimal commitment_Fees = 0;
                                Decimal.TryParse(item.Account[i].commitment_Fees.Replace(",", ""), out commitment_Fees);

                                decimal com_Contigent_Liab = 0;
                                Decimal.TryParse(item.Account[i].com_Contigent_Liab.Replace(",", ""), out com_Contigent_Liab);

                                decimal lc_Commission = 0;
                                Decimal.TryParse(item.Account[i].lc_Commission.Replace(",", ""), out lc_Commission);

                                decimal sms_Alert = 0;
                                Decimal.TryParse(item.Account[i].sms_Alert.Replace(",", ""), out lc_Commission);

                                if (item.Account[i].max_Credit_Balance != null || item.Account[i].max_Debit_Balance != null || item.Account[i].min_Credit_Balance != null || item.Account[i].min_Debit_Balance != null || item.Account[i].credit_Turnover != null || item.Account[i].debit_Turnover != null) {
                                    accounts.Add(new CustomerTurnoverViewModel
                                    {
                                        accountNumber = item.Account[i].foracid,
                                        customerCode = item.Account[i].cust_Id,
                                        period = item.Account[i].period,
                                        productName = item.Account[i].schm_Type,
                                        max_Credit_Balance = item.Account[i].max_Credit_Balance,
                                        max_Debit_Balance = item.Account[i].max_Debit_Balance,
                                        min_Credit_Balance = item.Account[i].min_Credit_Balance,
                                        min_Debit_Balance = item.Account[i].min_Debit_Balance,
                                        credit_Turnover = item.Account[i].credit_Turnover,
                                        debit_Turnover = item.Account[i].debit_Turnover,
                                        amc = amc,
                                        vat = vat,
                                        management_Fee = management_Fee,
                                        commitment_Fees = commitment_Fees,
                                        com_Contigent_Liab = com_Contigent_Liab,
                                        lc_Commission = lc_Commission,
                                        sms_Alert = sms_Alert,
                                        month = item.Account[i].month,
                                        year = item.Account[i].year,
                                    });
                                }

                            }

                        }

                    }

                }

                handler.Dispose();
                client.Dispose();

                FintrakBankingDatabaseCustomerTurnoverOperations(API_URL,
                    endpointUrl,
                    accountNumber,
                    requestTime,
                    responseTime,
                    $"Account Number: {accountNumber}, Duration In Months: {durationInMonths}", //"Cif_Id={cifid}&Month={month}",
                    responseMessage
                );

                return accounts;
            }

            public async Task<List<CustomerTurnoverViewModel>> GetCustomerInterestTransactions(string customerCode, int durationInMonths)
            {
                //API_URL = "http://10.111.13.47:7002/fintrakapi/v1/";
                var currentDate = DateTime.Now;
                var startDate = DateTime.Now.AddMonths(-durationInMonths);

                getAPIURLSettings("CustomerLoanInterestDetails");
                HttpClientHandler handler = new HttpClientHandler();
                HttpClient httpClientInstance;
                var endpointUrl = "";

                //var endpointUrl = $"api/Customer/GetCustomerLoanInterestDetails/{customerCode}/{durationInMonths}";
                //  endpointUrl = $"GetCustomerLoanInterestDetails/{customerCode}/{durationInMonths}-{startDate.Year}";

                if (durationInMonths > 9)
                    endpointUrl = $"GetCustomerLoanInterestDetails/{customerCode}/{durationInMonths}";
                else
                    endpointUrl = $"GetCustomerLoanInterestDetails/{customerCode}/0{durationInMonths}";

                httpClientInstance = new HttpClient();
                httpClientInstance.DefaultRequestHeaders.ConnectionClose = false;
                handler.UseDefaultCredentials = true;
                var token = new AuthenticationHeaderValue("Authorization", API_KEY);

                HttpClient client = new HttpClient(handler);
                client.Timeout = TimeSpan.FromSeconds(180);
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

                List<CustomerTurnoverViewModel> accounts = new List<CustomerTurnoverViewModel>();

                var responseMessage = await response.Content.ReadAsStringAsync();

                if (response.IsSuccessStatusCode)
                {
                    var responseData = await response.Content.ReadAsStringAsync();
                    JObject responseDataJsonString = JObject.Parse(responseData);

                    var data = responseDataJsonString["data"].ToString();
                    var apiData = JsonConvert.DeserializeObject<List<CustomerTurnoverViewModelAPI>>(data);

                    foreach (var item in apiData)
                    {
                        decimal float_Charge = 0;
                        Decimal.TryParse(item.float_Charge.Replace(",", ""), out float_Charge);
                        decimal interest = 0;
                        Decimal.TryParse(item.interest.Replace(",", ""), out interest);

                        accounts.Add(new CustomerTurnoverViewModel
                        {
                            accountNumber = item.foracid,
                            customerCode = item.cust_Id,
                            period = item.period,
                            productName = item.schm_Type,
                            interest = interest,
                            float_Charge = float_Charge,
                            //month = month,
                            //year = year,
                            month = startDate.Month,
                            year = startDate.Year,
                        });
                    }

                }


                handler.Dispose();
                client.Dispose();

                FintrakBankingDatabaseCustomerTurnoverOperations(API_URL,
                    endpointUrl,
                    customerCode,
                    requestTime,
                    responseTime,
                    $"Customer Code: {customerCode}, Duration In Months: {durationInMonths}", //"Cif_Id={cifid}&Month={month}",
                    responseMessage
                );

                return accounts;
            }

            private void FintrakBankingDatabaseCustomerTurnoverOperations(string baseUrl,
                string endpointUrl,
                string cifid,
                DateTime requestTime,
                DateTime responseTime,
                string requestMessage,
                string responseMessage
                
                )
            {

                var logs = new TBL_CUSTOM_API_LOGS
                {
                    APIURL = baseUrl + endpointUrl,
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

