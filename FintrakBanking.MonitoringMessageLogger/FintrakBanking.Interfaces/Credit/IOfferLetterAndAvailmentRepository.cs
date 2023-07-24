using FintrakBanking.ViewModels.Credit;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FintrakBanking.ViewModels;
using FintrakBanking.Interfaces.WorkFlow;
using FintrakBanking.ViewModels.WorkFlow;

namespace FintrakBanking.Interfaces.Credit
{
    public interface IOfferLetterAndAvailmentRepository
    {
        IQueryable<CamProcessedLoanViewModel> GetApplicationsAtOfferLetter(int staffId, int companyId);
        bool AddCRMSCollateralType(int applicationId, ApprovedLoanDetailViewModel model);

        IQueryable<CamProcessedLoanViewModel> GetApplicationsAtOfferLetter(int staffId, int branchId, int companyId);

        IQueryable<CamProcessedLoanViewModel> GetApplicationsDueForAvailment(int staffId, int companyId);

        OfferLetterTemplateViewModel GenerateOfferLetterTemplate(string applicationRefNumber);

        OfferLetterTemplateViewModel GetDraftOfferLetterByApplRefNumber(string applicationRefNumber);

        IEnumerable<OfferLetterTemplateViewModel> GetAllDraftOfferLetters();

        bool SaveDraftOfferLetter(OfferLetterTemplateViewModel model);

        bool UpdateDraftOfferLetter(int documentId, OfferLetterTemplateViewModel model);

        bool SaveFinalOfferLetter(int loanApplicationId, OfferLetterTemplateViewModel model);

        int ApproveLoanAvailmentDecision(LoanAvailmentApprovalViewModel entity);

        IQueryable<OfferLetterTemplateViewModel> GetAllFinalOfferLetters();

        OfferLetterTemplateViewModel GetFinalOfferLetterByApplRefNumber(int loanApplicationId);

        //bool LogApplicationForApprovalDuringAvailment(LoanAvailmentApprovalViewModel model);

        Form3800ViewModel GenerateForm3800Template(string applicationRefNumber);

        WorkflowResponse ApproveOfferLetterGeneration(LoanAvailmentApprovalViewModel entity);

        bool ForwardBondsAndGuarantee(ForwardViewModel entity);

        bool UpdateFinalOfferLetter(int loanApplicationId, OfferLetterTemplateViewModel model);

        bool OfferLetterRejection(ForwardViewModel entity);

       // bool OfferLetterReferBack(ApprovalViewModel model);

        IEnumerable<CommentOnLoanAvailmentViewModel> GetCommentOnLoanAvailment(string applicationRefNumber);

        Form3800ViewModel GenerateForm3800TemplateLMS(string refNumber);

        bool SendBackToBusinessAvailment(LoanAvailmentApprovalViewModel entity);

       void AddOfferLetterClauses(int applicationId, int staffId,bool isLMS, bool callSaveChanges);
        bool EditOfferLetterTitle(int custimerId, string data, int staffId, int branchId);
        bool EditOfferLetterSalutation(int custimerId, string data, int staffId, int branchId);
        bool EditOfferLetterAcceptance(int applicationId, string data, bool isLMS, int staffId, int branchId);
        bool EditOfferLetterClause(int applicationId, string data, bool isLMS, int staffId, int branchId);

        OfferLetterViewModel GetOfferLetterTitle(int custimerId);
        OfferLetterViewModel GetOfferLetterSalutation(int custimerId);
        OfferLetterViewModel GetOfferLetterAcceptance(int applicationId);
        OfferLetterViewModel GetOfferLetterClause(int applicationId);
        bool ReferBackOneStep(LoanAvailmentApprovalViewModel entity);
        List<CamProcessedLoanViewModel> GetApplicationsDueForAvailmentRoute(int staffId, int companyId);
        bool IsOfferLetterGenerated(int templateId, int loanApplicationId, int staffId, int branchId);
        bool ApplyTemplateToOfferLetter(int templateId, int loanApplicationId, int staffId, int branchId);
    }
}
