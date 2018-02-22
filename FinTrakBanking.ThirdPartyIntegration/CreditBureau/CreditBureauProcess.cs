using FintrakBanking.Common;
using FintrakBanking.Common.Enum;
using FintrakBanking.Interfaces.Credit;
using FintrakBanking.ViewModels.ThridPartyIntegration;
using FinTrakBanking.ThirdPartyIntegration.CreditBureau.XDS;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace FinTrakBanking.ThirdPartyIntegration.CreditBureau
{
    public class CreditBureauProcess : ICreditBureauProcess
    {
        XmlDocument xdoc = new XmlDocument();
        string ticket = string.Empty;

        public string XDSSearchCreditBureau(CreditBureauSearchViewModel searchInfo)
        {
            var xds = new XDSService();

            if (!xds.IsticketActive(searchInfo.userName))
            {
                xds.Login(searchInfo.userName, searchInfo.password);
            }

            if (searchInfo.creditBureauId == (short)CreditBureauEnum.XDSCreditBureau)
            {
                if (searchInfo.searchType == (int)CreditBureauTypeEnum.CommercialSearch)
                {
                    var ticketState = xds.IsticketActive(searchInfo.userName);

                    return DoXDSCommercialSearch(searchInfo);
                }

                if (searchInfo.searchType == (int)CreditBureauTypeEnum.ConsumerSearch)
                {
                    return DoXDSIndividualSearch(searchInfo);
                }
            }

            if (searchInfo.creditBureauId == (short)CreditBureauEnum.CRCCreditBureau)
            {

            }
            return "";
        }
        public List<dynamic> GetApprovedSearchReasons()
        {
            var xds = new XDSService();
            return xds.GetApprovedReasons();
        }
        public string GetFullSearchResult(SearchInput searchInput)
        {
            var xds = new XDSService();

            if (!xds.IsticketActive(searchInput.userName))
            {
                xds.Login(searchInput.userName, searchInput.password);
            }
            if (searchInput.creditBureauId == (short)CreditBureauEnum.XDSCreditBureau)
            {
                if (searchInput.searchType == (int)CreditBureauTypeEnum.CommercialSearch)
                {
                    return GetXDSCommercialFullCreditReport(searchInput);
                }

                if (searchInput.searchType == (int)CreditBureauTypeEnum.ConsumerSearch)
                {
                    return GetXDSConsumerFullCreditReport(searchInput);
                }
            }       

            return null;
        }

        private string GetXDSCommercialFullCreditReport(SearchInput searchInput)
        {
            try
            {
                string result = string.Empty;
                string mergeLst = MergeListToString(searchInput.mergeList);
                XDSService xds = new XDSService();
                var data = new SearchFullResultViewModel
                {
                    ConsumerID = searchInput.consumerID,
                    MergeList = mergeLst,
                    DataTicket = string.Empty,
                    EnquiryID = searchInput.enquiryID,
                    SubscriberEnquiryEngineID = searchInput.subscriberEnquiryEngineID
                };

                return xds.GetCommercialFullCreditReport(data);
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }
        private string GetXDSConsumerFullCreditReport(SearchInput searchInput)
        {
            try
            {
                string result = string.Empty;
                string mergeLst = MergeListToString(searchInput.mergeList);
                XDSService xds = new XDSService();
                var data = new SearchFullResultViewModel
                {
                    ConsumerID = searchInput.consumerID,
                    MergeList = mergeLst,
                    DataTicket = string.Empty,
                    EnquiryID = searchInput.enquiryID,
                    SubscriberEnquiryEngineID = searchInput.subscriberEnquiryEngineID
                };

                return xds.GetConsumerFullCreditReport(data);
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }
        private string MergeListToString(List<int> mergeId)
        {
            string str = string.Empty;
            foreach(var item in mergeId )
            {
                str += item.ToString() + ",";
            }
            return str;
        }
        private string DoXDSCommercialSearch(CreditBureauSearchViewModel searchInfo)
        {
            string result = string.Empty;
            XDSService xds = new XDSService();

            var data = new XDSCommercialSearchViewModel
            {
                AccountNumber = searchInfo.accountOrRegistrationNumber,
                BusinessName = searchInfo.customerName,
                BusinessRegistrationNumber = searchInfo.accountOrRegistrationNumber,
                EnquiryReason = searchInfo.enquiryReason,
                DataTicket = ticket,
                 ProductID = searchInfo.productId
            };
            result = xds.ConnectCommercialMatch(data);
            xdoc.Load(result);
            result = new CreditBureauHelp().ConvertXmlToJson(xdoc);


            return result;
        }
        private string DoXDSIndividualSearch(CreditBureauSearchViewModel searchInfo)
        {
            string result = string.Empty;
            XDSService xds = new XDSService();
            var data = new XDSIndividualSearchViewModel
            {
                AccountNumber = searchInfo.accountOrRegistrationNumber,
                ConsumerName = searchInfo.customerName,
                DateOfBirth = searchInfo.dateOfBirth,
                Identification = searchInfo.identification,
                EnquiryReason = searchInfo.enquiryReason,
                DataTicket = ticket,
                ProductID = searchInfo.productId
            };
            result = xds.ConnectConsumerMatch(data);
            xdoc.Load(result);
            result = new CreditBureauHelp().ConvertXmlToJson(xdoc);
            return result;
        }


    }

  
}
