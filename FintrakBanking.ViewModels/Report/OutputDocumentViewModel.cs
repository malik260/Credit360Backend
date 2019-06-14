using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FintrakBanking.ViewModels.Report
{
    public class OutputDocumentViewModel
    {
       
    }
    public class OutPutDocumentCustomerInformationViewModel
    {
        public string borrower { get; set; }
        public string location { get; set; }
        public string business { get; set; }
        public string accountNumber { get; set; }
        public string incorporationDate { get; set; }
        public string principalPromoters { get; set; }
        public string customerRiskRating { get; set; }
        public string classification { get; set; }
        public string accountOpeningDate { get; set; }
        public string businessCommencementDate { get; set; }
    }

    public class OutPutDocumentCustomerFacilitiesViewModel
    {
        public string facility { get; set; }
        public string amount { get; set; }
        public DateTime? maturity { get; set; }
        public string security { get; set; }
        public string performance { get; set; }
        public int operationId { get; set; }
        public int customerId { get; set; }
        public string customerCode { get; set; }
        public int createdBy { get; set; }
        public string applicationReferenceNumber { get; set; }
        public string targetReferenceNumber { get; set; }
    }

    public class OutPutDocumentMonthsActivityViewModel
    {
        public string months { get; set; }
        public decimal debits { get; set; }
        public decimal credits { get; set; }
        public decimal EOMBalance { get; set; }
        public decimal returnCheque { get; set; }
    }

    public class OutPutDocumentConcurrencesViewModel
    {
        public string officer { get; set; }
        public string name { get; set; }
        public string signature { get; set; }
        public int id { get; set; }
    }

    public class OutPutDocumentCollateralViewModel
    {
        public string collateralDetail { get; set; }
        public decimal collateralValue { get; set; }
        public decimal stapedToCoverAmount { get; set; }
    }

    public class OutPutDocumentChecklistViewModel
    {
        public string item { get; set; }
        public string required { get; set; }
        public string actual { get; set; }
        public string exception { get; set; }
        public string checklistType { get; set; }
    }

    public class OutPutDocumentApprovalViewModel {
        public string officer { get; set; }
        public string name { get; set; }
        public string signature { get; set; }
        public int id { get; set; }
    }

    public class OutPutDocumentFeeViewModel
    {
        public string feeName { get; set; }
        public decimal rateValue { get; set; }
        public string productName { get; set; }
    }

    public class OutPutDocumentMonthActivitySignViewModel
    {
        public decimal currentBookBalance { get; set; }
        public decimal averageMonthly { get; set; }
        public string otherBankers { get; set; }
        public string exixstingFacility { get; set; }
        public string securitySuppport { get; set; }
    }
    public class CurrentRequestViewModel
    {
        public string productName { get; set; }
        public decimal loanAmount { get; set; }
        public string purpose { get; set; }
        public int tenor { get; set; }
        public string repaymentSchedule { get; set; }
    }

    public class OutputDocumentSummaryViewModel
    {
        public decimal currentBookBalance { get; set; }
        public decimal averageMonthlyCreditTurnover { get; set; }
        public string purpose { get; set; }
        public int tenor { get; set; }
        public string repaymentPlan { get; set; }
    }
}
