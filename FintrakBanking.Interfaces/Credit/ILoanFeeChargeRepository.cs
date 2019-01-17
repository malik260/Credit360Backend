using FintrakBanking.Common.Enum;
using FintrakBanking.Interfaces.WorkFlow;
using FintrakBanking.ViewModels;
using FintrakBanking.ViewModels.Credit;
using FintrakBanking.ViewModels.WorkFlow;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FintrakBanking.Interfaces.Credit
{
    public interface ILoanFeeChargeRepository
    {
        string SubmitTakeFee(LoanFeeChargesViewModel entity);
        IEnumerable<LoanReviewOperationApprovalViewModel> GetTakeFeeAwaitingApproval(int staffId, int companyId);
        ApprovalStatusEnum ApproveTakeFee(ApprovalViewModel userModel);

    }
}
