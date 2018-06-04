namespace FinTrakBanking.ThirdPartyIntegration
{
    using FintrakBanking.Entities.Models;
    using FintrakBanking.ViewModels.CASA;
    using FintrakBanking.ViewModels.Credit;
    using FintrakBanking.ViewModels.Finance;
    using FintrakBanking.ViewModels.ThridPartyIntegration;
    using Newtonsoft.Json;
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

           

            private HttpClientHandler handler = new HttpClientHandler();
            private static HttpClient httpClientInstance;



            public async Task<CurrencyExchangeRateViewModel> GetExchangeRate(string fromCurrencyCode,
                string toCurrencyCode, string rateCode)
            {
                handler.UseDefaultCredentials = true;
                HttpClient client = new HttpClient(handler);
                var token = new AuthenticationHeaderValue("Authorization", API_KEY);
                httpClientInstance = new HttpClient();
                httpClientInstance.DefaultRequestHeaders.ConnectionClose = false;
                client.Timeout = TimeSpan.FromSeconds(30);
                client.BaseAddress = new Uri(API_URL);
                client.DefaultRequestHeaders.Accept.Clear();
                client.DefaultRequestHeaders.Accept.Add(
                    new MediaTypeWithQualityHeaderValue("application/json"));

                CurrencyExchangeRateViewModel exchangeRateOutput = new CurrencyExchangeRateViewModel();
                CurrencyExchangeRateIntegrationViewModel exchangeRateAPI =
                    new CurrencyExchangeRateIntegrationViewModel();
                ServicePointManager.ServerCertificateValidationCallback +=
                    (sender, cert, chain, sslPolicyErrors) => true;
                //HttpResponseMessage response = await client.GetAsync($"api/ExchangeRate/GetExchangeRateProduct?rateProduct.fromCurrencyCode={fromCurrencyCode}&rateProduct.toCurrencyCode={toCurrencyCode}&rateProduct.rateCode={rateCode}");
                HttpResponseMessage response = await client.GetAsync(
                    $"api/ExchangeRate/GetExchangeRateProduct?model.fromCurrencyCode={fromCurrencyCode}&model.toCurrencyCode={toCurrencyCode}&model.rateCode={rateCode}");
                if (response.IsSuccessStatusCode)
                {
                    exchangeRateAPI = await response.Content.ReadAsAsync<CurrencyExchangeRateIntegrationViewModel>();
                }

                var currencyId = context.TBL_CURRENCY
                    .FirstOrDefault(x => x.CURRENCYCODE == exchangeRateAPI.currencyCode).CURRENCYID;
                exchangeRateOutput.sellingRate = exchangeRateAPI.exchangeRate;
                exchangeRateOutput.buyingRate = exchangeRateAPI.exchangeRate;
                exchangeRateOutput.currencyId = (short) currencyId;
                exchangeRateOutput.date = exchangeRateAPI.webRequestDate;
                exchangeRateOutput.webRequestStatus = exchangeRateAPI.webRequestStatus;

                handler.Dispose();
                client.Dispose();

                return exchangeRateOutput;


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
            //    client.Timeout = TimeSpan.FromSeconds(30);
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
            //        throw new Exception($"Transaction {responseAPI.webRequestStatus}");
            //    }

            //    return output;


            //}

            public async Task<ResponseMessage> ApiPostCrossCurrencyTransactions(List<TransactionPostingViewModel> model)
            {

                var token = new AuthenticationHeaderValue("Authorization", API_KEY);

                var dta = context.TBL_SETUP_GLOBAL.ToList();

                handler.UseDefaultCredentials = true;
                HttpClient client = new HttpClient(handler);

                httpClientInstance = new HttpClient();
                httpClientInstance.DefaultRequestHeaders.ConnectionClose = false;
                client.Timeout = TimeSpan.FromSeconds(30);
                client.DefaultRequestHeaders.Authorization = token;
                client.BaseAddress = new Uri(API_URL);
                client.DefaultRequestHeaders.Accept.Clear();
                client.DefaultRequestHeaders.Accept.Add(
                new MediaTypeWithQualityHeaderValue("application/json"));

                ServicePointManager.ServerCertificateValidationCallback += (sender, cert, chain, sslPolicyErrors) => true;
                HttpResponseMessage response = client.PostAsync("api/Transactions/PostTransactions", new StringContent(
                                                new JavaScriptSerializer().Serialize(model), Encoding.UTF8, "application/json")).Result;

                ResponseMessageViewModel responseApi = new ResponseMessageViewModel();
                ResponseMessage responseMsg = null;
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
                handler.Dispose();
                client.Dispose();
                return responseMsg;






                //if (responseModel.responseCode == "0")
                //{
                //  //  AddCustomTransactions(model);
                //    output = true;
                //}
                //else
                //{
                //    output = false;
                //    throw new Exception($"Transaction {responseAPI.webRequestStatus}");
                //}

                //return output;


            }


            public async Task<ResponseMessage> ApiTransactionPosting(List<TransactionPostingViewModel> model)
            {

                var token = new AuthenticationHeaderValue("Authorization", API_KEY);
            
                var dta = context.TBL_SETUP_GLOBAL.ToList();

                var objData = new JavaScriptSerializer().Serialize(model);

                handler.UseDefaultCredentials = true;
                HttpClient client = new HttpClient(handler);

                httpClientInstance = new HttpClient();
                httpClientInstance.DefaultRequestHeaders.ConnectionClose = false;
                client.Timeout = TimeSpan.FromSeconds(30);
                client.DefaultRequestHeaders.Authorization = token;
                client.BaseAddress = new Uri(API_URL);
                client.DefaultRequestHeaders.Accept.Clear();
                client.DefaultRequestHeaders.Accept.Add(
                new MediaTypeWithQualityHeaderValue("application/json"));

                ServicePointManager.ServerCertificateValidationCallback += (sender, cert, chain, sslPolicyErrors) => true;
                HttpResponseMessage response = client.PostAsync("api/Transactions/PostTransactions", new StringContent(
                                                new JavaScriptSerializer().Serialize(model), Encoding.UTF8, "application/json")).Result;

                TransactionPostingViewModel responseApi = new TransactionPostingViewModel();
                ResponseMessage responseMsg = null;
                bool result = false;
               
                if (response.IsSuccessStatusCode)
                {
                    result = response.IsSuccessStatusCode;
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
                handler.Dispose();
                client.Dispose();
                return responseMsg;





            
                //if (responseModel.responseCode == "0")
                //{
                //  //  AddCustomTransactions(model);
                //    output = true;
                //}
                //else
                //{
                //    output = false;
                //    throw new Exception($"Transaction {responseAPI.webRequestStatus}");
                //}

                //return output;


            }


            public async Task<bool> APIProcessLien(CasaLienViewModel model, string lienType)
            {
                bool output = false;
                LienProcessViewModel apiModel = new LienProcessViewModel
                {
                    account = model.productAccountNumber,
                    lienProcessType = lienType, //"PLACE" or LIFTLIEN
                    lienReasonCode = "VIA",
                    lienReason = model.description,
                    lienAmount = model.lienAmount,
                    lienUniqueReferenceNumber = model.lienReferenceNumber,
                    lienAccountCurrency = context.TBL_CASA.FirstOrDefault(x =>
                            x.PRODUCTACCOUNTNUMBER == model.productAccountNumber && x.COMPANYID == model.companyId)
                        .TBL_CURRENCY.CURRENCYCODE
                };

                var token = new AuthenticationHeaderValue("Authorization", API_KEY);

                handler.UseDefaultCredentials = true;
                HttpClient client = new HttpClient(handler);

                httpClientInstance = new HttpClient();
                httpClientInstance.DefaultRequestHeaders.ConnectionClose = false;
                client.Timeout = TimeSpan.FromSeconds(30);
                client.BaseAddress = new Uri(API_URL);
                client.DefaultRequestHeaders.Accept.Clear();

                // client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                client.DefaultRequestHeaders.Authorization = token;

                LienProcessViewModel responseModel = new LienProcessViewModel();
                ServicePointManager.ServerCertificateValidationCallback +=
                    (sender, cert, chain, sslPolicyErrors) => true;
                HttpResponseMessage response = client.PostAsync("api/Lien/ProcessLien", new StringContent(
                    new JavaScriptSerializer().Serialize(apiModel), Encoding.UTF8, "application/json")).Result;
                if (response.IsSuccessStatusCode)
                {
                    responseModel = await response.Content.ReadAsAsync<LienProcessViewModel>();
                }

                ResponseViewModel responseAPI = new ResponseViewModel();
                responseAPI.responseCode = responseModel.responseCode;
                responseAPI.webRequestDate = responseModel.webRequestDate;
                responseAPI.webRequestStatus = responseModel.webRequestStatus;
                responseAPI.referenceNumber = responseModel.referenceNumber;

                handler.Dispose();
                client.Dispose();
                if (responseModel.responseCode == "0")
                {
                    output = true;
                }
                else
                {
                    output = false;
                }
                return output;
            }
         
        }
    }
}