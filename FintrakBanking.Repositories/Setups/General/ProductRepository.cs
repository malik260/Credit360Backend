using FintrakBanking.Common;
using FintrakBanking.Common.CustomException;
using FintrakBanking.Common.Enum;
using FintrakBanking.Entities.DocumentModels;
using FintrakBanking.Entities.Models;
using FintrakBanking.Interfaces.Admin;
using FintrakBanking.Interfaces.Credit;
using FintrakBanking.Interfaces.Setups.Approval;
using FintrakBanking.Interfaces.Setups.General;
using FintrakBanking.Interfaces.WorkFlow;
using FintrakBanking.ViewModels;
using FintrakBanking.ViewModels.Credit;
using FintrakBanking.ViewModels.External.Product;
using FintrakBanking.ViewModels.Setups.General;
using FintrakBanking.ViewModels.WorkFlow;
using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Configuration;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;

namespace FintrakBanking.Repositories.Setups.General
{

    public class ProductRepository : IProductRepository
    {
        private FinTrakBankingContext context;
        private FinTrakBankingDocumentsContext docContext;
        private IGeneralSetupRepository genSetup;
        private IAuditTrailRepository auditTrail;
        private IWorkflow workFlow;
        private IApprovalLevelStaffRepository level;
        private IProductFeeRepository productFee;
        private IProductCollateralTypeRepository productCollateralType;
        private ILoanOperationsRepository loanOperations;

        public ProductRepository(FinTrakBankingContext _context,
                                FinTrakBankingDocumentsContext _docContext,
                                IGeneralSetupRepository _genSetup,
                                IAuditTrailRepository _auditTrail,
                                IWorkflow _workFlow,
                                IApprovalLevelStaffRepository _level,
                                IProductFeeRepository _productFee,
                                IProductCollateralTypeRepository _productCollateralType, ILoanOperationsRepository _loanOperations)
        {
            this.context = _context;
            this.docContext = _docContext;
            this.genSetup = _genSetup;
            this.auditTrail = _auditTrail;
            this.workFlow = _workFlow;
            level = _level;
            productFee = _productFee;
            productCollateralType = _productCollateralType;
            loanOperations = _loanOperations;
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
                    orderby data.PRODUCTCLASSNAME ascending
                    //where data.OperationTypeId == operationTypeId
                    select new LookupViewModel()
                    {
                        lookupId = (short)data.PRODUCTCLASSID,
                        lookupName = data.PRODUCTCLASSNAME,
                        lookupTypeId = data.PRODUCTCLASSTYPEID,
                        lookupTypeName = data.TBL_PRODUCT_CLASS_TYPE.PRODUCTCLASSTYPENAME,
                        businessUnitName = data.BUSINESSUNITID != null ? data.TBL_PROFILE_BUSINESS_UNIT.BUSINESSUNITNAME : null,
                        businessUnitId = data.BUSINESSUNITID,
                    });
        }

        public IEnumerable<LookupViewModel> GetAllProductClass(int customerTypeId, int processId)
        {
            if (customerTypeId == 0)
            {
                customerTypeId = 2;
            }
            return (from data in context.TBL_PRODUCT_CLASS
                    where data.PRODUCT_CLASS_PROCESSID == processId
                    //where data.CUSTOMERTYPEID == customerTypeId && data.PRODUCT_CLASS_PROCESSID == processId
                    //where data.OperationTypeId == operationTypeId
                    select new LookupViewModel()
                    {
                        lookupId = (short)data.PRODUCTCLASSID,
                        lookupName = data.PRODUCTCLASSNAME,
                        lookupTypeId = data.PRODUCTCLASSTYPEID,
                        lookupTypeName = data.TBL_PRODUCT_CLASS_TYPE.PRODUCTCLASSTYPENAME,
                        businessUnitName = data.BUSINESSUNITID != null ? data.TBL_PROFILE_BUSINESS_UNIT.BUSINESSUNITNAME : null,
                        businessUnitId = data.BUSINESSUNITID,

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
                        lookupTypeName = data.TBL_PRODUCT_CLASS_TYPE.PRODUCTCLASSTYPENAME,
                        businessUnitName = data.BUSINESSUNITID != null ? data.TBL_PROFILE_BUSINESS_UNIT.BUSINESSUNITNAME : null,
                        businessUnitId = data.BUSINESSUNITID,

                    }).ToList();
        }

        public IEnumerable<LookupViewModel> GetAllProductClassByCustomerTypeId(int customerTypeId)
        {
            return (from data in context.TBL_PRODUCT_CLASS
                        //where data.CUSTOMERTYPEID == customerTypeId
                        //where data.OperationTypeId == operationTypeId
                    select new LookupViewModel()
                    {
                        lookupId = (short)data.PRODUCTCLASSID,
                        lookupName = data.PRODUCTCLASSNAME,
                        lookupTypeId = data.PRODUCTCLASSTYPEID,
                        lookupTypeName = data.TBL_PRODUCT_CLASS_TYPE.PRODUCTCLASSTYPENAME,
                        businessUnitName = data.BUSINESSUNITID != null ? data.TBL_PROFILE_BUSINESS_UNIT.BUSINESSUNITNAME : null,
                        businessUnitId = data.BUSINESSUNITID,

                    });
        }
        public ProductBehaviourViewModel GetProductBehaviour(int productId)
            {
                ProductBehaviourViewModel data;
                data = context.TBL_PRODUCT_BEHAVIOUR.Where(p => p.PRODUCTID == productId).Select(p => new ProductBehaviourViewModel
                {
                    allowFundaUsage = p.ALLOWFUNDUSAGE,
                    collateralFcyLimit = p.COLLATERAL_FCY_LIMIT ?? 0,
                    customerLimit = p.CUSTOMER_LIMIT,
                    fcyLimit = p.COLLATERAL_FCY_LIMIT,
                    isInvoiceBased = p.ISINVOICEBASED,
                    isTemporaryOverDraft = p.ISTEMPORARYOVERDRAFT,
                    lcyLimit = p.COLLATERAL_LCY_LIMIT,
                    collateralLcyLimit = p.COLLATERAL_LCY_LIMIT,
                    productLimit = p.PRODUCT_LIMIT,
                    invoiceLimit = p.INVOICE_LIMIT,
                    requireCasaAccount = p.REQUIRECASAACCOUNT
                }).FirstOrDefault();
                return data;
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


            public async Task<List<ProductForReturn>> GetAllExternalProductAsync()
            {
                using (FinTrakBankingContext context = new FinTrakBankingContext())
                {
                    var productData = await (from data in context.TBL_PRODUCT

                                             where data.DELETED == false && data.ISEXTERNAL == true
                                             orderby data.PRODUCTNAME ascending
                                             select new ProductForReturn
                                             {
                                                 productId = data.PRODUCTID,
                                                 productCode = data.PRODUCTCODE,
                                                 productName = data.PRODUCTNAME,
                                                 productDescription = data.PRODUCTDESCRIPTION,
                                                 productTenor = data.MAXIMUMTENOR

                                             }).ToListAsync();

                    return productData;
                }
            }

            public List<ProductForReturn> GetAllExternalProduct()
            {
                using (FinTrakBankingContext context = new FinTrakBankingContext())
                {
                    var productData =  (from data in context.TBL_PRODUCT
                                             where data.DELETED == false && data.ISEXTERNAL == true
                                             orderby data.PRODUCTNAME ascending
                                             select new ProductForReturn
                                             {
                                                 productId = data.PRODUCTID,
                                                 productCode = data.PRODUCTCODE,
                                                 productName = data.PRODUCTNAME,
                                                 productDescription = data.PRODUCTDESCRIPTION,
                                                 maxProductTenor = data.MAXIMUMTENOR,
                                                 minProductTenor = data.MINIMUMTENOR,
                                                 maxProductAmount = data.MAXIMUMAMOUNT,
                                                 minProductAmount= data.MINIMUMAMOUNT,

                                             }).ToList();

                    return productData;
                }
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
                    throw new SecureException("Product group already exists!");
                }

                var isProductCodeExist = context.TBL_PRODUCT_GROUP.Any(x =>
                    x.PRODUCTGROUPCODE.ToLower() == productGroupModel.productGroupCode.ToLower());

                if (isProductCodeExist)
                {
                    throw new SecureException("Product group with that code already exists!");
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
                    IPADDRESS = CommonHelpers.GetLocalIpAddress(),
                    URL = productGroupModel.applicationUrl,
                    APPLICATIONDATE = genSetup.GetApplicationDate(),
                    SYSTEMDATETIME = DateTime.Now,
                    DEVICENAME = CommonHelpers.GetDeviceName(),
                    OSNAME = CommonHelpers.FriendlyName(),
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
                    IPADDRESS = CommonHelpers.GetLocalIpAddress(),
                    URL = productGroup.applicationUrl,
                    APPLICATIONDATE = genSetup.GetApplicationDate(),
                    SYSTEMDATETIME = DateTime.Now,
                    DEVICENAME = CommonHelpers.GetDeviceName(),
                    OSNAME = CommonHelpers.FriendlyName(),
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
                    IPADDRESS = CommonHelpers.GetLocalIpAddress(),
                    URL = user.applicationUrl,
                    APPLICATIONDATE = genSetup.GetApplicationDate(),
                    SYSTEMDATETIME = DateTime.Now,
                    TARGETID = productGroupId,
                    DEVICENAME = CommonHelpers.GetDeviceName(),
                    OSNAME = CommonHelpers.FriendlyName(),
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
                    throw new SecureException("Product type already exists!");
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
                    IPADDRESS = CommonHelpers.GetLocalIpAddress(),
                    URL = productType.applicationUrl,
                    APPLICATIONDATE = genSetup.GetApplicationDate(),
                    SYSTEMDATETIME = DateTime.Now,
                    DEVICENAME = CommonHelpers.GetDeviceName(),
                    OSNAME = CommonHelpers.FriendlyName(),
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
                        throw new SecureException("The product group for this product type cannot be changed because the product type is already in use");
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
                    IPADDRESS = CommonHelpers.GetLocalIpAddress(),
                    URL = productType.applicationUrl,
                    APPLICATIONDATE = genSetup.GetApplicationDate(),
                    SYSTEMDATETIME = DateTime.Now,
                    DEVICENAME = CommonHelpers.GetDeviceName(),
                    OSNAME = CommonHelpers.FriendlyName(),
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
                    IPADDRESS = CommonHelpers.GetLocalIpAddress(),
                    URL = user.applicationUrl,
                    APPLICATIONDATE = genSetup.GetApplicationDate(),
                    SYSTEMDATETIME = DateTime.Now,
                    TARGETID = productTypeId,
                    DEVICENAME = CommonHelpers.GetDeviceName(),
                    OSNAME = CommonHelpers.FriendlyName(),
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

            public IEnumerable<ProductViewModel> GetProductsByProductClassProcess(int productClassProcessId)
            {
                var productData = (from p in context.TBL_PRODUCT
                                   //join pc in context.TBL_PRODUCT_CLASS on p.PRODUCTCLASSID equals pc.PRODUCTCLASSID
                                   //join pcp in context.TBL_PRODUCT_CLASS_PROCESS on pc.PRODUCT_CLASS_PROCESSID equals pcp.PRODUCT_CLASS_PROCESSID
                                   where p.TBL_PRODUCT_CLASS.PRODUCT_CLASS_PROCESSID == productClassProcessId
                                   && p.DELETED == false
                                   select new ProductViewModel()
                                   {
                                       productId = p.PRODUCTID,
                                       usedByLos = p.USEDBYLOS,
                                       productName = p.PRODUCTNAME + " " + p.PRODUCTCODE
                                   }).ToList();
                
                return productData;
            }

            private IEnumerable<ProductViewModel> AllProduct()
            {
                var productData = (from data in context.TBL_PRODUCT
                                   join g in context.TBL_PRODUCT_TYPE on data.PRODUCTTYPEID equals g.PRODUCTTYPEID
                                   //where data.PRODUCTCODE !="EBFC"
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
                                       customerTypeId = data.CUSTOMERTYPEID,
                                       riskRatingId = data.RISKRATINGID,
                                       //customerId = data.TBL_PRODUCT_CLASS.CUSTOMERTYPEID,
                                       penalChargeGl = data.PENALCHARGEGL,
                                       penalChargeGlCode = (data.PENALCHARGEGL.HasValue ? data.TBL_CHART_OF_ACCOUNT.ACCOUNTCODE : ""),
                                       isFacilityLine = (data.ISFACILITYLINE.HasValue ? data.ISFACILITYLINE.Value: false),

                                       //customerTypeId = data.TBL_PRODUCT_CLASS.CUSTOMERTYPEID,

                                       productPriceIndexId = data.PRODUCTPRICEINDEXID,
                                       productPriceIndexName = data.TBL_PRODUCT_PRICE_INDEX.PRICEINDEXNAME,
                                       productPriceIndexSpread = data.PRODUCTPRICEINDEXSPREAD,
                                       excludeFromLitigation = data.EXCLUDEFROMLITIGATION,
                                       isPaydayProduct = data.ISPAYDAYPRODUCT,

                                       productCode = data.PRODUCTCODE,
                                       productName = data.PRODUCTNAME + " " + data.PRODUCTCODE,
                                       productDescription = data.PRODUCTDESCRIPTION,

                                       productGroupId = g.PRODUCTGROUPID,

                                       principalBalanceGl = data.PRINCIPALBALANCEGL,
                                       principalBalanceGlCode = (data.PRINCIPALBALANCEGL.HasValue ? data.TBL_CHART_OF_ACCOUNT.ACCOUNTCODE : ""),

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
                                       usedByLos = data.USEDBYLOS,
                                       penalChargeRate = data.PENALCHARGERATE,

                                   }).ToList();
                var Productdata = productData;
                foreach (var item in productData)
                {
                    item.currencies =(from c in context.TBL_PRODUCT_CURRENCY where c.PRODUCTID == item.productId && c.DELETED != false
                                  select new ProductCurrencyViewModel()
                                  {
                                      productCurrencyId = c.PRODUCTCURRENCYID,
                                      currencyId = c.CURRENCYID,
                                      currencyName = c.TBL_CURRENCY.CURRENCYCODE + " -- " + c.TBL_CURRENCY.CURRENCYNAME
                                  }).ToList();
                    item.fees = (from pf in context.TBL_PRODUCT_CHARGE_FEE where pf.PRODUCTID == item.productId select new ProductFeeViewModel()
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
                    item.productBehaviour = (from d in context.TBL_PRODUCT_BEHAVIOUR where d.PRODUCTID == item.productId select new ProductBehaviourViewModel()
                    {
                        crmsRegulatoryId = d.CRMSREGULATORYID,
                        customerLimit = d.CUSTOMER_LIMIT,
                        collateralFcyLimit = d.COLLATERAL_FCY_LIMIT ?? 0,
                        collateralLcyLimit = d.COLLATERAL_LCY_LIMIT ?? 0,
                        productLimit = d.PRODUCT_LIMIT,
                        invoiceLimit = d.INVOICE_LIMIT,
                        isInvoiceBased = d.ISINVOICEBASED,
                        requireCasaAccount = (bool)d.REQUIRECASAACCOUNT,
                        allowFundUsage = d.ALLOWFUNDUSAGE != null ? (bool)d.ALLOWFUNDUSAGE : false,
                        isTemporaryOverDraft = d.ISTEMPORARYOVERDRAFT != null ? (bool)d.ISTEMPORARYOVERDRAFT : false,

                    }).FirstOrDefault();

                }

                //            var productData = (from data in context.TBL_PRODUCT
                //                               select new ProductViewModel()
                //                               {
                //                                   productId = data.PRODUCTID,
                //                                   companyId = data.COMPANYID,
                //                                   productTypeId = data.PRODUCTTYPEID,
                //                                   productTypeName = data.TBL_PRODUCT_TYPE.PRODUCTTYPENAME,
                //                                   productGroupName = data.TBL_PRODUCT_TYPE.TBL_PRODUCT_GROUP.PRODUCTGROUPNAME,
                //                                   productCategoryId = data.PRODUCTCATEGORYID,
                //                                   productCategoryName = data.TBL_PRODUCT_CATEGORY.PRODUCTCATEGORYNAME,
                //                                   productClassId = data.PRODUCTCLASSID,
                //                                   productClassName = data.TBL_PRODUCT_CLASS.PRODUCTCLASSNAME,

                //                                   customerId = data.TBL_PRODUCT_CLASS.CUSTOMERTYPEID,

                //                                   productPriceIndexId = data.PRODUCTPRICEINDEXID,
                //                                   productPriceIndexName = data.TBL_PRODUCT_PRICE_INDEX.PRICEINDEXNAME,
                //                                   productPriceIndexSpread = data.PRODUCTPRICEINDEXSPREAD,

                //                                   productCode = data.PRODUCTCODE,
                //                                   productName = data.PRODUCTNAME,
                //                                   productDescription = data.PRODUCTDESCRIPTION,

                //                                   productGroupId = data.TBL_PRODUCT_TYPE.PRODUCTGROUPID,

                //                                   principalBalanceGl = data.PRINCIPALBALANCEGL,
                //                                   principalBalanceGlCode = (data.PRINCIPALBALANCEGL.HasValue ? data.TBL_CHART_OF_ACCOUNT.ACCOUNTCODE : ""),

                //                                   principalBalanceGl2 = data.PRINCIPALBALANCEGL2,
                //                                   principalBalanceGl2Code = (data.PRINCIPALBALANCEGL2.HasValue ? data.TBL_CHART_OF_ACCOUNT.ACCOUNTCODE : ""),

                //                                   interestIncomeExpenseGl = data.INTERESTINCOMEEXPENSEGL,
                //                                   interestIncomeExpenseGlCode = (data.INTERESTINCOMEEXPENSEGL.HasValue ? data.TBL_CHART_OF_ACCOUNT.ACCOUNTCODE : ""),

                //                                   interestReceivablePayableGl = data.INTERESTRECEIVABLEPAYABLEGL,
                //                                   interestReceivablePayableGlCode = (data.INTERESTRECEIVABLEPAYABLEGL.HasValue ? data.TBL_CHART_OF_ACCOUNT.ACCOUNTCODE : ""),

                //                                   dormantGl = data.DORMANTGL,
                //                                   premiumDiscountGl = data.PREMIUMDISCOUNTGL,

                //                                   dealTypeId = data.DEALTYPEID,
                //                                   dealTypeName = data.TBL_DEAL_TYPE.DEALTYPENAME,
                //                                   dealClassificationId = data.DEALCLASSIFICATIONID,
                //                                   dealClassificationName = data.TBL_DEAL_CLASSIFICATION.CLASSIFICATION,
                //                                   dayCountId = data.DAYCOUNTCONVENTIONID,
                //                                   dayCountName = data.TBL_DAY_COUNT_CONVENTION.DAYCOUNTCONVENTIONNAME,

                //                                   maximumTenor = data.MAXIMUMTENOR,
                //                                   minimumTenor = data.MINIMUMTENOR,
                //                                   maximumRate = data.MAXIMUMRATE,
                //                                   minimumRate = data.MINIMUMRATE,
                //                                   minimumBalance = data.MINIMUMBALANCE,
                //                                   approvedBy = data.APPROVEDBY,
                //                                   completed = data.COMPLETED,
                //                                   approved = data.APPROVED,



                //                                   dateTimeUpdated = data.DATETIMEUPDATED,
                //                                   deleted = data.DELETED,
                //                                   deletedBy = data.DELETEDBY,
                //                                   dateTimeDeleted = data.DATETIMEDELETED,

                //                                   allowCustomerAccountForceDebit = data.ALLOWCUSTOMERACCOUNTFORCEDEBIT,
                //                                   allowMoratorium = data.ALLOWMORATORIUM,
                //                                   allowScheduleTypeOverride = data.ALLOWSCHEDULETYPEOVERRIDE,
                //                                   allowTenor = data.ALLOWTENOR,
                //                                   allowRate = data.ALLOWOVERDRAWN,
                //                                   allowOverdrawn = data.ALLOWOVERDRAWN,

                //                                   cleanupPeriod = data.CLEANUPPERIOD,
                //                                   defaultGracePeriod = data.DEFAULTGRACEPERIOD,
                //                                   equityContribution = data.EQUITYCONTRIBUTION,
                //                                   expiryPeriod = data.EXPIRYPERIOD,
                //                                   scheduleTypeId = data.SCHEDULETYPEID,


                //                               });
                //foreach (var item in productData)
                //            {
                //                item.currencies = context.TBL_PRODUCT_CURRENCY.Where(curr => curr.PRODUCTID == item.productId && curr.DELETED != false)
                //                              .Select(c => new ProductCurrencyViewModel()
                //                              {
                //                                  productCurrencyId = c.PRODUCTCURRENCYID,
                //                                  currencyId = c.CURRENCYID,
                //                                  currencyName = c.TBL_CURRENCY.CURRENCYCODE + " -- " + c.TBL_CURRENCY.CURRENCYNAME
                //                              }).ToList();

                //                item.ProductBehaviour = context.TBL_PRODUCT_BEHAVIOUR.Where(d => d.PRODUCTID == item.productId).Select(d => new ProductBehaviourViewModel()
                //                {
                //                    customerLimit = d.CUSTOMER_LIMIT,
                //                    collateralFcyLimit = d.COLLATERAL_FCY_LIMIT ?? 0,
                //                    collateralLcyLimit = d.COLLATERAL_LCY_LIMIT ?? 0,
                //                    productLimit = d.PRODUCT_LIMIT,
                //                    isInvoiceBased = d.ISINVOICEBASED,
                //                    requireCasaAccount = (bool)d.REQUIRECASAACCOUNT,
                //                    allowFundUsage = d.ALLOWFUNDUSAGE != null ? (bool)d.ALLOWFUNDUSAGE : false,
                //                    isTemporaryOverDraft = d.ISTEMPORARYOVERDRAFT != null ? (bool)d.ISTEMPORARYOVERDRAFT : false,

                //                }).FirstOrDefault();

                //            }

                return Productdata;
            }

        public IEnumerable<ProductViewModel> Products()
        {
            var productData = (from data in context.TBL_PRODUCT
                               select new ProductViewModel()
                               {
                                   productId = data.PRODUCTID,
                                   productName = data.PRODUCTNAME,
                                   productTypeName = data.PRODUCTNAME,
                                   productTypeId = data.PRODUCTTYPEID

                               }).ToList();
           

            return productData;
        }


        public IEnumerable<ProductLiteViewModel> GetAllProductLite()
            {
                var productData = (from data in context.TBL_PRODUCT
                                   join g in context.TBL_PRODUCT_TYPE on data.PRODUCTTYPEID equals g.PRODUCTTYPEID
                                   orderby data.PRODUCTNAME ascending
                                   select new ProductLiteViewModel()
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

                                       productCode = data.PRODUCTCODE,
                                       productName = data.PRODUCTNAME,
                                       productGroupId = g.PRODUCTGROUPID,

                                       dateTimeUpdated = data.DATETIMEUPDATED,
                                       deleted = data.DELETED,
                                       deletedBy = data.DELETEDBY,
                                       dateTimeDeleted = data.DATETIMEDELETED,
                                   });

                return productData.ToList();
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
                        invoiceLimit = c.INVOICE_LIMIT,
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



            public IEnumerable<ProductViewModel> GetAllProductByProductClassAndCustomerType(int productClassId, int customerTypeId)
            {
                List<ProductViewModel> product = new List<ProductViewModel>();

                if (customerTypeId == 0) customerTypeId = 3;

                product = AllProduct().Where(c =>
                        c.productClassId == (short?)productClassId
                        //&& (c.customerTypeId == (short)customerTypeId || c.customerTypeId == 3)
                        && (c.productGroupId == 1)
                    ).ToList();

                foreach (var item in product)
                {
                    var ProductBehaviour = context.TBL_PRODUCT_BEHAVIOUR.Where(d => d.PRODUCTID == item.productId).Select(d => new ProductBehaviourViewModel()
                    {
                        customerLimit = d.CUSTOMER_LIMIT,
                        collateralFcyLimit = d.COLLATERAL_FCY_LIMIT ?? 0,
                        collateralLcyLimit = d.COLLATERAL_LCY_LIMIT ?? 0,
                        productLimit = d.PRODUCT_LIMIT,
                        invoiceLimit = d.INVOICE_LIMIT,
                        isInvoiceBased = d.ISINVOICEBASED

                    }).FirstOrDefault();

                    item.productBehaviour = ProductBehaviour;


                    var currencies = context.TBL_PRODUCT_CURRENCY.Where(curr => curr.PRODUCTID == item.productId && curr.DELETED != false)
                                    .Select(c => new ProductCurrencyViewModel()
                                    {
                                        productId = c.PRODUCTID,
                                        productCurrencyId = c.PRODUCTCURRENCYID,
                                        currencyId = c.CURRENCYID,
                                        currencyName = c.TBL_CURRENCY.CURRENCYCODE + " -- " + c.TBL_CURRENCY.CURRENCYNAME
                                    }).ToList();
                    item.currencies = currencies;
                }


                return product.OrderBy(p => p.productName);

                //return AllProduct().Where(c => c.productClassId == productClassId && (c.productGroupId == 1));
            }


            public IEnumerable<ProductViewModel> GetAllProductByProductClass(int productClassId, int customerTypeId)
            {
                if (customerTypeId == 0)
                {
                    customerTypeId = 2;
                }
                //var productData = AllProduct().Where(c => c.productClassId == productClassId && (c.productGroupId == 1));
                var product = AllProduct().Where(c => (c.productClassId == productClassId) && (c.productGroupId == 1)).ToList();
                foreach (var item in product)
                {
                    var ProductBehaviour = context.TBL_PRODUCT_BEHAVIOUR.Where(d => d.PRODUCTID == item.productId).Select(d => new ProductBehaviourViewModel()
                    {
                        customerLimit = d.CUSTOMER_LIMIT,
                        collateralFcyLimit = d.COLLATERAL_FCY_LIMIT ?? 0,
                        collateralLcyLimit = d.COLLATERAL_LCY_LIMIT ?? 0,
                        productLimit = d.PRODUCT_LIMIT,
                        invoiceLimit = d.INVOICE_LIMIT,
                        isInvoiceBased = d.ISINVOICEBASED

                    }).FirstOrDefault();

                    item.productBehaviour = ProductBehaviour;


                    var currencies = context.TBL_PRODUCT_CURRENCY.Where(curr => curr.PRODUCTID == item.productId && curr.DELETED != false)
                                    .Select(c => new ProductCurrencyViewModel()
                                    {
                                        productId = c.PRODUCTID,
                                        productCurrencyId = c.PRODUCTCURRENCYID,
                                        currencyId = c.CURRENCYID,
                                        currencyName = c.TBL_CURRENCY.CURRENCYCODE + " -- " + c.TBL_CURRENCY.CURRENCYNAME
                                    }).ToList();
                    item.currencies = currencies;
                }


                return product;

                //return AllProduct().Where(c => c.productClassId == productClassId && (c.productGroupId == 1));
            }

            public IEnumerable<ProductViewModel> GetAllProductsByProductClassIdAndCustomerTypeId(int productClassId, int customerTypeId)
            {
                var product = AllProduct().Where(c => ((c.productClassId == productClassId) && (c.customerTypeId == (short)customerTypeId || c.customerTypeId == 3) && (c.productGroupId == 1))).ToList();
                foreach (var item in product)
                {
                    var ProductBehaviour = context.TBL_PRODUCT_BEHAVIOUR.Where(d => d.PRODUCTID == item.productId).Select(d => new ProductBehaviourViewModel()
                    {
                        customerLimit = d.CUSTOMER_LIMIT,
                        collateralFcyLimit = d.COLLATERAL_FCY_LIMIT ?? 0,
                        collateralLcyLimit = d.COLLATERAL_LCY_LIMIT ?? 0,
                        productLimit = d.PRODUCT_LIMIT,
                        invoiceLimit = d.INVOICE_LIMIT,
                        isInvoiceBased = d.ISINVOICEBASED

                    }).FirstOrDefault();

                    item.productBehaviour = ProductBehaviour;


                    var currencies = context.TBL_PRODUCT_CURRENCY.Where(curr => curr.PRODUCTID == item.productId && curr.DELETED != false)
                                    .Select(c => new ProductCurrencyViewModel()
                                    {
                                        productId = c.PRODUCTID,
                                        productCurrencyId = c.PRODUCTCURRENCYID,
                                        currencyId = c.CURRENCYID,
                                        currencyName = c.TBL_CURRENCY.CURRENCYCODE + " -- " + c.TBL_CURRENCY.CURRENCYNAME
                                    }).ToList();
                    item.currencies = currencies;
                }
                return product;
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
                                                  join atrail in context.TBL_APPROVAL_TRAIL on c.TEMP_PRODUCTID equals atrail.TARGETID
                                                  where (atrail.APPROVALSTATUSID == (int)ApprovalStatusEnum.Pending
                                                        || atrail.APPROVALSTATUSID == (int)ApprovalStatusEnum.Processing) 
                                                        && c.ISCURRENT == true
                                                        && atrail.RESPONSESTAFFID == null
                                                        && atrail.OPERATIONID == (int)OperationsEnum.ProductCreation
                                                                                      //&& atrail.TOAPPROVALLEVELID == staffApprovalLevelId
                                                                                      && ids.Contains((int)atrail.TOAPPROVALLEVELID)

                                                  select new ProductViewModel()
                                                  {
                                                      productId = c.TEMP_PRODUCTID,
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
                                                      excludeFromLitigation = c.EXCLUDEFROMLITIGATION,
                                                      isPaydayProduct = c.ISPAYDAYPRODUCT,

                                                      productCode = c.PRODUCTCODE,
                                                      productName = c.PRODUCTNAME,
                                                      productDescription = c.PRODUCTDESCRIPTION,

                                                      productGroupId = c.TBL_PRODUCT_TYPE.PRODUCTGROUPID,
                                                      productGroupName = c.TBL_PRODUCT_TYPE.TBL_PRODUCT_GROUP.PRODUCTGROUPNAME,

                                                      principalBalanceGl = c.PRINCIPALBALANCEGL,
                                                      principalBalanceGlCode = (c.PRINCIPALBALANCEGL.HasValue ? c.TBL_CHART_OF_ACCOUNT.ACCOUNTCODE : ""),

                                                      principalBalanceGl2 = c.PRINCIPALBALANCEGL2,
                                                      principalBalanceGl2Code = (c.PRINCIPALBALANCEGL2.HasValue ? c.TBL_CHART_OF_ACCOUNT.ACCOUNTCODE : ""),
                                                      penalChargeGl = c.PENALCHARGEGL,
                                                      penalChargeGlCode = (c.PENALCHARGEGL.HasValue ? c.TBL_CHART_OF_ACCOUNT.ACCOUNTCODE : ""),

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
                                                      riskRatingId = c.RISKRATINGID,
                                                      riskRatingName = context.TBL_CUSTOMER_RISK_RATING.Where(x => x.RISKRATINGID == c.RISKRATINGID).Select(x => x.RISKRATING).FirstOrDefault(),
                                                      //approvalStatusId = c.APPROVALSTATUSID,
                                                      operationId = atrail.OPERATIONID,
                                                      //currencies = context.TBL_TEMP_PRODUCT_CURRENCY.Where(curr => curr.PRODUCTID == c.PRODUCTID && curr.DELETED == false).Any() ? context.TBL_TEMP_PRODUCT_CURRENCY.Where(curr => curr.PRODUCTID == c.PRODUCTID && curr.DELETED == false).Select(pc => new ProductCurrencyViewModel()
                                                      //{
                                                      //    productId = c.PRODUCTID,
                                                      //    productCurrencyId = pc.PRODUCTCURRENCYID,
                                                      //    currencyId = pc.CURRENCYID,
                                                      //    currencyName = pc.TBL_CURRENCY.CURRENCYCODE + " -- " + pc.TBL_CURRENCY.CURRENCYNAME
                                                      //}).ToList() : null,
                                                      //fees = context.TBL_TEMP_PRODUCT_CHARGE_FEE.Where(curr => curr.TEMP_PRODUCTID == c.TEMP_PRODUCTID && c.DELETED == false).Select(pf => new ProductFeeViewModel()
                                                      //{
                                                      //    productFeeId = pf.PRODUCTFEEID,
                                                      //    productId = pf.TEMP_PRODUCTID,
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
                                                      collaterals = context.TBL_TEMP_PRODUCT_COLLATERALTYP.Where(coll => coll.TEMP_PRODUCTID == c.TEMP_PRODUCTID && coll.DELETED == false).Select(prodColl => new ProductCollateralTypeViewModel()
                                                      {
                                                          productId = prodColl.TEMP_PRODUCTID,
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


                    List<ProductFeeViewModel> fees = context.TBL_TEMP_PRODUCT_CHARGE_FEE.Where(curr => curr.DELETED == false).Select(pf => new ProductFeeViewModel()
                    {
                        productId = pf.TEMP_PRODUCTID,
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

                    var pendingProducts = pendingProductsChanges.ToList();

                    foreach (var item in pendingProducts)
                    {
                        var behaviour = context.TBL_TEMP_PRODUCT_BEHAVIOUR.Where(x => x.TEMP_PRODUCTID == item.productId).ToList();
                        if (behaviour.Any())
                        {
                            var x = behaviour.FirstOrDefault();
                            //behaviour.COLLATERAL_LCY_LIMIT = item.collateralLCYLimit;
                            //behaviour.COLLATERAL_FCY_LIMIT = item.collateralFCYLimit;
                            //behaviour.CUSTOMER_LIMIT = item.customerLimit;
                            //behaviour.PRODUCT_LIMIT = item.productLimit;
                            //behaviour.ISINVOICEBASED = item.invoiceBased;
                            //behaviour.REQUIRECASAACCOUNT = item.requireCasaAccount;
                            //behaviour.ALLOWFUNDUSAGE = item.allowFundUsage;
                            //behaviour.CRMSREGULATORYID = item.ProductBehaviour.crmsRegulatoryId;


                            item.collateralLCYLimit = x.COLLATERAL_LCY_LIMIT;
                            item.collateralFCYLimit = x.COLLATERAL_FCY_LIMIT;
                            item.customerLimit = x.CUSTOMER_LIMIT;
                            item.productLimit = x.PRODUCT_LIMIT;
                            item.invoiceLimit = x.INVOICE_LIMIT;
                            item.invoiceBased = x.ISINVOICEBASED;
                            item.requireCasaAccount = x.REQUIRECASAACCOUNT;
                            item.allowFundUsage = x.ALLOWFUNDUSAGE;
                            item.crmsRegulatoryId = x.CRMSREGULATORYID;
                            item.productBehaviourId = x.TEMPPRODUCT_BEHAVIOURID;
                            //item.productBehaviour.crmsRegulatoryId = x.CRMSREGULATORYID;
                        }
                    }
                    foreach (var currency in pendingProducts)
                    {
                        var currencies = context.TBL_TEMP_PRODUCT_CURRENCY.Where(curr => curr.TEMP_PRODUCTID == currency.productId && curr.DELETED == false).Select(pc => new ProductCurrencyViewModel()
                        {
                            productId = pc.TEMP_PRODUCTID,
                            productCurrencyId = pc.PRODUCTCURRENCYID,
                            currencyId = pc.CURRENCYID,
                            currencyName = pc.TBL_CURRENCY.CURRENCYCODE + " -- " + pc.TBL_CURRENCY.CURRENCYNAME
                        }).ToList();


                        if (currencies != null) currency.currencies = currencies;
                    }
                    foreach (var item in pendingProducts)
                    {
                        List<ProductFeeViewModel> values = fees.Where(curr => curr.productId == item.productId).ToList();
                        item.fees = values;
                        //var f= pendingProductsChanges.ToList();


                    }

                    var a = pendingProductsChanges;
                    var b = pendingProducts;

                    return pendingProducts.Where(x => x.approvalStatusId == (int)ApprovalStatusEnum.Pending).ToList();

                }
                catch (Exception ex)
                {
                    throw new SecureException("" + ex);
                }


            }
            public IEnumerable<LookupViewModel> GetAllCRMSType(int companyId)
            {
                return context.TBL_CRMS_REGULATORY.Where(x => x.CRMSTYPEID == (int)RegulatoryTypeEnum.LoanType && x.COMPANYID == companyId).Select(x => new LookupViewModel()
                {
                    lookupId = (short)x.CRMSREGULATORYID,
                    lookupName = x.CODE + "-" + x.DESCRIPTION
                }).ToList();
            }
            public IEnumerable<LookupViewModel> GetAllRiskRatingType(int companyId)
            {
                return context.TBL_CUSTOMER_RISK_RATING.Where(x => x.COMPANYID == companyId).Select(x => new LookupViewModel()
                {
                    lookupId = (short)x.RISKRATINGID,
                    lookupName = x.RISKRATING
                }).ToList();
            }
            public ProductViewModel GetTempProductDetail(int productId)
            {
                //return GetTempStaffDetails().Where(x => x.StaffId == staffId).Single();

                var productData = (from tp in context.TBL_TEMP_PRODUCT
                                   join coy in context.TBL_COMPANY on tp.COMPANYID equals coy.COMPANYID
                                   where tp.TEMP_PRODUCTID == productId
                                   select new ProductViewModel()
                                   {
                                       productId = tp.TEMP_PRODUCTID,
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
                                       excludeFromLitigation = tp.EXCLUDEFROMLITIGATION,
                                       isPaydayProduct = tp.ISPAYDAYPRODUCT,

                                       productGroupId = tp.TBL_PRODUCT_TYPE.PRODUCTGROUPID,

                                       principalBalanceGl = tp.PRINCIPALBALANCEGL,
                                       principalBalanceGlCode = (tp.PRINCIPALBALANCEGL.HasValue ? tp.TBL_CHART_OF_ACCOUNT.ACCOUNTCODE : ""),

                                       principalBalanceGl2 = tp.PRINCIPALBALANCEGL2,
                                       principalBalanceGl2Code = (tp.PRINCIPALBALANCEGL2.HasValue ? context.TBL_CHART_OF_ACCOUNT.Find(tp.PRINCIPALBALANCEGL2).ACCOUNTCODE : ""),
                                       penalChargeGl = tp.PENALCHARGEGL,
                                       penalChargeGlCode = (tp.PENALCHARGEGL.HasValue ? context.TBL_CHART_OF_ACCOUNT.Find(tp.PENALCHARGEGL).ACCOUNTCODE : ""),


                                       riskRatingId = tp.RISKRATINGID,

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

                                       collaterals = context.TBL_TEMP_PRODUCT_COLLATERALTYP.Where(curr => curr.TEMP_PRODUCTID == tp.TEMP_PRODUCTID && curr.DELETED != false).Select(pcc => new ProductCollateralTypeViewModel()
                                       {
                                           productId = pcc.TEMP_PRODUCTID,
                                           productCollateralId = pcc.PRODUCTCOLLATERALTYPEID,
                                           collateralTypeId = pcc.COLLATERALTYPEID,
                                           collateralTypeName = pcc.TBL_COLLATERAL_TYPE.COLLATERALTYPENAME
                                       }).ToList(),
                                       productBehaviour = context.TBL_TEMP_PRODUCT_BEHAVIOUR.Where(d => d.PRODUCTCODE == tp.PRODUCTCODE).Select(d => new ProductBehaviourViewModel()
                                       {
                                           customerLimit = d.CUSTOMER_LIMIT,
                                           collateralFcyLimit = d.COLLATERAL_FCY_LIMIT ?? 0,
                                           collateralLcyLimit = d.COLLATERAL_LCY_LIMIT ?? 0,
                                           productLimit = d.PRODUCT_LIMIT,
                                           invoiceLimit = d.INVOICE_LIMIT,
                                           isInvoiceBased = d.ISINVOICEBASED,
                                           allowFundUsage = d.ALLOWFUNDUSAGE != null ? (bool)d.ALLOWFUNDUSAGE : false,
                                           crmsRegulatoryId = d.CRMSREGULATORYID,

                                       }).FirstOrDefault(),

                                       dateTimeUpdated = tp.DATETIMEUPDATED,
                                       deleted = tp.DELETED,
                                       deletedBy = tp.DELETEDBY,
                                       dateTimeDeleted = tp.DATETIMEDELETED
                                   }).FirstOrDefault();


                var currencies = context.TBL_TEMP_PRODUCT_CURRENCY.Where(curr => curr.TEMP_PRODUCTID == productData.productId && curr.DELETED != false).Select(pc => new ProductCurrencyViewModel()
                {
                    productId = pc.TEMP_PRODUCTID,
                    productCurrencyId = pc.PRODUCTCURRENCYID,
                    currencyId = pc.CURRENCYID,
                    currencyName = pc.TBL_CURRENCY.CURRENCYCODE + " -- " + pc.TBL_CURRENCY.CURRENCYNAME
                }).ToList();

                var fees = context.TBL_TEMP_PRODUCT_CHARGE_FEE.Where(curr => curr.TEMP_PRODUCTID == productData.productId && curr.DELETED != false).Select(pf => new ProductFeeViewModel()
                {
                    productId = pf.TEMP_PRODUCTID,
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
                        // workFlow.LogForApproval(entity);
                        // var b = workFlow.NextLevelId ?? 0;

                        workFlow.StaffId = entity.staffId;
                        workFlow.CompanyId = entity.companyId;
                        workFlow.StatusId = ((int)entity.approvalStatusId == (int)ApprovalStatusEnum.Approved) ? (int)ApprovalStatusEnum.Processing : (int)entity.approvalStatusId;
                        workFlow.TargetId = entity.targetId;
                        workFlow.Comment = entity.comment;
                        workFlow.OperationId = entity.operationId;
                        workFlow.DeferredExecution = true;
                        workFlow.ExternalInitialization = false;

                        workFlow.LogActivity();

                        //context.savechanges();
                        //if (b == 0 && workFlow.NewState != (int)ApprovalState.Ended) // check if this is the last level
                        //{
                        //    trans.Rollback();
                        //    throw new SecureException("Approval Failed");
                        //}

                        if (entity.approvalStatusId == (short)ApprovalStatusEnum.Disapproved)
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
                                try
                                {
                                    context.SaveChanges();
                                    trans.Commit();
                                }
                                catch (Exception ex)
                                {
                                    var EXE = ex;
                                }
                            }
                            else throw new ConditionNotMetException("One or more errors occured on commit.");
                            return 1;
                        }
                        else
                        {
                            context.SaveChanges();
                            trans.Commit();
                        }

                        return 0;
                    }
                    catch (Exception ex)
                    {
                        trans.Rollback();
                        throw new SecureException(ex.Message);
                    }
                }
            }

            private bool ApproveProduct(int productId, short approvalStatusId, UserInfo user)
            {
                var productModel = context.TBL_TEMP_PRODUCT.Find(productId);
                var productBehaviourModel = context.TBL_TEMP_PRODUCT_BEHAVIOUR.Where(x => x.TEMP_PRODUCTID == productModel.TEMP_PRODUCTID).FirstOrDefault();

                var productToUpdate = context.TBL_PRODUCT.Where(x => x.PRODUCTID == productModel.PRODUCTID).FirstOrDefault();
                var productBehaviourToUpdate = context.TBL_PRODUCT_BEHAVIOUR.FirstOrDefault(x => x.PRODUCTID == productModel.PRODUCTID);

                var currModel = context.TBL_TEMP_PRODUCT_CURRENCY.Where(c => c.TEMP_PRODUCTID == productModel.TEMP_PRODUCTID && c.DELETED == false).ToList();
                var currListToUpdate = new List<TBL_PRODUCT_CURRENCY>();

                var feeModel = context.TBL_TEMP_PRODUCT_CHARGE_FEE.Where(c => c.TEMP_PRODUCTID == productModel.TEMP_PRODUCTID && c.DELETED == false);
                var feeListToUpdate = new List<TBL_PRODUCT_CHARGE_FEE>();

                var collateralModel = context.TBL_TEMP_PRODUCT_COLLATERALTYP.Where(c => c.TEMP_PRODUCTID == productModel.TEMP_PRODUCTID && c.DELETED == false);
                var collateralListToUpdate = new List<TBL_PRODUCT_COLLATERALTYPE>();

                List<TBL_PRODUCT_CHARGE_FEE> productFees = new List<TBL_PRODUCT_CHARGE_FEE>();
                List<TBL_PRODUCT_COLLATERALTYPE> productCollateral = new List<TBL_PRODUCT_COLLATERALTYPE>();
                List<TBL_PRODUCT_CURRENCY> productCurrencies = new List<TBL_PRODUCT_CURRENCY>();
                try
                {
                    if (productToUpdate != null)
                    {
                        currListToUpdate = context.TBL_PRODUCT_CURRENCY.Where(x => x.PRODUCTID == productToUpdate.PRODUCTID && x.DELETED == false).ToList();

                        feeListToUpdate = context.TBL_PRODUCT_CHARGE_FEE.Where(x => x.PRODUCTID == productToUpdate.PRODUCTID && x.DELETED == false).ToList();

                        collateralListToUpdate = context.TBL_PRODUCT_COLLATERALTYPE.Where(x => x.PRODUCTID == productToUpdate.PRODUCTID && x.DELETED == false).ToList();

                       if (currListToUpdate != null)
                        {
                            foreach (var curr in currListToUpdate)
                            {
                                context.TBL_PRODUCT_CURRENCY.Remove(curr);
                            }
                        }

                        if (feeListToUpdate != null)
                        {
                            foreach (var item in feeListToUpdate)
                            {
                                context.TBL_PRODUCT_CHARGE_FEE.Remove(item);
                            }
                        }

                        if (collateralListToUpdate != null)
                        {
                            foreach (var item in collateralListToUpdate)
                            {
                                context.TBL_PRODUCT_COLLATERALTYPE.Remove(item);
                            }
                        }

                        // Insert updated records for currencies
                        if (currModel.Count() > 0)
                        {
                            foreach (var c in currModel.ToList())
                            {
                                var curr = new TBL_PRODUCT_CURRENCY()
                                {
                                    PRODUCTID = (short)productModel.PRODUCTID,
                                    CURRENCYID = c.CURRENCYID,
                                    CREATEDBY = c.CREATEDBY,
                                    DATETIMECREATED = genSetup.GetApplicationDate(),
                                };
                                productCurrencies.Add(curr);
                            }
                        }

                        if (feeModel.Count() > 0)
                        {
                            foreach (var item in feeModel.ToList())
                            {
                                var feeList = new TBL_PRODUCT_CHARGE_FEE()
                                {
                                    PRODUCTID = (short)productModel.PRODUCTID,
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
                        }

                        if (collateralModel.Count() > 0)
                        {
                            foreach (var item in collateralModel.ToList())
                            {
                                var productCollaterals = new TBL_PRODUCT_COLLATERALTYPE()
                                {
                                    PRODUCTID = (short)productModel.PRODUCTID,
                                    //PRODUCTCOLLATERALTYPEID = item.PRODUCTCOLLATERALTYPEID,
                                    COLLATERALTYPEID = item.COLLATERALTYPEID,
                                    COMPANYID = item.COMPANYID,
                                    CREATEDBY = item.CREATEDBY,
                                    DATETIMECREATED = genSetup.GetApplicationDate(),
                                    DELETED = false,

                                };
                                productCollateral.Add(productCollaterals);

                            }
                        }

                        var existingProduct = productToUpdate;
                        if (productModel != null)
                        {

                            productModel.APPROVALSTATUSID = approvalStatusId;
                            existingProduct.PRODUCTCLASSID = productModel.PRODUCTCLASSID;
                            existingProduct.CUSTOMERTYPEID = productModel.CUSTOMERTYPEID;
                            //existingProduct.PRODUCTCODE = productModel.PRODUCTCODE;
                            existingProduct.PRODUCTNAME = productModel.PRODUCTNAME;
                            existingProduct.PRODUCTDESCRIPTION = productModel.PRODUCTDESCRIPTION;
                            existingProduct.RISKRATINGID = productModel.RISKRATINGID;
                            existingProduct.PRINCIPALBALANCEGL = productModel.PRINCIPALBALANCEGL;
                            existingProduct.PRINCIPALBALANCEGL2 = productModel.PRINCIPALBALANCEGL2;
                            existingProduct.PENALCHARGEGL = productModel.PENALCHARGEGL;
                            existingProduct.PENALCHARGERATE = productModel.PENALCHARGERATE;
                            existingProduct.USEDBYLOS = productModel.USEDBYLOS;
                            existingProduct.INTERESTINCOMEEXPENSEGL = productModel.INTERESTINCOMEEXPENSEGL;
                            existingProduct.INTERESTRECEIVABLEPAYABLEGL = productModel.INTERESTRECEIVABLEPAYABLEGL;
                            existingProduct.DORMANTGL = productModel.DORMANTGL;
                            existingProduct.PREMIUMDISCOUNTGL = productModel.PREMIUMDISCOUNTGL;
                            existingProduct.OVERDRAWNGL = productModel.OVERDRAWNGL;
                            existingProduct.ISFACILITYLINE = productModel.ISFACILITYLINE;
                            existingProduct.EXCLUDEFROMLITIGATION = productModel.EXCLUDEFROMLITIGATION;
                            existingProduct.ISPAYDAYPRODUCT = productModel.ISPAYDAYPRODUCT;

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
                                existingProductBehaviour.CRMSREGULATORYID = productBehaviourModel.CRMSREGULATORYID;
                                existingProductBehaviour.PRODUCTID = existingProduct.PRODUCTID;
                                existingProductBehaviour.PRODUCT_LIMIT = productBehaviourModel.PRODUCT_LIMIT;
                                existingProductBehaviour.INVOICE_LIMIT = productBehaviourModel.INVOICE_LIMIT;
                                existingProductBehaviour.CUSTOMER_LIMIT = productBehaviourModel.CUSTOMER_LIMIT;
                                existingProductBehaviour.COLLATERAL_FCY_LIMIT = productBehaviourModel.COLLATERAL_FCY_LIMIT;
                                existingProductBehaviour.COLLATERAL_LCY_LIMIT = productBehaviourModel.COLLATERAL_LCY_LIMIT;
                                existingProductBehaviour.ISINVOICEBASED = productBehaviourModel.ISINVOICEBASED;
                                existingProductBehaviour.ALLOWFUNDUSAGE = productBehaviourModel.ALLOWFUNDUSAGE;
                                productBehaviourModel.APPROVALSTATUSID = approvalStatusId;

                            }

                        }

                        //context.Entry(existingProduct).State = System.Data.Entity.EntityState.Modified;
                    }
                    else
                    {
                        if (currModel.Count() > 0)
                        {
                            foreach (var c in currModel.ToList())
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
                        }

                        if (collateralModel.Count() > 0)
                        {
                            foreach (var item in collateralModel.ToList())
                            {
                                var productCollaterals = new TBL_PRODUCT_COLLATERALTYPE()
                                {
                                    PRODUCTID = item.TEMP_PRODUCTID,
                                    //PRODUCTCOLLATERALTYPEID = item.PRODUCTCOLLATERALTYPEID,
                                    COLLATERALTYPEID = item.COLLATERALTYPEID,
                                    COMPANYID = item.COMPANYID,
                                    CREATEDBY = item.CREATEDBY,
                                    DATETIMECREATED = genSetup.GetApplicationDate(),
                                    DELETED = false,
                                };
                                productCollateral.Add(productCollaterals);
                            }
                        }

                        if (feeModel.Count() > 0)
                        {
                            foreach (var item in feeModel.ToList())
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
                                //product.TBL_PRODUCT_CHARGE_FEE.Add(feeList);

                                productFees.Add(feeList);
                            }
                        }

                        if (productModel != null)
                        {
                            var product = new TBL_PRODUCT()
                            {
                                COMPANYID = productModel.COMPANYID,
                                PRODUCTTYPEID = productModel.PRODUCTTYPEID,
                                //CUSTOMERTYPEID = productModel.CUSTOMERTYPEID,
                                PRODUCTCATEGORYID = productModel.PRODUCTCATEGORYID,
                                PRODUCTCLASSID = productModel.PRODUCTCLASSID,
                                PRODUCTCODE = productModel.PRODUCTCODE,
                                PRODUCTNAME = productModel.PRODUCTNAME,
                                PRODUCTDESCRIPTION = productModel.PRODUCTDESCRIPTION,
                                ISFACILITYLINE = productModel.ISFACILITYLINE,
                                PRINCIPALBALANCEGL = productModel.PRINCIPALBALANCEGL,
                                PRINCIPALBALANCEGL2 = productModel.PRINCIPALBALANCEGL2,
                                PENALCHARGEGL = productModel.PENALCHARGEGL,
                                PENALCHARGERATE = productModel.PENALCHARGERATE,
                                USEDBYLOS = productModel.USEDBYLOS,
                                INTERESTINCOMEEXPENSEGL = productModel.INTERESTINCOMEEXPENSEGL,
                                INTERESTRECEIVABLEPAYABLEGL = productModel.INTERESTRECEIVABLEPAYABLEGL,
                                DORMANTGL = productModel.DORMANTGL,
                                PREMIUMDISCOUNTGL = productModel.PREMIUMDISCOUNTGL,
                                OVERDRAWNGL = productModel.OVERDRAWNGL,
                                RISKRATINGID = productModel.RISKRATINGID,
                                PRODUCTPRICEINDEXID = productModel.PRODUCTPRICEINDEXID,
                                PRODUCTPRICEINDEXSPREAD = productModel.PRODUCTPRICEINDEXSPREAD,
                                EXCLUDEFROMLITIGATION = productModel.EXCLUDEFROMLITIGATION,
                                ISPAYDAYPRODUCT = productModel.ISPAYDAYPRODUCT,

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
                            productModel.APPROVALSTATUSID = approvalStatusId;
                            productModel.ISCURRENT = false;
                            context.TBL_PRODUCT.Add(product);

                            if (productBehaviourModel != null)
                            {

                                TBL_PRODUCT_BEHAVIOUR productBehaviour = new TBL_PRODUCT_BEHAVIOUR()
                                {
                                    CRMSREGULATORYID = productBehaviourModel.CRMSREGULATORYID,
                                    //PRODUCTID = productModel.TEMP_PRODUCTID,
                                    ISINVOICEBASED = productBehaviourModel.ISINVOICEBASED,
                                    COLLATERAL_LCY_LIMIT = productBehaviourModel.COLLATERAL_LCY_LIMIT,
                                    COLLATERAL_FCY_LIMIT = productBehaviourModel.COLLATERAL_FCY_LIMIT,
                                    CUSTOMER_LIMIT = productBehaviourModel.CUSTOMER_LIMIT,
                                    PRODUCT_LIMIT = productBehaviourModel.PRODUCT_LIMIT,
                                    INVOICE_LIMIT = productBehaviourModel.INVOICE_LIMIT,
                                    ALLOWFUNDUSAGE = productBehaviourModel.ALLOWFUNDUSAGE,
                                    ISTEMPORARYOVERDRAFT = productBehaviourModel.ISTEMPORARYOVERDRAFT,
                                    REQUIRECASAACCOUNT = productBehaviourModel.REQUIRECASAACCOUNT
                                };
                                //context.TBL_PRODUCT_BEHAVIOUR.Add(productBehaviour);
                                productBehaviourModel.APPROVALSTATUSID = approvalStatusId;
                                productBehaviourModel.ISCURRENT = false;
                                product.TBL_PRODUCT_BEHAVIOUR.Add(productBehaviour);

                            }

                        }
                    }
                    return true;
                }
                catch (Exception ex)
                {
                    throw ex;
                }

            }

            public async Task<ProductViewModel> AddTempProduct(ProductViewModel productModel)
            {
                var isPrincipalGLRequired = context.TBL_PRODUCT_TYPE.Any(x => x.PRODUCTTYPEID == productModel.productTypeId && x.REQUIREPRINCIPALGL == true || x.PRODUCTTYPEID == productModel.productTypeId && x.REQUIREPRINCIPALGL2 == true);

                if (isPrincipalGLRequired)
                {
                    if (productModel.currencies == null)
                        throw new SecureException("Product Currency must be specified. Please select a principal GL with mapped currencies");
                }

            var isProductCodeExist = context.TBL_TEMP_PRODUCT.Any(x => x.PRODUCTCODE == productModel.productCode);

            if (isProductCodeExist)
            {
                throw new SecureException("Product code already exist");
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
                    throw new SecureException("Product Information already exist and is undergoing approval");
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
                    ISFACILITYLINE = productModel.isFacilityLine,
                    PENALCHARGERATE = productModel.penalChargeRate,
                    USEDBYLOS = productModel.usedByLos,
                    COMPANYID = productModel.companyId,
                    CUSTOMERTYPEID = productModel.customerTypeId,
                    PRODUCTTYPEID = productModel.productTypeId,
                    PRODUCTCATEGORYID = productModel.productCategoryId,
                    PRODUCTCLASSID = (short)productModel.productClassId,
                    PRODUCTCODE = productModel.productCode,
                    // PRODUCTCODE = GenerateProductCode(productModel.companyId),
                    PRODUCTNAME = productModel.productName,
                    PRODUCTDESCRIPTION = productModel.productDescription,
                    RISKRATINGID = productModel.riskRatingId,
                    PRINCIPALBALANCEGL = productModel.principalBalanceGl,
                    PRINCIPALBALANCEGL2 = productModel.principalBalanceGl2,
                    PENALCHARGEGL = productModel.penalChargeGl,
                    INTERESTINCOMEEXPENSEGL = productModel.interestIncomeExpenseGl,
                    INTERESTRECEIVABLEPAYABLEGL = productModel.interestReceivablePayableGl,
                    DORMANTGL = productModel.dormantGl,
                    PREMIUMDISCOUNTGL = productModel.premiumDiscountGl,
                    OVERDRAWNGL = productModel.overdrawnGl,
                    EXCLUDEFROMLITIGATION = productModel.excludeFromLitigation,
                    ISPAYDAYPRODUCT = productModel.isPaydayProduct,

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
                    OPERATION = "insert",
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

                var behaviour = productModel.productBehaviour;
                var productBehaviour = new TBL_TEMP_PRODUCT_BEHAVIOUR()
                {
                    APPROVALSTATUSID = (short)ApprovalStatusEnum.Pending,
                    ISCURRENT = true,
                    PRODUCTCODE = product.PRODUCTCODE,
                    COLLATERAL_LCY_LIMIT = behaviour.collateralLcyLimit,
                    COLLATERAL_FCY_LIMIT = behaviour.collateralFcyLimit,
                    CUSTOMER_LIMIT = behaviour.customerLimit,
                    PRODUCT_LIMIT = behaviour.invoiceLimit,
                    INVOICE_LIMIT = behaviour.productLimit,
                    ISINVOICEBASED = behaviour.isInvoiceBased,
                    ISTEMPORARYOVERDRAFT = behaviour.isTemporaryOverDraft,
                    ALLOWFUNDUSAGE = behaviour.allowFundUsage,
                    DATETIMECREATED = DateTime.Now,
                    CREATEDBY = productModel.createdBy,
                    CRMSREGULATORYID = behaviour.crmsRegulatoryId,
                };

                // Audit Section ---------------------------
                var audit = new TBL_AUDIT
                {
                    AUDITTYPEID = (short)AuditTypeEnum.ProductAdded,
                    STAFFID = productModel.createdBy,
                    BRANCHID = (short)productModel.userBranchId,
                    DETAIL = $"Initiated Product Creation for '{productModel.productName}' with code'{productModel.productCode}'",
                    IPADDRESS = CommonHelpers.GetLocalIpAddress(),
                    URL = productModel.applicationUrl,
                    APPLICATIONDATE = genSetup.GetApplicationDate(),
                    SYSTEMDATETIME = DateTime.Now,
                    DEVICENAME = CommonHelpers.GetDeviceName(),
                    OSNAME = CommonHelpers.FriendlyName(),
                };

                using (var trans = context.Database.BeginTransaction())
                {
                    try
                    {
                        auditTrail.AddAuditTrail(audit);
                        context.TBL_TEMP_PRODUCT.Add(product);
                        context.SaveChanges();
                        productBehaviour.TEMP_PRODUCTID = product.TEMP_PRODUCTID;
                        context.TBL_TEMP_PRODUCT_BEHAVIOUR.Add(productBehaviour);
                        output = context.SaveChanges() > 0;

                        //productBehaviour.TEMP_PRODUCTID = product.TEMP_PRODUCTID;

                        var entity = new ApprovalViewModel
                        {
                            staffId = productModel.createdBy,
                            companyId = productModel.companyId,
                            approvalStatusId = (int)ApprovalStatusEnum.Pending,
                            comment = "Please approve this product",
                            targetId = product.TEMP_PRODUCTID,
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
                                return new ProductViewModel { productId = product.TEMP_PRODUCTID, productCode = product.PRODUCTCODE };
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        trans.Rollback();
                        throw new SecureException(ex.Message);
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

            public async Task<bool> UpdateProduct(int productId, ProductViewModel productModel)
            {
            try
            {
                bool output = false;
                var targetProductId = 0;

                var existingTempProduct = context.TBL_TEMP_PRODUCT
                        .FirstOrDefault(x => x.PRODUCTCODE.ToLower() ==
                            productModel.productCode.ToLower()
                                && x.APPROVALSTATUSID == (int)ApprovalStatusEnum.Approved);

                var existingProductCurrencies = new List<TBL_TEMP_PRODUCT_CURRENCY>();
                var existingProductFees = new List<TBL_TEMP_PRODUCT_CHARGE_FEE>();
                var existingProductCollateral = new List<TBL_TEMP_PRODUCT_COLLATERALTYP>();

                List<TBL_TEMP_PRODUCT_CHARGE_FEE> productFees = new List<TBL_TEMP_PRODUCT_CHARGE_FEE>();
                List<TBL_TEMP_PRODUCT_COLLATERALTYP> productCollaterals = new List<TBL_TEMP_PRODUCT_COLLATERALTYP>();
                List<TBL_TEMP_PRODUCT_CURRENCY> productCurrencies = new List<TBL_TEMP_PRODUCT_CURRENCY>();

                var unApprovedProductEdit = context.TBL_TEMP_PRODUCT
                    .Where(x => x.APPROVALSTATUSID == (int)ApprovalStatusEnum.Pending
                    && x.PRODUCTCODE.ToLower() == productModel.productCode.ToLower()).FirstOrDefault();

                TBL_TEMP_PRODUCT tempProduct = new TBL_TEMP_PRODUCT();
                TBL_TEMP_PRODUCT_BEHAVIOUR tempProductBehaviour = new TBL_TEMP_PRODUCT_BEHAVIOUR();

                if (unApprovedProductEdit != null)
                {
                    throw new SecureException("Product is already undergoing approval");
                }

                if (existingTempProduct != null)
                {
                    var existingTempProductBehaviour = context.TBL_TEMP_PRODUCT_BEHAVIOUR
                        .FirstOrDefault(x => x.PRODUCTCODE.ToLower() == productModel.productCode.ToLower()
                                && x.APPROVALSTATUSID == (int)ApprovalStatusEnum.Approved);
                    existingProductCurrencies = context.TBL_TEMP_PRODUCT_CURRENCY.Where(x => x.TEMP_PRODUCTID == existingTempProduct.TEMP_PRODUCTID).ToList();
                    existingProductFees = context.TBL_TEMP_PRODUCT_CHARGE_FEE.Where(x => x.TEMP_PRODUCTID == existingTempProduct.TEMP_PRODUCTID).ToList();
                    existingProductCollateral = context.TBL_TEMP_PRODUCT_COLLATERALTYP.Where(x => x.TEMP_PRODUCTID == existingTempProduct.TEMP_PRODUCTID).ToList();

                    // Remove exisiting product fees, currency and collaterals

                    if (existingProductCurrencies.Count > 0 && productModel.currencies.Count() > 0)
                    {
                        foreach (var curr in existingProductCurrencies)
                        {
                            context.TBL_TEMP_PRODUCT_CURRENCY.Remove(curr);
                        }
                    }

                    if (existingProductFees.Count > 0 && productModel.fees.Count() > 0)
                    {
                        foreach (var fee in existingProductFees)
                        {
                            context.TBL_TEMP_PRODUCT_CHARGE_FEE.Remove(fee);
                        }
                    }

                    if (existingProductCollateral.Count > 0 && productModel.collaterals.Count() > 0)
                    {
                        foreach (var coll in existingProductCollateral)
                        {
                            context.TBL_TEMP_PRODUCT_COLLATERALTYP.Remove(coll);
                        }
                    }

                    if (productModel.currencies.Count() > 0)
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
                            productCurrencies.Add(productCurrency);
                        }
                       
                    }

                    if (productModel.fees.Count() > 0)
                    {
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
                    }

                    if (productModel.collaterals.Count() > 0)
                    {
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
                    }

                    var tempProductToUpdate = existingTempProduct;
                    tempProductToUpdate.PENALCHARGERATE = productModel.penalChargeRate;
                    tempProductToUpdate.USEDBYLOS = productModel.usedByLos;
                    tempProductToUpdate.PRODUCTID = (short)productModel.productId;
                    tempProductToUpdate.PRODUCTCLASSID = (short)productModel.productClassId;
                    tempProductToUpdate.PRODUCTCODE = productModel.productCode;
                    tempProductToUpdate.PRODUCTNAME = productModel.productName;
                    tempProductToUpdate.PRODUCTDESCRIPTION = productModel.productDescription;
                    tempProductToUpdate.RISKRATINGID = productModel.riskRatingId;
                    tempProductToUpdate.ISFACILITYLINE = productModel.isFacilityLine;
                    tempProductToUpdate.PRINCIPALBALANCEGL = productModel.principalBalanceGl;
                    tempProductToUpdate.PRINCIPALBALANCEGL2 = productModel.principalBalanceGl2;
                    tempProductToUpdate.PENALCHARGEGL = productModel.penalChargeGl;
                    tempProductToUpdate.EXCLUDEFROMLITIGATION = productModel.excludeFromLitigation;
                    tempProductToUpdate.ISPAYDAYPRODUCT = productModel.isPaydayProduct;

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
                    tempProductToUpdate.OPERATION = "update";
                    tempProductToUpdate.APPROVALSTATUSID = (int)ApprovalStatusEnum.Pending;

                    tempProductToUpdate.TBL_TEMP_PRODUCT_CURRENCY = productCurrencies;
                    tempProductToUpdate.TBL_TEMP_PRODUCT_CHARGE_FEE = productFees;
                    tempProductToUpdate.TBL_TEMP_PRODUCT_COLLATERALTYP = productCollaterals;

                    tempProductBehaviour = new TBL_TEMP_PRODUCT_BEHAVIOUR()
                    {
                        //TEMP_PRODUCTID = tempProductToUpdate.TEMP_PRODUCTID,
                        PRODUCTCODE = productModel.productCode,
                        COLLATERAL_FCY_LIMIT = productModel.productBehaviour.collateralFcyLimit,
                        COLLATERAL_LCY_LIMIT = productModel.productBehaviour.collateralLcyLimit,
                        CUSTOMER_LIMIT = productModel.productBehaviour.customerLimit,
                        ISCURRENT = true,
                        APPROVALSTATUSID = (int)ApprovalStatusEnum.Pending,
                        ISINVOICEBASED = productModel.productBehaviour.isInvoiceBased,
                        PRODUCT_LIMIT = productModel.productBehaviour.productLimit,
                        INVOICE_LIMIT = productModel.productBehaviour.invoiceLimit,
                        ISTEMPORARYOVERDRAFT = productModel.productBehaviour.isTemporaryOverDraft,
                        ALLOWFUNDUSAGE = productModel.productBehaviour.allowFundUsage,
                        DATETIMECREATED = DateTime.Now,
                        CREATEDBY = productModel.createdBy,
                        CRMSREGULATORYID = productModel.productBehaviour.crmsRegulatoryId,
                    };
                    
                    context.TBL_TEMP_PRODUCT.Add(tempProductToUpdate);
                    context.TBL_TEMP_PRODUCT_BEHAVIOUR.Add(tempProductBehaviour);
                        
                    context.SaveChanges();
                    tempProductBehaviour.TEMP_PRODUCTID = tempProductToUpdate.TEMP_PRODUCTID;

                }
                else
                {
                    var targetProduct = context.TBL_PRODUCT.Find(productId);
                    var targetProductBehaviour = context.TBL_PRODUCT_BEHAVIOUR.Where(x => x.PRODUCTID == targetProduct.PRODUCTID).FirstOrDefault();
                    //Storing the updated product currencies
                    if (productModel.currencies.Count() > 0)
                    {
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
                    }

                    if (productModel.fees.Count() > 0)
                    {
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
                    }

                    if (productModel.collaterals.Count() > 0)
                    {
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
                    }
                    //End of storing the updated product currencies
                    tempProduct = new TBL_TEMP_PRODUCT()
                    {
                        ISFACILITYLINE = productModel.isFacilityLine,
                        PENALCHARGERATE = productModel.penalChargeRate,
                        USEDBYLOS = productModel.usedByLos,
                        COMPANYID = productModel.companyId,
                        PRODUCTTYPEID = productModel.productTypeId,
                        PRODUCTCATEGORYID = productModel.productCategoryId,
                        PRODUCTCLASSID = (short)productModel.productClassId,
                        PRODUCTCODE = targetProduct?.PRODUCTCODE,
                        PRODUCTNAME = productModel.productName,
                        PRODUCTDESCRIPTION = productModel.productDescription,

                        PRINCIPALBALANCEGL = productModel.principalBalanceGl,
                        PRINCIPALBALANCEGL2 = productModel.principalBalanceGl2,
                        PENALCHARGEGL = productModel.penalChargeGl,

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
                        EXCLUDEFROMLITIGATION = productModel.excludeFromLitigation,
                        ISPAYDAYPRODUCT = productModel.isPaydayProduct,

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
                        OPERATION = "update",
                        RISKRATINGID = productModel.riskRatingId,
                        TBL_TEMP_PRODUCT_CURRENCY = productCurrencies,
                        TBL_TEMP_PRODUCT_COLLATERALTYP = productCollaterals,
                        TBL_TEMP_PRODUCT_CHARGE_FEE = productFees,
                        PRODUCTID = (short)productModel.productId
                    };

                    tempProductBehaviour = new TBL_TEMP_PRODUCT_BEHAVIOUR()
                    {
                        PRODUCTCODE = productModel.productCode,
                        COLLATERAL_FCY_LIMIT = productModel.productBehaviour.collateralFcyLimit,
                        COLLATERAL_LCY_LIMIT = productModel.productBehaviour.collateralLcyLimit,
                        CUSTOMER_LIMIT = productModel.productBehaviour.customerLimit,
                        ISCURRENT = true,
                        ISINVOICEBASED = productModel.productBehaviour.isInvoiceBased,
                        PRODUCT_LIMIT = productModel.productBehaviour.productLimit,
                        INVOICE_LIMIT = productModel.productBehaviour.invoiceLimit,
                        ALLOWFUNDUSAGE = productModel.productBehaviour.allowFundUsage,
                        ISTEMPORARYOVERDRAFT = productModel.productBehaviour.isTemporaryOverDraft,
                        DATETIMECREATED = DateTime.Now,
                        CREATEDBY = productModel.createdBy,
                        CRMSREGULATORYID = productModel.productBehaviour.crmsRegulatoryId,
                        APPROVALSTATUSID = (short)ApprovalStatusEnum.Pending,

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
                    IPADDRESS = CommonHelpers.GetLocalIpAddress(),
                    URL = productModel.applicationUrl,
                    APPLICATIONDATE = genSetup.GetApplicationDate(),
                    SYSTEMDATETIME = DateTime.Now,
                    TARGETID = productId,
                    DEVICENAME = CommonHelpers.GetDeviceName(),
                    OSNAME = CommonHelpers.FriendlyName(),
                };

                using (var trans = context.Database.BeginTransaction())
                {
                    try
                    {
                        this.auditTrail.AddAuditTrail(audit);
                        //end of Audit section -------------------------------

                        output = context.SaveChanges() > 0;

                        if (existingTempProduct == null) { tempProductBehaviour.TEMP_PRODUCTID = tempProduct.TEMP_PRODUCTID; }

                        targetProductId = existingTempProduct?.TEMP_PRODUCTID ?? tempProduct.TEMP_PRODUCTID;

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
                        throw new SecureException(ex.Message);
                    }
                }
            }catch(Exception ex)
            {
                throw ex;
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

        public WorkflowResponse GoForApprovalGlobalPriceIndex(ApprovalViewModel entity)
        {
                entity.operationId = (int)OperationsEnum.GlobalInterestRateChangeApproval;
               // entity.operationId = (int)OperationsEnum.GlobalInterestRateChange;

                entity.externalInitialization = false;


                using (var trans = context.Database.BeginTransaction())
                {
                    try
                    {
                        // workFlow.LogForApproval(entity);
                        // var b = workFlow.NextLevelId ?? 0;

                        workFlow.StaffId = entity.staffId;
                        workFlow.CompanyId = entity.companyId;
                        workFlow.StatusId = ((int)entity.approvalStatusId == (int)ApprovalStatusEnum.Approved) ? (int)ApprovalStatusEnum.Processing : (int)entity.approvalStatusId;
                        workFlow.TargetId = entity.targetId;
                        workFlow.Comment = entity.comment;
                        workFlow.OperationId = entity.operationId;
                        workFlow.DeferredExecution = true;
                        workFlow.ExternalInitialization = false;
                        workFlow.LogActivity();

                        var saved = this.context.SaveChanges();
                        var globalPriceIndex = context.TBL_PRODUCT_PRICE_INDEX_GLOBAL.Find(entity.targetId);

                        List<string> receiverEmailList = new List<string>();
                        AlertsViewModel alert = new AlertsViewModel();

                        var productPriceIndex = context.TBL_PRODUCT_PRICE_INDEX.Find(globalPriceIndex.PRODUCTPRICEINDEXID);
                        var dynamicMessage = string.Empty;
                        var staffEmail = context.TBL_STAFF.Find(globalPriceIndex.CREATEDBY);
                        var messageStatus = "";
                            dynamicMessage = "Global Interest Rate Change on Product Price Index: " + productPriceIndex.PRICEINDEXDESCRIPTION.ToUpper() + " from old interest rate " + globalPriceIndex.OLDRATE+ " to new interest rate " + globalPriceIndex .NEWRATE+ " has been " +messageStatus;

                        if (workFlow.NewState != (int)ApprovalState.Ended)
                        {
                            globalPriceIndex.APPROVALSTATUSID = (short)ApprovalStatusEnum.Processing;
                            context.SaveChanges();
                            trans.Commit();
                            //trans.Dispose();
                            return workFlow.Response;
                            //return 3;

                        }
                        if (workFlow.NewState == (int)ApprovalState.Ended)
                        {
                            alert.receiverEmailList.Add(staffEmail.EMAIL);
                            if (workFlow.StatusId == (int)ApprovalStatusEnum.Approved)
                            {
                                
                                messageStatus = "Approved";
                                var appDate = genSetup.GetApplicationDate();
                                if (globalPriceIndex.EFFECTIVEDATE == appDate)
                                {
                                    var priceIndex = context.TBL_PRODUCT_PRICE_INDEX.Find(globalPriceIndex.PRODUCTPRICEINDEXID);
                                    priceIndex.PRICEINDEXRATE = globalPriceIndex.NEWRATE;
                                    priceIndex.DATETIMEUPDATED = DateTime.Now;
                                    priceIndex.LASTUPDATEDBY = entity.createdBy;
                                }
                                globalPriceIndex.APPROVALSTATUSID = (short)ApprovalStatusEnum.Approved;
                                globalPriceIndex.DATETIMEUPDATED = DateTime.Now;
                                globalPriceIndex.LASTUPDATEDBY = entity.createdBy;
                                context.SaveChanges();
                                LogEmailAlert(dynamicMessage, "Global Interest Rate Change Notification ", alert.receiverEmailList, "10021", 10022, "GlobalInterestRateChange");
                                loanOperations.ProcessGlobalInterestRepricing(globalPriceIndex.EFFECTIVEDATE, globalPriceIndex.PRODUCTPRICEINDEXID, (short)entity.createdBy, globalPriceIndex.ISMARKETINDUCED, globalPriceIndex.PRODUCTPRICEINDEXGLOBALID);
                                trans.Commit();
                                trans.Dispose();
                                return workFlow.Response;
                                //return 1;

                            }
                            else if (workFlow.StatusId == (int)ApprovalStatusEnum.Disapproved)
                            {
                                messageStatus = "Disapproved";
                                globalPriceIndex.APPROVALSTATUSID = (short)ApprovalStatusEnum.Approved;
                                globalPriceIndex.DATETIMEUPDATED = DateTime.Now;
                                globalPriceIndex.LASTUPDATEDBY = entity.createdBy;
                                LogEmailAlert(dynamicMessage, "Global Interest Rate Change Notification ", alert.receiverEmailList, "10021", 10022, "GlobalInterestRateChange");
                                context.SaveChanges();
                                trans.Commit();
                                trans.Dispose();
                                return workFlow.Response;
                                //return 2;
                            }

                        }
                        else
                        {
                            trans.Commit();
                            trans.Dispose();

                        }
                        return workFlow.Response;
                        //return 0;
                    }
                    catch (Exception ex)
                    {
                        trans.Rollback();
                        throw new SecureException(ex.Message);
                    }
                }

        }

        public void LogEmailAlert(string messageBody, string alertSubject, List<string> recipients, string referenceCode, int targetId, string operationMehtod)
        {
            try
            {
                var title = alertSubject.Trim();
                if (title.Contains("&"))
                {
                    title = title.Replace("&", "AND");
                }
                if (title.Contains("."))
                {
                    title = title.Replace(".", "");
                }

                string recipient = string.Join("", recipients.ToArray());
                string messageSubject = title;
                string messageContent = messageBody;
                MessageLogViewModel messageModel = new MessageLogViewModel
                {
                    MessageSubject = messageSubject,
                    MessageBody = messageContent,
                    MessageStatusId = 1,
                    MessageTypeId = 1,
                    FromAddress = ConfigurationManager.AppSettings["SupportEmailAddr"],
                    ToAddress = $"{recipient}",
                    DateTimeReceived = DateTime.Now,
                    SendOnDateTime = DateTime.Now,
                    ReferenceCode = referenceCode,
                    targetId = targetId,
                    operationMethod = operationMehtod,
                };
                SaveMessageDetails(messageModel);
            }
            catch (Exception ex)
            {
                new SecureException(ex.ToString());
            }
        }

        private void SaveMessageDetails(MessageLogViewModel model)
        {
            var message = new TBL_MESSAGE_LOG()
            {
                //MessageId = model.MessageId,
                MESSAGESUBJECT = model.MessageSubject,
                MESSAGEBODY = model.MessageBody,
                MESSAGESTATUSID = model.MessageStatusId,
                MESSAGETYPEID = model.MessageTypeId,
                FROMADDRESS = model.FromAddress,
                TOADDRESS = model.ToAddress,
                DATETIMERECEIVED = model.DateTimeReceived,
                SENDONDATETIME = model.SendOnDateTime,
                ATTACHMENTCODE = model.ReferenceCode,
                ATTACHMENTTYPEID = (short)AttachementTypeEnum.JobRequest,
                TARGETID = (int)model.targetId,
                OPERATIONMETHOD = model.operationMethod
            };

            context.TBL_MESSAGE_LOG.Add(message);
            context.SaveChanges();

        }

        //public int GoForApprovalGlobalPriceIndex(ApprovalViewModel entity)
        //{
        //    entity.operationId = (int)OperationsEnum.GlobalInterestRateChange;

        //    entity.externalInitialization = false;


        //    using (var trans = context.Database.BeginTransaction())
        //    {
        //        try
        //        {
        //            // workFlow.LogForApproval(entity);
        //            // var b = workFlow.NextLevelId ?? 0;

        //            workFlow.StaffId = entity.staffId;
        //            workFlow.CompanyId = entity.companyId;
        //            workFlow.StatusId = ((int)entity.approvalStatusId == (int)ApprovalStatusEnum.Approved) ? (int)ApprovalStatusEnum.Processing : (int)entity.approvalStatusId;
        //            workFlow.TargetId = entity.targetId;
        //            workFlow.Comment = entity.comment;
        //            workFlow.OperationId = entity.operationId;
        //            workFlow.DeferredExecution = true;
        //            workFlow.ExternalInitialization = false;

        //            workFlow.LogActivity();

        //            //context.savechanges();
        //            //if (b == 0 && workFlow.NewState != (int)ApprovalState.Ended) // check if this is the last level
        //            //{
        //            //    trans.Rollback();
        //            //    throw new SecureException("Approval Failed");
        //            //}

        //            var globalPriceIndex = context.TBL_PRODUCT_PRICE_INDEX_GLOBAL.Find(entity.targetId);

        //            //if (entity.approvalStatusId == (short)ApprovalStatusEnum.Disapproved)
        //            //{

        //            //    globalPriceIndex.APPROVALSTATUSID = (short)ApprovalStatusEnum.Disapproved;
        //            //    context.SaveChanges();
        //            //    trans.Commit();
        //            //    return 2;
        //            //}
        //            if (workFlow.NewState != (int)ApprovalState.Ended)
        //            {
        //                globalPriceIndex.APPROVALSTATUSID = (short)ApprovalStatusEnum.Processing;
        //                context.SaveChanges();
        //                trans.Commit();
        //                return 3;

        //            }
        //            if (workFlow.NewState == (int)ApprovalState.Ended)
        //            {
        //                if (workFlow.StatusId == (int)ApprovalStatusEnum.Approved)
        //                {
        //                    var appDate = genSetup.GetApplicationDate();
        //                    if (globalPriceIndex.EFFECTIVEDATE == appDate)
        //                    {
        //                        var priceIndex = context.TBL_PRODUCT_PRICE_INDEX.Find(globalPriceIndex.PRODUCTPRICEINDEXID);
        //                        priceIndex.PRICEINDEXRATE = globalPriceIndex.NEWRATE;
        //                        priceIndex.DATETIMEUPDATED = DateTime.Now;
        //                        priceIndex.LASTUPDATEDBY = entity.createdBy;
        //                    }
        //                    globalPriceIndex.APPROVALSTATUSID = (short)ApprovalStatusEnum.Approved;
        //                    globalPriceIndex.DATETIMEUPDATED = DateTime.Now;
        //                    globalPriceIndex.LASTUPDATEDBY = entity.createdBy;
        //                    context.SaveChanges();
        //                    loanOperations.ProcessGlobalInterestRepricing(globalPriceIndex.EFFECTIVEDATE, globalPriceIndex.PRODUCTPRICEINDEXID, (short)entity.createdBy, globalPriceIndex.ISMARKETINDUCED, globalPriceIndex.PRODUCTPRICEINDEXGLOBALID);
        //                    trans.Commit();
        //                    return 1;

        //                }
        //                else if (workFlow.StatusId == (int)ApprovalStatusEnum.Disapproved)
        //                {
        //                    globalPriceIndex.APPROVALSTATUSID = (short)ApprovalStatusEnum.Approved;
        //                    globalPriceIndex.DATETIMEUPDATED = DateTime.Now;
        //                    globalPriceIndex.LASTUPDATEDBY = entity.createdBy;
        //                    context.SaveChanges();
        //                    trans.Commit();
        //                    return 2;
        //                }

        //            }
        //            else
        //            {
        //                trans.Commit();
        //            }

        //            return 0;
        //        }
        //        catch (Exception ex)
        //        {
        //            trans.Rollback();
        //            throw new SecureException(ex.Message);
        //        }
        //    }



        //}

        public List<ProductPriceIndexDailyViewModel> getProductPriceIndexHistory(DateTime startDate, DateTime endDate, int companyId)
            {

                var result = from x in context.TBL_PRODUCT_PRICE_INDEX_DAILY
                             join b in context.TBL_PRODUCT_PRICE_INDEX
                             on x.PRODUCTPRICEINDEXID equals b.PRODUCTPRICEINDEXID
                             where x.DATE >= startDate && x.DATE <= endDate
                             select new ProductPriceIndexDailyViewModel
                             {
                                 priceIndexName = b.PRICEINDEXNAME,
                                 productPriceIndexId = x.PRODUCTPRICEINDEXID,
                                 priceIndexRate = x.PRICEINDEXRATE,
                                 date = x.DATE,
                             };
                return result.ToList();
            }

            private IEnumerable<ProductPriceIndexViewModel> GetAllProductPriceIndex(int companyId)
            {
                return (from data in context.TBL_PRODUCT_PRICE_INDEX
                        where data.COMPANYID == companyId && data.DELETED == false
                        select new ProductPriceIndexViewModel()
                        {
                            productPriceIndexId = data.PRODUCTPRICEINDEXID,
                            priceIndexDescription = data.PRICEINDEXDESCRIPTION,
                            priceIndexDuration = data.DURATION,
                            allowAutomaticRepricing = data.ALLOWAUTOMATICREPRICING,
                            companyId = data.COMPANYID,
                            priceIndexName = data.PRICEINDEXNAME,
                            priceIndexRate = data.PRICEINDEXRATE,
                            dateTimeUpdated = data.DATETIMEUPDATED,
                            deleted = data.DELETED,
                            deletedBy = data.DELETEDBY,
                            dateTimeDeleted = data.DATETIMEDELETED,
                            currencyId = data.CURRENCYID,
                        });
            }

        public ProductPriceIndexViewModel GetAllProductPriceIndicesById(int priceIndexId)
        {
            return (from data in context.TBL_PRODUCT_PRICE_INDEX
                    where data.PRODUCTPRICEINDEXID == priceIndexId && data.DELETED == false
                    select new ProductPriceIndexViewModel()
                    {
                        productPriceIndexId = data.PRODUCTPRICEINDEXID,
                        priceIndexDescription = data.PRICEINDEXDESCRIPTION,
                        priceIndexDuration = data.DURATION,
                        allowAutomaticRepricing = data.ALLOWAUTOMATICREPRICING,
                        companyId = data.COMPANYID,
                        priceIndexName = data.PRICEINDEXNAME,
                        priceIndexRate = data.PRICEINDEXRATE,
                        dateTimeUpdated = data.DATETIMEUPDATED,
                        deleted = data.DELETED,
                        deletedBy = data.DELETEDBY,
                        dateTimeDeleted = data.DATETIMEDELETED,
                        currencyId = data.CURRENCYID,
                    }).FirstOrDefault();
        }

        private IEnumerable<ProductPriceIndexGlobalViewModel> GetProductPriceIndexGlobalApprovalList(int staffId)
            {

                var ids = genSetup.GetStaffApprovalLevelIds(staffId, (int)OperationsEnum.GlobalInterestRateChangeApproval).ToList();

            return (from data in context.TBL_PRODUCT_PRICE_INDEX_GLOBAL
                    join atrail in context.TBL_APPROVAL_TRAIL on data.PRODUCTPRICEINDEXGLOBALID equals atrail.TARGETID
                    where
                    data.APPROVALSTATUSID != (int)ApprovalStatusEnum.Approved
                    && atrail.APPROVALSTATUSID != (int)ApprovalStatusEnum.Disapproved
                    && atrail.RESPONSESTAFFID == null
                    && atrail.OPERATIONID == (int)OperationsEnum.GlobalInterestRateChangeApproval
                    && (ids.Contains((int)atrail.TOAPPROVALLEVELID) && (atrail.TOSTAFFID == staffId || atrail.TOSTAFFID == null))
                    orderby atrail.APPROVALTRAILID descending
                    select new ProductPriceIndexGlobalViewModel()
                    {
                        productPriceIndexGlobalId = data.PRODUCTPRICEINDEXGLOBALID,
                        productPriceIndexId = data.PRODUCTPRICEINDEXID,
                        productPriceIndexName = context.TBL_PRODUCT_PRICE_INDEX.Where(x => x.PRODUCTPRICEINDEXID == data.PRODUCTPRICEINDEXID).Select(m => m.PRICEINDEXNAME).FirstOrDefault(),
                        oldRate = data.OLDRATE,
                        newRate = data.NEWRATE,
                        effectiveDate = data.EFFECTIVEDATE,
                        approvalStatusId = data.APPROVALSTATUSID,
                        hasBeenApplied = data.HASBEENAPPLIED,
                        isMarketInduced = data.ISMARKETINDUCED,
                        dateTimeCreated = data.DATETIMECREATED,
                        operationId = (int)OperationsEnum.GlobalInterestRateChangeApproval,
                        currentApprovalLevelId = atrail.TOAPPROVALLEVELID
                    });
            }

            private IEnumerable<ProductPriceIndexGlobalViewModel> GetAllProductPriceIndexGlobal(int staffId)
            {
                var ids = genSetup.GetStaffApprovalLevelIds(staffId, (int)OperationsEnum.GlobalInterestRateChangeApproval).ToList();
                var indexes = (from data in context.TBL_PRODUCT_PRICE_INDEX_GLOBAL
                               join atrail in context.TBL_APPROVAL_TRAIL on data.PRODUCTPRICEINDEXGLOBALID equals atrail.TARGETID
                               where 
                                data.APPROVALSTATUSID != (int)ApprovalStatusEnum.Approved
                               && (atrail.APPROVALSTATUSID == (int)ApprovalStatusEnum.Referred
                               || atrail.APPROVALSTATUSID == (int)ApprovalStatusEnum.Disapproved)
                               && atrail.RESPONSESTAFFID == null
                               && atrail.OPERATIONID == (int)OperationsEnum.GlobalInterestRateChangeApproval
                               && (!ids.Contains((int)atrail.TOAPPROVALLEVELID) && atrail.LOOPEDSTAFFID == staffId)
                               orderby atrail.APPROVALTRAILID descending
                               select new ProductPriceIndexGlobalViewModel()
                               {
                                   productPriceIndexGlobalId = data.PRODUCTPRICEINDEXGLOBALID,
                                   productPriceIndexId = data.PRODUCTPRICEINDEXID,
                                   productPriceIndexName = context.TBL_PRODUCT_PRICE_INDEX.Where(x => x.PRODUCTPRICEINDEXID == data.PRODUCTPRICEINDEXID).Select(m => m.PRICEINDEXNAME).FirstOrDefault(),
                                   oldRate = data.OLDRATE,
                                   newRate = data.NEWRATE,
                                   effectiveDate = data.EFFECTIVEDATE,
                                   approvalStatusId = data.APPROVALSTATUSID,
                                   hasBeenApplied = data.HASBEENAPPLIED,
                                   isMarketInduced = data.ISMARKETINDUCED,
                                   dateTimeCreated = data.DATETIMECREATED,
                                   //operationId = (int)OperationsEnum.GlobalInterestRateChange,
                                   operationId = (int)OperationsEnum.GlobalInterestRateChangeApproval,
                                   currentApprovalLevelId = atrail.TOAPPROVALLEVELID
                               }).ToList();

            return indexes;
                                
        }
            //public IEnumerable<ProductPriceIndexViewModel> GetAllProductPriceIndexByCurrencyId(int currencyId)
            //{
            //    var productIndex = (from a in context.TBL_PRODUCT_PRICE_INDEX
            //                        join b in context.TBL_PRODUCT_PRICE_INDEX_CURNCY
            //                        on a.PRODUCTPRICEINDEXID equals b.PRODUCTPRICEINDEXID
            //                        where b.CURRENCYID == currencyId && a.DELETED == false
            //                        select new ProductPriceIndexViewModel
            //                        {
            //                            productPriceIndexId = a.PRODUCTPRICEINDEXID,
            //                            priceIndexDescription = a.PRICEINDEXDESCRIPTION,
            //                            companyId = a.COMPANYID,
            //                            priceIndexName = a.PRICEINDEXNAME,
            //                            priceIndexRate = a.PRICEINDEXRATE,
            //                            dateTimeUpdated = a.DATETIMEUPDATED,
            //                            currencyId = a.CURRENCYID,
            //                        }).ToList();
            //    return productIndex;
            //}

            public ProductPriceIndexViewModel GetProductPriceIndexByProductId(int productId)
            {
                var data = (from b in context.TBL_PRODUCT
                            join a in context.TBL_PRODUCT_PRICE_INDEX on b.PRODUCTPRICEINDEXID equals a.PRODUCTPRICEINDEXID
                            where b.PRODUCTID == productId && a.DELETED == false
                            select new ProductPriceIndexViewModel()
                            {
                                productPriceIndexId = a.PRODUCTPRICEINDEXID,
                                priceIndexDescription = a.PRICEINDEXDESCRIPTION,
                                companyId = a.COMPANYID,
                                priceIndexName = a.PRICEINDEXNAME,
                                priceIndexRate = a.PRICEINDEXRATE,
                                currencyId = a.CURRENCYID,
                            }).FirstOrDefault();

                return data;
            }
            public IEnumerable<ProductPriceIndexViewModel> GetProductPriceIndex(int companyId)
            {
                return GetAllProductPriceIndex(companyId);
            }
            public IEnumerable<ProductPriceIndexGlobalViewModel> GetProductPriceIndexGlobal(int staffId)
            {
                return GetAllProductPriceIndexGlobal(staffId);
            }
            public IEnumerable<ProductPriceIndexGlobalViewModel> GetProductPriceIndexGlobalAwaitingApproval(int staffId)
            {
                return GetProductPriceIndexGlobalApprovalList(staffId);
            }
            public WorkflowResponse AddProductPriceIndexGlobal(ProductPriceIndexGlobalViewModel prodPriceIndexGlobal)
            {
                
                if (prodPriceIndexGlobal.effectiveDate > genSetup.GetApplicationDate())
                {
                    throw new ConditionNotMetException("Effective Date Can Not be More than Application Date.");
                }
                var data = new TBL_PRODUCT_PRICE_INDEX_GLOBAL()
                {
                    PRODUCTPRICEINDEXID = prodPriceIndexGlobal.productPriceIndexId,
                    OLDRATE = prodPriceIndexGlobal.oldRate,
                    NEWRATE = prodPriceIndexGlobal.newRate,
                    EFFECTIVEDATE = prodPriceIndexGlobal.effectiveDate,
                    APPROVALSTATUSID = (int)ApprovalStatusEnum.Pending,
                    HASBEENAPPLIED = prodPriceIndexGlobal.hasBeenApplied,
                    ISMARKETINDUCED = prodPriceIndexGlobal.isMarketInduced,
                    CREATEDBY = prodPriceIndexGlobal.createdBy,
                    DATETIMECREATED = DateTime.Now,
                };

                context.TBL_PRODUCT_PRICE_INDEX_GLOBAL.Add(data);

                // Audit Section ---------------------------
                var audit = new TBL_AUDIT
                {
                    AUDITTYPEID = (short)AuditTypeEnum.ProductPriceIndexGlobalAdded,
                    STAFFID = (int)prodPriceIndexGlobal.createdBy,
                    BRANCHID = (short)prodPriceIndexGlobal.userBranchId,
                    DETAIL = $"Added Price Index Global: '{prodPriceIndexGlobal.productPriceIndexGlobalId}' ",
                    IPADDRESS = CommonHelpers.GetLocalIpAddress(),
                    URL = prodPriceIndexGlobal.applicationUrl,
                    APPLICATIONDATE = genSetup.GetApplicationDate(),
                    SYSTEMDATETIME = DateTime.Now,
                    DEVICENAME = CommonHelpers.GetDeviceName(),
                    OSNAME = CommonHelpers.FriendlyName(),
                };
                bool output;
                var productPriceIndexId = 0;

                using (var trans = context.Database.BeginTransaction())
                {
                    try
                    {
                        this.auditTrail.AddAuditTrail(audit);
                        //end of Audit section -------------------------------

                        output = context.SaveChanges() > 0;

                        productPriceIndexId = data.PRODUCTPRICEINDEXGLOBALID;

                        var entity = new ApprovalViewModel
                        {
                            staffId = prodPriceIndexGlobal.createdBy,
                            companyId = prodPriceIndexGlobal.companyId,
                            approvalStatusId = (int)ApprovalStatusEnum.Pending,
                            targetId = productPriceIndexId,
                            operationId = (int)OperationsEnum.GlobalInterestRateChangeApproval,
                            //operationId = (int)OperationsEnum.GlobalInterestRateChange,
                            BranchId = prodPriceIndexGlobal.userBranchId,
                            externalInitialization = true,
                            comment = "Initiation"
                        };
                         workFlow.LogForApproval(entity);
                    //================================================================
                    //int lastStatusId = response.StatusId;
                    //if (currentOperationType == (short)OperationsEnum.LoanReviewApprovalAvailment) appl.APPROVALSTATUSID = (short)lastStatusId;

                    //if (workflow.NewState == (int)ApprovalState.Ended && model.isFlowTest == false)
                    //{
                    //    if (workflow.StatusId == (int)ApprovalStatusEnum.Approved)
                    //    {
                    //        short nextOperatioId = 0;
                    //        var flowOrder = context.TBL_LMSR_FLOW_ORDER.Where(x => x.OPERATIONID == model.operationId).FirstOrDefault();
                    //        var defaultFlowOrder = context.TBL_LMSR_FLOW_ORDER.Where(x => x.OPERATIONID == 0).FirstOrDefault();
                    //        if (model.operationId == (short)OperationsEnum.LoanReviewApprovalAvailment)
                    //        {
                    //            appl.APPROVALSTATUSID = (short)ApprovalStatusEnum.Approved;
                    //        }

                    //        if (flowOrder == null)
                    //        {
                    //            if (defaultFlowOrder.REQUIREOFFERLETTER && currentOperationType != (short)OperationsEnum.LoanReviewApprovalAvailment)
                    //            {
                    //                nextOperatioId = (short)OperationsEnum.LoanReviewApprovalOfferLetter;
                    //                LogLMSOperationForRouting(model, items, nextOperatioId, (short)operationId);
                    //            }
                    //            else if (defaultFlowOrder.REQUIREAVAILMENT)
                    //            {
                    //                if (workflow.Response.nextLevelName == null) workflow.Response.nextLevelName = "Credit Admin";
                    //                nextOperatioId = (short)OperationsEnum.LoanReviewApprovalAvailment;
                    //                LogLMSOperationForRouting(model, items, nextOperatioId, (short)operationId);
                    //                //appl.APPROVALSTATUSID = (short)ApprovalStatusEnum.Approved;
                    //            }
                    //            else
                    //            {
                    //                LogLMSOperationForRouting(model, items, nextOperatioId, (short)OperationsEnum.LoanReviewApprovalAvailment);
                    //            }
                    //        }

                            //=============================================================================
                            //if (response)
                           // {
                            trans.Commit();

                            return workFlow.Response;
                        //}

                       // return false;
                    }
                    catch (Exception ex)
                    {
                        trans.Rollback();
                        throw new SecureException(ex.Message);
                    }
                }

            }
        public bool UpdateProductPriceIndexGlobal(int productPriceIndexGlobalId, ProductPriceIndexGlobalViewModel prodPriceIndexGlobal)
        {
            TBL_PRODUCT_PRICE_INDEX_GLOBAL globalInterest = new TBL_PRODUCT_PRICE_INDEX_GLOBAL();
            globalInterest = this.context.TBL_PRODUCT_PRICE_INDEX_GLOBAL.FirstOrDefault(x => x.PRODUCTPRICEINDEXGLOBALID == productPriceIndexGlobalId && (x.APPROVALSTATUSID == (int)ApprovalStatusEnum.Pending || x.APPROVALSTATUSID == (int)ApprovalStatusEnum.Referred));

            if (globalInterest == null)
                throw new SecureException("Record is Undergoing Approval");

            //data.PRODUCTPRICEINDEXID = prodPriceIndexGlobal.productPriceIndexId;
            globalInterest.OLDRATE = prodPriceIndexGlobal.oldRate;
            globalInterest.NEWRATE = prodPriceIndexGlobal.newRate;
            globalInterest.EFFECTIVEDATE = prodPriceIndexGlobal.effectiveDate;
            globalInterest.HASBEENAPPLIED = prodPriceIndexGlobal.hasBeenApplied;
            globalInterest.ISMARKETINDUCED = prodPriceIndexGlobal.isMarketInduced;

            globalInterest.LASTUPDATEDBY = prodPriceIndexGlobal.lastUpdatedBy;
            globalInterest.DATETIMEUPDATED = DateTime.Now;
            globalInterest.APPROVALSTATUSID = (int)ApprovalStatusEnum.Pending;
            // Audit Section ---------------------------
            var audit = new TBL_AUDIT
            {
                AUDITTYPEID = (short)AuditTypeEnum.ProductPriceIndexUpdated,
                STAFFID = (int)prodPriceIndexGlobal.createdBy,
                BRANCHID = (short)prodPriceIndexGlobal.userBranchId,
                DETAIL = $"Updated Price Index Global: '{prodPriceIndexGlobal.productPriceIndexGlobalId}' ",
                IPADDRESS = CommonHelpers.GetLocalIpAddress(),
                URL = prodPriceIndexGlobal.applicationUrl,
                APPLICATIONDATE = genSetup.GetApplicationDate(),
                SYSTEMDATETIME = DateTime.Now,
                DEVICENAME = CommonHelpers.GetDeviceName(),
                OSNAME = CommonHelpers.FriendlyName(),
            };

            bool output;
            var productPriceIndexId = 0;
            output = context.SaveChanges() > 0;

            //if (globalInterest.APPROVALSTATUSID != (int)ApprovalStatusEnum.Referred) return output;

            using (var trans = context.Database.BeginTransaction())
                {
                    try
                    {
                        this.auditTrail.AddAuditTrail(audit);
                        //end of Audit section -------------------------------

                        productPriceIndexId = globalInterest.PRODUCTPRICEINDEXGLOBALID;
                        var entity = new ApprovalViewModel
                        {
                            staffId = prodPriceIndexGlobal.createdBy,
                            companyId = prodPriceIndexGlobal.companyId,
                            approvalStatusId = (int)ApprovalStatusEnum.Pending,
                            targetId = productPriceIndexId,
                            operationId = (int)OperationsEnum.GlobalInterestRateChangeApproval,
                            //operationId = (int)OperationsEnum.GlobalInterestRateChange,
                            BranchId = prodPriceIndexGlobal.userBranchId,
                            externalInitialization = true,
                            comment = prodPriceIndexGlobal.comment
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
                        throw new SecureException(ex.Message);
                    }
                }
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
                    var indexExists = context.TBL_PRODUCT_PRICE_INDEX.Where(x => x.PRICEINDEXNAME.ToLower() == prodPriceIndex.priceIndexName.ToLower() && x.DELETED == true).FirstOrDefault();
                    indexExists.ALLOWAUTOMATICREPRICING = prodPriceIndex.allowAutomaticRepricing;
                    indexExists.PRICEINDEXRATE = prodPriceIndex.priceIndexRate;
                    indexExists.PRICEINDEXDESCRIPTION = prodPriceIndex.priceIndexDescription;
                    indexExists.PRICEINDEXNAME = prodPriceIndex.priceIndexName;
                    indexExists.DELETED = false;
                }
                else
                {
                    var data = new TBL_PRODUCT_PRICE_INDEX()
                    {
                        COMPANYID = prodPriceIndex.companyId,
                        PRICEINDEXDESCRIPTION = prodPriceIndex.priceIndexDescription,
                        PRICEINDEXNAME = prodPriceIndex.priceIndexName,
                        PRICEINDEXRATE = prodPriceIndex.priceIndexRate,
                        ALLOWAUTOMATICREPRICING = prodPriceIndex.allowAutomaticRepricing,
                        DURATION = prodPriceIndex.priceIndexDuration,
                        CREATEDBY = prodPriceIndex.createdBy,
                        DATETIMECREATED = DateTime.Now,
                    };

                    this.context.TBL_PRODUCT_PRICE_INDEX.Add(data);
                }


                // Audit Section ---------------------------
                var audit = new TBL_AUDIT
                {
                    AUDITTYPEID = (short)AuditTypeEnum.ProductPriceIndexAdded,
                    STAFFID = (int)prodPriceIndex.createdBy,
                    BRANCHID = (short)prodPriceIndex.userBranchId,
                    DETAIL = $"Added tbl_Product Price Index: '{prodPriceIndex.priceIndexName}' ",
                    IPADDRESS = CommonHelpers.GetLocalIpAddress(),
                    URL = prodPriceIndex.applicationUrl,
                    APPLICATIONDATE = genSetup.GetApplicationDate(),
                    SYSTEMDATETIME = DateTime.Now,
                    DEVICENAME = CommonHelpers.GetDeviceName(),
                    OSNAME = CommonHelpers.FriendlyName(),
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
                data.ALLOWAUTOMATICREPRICING = prodPriceIndex.allowAutomaticRepricing;
                data.DURATION = prodPriceIndex.priceIndexDuration;
                data.LASTUPDATEDBY = prodPriceIndex.lastUpdatedBy;
                data.DATETIMEUPDATED = DateTime.Now;

                // Audit Section ---------------------------
                var audit = new TBL_AUDIT
                {
                    AUDITTYPEID = (short)AuditTypeEnum.ProductPriceIndexUpdated,
                    STAFFID = (int)prodPriceIndex.createdBy,
                    BRANCHID = (short)prodPriceIndex.userBranchId,
                    DETAIL = $"Updated tbl_Product Price Index: '{prodPriceIndex.priceIndexName}' ",
                    IPADDRESS = CommonHelpers.GetLocalIpAddress(),
                    URL = prodPriceIndex.applicationUrl,
                    APPLICATIONDATE = genSetup.GetApplicationDate(),
                    SYSTEMDATETIME = DateTime.Now,
                    DEVICENAME = CommonHelpers.GetDeviceName(),
                    OSNAME = CommonHelpers.FriendlyName(),
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
                    IPADDRESS = CommonHelpers.GetLocalIpAddress(),
                    URL = user.applicationUrl,
                    APPLICATIONDATE = genSetup.GetApplicationDate(),
                    SYSTEMDATETIME = DateTime.Now,
                    TARGETID = productPriceIndexId,
                    DEVICENAME = CommonHelpers.GetDeviceName(),
                    OSNAME = CommonHelpers.FriendlyName(),
                };

                this.auditTrail.AddAuditTrail(audit);

                //end of Audit section -------------------------------
                return this.SaveAll();
            }




            public IEnumerable<ProductPriceIndexCurrencyViewModel> GetProductPriceIndexCurrencyById(int productPriceIndexId)
            {
                return GetAllProductPriceIndexCurrency(productPriceIndexId).ToList();
            }

            private IEnumerable<ProductPriceIndexCurrencyViewModel> GetAllProductPriceIndexCurrency(int productPriceIndexId)
            {
                return (from data in context.TBL_PRODUCT_PRICE_INDEX
                        where data.DELETED == false && data.PRODUCTPRICEINDEXID == productPriceIndexId
                        select new ProductPriceIndexCurrencyViewModel()
                        {
                            //priceIndexCurrencyId = data.PRICEINDEXCURRENCYID,
                            productPriceIndexId = (short)data.PRODUCTPRICEINDEXID,
                            currencyId = (short)data.CURRENCYID,
                            dateTimeUpdated = data.DATETIMEUPDATED,
                            deleted = data.DELETED,
                            deletedBy = data.DELETEDBY,
                            dateTimeDeleted = data.DATETIMEDELETED
                        });
            }

            public List<ProductPriceIndexViewModel> GetProductPriceIndexByCurrencyId(int currencyId)
            {
                return (from p in context.TBL_PRODUCT_PRICE_INDEX
                            // join pc in context.TBL_PRODUCT_PRICE_INDEX_CURNCY on p.PRODUCTPRICEINDEXID equals pc.PRODUCTPRICEINDEXID
                        where p.DELETED == false && p.CURRENCYID == currencyId
                        select new ProductPriceIndexViewModel
                        {
                            priceIndexDescription = p.PRICEINDEXDESCRIPTION,
                            priceIndexName = p.PRICEINDEXNAME,
                            priceIndexRate = p.PRICEINDEXRATE,
                            priceIndexDuration = p.DURATION,
                            productPriceIndexId = p.PRODUCTPRICEINDEXID,
                            allowAutomaticRepricing = p.ALLOWAUTOMATICREPRICING,
                        }).ToList();
            }

            //public ProductPriceIndexCurrencyViewModel AddProductPriceIndexCurrency(ProductPriceIndexCurrencyViewModel prodPriceIndexCurrency)
            //{
            //    var isProductPriceIndexCurrencyExist = context.TBL_PRODUCT_PRICE_INDEX_CURNCY.Where(x => x.PRODUCTPRICEINDEXID == prodPriceIndexCurrency.productPriceIndexId && x.CURRENCYID == prodPriceIndexCurrency.currencyId).FirstOrDefault();

            //    if (isProductPriceIndexCurrencyExist != null)
            //    {
            //        throw new SecureException("Product price Currency already exists!");
            //    }
            //    var data = new TBL_PRODUCT_PRICE_INDEX_CURNCY()
            //    {
            //        PRODUCTPRICEINDEXID = prodPriceIndexCurrency.productPriceIndexId,
            //        CURRENCYID = prodPriceIndexCurrency.currencyId,
            //        CREATEDBY = prodPriceIndexCurrency.createdBy,
            //        DATETIMECREATED = DateTime.Now,
            //    };

            //    this.context.TBL_PRODUCT_PRICE_INDEX_CURNCY.Add(data);
            //    var isProductPriceIndexExist = context.TBL_PRODUCT_PRICE_INDEX.Where(x => x.PRODUCTPRICEINDEXID == prodPriceIndexCurrency.productPriceIndexId).FirstOrDefault();
            //    // Audit Section ---------------------------
            //    var audit = new TBL_AUDIT
            //    {
            //        AUDITTYPEID = (short)AuditTypeEnum.ProductPriceIndexAdded,
            //        STAFFID = (int)prodPriceIndexCurrency.createdBy,
            //        BRANCHID = (short)prodPriceIndexCurrency.userBranchId,
            //        DETAIL = $"Added  Currency For tbl_Product Price Index: '{isProductPriceIndexExist.PRICEINDEXNAME}' ",
            //        IPADDRESS = prodPriceIndexCurrency.userIPAddress,
            //        URL = prodPriceIndexCurrency.applicationUrl,
            //        APPLICATIONDATE = genSetup.GetApplicationDate(),
            //        SYSTEMDATETIME = DateTime.Now
            //    };

            //    this.auditTrail.AddAuditTrail(audit);
            //    //end of Audit section -------------------------------
            //    bool status;

            //    try
            //    {
            //        status = this.context.SaveChanges() > 0;
            //        if (status) { return prodPriceIndexCurrency; }
            //        else
            //            return null;
            //    }
            //    catch (Exception ex)
            //    {
            //        var det = ex;
            //    }
            //    //var status = this.SaveAll();

            //    //if (status)
            //    //{

            //    //}
            //    //else
            //    return null;
            //}

            //public bool UpdateProductPriceIndexCurrency(int priceIndexCurrencyId, ProductPriceIndexCurrencyViewModel prodPriceIndexCurrency)
            //{
            //    var data = this.context.TBL_PRODUCT_PRICE_INDEX_CURNCY.FirstOrDefault(x => x.PRICEINDEXCURRENCYID == priceIndexCurrencyId);

            //    if (data == null)
            //        return false;

            //    data.CURRENCYID = (int)prodPriceIndexCurrency.currencyId;
            //    data.LASTUPDATEDBY = prodPriceIndexCurrency.lastUpdatedBy;
            //    data.DATETIMEUPDATED = DateTime.Now;
            //    var isProductPriceIndexExist = context.TBL_PRODUCT_PRICE_INDEX.Where(x => x.PRODUCTPRICEINDEXID == prodPriceIndexCurrency.productPriceIndexId).FirstOrDefault();

            //    // Audit Section ---------------------------
            //    var audit = new TBL_AUDIT
            //    {

            //        AUDITTYPEID = (short)AuditTypeEnum.ProductPriceIndexUpdated,
            //        STAFFID = (int)prodPriceIndexCurrency.createdBy,
            //        BRANCHID = (short)prodPriceIndexCurrency.userBranchId,
            //        DETAIL = $"Updated  Currency For tbl_Product Price Index: '{isProductPriceIndexExist.PRICEINDEXNAME}' with CurrencyId: '{ prodPriceIndexCurrency.priceIndexCurrencyId}' ",
            //        IPADDRESS = prodPriceIndexCurrency.userIPAddress,
            //        URL = prodPriceIndexCurrency.applicationUrl,
            //        APPLICATIONDATE = genSetup.GetApplicationDate(),
            //        SYSTEMDATETIME = DateTime.Now,
            //        TARGETID = priceIndexCurrencyId
            //    };

            //    this.auditTrail.AddAuditTrail(audit);
            //    //end of Audit section -------------------------------
            //    return this.SaveAll();
            //}

            //public bool DeleteProductPriceIndexCurrency(int priceIndexCurrencyId, UserInfo user)
            //{
            //    var data = this.context.TBL_PRODUCT_PRICE_INDEX_CURNCY.Find(priceIndexCurrencyId);

            //    if (data == null)
            //        return false;

            //    data.DELETED = true;
            //    data.DATETIMEDELETED = genSetup.GetApplicationDate();

            //    // Audit Section ---------------------------
            //    var productPriceIndex = this.context.TBL_PRODUCT_PRICE_INDEX.FirstOrDefault(x => x.PRODUCTPRICEINDEXID == data.PRODUCTPRICEINDEXID);
            //    var audit = new TBL_AUDIT
            //    {
            //        AUDITTYPEID = (short)AuditTypeEnum.ProductPriceIndexDeleted,
            //        STAFFID = user.staffId,
            //        BRANCHID = (short)user.BranchId,
            //        DETAIL = $"Deleted Currency For tbl_Product Price Index: '{productPriceIndex.PRICEINDEXNAME}' with rate '{productPriceIndex.PRICEINDEXRATE}' ",
            //        IPADDRESS = user.userIPAddress,
            //        URL = user.applicationUrl,
            //        APPLICATIONDATE = genSetup.GetApplicationDate(),
            //        SYSTEMDATETIME = DateTime.Now,
            //        TARGETID = priceIndexCurrencyId
            //    };

            //    this.auditTrail.AddAuditTrail(audit);

            //    //end of Audit section -------------------------------
            //    return this.SaveAll();
            //}


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
                            //customerTypeId = p.TBL_PRODUCT.CUSTOMERTYPEID,
                            //customerType = p.TBL_CUSTOMER_TYPE.NAME,
                            profileBusinessUnitName = p.BUSINESSUNITID != null ? p.TBL_PROFILE_BUSINESS_UNIT.BUSINESSUNITNAME : null,
                            globalSla = p.GLOBALSLA,
                            businessUnitId = p.BUSINESSUNITID,

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
                                //productClass.CUSTOMERTYPEID = model.customerTypeId;
                                productClass.GLOBALSLA = model.globalSla;
                                productClass.BUSINESSUNITID = model.businessUnitId;
                            }
                        }
                        else
                        {
                            productClass = new TBL_PRODUCT_CLASS()
                            {
                                PRODUCTCLASSNAME = model.productClassName,
                                PRODUCTCLASSTYPEID = model.productClassTypeId,
                                PRODUCT_CLASS_PROCESSID = model.productClassProcessId,
                                //CUSTOMERTYPEID = model.customerTypeId,
                                GLOBALSLA = model.globalSla,
                                BUSINESSUNITID = model.businessUnitId
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
                            IPADDRESS = CommonHelpers.GetLocalIpAddress(),
                            URL = model.applicationUrl,
                            APPLICATIONDATE = genSetup.GetApplicationDate(),
                            SYSTEMDATETIME = DateTime.Now,
                            DEVICENAME = CommonHelpers.GetDeviceName(),
                            OSNAME = CommonHelpers.FriendlyName(),
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

            #region Product Document Mapping
        public IEnumerable<ProductDocumentMappingViewModel> GetAllProductDocumentMapping()
        {
            var products = context.TBL_PRODUCT.Where(p => p.DELETED == false).ToList();
            var productClasses = context.TBL_PRODUCT_CLASS.ToList();

            var mappings = (from p in docContext.TBL_DOC_MAPPING
                            where p.DELETED == false
                            select new ProductDocumentMappingViewModel()
                            {
                                productDocMapId = p.PRODUCTDOCMAPID,
                                documentCategoryId = p.TBL_DOCUMENT_TYPE.DOCUMENTCATEGORYID,
                                documentCategoryName = p.TBL_DOCUMENT_TYPE.TBL_DOCUMENT_CATEGORY.DOCUMENTCATEGORYNAME,
                                productId = p.PRODUCTID,
                                productClassId = p.PRODUCTCLASSID,
                                operationId = p.OPERATIONID,
                                mapToProductClass = p.MAPTOPRODUCTCLASS,
                                mapToProduct = p.MAPTOPRODUCT,
                                mapToOperation = p.MAPTOOPERATION,
                                mapToSector = p.MAPTOSECTOR,
                                mapToSubSector = p.MAPTOSUBSECTOR,
                                required = p.ISREQUIRED,
                                documentTypeId = p.DOCUMENTTYPEID,
                                documentType = p.TBL_DOCUMENT_TYPE.DOCUMENTTYPENAME,
                                sectorId = p.SECTORID,
                                subSectorId = p.SUBSECTORID,

                                //productName = p.TBL_PRODUCT.PRODUCTNAME,
                                //productClassName = p.TBL_PRODUCT_CLASS.PRODUCTCLASSNAME
                            }).ToList();

            foreach (var mapping in mappings)
            {
                mapping.sectorName = context.TBL_SECTOR.FirstOrDefault(s => s.SECTORID == mapping.sectorId)?.NAME;
                mapping.subSectorName = context.TBL_SUB_SECTOR.FirstOrDefault(s => s.SUBSECTORID == mapping.subSectorId)?.NAME;
                mapping.productName = products.FirstOrDefault(p => p.PRODUCTID == mapping?.productId)?.PRODUCTNAME;
                mapping.productClassName = productClasses.FirstOrDefault(p => p.PRODUCTCLASSID == mapping?.productClassId)?.PRODUCTCLASSNAME;
                mapping.sectorName = mapping.sectorName == null ? "N/A" : mapping.sectorName;
                mapping.subSectorName = mapping.subSectorName == null ? "N/A" : mapping.subSectorName;
                mapping.productName = mapping.productName == null ? "N/A" : mapping.productName;
                mapping.productClassName = mapping.productClassName == null ? "N/A" : mapping.productClassName;
            }

            return mappings;
        }


        public bool AddProductDocumentMapping(ProductDocumentMappingViewModel model)
        {
            if (model.mapToProductClass == false && model.mapToProduct == false && model.mapToSector == false && model.mapToSubSector == false)
            {
                throw new SecureException("Please select an item to map to!");
            }


            var data = new TBL_DOC_MAPPING()
            {
                PRODUCTID = model.productId,
                PRODUCTCLASSID = model.productClassId,
                OPERATIONID = model.operationId,
                MAPTOPRODUCTCLASS = model.mapToProductClass,
                MAPTOSECTOR = model.mapToSector,
                MAPTOSUBSECTOR = model.mapToSubSector,
                MAPTOPRODUCT = model.mapToProduct,
                MAPTOOPERATION = model.mapToOperation,
                ISREQUIRED = model.required,
                SECTORID = model.sectorId,
                SUBSECTORID = model.subSectorId,
                DOCUMENTTYPEID=model.documentTypeId,
                CREATEDBY = model.createdBy,
                DATETIMECREATED = DateTime.Now,
                DELETED = false
            };

            this.docContext.TBL_DOC_MAPPING.Add(data);

            // Audit Section ---------------------------
            var audit = new TBL_AUDIT
            {
                AUDITTYPEID = (short)AuditTypeEnum.ProductDocumentMappingAdded,
                STAFFID = (int)model.createdBy,
                BRANCHID = (short)model.userBranchId,
                DETAIL = $"Added : TBL_PRODUCT_DOCUMENT_MAPPING'{model.productId}' ",
                IPADDRESS = CommonHelpers.GetLocalIpAddress(),
                URL = model.applicationUrl,
                APPLICATIONDATE = genSetup.GetApplicationDate(),
                SYSTEMDATETIME = DateTime.Now,
                DEVICENAME = CommonHelpers.GetDeviceName(),
                OSNAME = CommonHelpers.FriendlyName(),
            };

            this.auditTrail.AddAuditTrail(audit);
            //end of Audit section -------------------------------

            return docContext.SaveChanges() > 0;
        }

        public bool UpdateProductDocumentMapping(ProductDocumentMappingViewModel model)
        {

            if (model.mapToProductClass == false && model.mapToProduct == false)
            {
                throw new SecureException("Please select an item to map to!");
            }

            var entity = docContext.TBL_DOC_MAPPING.Find(model.productDocMapId);
            entity.ISREQUIRED = model.required;
            entity.DATETIMEUPDATED = DateTime.Now;
            entity.PRODUCTID = model.productId;
            entity.PRODUCTCLASSID = model.productClassId;
            entity.SECTORID = model.sectorId;
            entity.SUBSECTORID = model.subSectorId;
            entity.OPERATIONID = model.operationId;
            entity.MAPTOPRODUCTCLASS = model.mapToProductClass;
            entity.MAPTOPRODUCT = model.mapToProduct;
            entity.MAPTOSECTOR = model.mapToSector;
            entity.MAPTOSUBSECTOR = model.mapToSubSector;
            entity.MAPTOOPERATION = model.mapToOperation;
            entity.DOCUMENTTYPEID = model.documentTypeId;
            entity.LASTUPDATEDBY = model.createdBy;
            docContext.Entry(entity).State = EntityState.Modified;
            return docContext.SaveChanges() != 0;
        }

        public ProductDocumentMappingViewModel GetProductDocumenetMapping(int Id)
        {
            var entity = docContext.TBL_DOC_MAPPING.Where(x => x.DELETED == false && x.PRODUCTDOCMAPID == Id)
                .Select(x => new ProductDocumentMappingViewModel
                {
                    required = x.ISREQUIRED,
                    productId = x.PRODUCTID,
                    productClassId = x.PRODUCTCLASSID,
                    operationId = x.OPERATIONID,
                    mapToProductClass = x.MAPTOPRODUCTCLASS,
                    mapToProduct = x.MAPTOPRODUCT,
                    mapToOperation = x.MAPTOOPERATION,
                    mapToSector = x.MAPTOSECTOR,
                    mapToSubSector = x.MAPTOSUBSECTOR,
                    productDocMapId = x.PRODUCTDOCMAPID,
                    documentCategoryId = x.TBL_DOCUMENT_TYPE.DOCUMENTCATEGORYID,
                    documentCategoryName = x.TBL_DOCUMENT_TYPE.TBL_DOCUMENT_CATEGORY.DOCUMENTCATEGORYNAME,
                    documentTypeId = x.TBL_DOCUMENT_TYPE.DOCUMENTTYPEID,
                    sectorId = x.SECTORID,
                    subSectorId = x.SUBSECTORID

                }).FirstOrDefault();

            return entity;
        }

        public bool DeleteProductDocumentMapping(int id)
        {
            var entity = docContext.TBL_DOC_MAPPING.Find(id);
            if (entity != null)
            {
                entity.DELETED = true;
            }

            return docContext.SaveChanges() > 0;
        }

        #endregion Product Document Mapping

        #region Product Document Definition
        public IEnumerable<DocumentDefinitionViewModel> GetAllDocumentDefinition()
    {
        return (from p in context.TBL_DOCUMENT_DEFINITION
                select new DocumentDefinitionViewModel()
                {
                    documentDefinitionId = p.DOCUMENTDEFINITIONID,
                    documentTitle = p.DOCUMENTTITLE,
                    inUse = p.INUSE
                });
    }


    public bool AddDocumentDefinition(DocumentDefinitionViewModel model)
    {
         var data = new TBL_DOCUMENT_DEFINITION()
        {
            DOCUMENTTITLE = model.documentTitle,
            INUSE = model.inUse
        };

        this.context.TBL_DOCUMENT_DEFINITION.Add(data);

        // Audit Section ---------------------------
        var audit = new TBL_AUDIT
        {
            AUDITTYPEID = (short)AuditTypeEnum.DocumentDefinitionAdded,
            STAFFID = (int)model.createdBy,
            BRANCHID = (short)model.userBranchId,
            DETAIL = $"Added : TBL_DOCUMENT_DEFINITION'{model.documentDefinitionId}' ",
            IPADDRESS = CommonHelpers.GetLocalIpAddress(),
            URL = model.applicationUrl,
            APPLICATIONDATE = genSetup.GetApplicationDate(),
            SYSTEMDATETIME = DateTime.Now,
            DEVICENAME = CommonHelpers.GetDeviceName(),
            OSNAME = CommonHelpers.FriendlyName(),
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

        



        #endregion Product Document Definition    
    }
}
