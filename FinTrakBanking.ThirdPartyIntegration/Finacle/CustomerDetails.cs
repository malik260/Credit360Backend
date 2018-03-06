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

namespace FinTrakBanking.ThirdPartyIntegration.Finacle
{
    public class CustomerDetails
    {

        public class WeatherResponseModel
        {
            public string accountNumber { get; set; }
            public string customerCode { get; set; }
            public string branchCode { get; set; }
            public string title { get; set; }
        }
        private HttpClientHandler handler = new HttpClientHandler();
        //private static  HttpClient client = new HttpClient(handler);
        private static HttpClient httpClientInstance;
      
       
        //httpClientInstance client = new HttpClient();
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

        public async Task<List<CustomerIntegrationViewModels>> GetAllCustomers(string customerAccount)
        {
            //client.BaseAddress = new Uri("https://172.16.249.195/FbnFintrak.Api.Test/");
            //client.DefaultRequestHeaders.Accept.Clear();
            //client.DefaultRequestHeaders.Accept.Add(
            //new MediaTypeWithQualityHeaderValue("application/json"));

            handler.UseDefaultCredentials = true;
            HttpClient client = new HttpClient(handler);

            CustomerIntegrationViewModels customerViewModels = new CustomerIntegrationViewModels();
            ServicePointManager.ServerCertificateValidationCallback += (sender, cert, chain, sslPolicyErrors) => true;
            HttpResponseMessage response = await client.GetAsync("api/Customer/MockDataGetAllCustomers");

            //this.CheckDisposedOrStarted();
            //client.Dispose();
            List<CustomerIntegrationViewModels> customers = new List<CustomerIntegrationViewModels>();

            if (response.IsSuccessStatusCode)
            {
                var jsonString = await response.Content.ReadAsStringAsync();
                var objData = JsonConvert.DeserializeObject<List<WeatherResponseModel>>(jsonString);

                
                foreach (var d in objData)
                {
                    //customerViewModels.c = d.accountNumber;
                    customerViewModels.branchCode = d.branchCode;
                    customerViewModels.customerCode = d.customerCode;
                    customerViewModels.title = d.title;

                    customers.Add(customerViewModels);
                }
            }
            return customers;
            //handler.Dispose(true);
            //client.Dispose(true);
        }

        //CustomerInformationStagingViewModels
        public async Task <List<CustomerViewModels>> GetCustomerByAccountNumber(string customerAccount)
        {

            handler.UseDefaultCredentials = true;
            HttpClient client = new HttpClient(handler);

            httpClientInstance = new HttpClient();
            httpClientInstance.DefaultRequestHeaders.ConnectionClose = false;
            client.Timeout = TimeSpan.FromSeconds(30);
            client.BaseAddress = new Uri("https://172.16.249.195/FbnFintrak.Api.Test/");
            client.DefaultRequestHeaders.Accept.Clear();
            client.DefaultRequestHeaders.Accept.Add(
            new MediaTypeWithQualityHeaderValue("application/json"));

            

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

            return customers;

            handler.Dispose();
            client.Dispose();
            //handler.Dispose();
        }



        //public async Task<CustomerIntegrationViewModels> GetCustomerByAccountNumber2(string customerAccount)
        //{
        //    CustomerIntegrationViewModels customerViewModels = new CustomerIntegrationViewModels();
        //    ServicePointManager.ServerCertificateValidationCallback += (sender, cert, chain, sslPolicyErrors) => true;
        //    HttpResponseMessage response = await client.GetAsync($"api/Customer/GetCustomerByAccountNumber?accountNumber={customerAccount}");
        //    if (response.IsSuccessStatusCode)
        //    {
        //        customerViewModels = await response.Content.ReadAsAsync<CustomerIntegrationViewModels>();
        //    }
        //    return customerViewModels;
        //}

        public async Task<List<CasaViewModel>> GetCustomerAccountsBalance(string customerCode)
        {

            handler.UseDefaultCredentials = true;
            HttpClient client = new HttpClient(handler);

            httpClientInstance = new HttpClient();
            httpClientInstance.DefaultRequestHeaders.ConnectionClose = false;
            client.Timeout = TimeSpan.FromSeconds(30);
            client.BaseAddress = new Uri("https://172.16.249.195/FbnFintrak.Api.Test/");
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
                        ledgerBalance  = d.balance,


                    });
                }

            }
            return casa;

            handler.Dispose();
            client.Dispose();
        }

    }
}
