using System;
using FintrakBanking.Interfaces.AppEmail;
using FintrakBanking.Common;
using System.Net.Mail;
using System.Text;
using System.Net;
//using System.ComponentModel.Composition;

namespace FintrakBanking.Repositories.AppEmail
{
    //[Export(typeof(IEmailRepository))]
    //[PartCreationPolicy(CreationPolicy.NonShared)]
    public class EmailRepository : IEmailRepository
    {
        const string EMAIL_USERNAME = "mail@fintraksoftware.com";
        const string EMAIL_PASSWORD = "p@ssw0rd";
        


        public void sendMail(string to, string from, string cc, string bcc, string subject, string message)
        {
            MailMessage   mailMessage = new MailMessage();
            mailMessage.To.Add(to);
            mailMessage.From = new MailAddress(from);
            if (!string.IsNullOrWhiteSpace(cc))
            {
                mailMessage.CC.Add (new MailAddress(cc));
            }

            if (!string.IsNullOrWhiteSpace(bcc))
            {
                mailMessage.Bcc.Add(new MailAddress(bcc));
            }

            mailMessage.Subject = subject;
            // var bodyBuilder = new  BodyBuilder();
            mailMessage.IsBodyHtml = true;
            mailMessage.Body  = message;
            mailMessage.BodyEncoding = Encoding.ASCII;

            
            SmtpClient mClient = new SmtpClient();
            mClient.Host = CommonHelpers.SmtpClientMail;
            mClient.Credentials = new NetworkCredential(EMAIL_USERNAME, EMAIL_PASSWORD);
            mClient.DeliveryMethod = SmtpDeliveryMethod.Network;
            mClient.Timeout = 100000;
            mClient.Send(mailMessage); 

        }
    }
}