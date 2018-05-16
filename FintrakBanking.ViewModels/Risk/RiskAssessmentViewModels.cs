using System;
using System.Collections.Generic;

namespace FintrakBanking.ViewModels.Risk
{
    public class RiskAssessmentViewModels : GeneralEntity
    {
        public int riskId { get; set; }
        public Decimal IndexScore { get; set; }
        public int parentId { get; set; }
        public int riskAssessmentTitleId { get; set; }
        public int loanApplicationId { get; set; }
        public string refCode { get; set; }
        public bool selected { get; set; }
        public bool submited { get; set; }
        public int productId { get; set; }
    }

    public class AssessmentFormViewModel
    {
        public int id { get; set; }
        public int riskId { get; set; }
        public string name { get; set; }
        public string titleName { get; set; }
        public string description { get; set; }
        public Decimal weight { get; set; }
        public int? parentId { get; set; }
        public int? level { get; set; }
        public short? indexTypeId { get; set; }
        public int titleId { get; set; }
        public string riskAssessmentTitle { get; set; }
        public decimal score { get; set; }
        public bool selected { get; set; }
        public int assessmentId { get; set; }
    }

    public class AssessmentFormSaveViewModel : GeneralEntity
    {
        public string command { get; set; }
        public int titleIndex { get; set; } //?
        public int riskAssessmentTitleId { get; set; }
        public int? targetId { get; set; }
        public int titleId { get; set; }//?
        public List<AssessmentFormViewModel> indexFields { get; set; }
    }

    public class AssessmentResultViewModel : GeneralEntity
    {
        public int assessmentResultId { get; set; }
        public int? targetId { get; set; }
        public int riskAssessmentTitleId { get; set; }
        public string refrenceNumber { get; set; }
        public string customerName { get; set; }
        public string assessmentTitle { get; set; }
        public string creditRating { get; set; }
        public decimal totalScore { get; set; }
    }
}
