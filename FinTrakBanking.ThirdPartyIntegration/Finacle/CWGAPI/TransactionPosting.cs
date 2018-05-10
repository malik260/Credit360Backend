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

namespace FinTrakBanking.ThirdPartyIntegration.Finacle.CWGAPI
{
    public class TransactionPosting
    {
         
        private FinTrakBankingContext context;
        string API_KEY, API_URL = string.Empty;

        public TransactionPosting(

        FinTrakBankingContext _context)
        {
            this.context = _context;
            var configdata = context.TBL_SETUP_COMPANY.FirstOrDefault();
            API_KEY = configdata.APIKEY;
            API_URL = configdata.APIURL;
        }
        

        private HttpClientHandler handler = new HttpClientHandler();
        private static HttpClient httpClientInstance;

        public async Task<CurrencyExchangeRateViewModel> GetExchangeRate(string fromCurrencyCode, string toCurrencyCode, string rateCode)
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
            CurrencyExchangeRateIntegrationViewModel exchangeRateAPI = new CurrencyExchangeRateIntegrationViewModel();
            ServicePointManager.ServerCertificateValidationCallback += (sender, cert, chain, sslPolicyErrors) => true;
            //HttpResponseMessage response = await client.GetAsync($"api/ExchangeRate/GetExchangeRateProduct?rateProduct.fromCurrencyCode={fromCurrencyCode}&rateProduct.toCurrencyCode={toCurrencyCode}&rateProduct.rateCode={rateCode}");
            HttpResponseMessage response = await client.GetAsync($"api/ExchangeRate/GetExchangeRateProduct?model.fromCurrencyCode={fromCurrencyCode}&model.toCurrencyCode={toCurrencyCode}&model.rateCode={rateCode}");
            if (response.IsSuccessStatusCode)
            {
                exchangeRateAPI = await response.Content.ReadAsAsync<CurrencyExchangeRateIntegrationViewModel>();
            }
            var currencyId = context.TBL_CURRENCY.FirstOrDefault(x => x.CURRENCYCODE == exchangeRateAPI.currencyCode).CURRENCYID;
            exchangeRateOutput.sellingRate = exchangeRateAPI.exchangeRate;
            exchangeRateOutput.buyingRate = exchangeRateAPI.exchangeRate;
            exchangeRateOutput.currencyId = currencyId;
            exchangeRateOutput.date = exchangeRateAPI.webRequestDate;
            exchangeRateOutput.webRequestStatus = exchangeRateAPI.webRequestStatus;

            handler.Dispose();
            client.Dispose();

            return exchangeRateOutput;


        }


        private bool AddCustomTransactions(List<TransactionPostingViewModel> entity)
        {
            bool output = false;
            foreach (var item in entity)            
            {
                var data = new TBL_CUSTOM_FIANCE_TRANSACTION();
                {
                    data.ACCOUNTID = item.accounts;
                    data.AMOUNT = item.amounts;
                    data.BATCHCODE = item.referenceNumber;
                    data.CONSUMED = false;
                    data.CURRENCYCODE = item.currencyType;
                    data.DATETIMECONSUMED = null;
                    data.DATETIMECREATED = DateTime.Now;
                    data.NARRATION = item.narration;
                    data.OPERATIONID = item.operationId;
                }             
                context.TBL_CUSTOM_FIANCE_TRANSACTION.Add(data);
            };

            context.SaveChanges();
            output = true;
            return output;

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
                };
                context.TBL_CUSTOM_LIEN_PROCESS.Add(data);
            //};

            context.SaveChanges();
            output = true;
            return output;

        }

        public async Task<bool> APITransactionPosting (List<FinanceTransactionViewModel> model)
        {

            var token = new AuthenticationHeaderValue("Authorization", API_KEY);
            bool output = false;
            var dta = context.TBL_SETUP_GLOBAL.ToList();
            TransactionPostingViewModel responseModel = new TransactionPostingViewModel();
            List<TransactionPostingViewModel> apiModel = new List<TransactionPostingViewModel>();
            foreach (var item in model)
            {
             

                apiModel.Add(new TransactionPostingViewModel
                {

                    accounts = item.casaAccountId.ToString(), //item.casaAccountId!= null ? context.TBL_CASA.FirstOrDefault(x => x.CASAACCOUNTID == item.casaAccountId).PRODUCTACCOUNTNUMBER : context.TBL_CHART_OF_ACCOUNT.FirstOrDefault(x => x.GLACCOUNTID == item.glAccountId).ACCOUNTCODE,                       
                        amounts = item.creditAmount > 0 ? "C" + item.creditAmount.ToString() : "D" + item.debitAmount.ToString(),
                        //amounts = item.sourceReferenceNumber,
                        narration = item.description,
                        referenceNumber = item.batchCode,
                        currencyType = context.TBL_CURRENCY.FirstOrDefault(x => x.CURRENCYID == item.currencyId).CURRENCYCODE,
                        operationId = item.operationId,// != null ? context.TBL_CASA.FirstOrDefault(x => x.CASAACCOUNTID == item.operationId).PRODUCTACCOUNTNUMBER : context.TBL_CHART_OF_ACCOUNT.FirstOrDefault(x => x.GLACCOUNTID == item.glAccountId).ACCOUNTCODE,
                    }
                    );
            }

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
                                            new JavaScriptSerializer().Serialize(apiModel), Encoding.UTF8, "application/json")).Result;

            if (response.IsSuccessStatusCode)
            {
                responseModel = await response.Content.ReadAsAsync<TransactionPostingViewModel>();
                
            }

            ResponseViewModel responseAPI = new ResponseViewModel();
            responseAPI.responseCode = responseModel.responseCode;
            responseAPI.webRequestDate = responseModel.webRequestDate;
            responseAPI.webRequestStatus = responseModel.webRequestStatus;

            handler.Dispose();
            client.Dispose();
            if (responseModel.responseCode == "0")
            {
                AddCustomTransactions(apiModel);
                output = true;
            }
            else
            {
                output = false;
                throw new Exception($"Transaction {responseAPI.webRequestStatus}");
            }

            return output;


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
                lienAccountCurrency = context.TBL_CASA.FirstOrDefault(x => x.PRODUCTACCOUNTNUMBER == model.productAccountNumber && x.COMPANYID == model.companyId).TBL_CURRENCY.CURRENCYCODE
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
            ServicePointManager.ServerCertificateValidationCallback += (sender, cert, chain, sslPolicyErrors) => true;
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

       
        //----------------------------------- OverDraft----------------------------------------

        public async Task<ResponseMessage> APIOverDraftNormal(OverDraftNormalViewModel model)
        {
            model.sanctionLevel  = "003";
            model.sanctionAuthorizer = "999";

       
            ResponseMessageViewModel responseModel = new ResponseMessageViewModel();

            var token = new AuthenticationHeaderValue("Authorization", API_KEY);

            handler.UseDefaultCredentials = true;
            HttpClient client = new HttpClient(handler);

            httpClientInstance = new HttpClient();
            httpClientInstance.DefaultRequestHeaders.ConnectionClose = false;
            client.Timeout = TimeSpan.FromSeconds(60);
            client.DefaultRequestHeaders.Authorization = token;
            client.BaseAddress = new Uri(  API_URL);
            client.DefaultRequestHeaders.Accept.Clear();
            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

            ServicePointManager.ServerCertificateValidationCallback += (sender, cert, chain, sslPolicyErrors) => true;
            HttpResponseMessage response = client.PostAsync("api/OverDraft/Normal", new StringContent(
                                            new JavaScriptSerializer().Serialize(model), Encoding.UTF8, "application/json")).Result;


            ResponseMessage responseMsg = null;

            ResponseMessageViewModel responseAPI = new ResponseMessageViewModel();
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

            return responseMsg;


        }

        public async Task<ResponseMessage> APIOverDraftTopUp(OverDraftTopUpAndRenewViewModel model)
        {
            ResponseMessageViewModel responseModel = new ResponseMessageViewModel();

            var token = new AuthenticationHeaderValue("Authorization",API_KEY);

            handler.UseDefaultCredentials = true;
            HttpClient client = new HttpClient(handler);

            httpClientInstance = new HttpClient();
            httpClientInstance.DefaultRequestHeaders.ConnectionClose = false;
            client.Timeout = TimeSpan.FromSeconds(60);
            client.DefaultRequestHeaders.Authorization = token;
            client.BaseAddress = new Uri(API_URL);
            client.DefaultRequestHeaders.Accept.Clear();
            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));


            ServicePointManager.ServerCertificateValidationCallback += (sender, cert, chain, sslPolicyErrors) => true;
            HttpResponseMessage response = client.PostAsync("api/OverDraft/TopUp", new StringContent(
                                            new JavaScriptSerializer().Serialize(model), Encoding.UTF8, "application/json")).Result;




            ResponseMessageViewModel responseAPI = new ResponseMessageViewModel();

            ResponseMessage responseMsg = null;
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

            return responseMsg;

        }

        public async Task<ResponseMessage> APIOverDraftRenew(OverDraftTopUpAndRenewViewModel model)
        {
            ResponseMessageViewModel responseModel = new ResponseMessageViewModel();

            var token = new AuthenticationHeaderValue("Authorization",  API_KEY);

            handler.UseDefaultCredentials = true;
            HttpClient client = new HttpClient(handler);

            httpClientInstance = new HttpClient();
            httpClientInstance.DefaultRequestHeaders.ConnectionClose = false;
            client.Timeout = TimeSpan.FromSeconds(60);
            client.DefaultRequestHeaders.Authorization = token;
            client.BaseAddress = new Uri( API_URL);
            client.DefaultRequestHeaders.Accept.Clear();
            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));


            ServicePointManager.ServerCertificateValidationCallback += (sender, cert, chain, sslPolicyErrors) => true;
            HttpResponseMessage response = client.PostAsync("api/OverDraft/Renew ", new StringContent(
                                            new JavaScriptSerializer().Serialize(model), Encoding.UTF8, "application/json")).Result;




            ResponseMessageViewModel responseAPI = new ResponseMessageViewModel();

            ResponseMessage responseMsg = null;
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

            return responseMsg;

        }

        public async Task<ResponseMessage> APIOverDraftExtend(OverDraftExtendViewModel model)
        {
             
            ResponseMessageViewModel responseModel = new ResponseMessageViewModel();

            var token = new AuthenticationHeaderValue("Authorization",  API_KEY);

            handler.UseDefaultCredentials = true;
            HttpClient client = new HttpClient(handler);

            httpClientInstance = new HttpClient();
            httpClientInstance.DefaultRequestHeaders.ConnectionClose = false;
            client.Timeout = TimeSpan.FromSeconds(60);
            client.DefaultRequestHeaders.Authorization = token;
            client.BaseAddress = new Uri( API_URL);
            client.DefaultRequestHeaders.Accept.Clear();
            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));


            ServicePointManager.ServerCertificateValidationCallback += (sender, cert, chain, sslPolicyErrors) => true;
            HttpResponseMessage response = client.PostAsync("api/OverDraft/Extend", new StringContent(
                                            new JavaScriptSerializer().Serialize(model), Encoding.UTF8, "application/json")).Result;

            ResponseMessageViewModel responseAPI = new ResponseMessageViewModel();
            ResponseMessage responseMsg = null;
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

            return responseMsg;

        }

     
        ////////////////////////////////////////////////////////////////////////////////////////////////

        //----------------------------------- TemporaryOverDraft----------------------------------------
        public async Task<ResponseMessage> APITemporaryOverDraftNormal(TemporaryOverDraftViewModel model)
        {

            
            ResponseMessageViewModel responseModel = new ResponseMessageViewModel();

            var token = new AuthenticationHeaderValue("Authorization",  API_KEY);

            handler.UseDefaultCredentials = true;
            HttpClient client = new HttpClient(handler);

            httpClientInstance = new HttpClient();
            httpClientInstance.DefaultRequestHeaders.ConnectionClose = false;
            client.Timeout = TimeSpan.FromSeconds(60);
            client.DefaultRequestHeaders.Authorization = token;
            client.BaseAddress = new Uri( API_URL);
            client.DefaultRequestHeaders.Accept.Clear();
            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));


            ServicePointManager.ServerCertificateValidationCallback += (sender, cert, chain, sslPolicyErrors) => true;
            HttpResponseMessage response = client.PostAsync("api/TemporaryOverDraft/Normal", new StringContent(
                                            new JavaScriptSerializer().Serialize(model), Encoding.UTF8, "application/json")).Result;




            ResponseMessageViewModel responseAPI = new ResponseMessageViewModel();
            ResponseMessage responseMsg = null;
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

            return responseMsg;


        }

        public async Task<ResponseMessage> APITemporaryOverDraftRunning(TemporaryOverDraftViewModel model)
        {
             
            ResponseMessageViewModel responseModel = new ResponseMessageViewModel();

            var token = new AuthenticationHeaderValue("Authorization", API_KEY);

            handler.UseDefaultCredentials = true;
            HttpClient client = new HttpClient(handler);

            httpClientInstance = new HttpClient();
            httpClientInstance.DefaultRequestHeaders.ConnectionClose = false;
            client.Timeout = TimeSpan.FromSeconds(60);
            client.DefaultRequestHeaders.Authorization = token;
            client.BaseAddress = new Uri(API_URL);
            client.DefaultRequestHeaders.Accept.Clear();
            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));


            ServicePointManager.ServerCertificateValidationCallback += (sender, cert, chain, sslPolicyErrors) => true;
            HttpResponseMessage response = client.PostAsync("api/TemporaryOverDraft/Running", new StringContent(
                                            new JavaScriptSerializer().Serialize(model), Encoding.UTF8, "application/json")).Result;




            ResponseMessageViewModel responseAPI = new ResponseMessageViewModel();
            ResponseMessage responseMsg = null;
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

            return responseMsg;


        }

        public async Task<ResponseMessage> APITemporaryOverDraftSingle(TemporaryOverDraftViewModel model)
        {
 
            ResponseMessageViewModel responseModel = new ResponseMessageViewModel();
            var token = new AuthenticationHeaderValue("Authorization",  API_KEY); ;
            handler.UseDefaultCredentials = true;
            HttpClient client = new HttpClient(handler);

            httpClientInstance = new HttpClient();
            httpClientInstance.DefaultRequestHeaders.ConnectionClose = false;
            client.Timeout = TimeSpan.FromSeconds(60);
            client.DefaultRequestHeaders.Authorization = token;
            client.BaseAddress = new Uri( API_URL);
            client.DefaultRequestHeaders.Accept.Clear();
            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

            ServicePointManager.ServerCertificateValidationCallback += (sender, cert, chain, sslPolicyErrors) => true;
            HttpResponseMessage response = client.PostAsync("api/TemporaryOverDraft/Single", new StringContent(
                                            new JavaScriptSerializer().Serialize(model), Encoding.UTF8, "application/json")).Result;

            ResponseMessageViewModel responseAPI = new ResponseMessageViewModel();

            ResponseMessage responseMsg = null;
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
            handler.Dispose();
            client.Dispose();
            return responseMsg;
        }

        public async Task<GLAccountDetailsViewModel> APIOfficeAccount(string glNumber)
        {
            handler.UseDefaultCredentials = true;
            HttpClient client = new HttpClient(handler);
            var token = new AuthenticationHeaderValue("Authorization", API_KEY);
            httpClientInstance = new HttpClient();
            httpClientInstance.DefaultRequestHeaders.ConnectionClose = false;
            client.Timeout = TimeSpan.FromSeconds(30);
            client.BaseAddress = new Uri(API_URL);
            client.DefaultRequestHeaders.Accept.Clear();
            client.DefaultRequestHeaders.Authorization = token;
            client.DefaultRequestHeaders.Accept.Add(
            new MediaTypeWithQualityHeaderValue("application/json"));

            CasaBalanceViewModel accountOutput = new CasaBalanceViewModel();
            CasaIntegrationViewModel accountAPI = new CasaIntegrationViewModel();
            ServicePointManager.ServerCertificateValidationCallback += (sender, cert, chain, sslPolicyErrors) => true;
            HttpResponseMessage response = await client.GetAsync($"api/OfficeAccount/GetGlAccountRecord?accountNumber={glNumber}");

            GLAccountDetailsViewModel result = null;
            if (response.IsSuccessStatusCode)
            {
                GLAccountDetailsViewModel data = await response.Content.ReadAsAsync<GLAccountDetailsViewModel>();
                result = new GLAccountDetailsViewModel
                {
                    accountName = data.accountName,
                    accountNumber = data.accountNumber,
                    balance = data.balance,
                    branch = data.branch,
                    currencyType = data.currencyType,
                    glSubHeadCode = data.glSubHeadCode,
                    partitionedFlag = data.partitionedFlag,
                    partitionedType = data.partitionedType,
                    product = data.product,
                    productName = data.productName,
                    productType = data.productType,
                    systemAccountFlag = data.systemAccountFlag,
                    response = response,
                };
                handler.Dispose();
                client.Dispose();
                return result;
            }
            handler.Dispose();
            client.Dispose();
            return result;
        }
     
    }
}
