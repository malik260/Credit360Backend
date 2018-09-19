namespace FinTrakBanking.ThirdPartyIntegration
{
    using FintrakBanking.Common.CustomException;
    using FintrakBanking.Entities.Models;
    using FintrakBanking.ViewModels.CASA;
    using FintrakBanking.ViewModels.Credit;
    using FintrakBanking.ViewModels.Finance;
    using FintrakBanking.ViewModels.ThridPartyIntegration;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Net;
    using System.Net.Http;
    using System.Net.Http.Headers;
    using System.Text;
    using System.Threading.Tasks;
    using System.Web.Script.Serialization;

    namespace Finacle 
    {
        public class TransactionPosting
        {

            private FinTrakBankingContext context;
            string API_KEY, API_URL = string.Empty;

            public TransactionPosting(FinTrakBankingContext _context)
            {
                this.context = _context;
                var configdata = context.TBL_SETUP_COMPANY.FirstOrDefault();
                API_KEY = configdata.APIKEY;
                API_URL = configdata.APIURL;
            }

            public async Task<CurrencyExchangeRateViewModel> GetExchangeRate(string fromCurrencyCode, string toCurrencyCode, string rateCode)
            {
                HttpClientHandler handler = new HttpClientHandler();
                HttpClient httpClientInstance;

                HttpClient client = new HttpClient(handler);
                DateTime requestDatetime = new DateTime(), responseDateTime = new DateTime();
                HttpResponseMessage response = null;
                ResponseMessageViewModel res = null;
                string responseMessage = "";
                try
                {
                    handler.UseDefaultCredentials = true;
                  
                    var token = new AuthenticationHeaderValue("Authorization", API_KEY);
                    httpClientInstance = new HttpClient();
                    httpClientInstance.DefaultRequestHeaders.ConnectionClose = false;
                    client.Timeout = TimeSpan.FromSeconds(60);
                    client.BaseAddress = new Uri(API_URL);
                    client.DefaultRequestHeaders.Accept.Clear();
                    client.DefaultRequestHeaders.Accept.Add(
                        new MediaTypeWithQualityHeaderValue("application/json"));

                    CurrencyExchangeRateViewModel exchangeRateOutput = new CurrencyExchangeRateViewModel();
                    CurrencyExchangeRateIntegrationViewModel exchangeRateAPI =
                        new CurrencyExchangeRateIntegrationViewModel();
                    ServicePointManager.ServerCertificateValidationCallback +=
                        (sender, cert, chain, sslPolicyErrors) => true;
                    requestDatetime = DateTime.Now;
                    //HttpResponseMessage response = await client.GetAsync($"api/ExchangeRate/GetExchangeRateProduct?rateProduct.fromCurrencyCode={fromCurrencyCode}&rateProduct.toCurrencyCode={toCurrencyCode}&rateProduct.rateCode={rateCode}");
                    response = await client.GetAsync(
                        $"api/ExchangeRate/GetExchangeRateProduct?model.fromCurrencyCode={fromCurrencyCode}&model.toCurrencyCode={toCurrencyCode}&model.rateCode={rateCode}");
                    responseDateTime = DateTime.Now;
                    if (response.IsSuccessStatusCode)
                    {
                        exchangeRateAPI = await response.Content.ReadAsAsync<CurrencyExchangeRateIntegrationViewModel>();

                        if (exchangeRateAPI.webRequestStatus != "SUCCESS")
                        {
                            throw new APIErrorException("Core Banking API error - "+exchangeRateAPI.webRequestStatus + " " + exchangeRateAPI.webRequestDate);
                        }

                        var currencyId = context.TBL_CURRENCY.Where(x => x.CURRENCYCODE == exchangeRateAPI.currencyCode).Select(x=>x.CURRENCYID).FirstOrDefault();
                        exchangeRateOutput.sellingRate = exchangeRateAPI.exchangeRate;
                        exchangeRateOutput.buyingRate = exchangeRateAPI.exchangeRate;
                        exchangeRateOutput.currencyId = (short)currencyId;
                        exchangeRateOutput.date = exchangeRateAPI.webRequestDate;
                        exchangeRateOutput.webRequestStatus = exchangeRateAPI.webRequestStatus;

                    }

                    responseMessage = await response.Content.ReadAsStringAsync();

                    //handler.Dispose();
                    //client.Dispose();

                    return exchangeRateOutput;
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
                        APIURL = $"api/ExchangeRate/GetExchangeRateProduct?model.fromCurrencyCode={fromCurrencyCode}&model.toCurrencyCode={toCurrencyCode}&model.rateCode={rateCode}",
                        LOGTYPEID = 3,
                        REFERENCENUMBER = fromCurrencyCode + "--" +   toCurrencyCode + "--" + rateCode,
                        REQUESTDATETIME = requestDatetime,
                        REQUESTMESSAGE = fromCurrencyCode + "--" + toCurrencyCode + "--" + rateCode,
                        RESPONSEDATETIME = responseDateTime,
                        RESPONSEMESSAGE = responseMessage,
                    };

                    FinTrakBankingContext logContext = new FinTrakBankingContext();

                    logContext.TBL_CUSTOM_API_LOGS.Add(logs);

                    logContext.SaveChanges();
                }
            }         
            private bool AddCustomLien(LienProcessViewModel entity)
            {
                bool output = false;
                //foreach (var item in entity)
                //{
                var data = new TBL_CUSTOM_LIEN_PROCESS();
                {
                    data.ACCOUNTID = entity.account;
                    data.AMOUNT = entity.lienAmount;
                    data.CURRENCYCODE = entity.lienAccountCurrency;
                    data.CONSUMED = false;
                    data.DATETIMECONSUMED = null;
                    data.DATETIMECREATED = DateTime.Now;
                    data.LIENTYPE = entity.lienProcessType;
                    data.REASONCODE = entity.lienReasonCode;
                    data.LIENREFERENCENUMBER = entity.lienUniqueReferenceNumber;
                    data.DESCRIPTION = entity.lienReason;
                }
                ;
                context.TBL_CUSTOM_LIEN_PROCESS.Add(data);
                //};

                context.SaveChanges();
                output = true;
                return output;

            }



            //public async Task<bool> APITransactionPosting(List<FinanceTransactionViewModel> model)
            //{

            //    var token = new AuthenticationHeaderValue("Authorization", API_KEY);
            //    bool output = false;
            //    var dta = context.TBL_SETUP_GLOBAL.ToList();
            //    TransactionPostingViewModel responseModel = new TransactionPostingViewModel();
            //    List<TransactionPostingViewModel> apiModel = new List<TransactionPostingViewModel>();
            //    foreach (var item in model)
            //    {


            //        apiModel.Add(new TransactionPostingViewModel
            //            {

            //                accounts =
            //                    item.casaAccountId
            //                        .ToString(), //item.casaAccountId!= null ? context.TBL_CASA.FirstOrDefault(x => x.CASAACCOUNTID == item.casaAccountId).PRODUCTACCOUNTNUMBER : context.TBL_CHART_OF_ACCOUNT.FirstOrDefault(x => x.GLACCOUNTID == item.glAccountId).ACCOUNTCODE,                       
            //                amounts = item.creditAmount > 0
            //                    ? "C" + item.creditAmount.ToString()
            //                    : "D" + item.debitAmount.ToString(),
            //                //amounts = item.sourceReferenceNumber,
            //                narration = item.description,
            //                referenceNumber = item.batchCode,
            //                currencyType = context.TBL_CURRENCY.FirstOrDefault(x => x.CURRENCYID == item.currencyId)
            //                    .CURRENCYCODE,
            //                operationId =
            //                    item.operationId, // != null ? context.TBL_CASA.FirstOrDefault(x => x.CASAACCOUNTID == item.operationId).PRODUCTACCOUNTNUMBER : context.TBL_CHART_OF_ACCOUNT.FirstOrDefault(x => x.GLACCOUNTID == item.glAccountId).ACCOUNTCODE,
            //            }
            //        );
            //    }

            //    handler.UseDefaultCredentials = true;
            //    HttpClient client = new HttpClient(handler);

            //    httpClientInstance = new HttpClient();
            //    httpClientInstance.DefaultRequestHeaders.ConnectionClose = false;
            //    client.Timeout = TimeSpan.FromSeconds(60);
            //    client.DefaultRequestHeaders.Authorization = token;
            //    client.BaseAddress = new Uri(API_URL);
            //    client.DefaultRequestHeaders.Accept.Clear();
            //    client.DefaultRequestHeaders.Accept.Add(
            //        new MediaTypeWithQualityHeaderValue("application/json"));

            //    ServicePointManager.ServerCertificateValidationCallback +=
            //        (sender, cert, chain, sslPolicyErrors) => true;
            //    HttpResponseMessage response = client.PostAsync("api/Transactions/PostTransactions", new StringContent(
            //        new JavaScriptSerializer().Serialize(apiModel), Encoding.UTF8, "application/json")).Result;

            //    if (response.IsSuccessStatusCode)
            //    {
            //        responseModel = await response.Content.ReadAsAsync<TransactionPostingViewModel>();

            //    }

            //    ResponseViewModel responseAPI = new ResponseViewModel();
            //    responseAPI.responseCode = responseModel.responseCode;
            //    responseAPI.webRequestDate = responseModel.webRequestDate;
            //    responseAPI.webRequestStatus = responseModel.webRequestStatus;

            //    handler.Dispose();
            //    client.Dispose();
            //    if (responseModel.responseCode == "0")
            //    {
            //        AddCustomTransactions(apiModel);
            //        output = true;
            //    }
            //    else
            //    {
            //        output = false;
            //        throw new SecureException($"Transaction {responseAPI.webRequestStatus}");
            //    }

            //    return output;


            //}

            public async Task<ResponseMessage> ApiPostCrossCurrencyTransactions(List<TransactionPostingViewModel> model)
            {
                HttpClientHandler handler = new HttpClientHandler();
                HttpClient httpClientInstance;

                HttpClient client = new HttpClient(handler);
                var objData = new JavaScriptSerializer().Serialize(model);
                DateTime requestDatetime = new DateTime(), responseDateTime = new DateTime();
                HttpResponseMessage response = null;
                ResponseMessageViewModel responseApi = new ResponseMessageViewModel();
                ResponseMessage responseMsg = null;
                string responseMessage = "";
                try
                {
                    var token = new AuthenticationHeaderValue("Authorization", API_KEY);

                    var dta = context.TBL_SETUP_GLOBAL.ToList();

                    handler.UseDefaultCredentials = true;
                    

                    httpClientInstance = new HttpClient();
                    httpClientInstance.DefaultRequestHeaders.ConnectionClose = false;
                    client.Timeout = TimeSpan.FromSeconds(60);
                    client.DefaultRequestHeaders.Authorization = token;
                    client.BaseAddress = new Uri(API_URL);
                    client.DefaultRequestHeaders.Accept.Clear();
                    client.DefaultRequestHeaders.Accept.Add(
                    new MediaTypeWithQualityHeaderValue("application/json"));

                    ServicePointManager.ServerCertificateValidationCallback += (sender, cert, chain, sslPolicyErrors) => true;
                    requestDatetime = DateTime.Now;
                    response = client.PostAsync("api/Transactions/PostCrossCurrencyTransactions", new StringContent(
                                                    new JavaScriptSerializer().Serialize(model), Encoding.UTF8, "application/json")).Result;
                    responseDateTime = DateTime.Now;
                    //ResponseMessageViewModel responseApi = new ResponseMessageViewModel();
                    //ResponseMessage responseMsg = null;
                    bool result = false;

                    if (response.IsSuccessStatusCode)
                    {
                        result = response.IsSuccessStatusCode;
                        await response.Content.ReadAsAsync<TransactionPostingViewModel>();

                        var res = new ResponseMessageViewModel
                        {
                            responseCode = responseApi.responseCode,
                            webRequestDate = responseApi.webRequestDate,
                            webRequestStatus = responseApi.webRequestStatus,
                            serialNumber = responseApi.serialNumber,
                            message = responseApi.message
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
                    //handler.Dispose();
                    //client.Dispose();
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
                        APIURL = "api/Transactions/PostCrossCurrencyTransactions",
                        LOGTYPEID = model.FirstOrDefault().operationId,
                        REFERENCENUMBER = model.FirstOrDefault().referenceNumber,
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
            public async Task<ResponseMessage> ApiTransactionPosting(List<TransactionPostingViewModel> model, bool isCrossCurrency = false)
            {
                HttpClientHandler handler = new HttpClientHandler();
                HttpClient httpClientInstance;

                HttpClient client = new HttpClient(handler);
                var inputJson = new JavaScriptSerializer().Serialize(model);
                DateTime requestDatetime = new DateTime(), responseDateTime = new DateTime();
                HttpResponseMessage response = null;
                TransactionPostingViewModel responseApi = new TransactionPostingViewModel();
                ResponseMessage responseMsg = null;
                string responseJson = "";
                try
                {
                    var token = new AuthenticationHeaderValue("Authorization", API_KEY);
                    var dta = context.TBL_SETUP_GLOBAL.ToList();
                    handler.UseDefaultCredentials = true;
                    httpClientInstance = new HttpClient();
                    httpClientInstance.DefaultRequestHeaders.ConnectionClose = false;
                    client.Timeout = TimeSpan.FromSeconds(60);
                    client.DefaultRequestHeaders.Authorization = token;
                    client.BaseAddress = new Uri(API_URL);
                    client.DefaultRequestHeaders.Accept.Clear();
                    client.DefaultRequestHeaders.Accept.Add(
                    new MediaTypeWithQualityHeaderValue("application/json"));

                    ServicePointManager.ServerCertificateValidationCallback += (sender, cert, chain, sslPolicyErrors) => true;
                    requestDatetime = DateTime.Now;

                    string apiUrl = "api/Transactions/PostTransactions";
                    if (isCrossCurrency == true)
                    {
                        apiUrl = "api/Transactions/PostCrossCurrencyTransactions";
                    }

                    response = client.PostAsync(apiUrl, new StringContent(
                                                    new JavaScriptSerializer().Serialize(model), Encoding.UTF8, "application/json")).Result;

                    //response = client.PostAsync("api/Transactions/PostCrossCurrencyTransactions", new StringContent(
                    //                                 new JavaScriptSerializer().Serialize(model), Encoding.UTF8, "application/json")).Result;


                    responseDateTime = DateTime.Now; 

                    if (response.IsSuccessStatusCode)
                    {

                        responseApi = await response.Content.ReadAsAsync<TransactionPostingViewModel>();

                        var res = new ResponseMessageViewModel
                        {
                            responseCode = responseApi.responseCode,
                            webRequestDate = responseApi.webRequestDate,
                            webRequestStatus = responseApi.webRequestStatus,

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

                    //handler.Dispose();
                    //client.Dispose();

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
                        APIURL = "api/Transactions/PostTransactions",
                        LOGTYPEID = model.FirstOrDefault().operationId,
                        REFERENCENUMBER = model.FirstOrDefault().referenceNumber,
                        REQUESTDATETIME = requestDatetime,
                        REQUESTMESSAGE = inputJson,
                        RESPONSEDATETIME = responseDateTime,
                        RESPONSEMESSAGE = responseJson,
                    };

                    FinTrakBankingContext logContext = new FinTrakBankingContext();

                    logContext.TBL_CUSTOM_API_LOGS.Add(logs);

                    logContext.SaveChanges();
                }


                //context.SaveChanges();
            }
            public async Task<ResponseMessage> APIProcessLien(CasaLienViewModel model, string lienType)
            {
                HttpClientHandler handler = new HttpClientHandler();
                HttpClient httpClientInstance;
                LienProcessViewModel responseModel = new LienProcessViewModel();
                bool output = false;
                HttpClient client = new HttpClient(handler);
                var objData = new JavaScriptSerializer().Serialize(model);
                DateTime requestDatetime = new DateTime(), responseDateTime = new DateTime();
                HttpResponseMessage response = null;
                //TransactionPostingViewModel responseApi = new TransactionPostingViewModel();
                ResponseMessage responseMsg = null;
                string responseMessage = "";
                string responseJson = "";

                try
                {
                    //var leinAmountString = String.Format("{0:0.00}", model.lienAmount)
                    //{ "account": "2022072744", "lienProcessType": "LIFTLIEN", "lienReasonCode": "VIA", "lienReason": "TESTING1", "lienAmount": 2000.00, "lienAccountCurrency": "NGN", "lienUniqueReferenceNumber": 992345678 }

                    var currencyCode = "";

                    if (model.isTermDeposit)
                    currencyCode = model.currencyCode;

                    else
                    {
                        currencyCode = context.TBL_CASA.Where(x =>
                                x.PRODUCTACCOUNTNUMBER == model.productAccountNumber && x.COMPANYID == model.companyId)
                            .Select(x => x.TBL_CURRENCY.CURRENCYCODE).FirstOrDefault();
                    }

                    LienAPIProcessViewModel apiModel = new LienAPIProcessViewModel
                    {
                        account = model.productAccountNumber,
                        lienProcessType = lienType, //"PLACE" or LIFTLIEN
                        lienReasonCode = "VIA",
                        lienReason = model.description,
                        lienAmount = String.Format("{0:0.00}", model.lienAmount), //model.lienAmount,  //
                        lienAccountCurrency = currencyCode,
                        lienUniqueReferenceNumber = model.lienReferenceNumber,
                    };

                    var token = new AuthenticationHeaderValue("Authorization", API_KEY);

                    handler.UseDefaultCredentials = true;
                    //HttpClient client = new HttpClient(handler);

                    httpClientInstance = new HttpClient();
                    httpClientInstance.DefaultRequestHeaders.ConnectionClose = false;
                    client.Timeout = TimeSpan.FromSeconds(60);
                    client.BaseAddress = new Uri(API_URL);
                    client.DefaultRequestHeaders.Accept.Clear();

                    // client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                    client.DefaultRequestHeaders.Authorization = token;

                    var serialiseModel = new JavaScriptSerializer().Serialize(apiModel);


                    ServicePointManager.ServerCertificateValidationCallback +=
                        (sender, cert, chain, sslPolicyErrors) => true;
                    requestDatetime = DateTime.Now;
                    response = client.PostAsync("api/Lien/ProcessLien", new StringContent(serialiseModel
                        , Encoding.UTF8, "application/json")).Result;
                    responseDateTime = DateTime.Now;
                    //if (response.IsSuccessStatusCode)
                    //{
                    //    responseModel = await response.Content.ReadAsAsync<LienProcessViewModel>();
                    //}

                    //ResponseViewModel responseAPI = new ResponseViewModel();
                    //responseAPI.responseCode = responseModel.responseCode;
                    //responseAPI.webRequestDate = responseModel.webRequestDate;
                    //responseAPI.webRequestStatus = responseModel.webRequestStatus;
                    //responseAPI.referenceNumber = responseModel.referenceNumber;

                    //responseMessage = await response.Content.ReadAsStringAsync();

                    //handler.Dispose();
                    //client.Dispose();

                    //if (responseModel.responseCode == "0")
                    //{
                    //    output = true;
                    //}
                    //else
                    //{
                    //    output = false;
                    //}

                    if (response.IsSuccessStatusCode)
                    {

                        responseModel = await response.Content.ReadAsAsync<LienProcessViewModel>();

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
                        APIURL = "api/Lien/ProcessLien",
                        LOGTYPEID = 2,
                        REFERENCENUMBER = model.sourceReferenceNumber,
                        REQUESTDATETIME = requestDatetime,
                        REQUESTMESSAGE = objData,
                        RESPONSEDATETIME = responseDateTime,
                        RESPONSEMESSAGE = responseJson,
                    };
                    FinTrakBankingContext logContext = new FinTrakBankingContext();

                    logContext.TBL_CUSTOM_API_LOGS.Add(logs);

                    logContext.SaveChanges();
                }

                
            }
            public async Task<ResponseMessage> APIPostInterestRate(InterestRateInquiryViewModel model, string accountType)
            {
                HttpClientHandler handler = new HttpClientHandler();
                HttpClient httpClientInstance;
                InterestRateInquiryViewModel responseModel = new InterestRateInquiryViewModel();
                bool output = false;
                HttpClient client = new HttpClient(handler);
                var objData = new JavaScriptSerializer().Serialize(model);
                DateTime requestDatetime = new DateTime(), responseDateTime = new DateTime();
                HttpResponseMessage response = null;
                ResponseMessage responseMsg = null;
                string responseMessage = "";

                try
                {
                    InterestRateInquiryIntegrationViewModel apiModel = new InterestRateInquiryIntegrationViewModel
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
                    client.Timeout = TimeSpan.FromSeconds(60);
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
                        RESPONSEMESSAGE = responseModel.webRequestStatus,
                    };
                    FinTrakBankingContext logContext = new FinTrakBankingContext();

                    logContext.TBL_CUSTOM_API_LOGS.Add(logs);

                    logContext.SaveChanges();
                }


            }

        }
    }
}