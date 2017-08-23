using FintrakBanking.ViewModels.Business;
using FintrakBanking.ViewModels.Credit;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FintrakBanking.Interfaces.Credit
{
    public interface ILoanPreliminaryEvaluationRepository
    {
        bool AddPreliminaryEvaluation(LoanPreliminaryEvaluationViewModel model);

        IEnumerable<LoanPreliminaryEvaluationViewModel> GetPreliminaryEvaluationsAwaitingApproval(int staffId, int companyId);

        bool GoForApproval(ApprovalViewModel entity);

        IEnumerable<LoanPreliminaryEvaluationViewModel> GetAllLoanPreliminaryEvaluations();
    }
}
