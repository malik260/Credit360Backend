using System;
using System.Data.Entity;

namespace FintrakBanking.ViewModels.Credit
{
    public class CurrentCustomerExposure
    {
        public DateTime? bookingDate { get; set; }
        public DateTime bookingDateString { get; set; }
        public DateTime? maturityDateString { get; set; }

        public string customerName { get; set; }
        public string facilityType { get; set; }
        public decimal existingLimit { get; set; }
        public decimal proposedLimit { get; set; }
        public decimal change { get { return (proposedLimit - recommendedLimit); } }
        //public decimal outstandings { get { return proposedLimit; } }
        public decimal outstandings { get; set; }
        public decimal outstandingsLcy { get; set; }
        public decimal approvedAmount { get; set; }
        public decimal approvedAmountLcy { get; set; }
        public int exposureTypeId { get; set; }
        public string exposureTypeCodeString { get; set; }
        public int exposureTypeCode { get; set; }
        public string adjFacilityTypeString { get; set; }
        public string adjFacilityTypeCode { get; set; }
        public int adjFacilityTypeId { get; set; }
        public string customerCode { get; set; }

        public decimal pastDueObligationsPrincipal { get; set; }
        public decimal PastDueObligationsInterest { get; set; }
        public DateTime reviewDate { get; set; }
        public string prudentialGuideline { get; set; }
        public string loanStatus { get; set; }
        public int? casaAccountId { get; set; }

        public decimal recommendedLimit { get; set; }
        public string referenceNumber { get; set; }
        public int productTypeId { get; set; }
        public int productId { get; set; }
        public string productIdString { get; set; }
        public string productCode { get; set; }
        public string productName { get; set; }
        public int loanId { get; set; }
        public short applicationStatusId { get; set; }
        public string currency { get; set; }
        public string currencyType { get; set; }
        public string currencyCode { get; set; }
        public DateTime? maturityDate { get; set; }
        public int tenor { get; set; }
        public string tenorString { get; set; }
        public decimal? totalBankExposure { get; set; }
        public decimal? companyLimit { get; set; }
    }

    public class FacilitySummary
    {
        public int number { get; set; }
        public string facilityType { get; set; }
        public decimal existingLimit { get; set; }
        public decimal recommendedLimit { get; set; } // proposed amount fixed
        public decimal proposedLimit { get; set; } // approved amount
        public decimal change { get { return (recommendedLimit - proposedLimit); } }
        public decimal outstanding { get; set; }
        public decimal PastDuePrincipal { get; set; }
        public decimal PastDueInterest { get; set; }
        public DateTime reviewDate { get; set; }

        public string prudentialGuideline { get; set; }
        public string loanStatus { get; set; }
    }

    public class CustomerProduct
    {
        public int CUSTOMERID { get; set; }
        public int PRODUCTID { get; set; }
    }

    public class GlobalExposureViewModel
    {
        public int id { get; set; }
        public string customerId { get; set; }
        public DateTime? date { get; set; }
        public string customerName { get; set; }
        public string groupObligorName { get; set; }
        public string referenceNumber { get; set; }
        public string accountNumber { get; set; }
        public string accountOfficerCode { get; set; }
        public string accountOfficerName { get; set; }
        public string alphaCode { get; set; }
        public string productCode { get; set; }
        public string currencyName { get; set; }
        public string productName { get; set; }
        public string facilityType { get; set; }
        public string adjFacilityType { get; set; }
        public string adjFacilityTypeId { get; set; }
        public string odStatus { get; set; }
        public string currencyType { get; set; }
        public string cbnSector { get; set; }
        public string cbnSectorAdjusted { get; set; }
        public string cbnClassification { get; set; }
        public string pwcClassification { get; set; }
        public string ifrsClassification { get; set; }
        public string tenor { get; set; }
        public DateTime? maturityDate { get; set; }
        public string location { get; set; }
        public DateTime? bookingDate { get; set; }
        public DateTime? valueDate { get; set; }
        public string maturityBand { get; set; }
        public string customerType { get; set; }
        public string branchName { get; set; }           
        public string branchCode { get; set; }
        public string obligorRiskRating { get; set; }
        public DateTime? lastCrDate { get; set; }
        public string productId { get; set; }
        public string exposureTypeCode { get; set; }
        public string exposureType { get; set; }
        public string teamCode { get; set; }
        public decimal? lastCreditAmount { get; set; }
        public decimal? principalOutStandingBaltcy { get; set; }
        public decimal? principalOutStandingBallcy { get; set; }
        public decimal? loanAmounyTcy { get; set; }
        public decimal? loanAmounyLcy { get; set; }
        public string cardLimit { get; set; }
        public string fxrate { get; set; }
        public decimal? shf { get; set; }
        public decimal? sectionedLoanLimitDirectFcy { get; set; }
        public decimal? sectionedLoanLimitDirectLcy { get; set; }
        public decimal? totalExposuLcyPreviousYear { get; set; }
        public decimal? totalExposuLcyPrevious6Months { get; set; }
        public decimal? totalExposuLcyPrevious3Months { get; set; }
        public decimal? totalExposuLcyPreviousMonths { get; set; }
        public decimal? totalExposuLcyPreviousDay { get; set; }
        public decimal? totalExposure { get; set; }
        public decimal? impairmentAmount { get; set; }
        public decimal? unpaidObligationAmount { get; set; }
        public decimal? unpoInterestAmount { get; set; }
        public decimal? interestReceivableTcy { get; set; }
        public decimal? amountDue { get; set; }
        public string interestrate { get; set; }
        public int maturityDays { get; set; }
        public string expiringBandId { get; set; }
        public string expiringBand { get; set; }
        public int? unPoDaysOverdue { get; set; }
        public DateTime scheduleDueDate { get; set; }

        //public int maturityDateEx
        //{
        //    get
        //    {
        //        return DbFunctions.DiffDays(DateTime.UtcNow, maturityDate).Value;
        //    }
        //}
    }

    public class AlertDailyReportViewModel
    {
        public int id { get; set; }
        public DateTime processingStartDate { get; set; }
        public DateTime processingEndDate { get; set; }
        public DateTime processingDate { get; set; }
        public string successfulProcessingInd { get; set; }
    }
}

/*
Customer Exposure --< Facility Summary
-----------------
Number	
Facility Type
Existing Limit
Proposed Limit	* approved
Change	
Outstanding	
Past Due Principal	
Past Due Interest	
Review Date
------------------
Total								
*/
