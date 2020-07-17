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
    using FintrakBanking.ViewModels.Admin;

    namespace StaffInfo
    {
        public class StaffDetails
        {
            private FinTrakBankingContext context;
            string API_KEY, API_URL = string.Empty;

            //private static HttpClient httpClientInstance;
            //private HttpClientHandler handler = new HttpClientHandler();

            public StaffDetails(FinTrakBankingContext _context)
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
 
            public async Task<Users> GetStaffRoleByStaffCode(string staffCode)
            {
                HttpClientHandler handler = new HttpClientHandler();
                HttpClient httpClientInstance;

                handler.UseDefaultCredentials = true;
                DateTime requestDatetime = new DateTime(), responseDateTime = new DateTime();
                HttpResponseMessage response = null;
                //ResponseMessageViewModel res = null;
                string responseMessage = "";
                HttpClient client = new HttpClient(handler);
                var token = new AuthenticationHeaderValue("Authorization", API_KEY);
                httpClientInstance = new HttpClient();
                httpClientInstance.DefaultRequestHeaders.ConnectionClose = false;
                client.Timeout = TimeSpan.FromSeconds(180);
                client.BaseAddress = new Uri(API_URL);
                client.DefaultRequestHeaders.Authorization = token;
                client.DefaultRequestHeaders.Accept.Clear();
                client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));



                Users usersViewModels = new Users();
                Users users = new Users();
                ServicePointManager.ServerCertificateValidationCallback += (sender, cert, chain, sslPolicyErrors) => true;

                requestDatetime = DateTime.Now;
                //ServicePointManager.FindServicePoint(client.BaseAddress).ConnectionLeaseTimeout = 60 * 1000;
                response = await client.GetAsync($"api/Staff/GetStaffRoleById?staffId={staffCode}");
                responseDateTime = DateTime.Now;
                if (response.IsSuccessStatusCode)
                {
                    usersViewModels = await response.Content.ReadAsAsync<Users>();


                    users.staffRole = usersViewModels.roleCode;


                }
                responseMessage = await response.Content.ReadAsStringAsync();
                handler.Dispose();
                client.Dispose();

                var logs = new TBL_CUSTOM_API_LOGS
                {
                    APIURL = $"api/Staff/GetStaffRoleById?staffId={staffCode}",
                    LOGTYPEID = 4,
                    REFERENCENUMBER = staffCode,
                    REQUESTDATETIME = requestDatetime,
                    REQUESTMESSAGE = staffCode,
                    RESPONSEDATETIME = responseDateTime,
                    RESPONSEMESSAGE = responseMessage,
                };
                FinTrakBankingContext logContext = new FinTrakBankingContext();

                logContext.TBL_CUSTOM_API_LOGS.Add(logs);

                logContext.SaveChanges();

                return users;


            } 

         
          
          

        }
    }

}

