using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FintrakBanking.ViewModels.ThridPartyIntegration
{
    public class TransactionPostingViewModel: GeneralEntity
    {
        public string accounts { get; set; }
        public string amounts { get; set; }
        public string narration { get; set; }
        public string referenceNumber { get; set; }
        public string currencyType { get; set; }
        public string webRequestStatus { get; set; }
        public DateTime webRequestDate { get; set; }
        public string responseCode { get; set; }
        public int operationId { get; set; }
        

    }


    public class LienProcessViewModel //: GeneralEntity
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
    public class CustomFinanceTransactionViewModel : GeneralEntity
    {
        public int customTransactionId{ get; set; }
        public string batchCode { get; set; }
        public string accountId { get; set; }
        public string amount { get; set; }
        public string currencyCode { get; set; }
        public string narration { get; set; }
        public int operationId { get; set; }
        public string referenceNumber { get; set; }
        public DateTime datetimeCreated { get; set; }
        public bool webRequestStatus { get; set; }
        public string consumed { get; set; }
        public DateTime datetimeConsumed { get; set; }

    }

}
