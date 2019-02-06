using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FintrakBanking.Interfaces.AlertMonitoring
{
  public  interface IEmailAlertLogger
    {
        void ComposeEmail(string referenceNumber, string emailBody,string emailSubject, string recipientEmail, bool callSaveChanges);
    }
}
