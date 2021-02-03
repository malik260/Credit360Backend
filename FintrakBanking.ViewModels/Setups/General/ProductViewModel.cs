using System;
using System.Collections.Generic;
using FintrakBanking.ViewModels.Setups.Finance;

namespace FintrakBanking.ViewModels.Setups.General
{
    public class ProductSearchViewModel 
    {

        public int productId { get; set; }
        public short productTypeId { get; set; }
        public string productTypeName { get; set; }
        public string productGroupName { get; set; }
        public short productCategoryId { get; set; }
        public string productCategoryName { get; set; }
        public short? productClassId { get; set; }
        public string productClassName { get; set; } 
        public string productCode { get; set; }
        public string productName { get; set; }
        public int productGroupId { get; set; }
        public short? dealClassificationId { get; set; }
        public string dealClassificationName { get; set; } 
        public int maximumTenor { get; set; }
        public int minimumTenor { get; set; }
        public decimal? maximumRate { get; set; }
        public decimal? minimumRate { get; set; }
        public double? equityContribution { get; set; }

        public ProductBehaviourViewModel ProductBehaviour { get; set; }
        public short productClassProcessId { get; set; }

    }
        public class ProductViewModel : GeneralEntity
    {
        public int? principalBalanceGl2 { get; set; }
        public string principalBalanceGl2Code { get; set; }
        public bool? requireCasaAccount { get; set; }
        public bool usedByLos { get; set; }
        public bool isFacilityLine { get; set; }
        public string operation { get; set; }
        

        public int productId { get; set; }
        public short productTypeId { get; set; }
        public string productTypeName { get; set; }
        public string productGroupName { get; set; }
        public short productCategoryId { get; set; }
        public string productCategoryName { get; set; }
        public short? productClassId { get; set; }
        public string productClassName { get; set; }
        public int productGroupId { get; set; }
        public string productCode { get; set; }
        public string productName { get; set; }
        public string productDescription { get; set; }
        public short currencyId { get; set; }
        public short customerId { get; set; }
        public int? principalBalanceGl { get; set; }
        public string principalBalanceGlCode { get; set; }
        public int? interestIncomeExpenseGl { get; set; }
        public string interestIncomeExpenseGlCode { get; set; }
        public int? interestReceivablePayableGl { get; set; }
        public int? penalChargeGl { get; set; }
        public string penalChargeGlCode { get; set; }

        public string interestReceivablePayableGlCode { get; set; }
        public short? productPriceIndexId { get; set; }
        public string productPriceIndexName { get; set; }
        public double? productPriceIndexSpread { get; set; }
        public short? riskRatingId { get; set; }
        public string riskRatingName { get; set; }

        public int? dormantGl { get; set; }
        public string dormantGlCode { get; set; }
        public int? premiumDiscountGl { get; set; }
        public string premiumDiscountGlCode { get; set; }
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
        public int operationId { get; set; }
        public string comment { get; set; }
        public bool allowScheduleTypeOverride { get; set; }
        public bool? allowMoratorium { get; set; }
        public bool? allowCustomerAccountForceDebit { get; set; }
        public int? defaultGracePeriod { get; set; }
        public int? cleanupPeriod { get; set; }
        public int? expiryPeriod { get; set; }
        public double? equityContribution { get; set; }

        public double? collateralLCYLimit { get; set; }
        public double? collateralFCYLimit { get; set; }
        public decimal? customerLimit { get; set; }
        public double? productLimit { get; set; }
        public double? invoiceLimit { get; set; }
        public bool? invoiceBased { get; set; }
        public bool? allowFundUsage { get; set; }
        public List<ProductCurrencyViewModel> currencies { get; set; }
        public int? crmsRegulatoryId { get; set; }
        public int? productBehaviourId { get; set; }

        public List<ProductFeeViewModel> fees { get; set; }

        public List<ProductCollateralTypeViewModel> collaterals { get; set; }
        public short? scheduleTypeId { get; set; }
        public short? dayCountConventionId { get; set; }
        //public short? productBehaviourId { get; set; }
        //public string productBehaviourName { get; set; }
        public ProductBehaviourViewModel productBehaviour { get;set;}
        public short productClassProcessId { get; set; }
        public short customerTypeId { get; set; }
        public decimal? penalChargeRate { get; set; }
        public bool excludeFromLitigation { get; set; }
        public bool isPaydayProduct { get; set; }
    }


    public class ProductLiteViewModel : GeneralEntity
    {
        public bool? requireCasaAccount;
        public int productId { get; set; }
        public short productTypeId { get; set; }
        public string productTypeName { get; set; }
        public string productGroupName { get; set; }
        public short productCategoryId { get; set; }
        public string productCategoryName { get; set; }
        public short? productClassId { get; set; }
        public string productClassName { get; set; }
        public int productGroupId { get; set; }
        public string productCode { get; set; }
        public string productName { get; set; }
        public short currencyId { get; set; }
        public short productClassProcessId { get; set; }
    }


    public class ProductCurrencyViewModel : GeneralEntity
    {
        public int productId { get; set; }
        public int productCurrencyId { get; set; }
        public short currencyId { get; set; }
        public string currencyName { get; set; }
    }

    public class ProductPriceIndexViewModel : GeneralEntity
    {
        public short productPriceIndexId { get; set; }
        public string priceIndexName { get; set; }
        public double priceIndexRate { get; set; }
        public string priceIndexDescription { get; set; }
        public int priceIndexDuration { get; set; }
        public bool allowAutomaticRepricing { get; set; }
        public int? currencyId { get; set; }

    }
    public class ProductPriceIndexGlobalViewModel : GeneralEntity
    {
        public short productPriceIndexGlobalId { get; set; }
        public short productPriceIndexId { get; set; }
        public string productPriceIndexName { get; set; }
        public string comment { get; set; }
        public double oldRate { get; set; }
        public double newRate { get; set; }
        public short approvalStatusId { get; set; }
        public bool hasBeenApplied { get; set; }
        public DateTime effectiveDate { get; set; }
        public bool isMarketInduced { get; set; }
        public int operationId { get; set; }
        public int? currentApprovalLevelId { get; set; }
    }
    public class ProductPriceIndexDailyViewModel : GeneralEntity
    {
        public string priceIndexName { get; set; }
        public short productPriceIndexId { get; set; }
        public double priceIndexRate { get; set; }
        public DateTime date { get; set; }
    }
    public class ProductPriceIndexCurrencyViewModel : GeneralEntity
    {
        public short priceIndexCurrencyId { get; set; }
        public short productPriceIndexId { get; set; }
        public short currencyId { get; set; }
    }
    public class ProductBehaviourViewModel
    {
        public bool? allowFundaUsage;
        public bool? isTemporaryOverDraft;
        public double? lcyLimit { get; set; }
        public double? fcyLimit { get; set; }
        public decimal? customerLimit { get; set; }
        public double? productLimit { get; set; }
        public double? invoiceLimit { get; set; }
        public bool? isInvoiceBased { get; set; }

        public short tempProductBehaviourId { get; set; }
        public string productCode { get; set; }
        public double? collateralLcyLimit { get; set; }
        public double? collateralFcyLimit { get; set; }
        public bool? requireCasaAccount { get; set; }
        public bool allowFundUsage { get; set; }
        //public bool isCurrent { get; set; }
        //public short approvalStatusId { get; set; }
        public int? crmsRegulatoryId { get; set; }

    }

    public class ProductClassProcessViewModel
    {
        private bool _useAmountLimit = false;

        public short productClassProcessId { get; set; }
        public string productClassProcessName { get; set; }
        public decimal? maximumAmount { get; set; }
        public bool useAmountLimit { get { return _useAmountLimit; }
            set
            {
                if (value == _useAmountLimit) return;
                _useAmountLimit = value;
            }
        }
    }

    public class ProductClassificationViewModel: GeneralEntity
    {
        public string profileBusinessUnitName { get; set; }

        public short productClassId { get; set; }
        public string productClassName { get; set; }
        public short productClassTypeId { get; set; }
        public string productClassType { get; set; }
        public short productClassProcessId { get; set; }
        public string productClassProcess { get; set; }
        public short customerTypeId { get; set; }
        public string customerType { get; set; }
        public int globalSla { get; set; }
        public int? businessUnitId { get; set; }
    }

    public class ProductDocumentMappingViewModel : GeneralEntity
    {
        public int productDocMapId { get; set; }
        //public int documentId { get; set; }
        public int documentTypeId { get; set; }
        public string documentCategoryName { get; set; }
        public string productName { get; set; }
        public string productClassName { get; set; }
        public string documentType { get; set; }
        public int documentCategoryId { get; set; }

        public int? productId { get; set; }
        public int? productClassId { get; set; }
        public int? operationId { get; set; }
        public bool mapToProductClass { get; set; }
        public bool mapToProduct { get; set; }
        public bool mapToOperation { get; set; }
        public bool required { get; set; }
        public int? sectorId { get; set; }
        public int? subSectorId { get; set; }
        public bool? mapToSector { get; set; }
        public bool? mapToSubSector { get; set; }
        public string sectorName { get; set; }
        public string subSectorName { get; set; }
                                           //public string documentTitle { get; set; }

    }

    public class DocumentDefinitionViewModel : GeneralEntity
    {
        public int documentDefinitionId { get; set; }

        public string documentTitle { get; set; }
        public bool inUse { get; set; }
    }
}