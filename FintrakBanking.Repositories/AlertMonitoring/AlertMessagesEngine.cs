using FintrakBanking.Interfaces.AlertMonitoring;
using System;
using System.Configuration;
using System.Net.Mail;

namespace FintrakBanking.Repositories.AlertMonitoring
{
    public class AlertMessagesEngine : IAlertMessagesEngine
    {
       // int mailId = 0;
        private string displayName = ConfigurationManager.AppSettings["emailDisplayName"];
        private string userName = ConfigurationManager.AppSettings["Username"];
        private string password = ConfigurationManager.AppSettings["Password"];
        private string smtpclient = ConfigurationManager.AppSettings["smtpClient"];
        private string enableSsl = ConfigurationManager.AppSettings["enableSsl"];
        private string postNumber = ConfigurationManager.AppSettings["smtpPort"];
        private string testingEmails = ConfigurationManager.AppSettings["testingEmails"];
        private string isTestEmail = ConfigurationManager.AppSettings["isTestEmail"];
        private string exceptionReportingEmails = ConfigurationManager.AppSettings["exceptionReportingEmails"];

        private string[] Addy = { };
        AlertMessageLogger logger = new AlertMessageLogger();
       
        public bool SendEmailOfException(string body)
        {
            using (SmtpClient client = new SmtpClient())
            {
                client.Port = Convert.ToInt32(postNumber);

                client.EnableSsl = Convert.ToBoolean(enableSsl);

                client.Host = smtpclient;

                client.DeliveryMethod = SmtpDeliveryMethod.Network;

                client.UseDefaultCredentials = true;

                client.Credentials = new System.Net.NetworkCredential(userName, password);

                MailMessage mail = new MailMessage();

                mail.From = new MailAddress(userName, "Fintrak Email Service");

                char[] seperators = { ',', ';' };

                Addy = exceptionReportingEmails.Split(seperators);

                foreach (var emailAddy in Addy)
                {
                    if (emailAddy != null && emailAddy != string.Empty)
                    {
                        mail.To.Add(new MailAddress(emailAddy));
                    }
                }
                mail.IsBodyHtml = true;
                mail.Subject = "Fintrak Credit 360 Service - Alert Message logger Exception";
                mail.Body = "Dear Sir/Ma, <br /><br /> ERROR EXCEPTION REPORT <br /><br /> The service has failed with error : " + body + "<br /><br /> Kindly escalate this issue to Fintrak Credit 360 support for urgent attention." +
                    "<br /><br /> Thanks <br /> Fintrak Credit 360.";


                client.Send(mail);


            }
            return true;
        }

        public bool Start()
        {
           var alertSetups = logger.getAlertMessageSetting();

            string title = string.Empty;
            string body = string.Empty;

            //logger.SendAlertsForCovenantsApproachingDueDate(title, body, alertSetups);

            //logger.SendAlertsForCovenantsOverDue(title, body, alertSetups);

            //logger.SendAlertsForExpiredBG(title, body, alertSetups);

            //logger.SendAlertForExpiredInsurance(title, body, alertSetups);

            //logger.SendAlertOnAccountWithExeption_Overdrawn(title, body, alertSetups);

            //logger.SendAlertOnAccountWithExeption_Watchist(title, body, alertSetups);

            //logger.SendAlertOnAccountWithExeption_Unauthorized(title, body, alertSetups);

            //logger.SendAlertOnInsuranceApprochingExpiration(title, body, alertSetups);

            //logger.SendAlertOnPastDueObligationAccounts(title, body, alertSetups);

            //logger.SendAlertOnTurnoverCovenant(title, body, alertSetups);

            //logger.SendAlertsForCollateralPropertyApproachingRevaluation(title, body, alertSetups);

            //logger.SendAlertsForCollateralPropertyDueForVisitation(title, body, alertSetups);

            //logger.SendAlertsOnExpiredActiveBondAndGuarantee(title, body, alertSetups);

            //logger.SendAlertsOnInActiveBondAndGuarantee(title, body, alertSetups);

            //logger.SendAlertsOnLoanCASAwithPND(title, body, alertSetups);

            //logger.SendAlertsOnOverDraftLoansAlmostDue(title, body, alertSetups);

            //logger.SendAlertsOnSelfLiquidatingLoanExpiry(title, body, alertSetups);

            //logger.SendAlertsForLoanRepayment(title, body, alertSetups);

            //logger.SendAlertToCustomerForLoanRepaymentApproachingDueDate(title, body, alertSetups);


            return true;
        }

        public bool Stop()
        {
            throw new NotImplementedException();
        }
    }
}
