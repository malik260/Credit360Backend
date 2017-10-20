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
        IEnumerable<ChecklistDefinitionViewModel> GetAllChecklistDefinitionById(int CheckListDefinitionId);
        bool AddChecklistDefinition(ChecklistDefinitionViewModel model);
        bool AddMultipleChecklistDefinition(List<ChecklistDefinitionViewModel> models);
        bool AddMultipleChecklistDefinitionWithMultipleItems(ChecklistDefinitionViewModel model);
        bool UpdateChecklistDefinition(int CheckListDefinitionId, ChecklistDefinitionViewModel model);
        bool DeleteChecklistDefinition(int CheckListDefinitionId, UserInfo user);
        IEnumerable<ChecklistDefinitionViewModel> GetChecklistDefinitionByApprovalLevel(int approvalLevelId);
        #endregion

        #region Loan Checklist Detail
        IEnumerable<ChecklistDetailViewModel> GetAllChecklistDetail();
        List<ChecklistDetailViewModel> GetAllChecklistDetailById(int ChecklistId);
        bool AddChecklistDetail(ChecklistDetailViewModel model);
        bool UpdateChecklistDetail(int ChecklistId, ChecklistDetailViewModel model);
        bool DeleteChecklistDetail(int ChecklistId, UserInfo user);
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
