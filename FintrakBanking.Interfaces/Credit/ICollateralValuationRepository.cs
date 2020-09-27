using FintrakBanking.Interfaces.WorkFlow;
using FintrakBanking.ViewModels;
using FintrakBanking.ViewModels.Credit;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FintrakBanking.Interfaces.Credit
{
    public interface ICollateralValuationRepository
    {
        CollateralValuationViewModel AddCollateralValuation(CollateralValuationViewModel model);
        ValuationPrerequisiteViewModel AddValuationPrerequisite(ValuationPrerequisiteViewModel model);
        bool UpdateValuationPrerequisite(int valuationPrerequisiteId, ValuationPrerequisiteViewModel model);
        CollateralValuationViewModel GetCollateralValuation(int collteralValuationId);
        List<CollateralValuationViewModel> GetAllCollateralValuations(int collateralId);
        List<ValuationPrerequisiteViewModel> GetAllValuationPrerequisitesById(int staffId, int collateralValuationId);
        WorkflowResponse GoForCollateralValuationApproval(ValuationPrerequisiteViewModel entity);
        String ResponseMessage(WorkflowResponse response, string itemHeading);

        IEnumerable<ValuationPrerequisiteViewModel> GetAllValuationRequest(int staffId);

        IEnumerable<ValuationPrerequisiteViewModel> GetCollateralValuationRequestWaitingForApproval(int staffId);

        WorkflowResponse SubmitApproval(ValuationPrerequisiteViewModel model);

        bool AddCollateralValurerInfo(ValuationPrerequisiteViewModel model);
        List<ValuationPrerequisiteViewModel> GetAllCollateralValuerIformation();

        List<ValuationPrerequisiteViewModel> GetAllCollateralValuerIformation(int id);

        bool DeleteValuationPrerequisite(int valuationPrerequisiteId, UserInfo user);

        List<ValuationPrerequisiteViewModel> GetCollateralValuationPrerequisiteById(int staffId, int valuationPrerequisiteId);

        bool UpdateValuationPrerequisiteStatus(int valuationPrerequisiteId, UserInfo user);
        List<ValuationPrerequisiteViewModel> SearchForCollateralValuation(string searchString);
    }
}
