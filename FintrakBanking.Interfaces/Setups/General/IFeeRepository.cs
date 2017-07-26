using FintrakBanking.ViewModels;
using FintrakBanking.ViewModels.Setups.General;
using System.Collections.Generic;

namespace FintrakBanking.Interfaces.Setups.General
{
    public interface IFeeRepository
    {
        IEnumerable<FeeViewModel> GetAllFee();

        FeeViewModel GetFeeViewModel(int feeId);

        int AddFee(FeeViewModel fee);

        bool UpdateFee(int feeId, FeeViewModel fee);

        IEnumerable<LookupViewModel> GetFeeAccountCategory();

        IEnumerable<LookupViewModel> GetFeeType();

        IEnumerable<LookupViewModel> GetFeeInterval();

        IEnumerable<LookupViewModel> GetFeeTarget();
    }
}