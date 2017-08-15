namespace FintrakBanking.ViewModels.Setups
{
    public class RiskAssessmentIndexViewModels : GeneralEntity
    {
        public int riskId { get; set; }
        public string name { get; set; }
        public string description { get; set; }
        public decimal weight { get; set; }
        public int? parentId { get; set; }
        public int? itemLevel { get; set; }
        public short indexTypeId { get; set; }
        public int riskAssessmentTitleId { get; set; }
        public string riskAssessmentTitle { get; set; }
        public bool selected { get; set; }
    }

    public class RiskAssessmentTitleViewModels : GeneralEntity
    {
        public int riskAssessmentTitleId { get; set; }
        public string riskTitle { get; set; }
        public int? productId { get; set; }
        public int riskTypeId { get; set; }
    }
}