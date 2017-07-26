using FintrakBanking.ViewModels;
using FintrakBanking.ViewModels.Setups.Approval;
using System;
using System.Collections.Generic;
using System.Text;

namespace FintrakBanking.Interfaces.Setups.Approval
{
    public interface IApprovalGroupRepository
    {
        IEnumerable<ApprovalGroupViewModel> GetAllApprovalGroup(int companyId);
        IEnumerable<ApprovalGroupViewModel> GetApprovalGroupById(int GroupId, int companyId);
        bool AddApprovalGroup(ApprovalGroupViewModel model);
        bool UpdateApprovalGroup(int GroupId, ApprovalGroupViewModel model);
        bool DeleteApprovalGroup(int GroupId, UserInfo user);
    }
}
