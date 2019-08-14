using FintrakBanking.Common.CustomException;
using FintrakBanking.Entities.Models;
using FintrakBanking.ViewModels.Finance;
using FinTrakBanking.ThirdPartyIntegration.TwoFactorAuthService;
using FinTrakBanking.ThirdPartyIntegration.TwoFactorAuthSoapService;
//using FinTrakBanking.ThirdPartyIntegration.ServiceReference1;
using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.Text;
using System.Threading.Tasks;
using static FinTrakBanking.ThirdPartyIntegration.TwoFactorAuthIntegration.TwoFactorAuthIntegrationService;

namespace FinTrakBanking.ThirdPartyIntegration.TwoFactorAuthIntegration
{
    public class TwoFactorAuthIntegrationService : ITwoFactorAuthIntegrationService
    {

        public TwoFactorAutheticationOutputViewModel Authenticate(string staffCode, string passCode)
        {
            try
            {
                var requestDatetime = DateTime.Now;
                //AuthWrapperClient client = new AuthWrapperClient();

                //AuthResponse authResponse = client.AuthMethod(new AuthRequest
                //{
                //    CustID = staffCode,
                //    PassCode = passCode
                //});

                var binding = new BasicHttpBinding();
                //var endPointAddress = new EndpointAddress("http://10.111.13.47:7080/Service?wsdl");
                //var endPointAddress = new EndpointAddress("https://10.111.13.47:7080/Service");

                //var client = new ServiceSoapClient(binding, endPointAddress);
                //var client = new TestService.WeatherSoapClient();
                //var res = client.GetCityWeatherByZIP("99501");

                var client = new ServiceSoapClient();
                //client.Security.Transport.ClientCredentialType = HttpClientCredentialType.Basic;
                var res = client.ResponseOnlyAsync(staffCode, passCode);
                var responseDateTime = DateTime.Now;

                var output = new TwoFactorAutheticationOutputViewModel()
                {
                    authenticated = false, //authResponse.Authenticated,
                    message = res.Result //authResponse.Message
                };

                client.Close();
                if(staffCode != null)
                {
                    var logs = new TBL_CUSTOM_API_LOGS
                    {
                        APIURL = "http://ho-bespoke07.nigeria.firstbank.local/EntrustWrapper/AuthWrapper.svc",
                        LOGTYPEID = 15,
                        REFERENCENUMBER = staffCode,
                        REQUESTDATETIME = requestDatetime,
                        REQUESTMESSAGE = $"CustId : {staffCode} , PassCode : {passCode}",
                        RESPONSEDATETIME = responseDateTime,
                        RESPONSEMESSAGE = res.Result //authResponse.Message,
                    };

                    FinTrakBankingContext logContext = new FinTrakBankingContext();
                    logContext.TBL_CUSTOM_API_LOGS.Add(logs);
                    logContext.SaveChanges();
                }
                
                return output;
            }
            catch (TwoFactorAuthenticationException ex)
            {
                throw new TwoFactorAuthenticationException(ex.Message);
            }

        }

        public interface ITwoFactorAuthIntegrationService
        {
            TwoFactorAutheticationOutputViewModel Authenticate(string staffCode, string passCode);
        }
    }
}
