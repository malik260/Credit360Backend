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
                output = authResponse.Authenticated;
                client.Close();
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
