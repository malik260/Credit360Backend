namespace FinTrakBanking.ThirdPartyIntegration
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Net;
    using System.Net.Http;
    using System.Net.Http.Headers;
    using System.Text;
    using System.Threading.Tasks;
    using System.Web.Script.Serialization;
    using FintrakBanking.Entities.Models;
    using FintrakBanking.ViewModels.ThridPartyIntegration;

    namespace ForeignCurrencyAccountCreation
    {

        public class ForeignCurrencyAccount
        {
            private FinTrakBankingContext _context;
            string API_KEY, API_URL = string.Empty;
            private readonly HttpClientHandler _handler = new HttpClientHandler();
            private static HttpClient _httpClientInstance;


            public ForeignCurrencyAccount(FinTrakBankingContext context)
            {
                this._context = context;
                var configdata = context.TBL_SETUP_COMPANY.FirstOrDefault();
                if (configdata != null)
                {
                    API_KEY = configdata.APIKEY;
                    API_URL = configdata.APIURL;
                }
            }

            public async Task<AccountCreationRespones> CreateAccount(CreateAccountViewModel entity)
            {
               // HttpClient client = new HttpClient(handler);
                var objData = new JavaScriptSerializer().Serialize(entity);
                DateTime requestDatetime = new DateTime(), responseDateTime = new DateTime();
                HttpResponseMessage response = null;
                AccountCreationRespones responseMsg = null;
                string responseMessage = "";

                ResponseMessageViewModel responseModel = new ResponseMessageViewModel();
                var token = new AuthenticationHeaderValue("Authorization", API_KEY); ;
                _handler.UseDefaultCredentials = true;
                HttpClient client = new HttpClient(_handler);

                _httpClientInstance = new HttpClient();
                _httpClientInstance.DefaultRequestHeaders.ConnectionClose = false;
                client.Timeout = TimeSpan.FromSeconds(180);
                client.DefaultRequestHeaders.Authorization = token;
                client.BaseAddress = new Uri(API_URL);
                client.DefaultRequestHeaders.Accept.Clear();
                client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

                ServicePointManager.ServerCertificateValidationCallback += (sender, cert, chain, sslPolicyErrors) => true;
                requestDatetime = DateTime.Now;
                response = client.PostAsync("api/ForeignCurrencyAccount/CreateAccount", new StringContent(
                                                new JavaScriptSerializer().Serialize(entity), Encoding.UTF8, "application/json")).Result;
                responseDateTime = DateTime.Now;
                AccountCreationResponseMessageViewModel responseAPI = new AccountCreationResponseMessageViewModel();

                //responseMsg = null;
                bool result = false;
                result = response.IsSuccessStatusCode;
                if (result)
                {

                    responseAPI = await response.Content.ReadAsAsync<AccountCreationResponseMessageViewModel>();
                    var res = new AccountCreationResponseMessageViewModel
                    {
                        responseCode = responseAPI.responseCode,
                        webRequestDate = responseAPI.webRequestDate,
                        webRequestStatus = responseAPI.webRequestStatus,
                        serialNumber = responseAPI.serialNumber,
                        accountNumber = responseAPI.accountNumber,
                        customerName = responseAPI.customerName,
                        errorMessage = responseAPI.errorMessage,
                        referenceNumber = responseAPI.referenceNumber,
                        message = responseAPI.message
                    };

                    responseMsg = new AccountCreationRespones
                    {
                        APIResponse = res,
                        APIStatus = result,
                        Message = response,

                    };
                }
                else
                {
                    responseMsg = new AccountCreationRespones
                    {
                        APIResponse = null,
                        APIStatus = result,
                        Message = response
                    };
                }

                responseMessage = await response.Content.ReadAsStringAsync();
                _handler.Dispose();
                client.Dispose();

                var logs = new TBL_CUSTOM_API_LOGS
                {
                    APIURL = "api/ForeignCurrencyAccount/CreateAccount",
                    LOGTYPEID = 10,
                    REFERENCENUMBER = entity.customerCode,
                    REQUESTDATETIME = requestDatetime,
                    REQUESTMESSAGE = objData,
                    RESPONSEDATETIME = responseDateTime,
                    RESPONSEMESSAGE = responseMessage,
                };
                FinTrakBankingContext logContext = new FinTrakBankingContext();

                logContext.TBL_CUSTOM_API_LOGS.Add(logs);

                logContext.SaveChanges();
                return responseMsg;
            }


        }
    }

}





