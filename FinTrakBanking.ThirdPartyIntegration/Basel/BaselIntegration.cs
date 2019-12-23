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
            //API_KEY = "WzKQBRQXboWsIVI";
            API_KEY = "XtSREijsrZYkt9S";
            API_URL = "http://10.1.12.186:94/api/Credit360API/";
            //API_URL = "http://10.1.9.197:94/api/Credit360API/";

            //var configdata = context.TBL_SETUP_COMPANY.FirstOrDefault();
            //if (configdata != null)
            //{
            //    API_KEY = configdata.APIKEY;
            //    API_URL = configdata.APIURL;
            //}
        }
        // private static HttpClient _httpClientInstance;

        // private ResponseMessageViewModel responseAPI;

        public async Task<List<SubGroupRatingAndRatioViewModel>> GetCustomerRatio( string customerNumber)
        {
            HttpClientHandler handler = new HttpClientHandler();
            HttpClient httpClientInstance;

            HttpClient client = new HttpClient(handler);
            DateTime requestDatetime = new DateTime(), responseDateTime = new DateTime();
            HttpResponseMessage response = null;
            ResponseMessageViewModel res = null;
            string responseMessage = "";
            //string endPointUrl = $"{API_URL}GetCorporateRatioPDConsolidatedByCustomerID/{customerNumber}?key={API_KEY}";
            string endPointUrl = $"{API_URL}GetCorporateRatioPDConsolidatedByCustomerID/{"000077293"}?key={API_KEY}";

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
                    response = await client.GetAsync(endPointUrl);
                    responseDateTime = DateTime.Now;
                }
                catch (Exception e) { throw new ConditionNotMetException(e.Message); }

                responseMessage = await response.Content.ReadAsStringAsync();
                List<SubGroupRatingAndRatioViewModel> customerRatios = new List<SubGroupRatingAndRatioViewModel>();

                if (response.IsSuccessStatusCode)
                {
                    var result = await response.Content.ReadAsAsync<List<SubGroupRatingAndRatioViewModel>>();
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

        public async Task<List<MainGroupRatingAndRatioViewModel>> GetAllCustomerRatios(string customerNumber)
        {
            HttpClientHandler handler = new HttpClientHandler();
            HttpClient httpClientInstance;
            HttpClient client = new HttpClient(handler);
            DateTime requestDatetime = new DateTime(), responseDateTime = new DateTime();
            HttpResponseMessage response = null;
            ResponseMessageViewModel res = null;
            string responseMessage = "";
            //string endPointUrl = $"{API_URL}GetAllCorporateRatios/{customerNumber}?key={API_KEY}";
            string endPointUrl = $"{API_URL}GetAllCorporateRatios/{"000077293"}?key={API_KEY}";

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

                //try
                //{
                    response = await client.GetAsync(endPointUrl);
                    responseDateTime = DateTime.Now;
                //}
                //catch (Exception e) { throw new ConditionNotMetException(e.Message); }

                responseMessage = await response.Content.ReadAsStringAsync();
                List<MainGroupRatingAndRatioViewModel> customerGroupRatios = new List<MainGroupRatingAndRatioViewModel>();

                if (response.IsSuccessStatusCode)
                {
                    var result = await response.Content.ReadAsAsync<List<MainGroupRatingAndRatioViewModel>>();
                    var responseData = await response.Content.ReadAsStringAsync();
                    customerGroupRatios = result;
                }

                return customerGroupRatios;
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
            string endPointUrl = $"{API_URL}GetCorporatePDByCustomerID/{customerNumber}?key={API_KEY}";
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

                CutomerRatingViewModel customerRating = new CutomerRatingViewModel();
                if (response.IsSuccessStatusCode)
                {
                    var result = await response.Content.ReadAsAsync<CutomerRatingViewModel>();
                    var responseData = await response.Content.ReadAsStringAsync();
                    customerRating = result;
                }

                return customerRating;
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

        public async Task<FacilityRatingViewModel> GetPersonalLoansRetailByCustomerCode(string customerNumber)
        {

            //THIS METHOD's VIEW MODEL, CLASSESES ARE  YET TO BE CREATED
            HttpClientHandler handler = new HttpClientHandler();
            HttpClient httpClientInstance;

            HttpClient client = new HttpClient(handler);
            DateTime requestDatetime = new DateTime(), responseDateTime = new DateTime();
            HttpResponseMessage response = null;
            ResponseMessageViewModel res = null;
            string responseMessage = "";
            string endPointUrl = $"{API_URL}GetPersonalLoansRetailPDByCustomerID/{customerNumber}?key={API_KEY}";
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

                FacilityRatingViewModel personalLoan = new FacilityRatingViewModel();
                if (response.IsSuccessStatusCode)
                {
                    var result = await response.Content.ReadAsAsync<FacilityRatingViewModel>();
                    //var responseData = await response.Content.ReadAsStringAsync();
                    if (result != null) personalLoan = result;
                }

                return personalLoan;
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

        public async Task<FacilityRatingViewModel> GetCreditCardRetailProbabilityOfDefaultByCustomerCode(string customerNumber)
        {

            //THIS METHOD's VIEW MODEL, CLASSESES ARE  YET TO BE CREATED
            HttpClientHandler handler = new HttpClientHandler();
            HttpClient httpClientInstance;

            HttpClient client = new HttpClient(handler);
            DateTime requestDatetime = new DateTime(), responseDateTime = new DateTime();
            HttpResponseMessage response = null;
            ResponseMessageViewModel res = null;
            string responseMessage = "";
            string endPointUrl = $"{API_URL}GetcreditCardRetailPDByCustomerID/{customerNumber}?key={API_KEY}";
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

                FacilityRatingViewModel creditCard = new FacilityRatingViewModel();
                if (response.IsSuccessStatusCode)
                {
                    var result = await response.Content.ReadAsAsync<FacilityRatingViewModel>();
                    //var responseData = await response.Content.ReadAsStringAsync();
                    if (result != null) creditCard = result;
                }

                return creditCard;
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

        public async Task<FacilityRatingViewModel> GetAutoLoanProbabilityOfDefaultByCustomerCode(string customerNumber)
        {

            //THIS METHOD's VIEW MODEL, CLASSESES ARE  YET TO BE CREATED
            HttpClientHandler handler = new HttpClientHandler();
            HttpClient httpClientInstance;

            HttpClient client = new HttpClient(handler);
            DateTime requestDatetime = new DateTime(), responseDateTime = new DateTime();
            HttpResponseMessage response = null;
            ResponseMessageViewModel res = null;
            string responseMessage = "";
            string endPointUrl = $"{API_URL}GetAutoLoanRetailPDByCustomerID/{customerNumber}?key={API_KEY}";
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

                FacilityRatingViewModel autoLoan = new FacilityRatingViewModel();
                if (response.IsSuccessStatusCode)
                {
                    var result = await response.Content.ReadAsAsync<FacilityRatingViewModel>();
                    //var responseData = await response.Content.ReadAsStringAsync();
                    if (result != null) autoLoan = result;
                }

                return autoLoan;
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

