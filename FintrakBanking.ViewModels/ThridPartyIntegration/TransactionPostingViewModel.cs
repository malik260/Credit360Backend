using FintrakBanking.ViewModels.Customer;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Net.Http;
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
        public string valueDate { get; set; }
        public string webRequestStatus { get; set; }
        public DateTime webRequestDate { get; set; }
        public string responseCode { get; set; }
        public int operationId { get; set; }

        public string currencycrosscode { get; set; }
        public string rateCode { get; set; }
        public string rateUnit { get; set; }

        public string sourceReferenceNumber { get; set; }

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
        public string username { get; set; }
        public string passCode { get; set; }
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
        //public string webRequestStatus { get; set; }
        //public DateTime webRequestDate { get; set; }
        //public string responseCode { get; set; }
        //public string referenceNumber { get; set; }


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
        public string sourceReferenceNumber { get; set; }
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
        public string interestRateAmount { get; set; }
    }

    public class OverDraftTopUpAndRenewViewModel
    {
        public string sourceReferenceNumber { get; set; }
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
        public string sanctionDate { get; set; }
        public int overdraftExtendId { get; set; }
        public string apiUrl { get; set; }
        public DateTime createdDate { get; set; }
    }
    

    public class OverDraftExtendViewModel
    {
        public string sourceReferenceNumber { get; set; }
        public string accountNumber { get; set; }
        public string sanctionReferenceNumber { get; set; }         
        public string sanctionLimit { get; set; }
        [DisplayFormat(DataFormatString = "{0:dd-MMM-yyyy}")]
        public string expiryDate { get; set; }
        public int overdraftExtendId { get; set; }
        public DateTime createdDate { get; set; }
        public string reviewedDate { get; set; }
        public string sanctionLevel { get; set; }
        public string sanctionAuthorizer { get; set; }
    }

    public class TemporaryOverDraftViewModel
    {
        public string sourceReferenceNumber { get; set; }

        public string AccountNumber { get; set; }
        public string TemporaryOverDraftFlag { get; set; }
        public string TemporaryOverDraftAmount { get; set; }
        [DisplayFormat(DataFormatString = "{0:dd-MMM-yyyy}")]
        public string TemporaryOverDraftDate { get; set; }
        public string TemporaryOverDraftNaration { get; set; }
        public string APIUrl { get; set; }
        public int TemporaryOverDraftId { get; set; }
        public string TemporaryOverDraftInterestRate { get; set; }
        
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
        public bool responseStatus { get; set; }
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
        public bool isSuccess { get; set; }
        public HttpResponseMessage response { get; set; }
        public string errorDesc { get; set; }
    }

    public class ResponseMessage
    {
        public ResponseMessageViewModel APIResponse { get; set; }
        public OfferLetterResponse APIOffetResponse { get; set; }
        public LoanStatusResponse LoanStatResponse { get; set; }
        public bool APIStatus { get; set; }
        public HttpResponseMessage Message { get; set; }
        public bool TransactionIsSuccessfull { get; set; }
        public string TransactionMessage { get; set; }
        public string responseMessage { get; set; }

        public CRMSCreditCheckViewModel responseObject { get; set; }

    }

    public class HeadOfficeFacilityApprovalViewModel
    {
        public int loanApplicationId { get; set; }
        public int loanApplicationDetailId { get; set; }
        public string applicationReferenceNumber { get; set; }
        public string relatedReferenceNumber { get; set; }
        public int subsidiaryId { get; set; }
        public int customerId { get; set; }
        public long customerGlobalId { get; set; }
        public DateTime applicationDate { get; set; }
        public double interestRate { get; set; }
        public int applicationTenor { get; set; }
        public int approvalStatusId { get; set; }
        public int approvalLevelId { get; set; }
        public int approvalLevelGlobalCode { get; set; }
        public int? toStaffId { get; set; }
        public short applicationStatusId { get; set; }
        public string operationName { get; set; }
        public int targetId { get; set; }
        public int operationId { get; set; }
        public string productClassName { get; set; }
        public string productName { get; set; }
        public string productClassProcess { get; set; }
        public string loanApplicationTypeName { get; set; }
        public string firstName { get; set; }
        public string lastName { get; set; }
        public string middleName { get; set; }
        public string businessUnitShortCode { get; set; }
        public decimal applicationAmount { get; set; }
        public decimal totalExposureAmount { get; set; }
        public DateTime systemDateTime { get; set; }
        public DateTime systemArrivalDateTime { get; set; }
        public int createdBy { get; set; }
        public int lastUpdatedBy { get; set; }
        public DateTime dateTimeCreated { get; set; }
        public DateTime? dateTimeUpdated { get; set; }
        public bool deleted { get; set; }
        public int? deletedBy { get; set; }
        public DateTime? dateTimeDeleted { get; set; }
        public string responseMessage { get; set; }
        public string responseCode { get; set; }
        public string createdByName { get; set; }
        public string countryCode { get; set; }
        public string staffRoleCode { get; set; }
        public bool actedOn { get; set; }
    }


    public class PostingResult
    {
        
        public bool posted { get; set; }
        
        public string responseMessage { get; set; }
        public string responseCode { get; set; }

        public CRMSCreditCheckViewModel responseObject { get; set; }
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

    public class ResponseMessageOverDraftViewModel
    {
        public string webRequestStatus { get; set; }
        [DisplayFormat(DataFormatString = "{0:dd-MMM-yyyy}")]
        public DateTime webRequestDate { get; set; }
        public string response_code { get; set; }
        public string bo_code { get; set; }
        public string bo_message { get; set; }
        public string lien_id { get; set; }
        public string response_message { get; set; }
        public string collateral_id { get; set; }

        // public HttpResponseMessage APIMessage { get; set; }
        // public bool responseStatus { get; set; }crmsCode
    }

    public class ResponseMessageFacilityViewModel
    {
        public string response_code { get; set; }
        public string response_message { get; set; }
        public string facility_id { get; set; }
        public string bo_code { get; set; }
        public string bo_message { get; set; } 
    }

    public class ResponseMessageLoanCreationViewModel
    {
        public string response_code { get; set; }
        public string response_desc { get; set; }
        public string reference_no { get; set; }
        public string bo_code { get; set; }
        public string bo_message { get; set; }
        public string response_message { get; set; }
    }

    public class ResponseMessageCRMSCodeViewModel
    {
        public string submit_return { get; set; }
     
    }

    public class LoanPrepaymentViewModel
    {
        public string channel_code { get; set; }
        public string auth_key { get; set; }
        public string review_date { get; set; }
        public string user_ref_no { get; set; }
        public string account_no { get; set; }
    }

    public class SubResponseLoanPrepaymentViewModel
    {
        public decimal creditTurnover { get; set; }
        public decimal debitTurnover { get; set; }
        public DateTime transactionDate { get; set; }

        public decimal account_balance { get; set; }

        public string user_ref_no { get; set; }
        public string account_number { get; set; }
        public string customer_acct { get; set; }
        public DateTime due_date { get; set; }
        public DateTime paid_date { get; set; }
        public string branch_code { get; set; }
        public string component_name { get; set; }
        public decimal amount_paid { get; set; }
    }

    public class ResponseLoanPrepaymentViewModel
    {
        public string response_code { get; set; }
        public string response_message { get; set; }
        public DateTime transaction_date { get; set; }
        public string account_number { get; set; }
        public string currency { get; set; }
        public decimal acct_balance { get; set; }
        public decimal debit_turnover { get; set; }
        public decimal credit_turnover { get; set; }
    }

    public class MainResponseLoanPrepaymentViewModel
    {
        public string response_code { get; set; }
        public string response_message { get; set; }
        public List<SubResponseLoanPrepaymentViewModel> getrepaymentdetailsresp { get; set; }
       
    }

    public class ApprovalPostingResult
    {
        public bool posted { get; set; }
        public string responseMessage { get; set; }
        public string responseCode { get; set; }
        public HeadOfficeFacilityApprovalViewModel responseObject { get; set; }
    }

}
