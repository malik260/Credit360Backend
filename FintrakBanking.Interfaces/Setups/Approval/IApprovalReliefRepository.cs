using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FintrakBanking.ViewModels;
using FintrakBanking.ViewModels.Setups.Approval;

namespace FintrakBanking.Interfaces.Setups.Approval
{
    public interface IApprovalReliefRepository
    {
        bool AddApprovalRelief(ApprovalReliefViewModel model);
        IEnumerable<ApprovalReliefViewModel> GetAllApprovalRelief(int companyId);
        bool UpdateApprovalRelief(int reliefId, ApprovalReliefViewModel model);
    }
}
