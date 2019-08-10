using FintrakBanking.Common.CustomException;
using FintrakBanking.Entities.Models;
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
            API_URL = "http://10.1.9.197:94/";
            //var configdata = context.TBL_SETUP_COMPANY.FirstOrDefault();
            //if (configdata != null)
            //{
            //    API_KEY = configdata.APIKEY;
            //    API_URL = configdata.APIURL;
            //}
        }
        private static HttpClient _httpClientInstance;

        private ResponseMessageViewModel responseAPI;

        //public async Task<ResponseMessage> APICustomerRatio(RatingAndRatioViewModel model, int customerNumber)
        //{
        //    HttpClientHandler handler = new HttpClientHandler();
        //    HttpClient httpClientInstance;

        //    HttpClient client = new HttpClient(handler);
        //    DateTime requestDatetime = new DateTime(), responseDateTime = new DateTime();
        //    HttpResponseMessage response = null;
        //    ResponseMessageViewModel res = null;
        //    string responseMessage = "";
        //    try
        //    {
        //        handler.UseDefaultCredentials = true;

        //        var token = new AuthenticationHeaderValue("Authorization", API_KEY);
        //        httpClientInstance = new HttpClient();
        //        httpClientInstance.DefaultRequestHeaders.ConnectionClose = false;
        //        client.Timeout = TimeSpan.FromSeconds(180);
        //        client.BaseAddress = new Uri(API_URL);
        //        client.DefaultRequestHeaders.Accept.Clear();
        //        client.DefaultRequestHeaders.Accept.Add(
        //            new MediaTypeWithQualityHeaderValue("application/json"));

        //        ServicePointManager.ServerCertificateValidationCallback +=
        //            (sender, cert, chain, sslPolicyErrors) => true;
        //        requestDatetime = DateTime.Now;
               
        //        response = await client.GetAsync(
        //            $"api/Credit360API/corporateRatioPDCosolidatedByCustomer_ID/{customerNumber}");
        //        responseDateTime = DateTime.Now;
        //        List<RatingAndRatioViewModel> customerRatios;
        //        if (response.IsSuccessStatusCode)
        //        {
        //            customerRatios = await response.Content.ReadAsAsync<RatingAndRatioViewModel>();

                   

        //        }

        //        responseMessage = await response.Content.ReadAsStringAsync();

        //        //handler.Dispose();
        //        //client.Dispose();

        //        return exchangeRateOutput;
        //    }
        //    catch (APIErrorException ex)
        //    {
        //        throw new APIErrorException(ex.Message);
        //    }
        //    catch (Exception ex)
        //    {
        //        throw new APIErrorException($"Error" + ex.Message);
        //    }
        //    finally
        //    {
        //        handler.Dispose();
        //        client.Dispose();



        //        var logs = new TBL_CUSTOM_API_LOGS
        //        {
        //            APIURL = $"api/ExchangeRate/GetExchangeRateProduct/{fromCurrencyCode}/{toCurrencyCode}/{rateCode}",
        //            LOGTYPEID = 3,
        //            REFERENCENUMBER = fromCurrencyCode + "--" + toCurrencyCode + "--" + rateCode,
        //            REQUESTDATETIME = requestDatetime,
        //            REQUESTMESSAGE = fromCurrencyCode + "--" + toCurrencyCode + "--" + rateCode,
        //            RESPONSEDATETIME = responseDateTime,
        //            RESPONSEMESSAGE = responseMessage,
        //        };

        //        FinTrakBankingContext logContext = new FinTrakBankingContext();

        //        logContext.TBL_CUSTOM_API_LOGS.Add(logs);

        //        logContext.SaveChanges();
        //    }
        //}

    }

}

