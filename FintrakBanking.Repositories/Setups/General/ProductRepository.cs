using FintrakBanking.Entities.Models;
using FintrakBanking.Interfaces.Admin;
using FintrakBanking.Interfaces.Setups.General;
using FintrakBanking.ViewModels.Setups.General;
using System;
using System.Collections.Generic;
using System.Linq;
using FintrakBanking.ViewModels;
using FintrakBanking.Common.Enum;
using System.ComponentModel.Composition;
using FintrakBanking.Interfaces.WorkFlow;
using FintrakBanking.Interfaces.Setups.Approval;
using FintrakBanking.ViewModels.WorkFlow;
using FintrakBanking.ViewModels.Credit;
using System.Threading.Tasks;

namespace FintrakBanking.Repositories.Setups.General
{
    [Export(typeof(IProductRepository))]
    [PartCreationPolicy(CreationPolicy.NonShared)]
    public class ProductRepository : IProductRepository
    {
        private FinTrakBankingContext context;
        private IGeneralSetupRepository genSetup;
        private IAuditTrailRepository auditTrail;
        private IWorkFlowRepository workFlow;
        private IApprovalLevelStaffRepository level;
        private IProductFeeRepository productFee;
        private IProductCollateralTypeRepository productCollateralType;

        public ProductRepository(FinTrakBankingContext _context,
                                IGeneralSetupRepository _genSetup,
                                IAuditTrailRepository _auditTrail,
                                IWorkFlowRepository _workFlow,
                                IApprovalLevelStaffRepository _level,
                                IProductFeeRepository _productFee,
                                IProductCollateralTypeRepository _productCollateralType)
        {
            this.context = _context;
            this.genSetup = _genSetup;
            this.auditTrail = _auditTrail;
            this.workFlow = _workFlow;
            level = _level;
            productFee = _productFee;
            productCollateralType = _productCollateralType;
        }

        private bool SaveAll()
        {
            return this.context.SaveChanges() > 0;
        }

        private string GenerateProductCode(int companyId)
        {
            var data = this.context.tbl_Product.Count(x => x.CompanyId == companyId);
            int counter = data + 1;
            var productCode = string.Empty;
            do
            {
                productCode = string.Format("{0}", counter.ToString().PadLeft(4, '0'));
                counter++;
            }
            while (context.tbl_Temp_Product.Any(x => x.ProductCode == productCode) == true);

            return productCode;
        }

        public IEnumerable<ProductCategoryViewModel> GetAllProductCategory()
        {
            return this.context.tbl_Product_Category.Select(p => new ProductCategoryViewModel()
            {
                productCategoryId = p.ProductCategoryId,
                productCategoryName = p.ProductCategoryName
            });
        }

        public IEnumerable<LookupViewModel> GetAllProductClass()
        {
            return (from data in context.tbl_Product_Class
                        //where data.OperationTypeId == operationTypeId
                    select new LookupViewModel()
                    {
                        lookupId = (short)data.ProductClassId,
                        lookupName = data.ProductClassName,
                        lookupTypeId = data.ProductClassTypeId,
                        lookupTypeName = data.tbl_Product_Class_Type.ProductClassTypeName
                    });
        }

        public IEnumerable<ProductGroupViewModel> GetAllProductGroup()
        {
            return (from p in context.tbl_Product_Group
                    orderby p.ProductGroupName ascending //, p.ProductGroupCode ascending
                    select new ProductGroupViewModel()
                    {
                        productGroupId = p.ProductGroupId,
                        productGroupCode = p.ProductGroupCode,
                        productGroupName = p.ProductGroupName
                    });
        }

        public ProductGroupViewModel GetProductGroupById(short productGroupId)
        {
            var data = this.context.tbl_Product_Group.FirstOrDefault(x => x.ProductGroupId == productGroupId); // .Find(accountId);

            if (data == null)
                return null;

            return new ProductGroupViewModel
            {
                productGroupId = data.ProductGroupId,
                productGroupCode = data.ProductGroupCode,
                productGroupName = data.ProductGroupName
            };
        }

        public bool UpdateProductGroup(int productGroupId, ProductGroupViewModel productGroup)
        {
            var data = this.context.tbl_Product_Group.FirstOrDefault(x => x.ProductGroupId == productGroupId);

            if (data == null)
                return false;

            data.ProductGroupName = productGroup.productGroupName;
            // Audit Section ---------------------------
            var audit = new tbl_Audit
            {
                AuditTypeId = (short)AuditTypeEnum.ProductGroupAdded,
                StaffId = productGroup.createdBy,
                BranchId = (short)productGroup.userBranchId,
                Detail = $"Updated tbl_Product Group: '{productGroup.productGroupName}' with code: '{productGroup.productGroupCode}' ",
                IPAddress = productGroup.userIPAddress,
                Url = productGroup.applicationUrl,
                ApplicationDate = genSetup.GetApplicationDate(),
                SystemDateTime = DateTime.Now
            };
            this.auditTrail.AddAuditTrail(audit);
            //end of Audit section ------------------------------- 
            return this.SaveAll();
        }

        public IQueryable<ProductTypeViewModel> AllProductType()
        {
            return (from p in context.tbl_Product_Type
                    select new ProductTypeViewModel()
                    {
                        productTypeId = p.ProductTypeId,
                        productTypeName = p.ProductTypeName,
                        productGroupId = p.ProductGroupId,
                        productGroupName = p.tbl_Product_Group.ProductGroupName,
                        requirePrincipalGl = p.RequirePrincipalGL,
                        requireInterestIncomeExpenseGl = p.RequireInterestIncomeExpenseGL,
                        requireInterestReceivablePayableGl = p.RequireInterestReceivablePayableGL,
                        requirePremiumDiscountGl = p.RequirePremiumDiscountGL,
                        requireDormantGl = p.RequireDormantGL,
                        requireOverdrawnGL = p.RequireOverdrawnGL,
                        requireRate = p.RequireRate,
                        requireTenor = p.RequireTenor,
                        dealClassificationId = p.DealClassificationId,
                        requireScheduleType = p.RequireScheduleType
                    });
        }

        public IEnumerable<ProductTypeViewModel> GetAllProductType()
        {
            return AllProductType();
        }

        public ProductTypeViewModel GetProductTypeById(short productTypeId)
        {
            //var data = this.context.TblProductType.FirstOrDefault(x => x.ProductTypeId == productTypeId); // .Find(accountId);

            //if (data == null)
            //    return null;

            //return new ProductTypeViewModel
            //{
            //    productTypeId = data.ProductTypeId,
            //    productTypeName = data.ProductTypeName,
            //    productGroupId = data.ProductGroupId,
            //    productGroupName =context.TblProductGroup.Single(x=>x.ProductGroupId==data.ProductGroupId).ProductGroupName
            //};


            return AllProductType().Where(p => p.productTypeId == productTypeId).FirstOrDefault();
        }

        public IEnumerable<ProductTypeViewModel> GetProductTypeByProductGroup(short productGroupId)
        {
            return AllProductType().Where(p => p.productGroupId == productGroupId);
        }

        public short AddProductType(ProductTypeViewModel productType)
        {
            var data = new tbl_Product_Type()
            {
                ProductTypeName = productType.productTypeName,
                ProductGroupId = productType.productGroupId,
                RequirePrincipalGL = productType.requirePrincipalGl,
                RequireInterestIncomeExpenseGL = productType.requireInterestIncomeExpenseGl,
                RequireInterestReceivablePayableGL = productType.requireInterestReceivablePayableGl,
                RequirePremiumDiscountGL = productType.requirePremiumDiscountGl,
                RequireDormantGL = productType.requireDormantGl,
                RequireOverdrawnGL = productType.requireOverdrawnGL,
                RequireRate = productType.requireRate,
                RequireTenor = productType.requireTenor,
                DealClassificationId = productType.dealClassificationId,
                RequireScheduleType = productType.requireScheduleType
            };

            this.context.tbl_Product_Type.Add(data);

            // Audit Section ---------------------------
            var audit = new tbl_Audit
            {
                AuditTypeId = (short)AuditTypeEnum.ProductTypeAdded,
                StaffId = productType.createdBy,
                BranchId = (short)productType.userBranchId,
                Detail = $"Added tbl_Product Type: '{productType.productTypeName}' ",
                IPAddress = productType.userIPAddress,
                Url = productType.applicationUrl,
                ApplicationDate = genSetup.GetApplicationDate(),
                SystemDateTime = DateTime.Now
            };

            this.auditTrail.AddAuditTrail(audit);
            //end of Audit section -------------------------------  

            var status = this.SaveAll();

            if (status)
                return data.ProductTypeId;
            else
                return -1;
        }

        public bool UpdateProductType(int productTypeId, ProductTypeViewModel productType)
        {
            var data = this.context.tbl_Product_Type.FirstOrDefault(x => x.ProductTypeId == productTypeId);

            if (data == null)
                return false;

            if (data.ProductGroupId != productType.productGroupId)
            {
                var countProductGroupUsed = this.context.tbl_Product.Count(x => x.ProductTypeId == productTypeId);
                if (countProductGroupUsed > 0)
                {
                    throw new Exception("The product group for this product type cannot be changed because the product type is already in use");
                }
            }

            data.ProductTypeName = productType.productTypeName;
            data.ProductGroupId = productType.productGroupId;
            data.RequirePrincipalGL = productType.requirePrincipalGl;
            data.RequirePremiumDiscountGL = productType.requirePremiumDiscountGl;
            data.RequireDormantGL = productType.requireDormantGl;
            data.RequireOverdrawnGL = productType.requireOverdrawnGL;
            data.RequireInterestIncomeExpenseGL = productType.requireInterestIncomeExpenseGl;
            data.RequireInterestReceivablePayableGL = productType.requireInterestReceivablePayableGl;
            data.DealClassificationId = productType.dealClassificationId;
            data.RequireRate = productType.requireRate;
            data.RequireTenor = productType.requireTenor;
            data.RequireScheduleType = productType.requireScheduleType;

            // Audit Section ---------------------------
            var audit = new tbl_Audit
            {
                AuditTypeId = (short)AuditTypeEnum.ProductTypeUpdated,
                StaffId = productType.createdBy,
                BranchId = (short)productType.userBranchId,
                Detail = $"Updated tbl_Product Type: '{productType.productTypeName}' ",
                IPAddress = productType.userIPAddress,
                Url = productType.applicationUrl,
                ApplicationDate = genSetup.GetApplicationDate(),
                SystemDateTime = DateTime.Now
            };

            this.auditTrail.AddAuditTrail(audit);
            //end of Audit section ------------------------------- 
            return this.SaveAll();
        }
        
        #region tbl_Product Region
        public IEnumerable<ApprovalStatusViewModel> GetApprovalStatus()
        {
            return from ap in context.tbl_Approval_Status
                   select new ApprovalStatusViewModel
                   {
                       approvalStatusId = ap.ApprovalStatusId,
                       approvalStatusName = ap.ApprovalStatusName,
                       forDisplay = ap.ForDisplay,
                   };
        }
        private IQueryable<ProductViewModel> AllProduct()
        {
            return (from data in context.tbl_Product
                    select new ProductViewModel()
                    {
                        productId = data.ProductId,
                        companyId = data.CompanyId,
                        productTypeId = data.ProductTypeId,
                        productTypeName = data.tbl_Product_Type.ProductTypeName,
                        productGroupName = data.tbl_Product_Type.tbl_Product_Group.ProductGroupName,
                        productCategoryId = data.ProductCategoryId,
                        productCategoryName = data.tbl_Product_Category.ProductCategoryName,
                        productClassId = data.ProductClassId,
                        productClassName = data.tbl_Product_Class.ProductClassName,

                        productPriceIndexId = data.ProductPriceIndexId,
                        productPriceIndexName = data.tbl_Product_Price_Index.PriceIndexName,
                        productPriceIndexSpread = data.ProductPriceIndexSpread,

                        productCode = data.ProductCode,
                        productName = data.ProductName,
                        productDescription = data.ProductDescription,

                        productGroupId = data.tbl_Product_Type.ProductGroupId,

                        principalBalanceGl = data.PrincipalBalanceGL,
                        principalBalanceGlCode = (data.PrincipalBalanceGL.HasValue ? data.tbl_Chart_Of_Account.AccountCode : ""),

                        interestIncomeExpenseGl = data.InterestIncomeExpenseGL,
                        interestIncomeExpenseGlCode = (data.InterestIncomeExpenseGL.HasValue ? data.tbl_Chart_Of_Account.AccountCode : ""),

                        interestReceivablePayableGl = data.InterestReceivablePayableGL,
                        interestReceivablePayableGlCode = (data.InterestReceivablePayableGL.HasValue ? data.tbl_Chart_Of_Account.AccountCode : ""),

                        dormantGl = data.DormantGL,
                        premiumDiscountGl = data.PremiumDiscountGL,

                        dealTypeId = data.DealTypeId,
                        dealTypeName = data.tbl_Deal_Type.DealTypeName,
                        dealClassificationId = data.DealClassificationId,
                        dealClassificationName = data.tbl_Deal_Classification.Classification,
                        dayCountId = data.DayCountConventionId,
                        dayCountName = data.tbl_Day_Count_Convention.DayCountConventionName,

                        maximumTenor = data.MaximumTenor,
                        minimumTenor = data.MinimumTenor,
                        maximumRate = data.MaximumRate,
                        minimumRate = data.MinimumRate,
                        minimumBalance = data.MinimumBalance,
                        approvedBy = data.ApprovedBy,
                        completed = data.Completed,
                        approved = data.Approved,
                        currencies = context.tbl_Product_Currency.Where(curr => curr.ProductId == data.ProductId && curr.Deleted != false)
                        .Select(c => new ProductCurrencyViewModel()
                        {
                            productId = c.ProductId,
                            productCurrencyId = c.ProductCurrencyId,
                            currencyId = c.CurrencyId,
                            currencyName = c.tbl_Currency.CurrencyCode + " -- " + c.tbl_Currency.CurrencyName

                        }).ToList(),

                        dateTimeUpdated = data.DateTimeUpdated,
                        deleted = data.Deleted,
                        deletedBy = data.DeletedBy,
                        dateTimeDeleted = data.DateTimeDeleted
                    });
        }

        public IEnumerable<ProductViewModel> GetAllProduct()
        {
            return AllProduct();
        }

        public ProductViewModel GetProductById(int productId)
        {
            return AllProduct().Where(p => p.productId == productId).SingleOrDefault();
        }

        public IEnumerable<ProductViewModel> GetProductByGroupAndCategory(short productGroupId, short productCategoryId)
        {
            return AllProduct().Where(p => p.productGroupId == productGroupId && p.productCategoryId == productCategoryId);
        }

        public IEnumerable<ProductViewModel> GetProductByTypeAndCategory(short productTypeId, short productCategoryId)
        {
            return AllProduct().Where(p => p.productTypeId == productTypeId && p.productCategoryId == productCategoryId);

        }

        public IEnumerable<ProductViewModel> GetProductAwaitingApprovals(int staffId, int companyId)
        {
            var levelResult = level.GetAllApprovalLevelStaffByStaffId(staffId, companyId, (int)OperationsEnum.ProductCreation);
            int staffApprovalLevelId = 0;

            if (levelResult != null) staffApprovalLevelId = levelResult.approvalLevelId;

            return (from c in context.tbl_Temp_Product
                    join coy in context.tbl_Company on c.CompanyId equals coy.CompanyId
                    join atrail in context.tbl_Approval_Trail on c.ProductId equals atrail.TargetId
                    where atrail.ApprovalStatusId == (int)ApprovalStatusEnum.Pending && c.IsCurrent == true
                          && atrail.OperationId == (int)OperationsEnum.ProductCreation && atrail.ToApprovalLevelId == staffApprovalLevelId
                    select new ProductViewModel()
                    {
                        productId = c.ProductId,
                        companyId = c.CompanyId,
                        productTypeId = c.ProductTypeId,
                        productTypeName = c.tbl_Product_Type.ProductTypeName,
                        productCategoryId = c.ProductCategoryId,
                        productCategoryName = c.tbl_Product_Category.ProductCategoryName,
                        productClassId = c.ProductClassId,
                        productClassName = c.tbl_Product_Class.ProductClassName,

                        productPriceIndexId = c.ProductPriceIndexId,
                        productPriceIndexName = c.tbl_Product_Price_Index.PriceIndexName,
                        productPriceIndexSpread = c.ProductPriceIndexSpread,

                        productCode = c.ProductCode,
                        productName = c.ProductName,
                        productDescription = c.ProductDescription,

                        productGroupId = c.tbl_Product_Type.ProductGroupId,
                        productGroupName = c.tbl_Product_Type.tbl_Product_Group.ProductGroupName,

                        principalBalanceGl = c.PrincipalBalanceGL,
                        principalBalanceGlCode = (c.PrincipalBalanceGL.HasValue ? c.tbl_Chart_Of_Account.AccountCode : ""),

                        interestIncomeExpenseGl = c.InterestIncomeExpenseGL,
                        interestIncomeExpenseGlCode = (c.InterestIncomeExpenseGL.HasValue ? c.tbl_Chart_Of_Account.AccountCode : ""),

                        interestReceivablePayableGl = c.InterestReceivablePayableGL,
                        interestReceivablePayableGlCode = (c.InterestReceivablePayableGL.HasValue ? c.tbl_Chart_Of_Account.AccountCode : ""),

                        dormantGl = c.DormantGL,
                        dormantGlCode = (c.DormantGL.HasValue ? c.tbl_Chart_Of_Account.AccountCode : ""),
                        premiumDiscountGl = c.PremiumDiscountGL,
                        premiumDiscountGlCode = (c.PremiumDiscountGL.HasValue ? c.tbl_Chart_Of_Account.AccountCode : ""),

                        dealTypeId = c.DealTypeId,
                        dealTypeName = c.tbl_Deal_Type.DealTypeName,
                        dealClassificationId = c.DealClassificationId,
                        dealClassificationName = c.tbl_Deal_Classification.Classification,
                        dayCountId = c.DayCountConventionId,
                        dayCountName = c.tbl_Day_Count_Convention.DayCountConventionName,

                        maximumTenor = c.MaximumTenor,
                        minimumTenor = c.MinimumTenor,
                        maximumRate = c.MaximumRate,
                        minimumRate = c.MinimumRate,
                        minimumBalance = c.MinimumBalance,
                        approvedBy = c.ApprovedBy,
                        completed = c.Completed,
                        approved = c.Approved,
                        approvalStatusId = c.ApprovalStatusId,
                        operationId = atrail.OperationId,
                        currencies = context.tbl_Temp_Product_Currency.Where(curr => curr.ProductId == c.ProductId && curr.Deleted == false).Select(c => new ProductCurrencyViewModel()
                        {
                            productId = c.ProductId,
                            productCurrencyId = c.ProductCurrencyId,
                            currencyId = c.CurrencyId,
                            currencyName = c.tbl_Currency.CurrencyCode + " -- " + c.tbl_Currency.CurrencyName

                        }).ToList(),
                        fees = context.tbl_Temp_Product_Fee.Where(curr => curr.ProductId == c.ProductId && curr.Deleted == false).Select(c => new ProductFeeViewModel()
                        {
                            productId = c.ProductId,
                            productFeeId = c.ProductFeeId,
                            feeId = c.ProductFeeId,
                            rateValue = c.RateValue,
                            dependentAmount = c.DependentAmount,
                            feeName = c.tbl_Fee.FeeName,
                            feeIntervalName = c.tbl_Fee.tbl_Fee_Interval.FeeIntervalName,
                            feeTargetName = c.tbl_Fee.tbl_Fee_Target.FeeTargetName,
                            feeTypeName = c.tbl_Fee.tbl_Fee_Type.FeeTypeName,
                            glAccountCode = c.tbl_Fee.tbl_Chart_Of_Account.AccountCode,
                            glAccountName = c.tbl_Fee.tbl_Chart_Of_Account.AccountName

                        }).ToList(),
                        collateral = context.tbl_Temp_Product_CollateralType.Where(coll => coll.ProductId == c.ProductId && coll.Deleted == false).Select(prodColl => new ProductCollateralTypeViewModel()
                        {
                            productId = prodColl.ProductId,
                            productCollateralId = prodColl.ProductCollateralTypeId,
                            collateralTypeName = prodColl.tbl_Collateral_Type.CollateralTypeName
                        }).ToList(),
                        dateTimeUpdated = c.DateTimeUpdated,
                        deleted = c.Deleted,
                        deletedBy = c.DeletedBy,
                        dateTimeDeleted = c.DateTimeDeleted

                    });
        }

        public ProductViewModel GetTempProductDetail(int productId)
        {
            //return GetTempStaffDetails().Where(x => x.StaffId == staffId).Single();

            return (from c in context.tbl_Temp_Product
                    join coy in context.tbl_Company on c.CompanyId equals coy.CompanyId
                    where c.ProductId == productId
                    select new ProductViewModel()
                    {
                        productId = c.ProductId,
                        companyId = c.CompanyId,
                        productTypeId = c.ProductTypeId,
                        productTypeName = c.tbl_Product_Type.ProductTypeName,
                        productGroupName = c.tbl_Product_Type.tbl_Product_Group.ProductGroupName,
                        productCategoryId = c.ProductCategoryId,
                        productCategoryName = c.tbl_Product_Category.ProductCategoryName,
                        productClassId = c.ProductClassId,
                        productClassName = c.tbl_Product_Class.ProductClassName,

                        productPriceIndexId = c.ProductPriceIndexId,
                        productPriceIndexName = c.tbl_Product_Price_Index.PriceIndexName,
                        productPriceIndexSpread = c.ProductPriceIndexSpread,

                        productCode = c.ProductCode,
                        productName = c.ProductName,
                        productDescription = c.ProductDescription,

                        productGroupId = c.tbl_Product_Type.ProductGroupId,

                        principalBalanceGl = c.PrincipalBalanceGL,
                        principalBalanceGlCode = (c.PrincipalBalanceGL.HasValue ? c.tbl_Chart_Of_Account.AccountCode : ""),

                        interestIncomeExpenseGl = c.InterestIncomeExpenseGL,
                        interestIncomeExpenseGlCode = (c.InterestIncomeExpenseGL.HasValue ? c.tbl_Chart_Of_Account.AccountCode : ""),

                        interestReceivablePayableGl = c.InterestReceivablePayableGL,
                        interestReceivablePayableGlCode = (c.InterestReceivablePayableGL.HasValue ? c.tbl_Chart_Of_Account.AccountCode : ""),

                        dormantGl = c.DormantGL,
                        premiumDiscountGl = c.PremiumDiscountGL,

                        dealTypeId = c.DealTypeId,
                        dealClassificationId = c.DealClassificationId,
                        dayCountId = c.DayCountConventionId,

                        maximumTenor = c.MaximumTenor,
                        minimumTenor = c.MinimumTenor,
                        maximumRate = c.MaximumRate,
                        minimumRate = c.MinimumRate,
                        minimumBalance = c.MinimumBalance,
                        approvedBy = c.ApprovedBy,
                        completed = c.Completed,
                        approved = c.Approved,
                        approvalStatusId = c.ApprovalStatusId,

                        currencies = context.tbl_Temp_Product_Currency.Where(curr => curr.ProductId == c.ProductId && curr.Deleted != false).Select(c => new ProductCurrencyViewModel()
                        {
                            productId = c.ProductId,
                            productCurrencyId = c.ProductCurrencyId,
                            currencyId = c.CurrencyId,
                            currencyName = c.tbl_Currency.CurrencyCode + " -- " + c.tbl_Currency.CurrencyName
                        }).ToList(),

                        fees = context.tbl_Temp_Product_Fee.Where(curr => curr.ProductId == c.ProductId && curr.Deleted != false).Select(c => new ProductFeeViewModel()
                        {
                            productId = c.ProductId,
                            productFeeId = c.ProductFeeId,
                            feeId = c.ProductFeeId,
                            rateValue = c.RateValue,
                            dependentAmount = c.DependentAmount,
                            feeName = c.tbl_Fee.FeeName,
                            feeIntervalName = c.tbl_Fee.tbl_Fee_Interval.FeeIntervalName,
                            feeTargetName = c.tbl_Fee.tbl_Fee_Target.FeeTargetName,
                            feeTypeName = c.tbl_Fee.tbl_Fee_Type.FeeTypeName,
                            glAccountCode = c.tbl_Fee.tbl_Chart_Of_Account.AccountCode,
                            glAccountName = c.tbl_Fee.tbl_Chart_Of_Account.AccountName

                        }).ToList(),

                        dateTimeUpdated = c.DateTimeUpdated,
                        deleted = c.Deleted,
                        deletedBy = c.DeletedBy,
                        dateTimeDeleted = c.DateTimeDeleted
                    }).FirstOrDefault();
        }

        public ProductViewModel GetProductDetail(string productCode, int companyId)
        {
            return AllProduct().Where(p => p.productCode == productCode && p.companyId == companyId).SingleOrDefault();
        }

        public async Task<bool> GoForApproval(ApprovalViewModel entity)
        {
            entity.operationId = (int)OperationsEnum.ProductCreation;

            var response = await workFlow.GoForApproval(entity);

            if (response.Item1)
            {
                return ApproveProduct(entity.targetId, response.Item2.approvalStatusId, entity);
            }
            else
            {
                return false;
            }

        }

        private bool ApproveProduct(int productId, short approvalStatusId, UserInfo user)
        {
            var productModel = context.tbl_Temp_Product.Find(productId);
            var productToUpdate = context.tbl_Product.Where(x => x.ProductCode == productModel.ProductCode);

            var currModel = context.tbl_Temp_Product_Currency.Where(c => c.ProductId == productModel.ProductId && c.Deleted == false);
            var currListToUpdate = context.tbl_Product_Currency.Where(x => x.ProductId == productModel.ProductId && x.Deleted == false);

            if (productToUpdate.Any()) //Update existing product with tempProduct record
            {
                var existingProduct = productToUpdate.First();
                existingProduct.PrincipalBalanceGL = productModel.PrincipalBalanceGL;
                existingProduct.InterestIncomeExpenseGL = productModel.InterestIncomeExpenseGL;
                existingProduct.InterestReceivablePayableGL = productModel.InterestReceivablePayableGL;
                existingProduct.DormantGL = productModel.DormantGL;
                existingProduct.PremiumDiscountGL = productModel.PremiumDiscountGL;
                existingProduct.OverdrawnGL = productModel.OverdrawnGL;

                existingProduct.ProductPriceIndexId = productModel.ProductPriceIndexId;
                existingProduct.ProductPriceIndexSpread = productModel.ProductPriceIndexSpread;

                existingProduct.DealTypeId = productModel.DealTypeId;
                existingProduct.DealClassificationId = productModel.DealClassificationId;
                existingProduct.DayCountConventionId = productModel.DayCountConventionId;

                existingProduct.MaximumTenor = productModel.MaximumTenor;
                existingProduct.MinimumTenor = productModel.MinimumTenor;
                existingProduct.MaximumRate = productModel.MaximumRate;
                existingProduct.MinimumRate = productModel.MinimumRate;
                existingProduct.MinimumBalance = productModel.MinimumBalance;

                existingProduct.AllowRate = productModel.AllowRate;
                existingProduct.AllowTenor = productModel.AllowTenor;
                existingProduct.AllowOverdrawn = productModel.AllowOverdrawn;

                foreach (var c in currListToUpdate)
                {
                    var curr = new tbl_Product_Currency()
                    {
                        //ProductId = c.ProductId,
                        CurrencyId = c.CurrencyId,
                        DateTimeCreated = genSetup.GetApplicationDate(),
                    };
                    context.tbl_Product_Currency.Add(curr);
                }
            }
            else //Insert a new product record into the real product table
            {
                var product = new tbl_Product()
                {
                    CompanyId = productModel.CompanyId,
                    ProductTypeId = productModel.ProductTypeId,
                    ProductCategoryId = productModel.ProductCategoryId,
                    ProductClassId = productModel.ProductClassId,
                    ProductCode = GenerateProductCode(productModel.CompanyId),
                    ProductName = productModel.ProductName,
                    ProductDescription = productModel.ProductDescription,


                    PrincipalBalanceGL = productModel.PrincipalBalanceGL,
                    InterestIncomeExpenseGL = productModel.InterestIncomeExpenseGL,
                    InterestReceivablePayableGL = productModel.InterestReceivablePayableGL,
                    DormantGL = productModel.DormantGL,
                    PremiumDiscountGL = productModel.PremiumDiscountGL,
                    OverdrawnGL = productModel.OverdrawnGL,

                    ProductPriceIndexId = productModel.ProductPriceIndexId,
                    ProductPriceIndexSpread = productModel.ProductPriceIndexSpread,

                    DealTypeId = productModel.DealTypeId,
                    DealClassificationId = productModel.DealClassificationId,
                    DayCountConventionId = productModel.DayCountConventionId,

                    MaximumTenor = productModel.MaximumTenor,
                    MinimumTenor = productModel.MinimumTenor,
                    MaximumRate = productModel.MaximumRate,
                    MinimumRate = productModel.MinimumRate,
                    MinimumBalance = productModel.MinimumBalance,

                    AllowRate = productModel.AllowRate,
                    AllowTenor = productModel.AllowTenor,
                    AllowOverdrawn = productModel.AllowOverdrawn,

                    CreatedBy = productModel.CreatedBy,
                    DateTimeCreated = genSetup.GetApplicationDate(),

                };
                context.tbl_Product.Add(product);

                foreach (var c in currModel)
                {
                    var curr = new tbl_Product_Currency()
                    {
                       ProductId = c.ProductId,
                        CurrencyId = c.CurrencyId,
                        DateTimeCreated = genSetup.GetApplicationDate(),
                    };
                    context.tbl_Product_Currency.Add(curr);
                }
                productFee.ApproveProductFee(productId, user);
                productCollateralType.ApproveProductCollateral(productId, user);
            }

            productModel.IsCurrent = false;
            productModel.ApprovalStatusId = approvalStatusId;
            productModel.DateTimeUpdated = DateTime.Now;


            // Audit Section ---------------------------
            var audit = new tbl_Audit
            {
                AuditTypeId = (short)AuditTypeEnum.StaffApproved,
                StaffId = user.staffId,
                BranchId = (short)user.BranchId,
                Detail = $"Approved Product '{productModel.ProductName}' with product code'{productModel.ProductCode}'",
                IPAddress = user.userIPAddress,
                Url = user.applicationUrl,
                ApplicationDate = genSetup.GetApplicationDate(),
                SystemDateTime = DateTime.Now
            };

            this.auditTrail.AddAuditTrail(audit);
            // Audit Section ---------------------------

            return this.SaveAll();
        }

        public async Task<ProductViewModel> AddTempProduct(ProductViewModel productModel)
        {
            if (productModel.currencies.Count < 1)
                throw new Exception("Product Currency must be specified. Please select a principal GL with mapped currencies");

            bool output = false;
            var existStingTempProduct = context.tbl_Temp_Product.Where(x => x.ProductCode.ToLower() == productModel.productCode.ToLower()
                                                                  && x.IsCurrent == true && x.CompanyId == productModel.companyId
                                                                  && x.ApprovalStatusId == (short)ApprovalStatusEnum.Pending);

            if (existStingTempProduct.Any())
            {
                throw new Exception("Product Information already exist and is undergoing approval");
            }

            List<tbl_Temp_Product_Currency> currencies = new List<tbl_Temp_Product_Currency>();

            //Storing the product currencies
            foreach (var item in productModel.currencies)
            {
                var productCurrency = new tbl_Temp_Product_Currency()
                {
                    //ProductId = (short)item.productId,
                    CurrencyId = item.currencyId,
                    CreatedBy = item.createdBy,
                    DateTimeCreated = genSetup.GetApplicationDate()
                };
                currencies.Add(productCurrency);
            }

            //End of storing the product currencies
            var product = new tbl_Temp_Product()
            {
                CompanyId = productModel.companyId,
                ProductTypeId = productModel.productTypeId,
                ProductCategoryId = productModel.productCategoryId,
                ProductClassId = productModel.productClassId,
                ProductCode = GenerateProductCode(productModel.companyId),
                ProductName = productModel.productName,
                ProductDescription = productModel.productDescription,


                PrincipalBalanceGL = productModel.principalBalanceGl,
                InterestIncomeExpenseGL = productModel.interestIncomeExpenseGl,
                InterestReceivablePayableGL = productModel.interestReceivablePayableGl,
                DormantGL = productModel.dormantGl,
                PremiumDiscountGL = productModel.premiumDiscountGl,
                OverdrawnGL = productModel.overdrawnGl,

                ProductPriceIndexId = productModel.productPriceIndexId,
                ProductPriceIndexSpread = productModel.productPriceIndexSpread,

                DealTypeId = productModel.dealTypeId,
                DealClassificationId = productModel.dealClassificationId,
                DayCountConventionId = productModel.dayCountId,

                MaximumTenor = productModel.maximumTenor,
                MinimumTenor = productModel.minimumTenor,
                MaximumRate = productModel.maximumRate,
                MinimumRate = productModel.minimumRate,
                MinimumBalance = productModel.minimumBalance,

                AllowRate = productModel.allowRate,
                AllowTenor = productModel.allowTenor,
                AllowOverdrawn = productModel.allowOverdrawn,

                CreatedBy = productModel.createdBy,
                DateTimeCreated = DateTime.Now,
                ApprovalStatusId = (short)ApprovalStatusEnum.Pending,
                IsCurrent = true,

                tbl_Temp_Product_Currency = currencies

            };

            // Audit Section ---------------------------
            var audit = new tbl_Audit
            {
                AuditTypeId = (short)AuditTypeEnum.ProductAdded,
                StaffId = productModel.createdBy,
                BranchId = (short)productModel.userBranchId,
                Detail = $"Initiated Product Creation for '{productModel.productName}' with code'{productModel.productCode}'",
                IPAddress = productModel.userIPAddress,
                Url = productModel.applicationUrl,
                ApplicationDate = genSetup.GetApplicationDate(),
                SystemDateTime = DateTime.Now
            };

            if (workFlow.CheckRouteForOperation((int)OperationsEnum.ProductCreation, productModel.companyId))
            {
                using (var trans = context.Database.BeginTransaction())
                {
                    try
                    {
                        auditTrail.AddAuditTrail(audit);
                        this.context.tbl_Temp_Product.Add(product);
                        output = await context.SaveChangesAsync() > 0;

                        var entity = new ApprovalViewModel
                        {
                            staffId = productModel.createdBy,
                            companyId = productModel.companyId,
                            approvalStatusId = (int)ApprovalStatusEnum.Pending,
                            targetId = product.ProductId,
                            operationId = (int)OperationsEnum.ProductCreation,
                            BranchId = productModel.userBranchId
                        };
                        var response = await workFlow.LogForApproval(entity);
                        trans.Commit();
                    }
                    catch (Exception ex)
                    {
                        trans.Rollback();
                        throw new Exception(ex.Message);
                    }
                }
            }
            else
            {
                throw new Exception("Approval route have not been defined for this operation");
            }

            if (output)
            {
                return new ProductViewModel { productId = product.ProductId, productCode = product.ProductCode };
            }
            else
                return null;
        }

        public bool IsProductCodeAlreadyExist(string productCode)
        {
            return context.tbl_Product.Any(x => x.ProductCode.ToLower() == productCode.ToLower());
        }

        public bool IsProductExist(string productCode)
        {
            return context.tbl_Temp_Product.Any(x => x.ProductCode.ToLower() == productCode.ToLower() && x.ApprovalStatusId == (int)ApprovalStatusEnum.Pending && x.IsCurrent == true);
        }

        //private ProductViewModel AddProduct2(ProductViewModel product)
        //{
        //    var data = new tbl_Product()
        //    {
        //        CompanyId = product.companyId,
        //        ProductTypeId = product.productTypeId,
        //        ProductCategoryId = product.productCategoryId,
        //        ProductClassId = product.productClassId,
        //        ProductCode = GenerateProductCode(product.companyId),
        //        ProductName = product.productName,
        //        ProductDescription = product.productDescription,


        //        PrincipalBalanceGL = product.principalBalanceGl,
        //        InterestIncomeExpenseGL = product.interestIncomeExpenseGl,
        //        InterestReceivablePayableGL = product.interestReceivablePayableGl,
        //        DormantGL = product.dormantGl,
        //        PremiumDiscountGL = product.premiumDiscountGl,
        //        OverdrawnGL = product.overdrawnGl,

        //        ProductPriceIndexId = product.productPriceIndexId,
        //        ProductPriceIndexSpread = product.productPriceIndexSpread,

        //        DealTypeId = product.dealTypeId,
        //        DealClassificationId = product.dealClassificationId,
        //        DayCountId = product.dayCountId,

        //        MaximumTenor = product.maximumTenor,
        //        MinimumTenor = product.minimumTenor,
        //        MaximumRate = product.maximumRate,
        //        MinimumRate = product.minimumRate,
        //        MinimumBalance = product.minimumBalance,

        //        AllowRate = product.allowRate,
        //        AllowTenor = product.allowTenor,
        //        AllowOverdrawn = product.allowOverdrawn,
        //        //ApprovedBy = product.approvedBy,
        //        //Completed = product.completed,
        //        //Approved = product.approved,

        //        CreatedBy = product.createdBy,
        //        DateTimeCreated = DateTime.Now,
        //    };

        //    this.context.tbl_Product.Add(data);

        //    // Audit Section ---------------------------
        //    var audit = new tbl_Audit
        //    {
        //        AuditTypeId = (short)AuditTypeEnum.ProductAdded,
        //        StaffId = (int)product.createdBy,
        //        BranchId = (short)product.userBranchId,
        //        Detail = $"Added tbl_Product: '{product.productTypeName}' ",
        //        IPAddress = product.userIPAddress,
        //        Url = product.applicationUrl,
        //        ApplicationDate = genSetup.GetApplicaionDate(),
        //        SystemDateTime = DateTime.Now
        //    };

        //    this.auditTrail.AddAuditTrail(audit);
        //    //end of Audit section ------------------------------- 

        //    var status = this.SaveAll();

        //    if (status)
        //    {
        //        product.productCode = data.ProductCode;
        //        product.productId = data.ProductId;
        //        return product;
        //    }
        //    else
        //        return null;
        //}
        public bool UpdateProduct(int productId, ProductViewModel productModel)
        {
            var existStingTempProduct = context.tbl_Temp_Product.Where(x => x.ProductCode.ToLower() == productModel.productCode.ToLower() && x.IsCurrent == true && x.ApprovalStatusId == (int)ApprovalStatusEnum.Approved);
            var existingTempCurrencies = context.tbl_Temp_Product_Currency.Where(x => x.ProductId == productId).ToList();

            if (existingTempCurrencies.Count > 0)
            {
                foreach (var curr in existingTempCurrencies)
                {
                    context.tbl_Temp_Product_Currency.Remove(curr);
                }
            }

            if (existStingTempProduct.Any() && existingTempCurrencies.Any())
            {
                foreach (var item in existStingTempProduct)
                {
                    item.IsCurrent = false;
                    item.DateTimeUpdated = DateTime.Now;
                }

                foreach (var item in existingTempCurrencies)
                {
                    item.IsCurrent = false;
                    item.DateTimeUpdated = DateTime.Now;
                }
            }

            var targetProduct = context.tbl_Product.Find(productId);

            var unApprovedProductEdit = context.tbl_Temp_Product.Where(x => x.IsCurrent == true && x.ApprovalStatusId == (int)ApprovalStatusEnum.Pending);

            tbl_Temp_Product tempProduct;
            List<tbl_Temp_Product_Currency> tempCurrencies = new List<tbl_Temp_Product_Currency>();

            if (unApprovedProductEdit.Any())
            {
                throw new Exception("Product is already undergoing approval");
            }
            else
            {
                //Storing the updated product currencies
                foreach (var item in productModel.currencies)
                {
                    var productCurrency = new tbl_Temp_Product_Currency()
                    {
                        //ProductId = item.productId,
                        CurrencyId = item.currencyId,
                        CreatedBy = productModel.createdBy,
                        DateTimeCreated = genSetup.GetApplicationDate()
                    };
                    tempCurrencies.Add(productCurrency);
                }

                //End of storing the updated product currencies
                tempProduct = new tbl_Temp_Product()
                {
                    CompanyId = productModel.companyId,
                    ProductTypeId = productModel.productTypeId,
                    ProductCategoryId = productModel.productCategoryId,
                    ProductClassId = productModel.productClassId,
                    ProductCode = GenerateProductCode(productModel.companyId),
                    ProductName = productModel.productName,
                    ProductDescription = productModel.productDescription,


                    PrincipalBalanceGL = productModel.principalBalanceGl,
                    InterestIncomeExpenseGL = productModel.interestIncomeExpenseGl,
                    InterestReceivablePayableGL = productModel.interestReceivablePayableGl,
                    DormantGL = productModel.dormantGl,
                    PremiumDiscountGL = productModel.premiumDiscountGl,
                    OverdrawnGL = productModel.overdrawnGl,

                    ProductPriceIndexId = productModel.productPriceIndexId,
                    ProductPriceIndexSpread = productModel.productPriceIndexSpread,

                    DealTypeId = productModel.dealTypeId,
                    DealClassificationId = productModel.dealClassificationId,
                    DayCountConventionId = productModel.dayCountId,

                    MaximumTenor = productModel.maximumTenor,
                    MinimumTenor = productModel.minimumTenor,
                    MaximumRate = productModel.maximumRate,
                    MinimumRate = productModel.minimumRate,
                    MinimumBalance = productModel.minimumBalance,

                    AllowRate = productModel.allowRate,
                    AllowTenor = productModel.allowTenor,
                    AllowOverdrawn = productModel.allowOverdrawn,

                    CreatedBy = productModel.createdBy,
                    DateTimeCreated = DateTime.Now,
                    ApprovalStatusId = (short)ApprovalStatusEnum.Pending,
                    IsCurrent = true,
                };
                context.tbl_Temp_Product.Add(tempProduct);
            }

            var audit = new tbl_Audit
            {
                AuditTypeId = (short)AuditTypeEnum.StaffUpdated,
                StaffId = productModel.createdBy,
                BranchId = (short)productModel.userBranchId,
                Detail = $"Updated Product '{productModel.productName}' with code'{productModel.productCode}'",
                IPAddress = productModel.userIPAddress,
                Url = productModel.applicationUrl,
                ApplicationDate = genSetup.GetApplicationDate(),
                SystemDateTime = DateTime.Now,
                TargetId = productId
            };

            this.auditTrail.AddAuditTrail(audit);
            //end of Audit section -------------------------------  

            var output = this.SaveAll();

            var entity = new ApprovalViewModel
            {
                staffId = productModel.createdBy,
                companyId = productModel.companyId,
                approvalStatusId = (int)ApprovalStatusEnum.Pending,
                targetId = tempProduct.ProductId,
                operationId = (int)OperationsEnum.ProductCreation,
                BranchId = productModel.userBranchId
            };
            var response = workFlow.LogForApproval(entity);

            return output;
        }

        //private bool UpdateProduct2(int productId, ProductViewModel product)
        //{
        //    var data = this.context.tbl_Product.FirstOrDefault(x => x.ProductId == productId);

        //    if (data == null)
        //        return false;

        //    //data.CompanyId = product.companyId;
        //    //data.ProductTypeId = product.productTypeId;
        //    //data.ProductCategoryId = product.productCategoryId;
        //    //data.ProductCode = product.productCode;
        //    data.ProductName = product.productName;
        //    //data.ProductPriceIndexId = product.productPriceIndexId;
        //    //data.ProductDescription = product.productDescription;
        //    //data.PrincipalBalanceGl = product.principalBalanceGl;
        //    //data.InterestIncomeExpenseGl = product.interestIncomeExpenseGl;
        //    //data.InterestReceivablePayableGl = product.interestReceivablePayableGl;
        //    //data.DormantGl = product.dormantGl;
        //    //data.PremiumDiscountGl = product.premiumDiscountGl;
        //    //data.DealType = product.dealType;
        //    //data.DealClassificationId = product.dealClassificationId;
        //    //data.MaximumTenor = product.maximumTenor;
        //    //data.MinimumTenor = product.minimumTenor;
        //    //data.MaximumRate = product.maximumRate;
        //    //data.MinimumRate = product.minimumRate;
        //    //data.MinimumBalance = product.minimumBalance;

        //    //data.ApprovedBy = product.approvedBy;
        //    //data.Completed = product.completed;
        //    //data.Approved = product.approved;

        //    data.LastUpdatedBy = product.lastUpdatedBy;
        //    data.DateTimeUpdated = DateTime.Now;

        //    // Audit Section ---------------------------
        //    var audit = new tbl_Audit
        //    {
        //        AuditTypeId = (short)AuditTypeEnum.ProductUpdated,
        //        StaffId = (int)product.createdBy,
        //        BranchId = (short)product.userBranchId,
        //        Detail = $"Updated tbl_Product: '{product.productTypeName}' ",
        //        IPAddress = product.userIPAddress,
        //        Url = product.applicationUrl,
        //        ApplicationDate = genSetup.GetApplicaionDate(),
        //        SystemDateTime = DateTime.Now
        //    };

        //    this.auditTrail.AddAuditTrail(audit);
        //    //end of Audit section -------------------------------
        //    return this.SaveAll();
        //}


        //public bool DeleteProduct(int productId)
        //{
        //    throw new NotImplementedException();
        //}
        #endregion tbl_Product Region

        #region product Price Index
        private IEnumerable<ProductPriceIndexViewModel> GetAllProductPriceIndex(int companyId)
        {
            return (from data in context.tbl_Product_Price_Index
                    where data.CompanyId == companyId
                    select new ProductPriceIndexViewModel()
                    {
                        productPriceIndexId = data.ProductPriceIndexId,
                        priceIndexDescription = data.PriceIndexDescription,
                        companyId = data.CompanyId,
                        priceIndexName = data.PriceIndexName,
                        priceIndexRate = data.PriceIndexRate,
                        dateTimeUpdated = data.DateTimeUpdated,
                        deleted = data.Deleted,
                        deletedBy = data.DeletedBy,
                        dateTimeDeleted = data.DateTimeDeleted
                    });
        }
        public IEnumerable<ProductPriceIndexViewModel> GetProductPriceIndex(int companyId)
        {
            return GetAllProductPriceIndex(companyId);
        }

        public ProductPriceIndexViewModel GetProductPriceIndexById(int productPriceIndexId, int companyId)
        {
            return GetAllProductPriceIndex(companyId).Where(c => c.productPriceIndexId == productPriceIndexId).SingleOrDefault();
        }

        public ProductPriceIndexViewModel AddProductPriceIndex(ProductPriceIndexViewModel prodPriceIndex)
        {
            var data = new tbl_Product_Price_Index()
            {
                CompanyId = prodPriceIndex.companyId,
                PriceIndexDescription = prodPriceIndex.priceIndexDescription,
                PriceIndexName = prodPriceIndex.priceIndexName,
                PriceIndexRate = prodPriceIndex.priceIndexRate,
                CreatedBy = prodPriceIndex.createdBy,
                DateTimeCreated = DateTime.Now,
            };

            this.context.tbl_Product_Price_Index.Add(data);

            // Audit Section ---------------------------
            var audit = new tbl_Audit
            {
                AuditTypeId = (short)AuditTypeEnum.ProductPriceIndexAdded,
                StaffId = (int)prodPriceIndex.createdBy,
                BranchId = (short)prodPriceIndex.userBranchId,
                Detail = $"Added tbl_Product Price Index: '{prodPriceIndex.priceIndexName}' ",
                IPAddress = prodPriceIndex.userIPAddress,
                Url = prodPriceIndex.applicationUrl,
                ApplicationDate = genSetup.GetApplicationDate(),
                SystemDateTime = DateTime.Now
            };

            this.auditTrail.AddAuditTrail(audit);
            //end of Audit section ------------------------------- 

            var status = this.SaveAll();

            if (status)
            {
                return prodPriceIndex;
            }
            else
                return null;
        }

        public bool UpdateProductPriceIndex(int productPriceIndexId, ProductPriceIndexViewModel prodPriceIndex)
        {
            var data = this.context.tbl_Product_Price_Index.FirstOrDefault(x => x.ProductPriceIndexId == productPriceIndexId);

            if (data == null)
                return false;

            data.PriceIndexName = prodPriceIndex.priceIndexName;
            data.PriceIndexDescription = prodPriceIndex.priceIndexDescription;
            data.PriceIndexRate = prodPriceIndex.priceIndexRate;

            data.LastUpdatedBy = prodPriceIndex.lastUpdatedBy;
            data.DateTimeUpdated = DateTime.Now;

            // Audit Section ---------------------------
            var audit = new tbl_Audit
            {
                AuditTypeId = (short)AuditTypeEnum.ProductPriceIndexUpdated,
                StaffId = (int)prodPriceIndex.createdBy,
                BranchId = (short)prodPriceIndex.userBranchId,
                Detail = $"Updated tbl_Product Price Index: '{prodPriceIndex.priceIndexName}' ",
                IPAddress = prodPriceIndex.userIPAddress,
                Url = prodPriceIndex.applicationUrl,
                ApplicationDate = genSetup.GetApplicationDate(),
                SystemDateTime = DateTime.Now
            };

            this.auditTrail.AddAuditTrail(audit);
            //end of Audit section -------------------------------
            return this.SaveAll();
        }

        public bool DeleteProductPriceIndex(int productPriceIndexId, UserInfo user)
        {
            var data = this.context.tbl_Product_Price_Index.Find(productPriceIndexId);

            if (data == null)
                return false;

            data.Deleted = true;
            data.DateTimeDeleted = genSetup.GetApplicationDate();

            // Audit Section ---------------------------
            var productPriceIndex = this.context.tbl_Product_Price_Index.FirstOrDefault(x => x.ProductPriceIndexId == data.ProductPriceIndexId);
            var audit = new tbl_Audit
            {
                AuditTypeId = (short)AuditTypeEnum.ProductPriceIndexDeleted,
                StaffId = user.staffId,
                BranchId = (short)user.BranchId,
                Detail = $"Deleted tbl_Product Price Index: '{data.PriceIndexName}' with rate '{data.PriceIndexRate}' ",
                IPAddress = user.userIPAddress,
                Url = user.applicationUrl,
                ApplicationDate = genSetup.GetApplicationDate(),
                SystemDateTime = DateTime.Now,
                TargetId = productPriceIndexId
            };

            this.auditTrail.AddAuditTrail(audit);

            //end of Audit section -------------------------------
            return this.SaveAll();
        }

        #endregion product Price Index
    }
}