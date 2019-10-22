using FintrakBanking.Common.Enum;
using FintrakBanking.Entities.Models;
using FintrakBanking.Interfaces.Admin;
using FintrakBanking.Interfaces.Setups.General;
using FintrakBanking.ViewModels.Setups.General;
using System;
using System.Collections.Generic;
using FintrakBanking.Common.CustomException;
using System.ComponentModel.Composition;
using System.Linq;
using FintrakBanking.Common;

namespace FintrakBanking.Repositories.Setups.General
{

    public class JobTitleRepository : IJobTitleRepository
    {
        private FinTrakBankingContext context;
        private IAuditTrailRepository auditTrail;
        private IGeneralSetupRepository genSetup;
        public JobTitleRepository(FinTrakBankingContext _context, IAuditTrailRepository _auditTrail,
             IGeneralSetupRepository _genSetup)
        {
            this.context = _context;
            this.auditTrail = _auditTrail;
            this.genSetup = _genSetup;
        }

        public IEnumerable<JobTitleViewModel> JobTitle()
        {
            var jobtitle = from a in context.TBL_STAFF_JOBTITLE
                           select new JobTitleViewModel
                           {
                               jobTitle = a.JOBTITLENAME,
                               companyId = (short)a.COMPANYID,
                               jobTitleId = a.JOBTITLEID
                           };
            return jobtitle.ToList();
        }

        public JobTitleViewModel GetJobTitle(int jobTitleId)
        {
            var jobtitle = (from a in context.TBL_STAFF_JOBTITLE
                            where a.JOBTITLEID == jobTitleId
                            select new JobTitleViewModel
                            {
                                jobTitle = a.JOBTITLENAME,
                                companyId = (short)a.COMPANYID,
                                jobTitleId = a.JOBTITLEID
                            }).SingleOrDefault();
            return jobtitle;
        }

        public IEnumerable<JobTitleViewModel> GetJobTitleByCompanyId(int companyId)
        {
            var jobtitle = (from a in context.TBL_STAFF_JOBTITLE
                            where (short)a.COMPANYID == companyId
                            select new JobTitleViewModel
                            {
                                jobTitle = a.JOBTITLENAME,
                                companyId = (short)a.COMPANYID,
                                jobTitleId = a.JOBTITLEID
                            });
            return jobtitle;
        }
        public bool AddUpdateJobTitle(JobTitleViewModel entity)
        {
            if (entity != null)
            {
                try
                {

                    TBL_STAFF_JOBTITLE jobTitle;
                    if (entity.jobTitleId > 0)
                    {
                        jobTitle = context.TBL_STAFF_JOBTITLE.Find(entity.jobTitleId);
                        if (jobTitle != null)
                        {
                            jobTitle.JOBTITLENAME = entity.jobTitle;
                        }
                    }
                    else
                    {
                        jobTitle = new TBL_STAFF_JOBTITLE
                        {
                            JOBTITLENAME = entity.jobTitle,
                        COMPANYID = entity.companyId
                        };
                        context.TBL_STAFF_JOBTITLE.Add(jobTitle);
                    }
                    // Audit Section ----------------------------
                    var audit = new TBL_AUDIT
                    {
                        AUDITTYPEID = (short)AuditTypeEnum.CustomerUpdated,
                        STAFFID = entity.createdBy,
                        BRANCHID = (short)entity.userBranchId,
                        DETAIL = "Added/Modified Staff Role",
                        IPADDRESS = CommonHelpers.GetLocalIpAddress(),
                        URL = entity.applicationUrl,
                        APPLICATIONDATE = genSetup.GetApplicationDate(),
                        SYSTEMDATETIME = DateTime.Now,
                        DEVICENAME = CommonHelpers.GetDeviceName(),
                        OSNAME = CommonHelpers.FriendlyName(),
                    };

                    this.auditTrail.AddAuditTrail(audit);

                    var response = context.SaveChanges() != 0;
                    return response;
                }
                catch (Exception ex)
                {
                    throw new SecureException(ex.Message);
                }
            }

            return false;
        }
        public bool ValidateJobTitle(string jobTitleName)
        {
            return context.TBL_STAFF_JOBTITLE.Where(x => x.JOBTITLENAME == jobTitleName).Any();
        }
    }
}