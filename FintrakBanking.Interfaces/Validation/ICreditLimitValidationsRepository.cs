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
        IEnumerable<ContractorCriteriaViewModel> getContractorTieringForEdit(int contractorTieringId);
        IEnumerable<ContractorCriteriaOptionViewModel> getAllContractorCriteriaOption();
        bool UpdateContractorCriteriaOption(ContractorCriteriaOptionViewModel entity);
        bool AddContractorCriteriaOption(ContractorCriteriaOptionViewModel entity);
        IEnumerable<ContractorCriteriaViewModel> getAllCriteriaList();
        IEnumerable<ProjectRiskRatingViewModel> getProjectRiskRatingByApplicationDetailId(int loanApplicationId, int loanApplicationDetailId, int loanBookingRequestId);
        IEnumerable<ProjectRiskRatingViewModel> getProjectRiskRatingByApplicationAndApplicationDetailId(int loanApplicationId, int loanApplicationDetailId);
        IEnumerable<ProjectRiskRatingCategoryViewModel> getAllProjectRiskRatingByCategories();
        IEnumerable<ContractorTieringViewModel> getContractorTieringByApplicationAndCustomer(int loanApplicationId, int customerId);
        IEnumerable<ContractorTieringViewModel> getContractorTieringByApplication(int loanApplicationId, int customerId);
        IEnumerable<ContractorCriteriaViewModel> getAllContractorCriteria();
        IEnumerable<ProjectRiskRatingCriteriaViewModel> getAllProjectRiskRatingCriteria();
        bool UpdateProjectRiskCategory(ProjectRiskRatingCategoryViewModel entity);
        bool AddProjectRiskCategory(ProjectRiskRatingCategoryViewModel entity);
        bool UpdateProjectRiskCriteria(ProjectRiskRatingCriteriaViewModel entity);
        bool AddProjectRiskCriteria(ProjectRiskRatingCriteriaViewModel entity);
        bool UpdateContractorCriteria(ContractorCriteriaViewModel entity);
        IEnumerable<ProjectRiskRatingCategoryViewModel> getAllProjectRiskRatingCategories();
        bool AddContractorCriteria(ContractorCriteriaViewModel entity);
        CreditLimitValidationsModel ValidateAmountFacilityBySector(int sectorId);
        CreditLimitValidationsModel ValidateNPLByGroupFirstTwenty(LoanApplicationViewModel application);
        CreditLimitValidationsModel ValidateNPLByGroupFirstHundred(LoanApplicationViewModel application);
        CreditLimitValidationsModel ValidateNPLByCurrency(LoanApplicationViewModel application);
        IEnumerable<CurrencyLimitViewModel> GetAllCurrencyLimit();
        bool AddCurrencyLimits(CurrencyLimitViewModel entity);
        bool UpdateCurrencyLimits(CurrencyLimitViewModel entity);
        bool DeleteCurrencyLimit(int id, UserInfo user);
        IEnumerable<GroupLimitViewModel> GetAllGroupLimit();
        bool AddGroupLimits(GroupLimitViewModel entity);
        bool UpdateGroupLimits(GroupLimitViewModel entity);
        bool DeleteGroupLimit(int id, UserInfo user);
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
        bool ValidateIsInsiderCustomer(int customerId);
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