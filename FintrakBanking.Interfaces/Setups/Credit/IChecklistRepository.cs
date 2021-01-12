using FintrakBanking.Interfaces.WorkFlow;
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
        bool DeleteChecklistTypeMapping(int checklistTypeMappingId, UserInfo user);
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
        IEnumerable<ChecklistDefinitionAndDetailViewModel> GetChecklistDefinitionByApprovalLevelCheckListType(int staffId, int? productId, int loanTargetId, int operationId, int checkListTypeId , int? customerId=null);
        #endregion

        #region Loan Checklist Detail
        IEnumerable<ChecklistDetailViewModel> GetAllChecklistDetail();
        IEnumerable<ChecklistDetailViewModel> GetChecklistByTargetId(int targetId);
        IEnumerable<ChecklistDetailViewModel> GetChecklistByCheckListTypeAndTargetId(int targetId, int checkListtypeId, bool isCamChecklist,int? customerId=null);
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
        IEnumerable<ChecklistItemViewModel> GetAllChecklistItemBycheckListTypeId(int checkListTypeId);
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
        bool ValidateChecklistForDefferalOrWaival(ConditionPrecedentViewModel entity);
        #endregion

        // IEnumerable<ConditionPrecedentViewModel> GetConditionPrecedenceChecklist(int loanApplicationId);
        IEnumerable<ConditionPrecedentViewModel> GetConditionPrecedenceChecklist(int loanApplicationId, bool isAvailment);
        IEnumerable<ConditionPrecedentViewModel> GetConditionPrecedenceChecklistStatus(int loanApplicationId, bool isAvailment, int staffId = 0);
        bool UpdateLoanConditionPrecedenceStatus(ConditionPrecedentViewModel model);
        bool ForwardChecklistForApproval(List<ConditionPrecedentViewModel> model);
        WorkflowResponse GoForApproval(ApprovalViewModel entity);
        bool ExtendChecklistDeferralDate(ConditionPrecedentViewModel model);
        bool UpdateProvidedChecklist(ConditionPrecedentViewModel model);
        bool ValidateDeferralDateExpiration(int conditionId);
        IEnumerable<ChecklistApprovalViewModel> GetDeferralDocumentsAwaitingApproval(int staffId, int companyId);
        IEnumerable<ChecklistApprovalViewModel> GetDeferralExtensionsAwaitingApproval(int staffId, int companyId);

        String ResponseMessage(WorkflowResponse response, string itemHeading);
        WorkflowResponse SubmitDeferralDocumentForApproval(ConditionPrecedentViewModel model);
        bool SubmitDeferralExtensionForApproval(ConditionPrecedentViewModel model);


        IEnumerable<ChecklistApprovalViewModel> GetChecklistAwaitingApproval(int staffId, int companyId);
        IEnumerable<DeferredChecklistViewModel> GetAllDeferralChecklist();
        IEnumerable<DeferredChecklistViewModel> GetDeferralChecklistByConditionId(int conditionId);
        bool ValidateChecklist(int applicationId);

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

        IEnumerable<CheckListScores> GetCheckListScores(int checkListTypeId);
        IEnumerable<ESGChecklistDefinitionViewModel> GetGreenRatingDefinition();
        IEnumerable<ESGChecklistDetailViewModel> GetGreenRatingDetail(int loanApplicationDetailId);
        IEnumerable<ESGChecklistDefinitionAndDetailViewModel> GetGreenRatingStatus(int loanApplicationDetailId);
        ESGChecklistSummaryViewModel CalculateGreenRatingSummary(List<ESGChecklistDetailViewModel> models);
        bool AddGreenRatingDetail(List<ESGChecklistDetailViewModel> models);
        bool AddGreenRatingSummary(ESGChecklistSummaryViewModel models);
        bool AddGreenRatingDefinition(List<ESGChecklistDefinitionViewModel> models);
        bool DeleteGreenRatingDefinition(int esgChecklistDefinitionId, int staffId);


        ESGChecklistSummaryViewModel CalculateESGChecklistSummary(List<ESGChecklistDetailViewModel> models);
        IEnumerable<LoanApplicationDetailViewModel> GetAllFacilityDetails(int loanApplicationId, int companyId);
        bool AddESGCategory(ESGChecklistDefinitionViewModel model);
        bool AddESGSubCategory(ESGChecklistDefinitionViewModel model);
        bool UpdateESGCategory(ESGChecklistDefinitionViewModel model);
        bool UpdateESGSubCategory(ESGChecklistDefinitionViewModel model);
        bool DeleteESGCategory(int ESGCategoryId, UserInfo user);
        bool DeleteESGSubcategory(int ESGSubcategoryId, UserInfo user);
        bool AddESGChecklistDefinition(List<ESGChecklistDefinitionViewModel> models);
        bool AddESGChecklistDetail(List<ESGChecklistDetailViewModel> models);
        bool AddESGChecklistSummary(ESGChecklistSummaryViewModel models);

        bool DeleteESGChecklistDefinition(int esgChecklistDefinitionId, int staffId);

        #endregion

        bool RegulatoryChecklistAutomapping(int customerId, ChecklistDetailViewModel model);
        bool DeleteLoanConditionPrecedenceStatus(int conditionId, bool isLMSChecklist, UserInfo user);
        bool ValidatePrecedenceChecklistCompleted(int loanApplicationId);
        bool LMSValidatePrecedenceChecklistCompleted(int applicationId);

        #region Condition Precedence Checklist
        IEnumerable<ConditionPrecedentViewModel> GetLMSConditionPrecedenceChecklist(int loanReviewApplicationId);
        IEnumerable<ConditionPrecedentViewModel> GetLMSConditionPrecedenceChecklistStatus(int loanReviewApplicationId);
        #endregion
        IEnumerable<ChecklistDefinitionAndDetailViewModel> GetChecklistItemSimulationDetails(int productId);
        bool PopulateLoanApplicationChecklist(int loanApplicationId, int staffId, int companyId, int productClassProcessId);

        IEnumerable<ApprovalTrailViewModel> GetDeferralApprovalTrail(int targetId, int operationId);
    }
}
