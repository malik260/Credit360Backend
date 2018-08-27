using FintrakBanking.ViewModels;
using FintrakBanking.ViewModels.Credit;
using FintrakBanking.ViewModels.Setups.Credit;
using FintrakBanking.ViewModels.WorkFlow;
using System;
using System.Collections.Generic;
using System.Text;

namespace FintrakBanking.Interfaces.Setups
{
    public interface IChecklistRepository
    {
        #region Loan Checklist Definition
        IEnumerable<CheckListResponseTypeViewModel> GetAllChecklistResponseType();
        IEnumerable<CheckListTargetTypeViewModel> GetAllChecklistType();
        IEnumerable<ChecklistDefinitionViewModel> GetAllChecklistDefinition();
        IEnumerable<CheckListTargetTypeViewModel> GetChecklistTypeByApprovalLevel(int staffId, int companyId, int operationId, int productClassProcessId);
        IEnumerable<ChecklistDefinitionViewModel> GetAllMappedChecklistDefinitionByProductId(int productId);
        List<ChecklistDefinitionViewModel> GetAllChecklistDefinitionById(int CheckListDefinitionId);
        bool AddChecklistDefinition(ChecklistDefinitionViewModel model);
        bool AddMultipleChecklistDefinition(List<ChecklistDefinitionViewModel> models);
        bool AddMultipleChecklistDefinitionWithMultipleItems(ChecklistDefinitionViewModel model);
        bool UpdateChecklistDefinition(int CheckListDefinitionId, ChecklistDefinitionViewModel model);
        bool DeleteChecklistDefinition(int CheckListDefinitionId, UserInfo user);
        bool ValidateChecklistDetail(List<ValidateChecklistDetailViewModel> entity);
        IEnumerable<ChecklistDefinitionViewModel> GetAllMappedChecklistDefinitionByApprovalLevelAndProduct(int approvalLevelId, int productId);
        IEnumerable<ChecklistItemViewModel> GetAllUnmappedChecklistItemsToApprovalLevelAndProduct(int approvalLevelId, int productId);
        IEnumerable<ChecklistDefinitionViewModel> GetUnmappedChecklistDefintionToApprovalLevel(int approvalLevelId);
        // IEnumerable<ChecklistDefinitionViewModel> GetChecklistDefinitionByApprovalLevelCheckListType(int staffId, int? productId, int loanTargetId, int operationId, int checkListTypeId);
        IEnumerable<ChecklistDefinitionAndDetailViewModel> GetChecklistDefinitionByApprovalLevelCheckListType(int staffId, int? productId, int loanTargetId, int operationId, int checkListTypeId);
        #endregion

        #region Loan Checklist Detail
        IEnumerable<ChecklistDetailViewModel> GetAllChecklistDetail();
        IEnumerable<ChecklistDetailViewModel> GetChecklistByTargetId(int targetId);
        IEnumerable<ChecklistDetailViewModel> GetChecklistByCheckListTypeAndTargetId(int targetId, int checkListtypeId, bool isCamChecklist);
        // IEnumerable<ChecklistDetailViewModel> GetChecklistByCheckListTypeAndTargetId(int targetId, int checkListtypeId);
        List<ChecklistDetailViewModel> GetAllChecklistDetailById(int ChecklistId);
        List<ChecklistDetailViewModel> GetAllChecklistDetailByProductAndTargetId(int targetTypeId, int productId);
        List<ChecklistDetailViewModel> GetAllChecklistDetailByProductId(int targetId);
        List<ChecklistDetailViewModel> GetAllChecklistDetailByChecklistDefinitionId(int checklistDefinitionId);
        bool AddChecklistDetail(ChecklistDetailViewModel model);
        bool UpdateChecklistDetail(int ChecklistId, ChecklistDetailViewModel model);
        bool DeleteChecklistDetail(int ChecklistId, UserInfo user);
        bool AddMultipleChecklistDetails(List<ChecklistDetailViewModel> models, int staffId, short BranchId);
        #endregion

        #region CheckList Items
        IEnumerable<ChecklistItemViewModel> GetAllChecklistItem();
        List<ChecklistItemViewModel> GetAllChecklistItemById(int CheckListItemId);
        bool AddChecklistItem(ChecklistItemViewModel model);
        bool AddMultipleChecklistItem(List<ChecklistItemViewModel> model);
        bool UpdateChecklistItem(int CheckListItemId, ChecklistItemViewModel model);
        bool DeleteChecklistItem(int CheckListItemId, UserInfo user);

        #endregion

        #region CheckList Select Lists
        IEnumerable<CheckListStatusViewModel> GetAllChecklistStatus();
        IEnumerable<CheckListTargetTypeViewModel> GetAllChecklistTargetType();
        #endregion
        #region Checklist Validation
        
        bool ValidateChecklistDetailEntry(int checklistDefinitionId, int targetId);
        bool ValidateConditionPrecedentDetail(ConditionPrecedentViewModel entity);
        bool ValidateChecklistForDefferalOrWaival(int conditionId);
        #endregion

        // IEnumerable<ConditionPrecedentViewModel> GetConditionPrecedenceChecklist(int loanApplicationId);
        IEnumerable<ConditionPrecedentViewModel> GetConditionPrecedenceChecklist(int loanApplicationId, bool isAvailment);
        IEnumerable<ConditionPrecedentViewModel> GetConditionPrecedenceChecklistStatus(int loanApplicationId, bool isAvailment);
        bool UpdateLoanConditionPrecedenceStatus(ConditionPrecedentViewModel model);
        int GoForApproval(ApprovalViewModel entity);
        bool ExtendChecklistDeferralDate(ConditionPrecedentViewModel model);
        bool UpdateProvidedChecklist(ConditionPrecedentViewModel model);
        bool ValidateDeferralDateExpiration(int conditionId);
        IEnumerable<ChecklistApprovalViewModel> GetChecklistAwaitingApproval(int staffId, int companyId);
        IEnumerable<DeferredChecklistViewModel> GetAllDeferralChecklist();
        IEnumerable<DeferredChecklistViewModel> GetDeferralChecklistByConditionId(int conditionId);

        #region Checklist Type Mapping
        IEnumerable<CheckListTypeMappingViewModel> GetAllChecklistTypeMapping();
        bool AddChecklistTypeMapping(CheckListTypeMappingViewModel model);
        bool ValidateChecklistTypeMapping(short checklistTypeId, int approvallevelId);
        #endregion

        #region ESG Checklist
        IEnumerable<ESGClassViewModel> GetESGClass();
        IEnumerable<ESGTypeViewModel> GetESGType();
        IEnumerable<ESGCategoryViewModel> GetESGCategory();
        IEnumerable<ESGSubCategoryViewModel> GetESGSubCategory(int categoryId);
        IEnumerable<ESGChecklistDefinitionViewModel> GetESGChecklistDefinition();
        IEnumerable<ESGChecklistDetailViewModel> GetESGChecklistDetail(int loanApplicationDetailId);
        IEnumerable<ESGChecklistDefinitionAndDetailViewModel> GetESGChecklistStatus(int loanApplicationDetailId);
        IEnumerable<LoanApplicationDetailViewModel> GetAllFacilityDetails(int loanApplicationId, int companyId);
        bool AddESGChecklistDefinition(List<ESGChecklistDefinitionViewModel> models);
        bool AddESGChecklistDetail(List<ESGChecklistDetailViewModel> models);
        bool AddESGChecklistSummary(ESGChecklistSummaryViewModel models);
        #endregion
    }
}
