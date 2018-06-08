using System;
using System.Configuration;
using System.IO;
using System.Net.Mail;
using System.Web.Hosting;

namespace FintrakBanking.Common
{
    public class EmailHelpers
    {
        public bool SendMail(string recipient, string additionalRecipient, string messageSubject, string messageContent, string templateUrl)
        {
            var body = PopulateBody(messageContent, templateUrl);

            bool sentMail = false;

            try
            {
                SendHtmlFormattedEmail(recipient, additionalRecipient, messageSubject, body);

                sentMail = true;

                return sentMail;
            }
            catch (Exception ex)
            {
                throw ex;
            }

        }

        public static string PopulateBody(string description, string templateLink)
        {
            string body;

            using (var reader = new StreamReader(HostingEnvironment.MapPath(templateLink) ?? throw new InvalidOperationException()))
            {
                body = reader.ReadToEnd();
            }
            body = body.Replace("{Description}", description);
            return body;
        }

        private void SendHtmlFormattedEmail(string recepientEmail, string additionalRecipients, string subject, string body)
        {
            var smtpClient = new SmtpClient();
            var networkCred = new System.Net.NetworkCredential();
            using (var mailMessage = new MailMessage())
            {
                mailMessage.From = new MailAddress(ConfigurationManager.AppSettings["SupportEmailAddr"]);
                mailMessage.To.Add(new MailAddress(recepientEmail));
                if (additionalRecipients != null)
                {
                    mailMessage.Bcc.Add(new MailAddress(additionalRecipients));
                }
                mailMessage.Subject = subject;
                mailMessage.Body = body;
                mailMessage.IsBodyHtml = true;
                mailMessage.Priority = MailPriority.High;
                smtpClient.Host = ConfigurationManager.AppSettings["smtpClient"];
                smtpClient.EnableSsl = Convert.ToBoolean(ConfigurationManager.AppSettings["enableSsl"]);
                networkCred.UserName = ConfigurationManager.AppSettings["Username"];
                networkCred.Password = ConfigurationManager.AppSettings["Password"];
                smtpClient.UseDefaultCredentials = true;
                smtpClient.Credentials = networkCred;
                smtpClient.Port = int.Parse(ConfigurationManager.AppSettings["smtpPort"]);
                smtpClient.Timeout = 10000;
                smtpClient.Send(mailMessage);
            }
        }
    }

    public class EmailFormViewModel
    {
        public string Sender { get; set; }
        public string Recipient { get; set; }
        public string Body { get; set; }
        public string Subject { get; set; }
    }
}