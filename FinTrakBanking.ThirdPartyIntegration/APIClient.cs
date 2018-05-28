using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using FintrakBanking.ViewModels.Customer;
using FintrakBanking.ViewModels.ThridPartyIntegration;

namespace FinTrakBanking.ThirdPartyIntegration
{
    public class APIClient 
    {
        private HttpClientHandler handler = new HttpClientHandler();
        private static HttpClient _httpClientInstance;

        public string API_KEY { get; private set; }
        public string API_URL { get; private set; }

        public async Task< HttpResponseMessage> DynamiceClient(T value)
        {
            handler.UseDefaultCredentials = true;
            HttpClient client = new HttpClient(handler);
            var token = new AuthenticationHeaderValue("Authorization", API_KEY);
            _httpClientInstance = new HttpClient();
            _httpClientInstance.DefaultRequestHeaders.ConnectionClose = false;
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
                return response;
            }

            return response;
        }
    }
}
