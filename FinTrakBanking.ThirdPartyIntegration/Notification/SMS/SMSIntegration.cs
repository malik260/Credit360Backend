using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace FinTrakBanking.ThirdPartyIntegration.Notification.SMS
{

  public  class SMSIntegration
    {
        FbnAlertWS.IService proxy  = new FbnAlertWS.ServiceClient();

        public void SendMessage ()
        {
          var result =  proxy.SendMessage(new FbnAlertWS.SMSMessage
            {
                AccountNumber = "25653",
                AppCode = "df",
                Message = "woer",
                MobileNo = "080127650164"
            });
            
        }
    }
}
