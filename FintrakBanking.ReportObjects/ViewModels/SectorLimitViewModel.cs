namespace FintrakBanking.ReportObjects.ViewModels
{
    public class SectorLimitViewModel
    {
        public string companyName { get; set; }
        public short Id { get; set; }
        public string Name { get; set; }
        public string Code { get; set; }
        public decimal Limit { get; set; }
        public decimal Usage { get; set; }
        public decimal Balance { get { return (Limit - Usage); } }
        public bool allowOverride { get; set; }
    }
}
