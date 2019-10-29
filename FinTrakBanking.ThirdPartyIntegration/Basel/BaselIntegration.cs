using FintrakBanking.Common.CustomException;
using FintrakBanking.Entities.Models;
using FintrakBanking.ViewModels.ThridPartyIntegration;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using System.Web.Script.Serialization;

namespace FinTrakBanking.ThirdPartyIntegration.Basel
{
    public class BaselIntegration
    {
        private FinTrakBankingContext _context;
        private string API_KEY, API_URL = string.Empty;

        public BaselIntegration(FinTrakBankingContext context)
        {
            _context = context;
            API_KEY = "WzKQBRQXboWsIVI";
            API_URL = "http://10.1.9.197:94/api/Credit360API/";
            //var configdata = context.TBL_SETUP_COMPANY.FirstOrDefault();
            //if (configdata != null)
            //{
            //    API_KEY = configdata.APIKEY;
            //    API_URL = configdata.APIURL;
            //}
        }
       // private static HttpClient _httpClientInstance;

       // private ResponseMessageViewModel responseAPI;

        public async Task<List<RatingAndRatioViewModel>> GetCustomerRatio( string customerNumber)
        {
            HttpClientHandler handler = new HttpClientHandler();
            HttpClient httpClientInstance;

            HttpClient client = new HttpClient(handler);
            DateTime requestDatetime = new DateTime(), responseDateTime = new DateTime();
            HttpResponseMessage response = null;
            ResponseMessageViewModel res = null;
            string responseMessage = "";
            string endPointUrl = $"GetCorporateRatioPDConsolidatedByCustomerID/{customerNumber}?key={API_KEY}";
            try
            { 
                handler.UseDefaultCredentials = true;

                var token = new AuthenticationHeaderValue("Authorization", API_KEY);
                httpClientInstance = new HttpClient();
                httpClientInstance.DefaultRequestHeaders.ConnectionClose = false;
                client.Timeout = TimeSpan.FromSeconds(180);
                client.BaseAddress = new Uri(API_URL);
                client.DefaultRequestHeaders.Accept.Clear();
                client.DefaultRequestHeaders.Accept.Add(
                    new MediaTypeWithQualityHeaderValue("application/json"));

                ServicePointManager.ServerCertificateValidationCallback +=
                    (sender, cert, chain, sslPolicyErrors) => true;
                requestDatetime = DateTime.Now;

                try
                {
                    response = await client.GetAsync(endPointUrl
                    );
                    responseDateTime = DateTime.Now;
                }
                catch (Exception e) { throw new ConditionNotMetException(e.Message); }

                responseMessage = await response.Content.ReadAsStringAsync();

                List<RatingAndRatioViewModel> customerRatios = new List<RatingAndRatioViewModel>();
                if (response.IsSuccessStatusCode)
                {
                    var result = await response.Content.ReadAsAsync<List<RatingAndRatioViewModel>>();
                    
                    var responseData = await response.Content.ReadAsStringAsync();
                    //JObject responseDataJsonString = JObject.Parse(responseData);

                    //var data = responseDataJsonString["data"].ToString();
                    customerRatios = result;// JsonConvert.DeserializeObject<List<RatingAndRatioViewModel>>(data);

                }
              

                return customerRatios;
            }
            catch (APIErrorException ex)
            {
                throw new APIErrorException(ex.Message);
            }
            catch (Exception ex)
            {
                throw new APIErrorException($"Error" + ex.Message);
            }
            finally
            {
                handler.Dispose();
                client.Dispose();

                var logs = new TBL_CUSTOM_API_LOGS
                {
                    APIURL = endPointUrl,
                    LOGTYPEID = 3,
                    REFERENCENUMBER = customerNumber.ToString(),
                    REQUESTDATETIME = requestDatetime,
                    REQUESTMESSAGE = customerNumber,
                    RESPONSEDATETIME = responseDateTime,
                    RESPONSEMESSAGE = responseMessage,
                };

                FinTrakBankingContext logContext = new FinTrakBankingContext();

                logContext.TBL_CUSTOM_API_LOGS.Add(logs);

                logContext.SaveChanges();
            }
        }

        public async Task<CutomerRatingViewModel> GetCorporateCustomerRatingByCustomerCode(string customerNumber)
        {
            HttpClientHandler handler = new HttpClientHandler();
            HttpClient httpClientInstance;

            HttpClient client = new HttpClient(handler);
            DateTime requestDatetime = new DateTime(), responseDateTime = new DateTime();
            HttpResponseMessage response = null;
            ResponseMessageViewModel res = null;
            string responseMessage = "";
            string endPointUrl = $"GetCorporatePDByCustomerID/{customerNumber}?key={API_KEY}";
            try 
            {
                handler.UseDefaultCredentials = true;

                var token = new AuthenticationHeaderValue("Authorization", API_KEY);
                httpClientInstance = new HttpClient();
                httpClientInstance.DefaultRequestHeaders.ConnectionClose = false;
                client.Timeout = TimeSpan.FromSeconds(180);
                client.BaseAddress = new Uri(API_URL);
                client.DefaultRequestHeaders.Accept.Clear();
                client.DefaultRequestHeaders.Accept.Add(
                    new MediaTypeWithQualityHeaderValue("application/json"));

                ServicePointManager.ServerCertificateValidationCallback +=
                    (sender, cert, chain, sslPolicyErrors) => true;
                requestDatetime = DateTime.Now;

                    response = await client.GetAsync(endPointUrl);
                    responseDateTime = DateTime.Now;
                

                responseMessage = await response.Content.ReadAsStringAsync();

                CutomerRatingViewModel customerRatios = new CutomerRatingViewModel();
                if (response.IsSuccessStatusCode)
                {
                    //result = await response.Content.ReadAsAsync<List<CustomerTurnoverViewModelAPI>>();

                    var responseData = await response.Content.ReadAsStringAsync();
                    JObject responseDataJsonString = JObject.Parse(responseData);

                    var data = responseDataJsonString["data"].ToString();
                    customerRatios = JsonConvert.DeserializeObject<CutomerRatingViewModel>(data);
                }


                return customerRatios;
            }
            catch (APIErrorException ex)
            {
                throw new APIErrorException(ex.Message);
            }
            catch (Exception ex)
            {
                throw new APIErrorException($"Error" + ex.Message);
            }
            finally
            {
                handler.Dispose();
                client.Dispose();

                var logs = new TBL_CUSTOM_API_LOGS
                {
                    APIURL = endPointUrl,
                    LOGTYPEID = 3,
                    REFERENCENUMBER = customerNumber.ToString(),
                    REQUESTDATETIME = requestDatetime,
                    REQUESTMESSAGE = customerNumber,
                    RESPONSEDATETIME = responseDateTime,
                    RESPONSEMESSAGE = responseMessage,
                };

                FinTrakBankingContext logContext = new FinTrakBankingContext();

                logContext.TBL_CUSTOM_API_LOGS.Add(logs);

                logContext.SaveChanges();
            }
        }

        public async Task<List<FacilityRatingViewModel>> GetPersonalLoansRetailByCustomerCode(string customerNumber)
        {

            //THIS METHOD's VIEW MODEL, CLASSESES ARE  YET TO BE CREATED
            HttpClientHandler handler = new HttpClientHandler();
            HttpClient httpClientInstance;

            HttpClient client = new HttpClient(handler);
            DateTime requestDatetime = new DateTime(), responseDateTime = new DateTime();
            HttpResponseMessage response = null;
            ResponseMessageViewModel res = null;
            string responseMessage = "";
            string endPointUrl = $"api/Credit360API/GetPersonalLoansRetailPDByCustomerID/{customerNumber}?key={API_KEY}";
            try
            {
                handler.UseDefaultCredentials = true;

                var token = new AuthenticationHeaderValue("Authorization", API_KEY);
                httpClientInstance = new HttpClient();
                httpClientInstance.DefaultRequestHeaders.ConnectionClose = false;
                client.Timeout = TimeSpan.FromSeconds(180);
                client.BaseAddress = new Uri(API_URL);
                client.DefaultRequestHeaders.Accept.Clear();
                client.DefaultRequestHeaders.Accept.Add(
                    new MediaTypeWithQualityHeaderValue("application/json"));

                ServicePointManager.ServerCertificateValidationCallback +=
                    (sender, cert, chain, sslPolicyErrors) => true;
                requestDatetime = DateTime.Now;

                response = await client.GetAsync(endPointUrl);
                responseDateTime = DateTime.Now;


                responseMessage = await response.Content.ReadAsStringAsync();

                List<FacilityRatingViewModel> personalLoans = new List<FacilityRatingViewModel>();
                if (response.IsSuccessStatusCode)
                {
                    //result = await response.Content.ReadAsAsync<List<CustomerTurnoverViewModelAPI>>();
                    var responseData = await response.Content.ReadAsStringAsync();
                    JObject responseDataJsonString = JObject.Parse(responseData);

                    var data = responseDataJsonString["data"].ToString();
                    personalLoans = JsonConvert.DeserializeObject<List<FacilityRatingViewModel>>(data);
                }

                return personalLoans;
            }
            catch (APIErrorException ex)
            {
                throw new APIErrorException(ex.Message);
            }
            catch (Exception ex)
            {
                throw new APIErrorException($"Error" + ex.Message);
            }
            finally
            {
                handler.Dispose();
                client.Dispose();

                var logs = new TBL_CUSTOM_API_LOGS
                {
                    APIURL = endPointUrl,
                    LOGTYPEID = 3,
                    REFERENCENUMBER = customerNumber.ToString(),
                    REQUESTDATETIME = requestDatetime,
                    REQUESTMESSAGE = customerNumber,
                    RESPONSEDATETIME = responseDateTime,
                    RESPONSEMESSAGE = responseMessage,
                };

                FinTrakBankingContext logContext = new FinTrakBankingContext();

                logContext.TBL_CUSTOM_API_LOGS.Add(logs);

                logContext.SaveChanges();
            }
        }

        public async Task<List<FacilityRatingViewModel>> GetCreditCardRetailProbabilityOfDefaultByCustomerCode(string customerNumber)
        {

            //THIS METHOD's VIEW MODEL, CLASSESES ARE  YET TO BE CREATED
            HttpClientHandler handler = new HttpClientHandler();
            HttpClient httpClientInstance;

            HttpClient client = new HttpClient(handler);
            DateTime requestDatetime = new DateTime(), responseDateTime = new DateTime();
            HttpResponseMessage response = null;
            ResponseMessageViewModel res = null;
            string responseMessage = "";
            string endPointUrl = $"api/Credit360API/GetcreditCardRetailPDByCustomerID/{customerNumber}?key={API_KEY}";
            try
            {
                handler.UseDefaultCredentials = true;

                var token = new AuthenticationHeaderValue("Authorization", API_KEY);
                httpClientInstance = new HttpClient();
                httpClientInstance.DefaultRequestHeaders.ConnectionClose = false;
                client.Timeout = TimeSpan.FromSeconds(180);
                client.BaseAddress = new Uri(API_URL);
                client.DefaultRequestHeaders.Accept.Clear();
                client.DefaultRequestHeaders.Accept.Add(
                    new MediaTypeWithQualityHeaderValue("application/json"));

                ServicePointManager.ServerCertificateValidationCallback +=
                    (sender, cert, chain, sslPolicyErrors) => true;
                requestDatetime = DateTime.Now;

                response = await client.GetAsync(endPointUrl);
                responseDateTime = DateTime.Now;


                responseMessage = await response.Content.ReadAsStringAsync();

                List<FacilityRatingViewModel> creditCards = new List<FacilityRatingViewModel>();
                if (response.IsSuccessStatusCode)
                {
                    //result = await response.Content.ReadAsAsync<List<CustomerTurnoverViewModelAPI>>();
                    var responseData = await response.Content.ReadAsStringAsync();
                    JObject responseDataJsonString = JObject.Parse(responseData);

                    var data = responseDataJsonString["data"].ToString();
                    creditCards = JsonConvert.DeserializeObject<List<FacilityRatingViewModel>>(data);
                }

                return creditCards;
            }
            catch (APIErrorException ex)
            {
                throw new APIErrorException(ex.Message);
            }
            catch (Exception ex)
            {
                throw new APIErrorException($"Error" + ex.Message);
            }
            finally
            {
                handler.Dispose();
                client.Dispose();

                var logs = new TBL_CUSTOM_API_LOGS
                {
                    APIURL = endPointUrl,
                    LOGTYPEID = 3,
                    REFERENCENUMBER = customerNumber.ToString(),
                    REQUESTDATETIME = requestDatetime,
                    REQUESTMESSAGE = customerNumber,
                    RESPONSEDATETIME = responseDateTime,
                    RESPONSEMESSAGE = responseMessage,
                };

                FinTrakBankingContext logContext = new FinTrakBankingContext();

                logContext.TBL_CUSTOM_API_LOGS.Add(logs);

                logContext.SaveChanges();
            }
        }

        public async Task<List<FacilityRatingViewModel>> GetAutoLoanProbabilityOfDefaultByCustomerCode(string customerNumber)
        {

            //THIS METHOD's VIEW MODEL, CLASSESES ARE  YET TO BE CREATED
            HttpClientHandler handler = new HttpClientHandler();
            HttpClient httpClientInstance;

            HttpClient client = new HttpClient(handler);
            DateTime requestDatetime = new DateTime(), responseDateTime = new DateTime();
            HttpResponseMessage response = null;
            ResponseMessageViewModel res = null;
            string responseMessage = "";
            string endPointUrl = $"api/Credit360API/GetAutoLoanRetailPDByCustomerID/{customerNumber}?key={API_KEY}";
            try
            {
                handler.UseDefaultCredentials = true;

                var token = new AuthenticationHeaderValue("Authorization", API_KEY);
                httpClientInstance = new HttpClient();
                httpClientInstance.DefaultRequestHeaders.ConnectionClose = false;
                client.Timeout = TimeSpan.FromSeconds(180);
                client.BaseAddress = new Uri(API_URL);
                client.DefaultRequestHeaders.Accept.Clear();
                client.DefaultRequestHeaders.Accept.Add(
                    new MediaTypeWithQualityHeaderValue("application/json"));

                ServicePointManager.ServerCertificateValidationCallback +=
                    (sender, cert, chain, sslPolicyErrors) => true;
                requestDatetime = DateTime.Now;

                response = await client.GetAsync(endPointUrl);
                responseDateTime = DateTime.Now;


                responseMessage = await response.Content.ReadAsStringAsync();

                List<FacilityRatingViewModel> autoLoans = new List<FacilityRatingViewModel>();
                if (response.IsSuccessStatusCode)
                {
                    //result = await response.Content.ReadAsAsync<List<CustomerTurnoverViewModelAPI>>();
                    var responseData = await response.Content.ReadAsStringAsync();
                    JObject responseDataJsonString = JObject.Parse(responseData);

                    var data = responseDataJsonString["data"].ToString();
                    autoLoans = JsonConvert.DeserializeObject<List<FacilityRatingViewModel>>(data);
                }

                return autoLoans;
            }
            catch (APIErrorException ex)
            {
                throw new APIErrorException(ex.Message);
            }
            catch (Exception ex)
            {
                throw new APIErrorException($"Error" + ex.Message);
            }
            finally
            {
                handler.Dispose();
                client.Dispose();

                var logs = new TBL_CUSTOM_API_LOGS
                {
                    APIURL = endPointUrl,
                    LOGTYPEID = 3,
                    REFERENCENUMBER = customerNumber.ToString(),
                    REQUESTDATETIME = requestDatetime,
                    REQUESTMESSAGE = customerNumber,
                    RESPONSEDATETIME = responseDateTime,
                    RESPONSEMESSAGE = responseMessage,
                };

                FinTrakBankingContext logContext = new FinTrakBankingContext();
                logContext.TBL_CUSTOM_API_LOGS.Add(logs);
                logContext.SaveChanges();
            }
        }

    }

}

