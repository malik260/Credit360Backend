namespace FintrakBanking.ViewModels.Setups.General
{
    public class ProductFeeViewModel : GeneralEntity
    {
        public short dealTypeId { get; set; }

        public string valueBase { get; set; }
        public short approvalStatusId { get; set; }
        public string chargeFeeName { get; set; }
        public decimal feeAmount { get; set; }
        public bool isIntegralFee { get; set; }
        public bool? isRecurring { get; set; }
        public bool? isRequired { get; set; }
        public decimal feeRateValue { get; set; }

        public string approvalStatusName { get; set; }

        public int loanChargeFeeId { get; set; }
        public int loanApplicationDetailId { get; set; }
        public int chargeFeeId { get; set; }
        public bool hasConsession { get; set; }
        public string consessionReason { get; set; }
        public decimal defaultfeeRateValue { get; set; }
        public decimal recommededFeeRateValue { get; set; }

        public int productFeeId { get; set; }
        public short productId { get; set; }
        public string productName { get; set; }
        public int feeId { get; set; }
        public string feeName { get; set; }
        public string feeTargetName { get; set; }
        public string feeIntervalName { get; set; }
        public string feeTypeName { get; set; }
        public string glAccountCode { get; set; }
        public string glAccountName { get; set; }

        public decimal rateValue { get; set; }
        public decimal? dependentAmount { get; set; }
        public short feeTypeId { get; set; }
        public double SN { get; set; }
        public string feeDisplay { get; set; }
        public decimal customerShare { get; set; }
        public decimal bankShare { get; set; }
        public bool isDutiable { get; set; }

        //public int companyId { get; set; }
        //public int createdBy { get; set; }
        //public DateTime dateTimeCreated { get; set; }
        //public int? lastUpdatedBy { get; set; }
        //public DateTime? dateTimeUpdated { get; set; }
        //public bool? deleted { get; set; }
        //public int? deletedBy { get; set; }
        //public DateTime? dateTimeDeleted { get; set; }
    }

    public class SignatoryViewModel  : GeneralEntity
    {
        public int rmStaffId;
        public int bmStaffId;
        public string staffName { get; set; }
        public string rmStaffName { get; set; }
        public string bmStaffName { get; set; }
        
    }
} 