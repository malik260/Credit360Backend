using FintrakBanking.ViewModels;
using FintrakBanking.ViewModels.Setups.Credit;
using System;
using System.Collections.Generic;
using System.Text;

namespace FintrakBanking.Interfaces.Setups
{
    public interface IChecklistRepository
    {
        #region Loan Checklist Definition
        IEnumerable<ChecklistDefinitionViewModel> GetAllChecklistDefinition();
        IEnumerable<ChecklistDefinitionViewModel> GetAllMappedChecklistDefinitionByProductId(int productId);
        List<ChecklistDefinitionViewModel> GetAllChecklistDefinitionById(int CheckListDefinitionId);
        bool AddChecklistDefinition(ChecklistDefinitionViewModel model);
        bool AddMultipleChecklistDefinition(List<ChecklistDefinitionViewModel> models);
        bool AddMultipleChecklistDefinitionWithMultipleItems(ChecklistDefinitionViewModel model);
        bool UpdateChecklistDefinition(int CheckListDefinitionId, ChecklistDefinitionViewModel model);
        bool DeleteChecklistDefinition(int CheckListDefinitionId, UserInfo user);
        IEnumerable<ChecklistDefinitionViewModel> GetAllMappedChecklistDefinitionByApprovalLevelAndProduct(int approvalLevelId, int productId);
        IEnumerable<ChecklistItemViewModel> GetAllUnmappedChecklistItemsToApprovalLevelAndProduct(int approvalLevelId, int productId);
        IEnumerable<ChecklistDefinitionViewModel> GetUnmappedChecklistDefintionToApprovalLevel(int approvalLevelId);
        #endregion

        #region Loan Checklist Detail
        IEnumerable<ChecklistDetailViewModel> GetAllChecklistDetail();
        IEnumerable<ChecklistDetailViewModel> GetChecklistByTargetId(int targetId);
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
    }
}
