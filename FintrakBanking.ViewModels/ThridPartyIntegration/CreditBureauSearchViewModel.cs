using FintrakBanking.ViewModels.Credit;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FintrakBanking.ViewModels.ThridPartyIntegration
{

    public class CustomerCreditBureauUploadViewModel : GeneralEntity
    {
        public int documentId { get; set; }
        public int customerCreditBureauId { get; set; }
        public string documentTitle { get; set; }
        public string fileName { get; set; }
        public string fileExtension { get; set; }
        public byte[] fileData { get; set; }
        public DateTime systemDateTime { get; set; }

    }



    public class CreditBureauSearchViewModel : GeneralEntity
    {
        public string productId;

        public string userName { get; set; }
        public string password { get; set; }

        public short creditBureauId { get; set; } //dxs / crc
        public int searchType { get; set; }       // consumer/ commercial

        public string enquiryReason { get; set; }
        public string customerName { get; set; }
        public string gender { get; set; }
        public string dateOfBirth { get; set; }
        public string identification { get; set; }
        public string accountOrRegistrationNumber { get; set; }
    }

    public class SearchInput : GeneralEntity
    {
        public string productId;
        public string userName { get; set; }
        public string password { get; set; }

        public short creditBureauId { get; set; } //dxs / crc
        public int searchType { get; set; }       // consumer/ commercial

        public int consumerID { get; set; }
        public List<int> mergeList { get; set; }
        public string subscriberEnquiryEngineID { get; set; }
        public int enquiryID { get; set; }
        public LoanCreditBereauViewModel customerCreditBureauUploadDetails { get; set; }
        public int casaAccountId { get; set; }
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
        public string  MergeList { get; set; }
        public string SubscriberEnquiryEngineID { get; set; }
        public int EnquiryID { get; set; }
    }
}
