using FintrakBanking.Interfaces.WorkFlow;
using FintrakBanking.ViewModels.Credit;
using FintrakBanking.ViewModels.WorkFlow;
using System.Collections.Generic;

namespace FintrakBanking.Interfaces.Credit
{
    public interface ILoanFeeChargeRepository
    {
        WorkflowResponse SubmitTakeFee(LoanFeeChargesViewModel entity);
        IEnumerable<LoanReviewOperationApprovalViewModel> GetTakeFeeAwaitingApproval(int staffId, int companyId);
        WorkflowResponse ApproveTakeFee(ApprovalViewModel userModel);

    }
}
