using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FintrakBanking.Common.Enum
{
    public enum CreditBureauEnum
    {
        CRMS = 1,
        XDSCreditBureau = 2,
        CRCCreditBureau = 3,
        CRSCreditBureau = 4
    }
    public enum CreditBureauTypeEnum
    {

        ConsumerSearch = 1,
        CommercialSearch = 2
    }
    public enum XDSConnectProductEnum
    {
        ConsumerSnapCheckReport = 42,
        ConsumerBasicTraceReport = 43,
        ConsumerBasicCreditReport = 44,
        DetailedCreditProfileReport = 45,
        XScoreConsumerFullCreditReport = 50,
        BusinessEnquiryBasicCredit = 46,
        DetailedBusinessEnquiryReport = 47
    }

    public enum SearchCompletedStatusEnum
    {
        SearchIncomplete = 1,
        SearchCompleted = 2,
        SearchError = 3
    }

    public enum CRCSearchTypeEnum
    {
        ConsumerSearch = 6110,
        CommercialSearch = 6112
    }
}