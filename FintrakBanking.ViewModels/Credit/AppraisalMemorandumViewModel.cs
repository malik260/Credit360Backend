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
    }

    public class ApprovedLoanDetailViewModel : GeneralEntity
    {
        public int loanApplicationDetailId { get; set; }
        public int applicationId { get; set; }
        public string obligorName { get; set; }
        public int approvedTenor { get; set; }
        public double approvedRate { get; set; }
        public decimal approvedAmount { get; set; }
        //public int productId { get; set; }
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
        public int approvalLevelId { get; set; }
        public string approvalLevelName { get; set; }
        public int groupRoleId  { get; set; }
        public string approvalGroupName { get; set; }
        public int staffId { get; set; }
        public string staffName { get; set; }
        public int? vote { get; set; }
        public string comment { get; set; }
    }
}