using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace FintrakBanking.ViewModels.Credit
{

    public class LoanApplicationViewModel : GeneralEntity
    {
        public LoanApplicationViewModel()
        {
            LoanApplicationCollateral = new List<LoanApplicationCollateralViewModel>();
            LoanApplicationDetail = new List<LoanApplicationDetailViewModel>();
        }

        public int loanApplicationId { get; set; }
        public int loanApplicationDetailId { get; set; }
        public string applicationReferenceNumber { get; set; }
        public int? customerId { get; set; }
        public int? operationId { get; set; }

        public short ? branchId { get; set; }
        public short? productClassId { get; set; }
        public string productClassName { get; set; }
        public short productId { get; set; }
        public int? customerGroupId { get; set; }
        public string customerGroupCode { get; set; }
        public short loanTypeId { get; set; }
        public int casaAccountId { get; set; }
        public short currencyId { get; set; }
        public string currencyCode { get; set; }
        public short loanStatusId { get; set; }
        public string loanStatus { get; set; }
        public int relationshipOfficerId { get; set; }
        public int relationshipManagerId { get; set; }
        public DateTime applicationDate { get; set; }
        public decimal applicationAmount { get; set; }
        public decimal approvedAmount { get; set; }
        public int applicationTenor { get; set; }
        public double interestRate { get; set; }
        public DateTime ? effectiveDate { get; set; }
        public DateTime ? expiryDate { get; set; }
        public string customerAccount { get; set; }
        public short tenorModeId { get; set; }
        public string loanInformation { get; set; }
        public string misCode { get; set; }
        public string teamMisCode { get; set; }
        public bool submittedForAppraisal { get; set; }
        public bool isRelatedParty { get; set; }
        public bool isPoliticallyExposed { get; set; }
        public int approvalStatusId { get; set; }

        public decimal proposedAmount { get; set; }
        public int proposedTenor { get; set; }
        public int approvalLevelId { get; set; }

        public short subSectorId { get; set; }
        public int sectorId { get; set; }
        public bool isInvestmentGrade { set; get; }
        public string customerName { get; set; }
        public string branchName { get; set; }
        public string productName { get; set; }
        public string customerGroupName { get; set; }

        public string loanTypeName { get; set; }
        public string relationshipOfficerName { get; set; }
        public string relationshipManagerName { get; set; }
        public string tenorModeName { get; set; }

        //public string amount { get { return this.principalAmount.ToString("#,#.00#"); } }
        public string applicantName { get { return this.customerName + "(" + this.customerGroupName + ")"; } }

        public int? loanPreliminaryEvaluationId { get; set; }
        public double exchangeRate { get; set; }
        public List<LoanApplicationCollateralViewModel> LoanApplicationCollateral { get; set; }
        public List<LoanApplicationDetailViewModel> LoanApplicationDetail { get; set; }
        public int? currentApprovalStateId { get; set; }
        public int? currentApprovalLevelId { get; set; }
        public string currentApprovalLevel { get; set; }
        public string lastComment { get; set; }
        public int approvalTrailId { get; set; }
       // public decimal? approvedAmount { get; set; }
        public short applicationStatusId { get; set; }

        public bool isCollateralBacked { get; set; }
        
        //private int _tenor;

        public int tenor { get; set; }
        public bool customerInfoValidated { get; set; }
        public bool notInNegativeCrms { get; set; }
        public bool notInBlackbook { get; set; }
        public bool notInCamsol { get; set; }
        public bool notInXds { get; set; }
        public bool notInCrc { get; set; }
        public string firstName { get; set; }
        public string middleName { get; set; }
        public string customerCode { get; set; }
        public string lastName { get; set; }
        public int groupRoleId { get; set; }
        public string accountNumber { get; set; }
    }

    public class CollateralLenPlacementViewModel : GeneralEntity
    {
        public string acountNumber { get; set; }
        public int cityId { get; set; }
        public int stateId { get; set; }
        public int collateralTypeId { get; set; }
        public string certificateOfOccupancy { get; set; }
        public string loanApplicationRefrence { get; set; }
        public string customerName { get; set; }
    }

    public class ProductClassViewModel
    {
        public short productClassId { get; set; }
        public string productClassName { get; set; }
        public short productClassTypeId { get; set; }
    }

    public class LoanApplicationDetailViewModel : GeneralEntity
    {

        public int loanApplicationDetailId { get; set; }

        public int loanApplicationId { get; set; }

        public string applicationRefNo { get; set; }

        public int customerId { get; set; }

        public string customerName { get; set; }

        public short proposedProductId { get; set; }

        public string proposedProductName { get; set; }

        public int proposedTenor { get; set; }

        public double proposedInterestRate { get; set; }

        public decimal proposedAmount { get; set; }

        public short approvedProductId { get; set; }

        public int approvedTenor { get ; set; }

        public double approvedInterestRate { get; set; }

        public decimal approvedAmount { get; set; }

        public short currencyId { get; set; }

        public string currencyName { get; set; }

        public double exchangeRate { get; set; }

        public decimal exchangeAmount { get { return (decimal)exchangeRate * proposedAmount; } }

        public short subSectorId { get; set; }

        public short statusId { get; set; }

        public int casaAccountId { get; set; }

        public short sectorId { get; set; }

        public short productClassId { get; set; }

    }

    public class SearchViewModel
    {
        public string searchString { get; set; }
    }

    public class RegionLoanApplicationViewModel : GeneralEntity
    {
        public RegionLoanApplicationViewModel()
        {
            LoanApplicationDetail = new List<LoanApplicationDetailViewModel>();
        }

        public int loanApplicationId { get; set; }
        public int loanApplicationDetailId { get; set; }
        public string applicationReferenceNumber { get; set; }
        public int? customerId { get; set; }
        public int? operationId { get; set; }
        public short? branchId { get; set; }
        public short? productClassId { get; set; }
        public string productClassName { get; set; }
        public short productId { get; set; }
        public int? customerGroupId { get; set; }
        public string customerGroupCode { get; set; }
        public short loanTypeId { get; set; }
        public int casaAccountId { get; set; }
        public DateTime applicationDate { get; set; }
        public decimal applicationAmount { get; set; }
        public decimal approvedAmount { get; set; }
        public int applicationTenor { get; set; }
        public double interestRate { get; set; }

        public List<LoanApplicationDetailViewModel> LoanApplicationDetail { get; set; }
    }
}
