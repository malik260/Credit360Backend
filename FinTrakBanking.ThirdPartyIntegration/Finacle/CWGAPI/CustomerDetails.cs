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

namespace FinTrakBanking.ThirdPartyIntegration.Finacle.CWGAPI
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

        //public class WeatherResponseModel
        //{
        //    public string accountNumber { get; set; }
        //    public string customerCode { get; set; }
        //    public string branchCode { get; set; }
        //    public string title { get; set; }
        //}
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

        //public async Task<List<CustomerIntegrationViewModels>> GetAllCustomers(string customerAccount)
        //{
        //    //client.BaseAddress = new Uri("https://172.16.249.195/FbnFintrak.Api.Test/");
        //    //client.DefaultRequestHeaders.Accept.Clear();
        //    //client.DefaultRequestHeaders.Accept.Add(
        //    //new MediaTypeWithQualityHeaderValue("application/json"));

        //    handler.UseDefaultCredentials = true;
        //    HttpClient client = new HttpClient(handler);

        //    CustomerIntegrationViewModels customerViewModels = new CustomerIntegrationViewModels();
        //    ServicePointManager.ServerCertificateValidationCallback += (sender, cert, chain, sslPolicyErrors) => true;
        //    HttpResponseMessage response = await client.GetAsync("api/Customer/MockDataGetAllCustomers");

        //    //this.CheckDisposedOrStarted();
        //    //client.Dispose();
        //    List<CustomerIntegrationViewModels> customers = new List<CustomerIntegrationViewModels>();

        //    if (response.IsSuccessStatusCode)
        //    {
        //        var jsonString = await response.Content.ReadAsStringAsync();
        //        var objData = JsonConvert.DeserializeObject<List<WeatherResponseModel>>(jsonString);


        //        foreach (var d in objData)
        //        {
        //            //customerViewModels.c = d.accountNumber;
        //            customerViewModels.branchCode = d.branchCode;
        //            customerViewModels.customerCode = d.customerCode;
        //            customerViewModels.title = d.title;

        //            customers.Add(customerViewModels);
        //        }
        //    }
        //    return customers;
        //}

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
            //ServicePointManager.FindServicePoint(client.BaseAddress)
            //.ConnectionLeaseTimeout = (int)TimeSpan.FromMinutes(1).TotalMilliseconds;
            //ServicePointManager.DnsRefreshTimeout = (int)TimeSpan.FromMinutes(1).TotalMilliseconds;
            ServicePointManager.FindServicePoint(client.BaseAddress).ConnectionLeaseTimeout = 60 * 1000;
            HttpResponseMessage response = await client.GetAsync($"api/Customer/GetCustomerByAccountNumber?accountNumber={customerAccount}");

            if (response.IsSuccessStatusCode)
            {
                //var jsonString = await response.Content.ReadAsStringAsync();
                //var objData = JsonConvert.DeserializeObject<List<CustomerIntegrationViewModels>>(jsonString);
                customerViewModels = await response.Content.ReadAsAsync<CustomerIntegrationViewModels>();

                customers.Add(new CustomerViewModels
                {
                    customerCode = customerViewModels.customerCode,
                    firstName = customerViewModels.lastName,
                    lastName = customerViewModels.firstName,
                    middleName = customerViewModels.middleName,
                    customerTypeName = customerViewModels.customerType,
                    customerTypeId = (short)(customerViewModels.customerType == "CORPORATE" ? 2 : 1),
                });

            }
            handler.Dispose();
            client.Dispose();

            return customers;


        }

        public bool AddCustomerAccounts(string customerCode)
        {
            var customerId = this.context.TBL_CUSTOMER.FirstOrDefault(a => a.CUSTOMERCODE == customerCode).CUSTOMERID;
            bool output = false;
            var data = new List<CasaViewModel>();
            List<TBL_CASA> customerAcct = new List<TBL_CASA>();

            Task.Run(async () => { data = await GetCustomerAccountsBalanceByCustomerCode(customerCode); }).GetAwaiter().GetResult();

            foreach (var item in data)
            {
                var currencyId = context.TBL_CURRENCY.FirstOrDefault(x => x.CURRENCYCODE == item.currency).CURRENCYID;
                var accountStatusId = context.TBL_CASA_ACCOUNTSTATUS.FirstOrDefault(x => x.ACCOUNTSTATUSNAME == item.accountStatusName).ACCOUNTSTATUSID;
                TBL_CASA addCustomerAcct = new TBL_CASA();
                addCustomerAcct.CUSTOMERID = customerId;
                addCustomerAcct.AVAILABLEBALANCE = item.availableBalance;
                addCustomerAcct.LEDGERBALANCE = item.ledgerBalance;
                addCustomerAcct.PRODUCTACCOUNTNAME = item.productName;//item.productAccountName;
                addCustomerAcct.PRODUCTACCOUNTNUMBER = item.productAccountNumber;
                addCustomerAcct.PRODUCTID = (short)(item.productCode != "" ? 8 : 8);
                addCustomerAcct.COMPANYID = 1;
                addCustomerAcct.BRANCHID = (short)(item.branchCode != "" ? 1 : 1);
                addCustomerAcct.CURRENCYID = currencyId;//(short)(item.currency == "NGN" ? 1 : 0);
                addCustomerAcct.ISCURRENTACCOUNT = true;
                addCustomerAcct.ACCOUNTSTATUSID = accountStatusId;//(short)(item.accountStatusName == "Active" ? 1 : 3);
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
                //context.SaveChangesAsync();
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


        //public async Task<CasaIntegrationViewModel> GetCustomerAccountBalance(string customerAccount)
        //{
        //    handler.UseDefaultCredentials = true;
        //    HttpClient client = new HttpClient(handler);

        //    httpClientInstance = new HttpClient();
        //    httpClientInstance.DefaultRequestHeaders.ConnectionClose = false;
        //    client.Timeout = TimeSpan.FromSeconds(30);
        //    client.BaseAddress = new Uri("https://172.16.249.195/FbnFintrak.Api.Test/");
        //    client.DefaultRequestHeaders.Accept.Clear();
        //    client.DefaultRequestHeaders.Accept.Add(
        //    new MediaTypeWithQualityHeaderValue("application/json"));

        //    CasaIntegrationViewModel customerViewModels = new CasaIntegrationViewModel();
        //    ServicePointManager.ServerCertificateValidationCallback += (sender, cert, chain, sslPolicyErrors) => true;
        //    HttpResponseMessage response = await client.GetAsync($"api/Customer/GetCustomerAccountNumber?accountNumber={customerAccount}");
        //    if (response.IsSuccessStatusCode)
        //    {
        //        customerViewModels = await response.Content.ReadAsAsync<CasaIntegrationViewModel>();
        //    }
        //    return customerViewModels;
        //    handler.Dispose();
        //    client.Dispose();
        //}


        public async Task<CasaBalanceViewModel> GetCustomerAccountBalance(string customerAccount)
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
            HttpResponseMessage response = await client.GetAsync($"api/Customer/GetCustomerAccountNumber?accountNumber={customerAccount}");
            if (response.IsSuccessStatusCode)
            {
                accountAPI = await response.Content.ReadAsAsync<CasaIntegrationViewModel>();
            }
            var currencyId = context.TBL_CURRENCY.FirstOrDefault(x => x.CURRENCYCODE == accountAPI.currencyType).CURRENCYID;
            var accountStatusId = context.TBL_CASA_ACCOUNTSTATUS.FirstOrDefault(x => x.ACCOUNTSTATUSNAME == accountAPI.accountStatus).ACCOUNTSTATUSID;

            accountOutput.accountName = accountAPI.accountName;
            accountOutput.accountNo = accountAPI.accountNumber;
            accountOutput.availableBalance = accountAPI.balance;
            accountOutput.productName = accountAPI.productName;
            accountOutput.currencyId = currencyId;
            accountOutput.accountStatusId = (CASAAccountStatusEnum)accountStatusId;

            handler.Dispose();
            client.Dispose();

            return accountOutput;


        }

        public async Task<List<CasaViewModel>> GetCustomerAccountsBalanceByCustomerCode(string customerCode)
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
                        effectiveDate = d.lastTransactionDate,
                        availableBalance = d.balance,
                        ledgerBalance = d.balance,


                    });
                }

            }

            handler.Dispose();
            client.Dispose();

            return casa;
        }
        
        public async Task<string> CheckExposePerson(string customerCode)
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
              dynamic  dataObj = JsonConvert.DeserializeObject<string>(jsonString);

             foreach(var d in dataObj)
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
