using FintrakBanking.Common;
using FintrakBanking.Entities.Models;
using FintrakBanking.Interfaces.AlertMonitoring;
using FintrakBanking.ViewModels.AlertMonitoring;
using FintrakBanking.ViewModels.Setups.General;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FintrakBanking.Repositories.AlertMonitoring
{
    public class EmailAlertLogger : IEmailAlertLogger
    {
         FinTrakBankingContext context = new FinTrakBankingContext();
        public void ComposeEmail(string referenceNumber, string emailBody, string emailSubject, string recipientEmail, bool callSaveChanges)
        {
            string referenceNo = referenceNumber;

            var emailMessageBody = emailBody;
            string templateUrl = @"~/EmailTemplates/Monitoring.html";
            //string mailBody = EmailHelpers.PopulateBody(emailMessageBody, templateUrl);
            string mailBody = emailMessageBody;

            MessageLogViewModel messageModel = new MessageLogViewModel
            {
                MessageSubject = emailSubject,
                MessageBody = mailBody,
                MessageStatusId = 1,
                MessageTypeId = 1,
                FromAddress = ConfigurationManager.AppSettings["SupportEmailAddr"],
                ToAddress = $"{recipientEmail.Trim()}",
                DateTimeReceived = DateTime.Now,
                SendOnDateTime = DateTime.Now
            };
            SaveMessageDetails(messageModel, callSaveChanges);
        }
        private void SaveMessageDetails(MessageLogViewModel model, bool callSaveChanges)
        {
            var message = new TBL_MESSAGE_LOG()
            {
                //MessageId = model.MessageId,
                MESSAGESUBJECT = model.MessageSubject,
                MESSAGEBODY = model.MessageBody,
                MESSAGESTATUSID = model.MessageStatusId,
                MESSAGETYPEID = model.MessageTypeId,
                FROMADDRESS = model.FromAddress,
                TOADDRESS = model.ToAddress,
                DATETIMERECEIVED = model.DateTimeReceived,
                SENDONDATETIME = model.SendOnDateTime
            };

            context.TBL_MESSAGE_LOG.Add(message);

            if (callSaveChanges)
                context.SaveChanges();
        }

    }

}
