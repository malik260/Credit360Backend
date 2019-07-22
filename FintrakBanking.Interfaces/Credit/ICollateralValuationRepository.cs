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
        List<CollateralValuationViewModel> GetAllCollateralValuations(int collateralId);
        List<ValuationPrerequisiteViewModel> GetAllValuationPrerequisitesById(int collateralValuationId);
        bool GoForCollateralValuationApproval(CollateralValuationViewModel entity);

        IEnumerable<ValuationPrerequisiteViewModel> GetAllValuationRequest(int staffId);

        IEnumerable<ValuationPrerequisiteViewModel> GetCollateralValuationRequestWaitingForApproval(int staffId);

        bool SubmitApproval(CollateralValuationViewModel model);

        bool AddCollateralValurerInfo(ValuationPrerequisiteViewModel model);
        List<ValuationPrerequisiteViewModel> GetAllCollateralValuerIformation();

        List<ValuationPrerequisiteViewModel> GetAllCollateralValuerIformation(int id);
    }
}
