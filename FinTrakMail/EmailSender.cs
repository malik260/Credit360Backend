using FintrakBanking.Common.Enum;
using FintrakBanking.Entities.Models;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Net.Mail;

namespace FinTrakMail
{
    public class EmailSender
    {
   
        SmtpClient client = new SmtpClient();
        FinTrakBankingContext dbContext = new FinTrakBankingContext();
        int mailId = 0;

        public bool SendMail()
        {

            try
            {
                client.Port = Convert.ToInt32(ConfigurationManager.AppSettings["smtpPort"]);

                client.EnableSsl = Convert.ToBoolean(ConfigurationManager.AppSettings["enableSsl"]);

                client.Host = ConfigurationManager.AppSettings["smtpClient"];

                client.DeliveryMethod = SmtpDeliveryMethod.Network;

                client.UseDefaultCredentials = true;

                client.Credentials = new System.Net.NetworkCredential(ConfigurationManager.AppSettings["Username"], ConfigurationManager.AppSettings["Password"]);

                var listOfMails = dbContext.TBL_MESSAGE_LOG.Where(o => o.MESSAGESTATUSID == (int)MessageStatusEnum.Pending || o.MESSAGESTATUSID == (int)MessageStatusEnum.Attempted).ToList() ;

                foreach (var newMail in listOfMails)
                {
                    MailMessage mail = new MailMessage();

                    mail.From = new MailAddress(ConfigurationManager.AppSettings["Username"],"FBN Fintrak Credit 360");

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
                        UpdateMessageLogForEmailSent.UpdateMailDeliveryStatus(newMail.MESSAGEID, (int)MessageStatusEnum.Sent, "Email Sent Successfully");
                      //  AuditTrail.LogFileManager.LogToFile("Email has been sent to : " + " " + newMail.TOADDRESS + " - " + DateTime.Now.ToString());

                }

                return true;
            }
            catch (Exception ex)
            {
                UpdateMessageLogForEmailSent.UpdateMailDeliveryStatus(mailId, (int)MessageStatusEnum.Attempted, "Email sending failed. Error Response : " + ex.Message);

               // AuditTrail.LogFileManager.LogToFile("Error Occurred - " + ex.Message + " - " + ex.StackTrace.ToString() + " DATE : " + DateTime.Now.ToString());

                return false;
            }
        }

        public List<TBL_MESSAGE_LOG> GetMaillingList()
        {
            try
            {

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
              //  AuditTrail.LogFileManager.LogToFile("Error Occurred - " + ex.Message + " - " + ex.InnerException.ToString() + DateTime.Now.ToString());
                return new List<TBL_MESSAGE_LOG>();
            }
        }

       
        public bool UpdateMailStatus(int ID)
        {
            try
            {

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
             //   AuditTrail.LogFileManager.LogToFile("Error Occurred - " + ex.Message + " - " + ex.InnerException.ToString() + DateTime.Now.ToString());
                return false;
            }
        }
    }

}
