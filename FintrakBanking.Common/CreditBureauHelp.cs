using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Linq;

namespace FintrakBanking.Common
{
    public class CreditBureauHelp
    {
        public object XDSWebService { get; private set; }
        public string Ticket(string ticket)
        {

            return $" <string xmlns='{XDSWebService.ToString()}'>{ticket}</string>";
        }

        public string CBNApprovedEnquiryReason()
        {
            string str = "Application for credit by a borrower¬Reviewing of existing credit facilities¬Opening of new accounts (as part of KYC principle)¬Funds transfer of N 1, 000,000 (One Million Naira) and above¬Prospective/current employee checks¬Tenancy contracts (for identification purposes)¬Grant/review of insurance policies¬Acceptance of guarantee(s)¬Application for contracts/pre-paid services (telephone etc)¬Court judgement¬Credit scoring of the client by credit bureau¬A written consent from the client¬Legislation ";
            return str;
        }
        //public string FilePath()
        //{
        //    string folderName = string.Empty;
        //    string subFolderName = "CreditBureau";
        //    string rootPath = new DirectoryInfo(Environment.CurrentDirectory).Parent.Parent.FullName;
        //    folderName = Path.Combine(rootPath, subFolderName);
        //    return folderName;
        //}

        public   string ConvertXmlToJson(XmlDocument xmldoc)
        {
            string json = JsonConvert.SerializeXmlNode(xmldoc);
            return json;
        }
        public   XDocument ConvertJsonToXml(string jsonDoc)
        {
            XDocument xmldoc = JsonConvert.DeserializeXNode(jsonDoc);
            return xmldoc;
        }
    }


}
