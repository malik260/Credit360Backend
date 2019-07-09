using FintrakBanking.ViewModels.Credit;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FintrakBanking.Interfaces.Credit
{
    public interface ITestCollateralRepository
    {
        CollateralViewModel GetCustomerCollateralByCollateralId(int collateralId, int typeId);
    }
}
