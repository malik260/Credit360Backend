using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FintrakBanking.ViewModels.Credit
{
   public class ClassifiedAssetManagementViewModel
    {
        public string customerName { get; set; }
        public string accountNumber { get; set; }
        public string shareHolders { get; set; }
        public string directors { get; set; }
        public string signitories { get; set; }
        public string branchName { get; set; }
        public string branchAddress { get; set; }
        public string branchManager { get; set; }
        public decimal pricipalOutstanding { get; set; }
        public decimal interestOverdue { get; set; }
        public DateTime? provisionToDate { get; set; }
        public DateTime? dateClassified { get; set; }
        public decimal? amountPaidSoFar { get; set; }
        public decimal? amountProposed { get; set; }
        public decimal totalOutstanding { get; set; }
        public decimal totalPaidAndProposed { get; set; }
        public double proposedRepaymentTenor { get; set; }
        public decimal? proposedWriteOffAmount { get; set; }
        public string facilityType { get; set; }
        public decimal? facilityAmountGranted { get; set; }
        public DateTime? dateFacilityWasGranted { get; set; }
        public decimal? amountDisbursed { get; set; }
        public string securityType { get; set; }
        public string securityDescription { get; set; }
        public string securityLocation { get; set; }
        public decimal? securityOpenMarketValue { get; set; }
        public decimal? securityFirstSellValue { get; set; }
        public DateTime? securityValuationDate { get; set; }
        public byte securityPerfectionStatus { get; set; }
        public bool security { get; set; }
        public bool? securityOwnerOccupied { get; set; }
        public string nameOfInitialAccountOfficer { get; set; }
        public string incumbentAccountOfficer { get; set; }

        public int loanId { get; set; }
        public int customerId { get; set; }
        public bool? isResidential { get; set; }
        public bool? isOwnerOccupied { get; set; }
        public DateTime maturityDate { get; set; }
        public DateTime effectiveDate { get; set; }
    }
}
