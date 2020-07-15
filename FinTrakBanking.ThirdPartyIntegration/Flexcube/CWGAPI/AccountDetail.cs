using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using FintrakBanking.ViewModels.Finance;
using FintrakBanking.ViewModels.ThridPartyIntegration;

namespace FinTrakBanking.ThirdPartyIntegration
{
    using FintrakBanking.Common.CustomException;
    using FintrakBanking.Entities.Models;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;

    namespace AccountInformation
    {
        public class AccountDetail
        {

            private FinTrakBankingContext context;
            string API_KEY, API_URL = string.Empty;
            private HttpClientHandler handler = new HttpClientHandler();
            private static HttpClient _httpClientInstance;


            public AccountDetail(FinTrakBankingContext _context)
            {
                this.context = _context;
                var configdata = context.TBL_SETUP_COMPANY.FirstOrDefault();
                API_KEY = configdata.APIKEY;
                API_URL = configdata.APIURL;
            }

            public async Task<GLAccountDetailsViewModel> APIOfficeAccountGetGeneralLedgerAccountRecord(string glNumber)
            {
                HttpClient client = new HttpClient(handler);
                DateTime requestDatetime = new DateTime(), responseDateTime = new DateTime();
                HttpResponseMessage response = null;
                ResponseMessageViewModel res = null;
                string responseMessage = "";
                handler.UseDefaultCredentials = true;
               // HttpClient client = new HttpClient(handler);
                var token = new AuthenticationHeaderValue("Authorization", API_KEY);
                _httpClientInstance = new HttpClient();
                _httpClientInstance.DefaultRequestHeaders.ConnectionClose = false;
                client.Timeout = TimeSpan.FromSeconds(180);
                client.BaseAddress = new Uri(API_URL);
                client.DefaultRequestHeaders.Accept.Clear();
                client.DefaultRequestHeaders.Authorization = token;
                client.DefaultRequestHeaders.Accept.Add(
                    new MediaTypeWithQualityHeaderValue("application/json"));

                
                ServicePointManager.ServerCertificateValidationCallback +=
                    (sender, cert, chain, sslPolicyErrors) => true;
                requestDatetime = DateTime.Now;
                response = await client.GetAsync($"api/Office/GetGeneralLedgerAccountRecord/{glNumber}");
                responseDateTime = DateTime.Now;
                GLAccountDetailsViewModel result = null;
                if (response.IsSuccessStatusCode)
                {
                    GLAccountDetailsViewModel data = await response.Content.ReadAsAsync<GLAccountDetailsViewModel>();
                    if (data != null)
                    {
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

                        responseMessage = await response.Content.ReadAsStringAsync();
                        handler.Dispose();
                        client.Dispose();


                        var logs = new TBL_CUSTOM_API_LOGS
                        {
                            APIURL = $"api/Office/GetGeneralLedgerAccountRecord/{glNumber}",
                            LOGTYPEID = 8,
                            REFERENCENUMBER = glNumber,
                            REQUESTDATETIME = requestDatetime,
                            REQUESTMESSAGE = glNumber,
                            RESPONSEDATETIME = responseDateTime,
                            RESPONSEMESSAGE = responseMessage,
                        };
                        FinTrakBankingContext logContext = new FinTrakBankingContext();

                        logContext.TBL_CUSTOM_API_LOGS.Add(logs);

                        logContext.SaveChanges();
                        return result;
                    }
                    else throw new ConditionNotMetException("Account number not found on finacle");
                }
                else throw new ConditionNotMetException("Account Number Search. " + response.ReasonPhrase);

                //handler.Dispose();
               // client.Dispose();
               // return result;
            }

            public async Task<TDAccountRecordViewModel> APIOfficeAccountGetTermDepositAccountRecord(
                string teamDepositAccountNumber)
            {
                HttpClient client = new HttpClient(handler);
                DateTime requestDatetime = new DateTime(), responseDateTime = new DateTime();
                HttpResponseMessage response = null;
                //ResponseMessageViewModel res = null;
                string responseMessage = "";
                handler.UseDefaultCredentials = true;
                //HttpClient client = new HttpClient(handler);
                var token = new AuthenticationHeaderValue("Authorization", API_KEY);
                _httpClientInstance = new HttpClient();
                _httpClientInstance.DefaultRequestHeaders.ConnectionClose = false;
                client.Timeout = TimeSpan.FromSeconds(180);
                client.BaseAddress = new Uri(API_URL);
                client.DefaultRequestHeaders.Accept.Clear();
                client.DefaultRequestHeaders.Authorization = token;
                client.DefaultRequestHeaders.Accept.Add(
                    new MediaTypeWithQualityHeaderValue("application/json"));
                 
                ServicePointManager.ServerCertificateValidationCallback +=
                    (sender, cert, chain, sslPolicyErrors) => true;
                requestDatetime = DateTime.Now;
                response = await client.GetAsync(
                    $"api/Office/GetTermDepositAccountRecord/{teamDepositAccountNumber}");
                responseDateTime = DateTime.Now;
                var result = new TDAccountRecordViewModel();
                if (response.IsSuccessStatusCode)
                {
                    TDAccountRecordViewModel data = await response.Content.ReadAsAsync<TDAccountRecordViewModel>();
                    result = new TDAccountRecordViewModel
                    {
                        accountName = data.accountName,
                        accountNumber = data.accountNumber,
                        balance = data.balance,
                        branch = data.branch,
                        currencyType = data.currencyType,
                        customerCode = data.customerCode,
                        productName = data.productName,
                        productType = data.productType,
                        lienAmount = data.lienAmount,
                        productCode = data.productCode,
                        response = response,
                        isSuccess = true
                    };
                    responseMessage = await response.Content.ReadAsStringAsync();
                    handler.Dispose();
                    client.Dispose();

                    var logs = new TBL_CUSTOM_API_LOGS
                    {
                        APIURL = $"api/Office/GetTermDepositAccountRecord/{teamDepositAccountNumber}",
                        LOGTYPEID = 9,
                        REFERENCENUMBER = teamDepositAccountNumber,
                        REQUESTDATETIME = requestDatetime,
                        REQUESTMESSAGE = teamDepositAccountNumber,
                        RESPONSEDATETIME = responseDateTime,
                        RESPONSEMESSAGE = responseMessage,
                    };
                    FinTrakBankingContext logContext = new FinTrakBankingContext();
                    logContext.TBL_CUSTOM_API_LOGS.Add(logs);
                    
                    logContext.SaveChanges();
                    return result;
                }
                string a = response.ReasonPhrase;
                
                result.errorDesc =a;
                result.isSuccess = false;
                handler.Dispose();
                client.Dispose();
               
                return result;
            }

        }
    }

}


