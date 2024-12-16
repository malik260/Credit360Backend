using System;
using System.Collections.Generic;
using System.Linq;

using FintrakBanking.Common.CustomException;
using FintrakBanking.Common.Enum;
using FintrakBanking.Entities.Models;
using FintrakBanking.Interfaces.Admin;
using FintrakBanking.Interfaces.Setups.General;
using FintrakBanking.Interfaces.Risk;
using FintrakBanking.ViewModels;
using FintrakBanking.ViewModels.Risk;
using FintrakBanking.ViewModels.Setups.Approval;

namespace FintrakBanking.Repositories.Risk
{
    public class RiskAcceptanceCriteriaRepository : IRiskAcceptanceCriteriaRepository
    {
        private FinTrakBankingContext context;
        private IGeneralSetupRepository general;
        private IAuditTrailRepository audit;
        private IAdminRepository admin;

        public RiskAcceptanceCriteriaRepository(
                FinTrakBankingContext _context,
                IGeneralSetupRepository _general,
                IAuditTrailRepository _audit,
                IAdminRepository _admin
            )
        {
            this.context = _context;
            this.general = _general;
            this.audit = _audit;
            this.admin = _admin;
        }

        #region

        public RiskAcceptanceCriteriaViewModel GetRiskAcceptanceCriteriaByProduct(RiskAcceptanceCriteriaViewModel model)
        {
            RiskAcceptanceCriteriaViewModel rac = new RiskAcceptanceCriteriaViewModel();
            List<ProductRacCategory> productCategories = new List<ProductRacCategory>();
            List<RacCategoryViewModel> productRacCategories = new List<RacCategoryViewModel>();
            List<TBL_RAC_DEFINITION> racDefinition = new List<TBL_RAC_DEFINITION>();
            model.productId = 0;
            List<int> categoryIds = new List<int>();

            var customer = context.TBL_CUSTOMER.Where(x => x.CUSTOMERID == model.customerId ).FirstOrDefault();
            var employerType = context.TBL_CUSTOMER_EMPLOYMENTHISTORY.Where(x => x.CUSTOMERID == model.customerId).FirstOrDefault();
            
           int? customerTypeId = customer != null ? customer.CUSTOMERTYPEID : null;

            bool isCorporate = customerTypeId == (short)CustomerTypeEnum.Corporate;

            var employeeType = string.Empty;
            if(employerType != null && employerType?.EMPLOYERNAME?.ToLower() == "selfemployed" )
            {
                 employeeType = "SELFEMPLOYED";
            }
            else
            {
                if (isCorporate) { employeeType = "SELFEMPLOYED"; }
                else { employeeType = "EMPLOYEE";}
            }

            List<string> allEmployeeType = new List<string> { "BUSINESS OWNER", "SELFEMPLOYED", "EMPLOYEE" };
            List<TBL_RAC_DEFINITION> racDefinitionOnEmployer = new List<TBL_RAC_DEFINITION>();

            if (model.searchBasePlaceholder == "PRODUCT" || model.searchBasePlaceholder == "PRODUCTCLASS" && !model.isOperationbased && model.isAgricRac == false)
            {
                var productClass = context.TBL_PRODUCT.Where(x => x.PRODUCTID == model.productId).FirstOrDefault();
                var racDefinitionOnProduct = context.TBL_RAC_DEFINITION.Where(x => x.PRODUCTID == model.productId
                                                                                                && x.SEARCHPLACEHOLDER == "PRODUCT"
                                                                                                && (x.CUSTOMERTYPEID == null || (short)x.CUSTOMERTYPEID < 1)
                                                                                                && !allEmployeeType.Contains(x.EMPLOYMENTTYPE)
                                                                                                && x.SHOWATDRAWDOWN == model.isDrawdown
                                                                                                && x.ISACTIVE == true
                                                                                                && x.DELETED == false).ToList();

                        var racDefinitionOnProductClass = context.TBL_RAC_DEFINITION.Where(x => x.PRODUCTCLASSID == model.productClassId
                                                                                                  && x.SEARCHPLACEHOLDER == "PRODUCTCLASS"
                                                                                                  && x.SHOWATDRAWDOWN == model.isDrawdown
                                                                                                  && !allEmployeeType.Contains(x.EMPLOYMENTTYPE)
                                                                                                  && (x.CUSTOMERTYPEID == null || (short)x.CUSTOMERTYPEID < 1)
                                                                                                  && x.ISACTIVE == true && x.DELETED == false).ToList();

                
                var productRac = racDefinitionOnProduct.Union(racDefinitionOnProductClass);
                    racDefinition.AddRange(productRac);

                   // racDefinition = racDefinitionOnProduct.Count() > 0 ? racDefinitionOnProduct : racDefinitionOnProductClass;

                if (isCorporate)
                    {
                        var racDefinitionOnEmployerByProduct = context.TBL_RAC_DEFINITION.Where(x => x.PRODUCTID == model.productId && x.SEARCHPLACEHOLDER == "PRODUCT"
                                                                                               && (x.EMPLOYMENTTYPE == employeeType)
                                                                                               && x.SHOWATDRAWDOWN == model.isDrawdown
                                                                                               && (x.CUSTOMERTYPEID == (short)CustomerTypeEnum.Corporate)
                                                                                               && x.ISACTIVE == true
                                                                                               && x.DELETED == false).ToList();

                        var racDefinitionOnEmployerByProductClass = context.TBL_RAC_DEFINITION.Where(x => (x.PRODUCTCLASSID == model.productClassId && x.SEARCHPLACEHOLDER == "PRODUCTCLASS")
                                                                                               && (x.EMPLOYMENTTYPE == employeeType)
                                                                                               && x.SHOWATDRAWDOWN == model.isDrawdown
                                                                                               && (x.CUSTOMERTYPEID == (short)CustomerTypeEnum.Corporate)
                                                                                               && x.ISACTIVE == true
                                                                                               && x.DELETED == false).ToList();

                  

                    var employerRac = racDefinitionOnEmployerByProduct.Union(racDefinitionOnEmployerByProductClass);
                    //racDefinitionOnEmployer = racDefinitionOnEmployerByProduct.Count() > 0 ? racDefinitionOnEmployerByProduct : racDefinitionOnEmployerByProductClass;
                    racDefinition.AddRange(employerRac);

                }
                else if (!isCorporate)
                {
                    var racDefinitionOnEmployeeByProduct = context.TBL_RAC_DEFINITION.Where(x => x.PRODUCTID == model.productId && x.SEARCHPLACEHOLDER == "PRODUCT"
                                                                                         && (x.EMPLOYMENTTYPE == employeeType)
                                                                                         && x.SHOWATDRAWDOWN == model.isDrawdown
                                                                                         && (x.CUSTOMERTYPEID == (short)CustomerTypeEnum.Individual)
                                                                                         && x.ISACTIVE == true
                                                                                         && x.DELETED == false).ToList();

                    var racDefinitionOnEmployeeByProductClass = context.TBL_RAC_DEFINITION.Where(x => x.PRODUCTCLASSID == model.productClassId && x.SEARCHPLACEHOLDER == "PRODUCTCLASS"
                                                                                        && (x.EMPLOYMENTTYPE == employeeType)
                                                                                        && x.SHOWATDRAWDOWN == model.isDrawdown
                                                                                        && (x.CUSTOMERTYPEID == (short)CustomerTypeEnum.Individual)
                                                                                        && x.ISACTIVE == true
                                                                                        && x.DELETED == false).ToList();

                   

                    //racDefinitionOnEmployer = racDefinitionOnEmployeeByProduct.Count() > 0 ? racDefinitionOnEmployeeByProduct : racDefinitionOnEmployeeByProductClass;
                    var employerRac = racDefinitionOnEmployeeByProduct.Union(racDefinitionOnEmployeeByProductClass);
                    racDefinition.AddRange(employerRac);

                }

            }

            if (model.searchBasePlaceholder == "PRODUCT" || model.searchBasePlaceholder == "PRODUCTCLASS" && !model.isOperationbased && model.isAgricRac == true)
            {
                var productClass = context.TBL_PRODUCT.Where(x => x.PRODUCTID == model.productId).FirstOrDefault();
                var racDefinitionOnProduct = context.TBL_RAC_DEFINITION.Where(x => x.PRODUCTID == model.productId
                                                                                                && x.SEARCHPLACEHOLDER == "PRODUCT"
                                                                                                && (x.CUSTOMERTYPEID == null || (short)x.CUSTOMERTYPEID < 1)
                                                                                                && !allEmployeeType.Contains(x.EMPLOYMENTTYPE)
                                                                                                && x.SHOWATDRAWDOWN == model.isDrawdown
                                                                                                && x.ISACTIVE == true
                                                                                                && x.ISAGRICRAC == true
                                                                                                && x.DELETED == false).ToList();

                var racDefinitionOnProductClass = context.TBL_RAC_DEFINITION.Where(x => x.PRODUCTCLASSID == model.productClassId
                                                                                          && x.SEARCHPLACEHOLDER == "PRODUCTCLASS"
                                                                                          && x.SHOWATDRAWDOWN == model.isDrawdown
                                                                                          && !allEmployeeType.Contains(x.EMPLOYMENTTYPE)
                                                                                          && (x.CUSTOMERTYPEID == null || (short)x.CUSTOMERTYPEID < 1)
                                                                                          && x.ISACTIVE == true && x.ISAGRICRAC == true && x.DELETED == false).ToList();
                var productRac = racDefinitionOnProduct.Union(racDefinitionOnProductClass);
                racDefinition.AddRange(productRac);

                // racDefinition = racDefinitionOnProduct.Count() > 0 ? racDefinitionOnProduct : racDefinitionOnProductClass;

                if (isCorporate)
                {
                    var racDefinitionOnEmployerByProduct = context.TBL_RAC_DEFINITION.Where(x => x.PRODUCTID == model.productId && x.SEARCHPLACEHOLDER == "PRODUCT"
                                                                                           && (x.EMPLOYMENTTYPE == employeeType)
                                                                                           && x.SHOWATDRAWDOWN == model.isDrawdown
                                                                                           && (x.CUSTOMERTYPEID == (short)CustomerTypeEnum.Corporate)
                                                                                           && x.ISACTIVE == true
                                                                                           && x.ISAGRICRAC == true
                                                                                           && x.DELETED == false).ToList();

                    var racDefinitionOnEmployerByProductClass = context.TBL_RAC_DEFINITION.Where(x => (x.PRODUCTCLASSID == model.productClassId && x.SEARCHPLACEHOLDER == "PRODUCTCLASS")
                                                                                           && (x.EMPLOYMENTTYPE == employeeType)
                                                                                           && x.SHOWATDRAWDOWN == model.isDrawdown
                                                                                           && (x.CUSTOMERTYPEID == (short)CustomerTypeEnum.Corporate)
                                                                                           && x.ISACTIVE == true
                                                                                           && x.ISAGRICRAC == true
                                                                                           && x.DELETED == false).ToList();

                    var employerRac = racDefinitionOnEmployerByProduct.Union(racDefinitionOnEmployerByProductClass);
                    //racDefinitionOnEmployer = racDefinitionOnEmployerByProduct.Count() > 0 ? racDefinitionOnEmployerByProduct : racDefinitionOnEmployerByProductClass;
                    racDefinition.AddRange(employerRac);

                }
                else if (!isCorporate)
                {
                    var racDefinitionOnEmployeeByProduct = context.TBL_RAC_DEFINITION.Where(x => x.PRODUCTID == model.productId && x.SEARCHPLACEHOLDER == "PRODUCT"
                                                                                         && (x.EMPLOYMENTTYPE == employeeType)
                                                                                         && x.SHOWATDRAWDOWN == model.isDrawdown
                                                                                         && (x.CUSTOMERTYPEID == (short)CustomerTypeEnum.Individual)
                                                                                         && x.ISACTIVE == true
                                                                                         && x.ISAGRICRAC == true
                                                                                         && x.DELETED == false).ToList();

                    var racDefinitionOnEmployeeByProductClass = context.TBL_RAC_DEFINITION.Where(x => x.PRODUCTCLASSID == model.productClassId && x.SEARCHPLACEHOLDER == "PRODUCTCLASS"
                                                                                        && (x.EMPLOYMENTTYPE == employeeType)
                                                                                        && x.SHOWATDRAWDOWN == model.isDrawdown
                                                                                        && (x.CUSTOMERTYPEID == (short)CustomerTypeEnum.Individual)
                                                                                        && x.ISACTIVE == true
                                                                                        && x.ISAGRICRAC == true
                                                                                        && x.DELETED == false).ToList();

                    //racDefinitionOnEmployer = racDefinitionOnEmployeeByProduct.Count() > 0 ? racDefinitionOnEmployeeByProduct : racDefinitionOnEmployeeByProductClass;
                    var employerRac = racDefinitionOnEmployeeByProduct.Union(racDefinitionOnEmployeeByProductClass);
                    racDefinition.AddRange(employerRac);

                }

            }

            if (model.isOperationbased || model.searchBasePlaceholder != "CREDITCARD")
            {
                var racDefinitionOperation = context.TBL_RAC_DEFINITION.Where(x => x.OPERATIONID == model.operationId
                                                                                     && x.SEARCHPLACEHOLDER == "OPERATION"
                                                                                     && x.SHOWATDRAWDOWN == model.isDrawdown
                                                                                     && x.ISACTIVE == true
                                                                                     && x.DELETED == false).ToList();
                racDefinition.AddRange(racDefinitionOperation);
            }

            if (model.searchBasePlaceholder == "CREDITCARD" && !model.isOperationbased)
            {
                var localCurrencyId = context.TBL_COMPANY.FirstOrDefault()?.CURRENCYID ?? 0;
                racDefinition = context.TBL_RAC_DEFINITION.Where(x => x.CURRENCYTYPE == model.currencyType
                                                            && (
                                                                (x.CURRENCYTYPE == "LCY" && model.currencyId == localCurrencyId && model.currencyId != null)
                                                                || (x.CURRENCYTYPE == "FCY" && model.currencyId != localCurrencyId && model.currencyId != null)
                                                                || (x.CURRENCYTYPE == "CUSTOM" && x.CURRENCYID == model.currencyId && model.currencyId != null)
                                                                || (x.CURRENCYTYPE == null && x.CURRENCYID == model.currencyId)
                                                                || (x.CURRENCYTYPE == null && x.CURRENCYID == null)
                                                                )
                                                            && x.OPERATIONID == model.operationId
                                                            && x.SHOWATDRAWDOWN == model.isDrawdown
                                                            && x.ISACTIVE == true && x.DELETED == false).ToList();
            }

            if (racDefinition.Count() <= 0) return null;

            categoryIds.AddRange(racDefinition.Select(x => x.RACCATEGORYID).ToList());

            var racCategoryTypeId = racDefinition.Select(x => x.RACCATEGORYTYPEID).FirstOrDefault();

            productRacCategories = context.TBL_RAC_CATEGORY.Where(x => categoryIds.Contains(x.RACCATEGORYID)).Select(x => new RacCategoryViewModel
            {
                racCategoryId = x.RACCATEGORYID,
                categoryName = context.TBL_RAC_CATEGORY.Where(o => o.RACCATEGORYID == x.RACCATEGORYID).Select(o => o.CATEGORYNAME).FirstOrDefault(),
            }).ToList();

            /*
            value: '',
            status: 2,
            */

            foreach (var productRacCategory in productRacCategories)
            {
                ProductRacCategory racCategory = new ProductRacCategory();
                List<ProductRacItem> items = new List<ProductRacItem>();

                if (racCategoryTypeId != 0 && racCategoryTypeId != null)
                {
                    items = racDefinition.Where(x => x.RACCATEGORYTYPEID == racCategoryTypeId && x.ISACTIVE == true && x.DELETED == false)
                    .Select(x => new ProductRacItem
                    {
                        id = x.RACDEFINITIONID,
                        categoryId = x.RACCATEGORYID,
                        //id = x.TBL_RAC_ITEM.RACITEMID,
                        criteria = x.TBL_RAC_ITEM.CRITERIA,
                        required = x.TBL_RAC_ITEM.DESCRIPTION,
                        typeId = x.RACINPUTTYPEID,
                        type = context.TBL_RAC_INPUT_TYPE.FirstOrDefault(t => t.RACINPUTTYPEID == x.RACINPUTTYPEID).INPUTTAG,
                        optionId = x.RACOPTIONID,
                        fileUpload = x.REQUIREUPLOAD,
                        hasException = x.ISREQUIRED == false,
                        viewUpload = x.REQUIREDOCUMENTVIEW
                    })
                    .ToList();
                }
                else
                {
                    items = racDefinition
                    .Select(x => new ProductRacItem
                    {
                        id = x.RACDEFINITIONID,
                        categoryId = x.RACCATEGORYID,
                        //id = x.TBL_RAC_ITEM.RACITEMID,
                        criteria = x.TBL_RAC_ITEM.CRITERIA,
                        required = x.TBL_RAC_ITEM.DESCRIPTION,
                        typeId = x.RACINPUTTYPEID,
                        type = context.TBL_RAC_INPUT_TYPE.FirstOrDefault(t => t.RACINPUTTYPEID == x.RACINPUTTYPEID).INPUTTAG,
                        optionId = x.RACOPTIONID,
                        fileUpload = x.REQUIREUPLOAD,
                        hasException = x.ISREQUIRED == false,
                        viewUpload = x.REQUIREDOCUMENTVIEW
                    })
                    .ToList();
                }

                foreach (var item in items)
                {
                    item.options = item.optionId == null ? null
                                                        : context.TBL_RAC_OPTION_ITEM
                                                                    .Where(o => o.RACOPTIONID == item.optionId)
                                                                    .Select(o => new ProductRacOption
                                                                    {
                                                                        key = o.KEY,
                                                                        label = o.LABEL
                                                                    })
                                                                    .ToList();
                }

                racCategory.rows = items.Where(x => x.categoryId == productRacCategory.racCategoryId).ToList();
                racCategory.name = productRacCategory.categoryName;
                productCategories.Add(racCategory);
            }

            rac.count = productCategories.Count();
            rac.categories = productCategories;

            return rac;
        }

        //public RiskAcceptanceCriteriaViewModel GetDynamicRiskAcceptanceCriteriaByProduct(RiskAcceptanceCriteriaViewModel model)
        //{
        //    RiskAcceptanceCriteriaViewModel rac = new RiskAcceptanceCriteriaViewModel();
        //    List<ProductRacCategory> productCategories = new List<ProductRacCategory>();
        //    List<RacCategoryViewModel> productRacCategories = new List<RacCategoryViewModel>();

        //    List<int> categoryIds = new List<int>();
        //    if (model.searchBaseId  == (short)RacAccessEnum.Product || model.searchBaseId == null)
        //    {
        //         categoryIds = context.TBL_RAC_DEFINITION.Where(x => x.PRODUCTID == model.productId
        //                                                   && (x.SHOWATDRAWDOWN == model.isDrawdown  || x.SHOWATDRAWDOWN == null)
        //                                                   && x.ISACTIVE == true && x.DELETED == false)
        //       .Select(x => x.RACCATEGORYID)
        //       .ToList();
        //    }
        //    if (model.searchBaseId == (short)RacAccessEnum.CreditCard)
        //    {
        //        categoryIds = context.TBL_RAC_DEFINITION.Where(x => x.CURRENCYTYPE == model.currencyType
        //                                                    && x.CURRENCYID == model.currencyId
        //                                                    && x.OPERATIONID == model.operationId
        //                                                    && (x.SHOWATDRAWDOWN == model.isDrawdown  || x.SHOWATDRAWDOWN == null)
        //                                                    && x.ISACTIVE == true && x.DELETED == false)
        //        .Select(x => x.RACCATEGORYID)
        //        .ToList();
        //    }
            

        //    if (model.racCategoryTypeId != 0 )
        //    {
        //        productRacCategories = context.TBL_RAC_CATEGORY_TYPE.Where(x => x.RACCATEGORYTYPEID == model.racCategoryTypeId && categoryIds.Contains(x.RACCATEGORYID)).Select(x => new RacCategoryViewModel
        //        {
        //            racCategoryId = x.RACCATEGORYID,
        //            categoryName = context.TBL_RAC_CATEGORY.Where(o => o.RACCATEGORYID == x.RACCATEGORYID).Select(o => o.CATEGORYNAME).FirstOrDefault(),

        //        }).ToList();
        //    }
        //    else
        //    {
        //        productRacCategories = context.TBL_RAC_CATEGORY.Where(x => categoryIds.Contains(x.RACCATEGORYID)).Select(x => new RacCategoryViewModel
        //        {
        //            racCategoryId = x.RACCATEGORYID,
        //            categoryName = context.TBL_RAC_CATEGORY.Where(o => o.RACCATEGORYID == x.RACCATEGORYID).Select(o => o.CATEGORYNAME).FirstOrDefault(),

        //        }).ToList();

        //    }
        //    /*
        //    value: '',
        //    status: 2,
        //    */

        //    foreach (var productRacCategory in productRacCategories)
        //    {
        //        ProductRacCategory racCategory = new ProductRacCategory();
        //        List<ProductRacItem> items = new List<ProductRacItem>();

        //        if (model.racCategoryTypeId != 0 )
        //        {
        //            items = context.TBL_RAC_DEFINITION.Where(x => x.PRODUCTID == model.productId && x.RACCATEGORYTYPEID == model.racCategoryTypeId && x.ISACTIVE == true && x.DELETED == false)
        //            .Select(x => new ProductRacItem
        //            {
        //                id = x.RACDEFINITIONID,
        //                categoryId = x.RACCATEGORYID,
        //                //id = x.TBL_RAC_ITEM.RACITEMID,
        //                criteria = x.TBL_RAC_ITEM.CRITERIA,
        //                required = x.TBL_RAC_ITEM.DESCRIPTION,
        //                typeId = x.RACINPUTTYPEID,R
        //                type = context.TBL_RAC_INPUT_TYPE.FirstOrDefault(t => t.RACINPUTTYPEID == x.RACINPUTTYPEID).INPUTTAG,
        //                optionId = x.RACOPTIONID,
        //                fileUpload = x.REQUIREUPLOAD,
        //                hasException = x.ISREQUIRED == false,
        //            })
        //            .ToList();
        //        }
        //        else
        //        {
        //            items = context.TBL_RAC_DEFINITION.Where(x => x.PRODUCTID == model.productId && x.ISACTIVE == true && x.DELETED == false)
        //            .Select(x => new ProductRacItem
        //            {
        //                id = x.RACDEFINITIONID,
        //                categoryId = x.RACCATEGORYID,
        //                //id = x.TBL_RAC_ITEM.RACITEMID,
        //                criteria = x.TBL_RAC_ITEM.CRITERIA,
        //                required = x.TBL_RAC_ITEM.DESCRIPTION,
        //                typeId = x.RACINPUTTYPEID,
        //                type = context.TBL_RAC_INPUT_TYPE.FirstOrDefault(t => t.RACINPUTTYPEID == x.RACINPUTTYPEID).INPUTTAG,
        //                optionId = x.RACOPTIONID,
        //                fileUpload = x.REQUIREUPLOAD,
        //                hasException = x.ISREQUIRED == false,
        //            })
        //            .ToList();
        //        }

        //        foreach (var item in items)
        //        {
        //            item.options = item.optionId == null ? null
        //                                                : context.TBL_RAC_OPTION_ITEM
        //                                                            .Where(o => o.RACOPTIONID == item.optionId)
        //                                                            .Select(o => new ProductRacOption
        //                                                            {
        //                                                                key = o.KEY,
        //                                                                label = o.LABEL
        //                                                            })
        //                                                            .ToList();
        //        }

        //        racCategory.rows = items.Where(x => x.categoryId == productRacCategory.racCategoryId).ToList();
        //        racCategory.name = productRacCategory.categoryName;
        //        productCategories.Add(racCategory);
        //    }

        //    rac.count = productCategories.Count();
        //    rac.categories = productCategories;

        //    return rac;
        //}

        public RiskAcceptanceCriteriaViewModel GetSavedRiskAcceptanceCriteria(int productId, int? targetId)
        {
            RiskAcceptanceCriteriaViewModel rac = new RiskAcceptanceCriteriaViewModel();
            List<ProductRacCategory> productCategories = new List<ProductRacCategory>();
            List<RacCategoryViewModel> productRacCategories = new List<RacCategoryViewModel>();
            int? productClassId = null;
            var product = context.TBL_PRODUCT.Find(productId);
            if (product != null) productClassId = product.PRODUCTCLASSID;

            var categoryIds = context.TBL_RAC_DEFINITION.Join(context.TBL_RAC_DETAIL.Where(x => x.TARGETID == targetId), a => a.RACDEFINITIONID, b => b.RACDEFINITIONID, (a, b) => new { a, b })
                .Select(x => x.a.RACCATEGORYID)
                .ToList();

            productRacCategories = context.TBL_RAC_CATEGORY.Where(x => categoryIds.Contains(x.RACCATEGORYID)).Select(x => new RacCategoryViewModel
            {
                racCategoryId = x.RACCATEGORYID,
                categoryName = context.TBL_RAC_CATEGORY.Where(o => o.RACCATEGORYID == x.RACCATEGORYID).Select(o => o.CATEGORYNAME).FirstOrDefault(),
            }).ToList();


            foreach (var productRacCategory in productRacCategories)
            {
                ProductRacCategory racCategory = new ProductRacCategory();

                List<ProductRacItem> items = (from x in context.TBL_RAC_DETAIL
                                              join d in context.TBL_RAC_DEFINITION on x.RACDEFINITIONID equals d.RACDEFINITIONID
                                              where d.RACCATEGORYID == productRacCategory.racCategoryId && x.TARGETID == targetId
                                              select new ProductRacItem
                                              {
                                                  criteria = context.TBL_RAC_ITEM.Where(o => o.RACITEMID == d.RACITEMID).Select(o => o.CRITERIA).FirstOrDefault(),
                                                  value = x.ACTUALVALUE,
                                                  categoryId = d.RACCATEGORYID
                                              }).ToList();


                foreach (var item in items)
                {
                    if (item.value == "1")
                    {
                        item.value = "YES";
                    }
                    else if (item.value == "2")
                    {
                        item.value = "NO";
                    }
                    else
                    {
                        item.value = item.value;
                    }
                }

                racCategory.rows = items.Where(x => x.categoryId == productRacCategory.racCategoryId).ToList();
                racCategory.name = productRacCategory.categoryName;
                productCategories.Add(racCategory);
            }

            rac.count = productCategories.Count();
            rac.categories = productCategories;

            return rac;
        }

        private string GetRacValue(string rac)
        {
            if (rac.Length > 1)
            {
                return rac.ToString();
            }
            else
            {
                if (int.Parse(rac) == 1)
                {
                    return "YES";
                }

                return "NO";
            }

        }

        public List<RacCategoryViewModel> GetRacCategoryTypes(int productId)
        {
            var subCategories = new List<RacCategoryViewModel>();
            var teir = new List<RacCategoryViewModel>();

            var categoryId = context.TBL_RAC_DEFINITION.Where(x => x.PRODUCTID == productId && x.ISACTIVE == true && x.RACCATEGORYTYPEID !=null && x.DELETED == false)
               .Select(x => x.RACCATEGORYID).Distinct()
               .FirstOrDefault();

            teir = context.TBL_RAC_CATEGORY_TYPE.Where(x => x.RACCATEGORYID == categoryId).Select(x => new RacCategoryViewModel
            {
                racCategoryType = x.RACCATEGORYTYPE,
                racCategoryTypeId = x.RACCATEGORYTYPEID,
                racCategoryId = x.RACCATEGORYID,
                categoryTypeName = x.RACCATEGORYTYPE,
                categoryName = context.TBL_RAC_CATEGORY.Where(o => o.RACCATEGORYID == categoryId).Select(o => o.CATEGORYNAME).FirstOrDefault()

            }).ToList();

            subCategories.AddRange(teir);

            return subCategories;
        }
        public RiskAcceptanceCriteriaViewModel GetRiskAcceptanceCriteriaByProductAndTarget(int productId, int targetId)
        {
            RiskAcceptanceCriteriaViewModel rac = new RiskAcceptanceCriteriaViewModel();
            List<ProductRacCategory> productCategories = new List<ProductRacCategory>();

            var categoryIds = context.TBL_RAC_DEFINITION.Where(x => x.PRODUCTID == productId && x.ISACTIVE == true && x.DELETED == false)
                .Select(x => x.RACCATEGORYID)
                .ToList();

            var productRacCategories = context.TBL_RAC_CATEGORY.Where(x => categoryIds.Contains(x.RACCATEGORYID));

            foreach (var productRacCategory in productRacCategories)
            {
                ProductRacCategory racCategory = new ProductRacCategory();

                List<ProductRacItem> items = context.TBL_RAC_DEFINITION.Where(x => x.PRODUCTID == productId && x.ISACTIVE == true && x.DELETED == false)
                    .Join(context.TBL_RAC_DETAIL.Where(x => x.TARGETID == targetId), a => a.RACDEFINITIONID, b => b.RACDEFINITIONID, (a, b) => new { a, b })
                    .Select(x => new ProductRacItem
                    {
                        id = x.a.RACDEFINITIONID,
                        categoryId = x.a.RACCATEGORYID,
                        //id = x.TBL_RAC_ITEM.RACITEMID,
                        criteria = x.a.TBL_RAC_ITEM.CRITERIA,
                        required = x.a.TBL_RAC_ITEM.DESCRIPTION,
                        typeId = x.a.RACINPUTTYPEID,
                        type = context.TBL_RAC_INPUT_TYPE.FirstOrDefault(t => t.RACINPUTTYPEID == x.a.RACINPUTTYPEID).INPUTTAG,
                        optionId = x.a.RACOPTIONID,
                        fileUpload = x.a.REQUIREUPLOAD,
                        hasException = x.a.ISREQUIRED == false,
                        value = x.b.ACTUALVALUE
                    })
                    .ToList();


                foreach (var item in items)
                {
                    item.options = item.optionId == null ? null
                                                        : context.TBL_RAC_OPTION_ITEM
                                                                    .Where(o => o.RACOPTIONID == item.optionId)
                                                                    .Select(o => new ProductRacOption
                                                                    {
                                                                        key = o.KEY,
                                                                        label = o.LABEL
                                                                    })
                                                                    .ToList();
                }

                racCategory.rows = items.Where(x => x.categoryId == productRacCategory.RACCATEGORYID).ToList();
                racCategory.name = productRacCategory.CATEGORYNAME;
                productCategories.Add(racCategory);
            }

            rac.count = productCategories.Count();
            rac.categories = productCategories;

            return rac;
        }


        //public IEnumerable<RiskAcceptanceCriteriaViewModel> GetRacForLoanApplication(int loanApplicationId)
        //{

        //}
        #endregion

        #region RacCategoryRepository
        public IEnumerable<RacCategoryViewModel> GetRacCategorys()
        {
            return context.TBL_RAC_CATEGORY.Where(x => x.DELETED == false)
                .Select(x => new RacCategoryViewModel
                {
                    racCategoryId = x.RACCATEGORYID,
                    categoryName = x.CATEGORYNAME,
                }).OrderBy(o => o.categoryName)
                .ToList();
        }

        public RacCategoryViewModel GetRacCategory(int id)
        {
            var entity = context.TBL_RAC_CATEGORY.FirstOrDefault(x => x.RACCATEGORYID == id && x.DELETED == false);

            return new RacCategoryViewModel
            {
                racCategoryId = entity.RACCATEGORYID,
                categoryName = entity.CATEGORYNAME,
            };
        }
        public IEnumerable<RacCategoryViewModel> GetRacCategoryType(int id)
        {
            return context.TBL_RAC_CATEGORY_TYPE.Where(x => x.RACCATEGORYID == id).Select(x => new RacCategoryViewModel
            {
                racCategoryTypeId = x.RACCATEGORYTYPEID,
                racCategoryType = x.RACCATEGORYTYPE,
            }).ToList()?.OrderBy(r => r.racCategoryType);
        }

        public bool AddRacCategory(RacCategoryViewModel model)
        {
            var entity = new TBL_RAC_CATEGORY
            {
                CATEGORYNAME = model.categoryName,
                // COMPANYID = model.companyId,
                CREATEDBY = model.createdBy,
                DATETIMECREATED = general.GetApplicationDate(),
            };

            context.TBL_RAC_CATEGORY.Add(entity);

            var auditStaff = (context.TBL_STAFF.Where(x => x.STAFFID == model.createdBy).Select(x => x.STAFFCODE));
            // Audit Section ---------------------------
            //this.audit.AddAuditTrail(new TBL_AUDIT
            //{
            //    AUDITTYPEID = (short)AuditTypeEnum.RacCategoryAdded,
            //    STAFFID = model.createdBy,
            //    BRANCHID = (short)model.userBranchId,
            //    DETAIL = $"TBL_Rac Category with id '{entity.RACCATEGORYID}' created by {auditStaff}",
            //    IPADDRESS = model.userIPAddress,
            //    URL = model.applicationUrl,
            //    APPLICATIONDATE = general.GetApplicationDate(),
            //    SYSTEMDATETIME = DateTime.Now
            //});
            // Audit Section end ------------------------

            return context.SaveChanges() != 0;
        }

        public bool UpdateRacCategory(RacCategoryViewModel model, int id, UserInfo user)
        {
            var entity = this.context.TBL_RAC_CATEGORY.Find(id);
            entity.CATEGORYNAME = model.categoryName;

            entity.LASTUPDATEDBY = user.createdBy;
            entity.DATETIMEUPDATED = DateTime.Now;

            var auditStaff = (context.TBL_STAFF.Where(x => x.STAFFID == user.createdBy).Select(x => x.STAFFCODE));
            // Audit Section ---------------------------
            //this.audit.AddAuditTrail(new TBL_AUDIT
            //{
            //    AUDITTYPEID = (short)AuditTypeEnum.RacCategoryUpdated,
            //    STAFFID = user.createdBy,
            //    BRANCHID = (short)user.BranchId,
            //    DETAIL = $"TBL_Rac Category with id '{entity.RACCATEGORYID}' was updated by {auditStaff}",
            //    IPADDRESS = user.userIPAddress,
            //    URL = user.applicationUrl,
            //    APPLICATIONDATE = general.GetApplicationDate(),
            //    SYSTEMDATETIME = DateTime.Now,
            //    TARGETID = entity.RACCATEGORYID
            //});
            // Audit Section end ------------------------

            return context.SaveChanges() != 0;
        }

        public bool DeleteRacCategory(int id, UserInfo user)
        {
            var entity = this.context.TBL_RAC_CATEGORY.Find(id);
            entity.DELETED = true;
            entity.DELETEDBY = user.createdBy;
            entity.DATETIMEDELETED = general.GetApplicationDate();

            var auditStaff = (context.TBL_STAFF.Where(x => x.STAFFID == user.createdBy).Select(x => x.STAFFCODE));
            // Audit Section ---------------------------
            //this.audit.AddAuditTrail(new TBL_AUDIT
            //{
            //    AUDITTYPEID = (short)AuditTypeEnum.RacCategoryDeleted,
            //    STAFFID = user.createdBy,
            //    BRANCHID = (short)user.BranchId,
            //    DETAIL = $"TBL_Rac Category with id '{entity.RACCATEGORYID}' was deleted by {auditStaff}",
            //    IPADDRESS = user.userIPAddress,
            //    URL = user.applicationUrl,
            //    APPLICATIONDATE = general.GetApplicationDate(),
            //    SYSTEMDATETIME = DateTime.Now,
            //    TARGETID = entity.RACCATEGORYID
            //});
            // Audit Section end ------------------------

            return context.SaveChanges() != 0;
        }
        #endregion

        #region RacDefinitionRepository
        public IEnumerable<RacDefinitionViewModel> GetRacDefinitions()
        {
            return context.TBL_RAC_DEFINITION.Where(x => x.DELETED == false)
                .Select(x => new RacDefinitionViewModel
                {
                    racDefinitionId = x.RACDEFINITIONID,
                    productId = x.PRODUCTID,
                    racCategoryId = x.RACCATEGORYID,
                    isActive = x.ISACTIVE,
                    isRequired = x.ISREQUIRED,
                    racItemId = x.RACITEMID,
                    racInputTypeId = x.RACINPUTTYPEID,
                    racOptionId = x.RACOPTIONID,
                    conditionalOperatorId = x.CONDITIONALOPERATORID,
                    definedFunctionId = x.DEFINEDFUNCTIONID,
                    requireUpload = x.REQUIREUPLOAD,
                    operationId = x.OPERATIONID,
                    approvalLevelId = x.APPROVALLEVELID,
                    roleId = x.ROLEID,
                    isRacTierControlKey= x.ISRACTIERCONTROLKEY,
                    racCategoryType = context.TBL_RAC_CATEGORY_TYPE.Where(o => o.RACCATEGORYTYPEID == x.RACCATEGORYTYPEID).Select(o => o.RACCATEGORYTYPE).FirstOrDefault() ?? "N/A",
                    productName = context.TBL_PRODUCT.Where(o => o.PRODUCTID == x.PRODUCTID).Select(o => o.PRODUCTNAME).FirstOrDefault() ?? "N/A",
                    productClassName = context.TBL_PRODUCT_CLASS.Where(o => o.PRODUCTCLASSID == x.PRODUCTCLASSID).Select(o => o.PRODUCTCLASSNAME).FirstOrDefault() ?? "N/A",
                    CategoryName = context.TBL_RAC_CATEGORY.Where(o => o.RACCATEGORYID == x.RACCATEGORYID).Select(o => o.CATEGORYNAME).FirstOrDefault(),
                    racItemName = context.TBL_RAC_ITEM.Where(o => o.RACITEMID == x.RACITEMID).Select(o => o.CRITERIA).FirstOrDefault(),
                    racInputType = context.TBL_RAC_INPUT_TYPE.Where(o => o.RACINPUTTYPEID == x.RACINPUTTYPEID).Select(o => o.INPUTTYPENAME).FirstOrDefault(),
                    racOptionName = context.TBL_RAC_OPTION.Where(o => o.RACOPTIONID == x.RACOPTIONID).Select(o => o.OPTIONNAME).FirstOrDefault(),
                    conditionalOperatorName = context.TBL_CONDITIONAL_OPERATOR.Where(o => o.CONDITIONALOPERATORID == x.CONDITIONALOPERATORID).Select(o => o.OPERATORNAME).FirstOrDefault(),
                    definedFunctionName = context.TBL_DEFINED_FUNCTION.Where(o => o.DEFINEDFUNCTIONID == x.DEFINEDFUNCTIONID).Select(o => o.FUNCTIONNAME).FirstOrDefault(),
                    operationName = context.TBL_OPERATIONS.Where(o => o.OPERATIONID == x.OPERATIONID).Select(o => o.OPERATIONNAME).FirstOrDefault(),
                    approvalLevelName = context.TBL_APPROVAL_LEVEL.Where(o => o.APPROVALLEVELID == x.APPROVALLEVELID).Select(o => o.LEVELNAME).FirstOrDefault(),
                    controlAmount = x.CONTROLAMOUNT,
                    controlAmountMax = x.CONTROLAMOUNTMAX,
                    controlOptionId = x.CONTROLOPTIONID,
                    controlOption = context.TBL_RAC_OPTION_ITEM.Where(o => o.RACOPTIONID == x.RACOPTIONID).Select(o => o.LABEL).FirstOrDefault(),
                    showAtDrawDown = x.SHOWATDRAWDOWN,
                    requireComment = x.REQUIRECOMMENT,
                    currencyId = x.CURRENCYID,
                    currencyType = x.CURRENCYTYPE,
                    racCategoryTypeId = x.RACCATEGORYTYPEID,
                    productClassId = x.PRODUCTCLASSID,
                    employmentType = x.EMPLOYMENTTYPE,
                    customerTypeId = x.CUSTOMERTYPEID,
                    searchBasePlaceholder = x.SEARCHPLACEHOLDER
                }).OrderByDescending(o => o.racDefinitionId)
                .ToList();
        }

        public RacDefinitionViewModel GetRacDefinition(int id)
        {
            var entity = context.TBL_RAC_DEFINITION.FirstOrDefault(x => x.RACDEFINITIONID == id && x.DELETED == false);

            return new RacDefinitionViewModel
            {
                racDefinitionId = entity.RACDEFINITIONID,
                productId = entity.PRODUCTID,
                racCategoryId = entity.RACCATEGORYID,
                isActive = entity.ISACTIVE,
                isRequired = entity.ISREQUIRED,
                racItemId = entity.RACITEMID,
                racInputTypeId = entity.RACINPUTTYPEID,
                racOptionId = entity.RACOPTIONID,
                conditionalOperatorId = entity.CONDITIONALOPERATORID,
                definedFunctionId = entity.DEFINEDFUNCTIONID,
                requireUpload = entity.REQUIREUPLOAD,
                operationId = entity.OPERATIONID,
                approvalLevelId = entity.APPROVALLEVELID,
                roleId = entity.ROLEID,
                controlAmountMax = entity.CONTROLAMOUNTMAX,
                controlAmount = entity.CONTROLAMOUNT,
                controlOptionId = entity.CONTROLOPTIONID,
                racCategoryTypeId = entity.RACCATEGORYTYPEID,
                showAtDrawDown = entity.SHOWATDRAWDOWN,
                requireComment = entity.REQUIRECOMMENT.Value,
                currencyId = entity.CURRENCYID,
                currencyType = entity.CURRENCYTYPE,
                isRacTierControlKey=entity.ISRACTIERCONTROLKEY,
                productClassId = entity.PRODUCTCLASSID,
                productName = context.TBL_PRODUCT.Where(o => o.PRODUCTID == entity.PRODUCTID).Select(o => o.PRODUCTNAME).FirstOrDefault() ?? "N/A",
                productClassName = context.TBL_PRODUCT_CLASS.Where(o => o.PRODUCTCLASSID == entity.PRODUCTCLASSID).Select(o => o.PRODUCTCLASSNAME).FirstOrDefault() ?? "N/A",
                employmentType = entity.EMPLOYMENTTYPE,
                customerTypeId = entity.CUSTOMERTYPEID,
                searchBasePlaceholder = entity.SEARCHPLACEHOLDER
            };
        }

        public bool AddRacDefinition(RacDefinitionViewModel model)
        {
            var entity = new TBL_RAC_DEFINITION
            {
                PRODUCTID = model.productId,
                PRODUCTCLASSID = model.productClassId,
                RACCATEGORYID = model.racCategoryId,
                ISACTIVE = true,
                ISREQUIRED = model.isRequired,
                RACITEMID = model.racItemId,
                RACINPUTTYPEID = model.racInputTypeId,
                RACOPTIONID = model.racOptionId,
                CONDITIONALOPERATORID = model.conditionalOperatorId,
                DEFINEDFUNCTIONID = model.definedFunctionId,
                REQUIREUPLOAD = model.requireUpload,
                OPERATIONID = model.operationId,
                APPROVALLEVELID = model.approvalLevelId,
                ROLEID = model.roleId,
                // COMPANYID = model.companyId,
                CREATEDBY = model.createdBy,
                DATETIMECREATED = general.GetApplicationDate(),
                CONTROLOPTIONID = model.controlOptionId,
                CONTROLAMOUNTMAX = model.controlAmountMax,
                CONTROLAMOUNT = model.controlAmount,
                REQUIRECOMMENT = model.requireComment,
                RACCATEGORYTYPEID = model.racCategoryTypeId,
                SHOWATDRAWDOWN = model.showAtDrawDown,
                CURRENCYTYPE = model.currencyType,
                CURRENCYID = model.currencyId,
                ISRACTIERCONTROLKEY = model.isRacTierControlKey,
                SEARCHPLACEHOLDER = model.searchBasePlaceholder,
                EMPLOYMENTTYPE = model.employmentType,
                CUSTOMERTYPEID = model.customerTypeId,
                ISAGRICRAC = model.isAgricRac,
            };

            context.TBL_RAC_DEFINITION.Add(entity);

            var auditStaff = (context.TBL_STAFF.Where(x => x.STAFFID == model.createdBy).Select(x => x.STAFFCODE));
            // Audit Section ---------------------------
            this.audit.AddAuditTrail(new TBL_AUDIT
            {
                AUDITTYPEID = (short)AuditTypeEnum.RacDefinitionAdded,
                STAFFID = model.createdBy,
                BRANCHID = (short)model.userBranchId,
                DETAIL = $"TBL_Rac Definition with id '{entity.RACDEFINITIONID}' created by {auditStaff}",
                IPADDRESS = model.userIPAddress,
                URL = model.applicationUrl,
                APPLICATIONDATE = general.GetApplicationDate(),
                SYSTEMDATETIME = DateTime.Now
            });
            // Audit Section end ------------------------

            return context.SaveChanges() != 0;
        }

        public bool UpdateRacDefinition(RacDefinitionViewModel model, int id, UserInfo user)
        {
            var entity = context.TBL_RAC_DEFINITION.Find(id);
            entity.PRODUCTID = model.productId;
            entity.RACCATEGORYID = model.racCategoryId;
            entity.ISACTIVE = model.isActive;
            entity.ISREQUIRED = model.isRequired;
            entity.RACITEMID = model.racItemId;
            entity.RACINPUTTYPEID = model.racInputTypeId;
            entity.RACOPTIONID = model.racOptionId;
            entity.CONDITIONALOPERATORID = model.conditionalOperatorId;
            entity.DEFINEDFUNCTIONID = model.definedFunctionId;
            entity.REQUIREUPLOAD = model.requireUpload;
            entity.OPERATIONID = model.operationId;
            entity.APPROVALLEVELID = model.approvalLevelId;
            entity.ROLEID = model.roleId;
            entity.CONTROLAMOUNTMAX = model.controlAmountMax;
            entity.CONTROLAMOUNT = model.controlAmount;
            entity.CONTROLOPTIONID = model.controlOptionId;
            entity.REQUIRECOMMENT = model.requireComment;
            entity.CURRENCYTYPE = model.currencyType;
            entity.CURRENCYID = model.currencyId;
            entity.ISRACTIERCONTROLKEY = model.isRacTierControlKey;
            entity.RACCATEGORYTYPEID = model.racCategoryTypeId;
            entity.LASTUPDATEDBY = user.createdBy;
            entity.DATETIMEUPDATED = DateTime.Now;
            entity.PRODUCTCLASSID = model.productClassId;
            entity.SEARCHPLACEHOLDER = model.searchBasePlaceholder;
            entity.EMPLOYMENTTYPE = model.employmentType;
            entity.CUSTOMERTYPEID = model.customerTypeId;
            entity.ISAGRICRAC = model.isAgricRac;

            var auditStaff = (context.TBL_STAFF.Where(x => x.STAFFID == user.createdBy).Select(x => x.STAFFCODE));
            // Audit Section ---------------------------
            this.audit.AddAuditTrail(new TBL_AUDIT
            {
                AUDITTYPEID = (short)AuditTypeEnum.RacDefinitionUpdated,
                STAFFID = user.createdBy,
                BRANCHID = (short)user.BranchId,
                DETAIL = $"TBL_Rac Definitionwith id '{entity.RACDEFINITIONID}' was updated by {auditStaff}",
                IPADDRESS = user.userIPAddress,
                URL = user.applicationUrl,
                APPLICATIONDATE = general.GetApplicationDate(),
                SYSTEMDATETIME = DateTime.Now,
                TARGETID = entity.RACDEFINITIONID
            });
            // Audit Section end ------------------------
            try
            {
                return context.SaveChanges() > 0;
            }catch(Exception e)
            {
                throw e;
            }
        }

        public bool DeleteRacDefinition(int id, UserInfo user)
        {
            var entity = this.context.TBL_RAC_DEFINITION.Find(id);
            entity.DELETED = true;
            entity.DELETEDBY = user.createdBy;
            entity.DATETIMEDELETED = general.GetApplicationDate();

            var auditStaff = (context.TBL_STAFF.Where(x => x.STAFFID == user.createdBy).Select(x => x.STAFFCODE));
            // Audit Section ---------------------------
            //this.audit.AddAuditTrail(new TBL_AUDIT
            //{
            //    AUDITTYPEID = (short)AuditTypeEnum.RacDefinitionDeleted,
            //    STAFFID = user.createdBy,
            //    BRANCHID = (short)user.BranchId,
            //    DETAIL = $"TBL_Rac Definition with id '{entity.RACDEFINITIONID}' was deleted by {auditStaff}",
            //    IPADDRESS = user.userIPAddress,
            //    URL = user.applicationUrl,
            //    APPLICATIONDATE = general.GetApplicationDate(),
            //    SYSTEMDATETIME = DateTime.Now,
            //    TARGETID = entity.RACDEFINITIONID
            //});
            // Audit Section end ------------------------

            return context.SaveChanges() != 0;
        }
        #endregion

        #region DetailsRepository
        public IEnumerable<RacDetailViewModel> GetRacDetails()
        {
            return context.TBL_RAC_DETAIL.Where(x => x.DELETED == false)
                .Select(x => new RacDetailViewModel
                {
                    racDetailId = x.RACDETAILID,
                    racDefinitionId = x.RACDEFINITIONID,
                    operationId = x.OPERATIONID,
                    targetId = x.TARGETID,
                    actualValue = x.ACTUALVALUE,
                    checklistStatus = x.CHECKLISTSTATUS,
                    checklistStatus2 = x.CHECKLISTSTATUS2,
                    checklistStatus3 = x.CHECKLISTSTATUS3,
                })
                .ToList();
        }

        public RacDetailViewModel GetRacDetail(int id)
        {
            var entity = context.TBL_RAC_DETAIL.FirstOrDefault(x => x.RACDETAILID == id && x.DELETED == false);

            return new RacDetailViewModel
            {
                racDetailId = entity.RACDETAILID,
                racDefinitionId = entity.RACDEFINITIONID,
                operationId = entity.OPERATIONID,
                targetId = entity.TARGETID,
                actualValue = entity.ACTUALVALUE,
                checklistStatus = entity.CHECKLISTSTATUS,
                checklistStatus2 = entity.CHECKLISTSTATUS2,
                checklistStatus3 = entity.CHECKLISTSTATUS3,
            };
        }

        public bool AddRacDetail(RacDetailViewModel model)
        {
            var entity = new TBL_RAC_DETAIL
            {
                RACDEFINITIONID = model.racDefinitionId,
                OPERATIONID = model.operationId,
                TARGETID = model.targetId,
                ACTUALVALUE = model.actualValue,
                CHECKLISTSTATUS = model.checklistStatus,
                CHECKLISTSTATUS2 = model.checklistStatus2,
                CHECKLISTSTATUS3 = model.checklistStatus3,
                // COMPANYID = model.companyId,
                CREATEDBY = model.createdBy,
                DATETIMECREATED = general.GetApplicationDate(),
            };

            context.TBL_RAC_DETAIL.Add(entity);

            var auditStaff = (context.TBL_STAFF.Where(x => x.STAFFID == model.createdBy).Select(x => x.STAFFCODE));
            // Audit Section ---------------------------
            //this.audit.AddAuditTrail(new TBL_AUDIT
            //{
            //    AUDITTYPEID = (short)AuditTypeEnum.RacDetailAdded,
            //    STAFFID = model.createdBy,
            //    BRANCHID = (short)model.userBranchId,
            //    DETAIL = $"TBL_Rac Detail with id '{entity.RACDETAILID}' created by {auditStaff}",
            //    IPADDRESS = model.userIPAddress,
            //    URL = model.applicationUrl,
            //    APPLICATIONDATE = general.GetApplicationDate(),
            //    SYSTEMDATETIME = DateTime.Now
            //});
            // Audit Section end ------------------------

            return context.SaveChanges() != 0;
        }

        public bool UpdateRacDetail(RacDetailViewModel model, int id, UserInfo user)
        {
            var entity = this.context.TBL_RAC_DETAIL.Find(id);
            entity.RACDEFINITIONID = model.racDefinitionId;
            entity.OPERATIONID = model.operationId;
            entity.TARGETID = model.targetId;
            entity.ACTUALVALUE = model.actualValue;
            entity.CHECKLISTSTATUS = model.checklistStatus;
            entity.CHECKLISTSTATUS2 = model.checklistStatus2;
            entity.CHECKLISTSTATUS3 = model.checklistStatus3;

            entity.LASTUPDATEDBY = user.createdBy;
            entity.DATETIMEUPDATED = DateTime.Now;

            var auditStaff = (context.TBL_STAFF.Where(x => x.STAFFID == user.createdBy).Select(x => x.STAFFCODE));
            // Audit Section ---------------------------
            //this.audit.AddAuditTrail(new TBL_AUDIT
            //{
            //    AUDITTYPEID = (short)AuditTypeEnum.RacDetailUpdated,
            //    STAFFID = user.createdBy,
            //    BRANCHID = (short)user.BranchId,
            //    DETAIL = $"TBL_Rac Detail with id '{entity.RACDETAILID}' was updated by {auditStaff}",
            //    IPADDRESS = user.userIPAddress,
            //    URL = user.applicationUrl,
            //    APPLICATIONDATE = general.GetApplicationDate(),
            //    SYSTEMDATETIME = DateTime.Now,
            //    TARGETID = entity.RACDETAILID
            //});
            // Audit Section end ------------------------

            return context.SaveChanges() != 0;
        }

        public bool DeleteRacDetail(int id, UserInfo user)
        {
            var entity = this.context.TBL_RAC_DETAIL.Find(id);
            entity.DELETED = true;
            entity.DELETEDBY = user.createdBy;
            entity.DATETIMEDELETED = general.GetApplicationDate();

            var auditStaff = (context.TBL_STAFF.Where(x => x.STAFFID == user.createdBy).Select(x => x.STAFFCODE));
            // Audit Section ---------------------------
            //this.audit.AddAuditTrail(new TBL_AUDIT
            //{
            //    AUDITTYPEID = (short)AuditTypeEnum.RacDetailDeleted,
            //    STAFFID = user.createdBy,
            //    BRANCHID = (short)user.BranchId,
            //    DETAIL = $"TBL_Rac Detail with id '{entity.RACDETAILID}' was deleted by {auditStaff}",
            //    IPADDRESS = user.userIPAddress,
            //    URL = user.applicationUrl,
            //    APPLICATIONDATE = general.GetApplicationDate(),
            //    SYSTEMDATETIME = DateTime.Now,
            //    TARGETID = entity.RACDETAILID
            //});
            // Audit Section end ------------------------

            return context.SaveChanges() != 0;
        }
        #endregion

        #region RacInputTypeRepository
        public IEnumerable<RacInputTypeViewModel> GetRacInputTypes()
        {
            return context.TBL_RAC_INPUT_TYPE
                .Select(x => new RacInputTypeViewModel
                {
                    racInputTypeId = x.RACINPUTTYPEID,
                    inputTypeName = x.INPUTTYPENAME,
                    inputTag = x.INPUTTAG,
                })
                .ToList();
        }

        public RacInputTypeViewModel GetRacInputType(int id)
        {
            var entity = context.TBL_RAC_INPUT_TYPE.FirstOrDefault(x => x.RACINPUTTYPEID == id);

            return new RacInputTypeViewModel
            {
                racInputTypeId = entity.RACINPUTTYPEID,
                inputTypeName = entity.INPUTTYPENAME,
                inputTag = entity.INPUTTAG,
            };
        }

        public bool AddRacInputType(RacInputTypeViewModel model)
        {
            var entity = new TBL_RAC_INPUT_TYPE
            {
                INPUTTYPENAME = model.inputTypeName,
                INPUTTAG = model.inputTag,
                // COMPANYID = model.companyId,
                //CREATEDBY = model.createdBy,
                //DATETIMECREATED = general.GetApplicationDate(),
            };

            context.TBL_RAC_INPUT_TYPE.Add(entity);

            var auditStaff = (context.TBL_STAFF.Where(x => x.STAFFID == model.createdBy).Select(x => x.STAFFCODE));
            // Audit Section ---------------------------
            //this.audit.AddAuditTrail(new TBL_AUDIT
            //{
            //    AUDITTYPEID = (short)AuditTypeEnum.RacInputTypeAdded,
            //    STAFFID = model.createdBy,
            //    BRANCHID = (short)model.userBranchId,
            //    DETAIL = $"TBL_Rac Input Type with id '{entity.RACINPUTTYPEID}' created by {auditStaff}",
            //    IPADDRESS = model.userIPAddress,
            //    URL = model.applicationUrl,
            //    APPLICATIONDATE = general.GetApplicationDate(),
            //    SYSTEMDATETIME = DateTime.Now
            //});
            // Audit Section end ------------------------

            return context.SaveChanges() != 0;
        }

        public bool UpdateRacInputType(RacInputTypeViewModel model, int id, UserInfo user)
        {
            var entity = this.context.TBL_RAC_INPUT_TYPE.Find(id);
            entity.INPUTTYPENAME = model.inputTypeName;
            entity.INPUTTAG = model.inputTag;

            // entity.LASTUPDATEDBY = user.createdBy;
            // entity.DATETIMEUPDATED = DateTime.Now;

            var auditStaff = (context.TBL_STAFF.Where(x => x.STAFFID == user.createdBy).Select(x => x.STAFFCODE));
            // Audit Section ---------------------------
            //this.audit.AddAuditTrail(new TBL_AUDIT
            //{
            //    AUDITTYPEID = (short)AuditTypeEnum.RacInputTypeUpdated,
            //    STAFFID = user.createdBy,
            //    BRANCHID = (short)user.BranchId,
            //    DETAIL = $"TBL_Rac Input Type with id '{entity.RACINPUTTYPEID}' was updated by {auditStaff}",
            //    IPADDRESS = user.userIPAddress,
            //    URL = user.applicationUrl,
            //    APPLICATIONDATE = general.GetApplicationDate(),
            //    SYSTEMDATETIME = DateTime.Now,
            //    TARGETID = entity.RACINPUTTYPEID
            //});
            // Audit Section end ------------------------

            return context.SaveChanges() != 0;
        }

        public bool DeleteRacInputType(int id, UserInfo user)
        {
            var entity = this.context.TBL_RAC_INPUT_TYPE.Find(id);
            // entity.DELETED = true;
            // entity.DELETEDBY = user.createdBy;
            // entity.DATETIMEDELETED = general.GetApplicationDate();

            var auditStaff = (context.TBL_STAFF.Where(x => x.STAFFID == user.createdBy).Select(x => x.STAFFCODE));
            // Audit Section ---------------------------
            //this.audit.AddAuditTrail(new TBL_AUDIT
            //{
            //    AUDITTYPEID = (short)AuditTypeEnum.RacInputTypeDeleted,
            //    STAFFID = user.createdBy,
            //    BRANCHID = (short)user.BranchId,
            //    DETAIL = $"TBL_Rac Input Type with id '{entity.RACINPUTTYPEID}' was deleted by {auditStaff}",
            //    IPADDRESS = user.userIPAddress,
            //    URL = user.applicationUrl,
            //    APPLICATIONDATE = general.GetApplicationDate(),
            //    SYSTEMDATETIME = DateTime.Now,
            //    TARGETID = entity.RACINPUTTYPEID
            //});
            // Audit Section end ------------------------

            return context.SaveChanges() != 0;
        }
        #endregion

        #region  RecItemRepository
        public IEnumerable<RacItemViewModel> GetRacItems()
        {
            return context.TBL_RAC_ITEM.Where(x => x.DELETED == false)
                .Select(x => new RacItemViewModel
                {
                    racItemId = x.RACITEMID,
                    criteria = x.CRITERIA,
                    description = x.DESCRIPTION,
                }).OrderBy(o => o.criteria)
                .ToList();
        }

        public RacItemViewModel GetRacItem(int id)
        {
            var entity = context.TBL_RAC_ITEM.FirstOrDefault(x => x.RACITEMID == id && x.DELETED == false);

            return new RacItemViewModel
            {
                racItemId = entity.RACITEMID,
                criteria = entity.CRITERIA,
                description = entity.DESCRIPTION,
            };
        }

        public IEnumerable<RacItemViewModel> GetRacItem(string searchQuery)
        {
            if (searchQuery != null)
                searchQuery = searchQuery.ToUpper();

            var entity = (from x in context.TBL_RAC_ITEM
                          where x.DELETED == false && x.CRITERIA.Trim().Contains(searchQuery)
                          select new RacItemViewModel
                          {
                              racItemId = x.RACITEMID,
                              criteria = x.CRITERIA,
                              description = x.DESCRIPTION,
                          });

            return entity.ToList();
        }

        public bool AddRacItem(RacItemViewModel model)
        {
            var entity = new TBL_RAC_ITEM
            {
                CRITERIA = model.criteria,
                DESCRIPTION = model.description,
                // COMPANYID = model.companyId,
                CREATEDBY = model.createdBy,
                DATETIMECREATED = general.GetApplicationDate(),
            };

            context.TBL_RAC_ITEM.Add(entity);

            var auditStaff = (context.TBL_STAFF.Where(x => x.STAFFID == model.createdBy).Select(x => x.STAFFCODE));
            // Audit Section ---------------------------
            //this.audit.AddAuditTrail(new TBL_AUDIT
            //{
            //    AUDITTYPEID = (short)AuditTypeEnum.RacItemAdded,
            //    STAFFID = model.createdBy,
            //    BRANCHID = (short)model.userBranchId,
            //    DETAIL = $"TBL_Rac Item '{entity.DESCRIPTION}' created by {auditStaff}",
            //    IPADDRESS = model.userIPAddress,
            //    URL = model.applicationUrl,
            //    APPLICATIONDATE = general.GetApplicationDate(),
            //    SYSTEMDATETIME = DateTime.Now
            //});
            // Audit Section end ------------------------

            return context.SaveChanges() != 0;
        }

        public bool UpdateRacItem(RacItemViewModel model, int id, UserInfo user)
        {
            var entity = this.context.TBL_RAC_ITEM.Find(id);
            entity.CRITERIA = model.criteria;
            entity.DESCRIPTION = model.description;

            entity.LASTUPDATEDBY = user.createdBy;
            entity.DATETIMEUPDATED = DateTime.Now;

            var auditStaff = (context.TBL_STAFF.Where(x => x.STAFFID == user.createdBy).Select(x => x.STAFFCODE));
            // Audit Section ---------------------------
            //this.audit.AddAuditTrail(new TBL_AUDIT
            //{
            //    AUDITTYPEID = (short)AuditTypeEnum.RacItemUpdated,
            //    STAFFID = user.createdBy,
            //    BRANCHID = (short)user.BranchId,
            //    DETAIL = $"TBL_Rac Item '{entity.DESCRIPTION}' was updated by {auditStaff}",
            //    IPADDRESS = user.userIPAddress,
            //    URL = user.applicationUrl,
            //    APPLICATIONDATE = general.GetApplicationDate(),
            //    SYSTEMDATETIME = DateTime.Now,
            //    TARGETID = entity.RACITEMID
            //});
            // Audit Section end ------------------------

            return context.SaveChanges() != 0;
        }

        public bool DeleteRacItem(int id, UserInfo user)
        {
            var entity = this.context.TBL_RAC_ITEM.Find(id);
            entity.DELETED = true;
            entity.DELETEDBY = user.createdBy;
            entity.DATETIMEDELETED = general.GetApplicationDate();

            var auditStaff = (context.TBL_STAFF.Where(x => x.STAFFID == user.createdBy).Select(x => x.STAFFCODE));
            // Audit Section ---------------------------
            //this.audit.AddAuditTrail(new TBL_AUDIT
            //{
            //    AUDITTYPEID = (short)AuditTypeEnum.RacItemDeleted,
            //    STAFFID = user.createdBy,
            //    BRANCHID = (short)user.BranchId,
            //    DETAIL = $"TBL_Rac Item '{entity.DESCRIPTION}' was deleted by {auditStaff}",
            //    IPADDRESS = user.userIPAddress,
            //    URL = user.applicationUrl,
            //    APPLICATIONDATE = general.GetApplicationDate(),
            //    SYSTEMDATETIME = DateTime.Now,
            //    TARGETID = entity.RACITEMID
            //});
            // Audit Section end ------------------------

            return context.SaveChanges() != 0;
        }
        #endregion

        #region RacOptionsRepository
        public IEnumerable<RacOptionViewModel> GetRacOptions()
        {
            return context.TBL_RAC_OPTION
                .Select(x => new RacOptionViewModel
                {
                    racOptionId = x.RACOPTIONID,
                    optionName = x.OPTIONNAME,
                })
                .ToList();
        }

        public RacOptionViewModel GetRacOption(int id)
        {
            var entity = context.TBL_RAC_OPTION.FirstOrDefault(x => x.RACOPTIONID == id);

            return new RacOptionViewModel
            {
                racOptionId = entity.RACOPTIONID,
                optionName = entity.OPTIONNAME,
            };
        }

        public bool AddRacOption(RacOptionViewModel model)
        {
            var entity = new TBL_RAC_OPTION
            {
                OPTIONNAME = model.optionName,
                //  DELETED = false
                // COMPANYID = model.companyId,
            };

            context.TBL_RAC_OPTION.Add(entity);

            var auditStaff = (context.TBL_STAFF.Where(x => x.STAFFID == model.createdBy).Select(x => x.STAFFCODE));
            // Audit Section ---------------------------
            //this.audit.AddAuditTrail(new TBL_AUDIT
            //{
            //    AUDITTYPEID = (short)AuditTypeEnum.RacOptionAdded,
            //    STAFFID = model.createdBy,
            //    BRANCHID = (short)model.userBranchId,
            //    DETAIL = $"TBL_Rac Option with id '{entity.RACOPTIONID}' created by {auditStaff}",
            //    IPADDRESS = model.userIPAddress,
            //    URL = model.applicationUrl,
            //    APPLICATIONDATE = general.GetApplicationDate(),
            //    SYSTEMDATETIME = DateTime.Now
            //});
            // Audit Section end ------------------------

            return context.SaveChanges() != 0;
        }

        public bool UpdateRacOption(RacOptionViewModel model, int id, UserInfo user)
        {
            var entity = this.context.TBL_RAC_OPTION.Find(id);
            entity.OPTIONNAME = model.optionName;

            //   entity.LASTUPDATEDBY = user.createdBy;
            //   entity.DATETIMEUPDATED = DateTime.Now;

            var auditStaff = (context.TBL_STAFF.Where(x => x.STAFFID == user.createdBy).Select(x => x.STAFFCODE));
            // Audit Section ---------------------------
            //this.audit.AddAuditTrail(new TBL_AUDIT
            //{
            //    AUDITTYPEID = (short)AuditTypeEnum.RacOptionUpdated,
            //    STAFFID = user.createdBy,
            //    BRANCHID = (short)user.BranchId,
            //    DETAIL = $"TBL_Rac Option with id '{entity.RACOPTIONID}' was updated by {auditStaff}",
            //    IPADDRESS = user.userIPAddress,
            //    URL = user.applicationUrl,
            //    APPLICATIONDATE = general.GetApplicationDate(),
            //    SYSTEMDATETIME = DateTime.Now,
            //    TARGETID = entity.RACOPTIONID
            //});
            // Audit Section end ------------------------

            return context.SaveChanges() != 0;
        }

        public bool DeleteRacOption(int id, UserInfo user)
        {
            var entity = this.context.TBL_RAC_OPTION.Find(id);
            // entity.DELETED = true;
            //  entity.DELETEDBY = user.createdBy;
            // entity.DATETIMEDELETED = general.GetApplicationDate();

            var auditStaff = (context.TBL_STAFF.Where(x => x.STAFFID == user.createdBy).Select(x => x.STAFFCODE));
            // Audit Section ---------------------------
            //this.audit.AddAuditTrail(new TBL_AUDIT
            //{
            //    AUDITTYPEID = (short)AuditTypeEnum.RacOptionDeleted,
            //    STAFFID = user.createdBy,
            //    BRANCHID = (short)user.BranchId,
            //    DETAIL = $"TBL_Rac Option with id '{entity.RACOPTIONID}' was deleted by {auditStaff}",
            //    IPADDRESS = user.userIPAddress,
            //    URL = user.applicationUrl,
            //    APPLICATIONDATE = general.GetApplicationDate(),
            //    SYSTEMDATETIME = DateTime.Now,
            //    TARGETID = entity.RACOPTIONID
            //});
            // Audit Section end ------------------------

            return context.SaveChanges() != 0;
        }
        #endregion

        #region
        public IEnumerable<RacOptionItemViewModel> GetRacOptionItems()
        {
            return context.TBL_RAC_OPTION_ITEM.Where(x => x.DELETED == false)
                .Select(x => new RacOptionItemViewModel
                {
                    racOptionItemId = x.RACOPTIONITEMID,
                    label = x.LABEL,
                    key = x.KEY,
                    optionName = context.TBL_RAC_OPTION.Where(o => o.RACOPTIONID == x.RACOPTIONID).Select(o => o.OPTIONNAME).FirstOrDefault(),
                    isSystemDefined = x.ISSYSTEMDEFINED,
                })
                .ToList();
        }

        public RacOptionItemViewModel GetRacOptionItem(int id)
        {
            var entity = context.TBL_RAC_OPTION_ITEM.FirstOrDefault(x => x.RACOPTIONITEMID == id && x.DELETED == false);

            return new RacOptionItemViewModel
            {
                racOptionItemId = entity.RACOPTIONITEMID,
                label = entity.LABEL,
                key = entity.KEY,
                isSystemDefined = entity.ISSYSTEMDEFINED,
            };
        }

        public bool AddRacOptionItem(RacOptionItemViewModel model)
        {
            var entity = new TBL_RAC_OPTION_ITEM
            {
                LABEL = model.label,
                KEY = model.key,
                ISSYSTEMDEFINED = model.isSystemDefined,
                RACOPTIONID = model.racOptionId,
                // COMPANYID = model.companyId,
                CREATEDBY = model.createdBy,
                DATETIMECREATED = general.GetApplicationDate(),
            };

            context.TBL_RAC_OPTION_ITEM.Add(entity);

            //var auditStaff = (context.TBL_STAFF.Where(x => x.STAFFID == model.createdBy).Select(x => x.STAFFCODE));
            // Audit Section ---------------------------
            //this.audit.AddAuditTrail(new TBL_AUDIT
            //{
            //    AUDITTYPEID = (short)AuditTypeEnum.RacOptionItemAdded,
            //    STAFFID = model.createdBy,
            //    BRANCHID = (short)model.userBranchId,
            //    DETAIL = $"TBL_Rac Option Item with id '{entity.RACOPTIONITEMID}' created by {auditStaff}",
            //    IPADDRESS = model.userIPAddress,
            //    URL = model.applicationUrl,
            //    APPLICATIONDATE = general.GetApplicationDate(),
            //    SYSTEMDATETIME = DateTime.Now
            //});
            // Audit Section end ------------------------

            return context.SaveChanges() != 0;
        }

        public bool UpdateRacOptionItem(RacOptionItemViewModel model, int id, UserInfo user)
        {
            var entity = this.context.TBL_RAC_OPTION_ITEM.Find(id);
            entity.LABEL = model.label;
            entity.KEY = model.key;
            entity.ISSYSTEMDEFINED = model.isSystemDefined;
            entity.RACOPTIONID = model.racOptionId;
            entity.LASTUPDATEDBY = user.createdBy;
            entity.DATETIMEUPDATED = DateTime.Now;

           // var auditStaff = (context.TBL_STAFF.Where(x => x.STAFFID == user.createdBy).Select(x => x.STAFFCODE));
            // Audit Section ---------------------------
            //this.audit.AddAuditTrail(new TBL_AUDIT
            //{
            //    AUDITTYPEID = (short)AuditTypeEnum.RacOptionItemUpdated,
            //    STAFFID = user.createdBy,
            //    BRANCHID = (short)user.BranchId,
            //    DETAIL = $"TBL_Rac Option Item with id '{entity.RACOPTIONITEMID}' was updated by {auditStaff}",
            //    IPADDRESS = user.userIPAddress,
            //    URL = user.applicationUrl,
            //    APPLICATIONDATE = general.GetApplicationDate(),
            //    SYSTEMDATETIME = DateTime.Now,
            //    TARGETID = entity.RACOPTIONITEMID
            //});
            // Audit Section end ------------------------

            return context.SaveChanges() != 0;
        }

        public bool DeleteRacOptionItem(int id, UserInfo user)
        {
            var entity = this.context.TBL_RAC_OPTION_ITEM.Find(id);
            entity.DELETED = true;
            entity.DELETEDBY = user.createdBy;
            entity.DATETIMEDELETED = general.GetApplicationDate();

            var auditStaff = (context.TBL_STAFF.Where(x => x.STAFFID == user.createdBy).Select(x => x.STAFFCODE));
            // Audit Section ---------------------------
            //this.audit.AddAuditTrail(new TBL_AUDIT
            //{
            //    AUDITTYPEID = (short)AuditTypeEnum.RacOptionItemDeleted,
            //    STAFFID = user.createdBy,
            //    BRANCHID = (short)user.BranchId,
            //    DETAIL = $"TBL_Rac Option Item with id '{entity.RACOPTIONITEMID}' was deleted by {auditStaff}",
            //    IPADDRESS = user.userIPAddress,
            //    URL = user.applicationUrl,
            //    APPLICATIONDATE = general.GetApplicationDate(),
            //    SYSTEMDATETIME = DateTime.Now,
            //    TARGETID = entity.RACOPTIONITEMID
            //});
            // Audit Section end ------------------------

            return context.SaveChanges() != 0;
        }
        #endregion


        public IEnumerable<ConditionalOperatorViewModel> GetConditionalOperators()
        {
            return context.TBL_CONDITIONAL_OPERATOR
                .Select(x => new ConditionalOperatorViewModel
                {
                    conditionalOperatorId = x.CONDITIONALOPERATORID,
                    operatorName = x.OPERATORNAME,
                })
                .ToList();
        }
        public IEnumerable<DefinedFunctionViewModel> GetDefinedFunctions()
        {
            return context.TBL_DEFINED_FUNCTION
                .Select(x => new DefinedFunctionViewModel
                {
                    definedFunctionId = x.DEFINEDFUNCTIONID,
                    functionName = x.FUNCTIONNAME,
                    description = x.DESCRIPTION,
                    isSystemDefined = x.ISSYSTEMDEFINED,
                })
                .ToList();
        }
        public IEnumerable<ApprovalLevelViewModel> GetApprovalLevel(int companyId)
        {
            var data = (from a in context.TBL_APPROVAL_LEVEL
                        where a.DELETED == false && a.TBL_APPROVAL_GROUP.COMPANYID == companyId
                        select new ApprovalLevelViewModel
                        {
                            approvalLevelId = a.APPROVALLEVELID,
                            levelName = a.LEVELNAME,
                        });

            return data.Distinct().OrderBy(o => o.levelName);
        }

        public bool AddRacCategoryType(RacCategoryTypeViewModel model)
        {
            var entity = new TBL_RAC_CATEGORY_TYPE
            {
                RACCATEGORYID = model.racCategoryTypeId,
                RACCATEGORYTYPE = model.racCategoryType
            };
            context.TBL_RAC_CATEGORY_TYPE.Add(entity);

            return context.SaveChanges() > 0;
        }

        public IEnumerable<RacCategoryTypeViewModel> GetAllRacCategoryType()
        {
            var record = from rct in context.TBL_RAC_CATEGORY_TYPE
                         join rc in context.TBL_RAC_CATEGORY on rct.RACCATEGORYID equals rc.RACCATEGORYID
                         select new RacCategoryTypeViewModel
                         {
                             racCategoryTypeId = rct.RACCATEGORYTYPEID,
                             racCategoryId = rct.RACCATEGORYID,
                             racCategoryType = rct.RACCATEGORYTYPE,
                             racCategoryName = rc.CATEGORYNAME
                         };

            return record.ToList();
        }

        public RacCategoryTypeViewModel GetRacCategoryTypeById(int id)
        {
            return context.TBL_RAC_CATEGORY_TYPE.Where(x => x.RACCATEGORYTYPEID == id)
                .Select(x => new RacCategoryTypeViewModel
                {
                    racCategoryId = x.RACCATEGORYID,
                    racCategoryType = x.RACCATEGORYTYPE
                })
                .FirstOrDefault();
        }

        public bool DeleteRacCategoryTypeById(int id)
        {
            var result = context.TBL_RAC_CATEGORY_TYPE.Where(x => x.RACCATEGORYTYPEID == id).FirstOrDefault();
            if (result != null) context.TBL_RAC_CATEGORY_TYPE.Remove(result);

            return context.SaveChanges() != 0;
        }

        public bool UpdateRacCategoryTypeById(RacCategoryTypeViewModel model, int id)
        {
            var result = context.TBL_RAC_CATEGORY_TYPE.Find(id);

            result.RACCATEGORYTYPE = model.racCategoryType;
            result.RACCATEGORYID = model.racCategoryTypeId;

            return context.SaveChanges() != 0;
        }
        public bool RacCategoryTypeExist(int productid, int racCategoryTypeId)
        {
            return context.TBL_RAC_DEFINITION.Any(o => o.PRODUCTID == productid && o.RACCATEGORYTYPEID == racCategoryTypeId);
        }

        public List<RacCategoryTypeViewModel> GetRacDetails(int targetId)
        {
            List<RacCategoryTypeViewModel> racItems = new List<RacCategoryTypeViewModel>();
            var racDetailsIds = context.TBL_RAC_DETAIL.Where(x => x.TARGETID == targetId).Select(x => x.RACDEFINITIONID);
            foreach (var racDefId in racDetailsIds)
            {
                racItems = (from a in context.TBL_RAC_DEFINITION.Where(a => a.RACDEFINITIONID == racDefId)
                            join b in context.TBL_RAC_ITEM on a.RACITEMID equals b.RACITEMID

                            select new RacCategoryTypeViewModel
                            {
                                 criteria = b.CRITERIA,
                                 //isActive = a.ISACTIVE
                            })
               .ToList();
               
            }
            return racItems;
        }
       
    }



}

// kernel.Bind<IRiskAcceptanceCriteriaRepository>().To<RiskAcceptanceCriteriaRepository>();
// RiskAcceptanceCriteriaAdded = ???, RiskAcceptanceCriteriaUpdated = ???, RiskAcceptanceCriteriaDeleted = ???,
