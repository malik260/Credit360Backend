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


        public List<ChargeRangeViewModel> ranges { get; set; }
        public string accountCategoryName { get; set; }
        public string frequencyTypeName { get; set; }
        public string amortizationTypeName { get; set; }
        public string targetName { get; set; }
        public string feeTypeName { get; set; }
        public string ledgerAccountName { get; set; }
        public string ledgerAccountCode { get; set; }
        public string amortisationTypeName { get; set; }
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
}
