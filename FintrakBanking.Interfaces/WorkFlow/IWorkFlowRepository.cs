using FintrakBanking.ViewModels.Business;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace FintrakBanking.Interfaces.WorkFlow
{
    public interface IWorkFlowRepository
    {
        Task<Tuple<bool, ApprovalViewModel>> GoForApproval(ApprovalViewModel entity);

        Tuple<bool, ApprovalViewModel> LogForApproval(ApprovalViewModel entity);
    }
}
