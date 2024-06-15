using FintrakBanking.Common;
using FintrakBanking.Common.CustomException;
using FintrakBanking.Common.Enum;
using FintrakBanking.Entities.Models;
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
    public class XDSService //: IXDSServiceRepository 
    {
        // XDSWebService.FirstCentralNigeriaWebServiceSoapClient proxy = new XDSWebService.FirstCentralNigeriaWebServiceSoapClient();
        //ESBFirstCentralService.FirstCentralNigeriaWebServiceSoapClient proxy = new ESBFirstCentralService.FirstCentralNigeriaWebServiceSoapClient();
        FCBCreditBureau.FirstCentralNigeriaWebServiceSoapClient proxy = new FCBCreditBureau.FirstCentralNigeriaWebServiceSoapClient();
        //for test //FirstCentralCRB.FirstCentralNigeriaWebServiceSoapClient proxy = new FirstCentralCRB.FirstCentralNigeriaWebServiceSoapClient();

        CreditBureauHelp helper = new CreditBureauHelp();
        FinTrakBankingContext innerContext = new FinTrakBankingContext();

        string ticket = string.Empty;
        string pathString = string.Empty;

        private string GetStoredTicket(string userName)
        {
            var token = innerContext.TBL_CREDIT_BUREAU.SingleOrDefault(x => x.CREDITBUREAUID == (short)CreditBureauEnum.XDSCreditBureau).TOKEN;
            return token;
        }

        private void StoredTicket(string userName, string ticket)
        {
            var xdsInfo = innerContext.TBL_CREDIT_BUREAU.SingleOrDefault(x => x.CREDITBUREAUID == (short)CreditBureauEnum.XDSCreditBureau);
                xdsInfo.TOKEN = ticket;
                innerContext.SaveChanges();
            return;
        }

        public string Login(string userName, string password)
        {
            var result = proxy.Login(userName, password);
            StoredTicket(userName, result);
            return result;
        }

        public bool IsticketActive(string userName)
        {
            string ticket = GetStoredTicket(userName);
            if(ticket.Length > 0)
            {
                return proxy.IsTicketValid(ticket);
            }
          return false;
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

        #region Consumer
        public string ConnectConsumerMatch(XDSIndividualSearchViewModel searchParam)
        {
            string result;
            string dataTicket = GetStoredTicket(searchParam.userName);
            result = proxy.ConnectConsumerMatch(dataTicket, searchParam.EnquiryReason, searchParam.ConsumerName,
                searchParam.DateOfBirth, searchParam.Identification, searchParam.AccountNumber, ((int)XDSConnectProductEnum.DetailedCreditProfileReport).ToString());
            return result;
        }

        public string GetConsumerFullCreditReport(SearchFullResultViewModel searchParam)
        {
            string result;
            string dataTicket = GetStoredTicket(searchParam.userName);
            result = proxy.GetConsumerFullCreditReport(dataTicket, searchParam.ConsumerID, searchParam.MergeList, searchParam.SubscriberEnquiryEngineID,
                searchParam.EnquiryID);
            return result;
        }

        public byte[] GetConsumerFullCreditReportBinary(SearchFullResultViewModel searchParam)
        {
            byte[] result;
            string dataTicket = GetStoredTicket(searchParam.userName);
            result = proxy.GetConsumerFullCreditReportBinary(dataTicket, searchParam.ConsumerID, searchParam.MergeList, searchParam.SubscriberEnquiryEngineID,
                searchParam.EnquiryID);
            return result;
        }

        public byte[] GetXSCoreConsumerFullCreditReportBinary(SearchFullResultViewModel searchParam)
        {
            byte[] result;
            string dataTicket = GetStoredTicket(searchParam.userName);
            result = proxy.GetXSCoreConsumerFullCreditReportBinary(dataTicket, searchParam.ConsumerID, searchParam.MergeList, searchParam.SubscriberEnquiryEngineID,
                searchParam.EnquiryID);
            return result;
        }

        public string GetConsumerBasicTraceReport(SearchFullResultViewModel searchParam)
        {
            string result;
            string dataTicket = GetStoredTicket(searchParam.userName);
            result = proxy.GetConsumerBasicTraceReport(dataTicket, searchParam.ConsumerID, searchParam.MergeList, searchParam.SubscriberEnquiryEngineID,
                searchParam.EnquiryID);
            return result;
        }

        public string GetXSCoreConsumerFullCreditReport(SearchFullResultViewModel searchParam)
        {
            string result;
            string dataTicket = GetStoredTicket(searchParam.userName);
            result = proxy.GetXSCoreConsumerFullCreditReport(dataTicket, searchParam.ConsumerID, searchParam.MergeList, searchParam.SubscriberEnquiryEngineID,
                searchParam.EnquiryID);
            return result;
        }
        #endregion

        #region Commercial
        public string ConnectCommercialMatch(XDSCommercialSearchViewModel searchParam)
        {
            string dataTicket = GetStoredTicket(searchParam.userName);
            string result;
            result = proxy.ConnectCommercialMatch(dataTicket, searchParam.EnquiryReason, searchParam.BusinessName,
                searchParam.BusinessRegistrationNumber, searchParam.AccountNumber, ((int)XDSConnectProductEnum.DetailedBusinessEnquiryReport).ToString());
            return result;
        }

        public string GetCommercialFullCreditReport(SearchFullResultViewModel searchParam)
        {
            string result;
            string dataTicket = GetStoredTicket(searchParam.userName);
            result = proxy.GetCommercialFullCreditReport(dataTicket, searchParam.ConsumerID, searchParam.MergeList, searchParam.SubscriberEnquiryEngineID,
                searchParam.EnquiryID);
            return result;
        }

        public string GetCommercialBasicCreditReport(SearchFullResultViewModel searchParam)
        {
            string result;
            string dataTicket = GetStoredTicket(searchParam.userName);
            result = proxy.GetCommercialBasicCreditReport(dataTicket, searchParam.ConsumerID, searchParam.MergeList, searchParam.SubscriberEnquiryEngineID,
                searchParam.EnquiryID);
            return result;
        }

        public string GetCommercialEnquiryReport(SearchFullResultViewModel searchParam)
        {
            string result;
            string dataTicket = GetStoredTicket(searchParam.userName);
            result = proxy.GetCommercialEnquiryReport(dataTicket, searchParam.ConsumerID,"45");
            return result;
        }
        
        public byte[] GetCommercialFullCreditReportBinary(SearchFullResultViewModel searchParam)
        {
            byte[] result;
            string dataTicket = GetStoredTicket(searchParam.userName);
            result = proxy.GetCommercialFullCreditReportBinary(dataTicket, searchParam.ConsumerID, searchParam.MergeList, searchParam.SubscriberEnquiryEngineID,
                searchParam.EnquiryID);
            return result;
        }

        #endregion

    }
}
