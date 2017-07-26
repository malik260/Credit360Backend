namespace FintrakBanking.ViewModels.Setups.General
{
    public class BranchLimitViewModel
    {
        public short branchLimitId { get; set; }
        public short? branchId { get; set; }
        public string limitType { get; set; }
        public decimal? limitAmount { get; set; }
        public double? limitPercentage { get; set; }
        public int? limitCount { get; set; }
    }
}