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
        public int targetId2 { get; set; }
        public short? checkListStatusId { get; set; }
        public DateTime? deferedDate { get; set; }
        public string checkListStatusName { get; set; }
        public bool? checkListValidationStatus1 { get; set; }
        public bool? checkListValidationStatus2 { get; set; }
        public string targetTypeName { get; set; }
        public string targetName { get; set; }
        public string checkListDefinitionItemName { get; set; }
        public short checkListTypeId { get; set; }
        public int checkListItemId { get; set; }
        public int? customerId { get; set; }
        public DateTime checklistDate { get; set; }
    }

    public class ChecklistDefinitionViewModel : GeneralEntity
    {

        public int checkListDefinitionId { get; set; }
        public short? productId { get; set; }
        public int? approvalLevelId { get; set; }
        public int checkListItemId { get; set; }
        public int checkListTypeId { get; set; }
        public string checkListTypeName { get; set; }
        public string itemDescription { get; set; }
        public int operationId { get; set; }
        public bool isRequired { get; set; }
        public bool isActive { get; set; }
        public String productName { get; set; }
        public String approvalLevelName { get; set; }
        public String checkListItemName { get; set; }
        public int responseTypeId { get; set; }
        public bool requireUpload { get; set; }
        public List<MultipleChecklistItemsViewModel> checklistItems { get; set; }
    }

    public class ValidateChecklistDetailViewModel
    {
        public long checklistId { get; set; }
        public int checkListDefinitionId { get; set; }
        public bool checkListStatusId2 { get; set; }
        public bool checkListStatusId3 { get; set; }
        public bool isCAMchecklist { get; set; }
        public bool isAvailmentChecklist { get; set; }
    }

    public class CheckListScores : GeneralEntity
    {
        public int checkListStatusId { get; set; }
        public int checklistScoresId { get; set; }
        public int? esgChecklistDefinitionId { get; set; }
        public string grade { get; set; }
        public int gradeScore { get; set; }
        public int checkListTypeId { get; set; }
        public string scoreWeight { get; set; }
        public string colourCode { get; set; }
        public string checklistStatusName { get; set; }
    }

    public class ChecklistItemViewModel : GeneralEntity
    {
        public int checkListItemId { get; set; }
        public int checkListTypeId { get; set; }
        public string checkListItemName { get; set; }
        public short responseTypeId { get; set; }
        public string responseTypeName { get; set; }
        public bool requireUpload { get; set; }
    }

    public class CheckListStatusViewModel
    {
        public short responseTypeId { get; set; }
        public int checkListScoresId { get; set; }
        public short checklistStatusId { get; set; }
        public string checklistStatusName { get; set; }
        public string grade { get; set; }
    }
    public class CheckListResponseTypeViewModel
    {
        public short responseId { get; set; }
        public string responseName { get; set; }
    }
    public class CheckListTargetTypeViewModel
    {
        public short targetTypeId { get; set; }
        public string targetTypeName { get; set; }
        public bool isproductbased { get; set; }
        public bool canDoChecklist { get; set; }
        public bool canValidateChecklist { get; set; }
        public int loanApplicationId { get; set; }
        public int staffId { get; set; }
        public int operationId { get; set; }
        public int productClassProcessId { get; set; }
    }
    public class CheckListTypeMappingViewModel : GeneralEntity
    {
        public int checklistTypeMappingId { get; set; }
        public short checklistTypeId { get; set; }
        public int approvalLevelId { get; set; }
        public bool validateChecklist { get; set; }
        public string checkListTypeName { get; set; }
        public string approvalLevel { get; set; }
    }
    public class MultipleChecklistItemsViewModel : ChecklistDefinitionViewModel
    {

    }
    public class ChecklistDefinitionAndDetailViewModel
    {
        public int? customerId;

        public ChecklistDefinitionAndDetailViewModel()
        {
            responseTypes = new List<CheckListStatusViewModel>();
        }
        public int checkListDefinitionId { get; set; }
        public long checkListDetailId { get; set; }
        public int responseTypeId { get; set; }
        public bool requireUpload { get; set; }
        public int checkListTypeId { get; set; }
        public string checkListTypeName { get; set; }
        public int checkListItemId { get; set; }
        public string checkListItemName { get; set; }
        public string itemDescription { get; set; }
        public int ? checklistStatusId { get; set; }
        public short? productId { get; set; }
        public int? approvalLevelId { get; set; }

        public List<CheckListStatusViewModel> responseTypes { get; set; }
        public DateTime checklistDate { get; set; }
    }
}
