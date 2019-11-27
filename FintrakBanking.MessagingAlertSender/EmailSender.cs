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
using FintrakBanking.Common.CustomException;
using System.IO;
using FintrakBanking.Entities.DocumentModels;
using Topshelf.Logging;

namespace FintrakBanking.MessagingAlertSender
{
    public class EmailSender 
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
        private  string[] Addy = { };
        private static readonly LogWriter _log = HostLogger.Get<WindowService>();
        FinTrakBankingContext dbContext = new FinTrakBankingContext();
        FinTrakBankingDocumentsContext docContext = new FinTrakBankingDocumentsContext();
       
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

                throw new SecureException("Failed with error : " + innerMessage);
            }
        }

       

        public bool SendEmailOfException(string body)
        {

            //Console.WriteLine("");
            //Console.WriteLine("Send Exception Email");
            //Console.WriteLine("");
            


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

                //Console.WriteLine("");
                //Console.WriteLine("Log all app settings for exception email");
                //Console.WriteLine("");


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
                Console.WriteLine("email body loaded and sending started  ~~~~~~~~~~~~~~~~~");
                Console.WriteLine("");

                try
                {
                    client.Send(mail);

                    Console.WriteLine("");
                    Console.WriteLine("email sent successfully ~~~~~~~~~~~~~~~");
                    Console.WriteLine("");
                }
                catch (Exception ex)
                {
                    Console.WriteLine("");
                    Console.WriteLine("error sending mail ~~~~~~~~~~~~~~~");
                    Console.WriteLine("");

                    throw new SecureException("Error : " + ex);
                }


            }
            return true;
        }

        public bool SendEmails()
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

                    var listOfMails = dbContext.TBL_MESSAGE_LOG.Where(o => o.MESSAGESTATUSID == (short)MessageStatusEnum.Pending 
                    || o.MESSAGESTATUSID == (short)MessageStatusEnum.Attempted).ToList();


                    if (listOfMails !=null)
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
                                mail.Subject = RemoveSpecial(newMail.MESSAGESUBJECT);
                                mail.Body = RemoveSpecial(newMail.MESSAGEBODY);
                                mailId = newMail.MESSAGEID;

                            if (newMail.ATTACHMENTTYPEID != null)
                            {
                                if (newMail.ATTACHMENTTYPEID == (short)AttachementTypeEnum.ContingentTermination)
                                {
                                    List<TBL_MEDIA_LOAN_DOCUMENTS> requestDoc = new List<TBL_MEDIA_LOAN_DOCUMENTS>();
                                    int loanOperationID = Convert.ToInt32(newMail.ATTACHMENTCODE);

                                    requestDoc = docContext.TBL_MEDIA_LOAN_DOCUMENTS.Where(x => x.LOANREVIEWOPERATIONID == loanOperationID)?.ToList();
                                    foreach (var binaryFile in requestDoc)
                                    {
                                        MemoryStream memoryStream = new MemoryStream(binaryFile.FILEDATA);
                                        Attachment attachment = new Attachment(memoryStream, binaryFile.FILENAME);
                                        mail.Attachments.Add(attachment);

                                    }

                                }

                                if (newMail?.ATTACHMENTTYPEID == (short)AttachementTypeEnum.JobRequest)
                                {
                                    List<TBL_MEDIA_JOB_REQUEST_DOCUMENT> requestDoc = new List<TBL_MEDIA_JOB_REQUEST_DOCUMENT>();
                                    
                                    requestDoc = docContext.TBL_MEDIA_JOB_REQUEST_DOCUMENT.Where(x => x.JOBREQUESTCODE == newMail.ATTACHMENTCODE).ToList();
                                    foreach (var binaryFile in requestDoc)
                                    {
                                       MemoryStream memoryStream = new MemoryStream(binaryFile.FILEDATA);
                                       Attachment attachment = new Attachment(memoryStream, binaryFile.FILENAME);
                                       mail.Attachments.Add(attachment);

                                    }

                                }
    

                            }
                            
                            client.Send(mail);
                            UpdateMailDeliveryStatus(newMail.MESSAGEID, (short)MessageStatusEnum.Sent, "Email Sent Successfully");
                        }
                    }
                   
                }
                return true;
            }
            catch (Exception ex)
            {
                //throw new SecureException("Failed with error sending mail: " + ex);
                UpdateMailDeliveryStatus(mailId, (short)MessageStatusEnum.Attempted, "Email sending failed. Error Response : " + ex.Message);
                throw new SecureException("Failed with error sending mail: " + ex);
                //return false;
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
                    mail.MESSAGESTATUSID = (short)MessageStatusEnum.Sent;
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

              //  throw new SecureException("Failed with error : " + innerMessage);
                return false;

            }
        }

        public string RemoveSpecial(string evalstr)
        {
            StringBuilder finalstr = new StringBuilder();
            foreach (char c in evalstr)
            {
                int charassci = Convert.ToInt16(c);
                if (!(charassci >= 33 && charassci <= 47))// special char ???
                    finalstr.Append(c);
            }
            return finalstr.ToString();
        }




    }
}
