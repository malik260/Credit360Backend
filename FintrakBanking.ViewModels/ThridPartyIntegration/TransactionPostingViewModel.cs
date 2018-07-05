using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Net.Http;
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
        public string valueDate { get; set; }
        public string webRequestStatus { get; set; }
        public DateTime webRequestDate { get; set; }
        public string responseCode { get; set; }
        public int operationId { get; set; }


        public string rateCode { get; set; }
        public string rateUnit { get; set; }

    }


    public class  CreateAccountViewModel
    {
        public string functionCode{get; set;}
        public string solId{get; set;}
        public string currencyCode{get; set;}
        public string customerCode  {get; set;}
        public string schemeCode  {get; set;}
        public string generalLedgerSubHeadCode  {get; set;}
        public string channel  {get; set;}
        public string sectorCode  {get; set;}
        public string subSectorCode  {get; set;}
        public string accountOccupationCode  {get; set;}
        public string borrowerCategoryCode  {get; set;}
        public string purposeOfAdavance  {get; set;}
        public string natureOfAdavance  {get; set;}
        public string modeOfAdavance  {get; set;}
        public string typeOfAdavance  {get; set;}
        public string freeCodeOne  {get; set;}
        //public string freeCodeTwo  {get; set;}
        //public string freeCodeThree  {get; set;}
        public string freeCodeFour  {get; set;}
        public string freeCodeFive  {get; set;}
        public string freeCodeSix  {get; set;}
        public string freeCodeSeven  {get; set;}
        public string freeCodeEight  {get; set;}
        public string freeCodeNine  {get; set;}
        public string freeCodeTen { get; set; }
        public HttpResponseMessage response { get; set; }

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

    public class LienAPIProcessViewModel //: GeneralEntity
    {
        public string account { get; set; }
        public string lienProcessType { get; set; }
        public string lienReasonCode { get; set; }
        public string lienReason { get; set; }
        public string lienAmount { get; set; }
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
        public string message { get; set; }

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

    public class OverDraftNormalViewModel
    {
        public string accountNumber { get; set; }
        public string sanctionReferenceNumber { get; set; }
         [DisplayFormat(DataFormatString = "{0:dd-MMM-yyyy}")]
        public string documentDate { get; set; }
        public string sanctionLevel { get; set; }
        public string sanctionAuthorizer { get; set; }
        [DisplayFormat(DataFormatString = "{0:dd-MMM-yyyy}")]
        public string reviewedDate { get; set; }
        public string sanctionLimit { get; set; }
        [DisplayFormat(DataFormatString = "{0:dd-MMM-yyyy}")]
        public string applicationDate { get; set; }
        [DisplayFormat(DataFormatString = "{0:dd-MMM-yyyy}")]
        public string expiryDate { get; set; }
        [DisplayFormat(DataFormatString = "{0:dd-MMM-yyyy}")]
        public string sanctionDate { get; set; }
        public int overdraftNormalId { get; set; }
    }

    public class OverDraftTopUpAndRenewViewModel
    {
        public string accountNumber { get; set; }
        public string sanctionReferenceNumber { get; set; } 
        public string sanctionLevel { get; set; }
        public string sanctionAuthorizer { get; set; }
        [DisplayFormat(DataFormatString = "{0:dd-MMM-yyyy}")]
        public string reviewedDate { get; set; }
        public string sanctionLimit { get; set; }
        [DisplayFormat(DataFormatString = "{0:dd-MMM-yyyy}")]
        public string applicationDate { get; set; }
        [DisplayFormat(DataFormatString = "{0:dd-MMM-yyyy}")]
        public string expiryDate { get; set; }
        public int overdraftExtendId { get; set; }
        public string apiUrl { get; set; }
    }
    

    public class OverDraftExtendViewModel
    {
        public string accountNumber { get; set; }
        public string sanctionReferenceNumber { get; set; }         
        public string sanctionLimit { get; set; }
        [DisplayFormat(DataFormatString = "{0:dd-MMM-yyyy}")]
        public string expiryDate { get; set; }
        public int overdraftExtendId { get; set; }
    }

    public class TemporaryOverDraftViewModel
    {
        public string AccountNumber { get; set; }
        public string TemporaryOverDraftFlag { get; set; }
        public string TemporaryOverDraftAmount { get; set; }
        [DisplayFormat(DataFormatString = "{0:dd-MMM-yyyy}")]
        public string TemporaryOverDraftDate { get; set; }
        public string TemporaryOverDraftNaration { get; set; }
        public string APIUrl { get; set; }
        public int TemporaryOverDraftId { get; set; }
    }
    
    public class ResponseMessageViewModel
    {
        public string webRequestStatus { get; set; }
        [DisplayFormat(DataFormatString = "{0:dd-MMM-yyyy}")]
        public DateTime webRequestDate { get; set; }
        public string responseCode { get; set; }
        public string serialNumber { get; set; }
        public string message { get; set; }
        public HttpResponseMessage APIMessage { get; set; }
    }

    public class AccountCreationResponseMessageViewModel : ResponseMessageViewModel
    {

        public string accountNumber { get; set; }
        public string referenceNumber { get; set; }
        public string customerName { get; set; }
        public string errorMessage { get; set; } 
    }


    public class BVNCustomerDetailsViewModel
    {
        public string phoneNumber { get; set; }
        public string contactAddress { get; set; }
        public string emailAddress { get; set; }
        public string firstName { get; set; }
        public string middleName { get; set; }
        public string lastName   { get; set; }
        public DateTime dateOfBirth { get; set; }
        public string accountNumber { get; set; }
    }
    public class GLAccountDetailsViewModel
    {
        public string accountNumber { get; set; }
        public string accountName { get; set; }
        public string product { get; set; }
        public string productType { get; set; }
        public string productName { get; set; }
        public string currencyType { get; set; }
        public decimal balance { get; set; }
        public string branch { get; set; }
        public string partitionedType { get; set; }
        public string partitionedFlag { get; set; }
        public string glSubHeadCode { get; set; }
        public string systemAccountFlag { get; set; }
        public HttpResponseMessage response { get; set; }
    }

    public class TDAccountRecordViewModel
    {
        public string accountNumber { get; set; }
        public string accountName { get; set; }
        public string customerCode { get; set; }
        public string productCode { get; set; }
        public string productName { get; set; }
        public string productType { get; set; }
        public string currencyType { get; set; }
        public decimal balance { get; set; }
        public string branch { get; set; }
        public decimal lienAmount { get; set; }
        public HttpResponseMessage response { get; set; }
    }

    public class ResponseMessage
    {
        public ResponseMessageViewModel APIResponse { get; set; }
        public bool APIStatus { get; set; }
        public HttpResponseMessage Message { get; set; }
        public bool TransactionIsSuccessfull { get; set; }
        public string TransactionMessage { get; set; }
    }

    public class AccountCreationRespones
    {
       public AccountCreationResponseMessageViewModel APIResponse { get; set; }
        public bool APIStatus { get; set; }
        public HttpResponseMessage Message { get; set; }
    }

    public class ItemValue
    {
        public string valueCode { get; set; }
        public string valueName { get; set; }
        
    }
}
