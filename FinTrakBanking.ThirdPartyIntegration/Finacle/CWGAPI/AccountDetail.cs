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
                handler.UseDefaultCredentials = true;
                HttpClient client = new HttpClient(handler);
                var token = new AuthenticationHeaderValue("Authorization", API_KEY);
                _httpClientInstance = new HttpClient();
                _httpClientInstance.DefaultRequestHeaders.ConnectionClose = false;
                client.Timeout = TimeSpan.FromSeconds(30);
                client.BaseAddress = new Uri(API_URL);
                client.DefaultRequestHeaders.Accept.Clear();
                client.DefaultRequestHeaders.Authorization = token;
                client.DefaultRequestHeaders.Accept.Add(
                    new MediaTypeWithQualityHeaderValue("application/json"));

                
                ServicePointManager.ServerCertificateValidationCallback +=
                    (sender, cert, chain, sslPolicyErrors) => true;
                HttpResponseMessage response =
                    await client.GetAsync($"api/OfficeAccount/GetGeneralLedgerAccountRecord?accountNumber={glNumber}");

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
                        handler.Dispose();
                        client.Dispose();
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
                handler.UseDefaultCredentials = true;
                HttpClient client = new HttpClient(handler);
                var token = new AuthenticationHeaderValue("Authorization", API_KEY);
                _httpClientInstance = new HttpClient();
                _httpClientInstance.DefaultRequestHeaders.ConnectionClose = false;
                client.Timeout = TimeSpan.FromSeconds(30);
                client.BaseAddress = new Uri(API_URL);
                client.DefaultRequestHeaders.Accept.Clear();
                client.DefaultRequestHeaders.Authorization = token;
                client.DefaultRequestHeaders.Accept.Add(
                    new MediaTypeWithQualityHeaderValue("application/json"));
                 
                ServicePointManager.ServerCertificateValidationCallback +=
                    (sender, cert, chain, sslPolicyErrors) => true;
                HttpResponseMessage response = await client.GetAsync(
                    $"api/OfficeAccount/GetTermDepositAccountRecord?accountNumber={teamDepositAccountNumber}");

                TDAccountRecordViewModel result = null;
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

}


