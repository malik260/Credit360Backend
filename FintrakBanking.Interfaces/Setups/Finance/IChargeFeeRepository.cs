using FintrakBanking.ViewModels;
using FintrakBanking.ViewModels.Setups.Finance;
using FintrakBanking.ViewModels.WorkFlow;
using System.Collections.Generic;

namespace FintrakBanking.Interfaces.Setups.Finance
{
    public interface IChargeFeeRepository
    {
        bool GoForApproval(ApprovalViewModel entity);

        IEnumerable<ChargeFeeViewModel> GetChargeFeeAwaitingApprovals(int staffId, int companyId); 

        ChargeFeeViewModel GetChargeFee(int chargeFeeId);

        IEnumerable<ChargeFeeViewModel> GetAllChargeFee();

        IEnumerable<ChargeFeeViewModel> GetAllChargeFeeByCompanyId(int companyId);

        IEnumerable<LookupViewModel> GetAllPostingType();
        IEnumerable<LookupViewModel> GetAllFeeType();
        IEnumerable<LookupViewModel> GetAllCRMSFeeType();

        IEnumerable<LookupViewModel> GetAllChargeFeeDetailType();
        IEnumerable<LookupViewModel> GetAllChargeFeeDetailClass();

        bool AddChargeFee(ChargeFeeViewModel model);

        bool UpdateChargeFee(ChargeFeeViewModel model, int chargeFeeId);

        bool DeleteChargeFee(int chargeFeeId, UserInfo user);
    }

}
