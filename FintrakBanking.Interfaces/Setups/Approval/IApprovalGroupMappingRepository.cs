using FintrakBanking.ViewModels;
using FintrakBanking.ViewModels.Setups.Approval;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace FintrakBanking.Interfaces.Setups.Approval
{
    public interface IApprovalGroupMappingRepository
    {
        int AddApprovalGroupMapping(ApprovalGroupMappingViewModel entity);

        bool UpdateApprovalGroupMapping(int operationMappingId, ApprovalGroupMappingViewModel entity);

        bool DeleteApprovalGroupMapping(int operationMappingId, UserInfo user);

        IQueryable<ApprovalGroupMappingViewModel> GetAllApprovalGroupMapping();

        ApprovalGroupMappingViewModel GetApprovalGroupMapping(int operationMappingId);

        IEnumerable<ApprovalGroupMappingViewModel> GetApprovalGroupMapping(int operationId, short? productClassId, short? productId);
        int GoForApproval(ApprovalGroupMappingViewModel model);
        List<ApprovalGroupMappingViewModel> GetTempApprovalGroupForApproval(int staffId);

    }
}
