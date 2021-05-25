using System;

namespace FintrakBanking.ViewModels.Setups.General
{
    public class BranchViewModel : GeneralEntity
    {
        public short branchId { get; set; }
        public int? stateId { get; set; }
        public int? cityId { get; set; }
        public int regionId { get; set; }
        public string regionName { get; set; }
        public string branchName { get; set; }
        public string stateName { get; set; }
        public string cityName { get; set; }
        public string branchCode { get; set; }
        public string addressLine1 { get; set; }
        public string addressLine2 { get; set; }
        public string comment { get; set; }
        public decimal branchLimit { get; set; }
        public bool allowOverride { get; set; }

    }

    public class AddBranchViewModel : GeneralEntity
    {
        public int? stateId { get; set; }
        public int cityId { get; set; }
        public int regionId { get; set; }
        public string branchName { get; set; }
        public string branchCode { get; set; }
        public string addressLine1 { get; set; }
        public string addressLine2 { get; set; }
        public string comment { get; set; }
    }
    public class BranchRegionViewModel : GeneralEntity
    {
        public int regionId { get; set; }
        public string regionName { get; set; }
        public int? houStaffId { get; set; }
        public string houStaffName { get; set; }
        public int? regionTypeId { get; set; }
        public string regionTypeName { get; set; }
    }
    public class BranchRegionStaffViewModel : GeneralEntity
    {
        public int staffRegionId { get; set; }
        public int regionId { get; set; }
        public int houStaffId { get; set; }
        public int regionStaffTypeId { get; set; }
        public string regionStaffTypeName { get; set; }
        public string houStaffName { get; set; }
    }

    public class CollectionsRetailCronSetupViewModel : GeneralEntity
    {
        public int cronJobId { get; set; }
        public DateTime startDate { get; set; }
        public string startTime { get; set; }
        public DateTime endDate { get; set; }
        public string endTime { get; set; }
        public int cronNature { get; set; }
    }

    public class CollectionsRetailComputationVariableSetupViewModel : GeneralEntity
    {
        public int computationVariableId { get; set; }
        public decimal vat { get; set; }
        public decimal wht { get; set; }
        public decimal commissionPayable { get; set; }
        public decimal commissionPayableLimit { get; set; }
        public int commissionRateExternal { get; set; }
        public decimal recoveredAmountBelow { get; set; }
        public decimal recoveredAmountAbove { get; set; }
        public int commissionRateExternal2 { get; set; }
        public decimal recoveredAmountExternalBelow { get; set; }
        public decimal recoveredAmountExternalAbove { get; set; }
    }

}