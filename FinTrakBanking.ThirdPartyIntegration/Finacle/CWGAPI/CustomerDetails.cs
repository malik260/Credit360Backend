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

    namespace CustomerInfo
    {
        public class CustomerDetails
        {

            private FinTrakBankingContext context;
            string API_KEY, API_URL = string.Empty;
            public CustomerDetails(

            FinTrakBankingContext _context)
            {
                this.context = _context;
                var configdata = context.TBL_SETUP_COMPANY.FirstOrDefault();
                API_KEY = configdata.APIKEY;
                API_URL = configdata.APIURL;
            } 
            private HttpClientHandler handler = new HttpClientHandler();
            private static HttpClient httpClientInstance;

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
                HttpClient client = new HttpClient(handler);
                var token = new AuthenticationHeaderValue("Authorization", API_KEY);
                httpClientInstance = new HttpClient();
                httpClientInstance.DefaultRequestHeaders.ConnectionClose = false;
                client.Timeout = TimeSpan.FromSeconds(30);
                client.BaseAddress = new Uri(API_URL);
                client.DefaultRequestHeaders.Authorization = token;
                client.DefaultRequestHeaders.Accept.Clear();
                client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));



                CustomerIntegrationViewModels customerViewModels = new CustomerIntegrationViewModels();
                List<CustomerViewModels> customers = new List<CustomerViewModels>();
                ServicePointManager.ServerCertificateValidationCallback += (sender, cert, chain, sslPolicyErrors) => true; 
                ServicePointManager.FindServicePoint(client.BaseAddress).ConnectionLeaseTimeout = 60 * 1000;
                HttpResponseMessage response = await client.GetAsync($"api/Customer/GetCustomerByAccountNumber?accountNumber={customerAccount}");

                if (response.IsSuccessStatusCode)
                { 
                    customerViewModels = await response.Content.ReadAsAsync<CustomerIntegrationViewModels>();

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
                handler.Dispose();
                client.Dispose();

                return customers;


            } 
            public async Task<CasaBalanceViewModel> GetCustomerAccountBalance(string customerAccount)
            {
                try
                {
                    handler.UseDefaultCredentials = true;
                    HttpClient client = new HttpClient(handler);
                    var token = new AuthenticationHeaderValue("Authorization", API_KEY);
                    httpClientInstance = new HttpClient();
                    httpClientInstance.DefaultRequestHeaders.ConnectionClose = false;
                    client.Timeout = TimeSpan.FromSeconds(30);
                    client.BaseAddress = new Uri(API_URL);
                    client.DefaultRequestHeaders.Accept.Clear();
                    client.DefaultRequestHeaders.Authorization = token;

                    client.DefaultRequestHeaders.Accept.Add(
                    new MediaTypeWithQualityHeaderValue("application/json"));

                    CasaBalanceViewModel accountOutput = new CasaBalanceViewModel();
                    CasaIntegrationViewModel accountAPI = new CasaIntegrationViewModel();
                    ServicePointManager.ServerCertificateValidationCallback += (sender, cert, chain, sslPolicyErrors) => true;
                    HttpResponseMessage response = await client.GetAsync($"api/Customer/GetCustomerAccountBalance?accountNumber={customerAccount}");
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
                    }
                    handler.Dispose();
                    client.Dispose();

                    return accountOutput;
                }
                catch (Exception ex)
                {
                    throw new APIErrorException("API Call: System could not establish connection to remote server. Contact Administrator" );
                }
            }

            public async Task<List<CasaViewModel>> GetCustomerAccountsBalanceByCustomerCode(string customerCode)
            {
                try
                {
                    handler.UseDefaultCredentials = true;
                    HttpClient client = new HttpClient(handler);
                    var token = new AuthenticationHeaderValue("Authorization", API_KEY);

                    httpClientInstance = new HttpClient();
                    httpClientInstance.DefaultRequestHeaders.ConnectionClose = false;
                    client.Timeout = TimeSpan.FromSeconds(30);
                    client.BaseAddress = new Uri(API_URL);
                    client.DefaultRequestHeaders.Authorization = token;
                    client.DefaultRequestHeaders.Accept.Clear();
                    client.DefaultRequestHeaders.Accept.Add(
                    new MediaTypeWithQualityHeaderValue("application/json"));


                    CasaViewModel casaViewModels = new CasaViewModel();
                    ServicePointManager.ServerCertificateValidationCallback += (sender, cert, chain, sslPolicyErrors) => true;
                    HttpResponseMessage response = await client.GetAsync($"api/Customer/GetCustomerAccountsBalance?customerCode={customerCode}");

                    List<CasaViewModel> casa = new List<CasaViewModel>();

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

                    return casa;
                }
                catch (Exception ex)
                {
                    throw new APIErrorException("Core Banking API Error - " +ex.Message);
                }
            }

            public async Task<string> CheckExposePerson(string customerCode)
            {
                try
                {
                    string result = string.Empty;
                    var token = new AuthenticationHeaderValue("Authorization", API_KEY);
                    handler.UseDefaultCredentials = true;
                    HttpClient client = new HttpClient(handler);

                    httpClientInstance = new HttpClient();
                    httpClientInstance.DefaultRequestHeaders.ConnectionClose = false;
                    client.Timeout = TimeSpan.FromSeconds(30);
                    client.DefaultRequestHeaders.Authorization = token;
                    client.BaseAddress = new Uri(API_URL);
                    client.DefaultRequestHeaders.Accept.Clear();
                    client.DefaultRequestHeaders.Accept.Add(
                    new MediaTypeWithQualityHeaderValue("application/json"));

                    ServicePointManager.ServerCertificateValidationCallback += (sender, cert, chain, sslPolicyErrors) => true;
                    HttpResponseMessage response = await client.GetAsync($"api/ExposePerson/Get?customerCode={customerCode}");

                    if (response.IsSuccessStatusCode)
                    {
                        var jsonString = await response.Content.ReadAsStringAsync();
                        result = JsonConvert.DeserializeObject<string>(jsonString);
                    }
                    handler.Dispose();
                    client.Dispose();
                    return result;
                }
                catch (Exception ex)
                {
                    throw new APIErrorException("Core Banking API Error - " + ex.Message);
                }
            }

            public async Task<BVNCustomerDetailsViewModel> BVNCustomerDetails(string customerCode)
            {
                string result = string.Empty;
                var token = new AuthenticationHeaderValue("Authorization", API_KEY);
                handler.UseDefaultCredentials = true;
                HttpClient client = new HttpClient(handler);

                httpClientInstance = new HttpClient();
                httpClientInstance.DefaultRequestHeaders.ConnectionClose = false;
                client.Timeout = TimeSpan.FromSeconds(30);
                client.DefaultRequestHeaders.Authorization = token;
                client.BaseAddress = new Uri(API_URL);
                client.DefaultRequestHeaders.Accept.Clear();
                client.DefaultRequestHeaders.Accept.Add(
                new MediaTypeWithQualityHeaderValue("application/json"));

                ServicePointManager.ServerCertificateValidationCallback += (sender, cert, chain, sslPolicyErrors) => true;
                HttpResponseMessage response = await client.GetAsync($"api/OfficeAccount/GetGlAccountRecord?customerCode={customerCode}");
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
                handler.Dispose();
                client.Dispose();
                return data;
            }
        }
    }

}

