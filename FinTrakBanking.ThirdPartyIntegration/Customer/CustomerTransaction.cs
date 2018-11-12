using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using FintrakBanking.Entities.Models;
//using FintrakBanking.ViewModels.Customer;
using FintrakBanking.ViewModels.ThridPartyIntegration;


namespace FinTrakBanking.ThirdPartyIntegration.Customer
{
    public class CustomerTransaction// : ICustomerTransaction
    {
        private FinTrakBankingContext context;
        string API_KEY, API_URL = string.Empty;
        private HttpClientHandler handler = new HttpClientHandler();
        private static HttpClient httpClientInstance;

        public CustomerTransaction(FinTrakBankingContext _context)
        {
            context = _context;
            var configdata = context.TBL_SETUP_COMPANY.FirstOrDefault();
            API_KEY = configdata.APIKEY;
            API_URL = configdata.APIURL;
        }
               
        public async Task<List<CustomerTurnoverViewModels>> GetCustomerTransactions(string cifid, int month)
        {
            month = 48;
            cifid = "483008974";

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

            //var logs = new TBL_CUSTOM_API_LOGS
            //{
            //    APIURL = $"api/Customer/GetCustomerByAccountNumber?accountNumber={customerAccount}",
            //    LOGTYPEID = 4,
            //    REFERENCENUMBER = customerAccount,
            //    REQUESTDATETIME = requestDatetime,
            //    REQUESTMESSAGE = customerAccount,
            //    RESPONSEDATETIME = responseDateTime,
            //    RESPONSEMESSAGE = responseMessage,
            //};

            //FinTrakBankingContext logContext = new FinTrakBankingContext();
            //logContext.TBL_CUSTOM_API_LOGS.Add(logs);
            //logContext.SaveChanges();

            return result;
        }

        public async Task<List<CustomerTurnoverInterestViewModels>> GetCustomerInterestTransactions(InputVM body)
        {
            var endpointUrl = $"api/Customer/GetCustomerLoanInterestDetails";

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
            response = await client.PostAsJsonAsync(endpointUrl, body);
            //response.EnsureSuccessStatusCode();
            responseTime = DateTime.Now;


            List<CustomerTurnoverInterestViewModels> result = null;
            var responseMessage = await response.Content.ReadAsStringAsync();
            //if (response.IsSuccessStatusCode)
                result = await response.Content.ReadAsAsync<List<CustomerTurnoverInterestViewModels>>();

            handler.Dispose();
            client.Dispose();

            //var logs = new TBL_CUSTOM_API_LOGS
            //{
            //    APIURL = $"api/Customer/GetCustomerByAccountNumber?accountNumber={customerAccount}",
            //    LOGTYPEID = 4,
            //    REFERENCENUMBER = customerAccount,
            //    REQUESTDATETIME = requestDatetime,
            //    REQUESTMESSAGE = customerAccount,
            //    RESPONSEDATETIME = responseDateTime,
            //    RESPONSEMESSAGE = responseMessage,
            //};

            //FinTrakBankingContext logContext = new FinTrakBankingContext();
            //logContext.TBL_CUSTOM_API_LOGS.Add(logs);
            //logContext.SaveChanges();

            return result;
        }
    }


    public class ResponseMessageViewModel
    {
        public string webRequestStatus { get; set; }
        [DisplayFormat(DataFormatString = "{0:dd-MMM-yyyy}")]
        public DateTime webRequestDate { get; set; }
        public string responseCode { get; set; }
        public string serialNumber { get; set; }
        public string message { get; set; }
        public HttpResponseMessage APIMessage { get; set; }
        public bool responseStatus { get; set; }
    }

    public interface ICustomerTransaction
    {

    }
}
