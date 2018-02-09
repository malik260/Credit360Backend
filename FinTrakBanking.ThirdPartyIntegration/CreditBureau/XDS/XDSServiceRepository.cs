using FintrakBanking.Common;
using FintrakBanking.Interfaces.ThridPartyIntegration;
using FintrakBanking.ViewModels.ThridPartyIntegration;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace FinTrakBanking.ThirdPartyIntegration.CreditBureau.XDS
{
    public class XDSServiceRepository : IXDSServiceRepository
    {
        XDSWebService.XDSNigeriaWebServiceSoapClient proxy = new XDSWebService.XDSNigeriaWebServiceSoapClient();
        CreditBureauHelp helper = new CreditBureauHelp();
        
        
        string ticket = string.Empty;
        string pathString = string.Empty;

        private string GetStoredTicket(string userName)
        {
            try
            {
                pathString = helper.FilePath();
                using (TextReader tr = new StreamReader(pathString))
                {
                    ticket = tr.ReadLine();
                }
            }
            catch (IOException e)
            {
                Console.WriteLine(e.Message);
            }
            return ticket;
        }

        private void StoredTicket(string userName, string ticket)
        {
            string folderName = string.Empty;
            folderName = pathString = helper.FilePath();
            pathString = Path.Combine(folderName, userName);

            if (!Directory.Exists(folderName))
            {
                Directory.CreateDirectory(folderName);
            }
            
            if (!File.Exists(pathString))
            {
                using (StreamWriter sw = new StreamWriter(pathString))
                {
                    sw.Write(ticket);
                }
                return;
            }
            else
            {
                DistroyTicket(pathString);
                StoredTicket(userName, ticket);
            }
        }

        private void DistroyTicket(string pathString)
        {
            try
            {
                System.IO.File.Delete(pathString);
            }
            catch (System.IO.IOException e)
            {
                throw new Exception(e.Message);
                
            }
        }

        public string ConnectConsumerMatch(IndividualSearchViewModel param)
        {
            string result;
            XmlDocument xmDoc = new XmlDocument();
            //string reason = GetApprovedReasons().FirstOrDefault().reason;
            var data = new IndividualSearchViewModel
            {
                DataTicket = GetStoredTicket("fir58767"),
                EnquiryReason = param.EnquiryReason,
                ProductID = param.ProductID,
                AccountNumber = param.AccountNumber,
                ConsumerName = param.ConsumerName,
                DateOfBirth = param.DateOfBirth,
                Identification = param.Identification

            };
            result = proxy.ConnectConsumerMatch(data.DataTicket, data.EnquiryReason, data.ConsumerName, data.DateOfBirth, data.Identification, data.AccountNumber, data.ProductID);
            xmDoc.LoadXml(result);
            result = helper.ConvertXmlToJson(xmDoc);
            return result;
        }

        public string Login(string userName, string password)
        {
            var result = proxy.Login(userName, password);
            StoredTicket(userName, result);
            return result;
        }

        public bool IsticketActive(string userName, string password)
        {
            string ticket = GetStoredTicket(userName);
            return proxy.IsTicketValid(ticket);
        }

        public List<dynamic> GetApprovedReasons()
        {
            var approvedReasonLst = new List<dynamic>();
            string[] approvedReason = helper.CBNApprovedEnquiryReason().Trim().Split('¬').ToArray();

            int count = 1;
            foreach (var item in approvedReason)
            {
                dynamic dat = new
                {
                    id = count,
                    reason = item
                };
                approvedReasonLst.Add(dat);
                ++count;
            }
            return approvedReasonLst;
        }



    }
}
