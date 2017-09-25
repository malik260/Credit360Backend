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

namespace FintrakBanking.Interfaces.Credit
{
    public interface ILoanApplicationRepository
    {
        bool CheckExistingCertificateOfOwnership(string certificateOfOwnership, int companyId);

        IEnumerable<ExistingLoanApplicationViewModel> ExistingLoanApplication(int customerId, int companyId);

        IEnumerable<LoanApplicationViewModel> GetAllLoanApplications(int companyId);

        IEnumerable<LoanApplicationViewModel> GetLoanApplicationById(int loanApplicationId, int companyId);

        IEnumerable<ProductClassViewModel> GetProductClass();

        IQueryable<LoanApplicationViewModel> GetPendingLoanApplications(int countryId, int branchId, int staffId);

        IEnumerable<LoanApplicationViewModel> FindLoanApplication(string referenceNumberOrName, int companyId);

        Task<bool> UpdateApprovalStatus(ApprovalViewModel entity);

        Task<bool> AddLoanApplication(LoanApplicationViewModel loan);

        IEnumerable<CamProcessedLoanViewModel> GetCamProcessedLoanApplications(int companyId);

        bool UpdateLoanApplicationStatus(string applicationRefNumber, short applicationStatusId);

        IEnumerable<CamProcessedLoanViewModel> GetApplicationsForReviewFromCreditUnit(int companyId);

        IEnumerable<CamProcessedLoanViewModel> GetCamProcessedLoanApplicationsDueForAvailment(int companyId);
    }
}
