namespace FintrakBanking.ViewModels.Credit
{
    public class AssessmentTemplatesViewModel : GenaralEntity
    {
        public int creditTemplateId { get; set; }
        public string templateTitle { get; set; }
        public string creditTemplate { get; set; }
        public int approvalLevelId { get; set; }
        public short  productClassId { get; set; }
    }
   
}
