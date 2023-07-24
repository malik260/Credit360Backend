using FintrakBanking.Entities.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FintrakBanking.Interfaces.AlertMonitoring
{
    public interface IEmailSender
    {
        bool SendEmails();
        bool SendEmailOfException(string body);
        bool SendEmailCompleted();
        List<TBL_MESSAGE_LOG> GetMaillingList();
        bool UpdateMailStatus(int ID);
        bool UpdateMailDeliveryStatus(int messageId, short statusId, string response);
        void LogMonitorringAlert();

    }
}
