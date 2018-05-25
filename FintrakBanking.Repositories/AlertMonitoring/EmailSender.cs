using FintrakBanking.Interfaces.AlertMonitoring;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FintrakBanking.Entities.Models;
using System.Configuration;
using System.Net.Mail;
using FintrakBanking.Common.Enum;

namespace FintrakBanking.Repositories.AlertMonitoring
{
    public class EmailSender : IEmailSender
    {
        int mailId = 0;

        public List<TBL_MESSAGE_LOG> GetMaillingList()
        {
            try
            {
                FinTrakBankingContext dbContext = new FinTrakBankingContext();

                var mails = dbContext.TBL_MESSAGE_LOG.Where(p => p.MESSAGESTATUSID == (int)MessageStatusEnum.Sent).Take(100).ToList();
                if (mails != null)
                {
                    return mails;
                }
                else
                {
                    return new List<TBL_MESSAGE_LOG>();
                }

            }
            catch (Exception ex)
            {
                return new List<TBL_MESSAGE_LOG>();
            }
        }

        public bool SendEmailCompleted()
        {
            return true;
        }

        public bool SendMail()
        {
            FinTrakBankingContext dbContext = new FinTrakBankingContext();

            try
            {
                using (SmtpClient client = new SmtpClient())
                {
                    client.Port = Convert.ToInt32(ConfigurationManager.AppSettings["smtpPort"]);

                    client.EnableSsl = Convert.ToBoolean(ConfigurationManager.AppSettings["enableSsl"]);

                    client.Host = ConfigurationManager.AppSettings["smtpClient"];

                    client.DeliveryMethod = SmtpDeliveryMethod.Network;

                    client.UseDefaultCredentials = true;

                    client.Credentials = new System.Net.NetworkCredential(ConfigurationManager.AppSettings["Username"], ConfigurationManager.AppSettings["Password"]);

                    var listOfMails = dbContext.TBL_MESSAGE_LOG.Where(o => o.MESSAGESTATUSID == (int)MessageStatusEnum.Pending || o.MESSAGESTATUSID == (int)MessageStatusEnum.Attempted).ToList();

                    foreach (var newMail in listOfMails)
                    {
                        MailMessage mail = new MailMessage();

                        mail.From = new MailAddress(ConfigurationManager.AppSettings["Username"], "FBN Fintrak Credit 360");

                        if (newMail.TOADDRESS != null && newMail.TOADDRESS != string.Empty)
                        {
                            char[] seperators = { ',', ';' };
                            //string[] Addy = newMail.TOADDRESS.Split(seperators);
                            string[] Addy = "isah.yarima@yahoo.com,anu.omotayo @fintraksoftware.com".Split(seperators);
                            foreach (var emailAddy in Addy)
                            {
                                if (emailAddy != null && emailAddy != string.Empty)
                                {
                                    mail.To.Add(new MailAddress(emailAddy));
                                }
                            }
                        }

                        mail.IsBodyHtml = true;
                        mail.Subject = newMail.MESSAGESUBJECT;
                        mail.Body = newMail.MESSAGEBODY;
                        mailId = newMail.MESSAGEID;

                        client.Send(mail);
                        UpdateMailDeliveryStatus(newMail.MESSAGEID, (int)MessageStatusEnum.Sent, "Email Sent Successfully");
                    }
                }
                return true;
            }
            catch (Exception ex)
            {
                UpdateMailDeliveryStatus(mailId, (int)MessageStatusEnum.Attempted, "Email sending failed. Error Response : " + ex.Message);

                return false;
            }
        }

        public bool UpdateMailDeliveryStatus(int messageId, short statusId, string response)
        {
            FinTrakBankingContext dbContext = new FinTrakBankingContext();
            var mailMessage = dbContext.TBL_MESSAGE_LOG.Find(messageId);

            if (mailMessage != null)
            {
                mailMessage.MESSAGESTATUSID = (short)statusId;

                mailMessage.DATETIMESENT = DateTime.Now;

                mailMessage.GATEWAYRESPONSE = response;

                var output = dbContext.SaveChanges() > 0;

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

        public bool UpdateMailStatus(int ID)
        {
            try
            {
                FinTrakBankingContext dbContext = new FinTrakBankingContext();
                var mail = dbContext.TBL_MESSAGE_LOG.Where(p => p.MESSAGEID == ID).FirstOrDefault();
                if (mail != null)
                {
                    mail.MESSAGESTATUSID = (int)MessageStatusEnum.Sent;
                    dbContext.SaveChanges();
                    return true;
                }
                else
                {
                    return false;
                }

            }
            catch (Exception ex)
            {
                return false;
            }
        }
    }
}
