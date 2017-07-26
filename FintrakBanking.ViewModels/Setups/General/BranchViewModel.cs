using System;

namespace FintrakBanking.ViewModels.Setups.General
{
    public class BranchViewModel : GenaralEntity
    {
        public short branchId { get; set; }
        public int? stateId { get; set; }
        public int cityId { get; set; }
        public string branchName { get; set; }
        public string stateName { get; set; }
        public string cityName { get; set; }
        public string branchCode { get; set; }
        public string addressLine1 { get; set; }
        public string addressLine2 { get; set; }
        public string comment { get; set; }
    }

    public class AddBranchViewModel : GenaralEntity
    {
        public int? stateId { get; set; }
        public int cityId { get; set; }
        public string branchName { get; set; }
        public string branchCode { get; set; }
        public string addressLine1 { get; set; }
        public string addressLine2 { get; set; }
        public string comment { get; set; }
    }
}