using FintrakBanking.ViewModels;
using FintrakBanking.ViewModels.Customer;
using System.Collections.Generic;
using System.Threading.Tasks;
using FintrakBanking.ViewModels.CreditLimitValidations;

namespace FintrakBanking.Interfaces.CreditLimitValidations
{
    public interface ICreditLimitValidationsRepository
    {
        int ValidateBlackList(int customerId);

        int ValidateWatchList(int customerId);

        int ValidateCamsol(int customerId);
    
        CreditLimitValidationsModel ValidateAmountByBranch(short branchId);
        CreditLimitValidationsModel ValidateNPLByBranch(short branchId);
        CreditLimitValidationsModel ValidateAmountBySector(int customerId);
        CreditLimitValidationsModel ValidateNPLBySector(int customerId);
        CreditLimitValidationsModel ValidateAmountByCustomer(int customerId);
        CreditLimitValidationsModel ValidateNPLByCustomer(int customerId);
        CreditLimitValidationsModel ValidateAmountByCustomerGroup(int customergroupId);
        CreditLimitValidationsModel ValidateNPLByCustomerGroup(int customergroupId);
        CreditLimitValidationsModel ValidateCreditLimitNPLByRMBM(short relationshipofficerId);

    }
}