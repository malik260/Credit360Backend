using FintrakBanking.ViewModels.Credit;
using FintrakBanking.ViewModels.WorkFlow;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace FintrakBanking.Interfaces.Credit
{
    public interface ILoanPreliminaryEvaluationRepository
    {
        Task<bool> AddPreliminaryEvaluation(LoanPreliminaryEvaluationViewModel model);

        IEnumerable<LoanPreliminaryEvaluationViewModel> GetPreliminaryEvaluationsAwaitingApproval(int staffId, int companyId);

        Task<bool> GoForApproval(ApprovalViewModel entity);

        IEnumerable<LoanPreliminaryEvaluationViewModel> GetAllLoanPreliminaryEvaluations();

        Task<bool> UpdatePreliminaryEvaluation(int loanPenId, LoanPreliminaryEvaluationViewModel model);

        bool SendPreliminaryEvaluationForLoanApplication(int loanPenId, LoanPreliminaryEvaluationViewModel model);
    }
}
