using FintrakBanking.Common.Enum;
using FintrakBanking.Entities.Models;
using FintrakBanking.Interfaces.Admin;
using FintrakBanking.Interfaces.Setups.Approval;
using FintrakBanking.Interfaces.Setups.General;
using FintrakBanking.Interfaces.WorkFlow;
using FintrakBanking.ViewModels;
using FintrakBanking.ViewModels.Credit;
using FintrakBanking.ViewModels.Setups.General;
using FintrakBanking.ViewModels.WorkFlow;
using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using System.Threading.Tasks;

namespace FintrakBanking.Repositories.Setups.General
{

    public class ProductRepository : IProductRepository
    {
        private FinTrakBankingContext context;
        private IGeneralSetupRepository genSetup;
        private IAuditTrailRepository auditTrail;
        private IWorkflow workFlow;
        private IApprovalLevelStaffRepository level;
        private IProductFeeRepository productFee;
        private IProductCollateralTypeRepository productCollateralType;

        public ProductRepository(FinTrakBankingContext _context,
                                IGeneralSetupRepository _genSetup,
                                IAuditTrailRepository _auditTrail,
            IWorkflow _workFlow,
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

        //public  IEnumerable<LookupViewModel> GetProductClassByProcessId(int processId)
        // {
        //     var data = context.TBL_PRODUCT_CLASS.Where(c => c.PRODUCT_CLASS_PROCESSID == processId).Select(c => new ProductClassViewModel
        //     {

        //     });
        // }

        private string GenerateProductCode(int companyId)
        {
            var data = this.context.TBL_PRODUCT.Count(x => x.COMPANYID == companyId);
            int counter = data + 1;
            var productCode = string.Empty;
            do
            {
                productCode = string.Format("{0}", counter.ToString().PadLeft(4, '0'));
                counter++;
            }
            while (context.TBL_TEMP_PRODUCT.Any(x => x.PRODUCTCODE == productCode) == true);

            return productCode;
        }

        public IEnumerable<ProductCategoryViewModel> GetAllProductCategory()
        {
            return this.context.TBL_PRODUCT_CATEGORY.Select(p => new ProductCategoryViewModel()
            {
                productCategoryId = p.PRODUCTCATEGORYID,
                productCategoryName = p.PRODUCTCATEGORYNAME
            });
        }

        public IEnumerable<RevolvingTypeViewModel> GetRevolvingTypes()
        {
            return this.context.TBL_LOAN_REVOLVING_TYPE.Select(r => new RevolvingTypeViewModel()
            {
                revolvingTypeId = r.REVOLVINGTYPEID,
                revolvingTypeName = r.REVOLVINGTYPENAME
            });
        }

        public IEnumerable<LookupViewModel> GetAllProductClass()
        {
            return (from data in context.TBL_PRODUCT_CLASS
                        //where data.OperationTypeId == operationTypeId
                    select new LookupViewModel()
                    {
                        lookupId = (short)data.PRODUCTCLASSID,
                        lookupName = data.PRODUCTCLASSNAME,
                        lookupTypeId = data.PRODUCTCLASSTYPEID,
                        lookupTypeName = data.TBL_PRODUCT_CLASS_TYPE.PRODUCTCLASSTYPENAME
                    });
        }

        public IEnumerable<LookupViewModel> GetAllProductClass(int customerTypeId, int processId)
        {
            return (from data in context.TBL_PRODUCT_CLASS
                    where data.CUSTOMERTYPEID == customerTypeId && data.PRODUCT_CLASS_PROCESSID == processId
                    //where data.OperationTypeId == operationTypeId
                    select new LookupViewModel()
                    {
                        lookupId = (short)data.PRODUCTCLASSID,
                        lookupName = data.PRODUCTCLASSNAME,
                        lookupTypeId = data.PRODUCTCLASSTYPEID,
                        lookupTypeName = data.TBL_PRODUCT_CLASS_TYPE.PRODUCTCLASSTYPENAME
                    });
        }

        public IEnumerable<LookupViewModel> GetProductClassByProcessId(int processId)
        {
            return (from data in context.TBL_PRODUCT_CLASS.Where(c => c.PRODUCT_CLASS_PROCESSID == processId)
                        //where data.OperationTypeId == operationTypeId
                    select new LookupViewModel()
                    {
                        lookupId = (short)data.PRODUCTCLASSID,
                        lookupName = data.PRODUCTCLASSNAME,
                        lookupTypeId = data.PRODUCTCLASSTYPEID,
                        lookupTypeName = data.TBL_PRODUCT_CLASS_TYPE.PRODUCTCLASSTYPENAME
                    }).ToList();
        }
        public IEnumerable<LookupViewModel> GetAllProductClassByCustomerTypeId(int customerTypeId)
        {
            return (from data in context.TBL_PRODUCT_CLASS
                    where data.CUSTOMERTYPEID == customerTypeId
                    //where data.OperationTypeId == operationTypeId
                    select new LookupViewModel()
                    {
                        lookupId = (short)data.PRODUCTCLASSID,
                        lookupName = data.PRODUCTCLASSNAME,
                        lookupTypeId = data.PRODUCTCLASSTYPEID,
                        lookupTypeName = data.TBL_PRODUCT_CLASS_TYPE.PRODUCTCLASSTYPENAME
                    });
        }

        public IEnumerable<LookupViewModel> GetAllProductBehaviourTypes()
        {
            var data = (from p in context.TBL_PRODUCT_BEHAVIOUR
                        select new LookupViewModel()
                        {
                            lookupId = (short)p.PRODUCT_BEHAVIOURID,
                            lookupName = "n/a" // p.PRODUCT_BEHAVIOURNAME
                        });

            return data;
        }

        public IEnumerable<LookupViewModel> GetProductCurrency(int productId)
        {
            var data = (from p in context.TBL_PRODUCT_CURRENCY
                        where p.PRODUCTID == productId
                        select new LookupViewModel()
                        {
                            lookupId = (short)p.CURRENCYID,
                            lookupName = p.TBL_CURRENCY.CURRENCYNAME + " " + p.TBL_CURRENCY.CURRENCYCODE,
                        });

            return data;
        }




        #region Product Group
        public IEnumerable<ProductGroupViewModel> GetAllProductGroup()
        {
            return (from p in context.TBL_PRODUCT_GROUP
                    where p.DELETED == false
                    orderby p.PRODUCTGROUPNAME
                    select new ProductGroupViewModel()
                    {
                        productGroupId = p.PRODUCTGROUPID,
                        productGroupCode = p.PRODUCTGROUPCODE,
                        productGroupName = p.PRODUCTGROUPNAME
                    });
        }

        public ProductGroupViewModel GetProductGroupById(short productGroupId)
        {
            var data = this.context.TBL_PRODUCT_GROUP.FirstOrDefault(x => x.PRODUCTGROUPID == productGroupId); // .Find(accountId);

            if (data == null)
                return null;

            return new ProductGroupViewModel
            {
                productGroupId = data.PRODUCTGROUPID,
                productGroupCode = data.PRODUCTGROUPCODE,
                productGroupName = data.PRODUCTGROUPNAME
            };
        }

        public bool AddProductGroup(ProductGroupViewModel productGroupModel)
        {
            var isProductGroupExist = context.TBL_PRODUCT_GROUP.Any(x =>
                x.PRODUCTGROUPNAME.ToLower() == productGroupModel.productGroupName.ToLower());

            if (isProductGroupExist)
            {
                throw new Exception("Product group already exists!");
            }

            var isProductCodeExist = context.TBL_PRODUCT_GROUP.Any(x =>
                x.PRODUCTGROUPCODE.ToLower() == productGroupModel.productGroupCode.ToLower());

            if (isProductCodeExist)
            {
                throw new Exception("Product group with that code already exists!");
            }

            var data = new TBL_PRODUCT_GROUP()
            {
                PRODUCTGROUPCODE = productGroupModel.productGroupCode,
                PRODUCTGROUPNAME = productGroupModel.productGroupName,
                CREATEDBY = productGroupModel.createdBy,
                DATETIMECREATED = DateTime.Now,
            };

            this.context.TBL_PRODUCT_GROUP.Add(data);

            // Audit Section ---------------------------
            var audit = new TBL_AUDIT
            {
                AUDITTYPEID = (short)AuditTypeEnum.ProductPriceIndexAdded,
                STAFFID = (int)productGroupModel.createdBy,
                BRANCHID = (short)productGroupModel.userBranchId,
                DETAIL = $"Added tbl_Product Group: '{productGroupModel.productGroupName}' ",
                IPADDRESS = productGroupModel.userIPAddress,
                URL = productGroupModel.applicationUrl,
                APPLICATIONDATE = genSetup.GetApplicationDate(),
                SYSTEMDATETIME = DateTime.Now
            };

            this.auditTrail.AddAuditTrail(audit);
            //end of Audit section -------------------------------

            var status = this.SaveAll();

            if (status)
            {
                return true;
            }

            return false;
        }

        public bool UpdateProductGroup(int productGroupId, ProductGroupViewModel productGroup)
        {
            var data = this.context.TBL_PRODUCT_GROUP.FirstOrDefault(x => x.PRODUCTGROUPID == productGroupId);

            if (data == null)
                return false;

            data.PRODUCTGROUPNAME = productGroup.productGroupName;
            // Audit Section ---------------------------
            var audit = new TBL_AUDIT
            {
                AUDITTYPEID = (short)AuditTypeEnum.ProductGroupAdded,
                STAFFID = productGroup.createdBy,
                BRANCHID = (short)productGroup.userBranchId,
                DETAIL = $"Updated tbl_Product Group: '{productGroup.productGroupName}' with code: '{productGroup.productGroupCode}' ",
                IPADDRESS = productGroup.userIPAddress,
                URL = productGroup.applicationUrl,
                APPLICATIONDATE = genSetup.GetApplicationDate(),
                SYSTEMDATETIME = DateTime.Now
            };
            this.auditTrail.AddAuditTrail(audit);
            //end of Audit section -------------------------------
            return this.SaveAll();
        }

        public bool DeleteProductGroup(int productGroupId, UserInfo user)
        {
            var data = context.TBL_PRODUCT_GROUP.Find(productGroupId);

            if (data == null)
                return false;

            data.DELETED = true;
            data.DATETIMEDELETED = genSetup.GetApplicationDate();

            // Audit Section ---------------------------
            var productPriceIndex = this.context.TBL_PRODUCT_GROUP.FirstOrDefault(x => x.PRODUCTGROUPID == data.PRODUCTGROUPID);
            var audit = new TBL_AUDIT
            {
                AUDITTYPEID = (short)AuditTypeEnum.ProductPriceIndexDeleted,
                STAFFID = user.staffId,
                BRANCHID = (short)user.BranchId,
                DETAIL = $"Deleted Product Group: '{productPriceIndex?.PRODUCTGROUPNAME}' with code '{productPriceIndex?.PRODUCTGROUPCODE}' ",
                IPADDRESS = user.userIPAddress,
                URL = user.applicationUrl,
                APPLICATIONDATE = genSetup.GetApplicationDate(),
                SYSTEMDATETIME = DateTime.Now,
                TARGETID = productGroupId
            };

            this.auditTrail.AddAuditTrail(audit);

            // end of Audit section -------------------------------
            return this.SaveAll();
        }
        #endregion Product Group

        #region Product Type
        public IQueryable<ProductTypeViewModel> AllProductType()
        {
            return (from p in context.TBL_PRODUCT_TYPE
                    where p.DELETED == false
                    select new ProductTypeViewModel()
                    {
                        productTypeId = p.PRODUCTTYPEID,
                        productTypeName = p.PRODUCTTYPENAME,
                        productGroupId = p.PRODUCTGROUPID,
                        productGroupName = p.TBL_PRODUCT_GROUP.PRODUCTGROUPNAME,
                        requirePrincipalGl = p.REQUIREPRINCIPALGL,
                        requirePrincipalGl2 = p.REQUIREPRINCIPALGL2,
                        requireInterestIncomeExpenseGl = p.REQUIREINTERESTINCOMEEXPENSEGL,
                        requireInterestReceivablePayableGl = p.REQUIRE_INT_RECEIVABL_PAYABLGL,
                        requirePremiumDiscountGl = p.REQUIREPREMIUMDISCOUNTGL,
                        requireDormantGl = p.REQUIREDORMANTGL,
                        requireOverdrawnGL = p.REQUIREOVERDRAWNGL,
                        requireRate = p.REQUIRERATE,
                        requireTenor = p.REQUIRETENOR,
                        dealClassificationId = p.DEALCLASSIFICATIONID,
                        requireScheduleType = p.REQUIRESCHEDULETYPE
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
            var isProductTypeExist = context.TBL_PRODUCT_TYPE.Any(x => x.PRODUCTTYPENAME.ToLower() == productType.productTypeName.ToLower());

            if (isProductTypeExist)
            {
                throw new Exception("Product type already exists!");
            }
            var data = new TBL_PRODUCT_TYPE()
            {
                PRODUCTTYPENAME = productType.productTypeName,
                PRODUCTGROUPID = productType.productGroupId,
                REQUIREPRINCIPALGL = productType.requirePrincipalGl,
                REQUIREPRINCIPALGL2 = productType.requirePrincipalGl2,
                REQUIREINTERESTINCOMEEXPENSEGL = productType.requireInterestIncomeExpenseGl,
                REQUIRE_INT_RECEIVABL_PAYABLGL = productType.requireInterestReceivablePayableGl,
                REQUIREPREMIUMDISCOUNTGL = productType.requirePremiumDiscountGl,
                REQUIREDORMANTGL = productType.requireDormantGl,
                REQUIREOVERDRAWNGL = productType.requireOverdrawnGL,
                REQUIRERATE = productType.requireRate,
                REQUIRETENOR = productType.requireTenor,
                DEALCLASSIFICATIONID = productType.dealClassificationId,
                REQUIRESCHEDULETYPE = productType.requireScheduleType
            };

            this.context.TBL_PRODUCT_TYPE.Add(data);

            // Audit Section ---------------------------
            var audit = new TBL_AUDIT
            {
                AUDITTYPEID = (short)AuditTypeEnum.ProductTypeAdded,
                STAFFID = productType.createdBy,
                BRANCHID = (short)productType.userBranchId,
                DETAIL = $"Added tbl_Product Type: '{productType.productTypeName}' ",
                IPADDRESS = productType.userIPAddress,
                URL = productType.applicationUrl,
                APPLICATIONDATE = genSetup.GetApplicationDate(),
                SYSTEMDATETIME = DateTime.Now
            };

            this.auditTrail.AddAuditTrail(audit);
            //end of Audit section -------------------------------

            var status = this.SaveAll();

            if (status)
                return data.PRODUCTTYPEID;
            else
                return -1;
        }

        public bool UpdateProductType(int productTypeId, ProductTypeViewModel productType)
        {
            var data = this.context.TBL_PRODUCT_TYPE.FirstOrDefault(x => x.PRODUCTTYPEID == productTypeId);

            if (data == null)
                return false;

            if (data.PRODUCTGROUPID != productType.productGroupId)
            {
                var countProductGroupUsed = this.context.TBL_PRODUCT.Count(x => x.PRODUCTTYPEID == productTypeId);
                if (countProductGroupUsed > 0)
                {
                    throw new Exception("The product group for this product type cannot be changed because the product type is already in use");
                }
            }

            data.PRODUCTTYPENAME = productType.productTypeName;
            data.PRODUCTGROUPID = productType.productGroupId;
            data.REQUIREPRINCIPALGL = productType.requirePrincipalGl2;
            data.REQUIREPRINCIPALGL2 = productType.requirePrincipalGl;
            data.REQUIREPREMIUMDISCOUNTGL = productType.requirePremiumDiscountGl;
            data.REQUIREDORMANTGL = productType.requireDormantGl;
            data.REQUIREOVERDRAWNGL = productType.requireOverdrawnGL;
            data.REQUIREINTERESTINCOMEEXPENSEGL = productType.requireInterestIncomeExpenseGl;
            data.REQUIRE_INT_RECEIVABL_PAYABLGL = productType.requireInterestReceivablePayableGl;
            data.DEALCLASSIFICATIONID = productType.dealClassificationId;
            data.REQUIRERATE = productType.requireRate;
            data.REQUIRETENOR = productType.requireTenor;
            data.REQUIRESCHEDULETYPE = productType.requireScheduleType;

            // Audit Section ---------------------------
            var audit = new TBL_AUDIT
            {
                AUDITTYPEID = (short)AuditTypeEnum.ProductTypeUpdated,
                STAFFID = productType.createdBy,
                BRANCHID = (short)productType.userBranchId,
                DETAIL = $"Updated tbl_Product Type: '{productType.productTypeName}' ",
                IPADDRESS = productType.userIPAddress,
                URL = productType.applicationUrl,
                APPLICATIONDATE = genSetup.GetApplicationDate(),
                SYSTEMDATETIME = DateTime.Now
            };

            this.auditTrail.AddAuditTrail(audit);
            //end of Audit section -------------------------------
            return this.SaveAll();
        }

        public bool DeleteProductType(int productTypeId, UserInfo user)
        {
            var data = this.context.TBL_PRODUCT_TYPE.Find(productTypeId);

            if (data == null)
                return false;

            data.DELETED = true;
            data.DATETIMEDELETED = genSetup.GetApplicationDate();

            // Audit Section ---------------------------
            var productPriceIndex = this.context.TBL_PRODUCT_TYPE.FirstOrDefault(x => x.PRODUCTTYPEID == data.PRODUCTTYPEID);
            var audit = new TBL_AUDIT
            {
                AUDITTYPEID = (short)AuditTypeEnum.ProductPriceIndexDeleted,
                STAFFID = user.staffId,
                BRANCHID = (short)user.BranchId,
                DETAIL = $"Deleted Product Type: '{data.PRODUCTTYPENAME}' under group '{data.TBL_PRODUCT_GROUP.PRODUCTGROUPNAME}' ",
                IPADDRESS = user.userIPAddress,
                URL = user.applicationUrl,
                APPLICATIONDATE = genSetup.GetApplicationDate(),
                SYSTEMDATETIME = DateTime.Now,
                TARGETID = productTypeId
            };

            this.auditTrail.AddAuditTrail(audit);

            // end of Audit section -------------------------------
            return this.SaveAll();
        }
        #endregion Product Type

        #region Product Region

        public IEnumerable<ApprovalStatusViewModel> GetApprovalStatus()
        {
            return from ap in context.TBL_APPROVAL_STATUS
                   select new ApprovalStatusViewModel
                   {
                       approvalStatusId = ap.APPROVALSTATUSID,
                       approvalStatusName = ap.APPROVALSTATUSNAME,
                       forDisplay = ap.FORDISPLAY,
                   };
        }

        private IQueryable<ProductViewModel> AllProduct()
        {
            var productData = (from data in context.TBL_PRODUCT
                               select new ProductViewModel()
                               {
                                   productId = data.PRODUCTID,
                                   companyId = data.COMPANYID,
                                   productTypeId = data.PRODUCTTYPEID,
                                   productTypeName = data.TBL_PRODUCT_TYPE.PRODUCTTYPENAME,
                                   productGroupName = data.TBL_PRODUCT_TYPE.TBL_PRODUCT_GROUP.PRODUCTGROUPNAME,
                                   productCategoryId = data.PRODUCTCATEGORYID,
                                   productCategoryName = data.TBL_PRODUCT_CATEGORY.PRODUCTCATEGORYNAME,
                                   productClassId = data.PRODUCTCLASSID,
                                   productClassName = data.TBL_PRODUCT_CLASS.PRODUCTCLASSNAME,

                                   customerId = data.TBL_PRODUCT_CLASS.CUSTOMERTYPEID,

                                   productPriceIndexId = data.PRODUCTPRICEINDEXID,
                                   productPriceIndexName = data.TBL_PRODUCT_PRICE_INDEX.PRICEINDEXNAME,
                                   productPriceIndexSpread = data.PRODUCTPRICEINDEXSPREAD,

                                   productCode = data.PRODUCTCODE,
                                   productName = data.PRODUCTNAME,
                                   productDescription = data.PRODUCTDESCRIPTION,

                                   productGroupId = data.TBL_PRODUCT_TYPE.PRODUCTGROUPID,

                                   principalBalanceGl = data.PRINCIPALBALANCEGL,
                                   principalBalanceGlCode = (data.PRINCIPALBALANCEGL.HasValue ? data.TBL_CHART_OF_ACCOUNT.ACCOUNTCODE : ""),

                                   principalBalanceGl2 = data.PRINCIPALBALANCEGL2,
                                   principalBalanceGl2Code = (data.PRINCIPALBALANCEGL2.HasValue ? data.TBL_CHART_OF_ACCOUNT.ACCOUNTCODE : ""),

                                   interestIncomeExpenseGl = data.INTERESTINCOMEEXPENSEGL,
                                   interestIncomeExpenseGlCode = (data.INTERESTINCOMEEXPENSEGL.HasValue ? data.TBL_CHART_OF_ACCOUNT.ACCOUNTCODE : ""),

                                   interestReceivablePayableGl = data.INTERESTRECEIVABLEPAYABLEGL,
                                   interestReceivablePayableGlCode = (data.INTERESTRECEIVABLEPAYABLEGL.HasValue ? data.TBL_CHART_OF_ACCOUNT.ACCOUNTCODE : ""),

                                   dormantGl = data.DORMANTGL,
                                   premiumDiscountGl = data.PREMIUMDISCOUNTGL,

                                   dealTypeId = data.DEALTYPEID,
                                   dealTypeName = data.TBL_DEAL_TYPE.DEALTYPENAME,
                                   dealClassificationId = data.DEALCLASSIFICATIONID,
                                   dealClassificationName = data.TBL_DEAL_CLASSIFICATION.CLASSIFICATION,
                                   dayCountId = data.DAYCOUNTCONVENTIONID,
                                   dayCountName = data.TBL_DAY_COUNT_CONVENTION.DAYCOUNTCONVENTIONNAME,

                                   maximumTenor = data.MAXIMUMTENOR,
                                   minimumTenor = data.MINIMUMTENOR,
                                   maximumRate = data.MAXIMUMRATE,
                                   minimumRate = data.MINIMUMRATE,
                                   minimumBalance = data.MINIMUMBALANCE,
                                   approvedBy = data.APPROVEDBY,
                                   completed = data.COMPLETED,
                                   approved = data.APPROVED,
                                 
                               

                                   dateTimeUpdated = data.DATETIMEUPDATED,
                                   deleted = data.DELETED,
                                   deletedBy = data.DELETEDBY,
                                   dateTimeDeleted = data.DATETIMEDELETED,

                                   allowCustomerAccountForceDebit = data.ALLOWCUSTOMERACCOUNTFORCEDEBIT,
                                   allowMoratorium = data.ALLOWMORATORIUM,
                                   allowScheduleTypeOverride = data.ALLOWSCHEDULETYPEOVERRIDE,
                                   allowTenor = data.ALLOWTENOR,
                                   allowRate = data.ALLOWOVERDRAWN,
                                   allowOverdrawn = data.ALLOWOVERDRAWN,

                                   cleanupPeriod = data.CLEANUPPERIOD,
                                   defaultGracePeriod = data.DEFAULTGRACEPERIOD,
                                   equityContribution = data.EQUITYCONTRIBUTION,
                                   expiryPeriod = data.EXPIRYPERIOD,
                                   scheduleTypeId = data.SCHEDULETYPEID,


                               });

            // from p in productData
            //join c in context.TBL_PRODUCT_CURRENCY on p.productId equals c.PRODUCTID
            //where p.deleted != false
            //select new ProductCurrencyViewModel
            //{
            //    productCurrencyId = c.PRODUCTCURRENCYID,
            //    currencyId = c.CURRENCYID,
            //    currencyName = c.TBL_CURRENCY.CURRENCYCODE + " -- " + c.TBL_CURRENCY.CURRENCYNAME
            //};

            
            foreach (var item in productData)
            {
                item.currencies = context.TBL_PRODUCT_CURRENCY.Where(curr => curr.PRODUCTID == item.productId && curr.DELETED != false)
                              .Select(c => new ProductCurrencyViewModel()
                              {
                                  productCurrencyId = c.PRODUCTCURRENCYID,
                                  currencyId = c.CURRENCYID,
                                  currencyName = c.TBL_CURRENCY.CURRENCYCODE + " -- " + c.TBL_CURRENCY.CURRENCYNAME
                              }).ToList();

                item.ProductBehaviour = context.TBL_PRODUCT_BEHAVIOUR.Where(d => d.PRODUCTID == item.productId).Select(d => new ProductBehaviourViewModel()
                {
                    customerLimit = d.CUSTOMER_LIMIT,
                    collateralFcyLimit = d.COLLATERAL_FCY_LIMIT ?? 0,
                    collateralLcyLimit = d.COLLATERAL_LCY_LIMIT ?? 0,
                    productLimit = d.PRODUCT_LIMIT,
                    isInvoiceBased = d.ISINVOICEBASED,
                    requireCasaAccount = (bool)d.REQUIRECASAACCOUNT,
                    allowFundUsage = d.ALLOWFUNDUSAGE != null ? (bool)d.ALLOWFUNDUSAGE : false,
                    isTemporaryOverDraft = d.ISTEMPORARYOVERDRAFT != null ? (bool)d.ISTEMPORARYOVERDRAFT : false,

                }).FirstOrDefault();

            }

            return productData.AsEnumerable().AsQueryable();
        }

        private IEnumerable<ProductSearchViewModel> ProductSearch(int companyId)
        {
            var data = context.TBL_PRODUCT.Where(p => p.COMPANYID == companyId).Select(p => new ProductSearchViewModel
            {
                dealClassificationId = p.DEALCLASSIFICATIONID,
                dealClassificationName = p.TBL_DEAL_CLASSIFICATION.CLASSIFICATION,
                equityContribution = p.EQUITYCONTRIBUTION,
                maximumRate = p.MAXIMUMRATE,
                maximumTenor = p.MAXIMUMTENOR,
                minimumRate = p.MINIMUMRATE,
                minimumTenor = p.MINIMUMTENOR,
                productCategoryId = p.PRODUCTCATEGORYID,
                productCategoryName = p.TBL_PRODUCT_CATEGORY.PRODUCTCATEGORYNAME,
                productClassId = p.PRODUCTCLASSID,
                productClassName = p.TBL_PRODUCT_CLASS.PRODUCTCLASSNAME,
                productCode = p.PRODUCTCODE,
                productId = p.PRODUCTID,
                productName = p.PRODUCTNAME,
                productGroupId = p.TBL_PRODUCT_TYPE.PRODUCTGROUPID,
                ProductBehaviour = p.TBL_PRODUCT_BEHAVIOUR.Where(c => c.PRODUCTID == p.PRODUCTID).Select(c => new ProductBehaviourViewModel
                {
                    collateralFcyLimit = (double)c.COLLATERAL_FCY_LIMIT,
                    collateralLcyLimit = (double)c.COLLATERAL_LCY_LIMIT,
                    customerLimit = c.CUSTOMER_LIMIT,
                    fcyLimit = c.COLLATERAL_FCY_LIMIT,
                    isInvoiceBased = c.ISINVOICEBASED,
                    lcyLimit = c.COLLATERAL_LCY_LIMIT,
                    requireCasaAccount = (bool)c.REQUIRECASAACCOUNT,
                    productLimit = c.PRODUCT_LIMIT,
                    allowFundUsage = c.ALLOWFUNDUSAGE != null ? (bool)c.ALLOWFUNDUSAGE : false,
                    isTemporaryOverDraft = c.ISTEMPORARYOVERDRAFT != null ? (bool)c.ISTEMPORARYOVERDRAFT : false,
                }).FirstOrDefault(),
            });
            return null;
        }

        public IEnumerable<ProductViewModel> GetAllProduct()
        {
            return AllProduct();
        }

        public IEnumerable<ProductSearchViewModel> GetAllLoanProduct(int companyId)
        {
            return ProductSearch(companyId).Where(c => c.productGroupId == (int)ProductGroupEnum.LoansAndAdvances);
        }

        public IEnumerable<ProductViewModel> GetAllProductByProductClass(int productClassId)
        {
            return AllProduct().Where(c => c.productClassId == productClassId && (c.productGroupId == 1));
        }

        public ProductViewModel GetProductById(int productId)
        {
            return AllProduct().FirstOrDefault(p => p.productId == productId);
        }

        public IEnumerable<ProductViewModel> GetProductByProductGroup(int companyId)
        {
            return AllProduct().Where(p => p.companyId == companyId && p.productGroupId == 1).ToList();
        }

        public IEnumerable<ProductViewModel> GetProductByGroupAndCategory(short productGroupId, short productCategoryId)
        {
            return AllProduct().Where(p => p.productGroupId == productGroupId && p.productCategoryId == productCategoryId);
        }

        public IEnumerable<ProductViewModel> GetProductByTypeAndCategory(short productTypeId, short productCategoryId)
        {
            var data = AllProduct().Where(p => p.productTypeId == productTypeId && p.productCategoryId == productCategoryId).ToList();
            return data;
        }

        public IEnumerable<ProductViewModel> GetProductAwaitingApprovals(int staffId, int companyId)
        {
            //var levelResult = level.GetAllApprovalLevelStaffByStaffId(staffId, companyId, (int)OperationsEnum.ProductCreation);
            //int staffApprovalLevelId = 0;

            var ids = genSetup.GetStaffApprovalLevelIds(staffId, (int)OperationsEnum.ProductCreation).ToList();

            //if (levelResult != null) staffApprovalLevelId = levelResult.approvalLevelId;

            try
            {
                var pendingProductsChanges = (from c in context.TBL_TEMP_PRODUCT
                                              //join r in context.TBL_TEMP_PRODUCT_CURRENCY on c.PRODUCTID equals r.PRODUCTID
                                              join coy in context.TBL_COMPANY on c.COMPANYID equals coy.COMPANYID
                                              join atrail in context.TBL_APPROVAL_TRAIL on c.PRODUCTID equals atrail.TARGETID
                                              where atrail.APPROVALSTATUSID == (int)ApprovalStatusEnum.Pending && c.ISCURRENT == true
                                                    && atrail.RESPONSESTAFFID == null
                                                    && atrail.OPERATIONID == (int)OperationsEnum.ProductCreation
                                                                                  //&& atrail.TOAPPROVALLEVELID == staffApprovalLevelId
                                                                                  && ids.Contains((int)atrail.TOAPPROVALLEVELID)

                                              select new ProductViewModel()
                                              {
                                                  productId = c.PRODUCTID,
                                                  companyId = c.COMPANYID,
                                                  productTypeId = c.PRODUCTTYPEID,
                                                  productTypeName = c.TBL_PRODUCT_TYPE.PRODUCTTYPENAME,
                                                  productCategoryId = c.PRODUCTCATEGORYID,
                                                  productCategoryName = c.TBL_PRODUCT_CATEGORY.PRODUCTCATEGORYNAME,
                                                  productClassId = c.PRODUCTCLASSID,
                                                  productClassName = c.TBL_PRODUCT_CLASS.PRODUCTCLASSNAME,

                                                  productPriceIndexId = c.PRODUCTPRICEINDEXID,
                                                  productPriceIndexName = c.TBL_PRODUCT_PRICE_INDEX.PRICEINDEXNAME,
                                                  productPriceIndexSpread = c.PRODUCTPRICEINDEXSPREAD,

                                                  productCode = c.PRODUCTCODE,
                                                  productName = c.PRODUCTNAME,
                                                  productDescription = c.PRODUCTDESCRIPTION,

                                                  productGroupId = c.TBL_PRODUCT_TYPE.PRODUCTGROUPID,
                                                  productGroupName = c.TBL_PRODUCT_TYPE.TBL_PRODUCT_GROUP.PRODUCTGROUPNAME,

                                                  principalBalanceGl = c.PRINCIPALBALANCEGL,
                                                  principalBalanceGlCode = (c.PRINCIPALBALANCEGL.HasValue ? c.TBL_CHART_OF_ACCOUNT.ACCOUNTCODE : ""),

                                                  principalBalanceGl2 = c.PRINCIPALBALANCEGL2,
                                                  principalBalanceGl2Code = (c.PRINCIPALBALANCEGL2.HasValue ? c.TBL_CHART_OF_ACCOUNT.ACCOUNTCODE : ""),


                                                  interestIncomeExpenseGl = c.INTERESTINCOMEEXPENSEGL,
                                                  interestIncomeExpenseGlCode = (c.INTERESTINCOMEEXPENSEGL.HasValue ? c.TBL_CHART_OF_ACCOUNT.ACCOUNTCODE : ""),

                                                  interestReceivablePayableGl = c.INTERESTRECEIVABLEPAYABLEGL,
                                                  interestReceivablePayableGlCode = (c.INTERESTRECEIVABLEPAYABLEGL.HasValue ? c.TBL_CHART_OF_ACCOUNT.ACCOUNTCODE : ""),

                                                  dormantGl = c.DORMANTGL,
                                                  dormantGlCode = (c.DORMANTGL.HasValue ? c.TBL_CHART_OF_ACCOUNT.ACCOUNTCODE : ""),
                                                  premiumDiscountGl = c.PREMIUMDISCOUNTGL,
                                                  premiumDiscountGlCode = (c.PREMIUMDISCOUNTGL.HasValue ? c.TBL_CHART_OF_ACCOUNT.ACCOUNTCODE : ""),

                                                  dealTypeId = c.DEALTYPEID,
                                                  dealTypeName = c.TBL_DEAL_TYPE.DEALTYPENAME,
                                                  dealClassificationId = c.DEALCLASSIFICATIONID,
                                                  dealClassificationName = c.TBL_DEAL_CLASSIFICATION.CLASSIFICATION,
                                                  dayCountId = c.DAYCOUNTCONVENTIONID,
                                                  dayCountName = c.TBL_DAY_COUNT_CONVENTION.DAYCOUNTCONVENTIONNAME,

                                                  maximumTenor = c.MAXIMUMTENOR,
                                                  minimumTenor = c.MINIMUMTENOR,
                                                  maximumRate = c.MAXIMUMRATE,
                                                  minimumRate = c.MINIMUMRATE,
                                                  minimumBalance = c.MINIMUMBALANCE,
                                                  approvedBy = c.APPROVEDBY,
                                                  completed = c.COMPLETED,
                                                  approved = c.APPROVED,

                                                  //approvalStatusId = c.APPROVALSTATUSID,
                                                  operationId = atrail.OPERATIONID,
                                                  //currencies = context.TBL_TEMP_PRODUCT_CURRENCY.Where(curr => curr.PRODUCTID == c.PRODUCTID && curr.DELETED == false).Any() ? context.TBL_TEMP_PRODUCT_CURRENCY.Where(curr => curr.PRODUCTID == c.PRODUCTID && curr.DELETED == false).Select(pc => new ProductCurrencyViewModel()
                                                  //{
                                                  //    productId = c.PRODUCTID,
                                                  //    productCurrencyId = pc.PRODUCTCURRENCYID,
                                                  //    currencyId = pc.CURRENCYID,
                                                  //    currencyName = pc.TBL_CURRENCY.CURRENCYCODE + " -- " + pc.TBL_CURRENCY.CURRENCYNAME
                                                  //}).ToList() : null,
                                                  //fees = context.TBL_TEMP_PRODUCT_CHARGE_FEE.Where(curr => curr.PRODUCTID == c.PRODUCTID && c.DELETED == false).Select(pf => new ProductFeeViewModel()
                                                  //{
                                                  //    productFeeId = pf.PRODUCTFEEID,
                                                  //    productId = pf.PRODUCTID,
                                                  //    feeId = pf.CHARGEFEEID,
                                                  //    feeName = pf.TBL_CHARGE_FEE.CHARGEFEENAME,
                                                  //    feeIntervalName = pf.TBL_CHARGE_FEE.TBL_FEE_INTERVAL.FEEINTERVALNAME,
                                                  //    feeTargetName = pf.TBL_CHARGE_FEE.TBL_FEE_TARGET.FEETARGETNAME,
                                                  //    feeTypeName = pf.TBL_CHARGE_FEE.TBL_FEE_TYPE.FEETYPENAME,
                                                  //    //    glAccountCode = pf.TBL_CHARGE_FEE.TBL_CHART_OF_ACCOUNT.ACCOUNTCODE,
                                                  //    //   glAccountName = pf.TBL_CHARGE_FEE.TBL_CHART_OF_ACCOUNT.ACCOUNTNAME,
                                                  //    companyId = pf.COMPANYID,

                                                  //    rateValue = pf.RATEVALUE,
                                                  //    dependentAmount = pf.DEPENDENTAMOUNT,

                                                  //    createdBy = pf.CREATEDBY,
                                                  //    dateTimeCreated = pf.DATETIMECREATED,

                                                  //}).ToList(),
                                                  collaterals = context.TBL_TEMP_PRODUCT_COLLATERALTYP.Where(coll => coll.PRODUCTID == c.PRODUCTID && coll.DELETED == false).Select(prodColl => new ProductCollateralTypeViewModel()
                                                  {
                                                      productId = prodColl.PRODUCTID,
                                                      productCollateralId = prodColl.PRODUCTCOLLATERALTYPEID,
                                                      collateralTypeName = prodColl.TBL_COLLATERAL_TYPE.COLLATERALTYPENAME
                                                  }).ToList(),
                                                  //ProductBehaviour = context.TBL_PRODUCT_BEHAVIOUR.Where(d => d.PRODUCTID == c.PRODUCTID).Select(d => new ProductBehaviourViewModel()
                                                  //{
                                                  //    customerLimit = d.CUSTOMER_LIMIT,
                                                  //    collateralFcyLimit = d.COLLATERAL_FCY_LIMIT ?? 0,
                                                  //    collateralLcyLimit = d.COLLATERAL_LCY_LIMIT ?? 0,
                                                  //    productLimit = d.PRODUCT_LIMIT,
                                                  //    allowFundUsage = (bool)d.ALLOWFUNDUSAGE,
                                                  //    isInvoiceBased = d.ISINVOICEBASED,
                                                  //    isTemporaryOverDraft = d.ISTEMPORARYOVERDRAFT != null ? (bool)d.ISTEMPORARYOVERDRAFT : false,
                                                  //    requireCasaAccount = (bool)d.REQUIRECASAACCOUNT,


                                                  //}).FirstOrDefault(),
                                                  dateTimeUpdated = c.DATETIMEUPDATED,
                                                  deleted = c.DELETED,
                                                  deletedBy = c.DELETEDBY,
                                                  dateTimeDeleted = c.DATETIMEDELETED,

                                                  allowCustomerAccountForceDebit = c.ALLOWCUSTOMERACCOUNTFORCEDEBIT,
                                                  allowMoratorium = c.ALLOWMORATORIUM,
                                                  allowScheduleTypeOverride = c.ALLOWSCHEDULETYPEOVERRIDE,
                                                  allowTenor = c.ALLOWTENOR,
                                                  allowRate = c.ALLOWRATE,
                                                  allowOverdrawn = c.ALLOWOVERDRAWN,

                                                  cleanupPeriod = c.CLEANUPPERIOD ?? 0,
                                                  defaultGracePeriod = c.DEFAULTGRACEPERIOD ?? 0,
                                                  equityContribution = c.EQUITYCONTRIBUTION ?? 0,
                                                  expiryPeriod = c.EXPIRYPERIOD ?? 0,
                                                  scheduleTypeId = c.SCHEDULETYPEID,
                                                  //productBehaviourId = c.PRODUCT_BEHAVIOURID,
                                                  //productBehaviourName = c.TBL_PRODUCT_BEHAVIOUR.PRODUCT_BEHAVIOUR_NAME
                                              }).GroupBy(x => x.productId).Select(g => g.FirstOrDefault());

                foreach(var item in pendingProductsChanges)
                {
                    var behaviour = context.TBL_PRODUCT_BEHAVIOUR.Where(x => x.PRODUCTID == item.productId);
                    if (behaviour.Any())
                    {
                        var x = behaviour.FirstOrDefault();
                        item.collateralLCYLimit = x.COLLATERAL_LCY_LIMIT;
                        item.collateralFCYLimit = x.COLLATERAL_FCY_LIMIT;
                        item.customerLimit = x.CUSTOMER_LIMIT;
                        item.productLimit = x.PRODUCT_LIMIT;
                        item.invoiceBased = x.ISINVOICEBASED;
                        item.requireCasaAccount = x.REQUIRECASAACCOUNT;
                        item.allowFundUsage = x.ALLOWFUNDUSAGE;
                       
                    }
                }
                foreach(var productData in pendingProductsChanges)
                {
                    var currencies = context.TBL_TEMP_PRODUCT_CURRENCY.Where(curr => curr.PRODUCTID == productData.productId && curr.DELETED != false).Select(pc => new ProductCurrencyViewModel()
                    {
                        productId = pc.PRODUCTID,
                        productCurrencyId = pc.PRODUCTCURRENCYID,
                        currencyId = pc.CURRENCYID,
                        currencyName = pc.TBL_CURRENCY.CURRENCYCODE + " -- " + pc.TBL_CURRENCY.CURRENCYNAME
                    }).ToList();

                    var fees = context.TBL_TEMP_PRODUCT_CHARGE_FEE.Where(curr => curr.PRODUCTID == productData.productId && curr.DELETED != false).Select(pf => new ProductFeeViewModel()
                    {
                        productId = pf.PRODUCTID,
                        productFeeId = pf.PRODUCTFEEID,
                        feeId = pf.CHARGEFEEID,
                        rateValue = pf.RATEVALUE,
                        dependentAmount = pf.DEPENDENTAMOUNT,
                        feeName = pf.TBL_CHARGE_FEE.CHARGEFEENAME,
                        feeIntervalName = pf.TBL_CHARGE_FEE.TBL_FEE_INTERVAL.FEEINTERVALNAME,
                        feeTargetName = pf.TBL_CHARGE_FEE.TBL_FEE_TARGET.FEETARGETNAME,
                        feeTypeName = pf.TBL_CHARGE_FEE.TBL_FEE_TYPE.FEETYPENAME,
                        //glAccountCode = pf.TBL_CHARGE_FEE..AccountCode,
                        // glAccountName = pf.tbl_Fee.tbl_Chart_Of_Account.AccountName

                    }).ToList();
                    if (currencies != null) productData.currencies = currencies;
                    if (fees != null) productData.fees = fees;
                }
               
                var b = pendingProductsChanges.ToList();

                return pendingProductsChanges;

            }
            catch (Exception ex)
            {
                throw new Exception("" + ex);
            }


        }

        public ProductViewModel GetTempProductDetail(int productId)
        {
            //return GetTempStaffDetails().Where(x => x.StaffId == staffId).Single();

            var productData = (from tp in context.TBL_TEMP_PRODUCT
                    join coy in context.TBL_COMPANY on tp.COMPANYID equals coy.COMPANYID
                    where tp.PRODUCTID == productId
                    select new ProductViewModel()
                    {
                        productId = tp.PRODUCTID,
                        companyId = tp.COMPANYID,
                        productTypeId = tp.PRODUCTTYPEID,
                        productTypeName = tp.TBL_PRODUCT_TYPE.PRODUCTTYPENAME,
                        productGroupName = tp.TBL_PRODUCT_TYPE.TBL_PRODUCT_GROUP.PRODUCTGROUPNAME,
                        productCategoryId = tp.PRODUCTCATEGORYID,
                        productCategoryName = tp.TBL_PRODUCT_CATEGORY.PRODUCTCATEGORYNAME,
                        productClassId = tp.PRODUCTCLASSID,
                        productClassName = tp.TBL_PRODUCT_CLASS.PRODUCTCLASSNAME,

                        productPriceIndexId = tp.PRODUCTPRICEINDEXID,
                        productPriceIndexName = tp.TBL_PRODUCT_PRICE_INDEX.PRICEINDEXNAME,
                        productPriceIndexSpread = tp.PRODUCTPRICEINDEXSPREAD,

                        productCode = tp.PRODUCTCODE,
                        productName = tp.PRODUCTNAME,
                        productDescription = tp.PRODUCTDESCRIPTION,

                        productGroupId = tp.TBL_PRODUCT_TYPE.PRODUCTGROUPID,

                        principalBalanceGl = tp.PRINCIPALBALANCEGL,
                        principalBalanceGlCode = (tp.PRINCIPALBALANCEGL.HasValue ? tp.TBL_CHART_OF_ACCOUNT.ACCOUNTCODE : ""),

                        principalBalanceGl2 = tp.PRINCIPALBALANCEGL2,
                        principalBalanceGl2Code = (tp.PRINCIPALBALANCEGL2.HasValue ? context.TBL_CHART_OF_ACCOUNT.Find(tp.PRINCIPALBALANCEGL2).ACCOUNTCODE : ""),


                        interestIncomeExpenseGl = tp.INTERESTINCOMEEXPENSEGL,
                        interestIncomeExpenseGlCode = (tp.INTERESTINCOMEEXPENSEGL.HasValue ? tp.TBL_CHART_OF_ACCOUNT.ACCOUNTCODE : ""),

                        interestReceivablePayableGl = tp.INTERESTRECEIVABLEPAYABLEGL,
                        interestReceivablePayableGlCode = (tp.INTERESTRECEIVABLEPAYABLEGL.HasValue ? tp.TBL_CHART_OF_ACCOUNT.ACCOUNTCODE : ""),

                        dormantGl = tp.DORMANTGL,
                        premiumDiscountGl = tp.PREMIUMDISCOUNTGL,

                        dealTypeId = tp.DEALTYPEID,
                        dealClassificationId = tp.DEALCLASSIFICATIONID,
                        dayCountId = tp.DAYCOUNTCONVENTIONID,

                        maximumTenor = tp.MAXIMUMTENOR,
                        minimumTenor = tp.MINIMUMTENOR,
                        maximumRate = tp.MAXIMUMRATE,
                        minimumRate = tp.MINIMUMRATE,
                        minimumBalance = tp.MINIMUMBALANCE,
                        approvedBy = tp.APPROVEDBY,
                        completed = tp.COMPLETED,
                        approved = tp.APPROVED,
                        approvalStatusId = tp.APPROVALSTATUSID,

                        collaterals = context.TBL_TEMP_PRODUCT_COLLATERALTYP.Where(curr => curr.PRODUCTID == tp.PRODUCTID && curr.DELETED != false).Select(pcc => new ProductCollateralTypeViewModel()
                        {
                            productId = pcc.PRODUCTID,
                            productCollateralId = pcc.PRODUCTCOLLATERALTYPEID,
                            collateralTypeId = pcc.COLLATERALTYPEID,
                            collateralTypeName = pcc.TBL_COLLATERAL_TYPE.COLLATERALTYPENAME
                        }).ToList(),
                        ProductBehaviour = context.TBL_TEMP_PRODUCT_BEHAVIOUR.Where(d => d.PRODUCTCODE == tp.PRODUCTCODE).Select(d => new ProductBehaviourViewModel()
                        {
                            customerLimit = d.CUSTOMER_LIMIT,
                            collateralFcyLimit = d.COLLATERAL_FCY_LIMIT ?? 0,
                            collateralLcyLimit = d.COLLATERAL_LCY_LIMIT ?? 0,
                            productLimit = d.PRODUCT_LIMIT,
                            isInvoiceBased = d.ISINVOICEBASED,
                            allowFundUsage = d.ALLOWFUNDUSAGE != null ? (bool)d.ALLOWFUNDUSAGE : false


                        }).FirstOrDefault(),

                        dateTimeUpdated = tp.DATETIMEUPDATED,
                        deleted = tp.DELETED,
                        deletedBy = tp.DELETEDBY,
                        dateTimeDeleted = tp.DATETIMEDELETED
                    }).FirstOrDefault();

            
                var currencies = context.TBL_TEMP_PRODUCT_CURRENCY.Where(curr => curr.PRODUCTID == productData.productId && curr.DELETED != false).Select(pc => new ProductCurrencyViewModel()
                {
                    productId = pc.PRODUCTID,
                    productCurrencyId = pc.PRODUCTCURRENCYID,
                    currencyId = pc.CURRENCYID,
                    currencyName = pc.TBL_CURRENCY.CURRENCYCODE + " -- " + pc.TBL_CURRENCY.CURRENCYNAME
                }).ToList();

                var fees = context.TBL_TEMP_PRODUCT_CHARGE_FEE.Where(curr => curr.PRODUCTID == productData.productId && curr.DELETED != false).Select(pf => new ProductFeeViewModel()
                {
                    productId = pf.PRODUCTID,
                    productFeeId = pf.PRODUCTFEEID,
                    feeId = pf.CHARGEFEEID,
                    rateValue = pf.RATEVALUE,
                    dependentAmount = pf.DEPENDENTAMOUNT,
                    feeName = pf.TBL_CHARGE_FEE.CHARGEFEENAME,
                    feeIntervalName = pf.TBL_CHARGE_FEE.TBL_FEE_INTERVAL.FEEINTERVALNAME,
                    feeTargetName = pf.TBL_CHARGE_FEE.TBL_FEE_TARGET.FEETARGETNAME,
                    feeTypeName = pf.TBL_CHARGE_FEE.TBL_FEE_TYPE.FEETYPENAME,
                    //glAccountCode = pf.TBL_CHARGE_FEE..AccountCode,
                   // glAccountName = pf.tbl_Fee.tbl_Chart_Of_Account.AccountName

                }).ToList();
            if(currencies != null)productData.currencies = currencies;
            if (fees != null) productData.fees = fees;

            return productData;
        }

        public ProductViewModel GetProductDetail(string productCode, int companyId)
        {
            return AllProduct().SingleOrDefault(p => p.productCode == productCode && p.companyId == companyId);
        }

        public int GoForApproval(ApprovalViewModel entity)
        {
            entity.operationId = (int)OperationsEnum.ProductCreation;

            entity.externalInitialization = false;

            using (var trans = context.Database.BeginTransaction())
            {
                try
                {
                    workFlow.LogForApproval(entity);
                    var b = workFlow.NextLevelId ?? 0;

                    //workFlow.StaffId = entity.createdBy;
                    //workFlow.CompanyId = entity.companyId;
                    //workFlow.StatusId = ((int)entity.approvalStatusId == (int)ApprovalStatusEnum.Approved) ? (int)ApprovalStatusEnum.Processing : (int)entity.approvalStatusId;
                    //workFlow.TargetId = entity.targetId;
                    //workFlow.Comment = entity.comment;
                    //workFlow.OperationId = entity.operationId;
                    //workFlow.DeferredExecution = true;
                    //workFlow.ExternalInitialization = false;

                    //workFlow.LogActivity();

                    //context.SaveChanges();
                    if (b == 0 && workFlow.NewState != (int)ApprovalState.Ended) // check if this is the last level
                    {
                        trans.Rollback();
                        throw new Exception("Approval Failed");
                    }

                    if(entity.approvalStatusId == (short)ApprovalStatusEnum.Disapproved)
                    {
                        var product = context.TBL_TEMP_PRODUCT.Find(entity.targetId);
                        product.APPROVALSTATUSID = (short)ApprovalStatusEnum.Disapproved;
                        context.SaveChanges();
                        trans.Commit();
                        return 2;
                    }

                    if (workFlow.NewState == (int)ApprovalState.Ended)
                    {
                        var response = ApproveProduct(entity.targetId, (short)workFlow.StatusId, entity);

                        if (response)
                        {
                            trans.Commit();
                        }
                        return 1;
                    }
                    else
                    {
                        trans.Commit();
                    }

                    return 0;
                }
                catch (Exception ex)
                {
                    trans.Rollback();
                    throw new Exception(ex.Message);
                }
            }
        }

        private bool ApproveProduct(int productId, short approvalStatusId, UserInfo user)
        {
            var productModel = context.TBL_TEMP_PRODUCT.Find(productId);
            var productBehaviourModel = context.TBL_PRODUCT_BEHAVIOUR.Where(x => x.PRODUCTID == productModel.PRODUCTID).FirstOrDefault();

            var productToUpdate = context.TBL_PRODUCT.FirstOrDefault(x => x.PRODUCTCODE == productModel.PRODUCTCODE);
            var productBehaviourToUpdate = context.TBL_PRODUCT_BEHAVIOUR.FirstOrDefault(x => x.PRODUCTID == productModel.PRODUCTID);

            var currModel = context.TBL_TEMP_PRODUCT_CURRENCY.Where(c => c.PRODUCTID == productModel.PRODUCTID && c.DELETED == false);
            var currListToUpdate = new List<TBL_PRODUCT_CURRENCY>();

            var feeModel =
                context.TBL_TEMP_PRODUCT_CHARGE_FEE.Where(c => c.PRODUCTID == productModel.PRODUCTID && c.DELETED == false);
            var feeListToUpdate = new List<TBL_PRODUCT_CHARGE_FEE>();

            var collateralModel =
                context.TBL_TEMP_PRODUCT_COLLATERALTYP.Where(c =>
                    c.PRODUCTID == productModel.PRODUCTID && c.DELETED == false);
            var collateralListToUpdate = new List<TBL_PRODUCT_COLLATERALTYPE>();

            List<TBL_PRODUCT_CHARGE_FEE> productFees = new List<TBL_PRODUCT_CHARGE_FEE>();
            List<TBL_PRODUCT_COLLATERALTYPE> productCollateral = new List<TBL_PRODUCT_COLLATERALTYPE>();
            List<TBL_PRODUCT_CURRENCY> productCurrencies = new List<TBL_PRODUCT_CURRENCY>();

            if (productToUpdate != null) //Update existing product with tempProduct record
            {
                currListToUpdate = context.TBL_PRODUCT_CURRENCY.Where(x => x.PRODUCTID == productToUpdate.PRODUCTID && x.DELETED == false).ToList();

                feeListToUpdate =
               context.TBL_PRODUCT_CHARGE_FEE.Where(x => x.PRODUCTID == productToUpdate.PRODUCTID && x.DELETED == false).ToList();

                collateralListToUpdate =
                context.TBL_PRODUCT_COLLATERALTYPE.Where(x =>
                    x.PRODUCTID == productToUpdate.PRODUCTID && x.DELETED == false).ToList();

                // remove exisiting records for currencies
                foreach (var curr in currListToUpdate)
                {
                    context.TBL_PRODUCT_CURRENCY.Remove(curr);
                }

                foreach (var item in feeListToUpdate)
                {
                    context.TBL_PRODUCT_CHARGE_FEE.Remove(item);
                }

                foreach (var item in collateralListToUpdate)
                {
                    context.TBL_PRODUCT_COLLATERALTYPE.Remove(item);
                }

                // Insert updated records for currencies
                foreach (var c in currModel)
                {
                    var curr = new TBL_PRODUCT_CURRENCY()
                    {
                        //ProductId = c.ProductId,
                        CURRENCYID = c.CURRENCYID,
                        CREATEDBY = c.CREATEDBY,
                        DATETIMECREATED = genSetup.GetApplicationDate(),
                    };
                    productCurrencies.Add(curr);
                }

                foreach (var item in feeModel)
                {
                    var feeList = new TBL_PRODUCT_CHARGE_FEE()
                    {
                        //ProductId = item.productId,
                        //ProductFeeId = item.ProductFeeId,
                        CHARGEFEEID = item.CHARGEFEEID,
                        DEPENDENTAMOUNT = item.DEPENDENTAMOUNT,
                        RATEVALUE = item.RATEVALUE,
                        COMPANYID = (int)item.COMPANYID,
                        CREATEDBY = (int)item.CREATEDBY,
                        DATETIMECREATED = genSetup.GetApplicationDate(),
                        DELETED = false
                    };
                    productFees.Add(feeList);
                }

                foreach (var item in collateralModel)
                {
                    var productCollaterals = new TBL_PRODUCT_COLLATERALTYPE()
                    {
                        //ProductId = item.productId,
                        COLLATERALTYPEID = item.COLLATERALTYPEID,
                        COMPANYID = item.COMPANYID,
                        CREATEDBY = item.CREATEDBY,
                        DATETIMECREATED = genSetup.GetApplicationDate()
                    };
                    productCollateral.Add(productCollaterals);
                }

                var existingProduct = productToUpdate;
                if (productModel != null)
                {
                    existingProduct.PRODUCTCLASSID = productModel.PRODUCTCLASSID;
                    existingProduct.PRODUCTCODE = productModel.PRODUCTCODE;
                    existingProduct.PRODUCTNAME = productModel.PRODUCTNAME;
                    existingProduct.PRODUCTDESCRIPTION = productModel.PRODUCTDESCRIPTION;

                    existingProduct.PRINCIPALBALANCEGL = productModel.PRINCIPALBALANCEGL;
                    existingProduct.PRINCIPALBALANCEGL2 = productModel.PRINCIPALBALANCEGL2;
                    existingProduct.INTERESTINCOMEEXPENSEGL = productModel.INTERESTINCOMEEXPENSEGL;
                    existingProduct.INTERESTRECEIVABLEPAYABLEGL = productModel.INTERESTRECEIVABLEPAYABLEGL;
                    existingProduct.DORMANTGL = productModel.DORMANTGL;
                    existingProduct.PREMIUMDISCOUNTGL = productModel.PREMIUMDISCOUNTGL;
                    existingProduct.OVERDRAWNGL = productModel.OVERDRAWNGL;

                    existingProduct.PRODUCTPRICEINDEXID = productModel.PRODUCTPRICEINDEXID;
                    existingProduct.PRODUCTPRICEINDEXSPREAD = productModel.PRODUCTPRICEINDEXSPREAD;

                    existingProduct.DEALTYPEID = productModel.DEALTYPEID;
                    existingProduct.DEALCLASSIFICATIONID = productModel.DEALCLASSIFICATIONID;
                    existingProduct.DAYCOUNTCONVENTIONID = productModel.DAYCOUNTCONVENTIONID;

                    existingProduct.MAXIMUMTENOR = productModel.MAXIMUMTENOR;
                    existingProduct.MINIMUMTENOR = productModel.MINIMUMTENOR;
                    existingProduct.MAXIMUMRATE = productModel.MAXIMUMRATE;
                    existingProduct.MINIMUMRATE = productModel.MINIMUMRATE;
                    existingProduct.MINIMUMBALANCE = productModel.MINIMUMBALANCE;

                    existingProduct.ALLOWRATE = productModel.ALLOWRATE;
                    existingProduct.ALLOWTENOR = productModel.ALLOWTENOR;
                    existingProduct.ALLOWOVERDRAWN = productModel.ALLOWOVERDRAWN;
                    existingProduct.ALLOWCUSTOMERACCOUNTFORCEDEBIT = productModel.ALLOWCUSTOMERACCOUNTFORCEDEBIT;
                    existingProduct.ALLOWMORATORIUM = productModel.ALLOWMORATORIUM;
                    existingProduct.ALLOWSCHEDULETYPEOVERRIDE = productModel.ALLOWSCHEDULETYPEOVERRIDE;

                    existingProduct.CLEANUPPERIOD = productModel.CLEANUPPERIOD;
                    existingProduct.DEFAULTGRACEPERIOD = productModel.DEFAULTGRACEPERIOD;
                    existingProduct.EQUITYCONTRIBUTION = productModel.EQUITYCONTRIBUTION;
                    existingProduct.EXPIRYPERIOD = productModel.EXPIRYPERIOD;
                    existingProduct.ISMULTIPLECURENCY = productModel.ISMULTIPLECURENCY;
                    existingProduct.SCHEDULETYPEID = productModel.SCHEDULETYPEID;
                    //existingProduct.PRODUCT_BEHAVIOURID = productModel.PRODUCT_BEHAVIOURID;

                    existingProduct.TBL_PRODUCT_CURRENCY = productCurrencies;
                    existingProduct.TBL_PRODUCT_CHARGE_FEE = productFees;
                    existingProduct.TBL_PRODUCT_COLLATERALTYPE = productCollateral;
                    existingProduct.APPROVED = true;
                    existingProduct.DELETED = false;
                    existingProduct.APPROVEDBY = productModel.CREATEDBY;

                    var existingProductBehaviour = productBehaviourToUpdate;
                    if (productBehaviourModel != null && productBehaviourToUpdate != null)
                    {
                        existingProductBehaviour.PRODUCTID = productModel.PRODUCTID;
                        existingProductBehaviour.PRODUCT_LIMIT = productBehaviourModel.PRODUCT_LIMIT;
                        existingProductBehaviour.CUSTOMER_LIMIT = productBehaviourModel.CUSTOMER_LIMIT;
                        existingProductBehaviour.COLLATERAL_FCY_LIMIT = productBehaviourModel.COLLATERAL_FCY_LIMIT;
                        existingProductBehaviour.COLLATERAL_LCY_LIMIT = productBehaviourModel.COLLATERAL_LCY_LIMIT;
                        existingProductBehaviour.ISINVOICEBASED = productBehaviourModel.ISINVOICEBASED;
                        existingProductBehaviour.ALLOWFUNDUSAGE = productBehaviourModel.ALLOWFUNDUSAGE;
                    }
                }
                else //Insert a new product record into the real product table
                {
                    foreach (var c in currModel)
                    {
                        var curr = new TBL_PRODUCT_CURRENCY()
                        {
                            //ProductId = c.ProductId,
                            CURRENCYID = c.CURRENCYID,
                            DATETIMECREATED = genSetup.GetApplicationDate(),
                            DELETED = false
                        };
                        productCurrencies.Add(curr);
                    }

                    foreach (var item in feeModel)
                    {
                        var feeList = new TBL_PRODUCT_CHARGE_FEE()
                        {
                            //ProductId = item.productId,
                            //ProductFeeId = item.ProductFeeId,
                            CHARGEFEEID = item.CHARGEFEEID,
                            DEPENDENTAMOUNT = item.DEPENDENTAMOUNT,
                            RATEVALUE = item.RATEVALUE,
                            COMPANYID = (int)item.COMPANYID,
                            CREATEDBY = (int)item.CREATEDBY,
                            DATETIMECREATED = genSetup.GetApplicationDate(),
                            DELETED = false
                        };
                        productFees.Add(feeList);
                    }

                    foreach (var item in collateralModel)
                    {
                        var productCollaterals = new TBL_PRODUCT_COLLATERALTYPE()
                        {
                            //ProductId = item.productId,
                            COLLATERALTYPEID = item.COLLATERALTYPEID,
                            COMPANYID = item.COMPANYID,
                            CREATEDBY = item.CREATEDBY,
                            DATETIMECREATED = genSetup.GetApplicationDate(),
                            DELETED = false
                        };
                        productCollateral.Add(productCollaterals);
                    }

                    if (productModel != null)
                    {
                        var product = new TBL_PRODUCT()
                        {
                            COMPANYID = productModel.COMPANYID,
                            PRODUCTTYPEID = productModel.PRODUCTTYPEID,
                            PRODUCTCATEGORYID = productModel.PRODUCTCATEGORYID,
                            PRODUCTCLASSID = productModel.PRODUCTCLASSID,
                            PRODUCTCODE = productModel.PRODUCTCODE,
                            PRODUCTNAME = productModel.PRODUCTNAME,
                            PRODUCTDESCRIPTION = productModel.PRODUCTDESCRIPTION,

                            PRINCIPALBALANCEGL = productModel.PRINCIPALBALANCEGL,
                            PRINCIPALBALANCEGL2 = productModel.PRINCIPALBALANCEGL2,
                            INTERESTINCOMEEXPENSEGL = productModel.INTERESTINCOMEEXPENSEGL,
                            INTERESTRECEIVABLEPAYABLEGL = productModel.INTERESTRECEIVABLEPAYABLEGL,
                            DORMANTGL = productModel.DORMANTGL,
                            PREMIUMDISCOUNTGL = productModel.PREMIUMDISCOUNTGL,
                            OVERDRAWNGL = productModel.OVERDRAWNGL,

                            PRODUCTPRICEINDEXID = productModel.PRODUCTPRICEINDEXID,
                            PRODUCTPRICEINDEXSPREAD = productModel.PRODUCTPRICEINDEXSPREAD,

                            DEALTYPEID = productModel.DEALTYPEID,
                            DEALCLASSIFICATIONID = productModel.DEALCLASSIFICATIONID,
                            DAYCOUNTCONVENTIONID = productModel.DAYCOUNTCONVENTIONID,

                            MAXIMUMTENOR = productModel.MAXIMUMTENOR,
                            MINIMUMTENOR = productModel.MINIMUMTENOR,
                            MAXIMUMRATE = productModel.MAXIMUMRATE,
                            MINIMUMRATE = productModel.MINIMUMRATE,
                            MINIMUMBALANCE = productModel.MINIMUMBALANCE,

                            ALLOWRATE = productModel.ALLOWRATE,
                            ALLOWTENOR = productModel.ALLOWTENOR,
                            ALLOWOVERDRAWN = productModel.ALLOWOVERDRAWN,

                            CREATEDBY = productModel.CREATEDBY,
                            DATETIMECREATED = genSetup.GetApplicationDate(),

                            ISMULTIPLECURENCY = productModel.ISMULTIPLECURENCY,
                            DEFAULTGRACEPERIOD = productModel.DEFAULTGRACEPERIOD,
                            EQUITYCONTRIBUTION = productModel.EQUITYCONTRIBUTION,
                            EXPIRYPERIOD = productModel.EXPIRYPERIOD,

                            ALLOWMORATORIUM = productModel.ALLOWMORATORIUM,
                            ALLOWCUSTOMERACCOUNTFORCEDEBIT = productModel.ALLOWCUSTOMERACCOUNTFORCEDEBIT,
                            CLEANUPPERIOD = productModel.CLEANUPPERIOD,
                            ALLOWSCHEDULETYPEOVERRIDE = productModel.ALLOWSCHEDULETYPEOVERRIDE,
                            SCHEDULETYPEID = productModel.SCHEDULETYPEID,
                            //PRODUCT_BEHAVIOURID = productModel.PRODUCT_BEHAVIOURID,

                            TBL_PRODUCT_CURRENCY = productCurrencies,
                            TBL_PRODUCT_COLLATERALTYPE = productCollateral,
                            TBL_PRODUCT_CHARGE_FEE = productFees,
                            APPROVED = true,
                            DELETED = false,
                            APPROVEDBY = productModel.CREATEDBY
                        };

                        var productBehaviour = new TBL_PRODUCT_BEHAVIOUR()
                        {
                            PRODUCTID = productModel.PRODUCTID,
                            ISINVOICEBASED = productBehaviourModel.ISINVOICEBASED,
                            COLLATERAL_LCY_LIMIT = productBehaviourModel.COLLATERAL_LCY_LIMIT,
                            COLLATERAL_FCY_LIMIT = productBehaviourModel.COLLATERAL_FCY_LIMIT,
                            CUSTOMER_LIMIT = productBehaviourModel.CUSTOMER_LIMIT,
                            PRODUCT_LIMIT = productBehaviourModel.PRODUCT_LIMIT,
                            ALLOWFUNDUSAGE = productBehaviourModel.ALLOWFUNDUSAGE,
                            ISTEMPORARYOVERDRAFT = productBehaviourModel.ISTEMPORARYOVERDRAFT,
                            REQUIRECASAACCOUNT = productBehaviourModel.REQUIRECASAACCOUNT
                        };
                        context.TBL_PRODUCT.Add(product);
                        context.TBL_PRODUCT_BEHAVIOUR.Add(productBehaviour);
                    }

                    //productFee.ApproveProductFee(productId, user);
                    //productCollateralType.ApproveProductCollateral(productId, user);
                }

                productModel.ISCURRENT = false;
                productModel.APPROVALSTATUSID = approvalStatusId;
                productModel.DATETIMEUPDATED = DateTime.Now;

                // Remove all tem products, currencies and fees
                //context.tbl_Temp_Product.Remove(productModel);

                //foreach (var curr in currModel)
                //{
                //    context.TBL_PRODUCT_CURRENCY.Remove(curr);
                //}

                //foreach (var fee in feeModel)
                //{
                //    context.TBL_TEMP_PRODUCT_CHARGE_FEE.Remove(fee);
                //}

                //foreach (var coll in collateralModel)
                //{
                //    context.TBL_TEMP_PRODUCT_COLLATERALTYPE.Remove(coll);
                //}

                // Audit Section ---------------------------
                var audit = new TBL_AUDIT
                {
                    AUDITTYPEID = (short)AuditTypeEnum.ProductUpdated,
                    STAFFID = user.staffId,
                    BRANCHID = (short)user.BranchId,
                    DETAIL = $"Approved Product '{productModel.PRODUCTNAME}' with product code'{productModel.PRODUCTCODE}'",
                    IPADDRESS = user.userIPAddress,
                    URL = user.applicationUrl,
                    APPLICATIONDATE = genSetup.GetApplicationDate(),
                    SYSTEMDATETIME = DateTime.Now
                };

                try
                {
                    context.TBL_AUDIT.Add(audit);
                    // Audit Section ---------------------------
                    var output = context.SaveChanges() > 0;

                    return output;
                }
                catch (Exception ex)
                {
                    throw new Exception(ex.Message);
                }
            }
            else return false;

        }

        public async Task<ProductViewModel> AddTempProduct(ProductViewModel productModel)
        {
            var isPrincipalGLRequired = context.TBL_PRODUCT_TYPE.Any(x => x.PRODUCTTYPEID == productModel.productTypeId && x.REQUIREPRINCIPALGL == true || x.PRODUCTTYPEID == productModel.productTypeId && x.REQUIREPRINCIPALGL2 == true);

            if (isPrincipalGLRequired)
            {
                if (productModel.currencies == null)
                    throw new Exception("Product Currency must be specified. Please select a principal GL with mapped currencies");
            }

            bool output = false;
            var existingTempProduct = context.TBL_TEMP_PRODUCT.FirstOrDefault(x => x.PRODUCTCODE.ToLower() == productModel.productCode.ToLower()
                                                                  && x.ISCURRENT == true && x.COMPANYID == productModel.companyId
                                                                  && x.APPROVALSTATUSID == (short)ApprovalStatusEnum.Pending);

            var existingProductCurrencies = new List<TBL_TEMP_PRODUCT_CURRENCY>();
            var existingProductFees = new List<TBL_TEMP_PRODUCT_CHARGE_FEE>();
            var existingProductCollateral = new List<TBL_TEMP_PRODUCT_COLLATERALTYP>();

            //if (existingTempProduct != null)
            //{
            //    existingProductCurrencies = context.TBL_TEMP_PRODUCT_CURRENCY.Where(c => c.ProductId == existingTempProduct.ProductId && c.Deleted == false).ToList();

            //    existingProductFees =
            //        context.TBL_TEMP_PRODUCT_CHARGE_FEE.Where(c => c.ProductId == existingTempProduct.ProductId && c.Deleted == false).ToList();

            //    existingProductCollateral =
            //        context.TBL_TEMP_PRODUCT_COLLATERALTYPE.Where(c => c.ProductId == existingTempProduct.ProductId && c.Deleted == false).ToList();
            //}


            if (existingTempProduct != null)
            {
                throw new Exception("Product Information already exist and is undergoing approval");
            }

            //// Remove exisiting product fees, currency and collaterals
            //if (existingProductCurrencies.Any())
            //{
            //    foreach (var curr in existingProductCurrencies)
            //    {
            //        context.TBL_TEMP_PRODUCT_CURRENCY.Remove(curr);
            //    }
            //}

            //if (existingProductFees.Any())
            //{
            //    foreach (var fee in existingProductFees)
            //    {
            //        context.TBL_TEMP_PRODUCT_CHARGE_FEE.Remove(fee);
            //    }
            //}

            //if (existingProductCollateral.Any())
            //{
            //    foreach (var coll in existingProductCollateral)
            //    {
            //        context.TBL_TEMP_PRODUCT_COLLATERALTYPE.Remove(coll);
            //    }
            //}

            List<TBL_TEMP_PRODUCT_CURRENCY> currencies = new List<TBL_TEMP_PRODUCT_CURRENCY>();
            List<TBL_TEMP_PRODUCT_CHARGE_FEE> chargeFees = new List<TBL_TEMP_PRODUCT_CHARGE_FEE>();
            List<TBL_TEMP_PRODUCT_COLLATERALTYP> collaterals = new List<TBL_TEMP_PRODUCT_COLLATERALTYP>();

            //Storing the product currencies
            if (productModel.currencies != null)
            {
                foreach (var item in productModel.currencies)
                {
                    var productCurrency = new TBL_TEMP_PRODUCT_CURRENCY
                    {
                        //ProductId = (short)item.productId,
                        CURRENCYID = item.currencyId,
                        CREATEDBY = item.createdBy,
                        DATETIMECREATED = genSetup.GetApplicationDate()
                    };
                    currencies.Add(productCurrency);
                }
            }

            //End of storing the product currencies

            if (productModel.fees != null)
            {
                foreach (var item in productModel.fees)
                {
                    var productFees = new TBL_TEMP_PRODUCT_CHARGE_FEE()
                    {
                        //ProductId = item.productId,
                        CHARGEFEEID = item.feeId,
                        DEPENDENTAMOUNT = item.dependentAmount,
                        RATEVALUE = item.rateValue,
                        COMPANYID = productModel.companyId,
                        CREATEDBY = (int)item.createdBy,
                        DATETIMECREATED = genSetup.GetApplicationDate(),
                        DELETED = false,
                    };
                    chargeFees.Add(productFees);
                }
            }

            if (productModel.collaterals != null)
            {
                foreach (var item in productModel.collaterals)
                {
                    var productCollaterals = new TBL_TEMP_PRODUCT_COLLATERALTYP()
                    {
                        //ProductId = item.productId,
                        COLLATERALTYPEID = item.collateralTypeId,
                        COMPANYID = productModel.companyId,
                        CREATEDBY = item.createdBy,
                        DATETIMECREATED = genSetup.GetApplicationDate(),
                        DELETED = false,
                    };
                    collaterals.Add(productCollaterals);
                }
            }

            var product = new TBL_TEMP_PRODUCT()
            {
                COMPANYID = productModel.companyId,
                PRODUCTTYPEID = productModel.productTypeId,
                PRODUCTCATEGORYID = productModel.productCategoryId,
                PRODUCTCLASSID = (short)productModel.productClassId,
                PRODUCTCODE = GenerateProductCode(productModel.companyId),
                PRODUCTNAME = productModel.productName,
                PRODUCTDESCRIPTION = productModel.productDescription,

                PRINCIPALBALANCEGL = productModel.principalBalanceGl,
                PRINCIPALBALANCEGL2 = productModel.principalBalanceGl2,
                INTERESTINCOMEEXPENSEGL = productModel.interestIncomeExpenseGl,
                INTERESTRECEIVABLEPAYABLEGL = productModel.interestReceivablePayableGl,
                DORMANTGL = productModel.dormantGl,
                PREMIUMDISCOUNTGL = productModel.premiumDiscountGl,
                OVERDRAWNGL = productModel.overdrawnGl,

                PRODUCTPRICEINDEXID = productModel.productPriceIndexId,
                PRODUCTPRICEINDEXSPREAD = productModel.productPriceIndexSpread,

                DEALTYPEID = productModel.dealTypeId,
                DEALCLASSIFICATIONID = productModel.dealClassificationId,
                DAYCOUNTCONVENTIONID = productModel.dayCountId,

                MAXIMUMTENOR = productModel.maximumTenor,
                MINIMUMTENOR = productModel.minimumTenor,
                MAXIMUMRATE = productModel.maximumRate,
                MINIMUMRATE = productModel.minimumRate,
                MINIMUMBALANCE = productModel.minimumBalance,

                ALLOWRATE = productModel.allowRate,
                ALLOWTENOR = productModel.allowTenor,
                ALLOWOVERDRAWN = productModel.allowOverdrawn,

                CREATEDBY = productModel.createdBy,
                DATETIMECREATED = DateTime.Now,
                APPROVALSTATUSID = (short)ApprovalStatusEnum.Pending,
                ISCURRENT = true,
                DELETED = false,
                TBL_TEMP_PRODUCT_CURRENCY = currencies,

                ISMULTIPLECURENCY = productModel.currencies != null,
                ALLOWCUSTOMERACCOUNTFORCEDEBIT = productModel.allowCustomerAccountForceDebit,
                ALLOWMORATORIUM = productModel.allowMoratorium,
                ALLOWSCHEDULETYPEOVERRIDE = productModel.allowScheduleTypeOverride,
                SCHEDULETYPEID = productModel.scheduleTypeId,

                DEFAULTGRACEPERIOD = productModel.defaultGracePeriod,
                CLEANUPPERIOD = productModel.cleanupPeriod,
                EQUITYCONTRIBUTION = productModel.equityContribution,
                EXPIRYPERIOD = productModel.expiryPeriod,

                TBL_TEMP_PRODUCT_CHARGE_FEE = chargeFees,
                TBL_TEMP_PRODUCT_COLLATERALTYP = collaterals
            };

            var behaviour = productModel.ProductBehaviour;
            var productBehaviour = new TBL_TEMP_PRODUCT_BEHAVIOUR()
            {
                APPROVALSTATUSID = (short)ApprovalStatusEnum.Pending,
                ISCURRENT = true,
                PRODUCTCODE = behaviour.productCode,
                COLLATERAL_LCY_LIMIT = behaviour.collateralLcyLimit,
                COLLATERAL_FCY_LIMIT = behaviour.collateralFcyLimit,
                CUSTOMER_LIMIT = behaviour.customerLimit,
                PRODUCT_LIMIT = behaviour.productLimit,
                ISINVOICEBASED = behaviour.isInvoiceBased,
                ISTEMPORARYOVERDRAFT = behaviour.isTemporaryOverDraft,
                ALLOWFUNDUSAGE = behaviour.allowFundUsage,
                DATETIMECREATED = DateTime.Now,
                CREATEDBY = productModel.createdBy
            };

            // Audit Section ---------------------------
            var audit = new TBL_AUDIT
            {
                AUDITTYPEID = (short)AuditTypeEnum.ProductAdded,
                STAFFID = productModel.createdBy,
                BRANCHID = (short)productModel.userBranchId,
                DETAIL = $"Initiated Product Creation for '{productModel.productName}' with code'{productModel.productCode}'",
                IPADDRESS = productModel.userIPAddress,
                URL = productModel.applicationUrl,
                APPLICATIONDATE = genSetup.GetApplicationDate(),
                SYSTEMDATETIME = DateTime.Now
            };

            using (var trans = context.Database.BeginTransaction())
            {
                try
                {
                    auditTrail.AddAuditTrail(audit);
                    context.TBL_TEMP_PRODUCT.Add(product);
                    //context.TBL_TEMP_PRODUCT_BEHAVIOUR.Add(productBehaviour);
                    output = await context.SaveChangesAsync() > 0;

                    var entity = new ApprovalViewModel
                    {
                        staffId = productModel.createdBy,
                        companyId = productModel.companyId,
                        approvalStatusId = (int)ApprovalStatusEnum.Pending,
                        comment = "Please approve this product",
                        targetId = product.PRODUCTID,
                        operationId = (int)OperationsEnum.ProductCreation,
                        BranchId = productModel.userBranchId,
                        externalInitialization = true
                    };
                    var response = workFlow.LogForApproval(entity);

                    if (response)
                    {
                        trans.Commit();

                        if (output)
                        {
                            return new ProductViewModel { productId = product.PRODUCTID, productCode = product.PRODUCTCODE };
                        }
                    }
                }
                catch (Exception ex)
                {
                    trans.Rollback();
                    throw new Exception(ex.Message);
                }
            }

            return new ProductViewModel();

        }

        public bool IsProductCodeAlreadyExist(string productCode)
        {
            return context.TBL_PRODUCT.Any(x => x.PRODUCTCODE.ToLower() == productCode.ToLower());
        }

        public bool IsProductExist(string productCode)
        {
            return context.TBL_TEMP_PRODUCT.Any(x => x.PRODUCTCODE.ToLower() == productCode.ToLower() && x.APPROVALSTATUSID == (int)ApprovalStatusEnum.Pending && x.ISCURRENT == true);
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
        //        PrincipalBalanceGL = product.principalBalanceGl2,
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
        public async Task<bool> UpdateProduct(int productId, ProductViewModel productModel)
        {
            bool output = false;
            var targetProductId = 0;

            var existingTempProduct = context.TBL_TEMP_PRODUCT
                    .FirstOrDefault(x => x.PRODUCTCODE.ToLower() ==
                        productModel.productCode.ToLower() && x.ISCURRENT == false
                            && x.APPROVALSTATUSID == (int)ApprovalStatusEnum.Approved);

            var existingProductCurrencies = new List<TBL_TEMP_PRODUCT_CURRENCY>();
            var existingProductFees = new List<TBL_TEMP_PRODUCT_CHARGE_FEE>();
            var existingProductCollateral = new List<TBL_TEMP_PRODUCT_COLLATERALTYP>();

            List<TBL_TEMP_PRODUCT_CHARGE_FEE> productFees = new List<TBL_TEMP_PRODUCT_CHARGE_FEE>();
            List<TBL_TEMP_PRODUCT_COLLATERALTYP> productCollaterals = new List<TBL_TEMP_PRODUCT_COLLATERALTYP>();
            List<TBL_TEMP_PRODUCT_CURRENCY> productCurrencies = new List<TBL_TEMP_PRODUCT_CURRENCY>();

            var unApprovedProductEdit = context.TBL_TEMP_PRODUCT
                .Where(x => x.ISCURRENT == true && x.APPROVALSTATUSID == (int)ApprovalStatusEnum.Pending
                && x.PRODUCTCODE.ToLower() == productModel.productCode.ToLower());

            TBL_TEMP_PRODUCT tempProduct = new TBL_TEMP_PRODUCT();
            TBL_TEMP_PRODUCT_BEHAVIOUR tempProductBehaviour = new TBL_TEMP_PRODUCT_BEHAVIOUR();

            if (unApprovedProductEdit.Any())
            {
                throw new Exception("Product is already undergoing approval");
            }

            if (existingTempProduct != null)
            {
                var existingTempProductBehaviour = context.TBL_TEMP_PRODUCT_BEHAVIOUR
                    .FirstOrDefault(x => x.PRODUCTCODE.ToLower() ==
                        productModel.productCode.ToLower() && x.ISCURRENT == false
                            && x.APPROVALSTATUSID == (int)ApprovalStatusEnum.Approved);
                existingProductCurrencies = context.TBL_TEMP_PRODUCT_CURRENCY.Where(x => x.PRODUCTID == existingTempProduct.PRODUCTID).ToList();
                existingProductFees = context.TBL_TEMP_PRODUCT_CHARGE_FEE.Where(x => x.PRODUCTID == existingTempProduct.PRODUCTID).ToList();
                existingProductCollateral = context.TBL_TEMP_PRODUCT_COLLATERALTYP.Where(x => x.PRODUCTID == existingTempProduct.PRODUCTID).ToList();

                // Remove exisiting product fees, currency and collaterals
                if (existingProductCurrencies.Count > 0)
                {
                    foreach (var curr in existingProductCurrencies)
                    {
                        context.TBL_TEMP_PRODUCT_CURRENCY.Remove(curr);
                    }
                }

                if (existingProductFees.Count > 0)
                {
                    foreach (var fee in existingProductFees)
                    {
                        context.TBL_TEMP_PRODUCT_CHARGE_FEE.Remove(fee);
                    }
                }

                if (existingProductCollateral.Count > 0)
                {
                    foreach (var coll in existingProductCollateral)
                    {
                        context.TBL_TEMP_PRODUCT_COLLATERALTYP.Remove(coll);
                    }
                }

                foreach (var item in productModel.currencies)
                {
                    var productCurrency = new TBL_TEMP_PRODUCT_CURRENCY
                    {
                        //ProductId = (short)item.productId,
                        CURRENCYID = item.currencyId,
                        CREATEDBY = item.createdBy,
                        DATETIMECREATED = genSetup.GetApplicationDate()
                    };
                    productCurrencies.Add(productCurrency);
                }

                foreach (var item in productModel.fees)
                {
                    var fee = new TBL_TEMP_PRODUCT_CHARGE_FEE()
                    {
                        //ProductId = item.productId,
                        CHARGEFEEID = item.feeId,
                        DEPENDENTAMOUNT = item.dependentAmount,
                        RATEVALUE = item.rateValue,
                        COMPANYID = productModel.companyId,
                        CREATEDBY = (int)item.createdBy,
                        DATETIMECREATED = genSetup.GetApplicationDate(),
                        DELETED = false
                    };
                    productFees.Add(fee);
                }

                foreach (var item in productModel.collaterals)
                {
                    var collateral = new TBL_TEMP_PRODUCT_COLLATERALTYP()
                    {
                        //ProductId = item.productId,
                        COLLATERALTYPEID = item.collateralTypeId,
                        COMPANYID = productModel.companyId,
                        CREATEDBY = item.createdBy,
                        DATETIMECREATED = genSetup.GetApplicationDate()
                    };
                    productCollaterals.Add(collateral);
                }

                var tempProductToUpdate = existingTempProduct;

                //tempProductToUpdate.ProductId = (short)productModel.productId;
                tempProductToUpdate.PRODUCTCLASSID = (short)productModel.productClassId;
                tempProductToUpdate.PRODUCTCODE = productModel.productCode;
                tempProductToUpdate.PRODUCTNAME = productModel.productName;
                tempProductToUpdate.PRODUCTDESCRIPTION = productModel.productDescription;

                tempProductToUpdate.PRINCIPALBALANCEGL = productModel.principalBalanceGl;
                tempProductToUpdate.PRINCIPALBALANCEGL2 = productModel.principalBalanceGl2;
                tempProductToUpdate.INTERESTINCOMEEXPENSEGL = productModel.interestIncomeExpenseGl;
                tempProductToUpdate.INTERESTRECEIVABLEPAYABLEGL = productModel.interestReceivablePayableGl;
                tempProductToUpdate.DORMANTGL = productModel.dormantGl;
                tempProductToUpdate.PREMIUMDISCOUNTGL = productModel.premiumDiscountGl;
                tempProductToUpdate.OVERDRAWNGL = productModel.overdrawnGl;

                tempProductToUpdate.PRODUCTPRICEINDEXID = productModel.productPriceIndexId;
                tempProductToUpdate.PRODUCTPRICEINDEXSPREAD = productModel.productPriceIndexSpread;

                tempProductToUpdate.DEALTYPEID = productModel.dealTypeId;
                tempProductToUpdate.DEALCLASSIFICATIONID = productModel.dealClassificationId;
                tempProductToUpdate.DAYCOUNTCONVENTIONID = productModel.dayCountId;

                tempProductToUpdate.MAXIMUMTENOR = productModel.maximumTenor;
                tempProductToUpdate.MINIMUMTENOR = productModel.minimumTenor;
                tempProductToUpdate.MAXIMUMRATE = productModel.maximumRate;
                tempProductToUpdate.MINIMUMRATE = productModel.minimumRate;
                tempProductToUpdate.MINIMUMBALANCE = productModel.minimumBalance;

                tempProductToUpdate.ALLOWRATE = productModel.allowRate;
                tempProductToUpdate.ALLOWTENOR = productModel.allowTenor;
                tempProductToUpdate.ALLOWOVERDRAWN = productModel.allowOverdrawn;
                tempProductToUpdate.ALLOWCUSTOMERACCOUNTFORCEDEBIT = productModel.allowCustomerAccountForceDebit;
                tempProductToUpdate.ALLOWMORATORIUM = productModel.allowMoratorium;
                tempProductToUpdate.ALLOWSCHEDULETYPEOVERRIDE = productModel.allowScheduleTypeOverride;

                tempProductToUpdate.CLEANUPPERIOD = productModel.cleanupPeriod;
                tempProductToUpdate.DEFAULTGRACEPERIOD = productModel.defaultGracePeriod;
                tempProductToUpdate.EQUITYCONTRIBUTION = productModel.equityContribution;
                tempProductToUpdate.EXPIRYPERIOD = productModel.expiryPeriod;
                tempProductToUpdate.ISMULTIPLECURENCY = productModel.currencies.Any();
                tempProductToUpdate.SCHEDULETYPEID = productModel.scheduleTypeId;
                tempProductToUpdate.ISCURRENT = true;
                tempProductToUpdate.DATETIMEUPDATED = DateTime.Now;
                tempProductToUpdate.DELETED = false;
                //tempProductToUpdate.PRODUCT_BEHAVIOURID = productModel.productBehaviourId;

                tempProductToUpdate.TBL_TEMP_PRODUCT_CURRENCY = productCurrencies;
                tempProductToUpdate.TBL_TEMP_PRODUCT_CHARGE_FEE = productFees;
                tempProductToUpdate.TBL_TEMP_PRODUCT_COLLATERALTYP = productCollaterals;

                //Product Behaviour Update
                if (existingTempProductBehaviour != null)
                {
                    var TempProductBehaviourToUpdate = existingTempProductBehaviour;
                    TempProductBehaviourToUpdate.COLLATERAL_FCY_LIMIT = productModel.ProductBehaviour.collateralFcyLimit;
                    TempProductBehaviourToUpdate.COLLATERAL_LCY_LIMIT = productModel.ProductBehaviour.collateralLcyLimit;
                    TempProductBehaviourToUpdate.CUSTOMER_LIMIT = productModel.ProductBehaviour.customerLimit;
                    TempProductBehaviourToUpdate.ISCURRENT = true;
                    TempProductBehaviourToUpdate.ISINVOICEBASED = productModel.ProductBehaviour.isInvoiceBased;
                    TempProductBehaviourToUpdate.PRODUCTCODE = productModel.ProductBehaviour.productCode;
                    TempProductBehaviourToUpdate.PRODUCT_LIMIT = productModel.ProductBehaviour.productLimit;
                    TempProductBehaviourToUpdate.ALLOWFUNDUSAGE = productModel.ProductBehaviour.allowFundUsage;
                    TempProductBehaviourToUpdate.ISTEMPORARYOVERDRAFT = productModel.ProductBehaviour.isTemporaryOverDraft;

                }
                else if (productModel.ProductBehaviour != null)
                {
                    tempProductBehaviour = new TBL_TEMP_PRODUCT_BEHAVIOUR()
                    {
                        PRODUCTCODE = productModel.productCode,
                        COLLATERAL_FCY_LIMIT = productModel.ProductBehaviour.collateralFcyLimit,
                        COLLATERAL_LCY_LIMIT = productModel.ProductBehaviour.collateralLcyLimit,
                        CUSTOMER_LIMIT = productModel.ProductBehaviour.customerLimit,
                        ISCURRENT = true,
                        ISINVOICEBASED = productModel.ProductBehaviour.isInvoiceBased,
                        PRODUCT_LIMIT = productModel.ProductBehaviour.productLimit,
                       ISTEMPORARYOVERDRAFT = productModel.ProductBehaviour.isTemporaryOverDraft,
                        ALLOWFUNDUSAGE = productModel.ProductBehaviour.allowFundUsage,
                        DATETIMECREATED = DateTime.Now,
                        CREATEDBY = productModel.createdBy
                    };
                    context.TBL_TEMP_PRODUCT_BEHAVIOUR.Add(tempProductBehaviour);
                }


            }
            else
            {
                var targetProduct = context.TBL_PRODUCT.Find(productId);
                var targetProductBehaviour = context.TBL_PRODUCT_BEHAVIOUR.Where(x => x.PRODUCTID == targetProduct.PRODUCTID).FirstOrDefault();
                //Storing the updated product currencies
                foreach (var item in productModel.currencies)
                {
                    var currency = new TBL_TEMP_PRODUCT_CURRENCY()
                    {
                        //ProductId = item.productId,
                        CURRENCYID = item.currencyId,
                        CREATEDBY = productModel.createdBy,
                        DATETIMECREATED = genSetup.GetApplicationDate()
                    };
                    productCurrencies.Add(currency);
                }

                foreach (var item in productModel.fees)
                {
                    var fee = new TBL_TEMP_PRODUCT_CHARGE_FEE()
                    {
                        CHARGEFEEID = item.feeId,
                        COMPANYID = productModel.companyId,

                        RATEVALUE = item.rateValue,
                        DEPENDENTAMOUNT = item.dependentAmount,

                        CREATEDBY = item.createdBy,
                        DATETIMECREATED = genSetup.GetApplicationDate(),
                        DELETED = false,
                        //IsCurrent = true
                    };
                    productFees.Add(fee);
                }

                foreach (var item in productModel.collaterals)
                {
                    var collateral = new TBL_TEMP_PRODUCT_COLLATERALTYP()
                    {
                        //ProductId = item.productId,
                        COLLATERALTYPEID = item.collateralTypeId,
                        COMPANYID = item.companyId,
                        CREATEDBY = item.createdBy,
                        DATETIMECREATED = genSetup.GetApplicationDate()
                    };
                    productCollaterals.Add(collateral);
                }

                //End of storing the updated product currencies
                tempProduct = new TBL_TEMP_PRODUCT()
                {
                    COMPANYID = productModel.companyId,
                    PRODUCTTYPEID = productModel.productTypeId,
                    PRODUCTCATEGORYID = productModel.productCategoryId,
                    PRODUCTCLASSID = (short)productModel.productClassId,
                    PRODUCTCODE = targetProduct?.PRODUCTCODE,
                    PRODUCTNAME = productModel.productName,
                    PRODUCTDESCRIPTION = productModel.productDescription,

                    PRINCIPALBALANCEGL = productModel.principalBalanceGl,
                    PRINCIPALBALANCEGL2 = productModel.principalBalanceGl2,
                    INTERESTINCOMEEXPENSEGL = productModel.interestIncomeExpenseGl,
                    INTERESTRECEIVABLEPAYABLEGL = productModel.interestReceivablePayableGl,
                    DORMANTGL = productModel.dormantGl,
                    PREMIUMDISCOUNTGL = productModel.premiumDiscountGl,
                    OVERDRAWNGL = productModel.overdrawnGl,

                    PRODUCTPRICEINDEXID = productModel.productPriceIndexId,
                    PRODUCTPRICEINDEXSPREAD = productModel.productPriceIndexSpread,

                    DEALTYPEID = productModel.dealTypeId,
                    DEALCLASSIFICATIONID = productModel.dealClassificationId,
                    DAYCOUNTCONVENTIONID = productModel.dayCountId,

                    MAXIMUMTENOR = productModel.maximumTenor,
                    MINIMUMTENOR = productModel.minimumTenor,
                    MAXIMUMRATE = productModel.maximumRate,
                    MINIMUMRATE = productModel.minimumRate,
                    MINIMUMBALANCE = productModel.minimumBalance,

                    ALLOWRATE = productModel.allowRate,
                    ALLOWTENOR = productModel.allowTenor,
                    ALLOWOVERDRAWN = productModel.allowOverdrawn,

                    CREATEDBY = productModel.createdBy,
                    DATETIMECREATED = DateTime.Now,
                    APPROVALSTATUSID = (short)ApprovalStatusEnum.Pending,
                    ISCURRENT = true,
                    DELETED = false,
                    ALLOWCUSTOMERACCOUNTFORCEDEBIT = productModel.allowCustomerAccountForceDebit,
                    ALLOWSCHEDULETYPEOVERRIDE = productModel.allowScheduleTypeOverride,
                    ALLOWMORATORIUM = productModel.allowMoratorium,
                    CLEANUPPERIOD = productModel.cleanupPeriod,
                    DEFAULTGRACEPERIOD = productModel.defaultGracePeriod,
                    EQUITYCONTRIBUTION = productModel.equityContribution,
                    EXPIRYPERIOD = productModel.expiryPeriod,
                    ISMULTIPLECURENCY = productModel.currencies.Any(),
                    //PRODUCT_BEHAVIOURID = productModel.productBehaviourId,

                    TBL_TEMP_PRODUCT_CURRENCY = productCurrencies,
                    TBL_TEMP_PRODUCT_COLLATERALTYP = productCollaterals,
                    TBL_TEMP_PRODUCT_CHARGE_FEE = productFees
                };

                tempProductBehaviour = new TBL_TEMP_PRODUCT_BEHAVIOUR()
                {
                    PRODUCTCODE = productModel.productCode,
                    COLLATERAL_FCY_LIMIT = productModel.ProductBehaviour.collateralFcyLimit,
                    COLLATERAL_LCY_LIMIT = productModel.ProductBehaviour.collateralLcyLimit,
                    CUSTOMER_LIMIT = productModel.ProductBehaviour.customerLimit,
                    ISCURRENT = true,
                    ISINVOICEBASED = productModel.ProductBehaviour.isInvoiceBased,
                    PRODUCT_LIMIT = productModel.ProductBehaviour.productLimit,
                    ALLOWFUNDUSAGE = productModel.ProductBehaviour.allowFundUsage,
                    ISTEMPORARYOVERDRAFT = productModel.ProductBehaviour.isTemporaryOverDraft,
                    DATETIMECREATED = DateTime.Now,
                    CREATEDBY = productModel.createdBy
                };

                context.TBL_TEMP_PRODUCT.Add(tempProduct);
                context.TBL_TEMP_PRODUCT_BEHAVIOUR.Add(tempProductBehaviour);
            }

            var audit = new TBL_AUDIT
            {
                AUDITTYPEID = (short)AuditTypeEnum.StaffUpdated,
                STAFFID = productModel.createdBy,
                BRANCHID = (short)productModel.userBranchId,
                DETAIL = $"Updated Product '{productModel.productName}' with code'{productModel.productCode}'",
                IPADDRESS = productModel.userIPAddress,
                URL = productModel.applicationUrl,
                APPLICATIONDATE = genSetup.GetApplicationDate(),
                SYSTEMDATETIME = DateTime.Now,
                TARGETID = productId
            };

            using (var trans = context.Database.BeginTransaction())
            {
                try
                {
                    this.auditTrail.AddAuditTrail(audit);
                    //end of Audit section -------------------------------

                    output = await context.SaveChangesAsync() > 0;

                    targetProductId = existingTempProduct?.PRODUCTID ?? tempProduct.PRODUCTID;

                    var entity = new ApprovalViewModel
                    {
                        staffId = productModel.createdBy,
                        companyId = productModel.companyId,
                        approvalStatusId = (int)ApprovalStatusEnum.Pending,
                        targetId = targetProductId,
                        operationId = (int)OperationsEnum.ProductCreation,
                        BranchId = productModel.userBranchId,
                        externalInitialization = true
                    };
                    var response = workFlow.LogForApproval(entity);

                    if (response)
                    {
                        trans.Commit();

                        return output;
                    }

                    return false;
                }
                catch (Exception ex)
                {
                    trans.Rollback();
                    throw new Exception(ex.Message);
                }
            }
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
            return (from data in context.TBL_PRODUCT_PRICE_INDEX
                    where data.COMPANYID == companyId && data.DELETED == false
                    select new ProductPriceIndexViewModel()
                    {
                        productPriceIndexId = data.PRODUCTPRICEINDEXID,
                        priceIndexDescription = data.PRICEINDEXDESCRIPTION,
                        companyId = data.COMPANYID,
                        priceIndexName = data.PRICEINDEXNAME,
                        priceIndexRate = data.PRICEINDEXRATE,
                        dateTimeUpdated = data.DATETIMEUPDATED,
                        deleted = data.DELETED,
                        deletedBy = data.DELETEDBY,
                        dateTimeDeleted = data.DATETIMEDELETED
                    });
        }

        public IEnumerable<ProductPriceIndexViewModel> GetProductPriceIndex(int companyId)
        {
            return GetAllProductPriceIndex(companyId);
        }

        public ProductPriceIndexViewModel GetProductPriceIndexById(int productPriceIndexId, int companyId)
        {
            return GetAllProductPriceIndex(companyId).SingleOrDefault(c => c.productPriceIndexId == productPriceIndexId);
        }

        public ProductPriceIndexViewModel AddProductPriceIndex(ProductPriceIndexViewModel prodPriceIndex)
        {
            var isProductPriceIndexExist = context.TBL_PRODUCT_PRICE_INDEX.Any(x => x.PRICEINDEXNAME.ToLower() == prodPriceIndex.priceIndexName.ToLower());

            if (isProductPriceIndexExist)
            {
                throw new Exception("Product price already exists!");
            }
            var data = new TBL_PRODUCT_PRICE_INDEX()
            {
                COMPANYID = prodPriceIndex.companyId,
                PRICEINDEXDESCRIPTION = prodPriceIndex.priceIndexDescription,
                PRICEINDEXNAME = prodPriceIndex.priceIndexName,
                PRICEINDEXRATE = prodPriceIndex.priceIndexRate,
                CREATEDBY = prodPriceIndex.createdBy,
                DATETIMECREATED = DateTime.Now,
            };

            this.context.TBL_PRODUCT_PRICE_INDEX.Add(data);

            // Audit Section ---------------------------
            var audit = new TBL_AUDIT
            {
                AUDITTYPEID = (short)AuditTypeEnum.ProductPriceIndexAdded,
                STAFFID = (int)prodPriceIndex.createdBy,
                BRANCHID = (short)prodPriceIndex.userBranchId,
                DETAIL = $"Added tbl_Product Price Index: '{prodPriceIndex.priceIndexName}' ",
                IPADDRESS = prodPriceIndex.userIPAddress,
                URL = prodPriceIndex.applicationUrl,
                APPLICATIONDATE = genSetup.GetApplicationDate(),
                SYSTEMDATETIME = DateTime.Now
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
            var data = this.context.TBL_PRODUCT_PRICE_INDEX.FirstOrDefault(x => x.PRODUCTPRICEINDEXID == productPriceIndexId);

            if (data == null)
                return false;

            data.PRICEINDEXNAME = prodPriceIndex.priceIndexName;
            data.PRICEINDEXDESCRIPTION = prodPriceIndex.priceIndexDescription;
            data.PRICEINDEXRATE = prodPriceIndex.priceIndexRate;

            data.LASTUPDATEDBY = prodPriceIndex.lastUpdatedBy;
            data.DATETIMEUPDATED = DateTime.Now;

            // Audit Section ---------------------------
            var audit = new TBL_AUDIT
            {
                AUDITTYPEID = (short)AuditTypeEnum.ProductPriceIndexUpdated,
                STAFFID = (int)prodPriceIndex.createdBy,
                BRANCHID = (short)prodPriceIndex.userBranchId,
                DETAIL = $"Updated tbl_Product Price Index: '{prodPriceIndex.priceIndexName}' ",
                IPADDRESS = prodPriceIndex.userIPAddress,
                URL = prodPriceIndex.applicationUrl,
                APPLICATIONDATE = genSetup.GetApplicationDate(),
                SYSTEMDATETIME = DateTime.Now
            };

            this.auditTrail.AddAuditTrail(audit);
            //end of Audit section -------------------------------
            return this.SaveAll();
        }

        public bool DeleteProductPriceIndex(int productPriceIndexId, UserInfo user)
        {
            var data = this.context.TBL_PRODUCT_PRICE_INDEX.Find(productPriceIndexId);

            if (data == null)
                return false;

            data.DELETED = true;
            data.DATETIMEDELETED = genSetup.GetApplicationDate();

            // Audit Section ---------------------------
            var productPriceIndex = this.context.TBL_PRODUCT_PRICE_INDEX.FirstOrDefault(x => x.PRODUCTPRICEINDEXID == data.PRODUCTPRICEINDEXID);
            var audit = new TBL_AUDIT
            {
                AUDITTYPEID = (short)AuditTypeEnum.ProductPriceIndexDeleted,
                STAFFID = user.staffId,
                BRANCHID = (short)user.BranchId,
                DETAIL = $"Deleted tbl_Product Price Index: '{data.PRICEINDEXNAME}' with rate '{data.PRICEINDEXRATE}' ",
                IPADDRESS = user.userIPAddress,
                URL = user.applicationUrl,
                APPLICATIONDATE = genSetup.GetApplicationDate(),
                SYSTEMDATETIME = DateTime.Now,
                TARGETID = productPriceIndexId
            };

            this.auditTrail.AddAuditTrail(audit);

            //end of Audit section -------------------------------
            return this.SaveAll();
        }

        #endregion product Price Index

        #region Product Class Process 

        public IEnumerable<ProductClassProcessViewModel> GetAllProductClassProcesses()
        {
            var data = (from p in context.TBL_PRODUCT_CLASS_PROCESS
                        select new ProductClassProcessViewModel
                        {
                            productClassProcessId = p.PRODUCT_CLASS_PROCESSID,
                            productClassProcessName = p.PRODUCT_CLASS_PROCESS_NAME,
                            maximumAmount = p.MAXIMUM_AMOUNT,
                            useAmountLimit = p.USE_AMOUNT_LIMIT
                        }).ToList();

            return data;
        }

        public bool AddProductClassProcess(ProductClassProcessViewModel model)
        {
            if (model != null)
            {
                var data = new TBL_PRODUCT_CLASS_PROCESS()
                {
                    PRODUCT_CLASS_PROCESS_NAME = model.productClassProcessName,
                    MAXIMUM_AMOUNT = (decimal)model.maximumAmount,
                    USE_AMOUNT_LIMIT = model.useAmountLimit
                };

                try
                {
                    context.TBL_PRODUCT_CLASS_PROCESS.Add(data);

                    return context.SaveChanges() > 0;
                }
                catch (Exception ex)
                {
                    throw ex;
                }
            }

            return false;
        }

        public bool UpdateProductClassProcess(int productClassProcessId, ProductClassProcessViewModel model)
        {
            var data = context.TBL_PRODUCT_CLASS_PROCESS.Find(productClassProcessId);

            if (data != null)
            {
                data.PRODUCT_CLASS_PROCESS_NAME = model.productClassProcessName;
                data.MAXIMUM_AMOUNT = (decimal)model.maximumAmount;
                data.USE_AMOUNT_LIMIT = model.useAmountLimit;

                try
                {
                    return context.SaveChanges() > 0;
                }
                catch (Exception ex)
                {
                    throw ex;
                }
            }

            return false;
        }

        public ProductClassProcessViewModel GetProductProcessByProcessId(int proccessId)
        {
            var data = context.TBL_PRODUCT_CLASS_PROCESS.Where(c => c.PRODUCT_CLASS_PROCESSID == proccessId).Select
            (c => new ProductClassProcessViewModel
            {
                maximumAmount = c.MAXIMUM_AMOUNT,
                productClassProcessId = c.PRODUCT_CLASS_PROCESSID,
                productClassProcessName = c.PRODUCT_CLASS_PROCESS_NAME,
                useAmountLimit = c.USE_AMOUNT_LIMIT

            });

            return data.FirstOrDefault();
        }

        #endregion Product Class Process


        #region Product Classification 
        public IEnumerable<LookupViewModel> GetAllProductClassType()
        {
            return (from data in context.TBL_PRODUCT_CLASS_TYPE
                    select new LookupViewModel()
                    {
                        lookupId = data.PRODUCTCLASSTYPEID,
                        lookupName = data.PRODUCTCLASSTYPENAME,
                    });
        }

        public IEnumerable<ProductClassificationViewModel> GetAllProductClassification()
        {
            var data = (from p in context.TBL_PRODUCT_CLASS
                        select new ProductClassificationViewModel
                        {
                            productClassId = p.PRODUCTCLASSID,
                            productClassName = p.PRODUCTCLASSNAME,
                            productClassTypeId = p.PRODUCTCLASSTYPEID,
                            productClassType = p.TBL_PRODUCT_CLASS_TYPE.PRODUCTCLASSTYPENAME,
                            productClassProcessId = p.PRODUCT_CLASS_PROCESSID,
                            productClassProcess = p.TBL_PRODUCT_CLASS_PROCESS.PRODUCT_CLASS_PROCESS_NAME,
                            customerTypeId = p.CUSTOMERTYPEID,
                            customerType = p.TBL_CUSTOMER_TYPE.NAME
                        }).ToList();
            return data;
        }

        public bool AddUpdateProductClassification(ProductClassificationViewModel model)
        {
            if (model != null)
            {
                try
                {
                    TBL_PRODUCT_CLASS productClass;
                    if (model.productClassId != 0 || model.productClassId > 0)
                    {
                        productClass = context.TBL_PRODUCT_CLASS.Find(model.productClassId);
                        if (productClass != null)
                        {
                            productClass.PRODUCTCLASSNAME = model.productClassName;
                            productClass.PRODUCTCLASSTYPEID = model.productClassTypeId;
                            productClass.PRODUCT_CLASS_PROCESSID = model.productClassProcessId;
                            productClass.CUSTOMERTYPEID = model.customerTypeId;
                        }
                    }
                    else
                    {
                        productClass = new TBL_PRODUCT_CLASS()
                        {  
                           PRODUCTCLASSNAME = model.productClassName,
                           PRODUCTCLASSTYPEID = model.productClassTypeId,
                           PRODUCT_CLASS_PROCESSID = model.productClassProcessId,
                          CUSTOMERTYPEID = model.customerTypeId
                        };
                        context.TBL_PRODUCT_CLASS.Add(productClass);
                    }
                    // Audit Section ---------------------------
                    var audit = new TBL_AUDIT
                    {
                        AUDITTYPEID = (short)AuditTypeEnum.ProductPriceIndexAdded,
                        STAFFID = (int)model.createdBy,
                        BRANCHID = (short)model.userBranchId,
                        DETAIL = $"Added/Updated Product Classification with name: '{model.productClassName}' ",
                        IPADDRESS = model.userIPAddress,
                        URL = model.applicationUrl,
                        APPLICATIONDATE = genSetup.GetApplicationDate(),
                        SYSTEMDATETIME = DateTime.Now
                    };

                    this.auditTrail.AddAuditTrail(audit);
                    //end of Audit section -------------------------------
                    return context.SaveChanges() > 0;
                }
                catch (Exception ex)
                {
                    throw ex;
                }
            }

            return false;
        }
        public bool ValidateProductClassification(string productClassName)
        {
            return context.TBL_PRODUCT_CLASS.Where(x => x.PRODUCTCLASSNAME == productClassName).Any();
        }
        #endregion
    }
}
