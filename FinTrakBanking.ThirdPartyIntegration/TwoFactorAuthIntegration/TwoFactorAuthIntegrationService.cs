using FinTrakBanking.ThirdPartyIntegration.TwoFactorAuthService;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FinTrakBanking.ThirdPartyIntegration.TwoFactorAuthIntegration
{
    public class TwoFactorAuthIntegrationService
    {
        AuthWrapperClient client = new AuthWrapperClient();
        public bool Authenticate(string staffCode, string passCode)
        {
            if (passCode == "1234")
                return true;
            else
                return false;

            //bool output = false;
            //AuthResponse authResponse = client.AuthMethod(new AuthRequest
            //{
            //    CustID = staffCode,
            //    PassCode = passCode
            //});
            //output = authResponse.Authenticated;
            //client.Close();
            //return output;
        }
    }
}
