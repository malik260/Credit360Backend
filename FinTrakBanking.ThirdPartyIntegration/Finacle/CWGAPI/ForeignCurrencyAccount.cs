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
                ResponseMessageViewModel responseModel = new ResponseMessageViewModel();
                var token = new AuthenticationHeaderValue("Authorization", API_KEY); ;
                _handler.UseDefaultCredentials = true;
                HttpClient client = new HttpClient(_handler);

                _httpClientInstance = new HttpClient();
                _httpClientInstance.DefaultRequestHeaders.ConnectionClose = false;
                client.Timeout = TimeSpan.FromSeconds(60);
                client.DefaultRequestHeaders.Authorization = token;
                client.BaseAddress = new Uri(API_URL);
                client.DefaultRequestHeaders.Accept.Clear();
                client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

                ServicePointManager.ServerCertificateValidationCallback += (sender, cert, chain, sslPolicyErrors) => true;
                HttpResponseMessage response = client.PostAsync("api/TemporaryOverDraft/Single", new StringContent(
                                                new JavaScriptSerializer().Serialize(entity), Encoding.UTF8, "application/json")).Result;

                AccountCreationResponseMessageViewModel responseAPI = new AccountCreationResponseMessageViewModel();

                AccountCreationRespones responseMsg = null;
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
                _handler.Dispose();
                client.Dispose();
                return responseMsg;
            }


        }
    }

}





