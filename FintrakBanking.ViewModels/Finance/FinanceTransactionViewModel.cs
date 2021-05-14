using FintrakBanking.Common.Enum;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace FintrakBanking.ViewModels.Finance
{
    //public class FinanceTransactionViewModel : GeneralEntity
    //{
    //    public FinanceTransactionViewModel()
    //    {
    //        transactionDetails = new List<FinanceTransactionDetailViewModel>();
    //    }

    //    public string batchCode { get; set; }
    //    public int? operationId { get; set; }
    //    public string description { get; set; }
    //    public DateTime valueDate { get; set; }
    //    public DateTime transactionDate { get; set; }
    //    public short currencyId { get; set; }
    //    public double currencyRate { get; set; }
    //    public DateTime postedDateTime { get; set; }
    //    public bool isApproved { get; set; }
    //    public int postedBy { get; set; }
    //    public int approvedBy { get; set; }
    //    public DateTime approvedDate { get; set; }
    //    public DateTime approvedDateTime { get; set; }
    //    public short sourceApplicationId { get; set; }
    //    public string amount  { get; set; }

    //    public List<FinanceTransactionDetailViewModel> transactionDetails { get; set; }

    //}

    //public class FinanceTransactionDetailViewModel
    //{
    //    public int transactionId { get; set; }
    //    public int glAccountId { get; set; }
    //    public string sourceReferenceNumber { get; set; }
    //    public int? casaAccountId { get; set; }
    //    public decimal debitAmount { get; set; }
    //    public decimal creditAmount { get; set; }
    //    public short sourceBranchId { get; set; }
    //    public short destinationBranchId { get; set; }
    //}


    public class FinanceTransactionViewModel : GeneralEntity
    {
        public string batchCode { get; set; }
        public int operationId { get; set; }
        public string description { get; set; }
        public DateTime valueDate { get; set; }
        public DateTime transactionDate { get; set; }
        public short currencyId { get; set; }
        public double currencyRate { get; set; }
        public DateTime postedDateTime { get; set; }
        public DateTime postedDate { get; set; }
        public bool isApproved { get; set; }
        public int postedBy { get; set; }
        public int approvedBy { get; set; }
        public DateTime approvedDate { get; set; }
        public DateTime approvedDateTime { get; set; }
        public short sourceApplicationId { get; set; }
        public int transactionId { get; set; }
        public int glAccountId { get; set; }
        public string sourceReferenceNumber { get; set; }
        public int? casaAccountId { get; set; }
        public decimal debitAmount { get; set; }
        public decimal creditAmount { get; set; }
        public new short sourceBranchId { get; set; }
        public short destinationBranchId { get; set; }
        public string batchId { get; set; }
        public string currencyCrossCode { get; set; }
        public string rateUnit { get; set; }
        public string rateCode { get; set; }
        public bool useDirectAccount { get; set; }
        public string accountNumber { get; set; }
    }


    public class TwoFactorAutheticationViewModel 
    {                
        public string username { get; set; }
        public string passcode { get; set; }
        public bool skipAuthentication { get; set; }
}

    public class TwoFactorAutheticationOutputViewModel
    {
        public bool authenticated { get; set; }
        public string message { get; set; }
    }
    public class CasaBalanceViewModel
    {
        public string accountNumber { get; set; }
        public string currencyCode { get; set; }

        public int casaAccountId { get; set; }
        public string productAccountNumber { get; set; }
        public string productAccountName { get; set; }

        public string  accountName { get; set; }
        public decimal availableBalance { get { return balance; } set { balance = value; } }
        public decimal balance { get; set; }
        public decimal ledgerBalance { get; set; }
        public string accountNo { get; set; }
        public string productName { get; set; }
        public int currencyId { get; set; }
        public string customerCode { get; set; }
        public string product  { get; set; }
        public string productType { get; set; }
        public string currencyType  { get; set; }
        public string accountStatus { get; set; }
        public string freezeStatus { get; set; }
        public string freezeReason { get; set; }
        public string lastTransactionDate { get; set; }
        public bool isCasaAccountDetailAvailable { get; set; }
        public CASAAccountStatusEnum accountStatusId { get; set; }

        public bool hasBalance { get; set; }
        public string errorMessage { get; set; }
    }

    public class CasaAccountLienViewModel
    {
        public int lienId { get; set; }
        public string lienReferenceNumber { get; set; }
        public string productAccountNumber { get; set; }
        public string sourceReferenceNumber { get; set; }
        public decimal lienAmount { get; set; }
        public string description { get; set; }
        public short lienTypeId { get; set; }
        public DateTime dateCreated { get; set; }

    }

    public class CasaLienTypeViewModel
    {
        public int lienTypeId { get; set; }
        public string lienTypeName { get; set; }
    }

    public class CasaLoanViewModel
    {
        public int loanId { get; set; }
        public string loanReferenceNumber { get; set; }
    }

    public class BasicTrasactionSourceInputModel : GeneralEntity
    {
        public string description { get; set; }
        public short sourceApplicationId { get; set; }
    }

    public class FinanceTransactionStagingViewModel : GeneralEntity
    {
        public string batchId  { get; set; }
        public int batchRefId { get; set; }
        public string transType { get; set; }
        public string flowType { get; set; }
        public int operationId { get; set; }
        public string description { get; set; }
        public DateTime valueDate { get; set; }
        public DateTime transactionDate { get; set; }
        public string currencyCode { get; set; }
        public double currencyRate { get; set; }
        public string currencyRateCode{ get; set; }
        public DateTime postedDateTime { get; set; }
        public bool isApproved { get; set; }
        public string postedBy { get; set; }
        public int approvedBy { get; set; }
        public DateTime approvedDate { get; set; }
        public DateTime approvedDateTime { get; set; }
        public short sourceApplicationId { get; set; }
        public int transactionId { get; set; }
        public string creditGlAccount { get; set; }
        public string debitGlAccount { get; set; }
        public string sourceReferenceNumber { get; set; }
        public decimal amount { get; set; }
        public decimal amountCollected { get; set; }
        public short sourceBranchId { get; set; }
        public short destinationBranchId { get; set; }
        public string bankId { get; set; }
        public short branchId { get; set; }
        public int recordCount { get; set; }
        public string status { get; set; }
        public string forceDebitAccount { get; set; }
        public int sid { get; set; }
        public int productId { get; set; }
        public int currencyId { get; set; }
        public decimal amountDue { get; set; }
        public decimal actualAmountCollected { get; set; }
        public int creditGlAccountId { get; set; }
        public int debitGlAccountId { get; set; }
        public int? creditCasaAccountId { get; set; }
        public int? debitCasaAccountId{ get; set; }
        public int? loanId { get; set; }
        public decimal lienAmount { get; set; }
    }


    public class InterestRateInquiryViewModel 
    {
        public string accountNumber { get; set; }
        public string interestRateAmount { get; set; }
        public string interestTableCode { get; set; }
        public string accountType { get; set; }
        public string interestSerialNumber { get; set; }
        public string startDate { get; set; }
        public string endDate { get; set; }
        public string lastChangedDate  { get; set; }
        public string webRequestStatus { get; set; }
        public DateTime webRequestDate { get; set; }
        public string responseCode { get; set; }
        public string serialNumber { get; set; }
        public string message { get; set; }
        public string APIUrl { get; set; }
    }
    
}
