using System;

namespace FintrakBanking.ViewModels.Setups.General
{
    public class ProductViewModel : GenaralEntity
    {
        public int productId { get; set; }
        
        public short productTypeId { get; set; }
        public string productTypeName { get; set; }
        public string productGroupName { get; set; }
        public short productCategoryId { get; set; }
        public string productCategoryName { get; set; }
        public short productClassId { get; set; }
        public string productClassName { get; set; }
        public int productGroupId { get; set; }
        public string productCode { get; set; }
        public string productName { get; set; }
        public string productDescription { get; set; }
        public short currencyId { get; set; }

        public int ? principalBalanceGl { get; set; }
        public string principalBalanceGlCode { get; set; }
        public int? interestIncomeExpenseGl { get; set; }
        public string interestIncomeExpenseGlCode { get; set; }
        public int? interestReceivablePayableGl { get; set; }
        public string interestReceivablePayableGlCode { get; set; }
        public short? productPriceIndexId { get; set; }
        public string productPriceIndexName { get; set; }
        public double? productPriceIndexSpread { get; set; }

        public int? dormantGl { get; set; }
        public int? premiumDiscountGl { get; set; }
        public short? dealTypeId { get; set; }
        public string dealTypeName { get; set; }
        public short? dealClassificationId { get; set; }
        public string dealClassificationName { get; set; }
        public short? dayCountId { get; set; }
        public string dayCountName { get; set; }

        public int maximumTenor { get; set; }
        public int minimumTenor { get; set; }
        public decimal? maximumRate { get; set; }
        public decimal? minimumRate { get; set; }
        public decimal? minimumBalance { get; set; }

        public bool? allowOverdrawn { get; set; }
        public int? overdrawnGl { get; set; }
        public bool? allowRate { get; set; }
        public bool? allowTenor { get; set; }

        public int? approvedBy { get; set; }
        public bool? completed { get; set; }
        public bool? approved { get; set; }

        public int approvalStatusId { get; set; }
        public string productCurrencies { get; set; }
  
    }

    public class ProductPriceIndexViewModel : GenaralEntity
    {
        public short productPriceIndexId { get; set; }
        public string priceIndexName { get; set; }
        public double priceIndexRate { get; set; }
        public string priceIndexDescription { get; set; }
    }
}