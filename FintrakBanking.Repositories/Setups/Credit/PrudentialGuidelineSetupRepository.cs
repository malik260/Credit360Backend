using FintrakBanking.Interfaces.Setups;
using FintrakBanking.Interfaces.Setups.Credit;
using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FintrakBanking.ViewModels;
using FintrakBanking.Entities.Models;
using FintrakBanking.ViewModels.Setups.Credit;
using FintrakBanking.Interfaces.Setups.General;
using FintrakBanking.Interfaces.Admin;
using FintrakBanking.Common.Enum;
using FintrakBanking.Common;

namespace FintrakBanking.Repositories.Setups.Credit
{
    [Export(typeof(IChecklistRepository))]
    [PartCreationPolicy(CreationPolicy.NonShared)]
  public  class PrudentialGuidelineSetupRepository : IPrudentialGuidelineSetupRepository
    {

        private FinTrakBankingContext context;
        private IGeneralSetupRepository genSetup;
        private IAuditTrailRepository auditTrail;


        public PrudentialGuidelineSetupRepository(FinTrakBankingContext _context,
                                        IGeneralSetupRepository genSetup, IGeneralSetupRepository _genSetup,
                                        IAuditTrailRepository _auditTrail)
        {
            this.context = _context;
            this.genSetup = _genSetup;
            auditTrail = _auditTrail;
        }


        public string AddGuideline(PrudentialGuidelineViewModel guideline)
        {
            int confirmation = 0;
           // string prudentialGuidelineName = context.TBL_LOAN_PRUDENT_GUIDE_TYPE.Where(w => w.PRUDENTIALGUIDELINETYPEID == guideline.prudentialGuidelineTypeId).FirstOrDefault().PRUDENTIALGUIDELINETYPENAME;
            if (guideline!=null)
            {
                short a = (short)guideline.prudentialGuidelineTypeId;
                int ass = guideline.prudentialGuidelineTypeId;

                var guidelineList = new TBL_LOAN_PRUDENTIALGUIDELINE()
                {
                    STATUSNAME = guideline.classification,
                    PRUDENTIALGUIDELINETYPEID = (short)guideline.prudentialGuidelineTypeId,
                    INTERNALMINIMUM=guideline.internalMinimun,
                    INTERNALMAXIMUM=guideline.internalMaximun,
                    EXTERNALMINIMUM=guideline.externalMinimun,
                    EXTERNALMAXIMUM=guideline.externalMaximun,
                    NARRATION=guideline.naration,
                    

                };

                context.TBL_LOAN_PRUDENTIALGUIDELINE.Add(guidelineList);

                confirmation = context.SaveChanges();

                var audit = new TBL_AUDIT
                {
                    AUDITTYPEID = (short)AuditTypeEnum.LoanPrincipalInserted,
                    STAFFID = guideline.staffId,
                    BRANCHID = (short)guideline.userBranchId,
                    DETAIL = $"Loan Loan prudential guideline with {guideline.classification} classification is added",
                    IPADDRESS = CommonHelpers.GetLocalIpAddress(),
                    URL = guideline.applicationUrl,
                    APPLICATIONDATE = genSetup.GetApplicationDate(),
                    SYSTEMDATETIME = DateTime.Now,
                    DEVICENAME = CommonHelpers.GetDeviceName(),
                    OSNAME = CommonHelpers.FriendlyName()
                };
                this.auditTrail.AddAuditTrail(audit);

                return "The record has been added successful";
            }
            return "The record has not been added";
            
        }

        public string DeleteGuideline(int prudentialGuidelineId)
        {
            if (prudentialGuidelineId!=null)
            {
                var guideline = context.TBL_LOAN_PRUDENTIALGUIDELINE.Where(o => o.PRUDENTIALGUIDELINESTATUSID == prudentialGuidelineId).FirstOrDefault();
                if (guideline!=null)
                {
                    context.TBL_LOAN_PRUDENTIALGUIDELINE.Remove(guideline);
                    context.SaveChanges();

                    return "The record has been updated successful";
                }
                return "The record is not delited";
            }
            return "Record not found";
        }

        public IEnumerable<PrudentialGuidelineViewModel> GetAllGuidelines(int companyId)
        {
            var guideline = (from a in context.TBL_LOAN_PRUDENTIALGUIDELINE
                            select new PrudentialGuidelineViewModel
                            {
                               prudentialGuidelineId = a.PRUDENTIALGUIDELINESTATUSID,
                               statusName= a.STATUSNAME,
                               classification= a.TBL_LOAN_PRUDENT_GUIDE_TYPE.PRUDENTIALGUIDELINETYPENAME,
                               internalMinimun= (int)a.INTERNALMINIMUM,
                               internalMaximun= (int)a.INTERNALMAXIMUM,
                               externalMinimun= (int)a.EXTERNALMINIMUM,
                               externalMaximun= (int)a.EXTERNALMAXIMUM,
                               naration= a.NARRATION,
                               prudentialGuidelineTypeId = a.PRUDENTIALGUIDELINETYPEID
                            }).ToList();
            return guideline;
        }

        public IEnumerable<PrudentialGuidelineViewModel> GetAllGuidelineTypes(int getCompanyId)
        {
            var guideline = (from a in context.TBL_LOAN_PRUDENT_GUIDE_TYPE
                             select new PrudentialGuidelineViewModel
                             {
                                 prudentialGuidelineTypeId = a.PRUDENTIALGUIDELINETYPEID,
                                 prudentialGuidelineTypeName = a.PRUDENTIALGUIDELINETYPENAME
                             }).ToList();
            return guideline;
        }

        public PrudentialGuidelineViewModel getGuideline(int prudentialGuidelineId)
        {
            var guideline = (from a in context.TBL_LOAN_PRUDENTIALGUIDELINE
                             where a.PRUDENTIALGUIDELINESTATUSID==prudentialGuidelineId
                             select new PrudentialGuidelineViewModel
                             {
                                 statusName = a.STATUSNAME,
                                classification = a.TBL_LOAN_PRUDENT_GUIDE_TYPE.PRUDENTIALGUIDELINETYPENAME,
                                 internalMinimun = (int)a.INTERNALMINIMUM,
                                 internalMaximun = (int)a.INTERNALMAXIMUM,
                                 externalMinimun = (int)a.EXTERNALMINIMUM,
                                 externalMaximun = (int)a.EXTERNALMAXIMUM,
                                 naration = a.NARRATION,
                                 prudentialGuidelineTypeId = a.PRUDENTIALGUIDELINETYPEID
                             }).FirstOrDefault();
            return guideline;
        }

        public string UpdateGuideline(PrudentialGuidelineViewModel guideline, int prudentialGuidelineId)
        {
            var data = context.TBL_LOAN_PRUDENTIALGUIDELINE.Where(o => o.PRUDENTIALGUIDELINESTATUSID == prudentialGuidelineId).FirstOrDefault();
            string prudentialGuidelineName = context.TBL_LOAN_PRUDENT_GUIDE_TYPE.Where(w => w.PRUDENTIALGUIDELINETYPEID == guideline.prudentialGuidelineTypeId).FirstOrDefault().PRUDENTIALGUIDELINETYPENAME;

            if (guideline != null)
            {
                data.STATUSNAME = prudentialGuidelineName;
               // data.CLASSIFICATION= guideline.classification;
                data.INTERNALMINIMUM=guideline.internalMinimun;
                data.INTERNALMAXIMUM=guideline.internalMaximun;
                data.EXTERNALMINIMUM=guideline.externalMinimun;
                data.EXTERNALMAXIMUM=guideline.externalMaximun;
                data.NARRATION=guideline.naration;

              context.SaveChanges();

                var audit = new TBL_AUDIT
                {
                    AUDITTYPEID = (short)AuditTypeEnum.LoanPrincipalInserted,
                    STAFFID = guideline.staffId,
                    BRANCHID = (short)guideline.userBranchId,
                    DETAIL = $"Loan Loan prudential guideline with {guideline.classification} classification is added",
                    IPADDRESS = guideline.userIPAddress,
                    URL = guideline.applicationUrl,
                    APPLICATIONDATE = genSetup.GetApplicationDate(),
                    SYSTEMDATETIME = DateTime.Now
                };
                this.auditTrail.AddAuditTrail(audit);

                return "The record has been updated successful";
            }
            return "The record has not been updated";
        }
    }
}
