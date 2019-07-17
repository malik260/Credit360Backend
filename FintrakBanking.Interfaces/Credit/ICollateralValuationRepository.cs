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
        List<CollateralValuationViewModel> GetAllCollateralValuations(int collateralId);
        bool GoForCollateralValuationApproval(CollateralValuationViewModel entity);

        IEnumerable<CollateralValuationViewModel> GetAllValuationRequest(int staffId);

        IEnumerable<CollateralValuationViewModel> GetCollateralValuationRequestWaitingForApproval(int staffId);

        bool SubmitApproval(CollateralValuationViewModel model);

        bool AddCollateralValurerInfo(CollateralValuationViewModel model);
        List<CollateralValuationViewModel> GetAllCollateralValuerIformation();

        List<CollateralValuationViewModel> GetAllCollateralValuerIformation(int id);
    }
}
