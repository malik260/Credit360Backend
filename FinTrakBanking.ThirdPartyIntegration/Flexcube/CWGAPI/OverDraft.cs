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
    using FintrakBanking.ViewModels.Finance;
    using FintrakBanking.ViewModels.Flexcube;

    namespace OverDraftTransactions
    {
        public class OverDraft
        {
            private FinTrakBankingContext _context;
            private string API_KEY, API_URL = string.Empty;
            private List<TBL_API_URL> APIUrlConfig;

            public OverDraft(FinTrakBankingContext context)
            {
                _context = context;

                APIUrlConfig = new List<TBL_API_URL>();
            }

            private void getAPIURLSettings(string typeName = null)
            {
                APIUrlConfig = _context.TBL_API_URL.ToList();
                var apiConfig = APIUrlConfig.Where(x => x.TYPENAME.ToLower() == typeName.ToLower()).FirstOrDefault();
                if (apiConfig != null)
                {
                    API_URL = apiConfig.URL.Trim();
                    API_KEY = apiConfig.APIKEY;
                }
                if (apiConfig == null)
                {
                    apiConfig = APIUrlConfig.Where(x => x.TYPENAME.ToUpper() == "DEFAULT").FirstOrDefault();
                    API_URL = apiConfig.URL.Trim();
                    API_KEY = apiConfig.APIKEY;
                }
            }

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
                getAPIURLSettings("OverDraft");

                try
                {
                    model.sanctionLevel = "003";
                    model.sanctionAuthorizer = "999";

                    var token = new AuthenticationHeaderValue("Authorization", API_KEY);

                    client = new HttpClient();
                    client.DefaultRequestHeaders.ConnectionClose = false;
                    client.Timeout = TimeSpan.FromSeconds(180);
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

            public async Task<ResponseMessage> FlexcubeAPIOverDraft(FlexcubeCreateOverdraftViewModel model, short loanSystemTypeId)
            {
                HttpClientHandler _handler = new HttpClientHandler();

                _handler.UseDefaultCredentials = true;
                HttpClient client = new HttpClient(_handler);
                DateTime requestDatetime = new DateTime(), responseDateTime = new DateTime();
                // HttpClient client = new HttpClient(_handler);
                var inputJson = new JavaScriptSerializer().Serialize(model);
                ResponseMessageOverDraftViewModel responseAPI = new ResponseMessageOverDraftViewModel();
                //DateTime requestDatetime = new DateTime(), responseDateTime = new DateTime();
                HttpResponseMessage response = null;
                ResponseMessage responseMsg = null;
                string responseJson = "";
                getAPIURLSettings("OverdraftWithoutLien");
                string apiUrl = "FCUBSCreateOverdraftWithoutLien";

                try
                {

                    var token = new AuthenticationHeaderValue("Authorization", API_KEY);

                    client = new HttpClient();
                    client.DefaultRequestHeaders.ConnectionClose = false;
                    client.Timeout = TimeSpan.FromSeconds(180);
                    client.DefaultRequestHeaders.Authorization = token;
                    client.BaseAddress = new Uri(API_URL);
                    client.DefaultRequestHeaders.Accept.Clear();
                    client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

                    ServicePointManager.ServerCertificateValidationCallback +=
                        (sender, cert, chain, sslPolicyErrors) => true;
                    requestDatetime = DateTime.Now;

                    //model.account_no = "0704490004";
                    response = client.PostAsync(apiUrl, new StringContent(
                                                new JavaScriptSerializer().Serialize(model), Encoding.UTF8, "application/json")).Result;
                    responseJson = await response.Content.ReadAsStringAsync();

                    responseDateTime = DateTime.Now;
                    responseMsg = null;

                    if (response.IsSuccessStatusCode)
                    {
                        responseAPI = await response.Content.ReadAsAsync<ResponseMessageOverDraftViewModel>();

                        var res = new ResponseMessageOverDraftViewModel
                        {
                            response_code = responseAPI.response_code,
                            //lien_id = responseAPI.lien_id,
                            collateral_id = responseAPI.collateral_id,
                            response_message = responseAPI.response_message,
                            bo_code = responseAPI.bo_code,
                            bo_message = responseAPI.bo_message
                        };

                        var specificRes = new ResponseMessageViewModel
                        {
                             message = res.response_message,
                             responseCode = res.response_code,
                             responseStatus = res.response_code == "00" ? true : false,
                             APIMessage = response,
                             webRequestDate = responseAPI.webRequestDate,
                             webRequestStatus = responseAPI.webRequestStatus,
                        };

                        responseMsg = new ResponseMessage
                        {
                            APIResponse = specificRes,
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

                    var loanMapping = new TBL_THIRDPARTY_LOAN_MAPPING
                    {
                        LOANAPPLICATIONID = model.loanApplicationId,
                        LOANSYSTEMTYPEID = loanSystemTypeId,
                        FACILITYMAPPINGID = responseAPI.collateral_id,
                        BOOKINGCODE = responseAPI.bo_code,
                    };

                   
                    var logs = new TBL_CUSTOM_API_LOGS
                    {
                        APIURL = API_URL + apiUrl,
                        LOGTYPEID = 11,
                       // REFERENCENUMBER = model.sanctionReferenceNumber,
                        REQUESTDATETIME = requestDatetime,
                        REQUESTMESSAGE = inputJson,
                        RESPONSEDATETIME = responseDateTime,
                        RESPONSEMESSAGE = responseJson,
                    };

                    FinTrakBankingContext logContext = new FinTrakBankingContext();

                    logContext.TBL_THIRDPARTY_LOAN_MAPPING.Add(loanMapping);
                    logContext.TBL_CUSTOM_API_LOGS.Add(logs);
                    logContext.SaveChanges();
                }
            }


            public async Task<ResponseMessage> FlexcubeCasaLien(FlexcubeLienViewModel model)
            {
                HttpClientHandler _handler = new HttpClientHandler();

                _handler.UseDefaultCredentials = true;
                HttpClient client = new HttpClient(_handler);
                DateTime requestDatetime = new DateTime(), responseDateTime = new DateTime();
                var inputJson = new JavaScriptSerializer().Serialize(model);
                ResponseMessageOverDraftViewModel responseAPI = new ResponseMessageOverDraftViewModel();

                HttpResponseMessage response = null;
                ResponseMessage responseMsg = null;
                string responseJson = "";
                getAPIURLSettings("Lien");
                string apiUrl = "FCUBSCreateLien";

                try
                {
                    var token = new AuthenticationHeaderValue("Authorization", API_KEY);

                    client = new HttpClient();
                    client.DefaultRequestHeaders.ConnectionClose = false;
                    client.Timeout = TimeSpan.FromSeconds(180);
                    client.DefaultRequestHeaders.Authorization = token;
                    client.BaseAddress = new Uri(API_URL);
                    client.DefaultRequestHeaders.Accept.Clear();
                    client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

                    ServicePointManager.ServerCertificateValidationCallback +=
                        (sender, cert, chain, sslPolicyErrors) => true;
                    requestDatetime = DateTime.Now;

                    response = client.PostAsync(apiUrl, new StringContent(
                                        new JavaScriptSerializer().Serialize(model), Encoding.UTF8, "application/json")).Result;
                    responseJson = await response.Content.ReadAsStringAsync();

                    responseDateTime = DateTime.Now;
                    responseMsg = null;

                    if (response.IsSuccessStatusCode)
                    {
                        responseAPI = await response.Content.ReadAsAsync<ResponseMessageOverDraftViewModel>();

                        var res = new ResponseMessageOverDraftViewModel
                        {
                            response_code = responseAPI.response_code,
                            lien_id = responseAPI.lien_id,
                            collateral_id = responseAPI.collateral_id,
                            response_message = responseAPI.response_message,
                            bo_code = responseAPI.bo_code,
                            bo_message = responseAPI.bo_message
                        };

                        var specificRes = new ResponseMessageViewModel
                        {
                            message = res.response_message,
                            responseCode = res.response_code,
                            responseStatus = res.response_code == "00" ? true : false,
                            APIMessage = response,
                            webRequestDate = responseAPI.webRequestDate,
                            webRequestStatus = responseAPI.webRequestStatus,
                        };

                        responseMsg = new ResponseMessage
                        {
                            APIResponse = specificRes,
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

                    var loanMapping = new TBL_THIRDPARTY_LOAN_MAPPING
                    {
                        LOANAPPLICATIONID = model.loanApplicationId,
                        LOANSYSTEMTYPEID = 2, //loanSystemTypeId,
                        FACILITYMAPPINGID = responseAPI.lien_id,
                        BOOKINGCODE = responseAPI.bo_code,
                    };

                    var logs = new TBL_CUSTOM_API_LOGS
                    {
                        APIURL = API_URL+ apiUrl,
                        LOGTYPEID = 11,
                        // REFERENCENUMBER = model.sanctionReferenceNumber,
                        REQUESTDATETIME = requestDatetime,
                        REQUESTMESSAGE = inputJson,
                        RESPONSEDATETIME = responseDateTime,
                        RESPONSEMESSAGE = responseJson,
                    };


                    FinTrakBankingContext logContext = new FinTrakBankingContext();
                    logContext.TBL_THIRDPARTY_LOAN_MAPPING.Add(loanMapping);
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
                getAPIURLSettings("OverDraftTopUp");
                model.sanctionLevel = "003";
                model.sanctionAuthorizer = "999";
                var token = new AuthenticationHeaderValue("Authorization", API_KEY);

                _handler.UseDefaultCredentials = true;


                _httpClientInstance = new HttpClient();
                _httpClientInstance.DefaultRequestHeaders.ConnectionClose = false;
                client.Timeout = TimeSpan.FromSeconds(180);
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
                model.sanctionLevel = "003";
                model.sanctionAuthorizer = "999";

                HttpClientHandler _handler = new HttpClientHandler();
                HttpClient client = new HttpClient(_handler);
                var objData = new JavaScriptSerializer().Serialize(model);
                DateTime requestDatetime = new DateTime(), responseDateTime = new DateTime();
                HttpResponseMessage response = null;
                ResponseMessage responseMsg = null;
                string responseMessage = "";

                getAPIURLSettings("OverDraftRenew");
                var token = new AuthenticationHeaderValue("Authorization", API_KEY);

                _handler.UseDefaultCredentials = true;


                _httpClientInstance = new HttpClient();
                _httpClientInstance.DefaultRequestHeaders.ConnectionClose = false;
                client.Timeout = TimeSpan.FromSeconds(180);
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
                getAPIURLSettings("OverDraftExtend");
                var token = new AuthenticationHeaderValue("Authorization", API_KEY);

                _handler.UseDefaultCredentials = true;
                //HttpClient client = new HttpClient(_handler);

                _httpClientInstance = new HttpClient();
                _httpClientInstance.DefaultRequestHeaders.ConnectionClose = false;
                client.Timeout = TimeSpan.FromSeconds(180);
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
                getAPIURLSettings("TempOverDraft");
                var token = new AuthenticationHeaderValue("Authorization", API_KEY);

                _handler.UseDefaultCredentials = true;
                // HttpClient client = new HttpClient(_handler);

                _httpClientInstance = new HttpClient();
                _httpClientInstance.DefaultRequestHeaders.ConnectionClose = false;
                client.Timeout = TimeSpan.FromSeconds(180);
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
                    REFERENCENUMBER = model.sourceReferenceNumber,
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
                getAPIURLSettings("TempOverDraftRunning");
                var token = new AuthenticationHeaderValue("Authorization", API_KEY);

                _handler.UseDefaultCredentials = true;
                // HttpClient client = new HttpClient(_handler);

                _httpClientInstance = new HttpClient();
                _httpClientInstance.DefaultRequestHeaders.ConnectionClose = false;
                client.Timeout = TimeSpan.FromSeconds(180);
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
                    REFERENCENUMBER = model.sourceReferenceNumber,
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
                getAPIURLSettings("TempOverDraftSingle");
                var token = new AuthenticationHeaderValue("Authorization", API_KEY);

                _handler.UseDefaultCredentials = true;
                //HttpClient client = new HttpClient(_handler);

                _httpClientInstance = new HttpClient();
                _httpClientInstance.DefaultRequestHeaders.ConnectionClose = false;
                client.Timeout = TimeSpan.FromSeconds(180);
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
                    REFERENCENUMBER = model.sourceReferenceNumber,
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

            public async Task<ResponseMessage> APIOverDraftInterestRate(InterestRateInquiryViewModel model, string accountType)
            {
                HttpClientHandler handler = new HttpClientHandler();
                HttpClient httpClientInstance;
                InterestRateInquiryViewModel responseModel = new InterestRateInquiryViewModel();
               // bool output = false;
                HttpClient client = new HttpClient(handler);
                var objData = new JavaScriptSerializer().Serialize(model);
                DateTime requestDatetime = new DateTime(), responseDateTime = new DateTime();
                //TransactionPostingViewModel responseApi = new TransactionPostingViewModel();
                HttpResponseMessage response = null;
                ResponseMessage responseMsg = null;
                string responseMessage = "";
                string responseJson = "";
                getAPIURLSettings("OverDraftInterestRate");
                try
                {
                    InterestRateDetails apiModel = new InterestRateDetails
                    {
                        accountNumber = model.accountNumber,
                        accountType = accountType,
                        interestTableCode = model.interestTableCode,
                        startDate = model.startDate,
                        endDate = model.endDate,
                        interestRateAmount = model.interestRateAmount,

                    };

                    var token = new AuthenticationHeaderValue("Authorization", API_KEY);

                    handler.UseDefaultCredentials = true;

                    httpClientInstance = new HttpClient();
                    httpClientInstance.DefaultRequestHeaders.ConnectionClose = false;
                    client.Timeout = TimeSpan.FromSeconds(180);
                    client.BaseAddress = new Uri(API_URL);
                    client.DefaultRequestHeaders.Accept.Clear();

                    client.DefaultRequestHeaders.Authorization = token;


                    ServicePointManager.ServerCertificateValidationCallback +=
                        (sender, cert, chain, sslPolicyErrors) => true;
                    requestDatetime = DateTime.Now;
                    response = client.PostAsync("api/InterestRateInquiry/PostInterestRate", new StringContent(
                        new JavaScriptSerializer().Serialize(apiModel), Encoding.UTF8, "application/json")).Result;
                    responseDateTime = DateTime.Now;

                    if (response.IsSuccessStatusCode)
                    {

                        responseModel = await response.Content.ReadAsAsync<InterestRateInquiryViewModel>();

                        var res = new ResponseMessageViewModel
                        {
                            responseCode = responseModel.responseCode,
                            webRequestDate = responseModel.webRequestDate,
                            webRequestStatus = responseModel.webRequestStatus,

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
                    responseJson = await response.Content.ReadAsStringAsync();
                    responseMsg.responseMessage = responseJson;

                    return responseMsg;
                }
                catch (Exception ex)
                {
                    var innerExceptionMessage = "";
                    if (ex.InnerException != null)
                        innerExceptionMessage = ex.InnerException.Message;

                    throw new APIErrorException($"Core Banking API Error - {ex.Message} - inner exception - {innerExceptionMessage}");
                }
                finally
                {
                    handler.Dispose();
                    client.Dispose();

                    var logs = new TBL_CUSTOM_API_LOGS
                    {
                        APIURL = "api/InterestRateInquiry/PostInterestRate",
                        LOGTYPEID = 19,
                        REFERENCENUMBER = model.accountNumber,
                        REQUESTDATETIME = requestDatetime,
                        REQUESTMESSAGE = objData,
                        RESPONSEDATETIME = responseDateTime,
                        RESPONSEMESSAGE = responseModel.webRequestStatus + ". " + responseJson,
                    };
                    FinTrakBankingContext logContext = new FinTrakBankingContext();

                    logContext.TBL_CUSTOM_API_LOGS.Add(logs);

                    logContext.SaveChanges();
                }


            }

            //public async Task<ResponseMessage> APIOverDraftTopUp(OverDraftTopUpAndRenewViewModel model)
            //{

            //    var token = new AuthenticationHeaderValue("Authorization", API_KEY);

            //    _handler.UseDefaultCredentials = true;
            //    HttpClient client = new HttpClient(_handler);

            //    _httpClientInstance = new HttpClient();
            //    _httpClientInstance.DefaultRequestHeaders.ConnectionClose = false;
            //    client.Timeout = TimeSpan.FromSeconds(180);
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
            //    client.Timeout = TimeSpan.FromSeconds(180);
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
            //    client.Timeout = TimeSpan.FromSeconds(180);
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
            //    client.Timeout = TimeSpan.FromSeconds(180);
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
            //    client.Timeout = TimeSpan.FromSeconds(180);
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
            //    client.Timeout = TimeSpan.FromSeconds(180);
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