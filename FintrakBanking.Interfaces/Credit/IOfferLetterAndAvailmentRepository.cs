using FintrakBanking.ViewModels.Credit;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FintrakBanking.ViewModels;

namespace FintrakBanking.Interfaces.Credit
{
    public interface IOfferLetterAndAvailmentRepository
    {
        bool UpdateLoanApplicationStatus(string applicationRefNumber, short applicationStatusId);

        IQueryable<CamProcessedLoanViewModel> GetApplicationsForReviewFromCreditUnit(int staffId, int branchId, int companyId);

        IEnumerable<CamProcessedLoanViewModel> GetApplicationsDueForAvailmentCheckList(int companyId);

        IQueryable<CamProcessedLoanViewModel> GetApplicationsDueForAvailment(int staffId, int companyId);

        IQueryable<CamProcessedLoanViewModel> GetApplicationsDueForOfferLetterGeneration(int staffId, int companyId);

        OfferLetterTemplateViewModel GenerateOfferLetterTemplate(string applicationRefNumber);

        OfferLetterTemplateViewModel GetDraftOfferLetterByApplRefNumber(string applicationRefNumber);

        IEnumerable<OfferLetterTemplateViewModel> GetAllDraftOfferLetters();

        bool SaveDraftOfferLetter(OfferLetterTemplateViewModel model);

        bool UpdateDraftOfferLetter(int documentId, OfferLetterTemplateViewModel model);

        bool SaveFinalOfferLetter(OfferLetterTemplateViewModel model);

        bool ApproveLoanAvailmentDecision(LoanAvailmentApprovalViewModel entity );

        IEnumerable<OfferLetterTemplateViewModel> GetAllFinalOfferLetters();

        OfferLetterTemplateViewModel GetFinalOfferLetterByApplRefNumber(string applicationRefNumber);

        bool LogApplicationForApprovalDuringAvailment(LoanAvailmentApprovalViewModel model);

        Form3800ViewModel GenerateForm3800Template(string applicationRefNumber);

        bool ApproveOfferLetterGeneration(LoanAvailmentApprovalViewModel entity);

        IQueryable<CamProcessedLoanViewModel> GetApplicationsUnderForReview(int companyId);

        bool ForwardBondsAndGuarantee(ForwardViewModel entity);

        bool UpdateFinalOfferLetter(string applicationRef, OfferLetterTemplateViewModel model);

        bool OfferLetterRejection(ForwardViewModel entity);

        IEnumerable<CommentOnLoanAvailmentViewModel> GetCommentOnLoanAvailment(string applicationRefNumber);

        //IEnumerable<LoanApplicationCollateralViewModel> GetLoanCollateral(int loanApplicationDetailId);

        Form3800ViewModel GenerateForm3800TemplateLMS(string refNumber);
    }
}
