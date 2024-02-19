using FintrakBanking.Common;
using FintrakBanking.Common.CustomException;
using FintrakBanking.Common.Enum;
using FintrakBanking.ViewModels.ThridPartyIntegration; 
//using FinTrakBanking.ThirdPartyIntegration.CRCWebService;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Linq;

namespace FinTrakBanking.ThirdPartyIntegration.CreditBureau.CRC
{
   public  class CRCService
    {
        const string DATA_PACKET = "DATAPACKET";
        const string ERROR = "ERROR-LIST";

        public CRCSearchResult CRCSearchRequest(CRCRequestViewModel request)
        {
          
            XElement xml = null;
            if (request.reportID == (int)CRCSearchTypeEnum.CommercialSearch)
            {
                switch (request.searchTypeCode)
                {
                    case 0: xml = CRCCorperateRequestXML0(request); break;

                    case 3:
                        xml = CRCCorperateRequestXML1(request); break;
                }
            }

            if (request.reportID == (int)CRCSearchTypeEnum.ConsumerSearch)
            {
                switch (request.searchTypeCode)
                {
                    case 0: xml = CRCRequestXML0(request); break;

                    case 4: xml = CRCRequestXML4(request); break;

                    case 5: xml = CRCRequestXML5(request); break;

                    case 6: xml = CRCRequestXML6(request); break;
                }
            }

            return SearchOutput(request.userName, request.password, xml);

          
    }

        public CRCSearchResult CRCMergeReport(MultiHitRequestViewModel request)
        {
            try
            {
                CRCSearchResult result = null;
                XElement xml = CRCMergeRequestXML(request);
                if (xml != null)
                {
                    result = SearchOutput(request.userName, request.password, xml);
                }

                return result;
            }
            catch (ConditionNotMetException ex)
            {
                throw new ConditionNotMetException(ex.ToString());
            }
            catch (APIErrorException ex)
            {
                throw new APIErrorException(ex.ToString());
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public string CRCMergeDirectReport(MultiHitRequestViewModel request)
        {
            try
            {
                string result = string.Empty;
                XElement xml = CRCMergeRequestXML(request);
                if (xml != null)
                {
                    result = SearchMergedOutput(request.userName, request.password, xml);
                }

                return result;
            }
            catch (ConditionNotMetException ex)
            {
                throw new ConditionNotMetException(ex.ToString());
            }
            catch (APIErrorException ex)
            {
                throw new APIErrorException(ex.ToString());
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }


        private CRCSearchResult SearchOutput(string userName, string password,   XElement xml)
        {
            XmlDocument xdoc = new XmlDocument();
            CRCSearchResult result = null;

            //LiveRequestInvokerSoapClient crc = new LiveRequestInvokerSoapClient();
            CRCWebServiceNew.LiveRequestInvokerSoapClient crc = new CRCWebServiceNew.LiveRequestInvokerSoapClient();
            string dataPacket = crc.PostRequest(xml.ToString(),  userName,  password);

            if (dataPacket != null) {
                if (dataPacket.Contains(DATA_PACKET))
                {
                    if (!dataPacket.Contains(ERROR))
                    {

                        xdoc.LoadXml(dataPacket);
                        result = new CRCSearchResult
                        {
                            SearchCompleted = (int)SearchCompletedStatusEnum.SearchIncomplete,
                            SearchResult = new CreditBureauHelp().ConvertXmlToJson(xdoc)
                        };
                    }
                    else
                    {
                        result = new CRCSearchResult
                        {
                            SearchCompleted = (int)SearchCompletedStatusEnum.SearchError,
                            SearchResult = dataPacket
                        };
                    }
                }
                else
                {
                    result = new CRCSearchResult
                    {
                        SearchCompleted = (int)SearchCompletedStatusEnum.SearchCompleted,
                        SearchResult = dataPacket
                    };
                };
            }
            else {
                result = new CRCSearchResult
                {
                    SearchCompleted = (int)SearchCompletedStatusEnum.SearchIncomplete,
                    SearchResult = dataPacket
                };
            }

            
            return result;
        }

        private string SearchMergedOutput(string userName, string password, XElement xml)
        {
            //LiveRequestInvokerSoapClient crc = new LiveRequestInvokerSoapClient();
            CRCWebServiceNew.LiveRequestInvokerSoapClient crc = new CRCWebServiceNew.LiveRequestInvokerSoapClient();
            string dataPacket = crc.PostRequest(xml.ToString(), userName, password);
            return dataPacket;
        }


        private XElement CRCMergeRequestXML(MultiHitRequestViewModel request)
        {
            var subjectType = 0;
            if (request.reportID == (int)CRCSearchTypeEnum.ConsumerSearch)
                subjectType = 1;

            return new XElement("REQUEST", new XAttribute("REQUEST_ID", 1),
                new XElement("REQUEST_PARAMETERS",
                new XElement("REPORT_PARAMETERS", new XAttribute("RESPONSE_TYPE", request.responseType),
                new XAttribute("SUBJECT_TYPE", subjectType), new XAttribute("REPORT_ID", request.reportID)),
                new XElement("INQUIRY_REASON", new XAttribute("CODE", request.enquiryReason)),
                new XElement("APPLICATION", new XAttribute("CURRENCY", request.currencyCode),
                new XAttribute("AMOUNT", request.amount), new XAttribute("NUMBER", request.number),
                new XAttribute("PRODUCT", "017")), 
                new XElement("REQUEST_REFERENCE", new XAttribute("REFERENCE-NO", request.referenceNo)),
                new XElement("MERGE_REPORT", new XAttribute("PRIMARY-BUREAU-ID", request.bureauID.FirstOrDefault()),
                from i in request.bureauID select new XElement("BUREAU_ID", i))));

        }

        private XElement CRCCorperateRequestXML0(CRCRequestViewModel request)
        {
            return new XElement("REQUEST", new XAttribute("REQUEST_ID", 1),
                            new XElement("REQUEST_PARAMETERS",
                                    new XElement("REPORT_PARAMETERS", new XAttribute("RESPONSE_TYPE", request.responseType),
                                    new XAttribute("SUBJECT_TYPE", 0), new XAttribute("REPORT_ID", request.reportID)),
                                    new XElement("INQUIRY_REASON", new XAttribute("CODE", request.enquiryReason)),
                                    new XElement("APPLICATION", new XAttribute("CURRENCY", request.currencyCode),
                                    new XAttribute("AMOUNT", request.amount), new XAttribute("NUMBER", request.number),
                                    new XAttribute("PRODUCT", request.productCode)
                                    )),
                                    new XElement("SEARCH_PARAMETERS", new XAttribute("SEARCH-TYPE", 0),
                                    new XElement("NAME", request.customerName)
                                     ));
        }

        private XElement CRCCorperateRequestXML1(CRCRequestViewModel request)
        {
            return new XElement("REQUEST", new XAttribute("REQUEST_ID", 1),
                            new XElement("REQUEST_PARAMETERS",
                                    new XElement("REPORT_PARAMETERS", new XAttribute("RESPONSE_TYPE", request.responseType),
                                    new XAttribute("SUBJECT_TYPE", 0), new XAttribute("REPORT_ID", request.reportID)),
                                    new XElement("INQUIRY_REASON", new XAttribute("CODE",  request.enquiryReason)),
                                    new XElement("APPLICATION", new XAttribute("CURRENCY", request.currencyCode),
                                    new XAttribute("AMOUNT", request.amount), new XAttribute("NUMBER",request.number),
                                    new XAttribute("PRODUCT", request.productCode)
                                    )),
                                    new XElement("SEARCH_PARAMETERS", new XAttribute("SEARCH-TYPE", 3),
                                    new XElement("BUSINESS_REG_NO", request.accountOrRegistrationNumber)
                                     ));

        }


        private XElement CRCRequestXML0(CRCRequestViewModel request)
        {
            return new XElement("REQUEST", new XAttribute("REQUEST_ID", 1),
                            new XElement("REQUEST_PARAMETERS",
                                    new XElement("REPORT_PARAMETERS", new XAttribute("RESPONSE_TYPE", request.responseType),
                                    new XAttribute("SUBJECT_TYPE", 1), new XAttribute("REPORT_ID", request.reportID)),
                                    new XElement("INQUIRY_REASON", new XAttribute("CODE", request.enquiryReason)),
                                    new XElement("APPLICATION", new XAttribute("CURRENCY", request.currencyCode),
                                    new XAttribute("AMOUNT", request.amount), new XAttribute("NUMBER", request.number),
                                    new XAttribute("PRODUCT", request.productCode)
                                    )),
                                    new XElement("SEARCH_PARAMETERS", new XAttribute("SEARCH-TYPE", 0),
                                    new XElement("NAME", request.customerName),
                                    new XElement("SURROGATES", new XElement("GENDER",
                                    new XAttribute("VALUE", request.genderCode)),
                                    new XElement("DOB", new XAttribute("VALUE", request.dateOfBirth)))
                                            ));

        }

        private XElement CRCRequestXML4(CRCRequestViewModel request)
        {
            return new XElement("REQUEST", new XAttribute("REQUEST_ID", 1),
                            new XElement("REQUEST_PARAMETERS",
                                    new XElement("REPORT_PARAMETERS", new XAttribute("RESPONSE_TYPE", request.responseType),
                                    new XAttribute("SUBJECT_TYPE", 1), new XAttribute("REPORT_ID", request.reportID)),
                                    new XElement("INQUIRY_REASON", new XAttribute("CODE", request.enquiryReason)),
                                    new XElement("APPLICATION", new XAttribute("CURRENCY", request.currencyCode),
                                    new XAttribute("AMOUNT", request.amount), new XAttribute("NUMBER", request.number),
                                    new XAttribute("PRODUCT", "017")
                                    )),
                            new XElement("SEARCH_PARAMETERS", new XAttribute("SEARCH-TYPE", 4),
                            new XElement("BVN_NO", request.identification)));
        }

        private XElement CRCRequestXML5(CRCRequestViewModel request)
        {
            return new XElement("REQUEST", new XAttribute("REQUEST_ID", 1),
                            new XElement("REQUEST_PARAMETERS",
                                    new XElement("REPORT_PARAMETERS", new XAttribute("RESPONSE_TYPE", request.responseType),
                                    new XAttribute("SUBJECT_TYPE", 1), new XAttribute("REPORT_ID", request.reportID)),
                                    new XElement("INQUIRY_REASON", new XAttribute("CODE", request.enquiryReason)),
                                    new XElement("APPLICATION", new XAttribute("CURRENCY", request.currencyCode),
                                    new XAttribute("AMOUNT", request.amount), new XAttribute("NUMBER", request.number),
                                    new XAttribute("PRODUCT", "017")
                                    )),
                new XElement("SEARCH_PARAMETERS", new XAttribute("SEARCH-TYPE", 5),
                new XElement("TELEPHONE_NO", request.phoneNumber)));
        }

        private XElement CRCRequestXML6(CRCRequestViewModel request)
        {
            return new XElement("REQUEST", new XAttribute("REQUEST_ID", 1),
                             new XElement("REQUEST_PARAMETERS",
                                     new XElement("REPORT_PARAMETERS", new XAttribute("RESPONSE_TYPE", 2),
                                     new XAttribute("SUBJECT_TYPE", 1), new XAttribute("REPORT_ID", request.reportID)),
                                     new XElement("INQUIRY_REASON", new XAttribute("CODE", request.enquiryReason)),
                                     new XElement("APPLICATION", new XAttribute("CURRENCY", request.currencyCode),
                                     new XAttribute("AMOUNT", request.amount), new XAttribute("NUMBER", request.number),
                                     new XAttribute("PRODUCT", request.productCode)
                                     )),
                 new XElement("SEARCH_PARAMETERS", new XAttribute("SEARCH-TYPE", 6),
                 new XElement("NAME", request.customerName),
                 new XElement("SURROGATES", new XElement("GENDER",
                 new XAttribute("VALUE", request.genderCode)),
                 new XElement("DOB", new XAttribute("VALUE", request.dateOfBirth))),
                 new XElement("BVN_NO", request.identification),
                  new XElement("TELEPHONE_NO", request.phoneNumber)
                 ));
        }
    }
}
