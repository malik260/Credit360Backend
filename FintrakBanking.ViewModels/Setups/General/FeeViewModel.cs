using System;

namespace FintrakBanking.ViewModels.Setups.General
{
    public class FeeViewModel : GeneralEntity
    {
        public int feeId { get; set; }
        public string feeName { get; set; }
        public short accountCategoryId { get; set; }
        public string accountCategoryName { get; set; }
        public short feeTypeId { get; set; }
        public string feeTypeName { get; set; }
        public bool feeTypeByAmountRequired { get; set; }
        public bool byAmountRequired { get; set; }
        public short feeIntervalId { get; set; }
        public string feeIntervalName { get; set; }
        public short productTypeId { get; set; }
        public string productTypeName { get; set; }
        public short feeTargetId { get; set; }
        public string feeTargetName { get; set; }
        public int glAccountId { get; set; }
        public string glAccountCode { get; set; }
        public string glAccountName { get; set; }
        public short? feeAmortisationTypeId { get; set; }
        public bool isIntegralFee { get; set; }

        public bool includeCutOffDay { get; set; }
        public short? cutOffDay { get; set; }
       
        public DateTime feeDate { get; set; }
    
    }
}