using FintrakBanking.Interfaces.WorkFlow;
using FintrakBanking.ViewModels;
using FintrakBanking.ViewModels.Credit;
using System.Collections.Generic;

namespace FintrakBanking.Interfaces.Credit
{
    public interface IFacilityModificationRepository
    {
        FacilityModificationViewModel GetFacilityModification(int id);

        IEnumerable<FacilityModificationViewModel> GetFacilityModificationsForApproval(int staffId);
        WorkflowResponse AddFacilityModification(FacilityModificationViewModel model);
        WorkflowResponse ApproveFacilityModification(ForwardViewModel model);
        bool UpdateFacilityModification(FacilityModificationViewModel model, int id, UserInfo user);

        bool DeleteFacilityModification(int id, UserInfo user);
    }
}
