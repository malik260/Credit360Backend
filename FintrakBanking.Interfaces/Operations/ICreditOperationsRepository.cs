using FintrakBanking.ViewModels;
using FintrakBanking.ViewModels.Customer;
using System.Collections.Generic;
using System.Threading.Tasks;
//using FintrakBanking.ViewModels.Operations;

namespace FintrakBanking.Interfaces.CreditOperations
{
    public interface ICreditOperationsRepository
    {
        decimal GetCollateralSearchChargeAmount(int stateId);
    }
}