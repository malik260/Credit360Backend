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


        public RiskAcceptanceCriteriaViewModel GetRiskAcceptanceCriteriaByProduct(int productId)
        {
            RiskAcceptanceCriteriaViewModel rac = new RiskAcceptanceCriteriaViewModel();
            List<ProductRacCategory> productCategories = new List<ProductRacCategory>();
            
            var categoryIds = context.TBL_RAC_DEFINITION.Where(x => x.PRODUCTID == productId)
                .Select(x => x.RACCATEGORYID)
                .ToList();

            var productRacCategories = context.TBL_RAC_CATEGORY.Where(x => categoryIds.Contains(x.RACCATEGORYID));

            /*
            value: '',
            status: 2,
            */

            foreach (var productRacCategory in productRacCategories)
            {
                ProductRacCategory racCategory = new ProductRacCategory();

                List<ProductRacItem> items = context.TBL_RAC_DEFINITION.Where(x => x.PRODUCTID == productId)
                    .Select(x => new ProductRacItem
                    {
                        id = x.TBL_RAC_ITEM.RACITEMID,
                        criteria = x.TBL_RAC_ITEM.CRITERIA,
                        required = x.TBL_RAC_ITEM.DESCRIPTION,
                        typeId = x.RACINPUTTYPEID,
                        type = context.TBL_RAC_INPUT_TYPE.FirstOrDefault(t => t.RACINPUTTYPEID == x.RACINPUTTYPEID).INPUTTAG,
                        optionId = x.RACOPTIONID,
                        fileUpload = x.REQUIREUPLOAD,
                        hasException = x.ISREQUIRED == false,
                        options = x.RACOPTIONID == null ? new List<ProductRacOption>() 
                                                        : context.TBL_RAC_OPTION_ITEM
                                                                    .Where(o => o.RACOPTIONID == (int)x.RACOPTIONID)
                                                                    .Select(o => new ProductRacOption
                                                                    {
                                                                        key = o.KEY,
                                                                        label = o.LABEL
                                                                    })
                                                                    .ToList()
                    })
                    .ToList();

                racCategory.rows = items;
                racCategory.name = productRacCategory.CATEGORYNAME;
                productCategories.Add(racCategory);
            }

            rac.categories = productCategories;

            return rac;
        }
        
    }
}

           // kernel.Bind<IRiskAcceptanceCriteriaRepository>().To<RiskAcceptanceCriteriaRepository>();
           // RiskAcceptanceCriteriaAdded = ???, RiskAcceptanceCriteriaUpdated = ???, RiskAcceptanceCriteriaDeleted = ???,
