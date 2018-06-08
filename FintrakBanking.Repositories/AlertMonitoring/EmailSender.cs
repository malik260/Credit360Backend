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
        private string displayName = ConfigurationManager.AppSettings["emailDisplayName"];
        private string userName = ConfigurationManager.AppSettings["Username"];
        private string password = ConfigurationManager.AppSettings["Password"];
        private string smtpclient = ConfigurationManager.AppSettings["smtpClient"];
        private string enableSsl = ConfigurationManager.AppSettings["enableSsl"];
        private string postNumber = ConfigurationManager.AppSettings["smtpPort"];
        private string testingEmails = ConfigurationManager.AppSettings["testingEmails"];
        private string isTestEmail = ConfigurationManager.AppSettings["isTestEmail"];
        private string requireCredential = ConfigurationManager.AppSettings["requireCredential"];
        private string exceptionReportingEmails = ConfigurationManager.AppSettings["exceptionReportingEmails"];
        private IAlertMessagesEngine logger;
        private  string[] Addy = { };

        FinTrakBankingContext dbContext = new FinTrakBankingContext();

        public EmailSender(IAlertMessagesEngine _logger)
        {
            logger = _logger;
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
                var innerException = ex.InnerException;
                string innerMessage = "";
                if (innerException != null)
                    innerMessage = innerException.Message;

                throw new Exception("Failed with error : " + innerMessage);
            }
        }

        public void LogMonitorringAlert()
        {
            logger.Start();
        }
        public bool SendEmailCompleted()
        {
            return true;
        }

        public bool SendEmailOfException(string body)
        {

            Console.WriteLine("");
            Console.WriteLine("Send Excetion Email ");
            Console.WriteLine("");


            using (SmtpClient client = new SmtpClient())
            {
                client.Port = Convert.ToInt32(postNumber);

                client.EnableSsl = Convert.ToBoolean(enableSsl);

                client.Host = smtpclient;

                client.DeliveryMethod = SmtpDeliveryMethod.Network;

                client.UseDefaultCredentials = true;

                if (!string.IsNullOrEmpty(requireCredential))
                {
                    if(Convert.ToBoolean(requireCredential)==true)
                    {
                        client.Credentials = new System.Net.NetworkCredential(userName, password);
                    }

                }

                Console.WriteLine("");
                Console.WriteLine("Log all app settings for exception email");
                Console.WriteLine("");


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
                mail.Subject = "Fintrak Credit 360 Service - Email Alert Sender Exception";
                mail.Body = "Dear Sir/Ma, <br /><br /> ERROR EXCEPTION REPORT <br /><br /> The service has failed with error : " + body + "<br /><br /> Kindly escalate this issue to Fintrak Credit 360 support for urgent attention." +
                    "<br /><br /> Thanks <br /> Fintrak Credit 360.";


                Console.WriteLine("");
                Console.WriteLine("email body loaded and sending stated  ~~~~~~~~~~~~~~~~~");
                Console.WriteLine("");

                try
                {
                    client.Send(mail);

                }
                catch (Exception ex)
                {
                    Console.WriteLine("");
                    Console.WriteLine("error sending mail ~~~~~~~~~~~~~~~");
                    Console.WriteLine("");

                    throw new Exception("Error : " + ex);
                }


            }
            return true;
        }

        public bool LogMonitoringEmailAlerts()
        {
            try
            {

                using (SmtpClient client = new SmtpClient())
                {
                    client.Port = Convert.ToInt32(postNumber);

                    client.EnableSsl = Convert.ToBoolean(enableSsl);

                    client.Host = smtpclient;

                    client.DeliveryMethod = SmtpDeliveryMethod.Network;

                    client.UseDefaultCredentials = true;

                    if (!string.IsNullOrEmpty(requireCredential))
                    {
                        if (Convert.ToBoolean(requireCredential) == true)
                        {
                            client.Credentials = new System.Net.NetworkCredential(userName, password);
                        }

                    }

                    Console.WriteLine("");
                    Console.WriteLine("Log all app settings");
                    Console.WriteLine("");

                    var listOfMails = dbContext.TBL_MESSAGE_LOG.Where(o => o.MESSAGESTATUSID == (int)MessageStatusEnum.Pending 
                    || o.MESSAGESTATUSID == (int)MessageStatusEnum.Attempted).ToList();


                    Console.WriteLine("");
                    Console.WriteLine("Returned records from database : " + listOfMails.Count());
                    Console.WriteLine("");

                    if (listOfMails!=null)
                    {
                        foreach (var newMail in listOfMails)
                        {
                            MailMessage mail = new MailMessage();

                            mail.From = new MailAddress(userName, displayName);

                            if (newMail.TOADDRESS != null && newMail.TOADDRESS != string.Empty)
                            {
                                char[] seperators = { ',', ';' };

                                if (!string.IsNullOrEmpty(isTestEmail))
                                {
                                    if (Convert.ToBoolean(isTestEmail))
                                    {
                                        Addy = testingEmails.Split(seperators);
                                    }
                                    else
                                    {
                                        Addy = newMail.TOADDRESS.Split(seperators);
                                    }
                                }
                                else
                                {
                                    Addy = newMail.TOADDRESS.Split(seperators);
                                }

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

                            Console.WriteLine("");
                            Console.WriteLine("Email Sending started ~~~~~~~~");
                            Console.WriteLine("");

                            try
                            {
                                client.Send(mail);

                            }catch(Exception ex)
                            {
                                throw new Exception("Error : " + ex);
                            }

                            UpdateMailDeliveryStatus(newMail.MESSAGEID, (int)MessageStatusEnum.Sent, "Email Sent Successfully");
                        }
                    }
                   
                }
                return true;
            }
            catch (Exception ex)
            {

                Console.WriteLine("");
                Console.WriteLine("Failed with error ");
                Console.WriteLine("");

                UpdateMailDeliveryStatus(mailId, (int)MessageStatusEnum.Attempted, "Email sending failed. Error Response : " + ex.Message);

                throw new Exception("Failed with error : " + ex.Message);
            }
        }

        public bool UpdateMailDeliveryStatus(int messageId, short statusId, string response)
        {

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
                var innerException = ex.InnerException;
                string innerMessage = "";
                if (innerException != null)
                    innerMessage = innerException.Message;

                throw new Exception("Failed with error : " + innerMessage);
            }
        }
    }
}
