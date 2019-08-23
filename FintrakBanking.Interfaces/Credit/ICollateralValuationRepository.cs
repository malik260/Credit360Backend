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
        bool GoForCollateralValuationApproval(ValuationPrerequisiteViewModel entity);

        IEnumerable<ValuationPrerequisiteViewModel> GetAllValuationRequest(int staffId);

        IEnumerable<ValuationPrerequisiteViewModel> GetCollateralValuationRequestWaitingForApproval(int staffId);

        bool SubmitApproval(ValuationPrerequisiteViewModel model);

        bool AddCollateralValurerInfo(ValuationPrerequisiteViewModel model);
        List<ValuationPrerequisiteViewModel> GetAllCollateralValuerIformation();

        List<ValuationPrerequisiteViewModel> GetAllCollateralValuerIformation(int id);

        bool DeleteValuationPrerequisite(int valuationPrerequisiteId, UserInfo user);

        List<ValuationPrerequisiteViewModel> GetCollateralValuationPrerequisiteById(int staffId, int valuationPrerequisiteId);
    }
}
