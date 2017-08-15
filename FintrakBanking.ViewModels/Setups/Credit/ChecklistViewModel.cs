using System;
using System.Collections.Generic;
using System.Text;

namespace FintrakBanking.ViewModels.Setups.Credit
{
    public class ChecklistDetailViewModel : GeneralEntity
    {
        public long checklistId { get; set; }
        public int checkListDefinitionId { get; set; }
        public string remark { get; set; }
        public int checkedBy { get; set; }
        public short targetTypeId { get; set; }
        public int targetId { get; set; }
        public short checkListStatusId { get; set; }
        public DateTime? deferedDate { get; set; }
        public string checkListStatusName { get; set; }
        public string targetTypeName { get; set; }
        public string targetName { get; set; }
        public string checkListDefinitionItemName { get; set; }
    }

    public class ChecklistDefinitionViewModel : GeneralEntity
    {
 
        public int checkListDefinitionId { get; set; }
        public short? productClassId { get; set; }
        public int? approvalLevelId { get; set; }
        public int checkListItemId { get; set; }
        public string itemDescription { get; set; }
        public bool isRequired { get; set; }
        public bool isActive { get; set; }
        public String productClassName { get; set; }
        public String approvalLevelName { get; set; }
        public String checkListItemName { get; set; }
        public String companyName { get; set; } 
        public List<MultipleChecklistItemsViewModel> checklistItems { get; set; }
    }

    public class ChecklistItemViewModel : GeneralEntity
    {
        public int checkListItemId { get; set; }
        public string checkListItemName { get; set; }
    }

    public class CheckListStatusViewModel
    {
        public short checklistStatusId { get; set; }
        public string checklistStatusName { get; set; }
    }

    public class CheckListTargetTypeViewModel
    {
        public short targetTypeId { get; set; }
        public string targetTypeName { get; set; }
    }

    public class MultipleChecklistItemsViewModel: ChecklistDefinitionViewModel
    {
        
    }
}
