using FintrakBanking.Common.Enum;
using FintrakBanking.Entities.Models;
using FintrakBanking.Interfaces.Admin;
using FintrakBanking.Interfaces.Setups.General;
using FintrakBanking.Interfaces.Setups.Risk;
using FintrakBanking.ViewModels;
using FintrakBanking.ViewModels.Setups;
using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using System.Threading.Tasks;

namespace FintrakBanking.Repositories.Setups.Risk
{
    [Export(typeof(IRiskSetupRepository))]
    [PartCreationPolicy(CreationPolicy.NonShared)]
    public class RiskSetupRepository : IRiskSetupRepository
    {
        private FinTrakBankingContext context;
        private IGeneralSetupRepository genSetup;
        private IAuditTrailRepository auditTrail;
        public RiskSetupRepository(FinTrakBankingContext _context,
            IGeneralSetupRepository _genSetup,
            IAuditTrailRepository _auditTrail)
        {
            this.context = _context;
            genSetup = _genSetup;
            auditTrail = _auditTrail;
        }

        #region Risk Assessment Indexs

        public async Task<bool> AddRiskAssessmentIndexs(RiskAssessmentIndexViewModels entity)
        {
            var index = new tbl_Risk_Assessment_Index
            {
                DateTimeCreated = genSetup.GetApplicationDate().Date,
                Name = entity.name,
                Description = entity.description,
                Weight = entity.weight,
                CompanyId = entity.companyId,
                CreatedBy = entity.createdBy,
                ParentId = entity.parentId,
                ItemLevel =entity.itemLevel ,
                 IndexTypeId = entity.indexTypeId ,
                RiskAssessmentTitleId = entity.riskAssessmentTitleId

                  

            };
            this.context.tbl_Risk_Assessment_Index.Add(index);

            // Audit Section ---------------------------
            var audit = new tbl_Audit
            {
                AuditTypeId = (short)AuditTypeEnum.RiskAssessmentIndexAdd,
                StaffId = entity.createdBy,
                BranchId = (short)entity.userBranchId,
                Detail = $"Added Risk assessment index: { entity.name } ",
                IPAddress = entity.userIPAddress,
                Url = entity.applicationUrl,
                ApplicationDate = genSetup.GetApplicationDate(),
                SystemDateTime = DateTime.Now
            };

            this.auditTrail.AddAuditTrail(audit);

            //end of Audit section -------------------------------
            return await context.SaveChangesAsync() != 0;
        }

        public async Task<bool> DeleteRiskAssessmentIndex(int riskId, UserInfo user)
        {
            var index = context.tbl_Risk_Assessment_Index.SingleOrDefault(c => c.RiskId == riskId);
            index.Deleted = true;
            index.DateTimeDeleted = genSetup.GetApplicationDate();
            index.DeletedBy = (int)user.staffId;

            // Audit Section ---------------------------
            var audit = new tbl_Audit
            {
                AuditTypeId = (short)AuditTypeEnum.RiskAssessmentIndexDelete,
                StaffId = user.staffId,
                BranchId = (short)user.BranchId,
                Detail = $"Deleted Risk assessment index: { index.Name } ",
                IPAddress = user.userIPAddress,
                Url = user.applicationUrl,
                ApplicationDate = genSetup.GetApplicationDate(),
                SystemDateTime = DateTime.Now
            };

            this.auditTrail.AddAuditTrail(audit);

            //end of Audit section -------------------------------
            return await context.SaveChangesAsync() != 0;
        }

        private IEnumerable<RiskAssessmentIndexViewModels> GetRiskAssessmentIndex(int companyId)
        {
            return context.tbl_Risk_Assessment_Index.Where(a => a.CompanyId == companyId && a.Deleted == false).Select(a => new
                        RiskAssessmentIndexViewModels
            {
                companyId = a.CompanyId,
                createdBy = a.CreatedBy,
                dateTimeCreated = a.DateTimeCreated,
                description = a.Description,
                name = a.Name,
                 indexTypeId = a.IndexTypeId ,
                riskAssessmentTitleId = a.RiskAssessmentTitleId,
                itemLevel = a.ItemLevel,
                weight = a.Weight,
                riskAssessmentTitle = context.tbl_Risk_Assessment_Title
                                    .SingleOrDefault(c => c.RiskAssessmentTitleId == a.RiskAssessmentTitleId).RiskTitle,

                riskId = a.RiskId,

                parentId = a.ParentId
            });

        }

        public RiskAssessmentIndexViewModels GetRiskAssessmentIndexById(int riskId, int companyId)
        {
            return GetRiskAssessmentIndex(companyId).SingleOrDefault(c => c.riskId == riskId);
        }

        public IEnumerable<RiskAssessmentIndexViewModels> GetRiskAssessmentIndexByRiskTitle(int riskTitleId, int companyId)
        {
            return GetRiskAssessmentIndex(companyId).Where(c => c.riskAssessmentTitleId == riskTitleId);
        }

        public IEnumerable<RiskAssessmentIndexViewModels> GetRiskAssessmentIndexByItemLevel(int levelId, int companyId)
        {

            return GetRiskAssessmentIndex(companyId).Where(c => c.itemLevel >= levelId);
        }

        public IEnumerable<RiskAssessmentIndexViewModels> GetRiskAssessmentIndexByParent(int parentId, int companyId)
        {
            return GetRiskAssessmentIndex(companyId).Where(c => c.parentId == parentId);
        }

        public async Task<bool> UpdateRiskAssessmentIndex(int riskId, RiskAssessmentIndexViewModels entity)
        {
            var index = context.tbl_Risk_Assessment_Index.Find(riskId);
            index.Name = entity.name;
            index.Description = entity.description;
            index.ParentId = entity.parentId;
            index.ItemLevel = entity.itemLevel;
            index.IndexTypeId = entity.indexTypeId;
            index.RiskAssessmentTitleId = entity.riskAssessmentTitleId;
            index.CompanyId = entity.companyId;
            index.Weight = entity.weight;
            index.DateTimeUpdated = genSetup.GetApplicationDate().Date;
            index.LastUpdatedBy = entity.lastUpdatedBy;

            // Audit Section ---------------------------
            var audit = new tbl_Audit
            {
                AuditTypeId = (short)AuditTypeEnum.RiskAssessmentIndexDelete,
                StaffId = entity.lastUpdatedBy,
                BranchId = (short)entity.userBranchId,
                Detail = $"Deleted Risk assessment index: { index.Name } ",
                IPAddress = entity.userIPAddress,
                Url = entity.applicationUrl,
                ApplicationDate = genSetup.GetApplicationDate(),
                SystemDateTime = DateTime.Now
            };

            this.auditTrail.AddAuditTrail(audit);

            //end of Audit section -------------------------------
            return await context.SaveChangesAsync() != 0;
        }
        #endregion Risk Assessment Indexs

        #region RiskRating

        public async Task<bool> AddRiskRating(RiskRatingViewModel entity)
        {
            var rating = new tbl_Risk_Rating
            {
                AdvicedRate = entity.advicedRate,
                DateTimeCreated = DateTime.Now.Date,
                MaxRange = entity.maxRange,
                MinRange = entity.minRange,
                Rates = entity.rates,
                ProductId = entity.productId,
                RatesDescription = entity.ratesDescription
            };
            this.context.tbl_Risk_Rating.Add(rating);
            return await context.SaveChangesAsync() != 0;
        }

        public async Task<bool> DeleteRiskRating(int ratingId, RiskRatingViewModel entity)
        {
            var rating = (from a in context.tbl_Risk_Rating where a.RiskRatingId == ratingId select a).SingleOrDefault();
            rating.DateTimeDeleted = DateTime.Now.Date;
            rating.DeletedBy = entity.deletedBy;
            rating.Deleted = true;
            return await context.SaveChangesAsync() != 0;
        }

        public IEnumerable<RiskRatingViewModel> GetRiskRating()
        {
            var rating = (from a in context.tbl_Risk_Rating
                          where a.Deleted == false
                          select new RiskRatingViewModel
                          {
                              riskRatingId = a.RiskRatingId,
                              advicedRate = a.AdvicedRate,
                              maxRange = a.MaxRange,
                              minRange = a.MinRange,
                              rates = a.Rates,
                              ratesDescription = a.RatesDescription,
                              productId = (short) a.RiskRatingId
                          }).ToList();
            return rating;
        }

        public IEnumerable<RiskRatingViewModel> GetRiskRatingByCompanyId(int companyId)
        {
            var rating = (from a in context.tbl_Risk_Rating
                          where a.Deleted == false && a.CompanyId == companyId
                          select new RiskRatingViewModel
                          {
                              riskRatingId = a.RiskRatingId,
                              advicedRate = a.AdvicedRate,
                              companyId = (short)a.CompanyId,
                              createdBy = a.CreatedBy,
                              dateTimeCreated = a.DateTimeCreated,
                              dateTimeUpdated = a.DateTimeUpdated,
                              lastUpdatedBy = a.LastUpdatedBy.Value,
                              maxRange = a.MaxRange,
                              minRange = a.MinRange,
                              rates = a.Rates,
                              ratesDescription = a.RatesDescription,
                              productId = (short) a.RiskRatingId
                          }).ToList();
            return rating;
        }

        public IEnumerable<RiskRatingViewModel> GetRiskRatingByProductId(int ratingId)
        {
            var rating = (from a in context.tbl_Risk_Rating
                          where a.Deleted == false && a.RiskRatingId == ratingId
                          select new RiskRatingViewModel
                          {
                              riskRatingId = a.RiskRatingId,
                              advicedRate = a.AdvicedRate,
                              companyId = a.CompanyId,
                              createdBy = a.CreatedBy,
                              dateTimeCreated = a.DateTimeCreated,
                              dateTimeUpdated = a.DateTimeUpdated,
                              lastUpdatedBy = a.LastUpdatedBy.Value,
                              maxRange = a.MaxRange,
                              minRange = a.MinRange,
                              rates = a.Rates,
                              ratesDescription = a.RatesDescription,
                              productId = (short)a.RiskRatingId
                          }).ToList();
            return rating;
        }

        public async Task<bool> UpdateRiskRating(int ratingId, RiskRatingViewModel entity)
        {
            var rating = (from a in context.tbl_Risk_Rating where a.RiskRatingId == ratingId select a).SingleOrDefault();
            rating.AdvicedRate = entity.advicedRate;
            rating.CompanyId = (short)entity.companyId;
            rating.MaxRange = entity.maxRange;
            rating.MinRange = entity.minRange;
            rating.ProductId = entity.productId;
            rating.Rates = entity.rates;
            rating.RatesDescription = entity.ratesDescription;
            rating.DateTimeUpdated = DateTime.Now.Date;
            rating.LastUpdatedBy = entity.lastUpdatedBy;
            return await context.SaveChangesAsync() != 0;
        }
        #endregion RiskRating

        #region Risk Assessment Title
        public async Task<bool> AddRiskAssessmentTitle(RiskAssessmentTitleViewModels entity)
        {
            var title = new tbl_Risk_Assessment_Title
            {
                RiskTitle = entity.riskTitle,
                DateTimeCreated = genSetup.GetApplicationDate().Date,
                CreatedBy = entity.createdBy,
                CompanyId = entity.companyId,
                RiskTypeId = entity.riskTypeId,
                ProductId = entity.productId,
            };
            this.context.tbl_Risk_Assessment_Title.Add(title);

            // Audit Section ---------------------------
            var audit = new tbl_Audit
            {
                AuditTypeId = (short)AuditTypeEnum.RiskAssessmentTitleAdd,
                StaffId = entity.createdBy,
                BranchId = (short)entity.userBranchId,
                Detail = $"Added Risk assessment title: { entity.riskTitle } ",
                IPAddress = entity.userIPAddress,
                Url = entity.applicationUrl,
                ApplicationDate = genSetup.GetApplicationDate(),
                SystemDateTime = DateTime.Now
            };

            this.auditTrail.AddAuditTrail(audit);

            //end of Audit section -------------------------------
            return await context.SaveChangesAsync() != 0;
        }

        public async Task<bool> UpdateRiskAssessmentTitle(int riskAssessmentTitleId, RiskAssessmentTitleViewModels entity)
        {
            var title = context.tbl_Risk_Assessment_Title.SingleOrDefault(c => c.RiskAssessmentTitleId == riskAssessmentTitleId);
            title.RiskTitle = entity.riskTitle;
            title.DateTimeUpdated = genSetup.GetApplicationDate();
            title.RiskTypeId = entity.riskTypeId;
            title.ProductId = entity.productId;
            title.LastUpdatedBy = entity.lastUpdatedBy;

            // Audit Section ---------------------------
            var audit = new tbl_Audit
            {
                AuditTypeId = (short)AuditTypeEnum.RiskAssessmentTitleUpdate,
                StaffId = entity.createdBy,
                BranchId = (short)entity.userBranchId,
                Detail = $"Update Risk assessment title: { entity.riskTitle } ",
                IPAddress = entity.userIPAddress,
                Url = entity.applicationUrl,
                ApplicationDate = genSetup.GetApplicationDate(),
                SystemDateTime = DateTime.Now
            };

            this.auditTrail.AddAuditTrail(audit);

            //end of Audit section -------------------------------
            return await context.SaveChangesAsync() != 0;
        }

        public async Task<bool> DeleteRiskAssessmentTitle(int riskAssessmentTitleId, UserInfo user)
        {
            var title = context.tbl_Risk_Assessment_Title.SingleOrDefault(c => c.RiskAssessmentTitleId == riskAssessmentTitleId);
            title.Deleted = true;
            title.DateTimeDeleted = genSetup.GetApplicationDate();
            title.DeletedBy = user.staffId;


            // Audit Section ---------------------------
            var audit = new tbl_Audit
            {
                AuditTypeId = (short)AuditTypeEnum.RiskAssessmentTitleDelete,
                StaffId = user.staffId,
                BranchId = (short)user.BranchId,
                Detail = $"Delete Risk assessment title: { title.RiskTitle } ",
                IPAddress = user.userIPAddress,
                Url = user.applicationUrl,
                ApplicationDate = genSetup.GetApplicationDate(),
                SystemDateTime = DateTime.Now
            };

            this.auditTrail.AddAuditTrail(audit);

            //end of Audit section -------------------------------
            return await context.SaveChangesAsync() != 0;
        }

        public RiskAssessmentTitleViewModels GetRiskAssessmentTitleById(int riskAssessmentTitleId, int companyId)
        {
            return RiskAssessmentTitle(companyId).SingleOrDefault(c => c.riskAssessmentTitleId == riskAssessmentTitleId);
        }

        public IEnumerable<RiskAssessmentTitleViewModels> GetRiskAssessmentTitle(int companyId)
        {
            return RiskAssessmentTitle(companyId);
        }

        public IEnumerable<RiskAssessmentTitleViewModels> GetRiskAssessmentTitleByProductId(int productId, int companyId)
        {
            return RiskAssessmentTitle(companyId).Where(c => c.productId == productId);
        }

        public IEnumerable<RiskAssessmentTitleViewModels> GetRiskAssessmentTitleByRiskType(int riskTypeId, int companyId)
        {
            return RiskAssessmentTitle(companyId).Where(c => c.riskTypeId == riskTypeId);
        }

        IEnumerable<RiskAssessmentTitleViewModels> RiskAssessmentTitle(int companyId)
        {

            return context.tbl_Risk_Assessment_Title.Where(c => c.CompanyId == companyId).Select(c => new RiskAssessmentTitleViewModels()
            {
                riskAssessmentTitleId = c.RiskAssessmentTitleId,
                companyId = c.CompanyId,
                createdBy = c.CreatedBy,
                riskTitle = c.RiskTitle,
                productId = c.ProductId,
                riskTypeId = c.RiskTypeId
            });
        }

        #endregion Risk Assessment Title

    }
}