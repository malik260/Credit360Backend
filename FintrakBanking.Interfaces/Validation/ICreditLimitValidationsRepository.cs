using FintrakBanking.ViewModels;
using FintrakBanking.ViewModels.Customer;
using System.Collections.Generic;
using System.Threading.Tasks;
using FintrakBanking.ViewModels.CreditLimitValidations;
using FintrakBanking.ViewModels.Credit;
using FintrakBanking.ViewModels.Setups.General;

namespace FintrakBanking.Interfaces.CreditLimitValidations
{
    public interface ICreditLimitValidationsRepository
    {
        int ValidateBlackList(string customerCode);
        // int ValidateBlackList(int customerId);

        int ValidateWatchList(int customerId);
        bool IsDirectorRelatedGroup(int? customerGroupId);
        bool CustomerIsDirector(int? customerId);
        CreditLimitValidationsModel ValidateNPLByDirectors(LoanApplicationViewModel application);
        //int ValidateCamsol(int customerId);

        //IEnumerable<CustomerEligibilityViewModel> ValidateCamsol(string customerCode);
        CreditLimitValidationsModel ValidateNPLByInsiderCustomer();
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
        CreditLimitValidationsModel ValidateSingleObligorLimit(LoanApplicationViewModel application);

        IEnumerable<ObligorLimitViewModel> GetAllObligorLimit();
        bool ValidateRiskRating(string riskRating);
        bool AddUpdateRiskRating(ObligorLimitViewModel entity);
        bool DeleteRiskRating(int id, UserInfo user);
        CreditLimitValidationsModel ValidateCreditLimitByRMBM(short relationshipofficerId);
        bool UpdateCustomerRating(ObligorLimitViewModel entity);
        bool UpdateApplicationCustomerRating(ObligorLimitViewModel entity);
        CreditLimitValidationsModel ValidateApplicationCustomerRating(ObligorLimitViewModel entity);

        CustomerEligibility GetCustomerEligibility(string customerCode);
        bool BranchLimitExceeded(int branchId, decimal applicationAmount);
        bool SectorLimitExceeded(int sectorId, decimal applicationAmount);
        bool ProductLimitExceeded(int productId, decimal applicationAmount);
        TotalExposureLimit GetTotalExposureLimit(ExposureLimitRequestModel model);
        TotalExposureLimit GetTotalExposureLimitReference(string reference, int getCompanyId);
    }
}