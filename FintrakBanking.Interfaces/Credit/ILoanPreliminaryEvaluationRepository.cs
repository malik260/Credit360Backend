using FintrakBanking.ViewModels;
using FintrakBanking.ViewModels.Credit;
using FintrakBanking.ViewModels.WorkFlow;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace FintrakBanking.Interfaces.Credit
{
    public interface ILoanPreliminaryEvaluationRepository
    {
        IEnumerable<LoanPreliminaryEvaluationViewModel> GetLoanPreliminaryEvaluationMappedToApplication();
        Task<LoanPreliminaryEvaluationViewModel> AddPreliminaryEvaluation(LoanPreliminaryEvaluationViewModel model);

        Task<LoanPreliminaryEvaluationViewModel> AddMultiplePreliminaryEvaluation(List<LoanPreliminaryEvaluationViewModel> model);

        IEnumerable<LoanPreliminaryEvaluationViewModel> GetSingleCustomerPreliminaryEvaluationsAwaitingApproval(int staffId, int companyId);

        IEnumerable<LoanPreliminaryEvaluationViewModel> GetGroupCustomerPreliminaryEvaluationsAwaitingApproval(
            int staffId, int companyId);

        bool GoForApproval(ApprovalViewModel entity);

        IEnumerable<LoanPreliminaryEvaluationViewModel> GetAllSingleCustomerLoanPreliminaryEvaluations();
        
        IEnumerable<LoanPreliminaryEvaluationViewModel> GetAllGroupCustomerLoanPreliminaryEvaluations();

        Task<bool> UpdatePreliminaryEvaluation(int loanPenId, LoanPreliminaryEvaluationViewModel model);

        bool SendPreliminaryEvaluationForLoanApplication(int loanPenId, LoanPreliminaryEvaluationViewModel model);

        IEnumerable<LoanPreliminaryEvaluationViewModel> GetLoanPreliminaryEvaluationsByLoanTypeId(int loanTypeId);

        IEnumerable<LoanPreliminaryEvaluationViewModel> GetLoanApplicationPreliminaryEvaluations(int applicationId);

        IEnumerable<LoanPreliminaryEvaluationViewModel> GetLoanPreliminaryEvaluationsAwaitingApprovalByLoanTypeId(
            int staffId, int companyId, int loanTypeId);
        IEnumerable<LookupViewModel> GetCustomerLoanPreliminaryEvaluations(int customerId, int loanTypeId, int customerGroupId = 0);
    }
}
