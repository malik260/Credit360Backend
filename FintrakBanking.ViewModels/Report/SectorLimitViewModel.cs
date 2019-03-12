namespace FintrakBanking.ViewModels.Reports
{
    public class SectorLimitViewModel
    {
        public string companyName { get; set; }
        public string companyLogo { get; set; }
        public string sectorName { get; set; }
        public string subsectorName { get; set; }
        public string sectorcode { get; set; }
        public string subsectorCode { get; set; }
        public decimal? limitMaximumValue { get; set; }
        public decimal? usage { get; set; }
        public decimal balance { get { return (decimal)((limitMaximumValue.HasValue ? limitMaximumValue : 0) - (usage.HasValue ? usage : 0)); } }
        public decimal balances { get; set; }
        public bool allowOverride { get; set; }
    }
}
