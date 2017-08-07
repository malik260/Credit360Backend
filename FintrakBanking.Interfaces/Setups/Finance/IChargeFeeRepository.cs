using FintrakBanking.ViewModels;
using FintrakBanking.ViewModels.Setups.Finance;
using System.Collections.Generic;

namespace FintrakBanking.Interfaces.Setups.Finance
{
    public interface IChargeFeeRepository
    {
        ChargeFeeViewModel GetChargeFee(int chargeFeeId);

        IEnumerable<ChargeFeeViewModel> GetAllChargeFee();

        IEnumerable<ChargeFeeViewModel> GetAllChargeFeeByCompanyId(int companyId);

        bool AddChargeFee(ChargeFeeViewModel model);

        bool UpdateChargeFee(ChargeFeeViewModel model, int chargeFeeId);

        bool DeleteChargeFee(int chargeFeeId, UserInfo user);
    }

}
