using FintrakBanking.ViewModels;
using FintrakBanking.ViewModels.Customer;
using System.Collections.Generic;
using System.Threading.Tasks;
using FintrakBanking.ViewModels.CreditLimitValidations;
using FintrakBanking.ViewModels.Credit;

namespace FintrakBanking.Interfaces.CreditLimitValidations
{
    public interface ICreditLimitValidationsRepository
    {
        int ValidateBlackList(string customerCode);
        // int ValidateBlackList(int customerId);

        int ValidateWatchList(int customerId);

        //int ValidateCamsol(int customerId);

        //IEnumerable<CustomerEligibilityViewModel> ValidateCamsol(string customerCode);

        IEnumerable<CustomerEligibilityViewModel> ValidateCustomerEligibility(string customerCode);

        CreditLimitValidationsModel ValidateAmountByBranch(short branchId);
        CreditLimitValidationsModel ValidateNPLByBranch(short branchId);
        CreditLimitValidationsModel ValidateAmountBySector(int customerId);
        CreditLimitValidationsModel ValidateNPLBySector(int subSectorId);
        CreditLimitValidationsModel ValidateAmountByCustomer(int subSectorId);
        CreditLimitValidationsModel ValidateNPLByCustomer(int customerId);
        CreditLimitValidationsModel ValidateAmountByCustomerGroup(int customergroupId);
        CreditLimitValidationsModel ValidateNPLByCustomerGroup(int customergroupId);
        CreditLimitValidationsModel ValidateCreditLimitNPLByRMBM(short relationshipofficerId);
        CreditLimitValidationsModel ValidateAmountBySegment(short segmentId);
        CreditLimitValidationsModel ValidateNPLBySegment(short segmentId);

        IEnumerable<ObligorLimitViewModel> GetAllObligorLimit();
        bool ValidateRiskRating(string riskRating);
        bool AddUpdateRiskRating(ObligorLimitViewModel entity);
    }
}