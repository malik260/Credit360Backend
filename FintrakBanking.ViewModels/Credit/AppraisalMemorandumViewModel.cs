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
        public decimal amount { get; set; }
        public bool politicallyExposed { get; set; }
        public bool vote { get; set; }
        public string comment { get; set; }
        public decimal principal { get; set; }
        public double rate { get; set; }
        public int tenor { get; set; }
        public bool investmentGrade { get; set; }
    }

    public class ApprovedLoanDetailViewModel : GeneralEntity
    {
        public int applicationId { get; set; }
        public decimal principal { get; set; }
        public double rate { get; set; }
        public int tenor { get; set; }
        public string approver { get; set; }
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
    }

    public class DocumentationViewModel : GeneralEntity
    {
        public int documentationId { get; set; }
        public string documentation { get; set; }
        public int appraisalMemorandumId { get; set; }
        public int approvalLevelId { get; set; }
    }
}
