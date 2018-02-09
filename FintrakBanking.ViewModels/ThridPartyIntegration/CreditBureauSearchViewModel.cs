using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FintrakBanking.ViewModels.ThridPartyIntegration
{
    public  class CreditBureauSearchViewModel
    {
    }

    public class CreditBureauIndividualSearchViewModel
    {
        public short creditBureauId { get; set; }
        public string enquiryReason { get; set; }
        public string customerName { get; set; }
        public string dateOfBirth { get; set; }
        public string gender { get; set; }
        public string identificationNumber { get; set; }
        public string accountNumber { get; set; }
        public string productID { get; set; }
    }

    public class CreditBureauCorporateSearchViewModel
    {
        public short creditBureauId { get; set; }
        public string enquiryReason { get; set; }
        public string customerName { get; set; }
        public string businessRegistrationNumber { get; set; }
        public string accountNumber { get; set; }
        public string productID { get; set; }
    }

    public class XDSIndividualSearchViewModel
    {
        public string DataTicket { get; set; }
        public string EnquiryReason { get; set; }
        public string ConsumerName { get; set; }
        public string DateOfBirth { get; set; }
        public string Identification { get; set; }
        public string AccountNumber { get; set; }
        public string ProductID { get; set; }
    }

    public class XDSCommercialSearchViewModel
    {
        public string DataTicket { get; set; }
        public string EnquiryReason { get; set; }
        public string BusinessName { get; set; }
        public string BusinessRegistrationNumber { get; set; }
        public string AccountNumber { get; set; }
        public string ProductID { get; set; }
    }

    public class SearchFullResultViewModel
    {
        public string DataTicket { get; set; }
        public int ConsumerID { get; set; }
        public string MergeList { get; set; }
        public string SubscriberEnquiryEngineID { get; set; }
        public int EnquiryID { get; set; }
    }
}
