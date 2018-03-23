using FintrakBanking.Entities.Models;
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

namespace FinTrakBanking.ThirdPartyIntegration.Finacle
{
    public class TransactionPosting
    {
        private FinTrakBankingContext context;
        public TransactionPosting(

        FinTrakBankingContext _context)
        {
            this.context = _context;
        }

        private HttpClientHandler handler = new HttpClientHandler();
        private static HttpClient httpClientInstance;

        public async Task<CurrencyExchangeRateViewModel> GetExchangeRate(string fromCurrencyCode, string toCurrencyCode, string rateCode)
        {
            handler.UseDefaultCredentials = true;
            HttpClient client = new HttpClient(handler);

            httpClientInstance = new HttpClient();
            httpClientInstance.DefaultRequestHeaders.ConnectionClose = false;
            client.Timeout = TimeSpan.FromSeconds(30);
            client.BaseAddress = new Uri("https://172.16.249.195/FbnFintrak.Api.Test/");
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
                    data.OPERATIONID = 1;
                }             
                context.TBL_CUSTOM_FIANCE_TRANSACTION.Add(data);
            };

            context.SaveChanges();
            output = true;
            return output;

        }

        public async Task<bool> APITransactionPosting (List<FinanceTransactionViewModel> model)
        {

            bool output = false;
            List<TransactionPostingViewModel> apiModel = new List<TransactionPostingViewModel>();
            foreach (var item in model)
            {
                    apiModel.Add(new TransactionPostingViewModel
                    {
                        accounts = item.casaAccountId!= null ? context.TBL_CASA.FirstOrDefault(x => x.CASAACCOUNTID == item.casaAccountId).PRODUCTACCOUNTNUMBER : context.TBL_CHART_OF_ACCOUNT.FirstOrDefault(x => x.GLACCOUNTID == item.glAccountId).ACCOUNTCODE,
                        //accounts = item.operationId != null ? context.TBL_CASA.FirstOrDefault(x => x.CASAACCOUNTID == item.operationId).PRODUCTACCOUNTNUMBER : context.TBL_CHART_OF_ACCOUNT.FirstOrDefault(x => x.GLACCOUNTID == item.glAccountId).ACCOUNTCODE,
                        amounts = item.creditAmount > 0 ? "C" + item.creditAmount.ToString() : "D" + item.debitAmount.ToString(),
                        //amounts = item.sourceReferenceNumber,
                        narration = item.description,
                        referenceNumber = item.batchCode,
                        currencyType = context.TBL_CURRENCY.FirstOrDefault(x => x.CURRENCYID == item.currencyId).CURRENCYCODE,
                    }
                    );
            }

            handler.UseDefaultCredentials = true;
            HttpClient client = new HttpClient(handler);

            httpClientInstance = new HttpClient();
            httpClientInstance.DefaultRequestHeaders.ConnectionClose = false;
            client.Timeout = TimeSpan.FromSeconds(30);
            client.BaseAddress = new Uri("https://172.16.249.195/FbnFintrak.Api.Test/");
            client.DefaultRequestHeaders.Accept.Clear();
            client.DefaultRequestHeaders.Accept.Add(
            new MediaTypeWithQualityHeaderValue("application/json"));
            TransactionPostingViewModel responseModel  = new TransactionPostingViewModel();
            ServicePointManager.ServerCertificateValidationCallback += (sender, cert, chain, sslPolicyErrors) => true;
            HttpResponseMessage response = client.PostAsync("api/Transactions/PostTransactionsList", new StringContent(
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



        public async Task<bool> APILienPosting(List<FinanceTransactionViewModel> model)
        {

            bool output = false;
            List<LienPostingViewModel> apiModel = new List<LienPostingViewModel>();
            foreach (var item in model)
            {
                apiModel.Add(new LienPostingViewModel
                {
                    account = context.TBL_CASA.FirstOrDefault(x => x.CASAACCOUNTID == item.operationId).PRODUCTACCOUNTNUMBER ,
                    lienProcessType = "",
                    lienReasonCode = "",
                    lienReason = item.description,
                    lienAmount = 0,
                    lienUniqueReferenceNumber = item.sourceReferenceNumber,
                    lienAccountCurrency = context.TBL_CURRENCY.FirstOrDefault(x => x.CURRENCYID == item.currencyId).CURRENCYCODE,
                    referenceNumber = item.batchCode,
                 
                }
                );
            }

            handler.UseDefaultCredentials = true;
            HttpClient client = new HttpClient(handler);

            httpClientInstance = new HttpClient();
            httpClientInstance.DefaultRequestHeaders.ConnectionClose = false;
            client.Timeout = TimeSpan.FromSeconds(30);
            client.BaseAddress = new Uri("https://172.16.249.195/FbnFintrak.Api.Test/");
            client.DefaultRequestHeaders.Accept.Clear();
            client.DefaultRequestHeaders.Accept.Add(
            new MediaTypeWithQualityHeaderValue("application/json"));
            LienPostingViewModel responseModel = new LienPostingViewModel();
            ServicePointManager.ServerCertificateValidationCallback += (sender, cert, chain, sslPolicyErrors) => true;
            HttpResponseMessage response = client.PostAsync("api/Lien/ProcessLien", new StringContent(
                                            new JavaScriptSerializer().Serialize(apiModel), Encoding.UTF8, "application/json")).Result;
            if (response.IsSuccessStatusCode)
            {
                responseModel = await response.Content.ReadAsAsync<LienPostingViewModel>();
            }
            ResponseViewModel responseAPI = new ResponseViewModel();
            responseAPI.responseCode = responseModel.responseCode;
            responseAPI.webRequestDate = responseModel.webRequestDate;
            responseAPI.webRequestStatus = responseModel.webRequestStatus;
            responseAPI.referenceNumber = responseModel.referenceNumber;

            handler.Dispose();
            client.Dispose();
            //if (responseModel.responseCode == "0")
            //{
            //    output = true;
            //}
            //else
            //{
            //    output = false;
            //}

            return output;


        }

    }
}
