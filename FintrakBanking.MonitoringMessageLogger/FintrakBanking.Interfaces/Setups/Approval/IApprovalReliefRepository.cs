using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FintrakBanking.ViewModels;
using FintrakBanking.ViewModels.Setups.Approval;
using FintrakBanking.ViewModels.WorkFlow;

namespace FintrakBanking.Interfaces.Setups.Approval
{
    public interface IApprovalReliefRepository
    {
        Task<ApprovalReliefViewModel> AddApprovalRelief(ApprovalReliefViewModel model); 
        IEnumerable<ApprovalReliefViewModel> GetAllApprovalRelief(int companyId); 
        Task<bool> UpdateApprovalRelief(int reliefId, ApprovalReliefViewModel model);
        IEnumerable<ApprovalReliefViewModel> GetApprovalReliefAwaitingApprovals(int staffId, int companyId);
        bool GoForApproval(ApprovalViewModel entity);
        IEnumerable<ApprovalReliefViewModel> GetAllStaffRelief(int companyId, int staffId);
        bool AddStaffRelief(ApprovalReliefViewModel model);

        void UpdateAllApprovalRelief(int companyId);


    }
}
