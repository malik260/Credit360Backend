using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FintrakBanking.ViewModels;
using FintrakBanking.ViewModels.Credit;

namespace FintrakBanking.Interfaces.Credit
{
    public interface IDeferredFeeRepository
    {
        LoanChargeFeeViewModel GetDeferredFee(int id);

        IEnumerable<LoanChargeFeeViewModel> GetDeferredFees();

        IEnumerable<LoanChargeFeeViewModel> GetLoanDetailDeferredFees(int loanDetailId);

        bool AddDeferredFee(List<LoanChargeFeeViewModel> model, UserInfo user);

        bool UpdateDeferredFee(LoanChargeFeeViewModel model, int id, UserInfo user);

        bool DeleteDeferredFee(int id, UserInfo user);
    }
}