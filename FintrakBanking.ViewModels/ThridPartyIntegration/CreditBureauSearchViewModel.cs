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
        public string userName { get; set; }
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
        public string userName { get; set; }
        public string DataTicket { get; set; }
        public string EnquiryReason { get; set; }
        public string BusinessName { get; set; }
        public string BusinessRegistrationNumber { get; set; }
        public string AccountNumber { get; set; }
        public string ProductID { get; set; }
    }

    public class SearchFullResultViewModel
    {
        public string userName { get; set; }
        public string DataTicket { get; set; }
        public int ConsumerID { get; set; }
        public string  MergeList { get; set; }
        public string SubscriberEnquiryEngineID { get; set; }
        public int EnquiryID { get; set; }
    }

    public class CRCRequestViewModel
    {
        public string productId;

        public string userName { get; set; }
        public string password { get; set; }
        /// <summary>
        /// response type is the formate you want your result to be. CRC - 1 --> XML , 2 --> PDF 
        /// </summary>
        public int responseType { get; set; }
        /// <summary>
        /// Credit bureau type, 1 --> CRMS (No endpoint), 2 --> XDS  , 3 --> CRC, 
        /// </summary>
        public short creditBureauId { get; set; } //dxs / crc
                                                  /// <summary>
                                                  /// this represent the kind of search the should be performed. 6110 for	INDIVIDUAL  6112 for CORPORATE  
                                                  /// </summary>
        public int reportID { get; set; }       // consumer/ commercial
                                                /// <summary>
                                                /// Search Type Code is for identifing the kind of search that is to be performed 
                                                /// and it depend of the number of parameter that is being provided. use 0 for (name, gender, dob), 
                                                /// 4 (BVN (identity)), 5 (Telephone), 6 for all 5 parameters
                                                /// </summary>
        public int searchTypeCode { get; set; }
        /// <summary>
        /// Inquiry Reason for the search
        /// </summary>
        public string enquiryReason { get; set; }
        /// <summary>
        /// Customer search name. this could be Individual or Corporate (Business Name)
        /// </summary>
        public string customerName { get; set; }
        public string gender { get; set; }
        public string dateOfBirth { get; set; }
        /// <summary>
        /// BVN or National Identification number (NIMC)
        /// </summary>
        public string identification { get; set; }
        public string phoneNumber { get; set; }
        /// <summary>
        /// This is should be an NUBAN or Company Registration Number
        /// </summary>
        public string accountOrRegistrationNumber { get; set; }

        public string genderCode
        {
            get
            {
                return creditBureauId == 3 ?
                    gender == "f" ? "001" : "002" :
                    gender == "f" ? "Female" : "Male";
            }
        }

        //crc Application
        public string currencyCode { get; set; }
        public decimal amount { get; set; }
        public string number { get; set; }
        public string productCode { get; set; }
        public string branchCode { get; set; }
    }

    public class MultiHitRequestViewModel
    {
        public string userName { get; set; }
        public string password { get; set; }


        public string currencyCode { get; set; }
        public decimal amount { get; set; }
        public string number { get; set; }
        public string productCode { get; set; }


        public List<int> bureauID { get; set; }
        public int referenceNo { get; set; }

        /// <summary>
        /// Inquiry Reason for the search
        /// </summary>
        public string enquiryReason { get; set; }

        /// <summary>
        /// response type is the formate you want your result to be. CRC - 1 --> XML , 2 --> PDF 
        /// </summary>
        public int responseType { get; set; }
        public object reportID { get; set; }
    }

    public class CRCSearchResult
    {
        public int SearchCompleted { get; set; }
        public string SearchResult { get; set; }
    }
}
