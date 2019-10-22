using FintrakBanking.Common;
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
            var index = new TBL_RISK_ASSESSMENT_INDEX
            {
                DATETIMECREATED = genSetup.GetApplicationDate().Date,
                NAME = entity.name,
                DESCRIPTION = entity.description,
                WEIGHT = entity.weight,
                COMPANYID = entity.companyId,
                CREATEDBY = entity.createdBy,
                PARENTID = entity.parentId,
                ITEMLEVEL = entity.itemLevel,
                INDEXTYPEID = entity.indexTypeId,
                RISKASSESSMENTTITLEID = entity.riskAssessmentTitleId



            };
            this.context.TBL_RISK_ASSESSMENT_INDEX.Add(index);

            // Audit Section ---------------------------
            var audit = new TBL_AUDIT
            {
                AUDITTYPEID = (short)AuditTypeEnum.RiskAssessmentIndexAdd,
                STAFFID = entity.createdBy,
                BRANCHID = (short)entity.userBranchId,
                DETAIL = $"Added Risk assessment index: { entity.name } ",
                IPADDRESS = CommonHelpers.GetLocalIpAddress(),
                URL = entity.applicationUrl,
                APPLICATIONDATE = genSetup.GetApplicationDate(),
                SYSTEMDATETIME = DateTime.Now,
                DEVICENAME = CommonHelpers.GetDeviceName(),
                OSNAME = CommonHelpers.FriendlyName(),
            };

            this.auditTrail.AddAuditTrail(audit);

            //end of Audit section -------------------------------
            return await context.SaveChangesAsync() != 0;
        }

        public async Task<bool> DeleteRiskAssessmentIndex(int riskId, UserInfo user)
        {
            var index = context.TBL_RISK_ASSESSMENT_INDEX.FirstOrDefault(c => c.RISKID == riskId);
            index.DELETED = true;
            index.DATETIMEDELETED = genSetup.GetApplicationDate();
            index.DELETEDBY = (int)user.staffId;

            // Audit Section ---------------------------
            var audit = new TBL_AUDIT
            {
                AUDITTYPEID = (short)AuditTypeEnum.RiskAssessmentIndexDelete,
                STAFFID = user.staffId,
                BRANCHID = (short)user.BranchId,
                DETAIL = $"Deleted Risk assessment index: { index.NAME } ",
                IPADDRESS = CommonHelpers.GetLocalIpAddress(),
                URL = user.applicationUrl,
                APPLICATIONDATE = genSetup.GetApplicationDate(),
                SYSTEMDATETIME = DateTime.Now,
                DEVICENAME = CommonHelpers.GetDeviceName(),
                OSNAME = CommonHelpers.FriendlyName(),
            };

            this.auditTrail.AddAuditTrail(audit);

            //end of Audit section -------------------------------
            return await context.SaveChangesAsync() != 0;
        }

        private IEnumerable<RiskAssessmentIndexViewModels> GetRiskAssessmentIndex(int companyId)
        {
            return context.TBL_RISK_ASSESSMENT_INDEX.Where(a => a.COMPANYID == companyId && a.DELETED == false)
                .Select(a => new RiskAssessmentIndexViewModels
                {
                    companyId = a.COMPANYID,
                    createdBy = a.CREATEDBY,
                    dateTimeCreated = a.DATETIMECREATED,
                    description = a.DESCRIPTION,
                    name = a.NAME,
                    indexTypeId = a.INDEXTYPEID,
                    riskAssessmentTitleId = a.RISKASSESSMENTTITLEID,
                    itemLevel = a.ITEMLEVEL,
                    weight = a.WEIGHT,
                    riskAssessmentTitle = context.TBL_RISK_ASSESSMENT_TITLE
                                    .FirstOrDefault(c => c.RISKASSESSMENTTITLEID == a.RISKASSESSMENTTITLEID).RISKTITLE,

                    riskId = a.RISKID,

                    parentId = a.PARENTID
                });

        }

        public RiskAssessmentIndexViewModels GetRiskAssessmentIndexById(int riskId, int companyId)
        {
            return GetRiskAssessmentIndex(companyId).FirstOrDefault(c => c.riskId == riskId);
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
            var index = context.TBL_RISK_ASSESSMENT_INDEX.Find(riskId);
            index.NAME = entity.name;
            index.DESCRIPTION = entity.description;
            index.PARENTID = entity.parentId;
            index.ITEMLEVEL = entity.itemLevel;
            index.INDEXTYPEID = entity.indexTypeId;
            index.RISKASSESSMENTTITLEID = entity.riskAssessmentTitleId;
            index.COMPANYID = entity.companyId;
            index.WEIGHT = entity.weight;
            index.DATETIMEUPDATED = genSetup.GetApplicationDate().Date;
            index.LASTUPDATEDBY = entity.lastUpdatedBy;

            // Audit Section ---------------------------
            var audit = new TBL_AUDIT
            {
                AUDITTYPEID = (short)AuditTypeEnum.RiskAssessmentIndexDelete,
                STAFFID = entity.lastUpdatedBy,
                BRANCHID = (short)entity.userBranchId,
                DETAIL = $"Deleted Risk assessment index: { index.NAME } ",
                IPADDRESS = CommonHelpers.GetLocalIpAddress(),
                URL = entity.applicationUrl,
                APPLICATIONDATE = genSetup.GetApplicationDate(),
                SYSTEMDATETIME = DateTime.Now,
                DEVICENAME = CommonHelpers.GetDeviceName(),
                OSNAME = CommonHelpers.FriendlyName(),
            };

            this.auditTrail.AddAuditTrail(audit);

            //end of Audit section -------------------------------
            return await context.SaveChangesAsync() != 0;
        }
        #endregion Risk Assessment Indexs

        #region RiskRating

        public async Task<bool> AddRiskRating(RiskRatingViewModel entity)
        {
            var rating = new TBL_RISK_RATING
            {
                ADVICEDRATE = entity.advicedRate,
                DATETIMECREATED = DateTime.Now.Date,
                MAXRANGE = entity.maxRange,
                MINRANGE = entity.minRange,
                RATES = entity.rates,
                PRODUCTID = entity.productId,
                RATESDESCRIPTION = entity.ratesDescription
            };
            this.context.TBL_RISK_RATING.Add(rating);
            return await context.SaveChangesAsync() != 0;
        }

        public async Task<bool> DeleteRiskRating(int ratingId, RiskRatingViewModel entity)
        {
            var rating = (from a in context.TBL_RISK_RATING where a.RISKRATINGID == ratingId select a).FirstOrDefault();
            rating.DATETIMEDELETED = DateTime.Now.Date;
            rating.DELETEDBY = entity.deletedBy;
            rating.DELETED = true;
            return await context.SaveChangesAsync() != 0;
        }

        public IEnumerable<RiskRatingViewModel> GetRiskRating()
        {
            var rating = (from a in context.TBL_RISK_RATING
                          where a.DELETED == false
                          select new RiskRatingViewModel
                          {
                              riskRatingId = a.RISKRATINGID,
                              advicedRate = a.ADVICEDRATE,
                              maxRange = a.MAXRANGE,
                              minRange = a.MINRANGE,
                              rates = a.RATES,
                              ratesDescription = a.RATESDESCRIPTION,
                              productId = (short)a.RISKRATINGID
                          }).ToList();
            return rating;
        }

        public IEnumerable<RiskRatingViewModel> GetRiskRatingByCompanyId(int companyId)
        {
            var rating = (from a in context.TBL_RISK_RATING
                          where a.DELETED == false && a.COMPANYID == companyId
                          select new RiskRatingViewModel
                          {
                              riskRatingId = a.RISKRATINGID,
                              advicedRate = a.ADVICEDRATE,
                              companyId = (short)a.COMPANYID,
                              createdBy = a.CREATEDBY,
                              dateTimeCreated = a.DATETIMECREATED,
                              dateTimeUpdated = a.DATETIMEUPDATED,
                              lastUpdatedBy = a.LASTUPDATEDBY.Value,
                              maxRange = a.MAXRANGE,
                              minRange = a.MINRANGE,
                              rates = a.RATES,
                              ratesDescription = a.RATESDESCRIPTION,
                              productId = (short)a.RISKRATINGID
                          }).ToList();
            return rating;
        }

        public IEnumerable<RiskRatingViewModel> GetRiskRatingByProductId(int ratingId)
        {
            var rating = (from a in context.TBL_RISK_RATING
                          where a.DELETED == false && a.RISKRATINGID == ratingId
                          select new RiskRatingViewModel
                          {
                              riskRatingId = a.RISKRATINGID,
                              advicedRate = a.ADVICEDRATE,
                              companyId = a.COMPANYID,
                              createdBy = a.CREATEDBY,
                              dateTimeCreated = a.DATETIMECREATED,
                              dateTimeUpdated = a.DATETIMEUPDATED,
                              lastUpdatedBy = a.LASTUPDATEDBY.Value,
                              maxRange = a.MAXRANGE,
                              minRange = a.MINRANGE,
                              rates = a.RATES,
                              ratesDescription = a.RATESDESCRIPTION,
                              productId = (short)a.RISKRATINGID
                          }).ToList();
            return rating;
        }

        public async Task<bool> UpdateRiskRating(int ratingId, RiskRatingViewModel entity)
        {
            var rating = (from a in context.TBL_RISK_RATING where a.RISKRATINGID == ratingId select a).FirstOrDefault();
            rating.ADVICEDRATE = entity.advicedRate;
            rating.COMPANYID = (short)entity.companyId;
            rating.MAXRANGE = entity.maxRange;
            rating.MINRANGE = entity.minRange;
            rating.PRODUCTID = entity.productId;
            rating.RATES = entity.rates;
            rating.RATESDESCRIPTION = entity.ratesDescription;
            rating.DATETIMEUPDATED = DateTime.Now.Date;
            rating.LASTUPDATEDBY = entity.lastUpdatedBy;
            return await context.SaveChangesAsync() != 0;
        }
        #endregion RiskRating

        #region Risk Assessment Title
        public async Task<bool> AddRiskAssessmentTitle(RiskAssessmentTitleViewModels entity)
        {
            var title = new TBL_RISK_ASSESSMENT_TITLE
            {
                RISKTITLE = entity.riskTitle,
                DATETIMECREATED = genSetup.GetApplicationDate().Date,
                CREATEDBY = entity.createdBy,
                COMPANYID = entity.companyId,
                RISKTYPEID = entity.riskTypeId,
                PRODUCTID = entity.productId,
            };
            this.context.TBL_RISK_ASSESSMENT_TITLE.Add(title);

            // Audit Section ---------------------------
            var audit = new TBL_AUDIT
            {
                AUDITTYPEID = (short)AuditTypeEnum.RiskAssessmentTitleAdd,
                STAFFID = entity.createdBy,
                BRANCHID = (short)entity.userBranchId,
                DETAIL = $"Added Risk assessment title: { entity.riskTitle } ",
                IPADDRESS = CommonHelpers.GetLocalIpAddress(),
                URL = entity.applicationUrl,
                APPLICATIONDATE = genSetup.GetApplicationDate(),
                SYSTEMDATETIME = DateTime.Now,
                DEVICENAME = CommonHelpers.GetDeviceName(),
                OSNAME = CommonHelpers.FriendlyName(),
            };

            this.auditTrail.AddAuditTrail(audit);

            //end of Audit section -------------------------------
            return await context.SaveChangesAsync() != 0;
        }

        public async Task<bool> UpdateRiskAssessmentTitle(int riskAssessmentTitleId, RiskAssessmentTitleViewModels entity)
        {
            var title = context.TBL_RISK_ASSESSMENT_TITLE.FirstOrDefault(c => c.RISKASSESSMENTTITLEID == riskAssessmentTitleId);
            title.RISKTITLE = entity.riskTitle;
            title.DATETIMEUPDATED = genSetup.GetApplicationDate();
            title.RISKTYPEID = entity.riskTypeId;
            title.PRODUCTID = (short) entity.productId;
            title.LASTUPDATEDBY = entity.lastUpdatedBy;

            // Audit Section ---------------------------
            var audit = new TBL_AUDIT
            {
                AUDITTYPEID = (short)AuditTypeEnum.RiskAssessmentTitleUpdate,
                STAFFID = entity.createdBy,
                BRANCHID = (short)entity.userBranchId,
                DETAIL = $"Update Risk assessment title: { entity.riskTitle } ",
                IPADDRESS = CommonHelpers.GetLocalIpAddress(),
                URL = entity.applicationUrl,
                APPLICATIONDATE = genSetup.GetApplicationDate(),
                SYSTEMDATETIME = DateTime.Now,
                DEVICENAME = CommonHelpers.GetDeviceName(),
                OSNAME = CommonHelpers.FriendlyName(),
            };

            this.auditTrail.AddAuditTrail(audit);

            //end of Audit section -------------------------------
            return await context.SaveChangesAsync() != 0;
        }

        public async Task<bool> DeleteRiskAssessmentTitle(int riskAssessmentTitleId, UserInfo user)
        {
            var title = context.TBL_RISK_ASSESSMENT_TITLE.FirstOrDefault(c => c.RISKASSESSMENTTITLEID == riskAssessmentTitleId);
            title.DELETED = true;
            title.DATETIMEDELETED = genSetup.GetApplicationDate();
            title.DELETEDBY = user.staffId;


            // Audit Section ---------------------------
            var audit = new TBL_AUDIT
            {
                AUDITTYPEID = (short)AuditTypeEnum.RiskAssessmentTitleDelete,
                STAFFID = user.staffId,
                BRANCHID = (short)user.BranchId,
                DETAIL = $"Delete Risk assessment title: { title.RISKTITLE } ",
                IPADDRESS = CommonHelpers.GetLocalIpAddress(),
                URL = user.applicationUrl,
                APPLICATIONDATE = genSetup.GetApplicationDate(),
                SYSTEMDATETIME = DateTime.Now,
                DEVICENAME = CommonHelpers.GetDeviceName(),
                OSNAME = CommonHelpers.FriendlyName(),
            };

            this.auditTrail.AddAuditTrail(audit);

            //end of Audit section -------------------------------
            return await context.SaveChangesAsync() != 0;
        }

        public RiskAssessmentTitleViewModels GetRiskAssessmentTitleById(int riskAssessmentTitleId, int companyId)
        {
            return RiskAssessmentTitle(companyId).FirstOrDefault(c => c.riskAssessmentTitleId == riskAssessmentTitleId);
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

            return context.TBL_RISK_ASSESSMENT_TITLE.Where(c => c.COMPANYID == companyId).Select(c => new RiskAssessmentTitleViewModels()
            {
                riskAssessmentTitleId = c.RISKASSESSMENTTITLEID,
                companyId = c.COMPANYID,
                createdBy = c.CREATEDBY,
                riskTitle = c.RISKTITLE,
                productId = c.PRODUCTID,
                riskTypeId = c.RISKTYPEID
            });
        }

        #endregion Risk Assessment Title

    }
}