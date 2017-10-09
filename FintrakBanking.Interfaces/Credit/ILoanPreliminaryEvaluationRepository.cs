using FintrakBanking.ViewModels.Credit;
using FintrakBanking.ViewModels.WorkFlow;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace FintrakBanking.Interfaces.Credit
{
    public interface ILoanPreliminaryEvaluationRepository
    {
        Task<LoanPreliminaryEvaluationViewModel> AddPreliminaryEvaluation(LoanPreliminaryEvaluationViewModel model);

        Task<LoanPreliminaryEvaluationViewModel> AddMultiplePreliminaryEvaluation(List<LoanPreliminaryEvaluationViewModel> model);

        IEnumerable<LoanPreliminaryEvaluationViewModel> GetPreliminaryEvaluationsAwaitingApproval(int staffId, int companyId);

        bool GoForApproval(ApprovalViewModel entity);

        IEnumerable<LoanPreliminaryEvaluationViewModel> GetAllLoanPreliminaryEvaluations();

        Task<bool> UpdatePreliminaryEvaluation(int loanPenId, LoanPreliminaryEvaluationViewModel model);

        bool SendPreliminaryEvaluationForLoanApplication(int loanPenId, LoanPreliminaryEvaluationViewModel model);
    }
}
