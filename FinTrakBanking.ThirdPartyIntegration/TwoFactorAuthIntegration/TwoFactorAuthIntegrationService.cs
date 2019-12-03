using FintrakBanking.Common.CustomException;
using FintrakBanking.Entities.Models;
using FintrakBanking.ViewModels.Finance;
using FinTrakBanking.ThirdPartyIntegration.TwoFactorAuthService;
using FinTrakBanking.ThirdPartyIntegration.TwoFactorAuthSoapService;
//using FinTrakBanking.ThirdPartyIntegration.TwoFactorAuthSoapService1;
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
                var groupName = "ACCESSUSERS";
                var requestDatetime = DateTime.Now;
                var binding = new BasicHttpBinding();


                var client = new ServiceSoapClient();
                client.Open();
                //var res = client.ResponseOnlyAsync(staffCode, passCode);
                var res = client.ResponseOnlyForGroup(staffCode, passCode, groupName);
                var responseDateTime = DateTime.Now;

                //var status = int.Parse(res.Split('~')[0]);
                var message = res;

                var output = new TwoFactorAutheticationOutputViewModel()
                {
                    message = message
                };

                if (res.ToLower().Contains("successful")) { 
                    output.message = "Authentication Successful!";
                    output.authenticated = true;
                }
                else
                {
                    output.authenticated = false;
                    output.message = "Two Factor Authentication Failed!";
                }

                if(staffCode != null)
                {
                    var logs = new TBL_CUSTOM_API_LOGS
                    {
                        APIURL = "https://esbentuser.accessbankplc.com:7085/Service?wsdl",
                        LOGTYPEID = 15,
                        REFERENCENUMBER = staffCode,
                        REQUESTDATETIME = requestDatetime,
                        REQUESTMESSAGE = $"CustId : {staffCode} , PassCode : {passCode}",
                        RESPONSEDATETIME = responseDateTime,
                        RESPONSEMESSAGE = res, //output.message //authResponse.Message,
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
