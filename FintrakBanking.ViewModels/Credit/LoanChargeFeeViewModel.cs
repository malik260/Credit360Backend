using FintrakBanking.ViewModels.Setups.Finance;

namespace FintrakBanking.ViewModels.Credit
{
    public class LoanChargeFeeViewModel : ChargeRangeViewModel

    {
        public int productFeeId { get; set; }
        public int loanChargeFeeId { get; set; }
        public int loanId { get; set; }
        public int productId { get; set; }
        public string chargeFeeName { get; set; }
        public decimal feeRateValue { get; set; }
        public decimal feeDependentAmount { get; set; }
        public decimal feeAmount { get; set; }
        public short feeIntervalId { get; set; }
        public string feeIntervalName { get; set; }
        public int feeTypeId { get; set; }
        public string feeTypeName { get; set; }
        public bool isIntegralFee { get; set; }

    }

}
