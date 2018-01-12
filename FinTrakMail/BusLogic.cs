using FintrakBanking.Common.Enum;
using FintrakBanking.Entities.Models;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Net.Mail;

namespace FinTrakMail
{
    public class BusLogic
    {
        MailMessage mail = new MailMessage();
        SmtpClient client = new SmtpClient();
        FinTrakBankingContext dbContext = new FinTrakBankingContext();

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


                var listOfMails = dbContext.TBL_MESSAGE_LOG.Where(o => o.MESSAGESTATUSID == (int)MessageStatusEnum.Pending).ToList();

                foreach (var newMail in listOfMails)
                {
                    mail.From = new MailAddress(ConfigurationManager.AppSettings["Username"],"FIRST BANK PLC");

                    if (newMail.TOADDRESS != null && newMail.TOADDRESS != string.Empty)
                    {
                        char[] seperators = { ',', ';' };
                        string[] Addy = newMail.TOADDRESS.Split(seperators);
                       // string[] Addy = "isah.yarima@yahoo.com,anu.omotayo @fintraksoftware.com".Split(seperators);
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

                    client.Send(mail);

                    AuditTrail.LogFileManager.LogToFile("Alert message has been sent to - " + newMail.TOADDRESS + " - Date sent : " + DateTime.Now.ToString());

                    //UPDATE MESSAGE SENT
                    UpdateMessageLogForEmailSent.UpdateMailDeliveryStatus(newMail.MESSAGEID, (int)MessageStatusEnum.Sent);

                }

                return true;
            }
            catch (Exception ex)
            {
                AuditTrail.LogFileManager.LogToFile("Error Occurred - " + ex.Message + " - " + ex.StackTrace.ToString() + DateTime.Now.ToString());
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
                AuditTrail.LogFileManager.LogToFile("Error Occurred - " + ex.Message + " - " + ex.InnerException.ToString() + DateTime.Now.ToString());
                return new List<TBL_MESSAGE_LOG>();
            }
        }

        //public EmailSetup GetEmailSetup()
        //{
        //    try
        //    {

        //            var setup = dbContext.EmailSetups.FirstOrDefault();
        //            if (setup != null)
        //            {
        //                return setup;
        //            }
        //            else
        //            {
        //                return new EmailSetup();
        //            }

        //    }
        //    catch (Exception ex)
        //    {
        //       AuditTrail.LogFileManager.LogToFile("Error Occurred - " + ex.Message + " - " + ex.InnerException.ToString() + DateTime.Now.ToString());
        //        return new EmailSetup();
        //    }
        //}

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
                AuditTrail.LogFileManager.LogToFile("Error Occurred - " + ex.Message + " - " + ex.InnerException.ToString() + DateTime.Now.ToString());
                return false;
            }
        }
    }

}
