using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FintrakBanking.ViewModels.ThridPartyIntegration
{
    public class TransactionPostingViewModel //: GeneralEntity
    {
        public string accounts { get; set; }
        public string amounts { get; set; }
        public string narration { get; set; }
        public string referenceNumber { get; set; }
        public string currencyType { get; set; }
        public string webRequestStatus { get; set; }
        public DateTime webRequestDate { get; set; }
        public string responseCode { get; set; }

    }


    public class LienPostingViewModel //: GeneralEntity
    {
        public string account { get; set; }
        public string lienProcessType { get; set; }
        public string lienReasonCode { get; set; }
        public string lienReason { get; set; }
        public decimal lienAmount { get; set; }
        public string lienAccountCurrency { get; set; }
        public string lienUniqueReferenceNumber { get; set; }
        public string webRequestStatus { get; set; }
        public DateTime webRequestDate { get; set; }
        public string responseCode { get; set; }
        public string referenceNumber { get; set; }


    }

    public class ResponseViewModel  : GeneralEntity
    {
        public string webRequestStatus { get; set; }
        public DateTime webRequestDate { get; set; }
        public string responseCode { get; set; }
        public string referenceNumber { get; set; }

    }


}
