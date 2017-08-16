using FintrakBanking.ViewModels.Business;
using FintrakBanking.ViewModels.Credit;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FintrakBanking.Interfaces.Credit
{
    public interface ILoanPrelimenaryEvaluationRepository
    {
        bool AddPrelimenaryEvaluation(LoanPrelimenaryEvaluationViewModel model);

        IEnumerable<LoanPrelimenaryEvaluationViewModel> GetPrelimenaryEvaluationsAwaitingApproval(int staffId, int companyId);

        bool GoForApproval(ApprovalViewModel entity);
    }
}
