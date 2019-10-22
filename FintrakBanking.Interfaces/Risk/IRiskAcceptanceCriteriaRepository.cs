using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FintrakBanking.ViewModels;
using FintrakBanking.ViewModels.Risk;
using FintrakBanking.ViewModels.Setups.Approval;

namespace FintrakBanking.Interfaces.Risk
{
    public interface IRiskAcceptanceCriteriaRepository
    {
        #region
        //RiskAcceptanceCriteriaViewModel GetDynamicRiskAcceptanceCriteriaByProduct(RiskAcceptanceCriteriaViewModel model);
        RiskAcceptanceCriteriaViewModel GetRiskAcceptanceCriteriaByProduct(RiskAcceptanceCriteriaViewModel model);

        RiskAcceptanceCriteriaViewModel GetRiskAcceptanceCriteriaByProductAndTarget(int productId, int targetId);

        #endregion

        #region IRacCategoryRepository
        RacCategoryViewModel GetRacCategory(int id);

        IEnumerable<RacCategoryViewModel> GetRacCategorys();

        bool AddRacCategory(RacCategoryViewModel model);

        bool UpdateRacCategory(RacCategoryViewModel model, int id, UserInfo user);

        bool DeleteRacCategory(int id, UserInfo user);
        #endregion

        #region IRacDefinitionRepository
        RacDefinitionViewModel GetRacDefinition(int id);

        IEnumerable<RacDefinitionViewModel> GetRacDefinitions();

        bool AddRacDefinition(RacDefinitionViewModel model);

        bool UpdateRacDefinition(RacDefinitionViewModel model, int id, UserInfo user);

        bool DeleteRacDefinition(int id, UserInfo user);
        #endregion

        #region IRacDetailRepository
        RacDetailViewModel GetRacDetail(int id);

        IEnumerable<RacDetailViewModel> GetRacDetails();

        bool AddRacDetail(RacDetailViewModel model);

        bool UpdateRacDetail(RacDetailViewModel model, int id, UserInfo user);

        bool DeleteRacDetail(int id, UserInfo user);
        #endregion

        #region IRacInputTypeRepository
        RacInputTypeViewModel GetRacInputType(int id);

        IEnumerable<RacInputTypeViewModel> GetRacInputTypes();

        bool AddRacInputType(RacInputTypeViewModel model);

        bool UpdateRacInputType(RacInputTypeViewModel model, int id, UserInfo user);

        bool DeleteRacInputType(int id, UserInfo user);

        IEnumerable<RacItemViewModel> GetRacItem(string searchQuery);
        #endregion

        #region RacItemRepository

        RacItemViewModel GetRacItem(int id);

        IEnumerable<RacItemViewModel> GetRacItems();

        bool AddRacItem(RacItemViewModel model);

        bool UpdateRacItem(RacItemViewModel model, int id, UserInfo user);

        bool DeleteRacItem(int id, UserInfo user);

        #endregion

        #region RacOption
        RacOptionViewModel GetRacOption(int id);

        IEnumerable<RacOptionViewModel> GetRacOptions();

        bool AddRacOption(RacOptionViewModel model);

        bool UpdateRacOption(RacOptionViewModel model, int id, UserInfo user);

        bool DeleteRacOption(int id, UserInfo user);
        #endregion

        #region IRacOptionItemRepository
        RacOptionItemViewModel GetRacOptionItem(int id);

        IEnumerable<RacOptionItemViewModel> GetRacOptionItems();

        bool AddRacOptionItem(RacOptionItemViewModel model);

        bool UpdateRacOptionItem(RacOptionItemViewModel model, int id, UserInfo user);

        bool DeleteRacOptionItem(int id, UserInfo user);

        #endregion

        #region RacCategoryType
        IEnumerable<RacCategoryTypeViewModel> GetAllRacCategoryType();
        RacCategoryTypeViewModel GetRacCategoryTypeById(int id);
        bool DeleteRacCategoryTypeById(int id);
        bool UpdateRacCategoryTypeById(RacCategoryTypeViewModel model, int id);
        bool AddRacCategoryType(RacCategoryTypeViewModel model);
        #endregion

        IEnumerable<ConditionalOperatorViewModel> GetConditionalOperators();
        IEnumerable<DefinedFunctionViewModel> GetDefinedFunctions();
        IEnumerable<ApprovalLevelViewModel> GetApprovalLevel(int companyId);

        IEnumerable< RacCategoryViewModel> GetRacCategoryType(int id);

        List<RacCategoryViewModel> GetRacCategoryTypes(int productId);

        //IEnumerable<RacDefinitionViewModel> GetRacForLoanApplication(int loanApplicationId);
        RiskAcceptanceCriteriaViewModel GetSavedRiskAcceptanceCriteria(int productId, int? targetId);

        bool RacCategoryTypeExist(int productid, int racCategoryTypeId);

        List<RacCategoryTypeViewModel> GetRacDetails(int targetId);
       




    }
}
