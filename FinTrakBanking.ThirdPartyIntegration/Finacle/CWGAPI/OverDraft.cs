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
    using FintrakBanking.Common.CustomException;

    namespace OverDraftTransactions
    {
        public class OverDraft
        {
            private FinTrakBankingContext _context;
            private string API_KEY, API_URL = string.Empty;

            public OverDraft(FinTrakBankingContext context)
            {
                _context = context;

                var configdata = context.TBL_SETUP_COMPANY.FirstOrDefault();
                if (configdata != null)
                {
                    API_KEY = configdata.APIKEY;
                    API_URL = configdata.APIURL;
                }
            }

            //FinTrakBankingContext logContext = new FinTrakBankingContext();
            //private HttpClientHandler _handler = new HttpClientHandler();
            private static HttpClient _httpClientInstance;

            private ResponseMessageViewModel responseAPI;
            //----------------------------------- OverDraft----------------------------------------

            public async Task<ResponseMessage> APIOverDraftNormal(OverDraftNormalViewModel model)
            {
                HttpClientHandler _handler = new HttpClientHandler();

                _handler.UseDefaultCredentials = true;
                HttpClient client = new HttpClient(_handler);

                DateTime requestDatetime = new DateTime(), responseDateTime = new DateTime();

                // HttpClient client = new HttpClient(_handler);
                var objData = new JavaScriptSerializer().Serialize(model);
                //DateTime requestDatetime = new DateTime(), responseDateTime = new DateTime();
                HttpResponseMessage response = null;
                ResponseMessage responseMsg = null;
                string responseMessage = "";

                try
                {
                    model.sanctionLevel = "003";
                    model.sanctionAuthorizer = "999";

                    var token = new AuthenticationHeaderValue("Authorization", API_KEY);

                    client = new HttpClient();
                    client.DefaultRequestHeaders.ConnectionClose = false;
                    client.Timeout = TimeSpan.FromSeconds(60);
                    client.DefaultRequestHeaders.Authorization = token;
                    client.BaseAddress = new Uri(API_URL);
                    client.DefaultRequestHeaders.Accept.Clear();
                    client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

                    ServicePointManager.ServerCertificateValidationCallback +=
                        (sender, cert, chain, sslPolicyErrors) => true;
                    requestDatetime = DateTime.Now;
                    response = client.PostAsync("api/OverDraft/Normal", new StringContent(
                        new JavaScriptSerializer().Serialize(model), Encoding.UTF8, "application/json")).Result;

                    responseDateTime = DateTime.Now;
                    responseMsg = null;

                    ResponseMessageViewModel responseAPI = new ResponseMessageViewModel();

                    if (response.IsSuccessStatusCode)
                    {
                        responseAPI = await response.Content.ReadAsAsync<ResponseMessageViewModel>();
                        var res = new ResponseMessageViewModel
                        {
                            responseCode = responseAPI.responseCode,
                            webRequestDate = responseAPI.webRequestDate,
                            webRequestStatus = responseAPI.webRequestStatus,
                            serialNumber = responseAPI.serialNumber,
                            message = responseAPI.message
                        };

                        responseMsg = new ResponseMessage
                        {
                            APIResponse = res,
                            APIStatus = response.IsSuccessStatusCode,
                            Message = response
                        };
                    }
                    else
                    {
                        responseMsg = new ResponseMessage
                        {
                            APIResponse = null,
                            APIStatus = response.IsSuccessStatusCode,
                            Message = response
                        }; 
                    }
                    responseMessage = await response.Content.ReadAsStringAsync();
                    _handler.Dispose();
                    client.Dispose();
                    return responseMsg;
                }
                catch (Exception ex)
                {
                    var innerExceptionMessage = "";
                    if (ex.InnerException != null)
                        innerExceptionMessage = ex.InnerException.Message;

                    throw new APIErrorException($"Core Banking API Error - {ex.Message} - inner exception - {innerExceptionMessage}");
                    //throw new APIErrorException("Could not establish connection to finacle. Please contact the system administrator.");
                }
                finally
                {
                    _handler.Dispose();
                    client.Dispose();

                    var logs = new TBL_CUSTOM_API_LOGS
                    {
                        APIURL = "api/OverDraft/Normal",
                        LOGTYPEID = 11,
                        REFERENCENUMBER = model.sanctionReferenceNumber,
                        REQUESTDATETIME = requestDatetime,
                        REQUESTMESSAGE = objData,
                        RESPONSEDATETIME = responseDateTime,
                        RESPONSEMESSAGE = responseMessage,
                    };


                    FinTrakBankingContext logContext = new FinTrakBankingContext();

                    logContext.TBL_CUSTOM_API_LOGS.Add(logs);

                    logContext.SaveChanges();
                   
                }
            }

            public async Task<ResponseMessage> APIOverDraftTopUp(OverDraftTopUpAndRenewViewModel model)
            {
                HttpClientHandler _handler = new HttpClientHandler();
                HttpClient client = new HttpClient(_handler);
                var objData = new JavaScriptSerializer().Serialize(model);
                DateTime requestDatetime = new DateTime(), responseDateTime = new DateTime();
                HttpResponseMessage response = null;
                ResponseMessage responseMsg = null;
                string responseMessage = "";

                model.sanctionLevel = "003";
                model.sanctionAuthorizer = "999";
                var token = new AuthenticationHeaderValue("Authorization", API_KEY);

                _handler.UseDefaultCredentials = true;


                _httpClientInstance = new HttpClient();
                _httpClientInstance.DefaultRequestHeaders.ConnectionClose = false;
                client.Timeout = TimeSpan.FromSeconds(60);
                client.DefaultRequestHeaders.Authorization = token;
                client.BaseAddress = new Uri(API_URL);
                client.DefaultRequestHeaders.Accept.Clear();
                client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));


                ServicePointManager.ServerCertificateValidationCallback +=
                    (sender, cert, chain, sslPolicyErrors) => true;
                requestDatetime = DateTime.Now;
                response = client.PostAsync("api/OverDraft/TopUp", new StringContent(
                    new JavaScriptSerializer().Serialize(model), Encoding.UTF8, "application/json")).Result;

                responseMsg = null;
                responseDateTime = DateTime.Now;
                bool result = response.IsSuccessStatusCode;
                if (result)
                {

                    responseAPI = await response.Content.ReadAsAsync<ResponseMessageViewModel>();
                    var res = new ResponseMessageViewModel
                    {

                        responseCode = responseAPI.responseCode,
                        webRequestDate = responseAPI.webRequestDate,
                        webRequestStatus = responseAPI.webRequestStatus,
                        serialNumber = responseAPI.serialNumber,
                        message = responseAPI.message
                    };

                    responseMsg = new ResponseMessage
                    {
                        APIResponse = res,
                        APIStatus = result,
                        Message = response
                    };
                }
                else
                {
                    responseMsg = new ResponseMessage
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
                    APIURL = "api/OverDraft/TopUp",
                    LOGTYPEID = 12,
                    REFERENCENUMBER = model.sanctionReferenceNumber,
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

            public async Task<ResponseMessage> APIOverDraftRenew(OverDraftTopUpAndRenewViewModel model)
            {
                HttpClientHandler _handler = new HttpClientHandler();
                HttpClient client = new HttpClient(_handler);
                var objData = new JavaScriptSerializer().Serialize(model);
                DateTime requestDatetime = new DateTime(), responseDateTime = new DateTime();
                HttpResponseMessage response = null;
                ResponseMessage responseMsg = null;
                string responseMessage = "";
                model.sanctionLevel = "003";
                model.sanctionAuthorizer = "999";

                var token = new AuthenticationHeaderValue("Authorization", API_KEY);

                _handler.UseDefaultCredentials = true;


                _httpClientInstance = new HttpClient();
                _httpClientInstance.DefaultRequestHeaders.ConnectionClose = false;
                client.Timeout = TimeSpan.FromSeconds(60);
                client.DefaultRequestHeaders.Authorization = token;
                client.BaseAddress = new Uri(API_URL);
                client.DefaultRequestHeaders.Accept.Clear();
                client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));


                ServicePointManager.ServerCertificateValidationCallback +=
                    (sender, cert, chain, sslPolicyErrors) => true;
                requestDatetime = DateTime.Now;
                response = client.PostAsync("api/OverDraft/Renew", new StringContent(
                    new JavaScriptSerializer().Serialize(model), Encoding.UTF8, "application/json")).Result;
                responseMsg = null;
                responseDateTime = DateTime.Now;
                bool result = response.IsSuccessStatusCode;
                if (result)
                {

                    responseAPI = await response.Content.ReadAsAsync<ResponseMessageViewModel>();
                    var res = new ResponseMessageViewModel
                    {
                        responseCode = responseAPI.responseCode,
                        webRequestDate = responseAPI.webRequestDate,
                        webRequestStatus = responseAPI.webRequestStatus,
                        serialNumber = responseAPI.serialNumber,
                        message = responseAPI.message
                    };

                    responseMsg = new ResponseMessage
                    {
                        APIResponse = res,
                        APIStatus = result,
                        Message = response
                    };
                }
                else
                {
                    responseMsg = new ResponseMessage
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
                    APIURL = "api/OverDraft/Renew",
                    LOGTYPEID = 13,
                    REFERENCENUMBER = model.sanctionReferenceNumber,
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

            public async Task<ResponseMessage> APIOverDraftExtend(OverDraftExtendViewModel model)
            {
                HttpClientHandler _handler = new HttpClientHandler();
                HttpClient client = new HttpClient(_handler);
                var objData = new JavaScriptSerializer().Serialize(model);
                DateTime requestDatetime = new DateTime(), responseDateTime = new DateTime();
                HttpResponseMessage response = null;
                ResponseMessage responseMsg = null;
                string responseMessage = "";
                model.sanctionLevel = "003";
                model.sanctionAuthorizer = "999";
                var token = new AuthenticationHeaderValue("Authorization", API_KEY);

                _handler.UseDefaultCredentials = true;
                //HttpClient client = new HttpClient(_handler);

                _httpClientInstance = new HttpClient();
                _httpClientInstance.DefaultRequestHeaders.ConnectionClose = false;
                client.Timeout = TimeSpan.FromSeconds(60);
                client.DefaultRequestHeaders.Authorization = token;
                client.BaseAddress = new Uri(API_URL);
                client.DefaultRequestHeaders.Accept.Clear();
                client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));


                ServicePointManager.ServerCertificateValidationCallback +=
                    (sender, cert, chain, sslPolicyErrors) => true;
                requestDatetime = DateTime.Now;
                response = client.PostAsync("api/OverDraft/Extend", new StringContent(
                    new JavaScriptSerializer().Serialize(model), Encoding.UTF8, "application/json")).Result;
                responseDateTime = DateTime.Now;
                ResponseMessageViewModel responseAPI = new ResponseMessageViewModel();
                responseMsg = null;

                if (response.IsSuccessStatusCode)
                {

                    responseAPI = await response.Content.ReadAsAsync<ResponseMessageViewModel>();
                    var res = new ResponseMessageViewModel
                    {
                        responseCode = responseAPI.responseCode,
                        webRequestDate = responseAPI.webRequestDate,
                        webRequestStatus = responseAPI.webRequestStatus,
                        serialNumber = responseAPI.serialNumber,
                        message = responseAPI.message
                    };

                    responseMsg = new ResponseMessage
                    {
                        APIResponse = res,
                        APIStatus = response.IsSuccessStatusCode,
                        Message = response
                    };
                }
                else
                {
                    responseMsg = new ResponseMessage
                    {
                        APIResponse = null,
                        APIStatus = response.IsSuccessStatusCode,
                        Message = response
                    };
                }

                responseMessage = await response.Content.ReadAsStringAsync();
                _handler.Dispose();
                client.Dispose();
                var logs = new TBL_CUSTOM_API_LOGS
                {
                    APIURL = "api/OverDraft/Extend",
                    LOGTYPEID = 14,
                    REFERENCENUMBER = model.sanctionReferenceNumber,
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


            ////////////////////////////////////////////////////////////////////////////////////////////////

            //----------------------------------- TemporaryOverDraft----------------------------------------
            public async Task<ResponseMessage> APITemporaryOverDraftNormal(TemporaryOverDraftViewModel model)
            {
                HttpClientHandler _handler = new HttpClientHandler();
                HttpClient client = new HttpClient(_handler);
                var objData = new JavaScriptSerializer().Serialize(model);
                DateTime requestDatetime = new DateTime(), responseDateTime = new DateTime();
                HttpResponseMessage response = null;
                ResponseMessage responseMsg = null;
                string responseMessage = "";
               
                var token = new AuthenticationHeaderValue("Authorization", API_KEY);

                _handler.UseDefaultCredentials = true;
                // HttpClient client = new HttpClient(_handler);

                _httpClientInstance = new HttpClient();
                _httpClientInstance.DefaultRequestHeaders.ConnectionClose = false;
                client.Timeout = TimeSpan.FromSeconds(60);
                client.DefaultRequestHeaders.Authorization = token;
                client.BaseAddress = new Uri(API_URL);
                client.DefaultRequestHeaders.Accept.Clear();
                client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));


                ServicePointManager.ServerCertificateValidationCallback +=
                    (sender, cert, chain, sslPolicyErrors) => true;
                requestDatetime = DateTime.Now;
                response = client.PostAsync("api/TemporaryOverDraft/TemporaryNormal", new StringContent(
                    new JavaScriptSerializer().Serialize(model), Encoding.UTF8, "application/json")).Result;
                responseDateTime = DateTime.Now;
                ResponseMessageViewModel responseAPI = new ResponseMessageViewModel();
                responseMsg = null;
                bool result = false;
                result = response.IsSuccessStatusCode;
                if (result)
                {

                    responseAPI = await response.Content.ReadAsAsync<ResponseMessageViewModel>();
                    var res = new ResponseMessageViewModel
                    {
                        responseCode = responseAPI.responseCode,
                        webRequestDate = responseAPI.webRequestDate,
                        webRequestStatus = responseAPI.webRequestStatus,
                        serialNumber = responseAPI.serialNumber,
                        message = responseAPI.message
                    };

                    responseMsg = new ResponseMessage
                    {
                        APIResponse = res,
                        APIStatus = result,
                        Message = response
                    };
                }
                else
                {
                    responseMsg = new ResponseMessage
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
                    APIURL = "api/TemporaryOverDraft/Normal",
                    LOGTYPEID = 15,
                    REFERENCENUMBER = model.TemporaryOverDraftNaration,
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

            public async Task<ResponseMessage> APITemporaryOverDraftRunning(TemporaryOverDraftViewModel model)
            {
                HttpClientHandler _handler = new HttpClientHandler();
                HttpClient client = new HttpClient(_handler);
                var objData = new JavaScriptSerializer().Serialize(model);
                DateTime requestDatetime = new DateTime(), responseDateTime = new DateTime();
                HttpResponseMessage response = null;
                ResponseMessage responseMsg = null;
                string responseMessage = "";

                var token = new AuthenticationHeaderValue("Authorization", API_KEY);

                _handler.UseDefaultCredentials = true;
                // HttpClient client = new HttpClient(_handler);

                _httpClientInstance = new HttpClient();
                _httpClientInstance.DefaultRequestHeaders.ConnectionClose = false;
                client.Timeout = TimeSpan.FromSeconds(60);
                client.DefaultRequestHeaders.Authorization = token;
                client.BaseAddress = new Uri(API_URL);
                client.DefaultRequestHeaders.Accept.Clear();
                client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));


                ServicePointManager.ServerCertificateValidationCallback +=
                    (sender, cert, chain, sslPolicyErrors) => true;
                requestDatetime = DateTime.Now;
                response = client.PostAsync("api/TemporaryOverDraft/Running", new StringContent(
                    new JavaScriptSerializer().Serialize(model), Encoding.UTF8, "application/json")).Result;
                responseDateTime = DateTime.Now;
                responseMsg = null;
                bool result = false;
                result = response.IsSuccessStatusCode;
                if (result)
                {

                    responseAPI = await response.Content.ReadAsAsync<ResponseMessageViewModel>();
                    var res = new ResponseMessageViewModel
                    {
                        responseCode = responseAPI.responseCode,
                        webRequestDate = responseAPI.webRequestDate,
                        webRequestStatus = responseAPI.webRequestStatus,
                        serialNumber = responseAPI.serialNumber,
                        message = responseAPI.message
                    };

                    responseMsg = new ResponseMessage
                    {
                        APIResponse = res,
                        APIStatus = result,
                        Message = response
                    };
                }
                else
                {
                    responseMsg = new ResponseMessage
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
                    APIURL = "api/TemporaryOverDraft/Running",
                    LOGTYPEID = 16,
                    REFERENCENUMBER = model.TemporaryOverDraftNaration,
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

            public async Task<ResponseMessage> APITemporaryOverDraftSingle(TemporaryOverDraftViewModel model)
            {
                HttpClientHandler _handler = new HttpClientHandler();
                HttpClient client = new HttpClient(_handler);
                var objData = new JavaScriptSerializer().Serialize(model);
                DateTime requestDatetime = new DateTime(), responseDateTime = new DateTime();
                HttpResponseMessage response = null;
                ResponseMessage responseMsg = null;
                string responseMessage = "";
                var token = new AuthenticationHeaderValue("Authorization", API_KEY);
                ;
                _handler.UseDefaultCredentials = true;
                //HttpClient client = new HttpClient(_handler);

                _httpClientInstance = new HttpClient();
                _httpClientInstance.DefaultRequestHeaders.ConnectionClose = false;
                client.Timeout = TimeSpan.FromSeconds(60);
                client.DefaultRequestHeaders.Authorization = token;
                client.BaseAddress = new Uri(API_URL);
                client.DefaultRequestHeaders.Accept.Clear();
                client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

                ServicePointManager.ServerCertificateValidationCallback +=
                    (sender, cert, chain, sslPolicyErrors) => true;
                requestDatetime = DateTime.Now;
                response = client.PostAsync("api/TemporaryOverDraft/Single", new StringContent(
                    new JavaScriptSerializer().Serialize(model), Encoding.UTF8, "application/json")).Result;
                responseDateTime = DateTime.Now;
                responseMsg = null;
                bool result = false;
                result = response.IsSuccessStatusCode;
                if (result)
                {

                    responseAPI = await response.Content.ReadAsAsync<ResponseMessageViewModel>();
                    var res = new ResponseMessageViewModel
                    {
                        responseCode = responseAPI.responseCode,
                        webRequestDate = responseAPI.webRequestDate,
                        webRequestStatus = responseAPI.webRequestStatus,
                        serialNumber = responseAPI.serialNumber,
                        message = responseAPI.message
                    };

                    responseMsg = new ResponseMessage
                    {
                        APIResponse = res,
                        APIStatus = result,
                        Message = response
                    };
                }
                else
                {
                    responseMsg = new ResponseMessage
                    {
                        APIResponse = null,
                        APIStatus = result,
                        Message = response
                    };
                }

                //_handler.Dispose();
                //client.Dispose();

                responseMessage = await response.Content.ReadAsStringAsync();
                _handler.Dispose();
                client.Dispose();
                var logs = new TBL_CUSTOM_API_LOGS
                {
                    APIURL = "api/TemporaryOverDraft/Single",
                    LOGTYPEID = 17,
                    REFERENCENUMBER = model.TemporaryOverDraftNaration,
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

            //public async Task<ResponseMessage> APIOverDraftTopUp(OverDraftTopUpAndRenewViewModel model)
            //{

            //    var token = new AuthenticationHeaderValue("Authorization", API_KEY);

            //    _handler.UseDefaultCredentials = true;
            //    HttpClient client = new HttpClient(_handler);

            //    _httpClientInstance = new HttpClient();
            //    _httpClientInstance.DefaultRequestHeaders.ConnectionClose = false;
            //    client.Timeout = TimeSpan.FromSeconds(60);
            //    client.DefaultRequestHeaders.Authorization = token;
            //    client.BaseAddress = new Uri(API_URL);
            //    client.DefaultRequestHeaders.Accept.Clear();
            //    client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));


            //    ServicePointManager.ServerCertificateValidationCallback +=
            //        (sender, cert, chain, sslPolicyErrors) => true;
            //    HttpResponseMessage response = client.PostAsync("api/OverDraft/TopUp", new StringContent(
            //        new JavaScriptSerializer().Serialize(model), Encoding.UTF8, "application/json")).Result;

            //    ResponseMessage responseMsg = null;

            //   bool result = response.IsSuccessStatusCode;
            //    if (result)
            //    {

            //        responseAPI = await response.Content.ReadAsAsync<ResponseMessageViewModel>();
            //        var res = new ResponseMessageViewModel
            //        {

            //            responseCode = responseAPI.responseCode,
            //            webRequestDate = responseAPI.webRequestDate,
            //            webRequestStatus = responseAPI.webRequestStatus,
            //            serialNumber = responseAPI.serialNumber,
            //            message = responseAPI.message
            //        };

            //        responseMsg = new ResponseMessage
            //        {
            //            APIResponse = res,
            //            APIStatus = result,
            //            Message = response
            //        };
            //    }
            //    else
            //    {
            //        responseMsg = new ResponseMessage
            //        {
            //            APIResponse = null,
            //            APIStatus = result,
            //            Message = response
            //        };
            //    }

            //    return responseMsg;

            //}

            //public async Task<ResponseMessage> APIOverDraftRenew(OverDraftTopUpAndRenewViewModel model)
            //{


            //    var token = new AuthenticationHeaderValue("Authorization", API_KEY);

            //    _handler.UseDefaultCredentials = true;
            //    HttpClient client = new HttpClient(_handler);

            //    _httpClientInstance = new HttpClient();
            //    _httpClientInstance.DefaultRequestHeaders.ConnectionClose = false;
            //    client.Timeout = TimeSpan.FromSeconds(60);
            //    client.DefaultRequestHeaders.Authorization = token;
            //    client.BaseAddress = new Uri(API_URL);
            //    client.DefaultRequestHeaders.Accept.Clear();
            //    client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));


            //    ServicePointManager.ServerCertificateValidationCallback +=
            //        (sender, cert, chain, sslPolicyErrors) => true;
            //    HttpResponseMessage response = client.PostAsync("api/OverDraft/Renew ", new StringContent(
            //        new JavaScriptSerializer().Serialize(model), Encoding.UTF8, "application/json")).Result;
            //    ResponseMessage responseMsg = null;
            //    bool result = response.IsSuccessStatusCode;
            //    if (result)
            //    {

            //        responseAPI = await response.Content.ReadAsAsync<ResponseMessageViewModel>();
            //        var res = new ResponseMessageViewModel
            //        {
            //            responseCode = responseAPI.responseCode,
            //            webRequestDate = responseAPI.webRequestDate,
            //            webRequestStatus = responseAPI.webRequestStatus,
            //            serialNumber = responseAPI.serialNumber,
            //            message = responseAPI.message
            //        };

            //        responseMsg = new ResponseMessage
            //        {
            //            APIResponse = res,
            //            APIStatus = result,
            //            Message = response
            //        };
            //    }
            //    else
            //    {
            //        responseMsg = new ResponseMessage
            //        {
            //            APIResponse = null,
            //            APIStatus = result,
            //            Message = response
            //        };
            //    }

            //    return responseMsg;

            //}

            //public async Task<ResponseMessage> APIOverDraftExtend(OverDraftExtendViewModel model)
            //{
            //    var token = new AuthenticationHeaderValue("Authorization", API_KEY);

            //    _handler.UseDefaultCredentials = true;
            //    HttpClient client = new HttpClient(_handler);

            //    _httpClientInstance = new HttpClient();
            //    _httpClientInstance.DefaultRequestHeaders.ConnectionClose = false;
            //    client.Timeout = TimeSpan.FromSeconds(60);
            //    client.DefaultRequestHeaders.Authorization = token;
            //    client.BaseAddress = new Uri(API_URL);
            //    client.DefaultRequestHeaders.Accept.Clear();
            //    client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));


            //    ServicePointManager.ServerCertificateValidationCallback +=
            //        (sender, cert, chain, sslPolicyErrors) => true;
            //    HttpResponseMessage response = client.PostAsync("api/OverDraft/Extend", new StringContent(
            //        new JavaScriptSerializer().Serialize(model), Encoding.UTF8, "application/json")).Result;

            //    ResponseMessageViewModel responseAPI = new ResponseMessageViewModel();
            //    ResponseMessage responseMsg = null;

            //    if (response.IsSuccessStatusCode)
            //    {

            //        responseAPI = await response.Content.ReadAsAsync<ResponseMessageViewModel>();
            //        var res = new ResponseMessageViewModel
            //        {
            //            responseCode = responseAPI.responseCode,
            //            webRequestDate = responseAPI.webRequestDate,
            //            webRequestStatus = responseAPI.webRequestStatus,
            //            serialNumber = responseAPI.serialNumber,
            //            message = responseAPI.message
            //        };

            //        responseMsg = new ResponseMessage
            //        {
            //            APIResponse = res,
            //            APIStatus = response.IsSuccessStatusCode,
            //            Message = response
            //        };
            //    }
            //    else
            //    {   
            //        responseMsg = new ResponseMessage
            //        {
            //            APIResponse = null,
            //            APIStatus = response.IsSuccessStatusCode,
            //            Message = response
            //        };
            //    }

            //    return responseMsg;

            //}


            ////////////////////////////////////////////////////////////////////////////////////////////////

            //----------------------------------- TemporaryOverDraft----------------------------------------
            //public async Task<ResponseMessage> APITemporaryOverDraftNormal(TemporaryOverDraftViewModel model)
            //{
            //    var token = new AuthenticationHeaderValue("Authorization", API_KEY);

            //    _handler.UseDefaultCredentials = true;
            //    HttpClient client = new HttpClient(_handler);

            //    _httpClientInstance = new HttpClient();
            //    _httpClientInstance.DefaultRequestHeaders.ConnectionClose = false;
            //    client.Timeout = TimeSpan.FromSeconds(60);
            //    client.DefaultRequestHeaders.Authorization = token;
            //    client.BaseAddress = new Uri(API_URL);
            //    client.DefaultRequestHeaders.Accept.Clear();
            //    client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));


            //    ServicePointManager.ServerCertificateValidationCallback +=
            //        (sender, cert, chain, sslPolicyErrors) => true;
            //    HttpResponseMessage response = client.PostAsync("api/TemporaryOverDraft/Normal", new StringContent(
            //        new JavaScriptSerializer().Serialize(model), Encoding.UTF8, "application/json")).Result;

            //    ResponseMessageViewModel responseAPI = new ResponseMessageViewModel();
            //    ResponseMessage responseMsg = null;
            //    bool result = false;
            //    result = response.IsSuccessStatusCode;
            //    if (result)
            //    {

            //        responseAPI = await response.Content.ReadAsAsync<ResponseMessageViewModel>();
            //        var res = new ResponseMessageViewModel
            //        {
            //            responseCode = responseAPI.responseCode,
            //            webRequestDate = responseAPI.webRequestDate,
            //            webRequestStatus = responseAPI.webRequestStatus,
            //            serialNumber = responseAPI.serialNumber,
            //            message = responseAPI.message
            //        };

            //        responseMsg = new ResponseMessage
            //        {
            //            APIResponse = res,
            //            APIStatus = result,
            //            Message = response
            //        };
            //    }
            //    else
            //    {
            //        responseMsg = new ResponseMessage
            //        {
            //            APIResponse = null,
            //            APIStatus = result,
            //            Message = response
            //        };
            //    }

            //    return responseMsg;


            //}

            //public async Task<ResponseMessage> APITemporaryOverDraftRunning(TemporaryOverDraftViewModel model)
            //{



            //    var token = new AuthenticationHeaderValue("Authorization", API_KEY);

            //    _handler.UseDefaultCredentials = true;
            //    HttpClient client = new HttpClient(_handler);

            //    _httpClientInstance = new HttpClient();
            //    _httpClientInstance.DefaultRequestHeaders.ConnectionClose = false;
            //    client.Timeout = TimeSpan.FromSeconds(60);
            //    client.DefaultRequestHeaders.Authorization = token;
            //    client.BaseAddress = new Uri(API_URL);
            //    client.DefaultRequestHeaders.Accept.Clear();
            //    client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));


            //    ServicePointManager.ServerCertificateValidationCallback +=
            //        (sender, cert, chain, sslPolicyErrors) => true;
            //    HttpResponseMessage response = client.PostAsync("api/TemporaryOverDraft/Running", new StringContent(
            //        new JavaScriptSerializer().Serialize(model), Encoding.UTF8, "application/json")).Result;
            //    ResponseMessage responseMsg = null;
            //    bool result = false;
            //    result = response.IsSuccessStatusCode;
            //    if (result)
            //    {

            //        responseAPI = await response.Content.ReadAsAsync<ResponseMessageViewModel>();
            //        var res = new ResponseMessageViewModel
            //        {
            //            responseCode = responseAPI.responseCode,
            //            webRequestDate = responseAPI.webRequestDate,
            //            webRequestStatus = responseAPI.webRequestStatus,
            //            serialNumber = responseAPI.serialNumber,
            //            message = responseAPI.message
            //        };

            //        responseMsg = new ResponseMessage
            //        {
            //            APIResponse = res,
            //            APIStatus = result,
            //            Message = response
            //        };
            //    }
            //    else
            //    {
            //        responseMsg = new ResponseMessage
            //        {
            //            APIResponse = null,
            //            APIStatus = result,
            //            Message = response
            //        };
            //    }

            //    return responseMsg;


            //}

            //public async Task<ResponseMessage> APITemporaryOverDraftSingle(TemporaryOverDraftViewModel model)
            //{


            //    var token = new AuthenticationHeaderValue("Authorization", API_KEY);
            //    ;
            //    _handler.UseDefaultCredentials = true;
            //    HttpClient client = new HttpClient(_handler);

            //    _httpClientInstance = new HttpClient();
            //    _httpClientInstance.DefaultRequestHeaders.ConnectionClose = false;
            //    client.Timeout = TimeSpan.FromSeconds(60);
            //    client.DefaultRequestHeaders.Authorization = token;
            //    client.BaseAddress = new Uri(API_URL);
            //    client.DefaultRequestHeaders.Accept.Clear();
            //    client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

            //    ServicePointManager.ServerCertificateValidationCallback +=
            //        (sender, cert, chain, sslPolicyErrors) => true;
            //    HttpResponseMessage response = client.PostAsync("api/TemporaryOverDraft/Single", new StringContent(
            //        new JavaScriptSerializer().Serialize(model), Encoding.UTF8, "application/json")).Result;
            //    ResponseMessage responseMsg = null;
            //    bool result = false;
            //    result = response.IsSuccessStatusCode;
            //    if (result)
            //    {

            //        responseAPI = await response.Content.ReadAsAsync<ResponseMessageViewModel>();
            //        var res = new ResponseMessageViewModel
            //        {
            //            responseCode = responseAPI.responseCode,
            //            webRequestDate = responseAPI.webRequestDate,
            //            webRequestStatus = responseAPI.webRequestStatus,
            //            serialNumber = responseAPI.serialNumber,
            //            message = responseAPI.message
            //        };

            //        responseMsg = new ResponseMessage
            //        {
            //            APIResponse = res,
            //            APIStatus = result,
            //            Message = response
            //        };
            //    }
            //    else
            //    {
            //        responseMsg = new ResponseMessage
            //        {
            //            APIResponse = null,
            //            APIStatus = result,
            //            Message = response
            //        };
            //    }

            //    _handler.Dispose();
            //    client.Dispose();
            //    return responseMsg;
            //}

        }
    }
}