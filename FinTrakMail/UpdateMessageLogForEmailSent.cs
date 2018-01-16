using FintrakBanking.Entities.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FinTrakMail
{
   public static class UpdateMessageLogForEmailSent
    {
        private static FinTrakBankingContext context = new FinTrakBankingContext(); 

        public static bool UpdateMailDeliveryStatus(int messageId, short statusId)
        {
            var mailMessage = context.TBL_MESSAGE_LOG.Find(messageId);

            if (mailMessage != null)
            {
                mailMessage.MESSAGESTATUSID = (short)statusId;

                var output = context.SaveChanges() > 0;

                if (output)
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }

            return false;
        }

    }
}
