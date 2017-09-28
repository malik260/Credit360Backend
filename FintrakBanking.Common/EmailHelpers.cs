using System;
using System.Configuration;
using System.IO;
using System.Net.Mail;
using System.Web.Hosting;

namespace FintrakBanking.Common
{
    public class EmailHelpers
    {
        private void SendMail(string recipient, string messageSubject, string messageContent)
        {
            var body = PopulateBody(messageContent, ConfigurationManager.AppSettings["AppLink"]);

            SendHtmlFormattedEmail(recipient, messageSubject, body);
        }

        private string PopulateBody(string description, string urlLink)
        {
            string body;
            using (var reader = new StreamReader(HostingEnvironment.MapPath("~/EmailTemplates/AssignNewTask.html") ?? throw new InvalidOperationException()))
            {
                body = reader.ReadToEnd();
            }
            body = body.Replace("{Description}", description);
            body = body.Replace("{Link}", urlLink);
            return body;
        }

        public void SendHtmlFormattedEmail(string recepientEmail, string subject, string body)
        {
            var smtpClient = new SmtpClient();
            var networkCred = new System.Net.NetworkCredential();
            using (var mailMessage = new MailMessage())
            {
                mailMessage.From = new MailAddress(ConfigurationManager.AppSettings["SupportEmailAddr"]);
                mailMessage.To.Add(new MailAddress(recepientEmail));
                mailMessage.Subject = subject;
                mailMessage.Body = body;
                mailMessage.IsBodyHtml = true;
                mailMessage.Priority = MailPriority.High;
                smtpClient.Host = ConfigurationManager.AppSettings["Host"];
                smtpClient.EnableSsl = Convert.ToBoolean(ConfigurationManager.AppSettings["EnableSsl"]);
                networkCred.UserName = ConfigurationManager.AppSettings["Username"];
                networkCred.Password = ConfigurationManager.AppSettings["Password"];
                smtpClient.UseDefaultCredentials = true;
                smtpClient.Credentials = networkCred;
                smtpClient.Port = int.Parse(ConfigurationManager.AppSettings["Port"]);
                smtpClient.Send(mailMessage);
            }
        }
    }

    public class EmailFormViewModel
    {
        public string Sender { get; set; }
        public string Secipient { get; set; }
        public string Body { get; set; }
        public string Subject { get; set; }
    }
}