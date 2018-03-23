using FintrakBanking.Common;
using FintrakBanking.Common.Enum;
using FintrakBanking.Interfaces.Credit;
using FintrakBanking.ViewModels.ThridPartyIntegration;
using FinTrakBanking.ThirdPartyIntegration.CreditBureau.CRC;
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

        public byte[] GetFullSearchResultInPDF(SearchInput searchInput)
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
                    return GetXDSPDFCommercialFullCreditReport(searchInput);
                }

                if (searchInput.searchType == (int)CreditBureauTypeEnum.ConsumerSearch)
                {
                    return GetXDSPDFConsumerFullCreditReport(searchInput);
                }
            }

            return null;
        }


        public CRCSearchResult CRCCreditBureauSearch(CRCRequestViewModel request)
        {
            try
            {
                CRCService crc = new CRCService();

               return  crc.CRCSearchRequest(request);
            }
            catch (Exception ex)
            {

                throw new Exception(ex.Message);
            }
        }
        public CRCSearchResult CRCCreditBureauMerge(MultiHitRequestViewModel request)
        {
            try
            {
                CRCService crc = new CRCService();

                return crc.CRCMergeReport(request);
            }
            catch (Exception ex)
            {

                throw new Exception(ex.Message);
            }
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
                userName = searchInfo .userName ,
                AccountNumber = "", //searchInfo.accountOrRegistrationNumber,
                BusinessName = "FINTRAK", //searchInfo.customerName,
                BusinessRegistrationNumber ="", // searchInfo.accountOrRegistrationNumber,
                EnquiryReason = searchInfo.enquiryReason,
                DataTicket = ticket,
                ProductID = searchInfo.productId
            };
            result = xds.ConnectCommercialMatch(data);
            xdoc.LoadXml(result);
            result = new CreditBureauHelp().ConvertXmlToJson(xdoc);


            return result;
        }

        private string DoXDSIndividualSearch(CreditBureauSearchViewModel searchInfo)
        {
            string result = string.Empty;
            XDSService xds = new  XDSService();
            var data = new XDSIndividualSearchViewModel
            {
                userName = searchInfo.userName,
                AccountNumber = "", //searchInfo.accountOrRegistrationNumber,
                ConsumerName = "Ogbonnaya", // searchInfo.customerName,
                DateOfBirth = "", //searchInfo.dateOfBirth,
                Identification = "", // searchInfo.identification,
                EnquiryReason = searchInfo.enquiryReason,
                DataTicket = ticket,
                ProductID = searchInfo.productId
            };
            result = xds.ConnectConsumerMatch(data);
            xdoc.LoadXml(result);
            result = new CreditBureauHelp().ConvertXmlToJson(xdoc);
            return result;
        }

        private byte[] GetXDSPDFCommercialFullCreditReport(SearchInput searchInput)
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

                return xds.GetCommercialFullCreditReportBinary(data);
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        private byte[] GetXDSPDFConsumerFullCreditReport(SearchInput searchInput)
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

                return xds.GetConsumerFullCreditReportBinary(data);
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

    }


}
