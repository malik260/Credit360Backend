using FintrakBanking.Entities.Models;
using FinTrakBanking.ThirdPartyIntegration.TwoFactorAuthService;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static FinTrakBanking.ThirdPartyIntegration.TwoFactorAuthIntegration.TwoFactorAuthIntegrationService;

namespace FinTrakBanking.ThirdPartyIntegration.TwoFactorAuthIntegration
{
    public class TwoFactorAuthIntegrationService : ITwoFactorAuthIntegrationService
    {

        public bool Authenticate(string staffCode, string passCode)
        {
            try
            {
                var requestDatetime = DateTime.Now;
                AuthWrapperClient client = new AuthWrapperClient();
                //if (passCode == "1234")
                //    return true;
                //else
                //    return false;

                bool output = false;
                AuthResponse authResponse = client.AuthMethod(new AuthRequest
                {
                    CustID = staffCode,
                    PassCode = passCode
                });
               var responseDateTime = DateTime.Now;
                output = authResponse.Authenticated;
                client.Close();
                var logs = new TBL_CUSTOM_API_LOGS
                {
                    APIURL = "http://ho-bespoke07.nigeria.firstbank.local/EntrustWrapper/AuthWrapper.svc",
                    LOGTYPEID = 15,
                    REFERENCENUMBER = staffCode,
                    REQUESTDATETIME = requestDatetime,
                    REQUESTMESSAGE = $"CustId : {staffCode} , PassCode : {passCode}",
                    RESPONSEDATETIME = responseDateTime,
                    RESPONSEMESSAGE = authResponse.Message,
                };
                FinTrakBankingContext logContext = new FinTrakBankingContext();

                logContext.TBL_CUSTOM_API_LOGS.Add(logs);

                logContext.SaveChanges();
                return output;
            }
            catch (Exception ex)
            {
                return false;
            }

        }
        public interface ITwoFactorAuthIntegrationService
        {
            bool Authenticate(string staffCode, string passCode);
        }
    }
}
