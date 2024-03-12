namespace FinTrakBanking.ThirdPartyIntegration
{
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
    using FintrakBanking.Common.CustomException;
    using System.Web.UI.WebControls;


    namespace CreditBureau
    {

        public class CreditBureauProcess : ICreditBureauProcess
        {
            XmlDocument xdoc = new XmlDocument();
            string ticket = string.Empty;

            public string XDSSearchCreditBureau(CreditBureauSearchViewModel searchInfo)
            {
                try
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
                            ticket = xds.Login(searchInfo.userName, searchInfo.password);
                            return DoXDSIndividualSearch(searchInfo);
                        }
                    }

                    if (searchInfo.creditBureauId == (short)CreditBureauEnum.CRCCreditBureau)
                    {

                    }

                    return "";
                }
                catch (Exception ex)
                {
                    var innerExceptionMessage = "";
                    if (ex.InnerException != null)
                        innerExceptionMessage = ex.InnerException.Message;

                    throw new APIErrorException($"Core Banking Credit Bureau API Error - {ex.Message} - inner exception - {innerExceptionMessage}");
                }
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

                if (searchInput.creditBureauId == (short) CreditBureauEnum.XDSCreditBureau)
                {
                    if (searchInput.searchType == (int) CreditBureauTypeEnum.CommercialSearch)
                    {
                        return GetXDSCommercialFullCreditReport(searchInput);
                    }

                    if (searchInput.searchType == (int) CreditBureauTypeEnum.ConsumerSearch)
                    {
                        return GetXDSConsumerFullCreditReport(searchInput);
                    }
                }

                return null;
            }

            public byte[] GetXDSFullSearchResultInPDF(SearchInput searchInput)
            {
                if(searchInput.mergeList.Count() == 0)
                {
                    throw new ConditionNotMetException("There are items in the merge list." +"\n"+" Please select items to matched.");
                }

                var xds = new XDSService();

                if (!xds.IsticketActive(searchInput.userName))
                {
                    ticket = xds.Login(searchInput.userName, searchInput.password);
                }

                if (searchInput.creditBureauId == (short) CreditBureauEnum.XDSCreditBureau)
                {
                    if (searchInput.searchType == (int) CreditBureauTypeEnum.CommercialSearch)
                    {
                        return GetXDSPDFCommercialFullCreditReport(searchInput);
                    }

                    if (searchInput.searchType == (int) CreditBureauTypeEnum.ConsumerSearch)
                    {
                        return GetXDSPDFConsumerFullCreditReport(searchInput);
                    }
                }

                return null;
            }


            public CRCSearchResult CRCCreditBureauSearch(CRCRequestViewModel request)
            {
                if(request.dateOfBirth != null)
                {
                    request.dateOfBirth = Convert.ToDateTime(request.dateOfBirth).ToString("dd-MMM-yyyy");
                     //request.dateOfBirth = "03-Jun-1998";  
                }

                try
                {
                    CRCService crc = new CRCService();

                    return crc.CRCSearchRequest(request);
                }
                catch(TimeoutException ex)
                {
                    throw  new CustomTimeoutException("Connection timed out!");
                }
                catch (Exception ex)
                {

                    throw new SecureException(ex.Message);
                }
            }

            public string CRCCreditBureauMerge(MultiHitRequestViewModel request)
            {
                try
                {
                    CRCService crc = new CRCService();

                    return crc.CRCMergeDirectReport(request);
                }
                catch (TimeoutException ex)
                {
                    throw new TimeoutException( ex.Message);
                }
                catch (ConditionNotMetException ex)
                {
                    throw new ConditionNotMetException(ex.Message);
                }
                catch (APIErrorException ex)
                {
                    throw new APIErrorException(ex.Message);
                }
                catch (Exception ex)
                {

                    throw new SecureException(ex.Message);
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
                    throw new SecureException(ex.Message);
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
                    throw new SecureException(ex.Message);
                }
            }

            private string MergeListToString(List<string> mergeId)
            {
                string str = string.Empty;
                foreach (var item in mergeId)
                {
                    str += item.ToString() + ",";
                }

                return str;
            }
            private int MergeListInt(List<string> mergeId)
            {
                int str = 0;
                foreach (var item in mergeId)
                {
                    str = Int32.Parse(item);
                }

                return str;
            }

            private string DoXDSCommercialSearch(CreditBureauSearchViewModel searchInfo)
            {
                string result = string.Empty;
                XDSService xds = new XDSService();

                var data = new XDSCommercialSearchViewModel
                {
                    userName = searchInfo.userName,
                    AccountNumber = searchInfo.accountOrRegistrationNumber,
                    BusinessName = searchInfo.customerName,
                    BusinessRegistrationNumber = searchInfo.accountOrRegistrationNumber,
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
                XDSService xds = new XDSService();
                if (searchInfo.dateOfBirth == "01-Jan-0001" || searchInfo.dateOfBirth == null) searchInfo.dateOfBirth = string.Empty;
                if (searchInfo.identification == null) searchInfo.identification = string.Empty;
                if (searchInfo.customerName == null) searchInfo.customerName = string.Empty;

                var data = new XDSIndividualSearchViewModel
                {
                    userName = searchInfo.userName,
                    AccountNumber = searchInfo.accountOrRegistrationNumber ?? "",
                    ConsumerName = searchInfo.customerName,
                    DateOfBirth = searchInfo.dateOfBirth,
                    Identification = searchInfo.identification,
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
                        MergeList = mergeLst.TrimEnd(','),
                        DataTicket = string.Empty,
                        EnquiryID = searchInput.enquiryID,
                        SubscriberEnquiryEngineID = searchInput.subscriberEnquiryEngineID
                    };

                    return xds.GetCommercialFullCreditReportBinary(data);
                }
                catch (Exception ex)
                {
                    throw new SecureException(ex.Message);
                }
            }

            private byte[] GetXDSPDFConsumerFullCreditReport(SearchInput searchInput)
            {
                try
                {
                    string result = string.Empty;
                    string mergeLst = MergeListToString(searchInput.mergeList);
                    XDSService xds = new XDSService();
                    var idString = mergeLst.TrimEnd(',');
                    var id = Int32.Parse(idString);
                    var data = new SearchFullResultViewModel
                    {
                        ConsumerID = id,
                        MergeList = mergeLst.TrimEnd(','),
                        DataTicket = string.Empty,
                        EnquiryID = searchInput.enquiryID,
                        SubscriberEnquiryEngineID = searchInput.subscriberEnquiryEngineID
                    };

                    return xds.GetConsumerFullCreditReportBinary(data);
                }
                catch (Exception ex)
                {
                    throw new SecureException(ex.Message);
                }
            }

        }


    }
}