using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FintrakBanking.ViewModels.Credit
{
    public class AppraisalMemorandumViewModel : GeneralEntity
    {
        public int appraisalMemorandumId { get; set; }
        public int loanApplicationId { get; set; }
        public int levelId { get; set; }
        public string camRef { get; set; }
        public bool isCompleted { get; set; }
        public bool riskRated { get; set; }
        public string camDocumentation { get; set; }
        //public string loanDetails { get; set; }
        public bool politicalyExposed { get; set; }
        public string comment { get; set; }
        public decimal loanAmount { get; set; }
        public int approvalLevelId { get; set; }
        public int documentationId { get; set; }
    }

    public class ForwardViewModel : GeneralEntity
    {
        public int forwardAction { get; set; } // statusId
        public int applicationId { get; set; } // targetId
        public int appraisalMemorandumId { get; set; }
        public int? productClassId { get; set; }
        public int? productId { get; set; }
        public int receiverLevelId { get; set; }
        public int? receiverStaffId { get; set; }
        public int? trailId { get; set; }
        public decimal amount { get; set; }
        public bool politicallyExposed { get; set; }
        public short? vote { get; set; }
        public string comment { get; set; }
        public decimal principal { get; set; }
        public double rate { get; set; }
        //public int tenor { get; set; }
        public bool investmentGrade { get; set; }
        public List<ApprovedLoanDetailViewModel> lineItems { get; set; }
        public List<RecommendedChangesViewModel> recommendedChanges { get; set; }
        public int applicationTenor { get; set; }
        public int operationId { get; set; }
        public bool untenored { get; set; }
        public bool isBusiness { get; set; }
        public float? interestRateConcession { get; set; }
        public float? feeRateConcession { get; set; }
    }

    public class ApprovedLoanDetailViewModel : GeneralEntity
    {
        public int loanApplicationDetailId { get; set; }
        public int applicationId { get; set; }
        public string obligorName { get; set; }
        public int approvedTenor { get; set; }
        public double approvedRate { get; set; }

        public string priceIndexName { get; set; }
        public int? priceIndexId { get; set; }
        public double priceIndexRate { get; set; }
        public string liborInfo { get { return priceIndexId == null ? "" : "("+ priceIndexName + ")"; } }
        //public int productId { get; set; }

        public decimal approvedAmount { get; set; }
        public string approvedProductName { get; set; }
        public short statusId { get; set; }
        public double exchangeRate { get; set; }
        public string currencyCode { get; set; }
        public string proposedProductName { get; set; }
        public int proposedTenor { get; set; }
        public double proposedRate { get; set; }
        public decimal proposedAmount { get; set; }
        //public double proposedExchangeRate { get; set; }
        //public double approvedExchangeRate { get; set; }
        public short proposedProductId { get; set; }
        public short approvedProductId { get; set; }
        public decimal convertedApprovedAmount { get { return this.approvedAmount * (decimal)this.exchangeRate; } }

        public int customerId { get; set; }
        public string terms { get; set; }
        public string schedule { get; set; }
        public bool securedByCollateral { get; set; }
        public int? crmsCollateralTypeId { get; set; }
        public bool isSpecialised { get; set; }

        public string proposedTenorString
        {
            get
            {
                var units = proposedTenor == 1 ? " day" : " days";
                if (proposedTenor < 15) return proposedTenor.ToString() + units;
                var months = Math.Ceiling((Math.Floor(proposedTenor / 15.00)) / 2);
                units = months == 1 ? " month" : " months";
                return months.ToString() + " " + units;
            }
        }
        public string approvedTenorString
        {
            get
            {
                var units = approvedTenor == 1 ? " day" : " days";
                if (approvedTenor < 15) return approvedTenor.ToString() + units;
                var months = Math.Ceiling((Math.Floor(approvedTenor / 15.00)) / 2);
                units = months == 1 ? " month" : " months";
                return months.ToString() + " " + units;
            }
        }
    }

    public class LoanApplicationDetailsViewModel : GeneralEntity // TO CARRY ALL APPLICATION DETAILS PAYLOAD IN ONE API CALL
    {
        public List<DedupeApplicationViewModel> duplications { get; set; }
        public List<ApprovedLoanDetailViewModel> facilities { get; set; }
        public LoanApplicationViewModel application { get; set; }
    }

    public class DedupeApplicationViewModel
    {
        public string applicationReferenceNumber { get; set; }
        public DateTime applicationDate { get; set; }
        public decimal applicationAmount { get; set; }
        public double interestRate { get; set; }
        public int applicationTenor { get; set; }
        public string branchName { get; set; }
        public string productName { get; set; }
    }

    public class PrivilegeViewModel : GeneralEntity
    {
        public bool viewCamDocument { get; set; }
        public bool viewUploadedFiles { get; set; }
        public bool viewApproval { get; set; }
        public bool canMakeChanges { get; set; }
        public bool canAppendTemplate { get; set; }
        public bool canApprove { get; set; }
        public bool canUploadFile { get; set; }
        public bool canSendRequest { get; set; }
        public decimal approvalLimit { get; set; }
        public decimal investmentGradeApprovalLimit { get; set; }
        public List<int> userApprovalLevelIds { get; set; }
        public int maximumTenor { get; set; }
        public int roleId { get; set; }
        public int approvalLevelId { get; set; }
        public int groupRoleId { get; set; }
        public bool canEscalate { get; set; }
        public bool owner { get; set; }
    }

    public class DocumentationViewModel : GeneralEntity
    {
        public int documentationId { get; set; }
        public string documentation { get; set; }
        public int appraisalMemorandumId { get; set; }
        public int approvalLevelId { get; set; }
    }

    public class RecommendedChangesViewModel : GeneralEntity
    {
        public int detailId { get; set; }
        public int productId { get; set; }
        public int statusId { get; set; }
        public decimal amount { get; set; }
        public double exchangeRate { get; set; }
        public double interestRate { get; set; }
        public int tenor { get; set; }
        public string productName { get; set; }
        public int convertedAmount { get; set; }
        public int loanApplicationDetailId { get; set; }
    }

    public class CurrentCommitteeViewModel
    {
        public int position { get; set; }
        public int approvalLevelId { get; set; }
        public string approvalLevelName { get; set; }
        public int numberOfApprovals { get; set; }
        public int groupRoleId  { get; set; }
        public string approvalGroupName { get; set; }
        public int staffId { get; set; }
        public string staffName { get; set; }
        public int? vote { get; set; }
        public string comment { get; set; }
    }

    public class ForwardCommitteeCamViewModel : GeneralEntity
    {
        public int applicationId { get; set; }
        public decimal amount { get; set; }
        public int tenor { get; set; }
        public bool investmentGrade { get; set; }
        public bool politicallyExposed { get; set; }
        public List<CurrentCommitteeViewModel> votes { get; set; }
    }

    public class LoanApplicationDetailLogViewModel : GeneralEntity
    {
        public int loanApplicationDetailId { get; set; }

        public string customerName { get; set; }

        public string approvedProductName { get; set; }

        public short approvedProductId { get; set; }

        public int approvedTenor { get; set; }

        public double approvedRate { get; set; }

        public decimal approvedAmount { get; set; }

        public string currencyName { get; set; }

        public double exchangeRate { get; set; }

        public short statusId { get; set; }

        public decimal exchangeAmount { get { return (decimal)exchangeRate * approvedAmount; } }

        public int customerId { get; set; }

        public int applicationId { get; set; }

        public string staffName { get; set; }
        public short? decision { get; set; }
    }

    public class CAMApprovalLevelStaffViewModel
    {
        public int approvalLevelId { get; set; }
        public string approvalLevelName { get; set; }
        public int staffId { get; set; }
        public string staffName { get; set; }
    }

    public class PendingProductProgramViewModel
    {
        public short? productClassId { get; set; }
        public string productClassName { get; set; }
        public int pendingNumber { get; set; }
    }

    public class AuthoritySignatureViewModel : GeneralEntity
    {
        public int targetId { get; set; }
        public int operationId { get; set; }
        public int? productClassId { get; set; }
        public int? productId { get; set; }
        public int? levelId { get; set; }
    }

    public class LoanDetailsFeeViewModel
    {
        public short statusId { get; set; }
        //public int applicationId { get; set; }
        public int loanApplicationDetailId { get; set; }
        public int loanChargeFeeId { get; set; }
        public int chargeFeeId { get; set; }
        public string concessionReason { get; set; }
        public decimal recommendedFeeRate { get; set; }
        public decimal defaultFeeRate { get; set; }
        public bool hasConcession { get; set; }
        public string approvalStatus { get; set; }
        public string feeName { get; set; }
        public string productName { get; set; }
    }

    public class MonitoringTriggersViewModel
    {
        public int applicationDetailId { get; set; }
        public string monitoringTrigger { get; set; }
        public int? monitoringTriggerId { get; set; }
        public string productCustomerName { get; set; }
    }

    public class RepaymentScheduleTermsViewModel
    {
        public string terms { get; set; }
        public string schedule { get; set; }
        public int applicationDetailId { get; set; }
        public string productCustomerName { get; set; }
    }

    public class ProductLimitValidationViewModel
    {
        public int applicationDetailId { get; set; }
        public string productCustomerName { get; set; }
        public double? percentageLimit { get; set; }
        public decimal recommendedAmount { get; set; }
        public decimal controlAmount { get; set; }

        public decimal limit
        {
            get
            {
                return percentageLimit == null ? 0 : (decimal)(percentageLimit / 100) * controlAmount;
            }
        }
        public bool isValid { get { return recommendedAmount <= limit;  } }

        public int productClassId { get; set; }
    }

    public class RecommendedCollateralViewModel : GeneralEntity
    {
        public int? id { get; set; }
        public decimal collateralValue { get; set; }
        public string collateralDetail { get; set; }
        public decimal stampedToCoverAmount { get; set; }
        public int applicationDetailId { get; set; }
        public string productCustomerName { get; set; }
        public string staffName { get; set; }
        public int applicationId { get; set; }
    }

    public class TranchDisbursmentViewModel
    {
        public int approvalLevelId { get; set; }
        public int loanApplicationId { get; set; }
    }
}