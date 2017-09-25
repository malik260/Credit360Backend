namespace FintrakBanking.ReportObjects.ViewModels
{
    public class SectorLimitViewModel
    {
        public string companyName { get; set; }
        public short sectorId { get; set; }
        public string sectorName { get; set; }
        public string sectorCode { get; set; }
        public decimal sectorLimit { get; set; }
        public decimal sectorUsage { get; set; }
        public decimal sectorBalance { get; set; }
        public bool allowOverride { get; set; }
    }
}
