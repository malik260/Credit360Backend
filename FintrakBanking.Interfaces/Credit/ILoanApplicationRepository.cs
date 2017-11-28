using FintrakBanking.Common;
using FintrakBanking.Common.Enum;
using FintrakBanking.ViewModels;
using FintrakBanking.ViewModels.WorkFlow;
using FintrakBanking.ViewModels.Credit;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using System.Linq;
using FintrakBanking.Entities.Models;

namespace FintrakBanking.Interfaces.Credit
{
    public interface ILoanApplicationRepository
    {
        bool CheckExistingCertificateOfOwnership(string certificateOfOwnership, int companyId);

        IEnumerable<ExistingLoanApplicationViewModel> ExistingLoanApplication(int customerId, int companyId);

        IEnumerable<LoanApplicationViewModel> GetAllLoanApplications(int companyId);

        IEnumerable<LoanApplicationViewModel> GetLoanApplicationById(int loanApplicationId, int companyId);

        IEnumerable<ProductClassViewModel> GetProductClass();

        bool UpdateApprovalStatusForApplication(int applocationId);

        IEnumerable<dynamic> GetLoanApplicationByRelationshipOfficerId(int relationshipOfficerId, int companyId);

        IEnumerable<LoanApplicationViewModel> FindLoanApplication(string referenceNumberOrName, int companyId);

        Task<bool> UpdateApprovalStatus(ApprovalViewModel entity);

        IEnumerable<LoanApplicationDetailViewModel> GetLoanApplicationsDetails(int loanApplicationId, int companyId);

        int AddLoanApplication(LoanApplicationViewModel loan);

        //IEnumerable<CamProcessedLoanViewModel> GetCamProcessedLoanApplications(int companyId);

        bool UpdateLoanApplicationStatus(string applicationRefNumber, short applicationStatusId);

        IEnumerable<CamProcessedLoanViewModel> GetApplicationsForReviewFromCreditUnit(int companyId);

        IEnumerable<CamProcessedLoanViewModel> GetApplicationsDueForAvailment(int staffId, int companyId);

        IEnumerable<CamProcessedLoanViewModel> GetApplicationsDueForOfferLetterGeneration(int companyId);

        IQueryable<LoanApplicationDetailViewModel> GetLoanApplicationsAwaitingCheckList(int companyId);

        IEnumerable<LoanApplicationViewModel> Search(string searchString);

        OfferLetterTemplateViewModel GenerateOfferLetterTemplate(string applicationRefNumber);

        OfferLetterTemplateViewModel GetDraftOfferLetterByApplRefNumber(string applicationRefNumber);

        IEnumerable<OfferLetterTemplateViewModel> GetAllDraftOfferLetters();

        bool SaveDraftOfferLetter(OfferLetterTemplateViewModel model);

        bool UpdateDraftOfferLetter(int documentId, OfferLetterTemplateViewModel model);

        bool AddLoanApplicationCollateral(List<LoanApplicationCollateralViewModel> entity);

        IEnumerable<LoanApplicationCollateralViewModel> GetLoanApplicationCollateral(int loanApplicatioinCollateralId);


        bool SaveFinalOfferLetter(OfferLetterTemplateViewModel model);

        bool ApproveLoanAvailmentDecision(LoanAvailmentApprovalViewModel entity);

        IEnumerable<OfferLetterTemplateViewModel> GetAllFinalOfferLetters();

        OfferLetterTemplateViewModel GetFinalOfferLetterByApplRefNumber(string applicationRefNumber);

        bool LogApplicationForApproval(LoanAvailmentApprovalViewModel model);

        Form3800ViewModel GenerateForm3800Template(string applicationRefNumber);
    }
}