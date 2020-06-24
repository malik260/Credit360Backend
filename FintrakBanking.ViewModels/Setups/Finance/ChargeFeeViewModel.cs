using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FintrakBanking.ViewModels.Setups.Finance
{
    public class ChargeFeeViewModel : GeneralEntity
    {
        public ChargeFeeViewModel()
        {
            ranges = new List<ChargeRangeViewModel>();
            chargeFeeDetails = new List<ChargeFeeDetailsViewModel>();
        }
        public int chargeFeeId { get; set; }
        public string chargeName { get; set; }
        public short? productTypeId { get; set; }
        public int? operationId { get; set; }
        public decimal? amount { get; set; }
        public double? rate { get; set; }
        public short feeTypeId { get; set; }
        public short valueSource { get; set; }
        public int ledgerAccountId { get; set; }
        public bool recurring { get; set; }
        public short frequencyTypeId { get; set; }
        public int? primaryTaxId { get; set; }
        public int? secondaryTaxId { get; set; }
        public short accountCategoryId { get; set; }
        public short targetId { get; set; }
        public short? amortisationTypeId { get; set; }
        public bool includeCutOffDay { get; set; }
        public short? cutOffDay { get; set; }
        public bool isIntegral { get; set; }
        public int? crmsRegulatoryId { get; set; }

        public List<ChargeFeeDetailsViewModel> chargeFeeDetails { get; set; }
        public List<ChargeRangeViewModel> ranges { get; set; }
        public string accountCategoryName { get; set; }
        public string frequencyTypeName { get; set; }
        public string amortizationTypeName { get; set; }
        public string targetName { get; set; }
        public string feeTypeName { get; set; }
        public string ledgerAccountName { get; set; }
        public string ledgerAccountCode { get; set; }
        public string amortisationTypeName { get; set; }

        public int loanReviewOperationId { get; set; }
        public int? loanReviewApplicationId { get; set; }
        public int loanId { get; set; }
        public int loanSystemTypeId { get; set; }
        //public string requestStaffName { get; set; }
        //public string requestApprovalLevel { get; set; }
        //public string responseStaffName { get; set; }
        //public string responseApprovalLevel { get; set; }
        //public string approvalStatus { get; set; }
        //public string comment { get; set; }

        //public DateTime systemArrivalDate { get; set; }
        //public DateTime? systemResponseDate { get; set; }

        public int? loanChargeFeeId { get; set; }

    }

    public class ChargeRangeViewModel : GeneralEntity
    {
        public int chargeRangeId { get; set; }
        public decimal? minimum { get; set; }
        public decimal? maximum { get; set; }
        public bool? minimumAndAbove { get; set; }
        public bool? maximumAndBelow { get; set; }
        public double? rate { get; set; }
        public decimal? amount { get; set; }
        public int chargeFeeId { get; set; }
    }

    public class ChargeFeeDetailsViewModel
    {
        public int chargeFeeDetailId { get; set; }
        public string description { get; set; }
        public int chargeFeeId { get; set; }
        public int? glAccountId1 { get; set; }
        public int? glAccountId2 { get; set; }
        public short detailTypeId { get; set; }
        public short postingTypeId { get; set; }
        public double amount { get; set; }
        public double rate { get; set; }
        public short feeTypeId { get; set; }
        public short detailClassId { get; set; }
        public bool requireAmortization { get; set; }
        public short postingGroup { get; set; }
    }
}
