using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FintrakBanking.ViewModels.Setups.Credit
{
    public class ESGChecklistDetailViewModel : GeneralEntity
    {
        public int esgChecklistDetailId { get; set; }
        public int esgChecklistDefinitionId { get; set; }
        public int loanApplicationDetailId { get; set; }
        public short esgClassId { get; set; }
        public short esgTypeId { get; set; }
        public short checkStatusId { get; set; }
        public string description { get; set; }
        public string comment { get; set; }
        public int overAllRiskStatusId { get; set; }
        public string overSummary { get; set; }
        public string categoryName { get; set; }
        public string subCategoryName { get; set; }
        public string esgCheckListItemName { get; set; }
        public string sectorName { get; set; }
        public short sectorId { get; set; }
    }
    public class ESGChecklistDefinitionViewModel : GeneralEntity
    {
        public int esgChecklistDefinitionId { get; set; }
        public int checklistItemId { get; set; }
        public string checklistItemName { get; set; }
        public int? esgCategoryId { get; set; }
        public string esgCategoryName { get; set; }
        public int? esgSubCategoryId { get; set; }
        public string esgSubCategoryName { get; set; }
        public bool isCompulsory { get; set; }
        public string itemDescription { get; set; }
        public int yesGradeScore { get; set; }
        public int noGradeScore { get; set; }
        public int yesChecklistScoresId { get; set; }
        public int noChecklistScoresId { get; set; }
        public string yesGrade { get; set; }
        public string noGrade { get; set; }
        public string sectorName { get; set; }
        public string scoreWeight { get; set; }
        public string scoreColourCode { get; set; }
        public int? sectorId { get; set; }
    }

    public class ESGChecklistSummaryViewModel : GeneralEntity
    {
        public int esgChecklistSummaryId { get; set; }

        public int loanApplicationDetailId { get; set; }
        public int loanApplicationId { get; set; }

        public string comment { get; set; }
        public string colourCode { get; set; }

        public int ratingId { get; set; }
        public string productCustomerName { get; set; }
        public string grade { get; set; }
        public int score { get; set; }

        public string customerId { get; set; }
    }
    public class ESGSubCategoryViewModel
    {
        public int esgSubCategoryId { get; set; }
        public int esgCategoryId { get; set; }
        public string esgSubCategoryName { get; set; }
    }
    public class ESGCategoryViewModel
    {
        public int esgCategoryId { get; set; }
        public string esgCategoryName { get; set; }
    }
    public class ESGTypeViewModel
    {
        public int esgTypeId { get; set; }
        public string esgTypeName { get; set; }
    }
    public class ESGClassViewModel
    {
        public int esgClassId { get; set; }
        public string esgClassName { get; set; }
    }
    public class ESGChecklistDefinitionAndDetailViewModel
    {
        public ESGChecklistDefinitionAndDetailViewModel()
        {
            responseTypes = new List<CheckListStatusViewModel>();
        }
        public int checkListDefinitionId { get; set; }
        public long checkListDetailId { get; set; }
        public int responseTypeId { get; set; }
        public bool requireComment { get; set; }
        public int checkListItemId { get; set; }
        public string checkListItemName { get; set; }
        public string comment { get; set; }
        public int checklistStatusId { get; set; }
        public string categoryName { get; set; }
        public string subCategoryName { get; set; }
        public int loanApplicationDetailId { get; set; }
        public short? esgClassId { get; set; }
        public short? esgTypeId { get; set; }
        public string sectorName { get; set; }
        public string checklistStatusName { get; set; }
        public string grade { get; set; }
        public int? sectorId { get; set; }
        public int yesChecklistScoresId { get; set; }
        public int noChecklistScoresId { get; set; }

        public List<CheckListStatusViewModel> responseTypes { get; set; }
        public CheckListStatusViewModel yesResponseTypes { get; set; }
        public CheckListStatusViewModel noResponseTypes { get; set; }
    }

    public class GreenRatingDefinitionAndDetailViewModel
    {
        public GreenRatingDefinitionAndDetailViewModel()
        {
            responseTypes = new List<CheckListStatusViewModel>();
        }
        public int checkListDefinitionId { get; set; }
        public long checkListDetailId { get; set; }
        public int responseTypeId { get; set; }
        public bool requireComment { get; set; }
        public int checkListItemId { get; set; }
        public string checkListItemName { get; set; }
        public string comment { get; set; }
        public int checklistStatusId { get; set; }
        public string sectorName { get; set; }
        public int loanApplicationDetailId { get; set; }
        public short sectorId { get; set; }
        public int yesChecklistScoresId { get; set; }
        public int noChecklistScoresId { get; set; }

        public List<CheckListStatusViewModel> responseTypes { get; set; }
    }
}
