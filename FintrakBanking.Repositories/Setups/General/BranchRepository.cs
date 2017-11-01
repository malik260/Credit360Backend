using FintrakBanking.Common.Enum;
using FintrakBanking.Entities.Models;
using FintrakBanking.Interfaces.Admin;
using FintrakBanking.Interfaces.Setups.General;
using FintrakBanking.ViewModels;
using FintrakBanking.ViewModels.Setups.General;
using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using System.Threading.Tasks;

namespace FintrakBanking.Repositories.Setups.General
{
    [Export(typeof(IBranchRepository))]
    [PartCreationPolicy(CreationPolicy.NonShared)]
    public class BranchRepository : IBranchRepository
    {
        private FinTrakBankingContext context;
        private IAuditTrailRepository auditTrail;
        IGeneralSetupRepository genSetup;

        public BranchRepository(FinTrakBankingContext _context,
                                IAuditTrailRepository _auditTrail,
                                IGeneralSetupRepository _genSetup)
        {
            this.context = _context;
            this.auditTrail = _auditTrail;
            this.genSetup = _genSetup;
        }

        //private bool SaveAll()
        //{
        //    return this.context.SaveChanges() > 0;
        //}

        #region tbl_Branch Setup

        public BranchViewModel GetBranch(short id)
        {
            var branch = context.TBL_BRANCH.Find(id);
            
            if (branch != null)
            {
                return new BranchViewModel()
                {
                    branchId = branch.BRANCHID,
                    stateId = branch.STATEID,
                    companyId = branch.COMPANYID,
                    branchName = branch.BRANCHNAME,
                    branchCode = branch.BRANCHCODE,
                    addressLine1 = branch.ADDRESSLINE1,
                    addressLine2 = branch.ADDRESSLINE2,
                    comment = branch.COMMENT,
                };
            }

            return new BranchViewModel();
        }

        public IEnumerable<BranchViewModel> GetAllBranch()
        {
            var branches = context.TBL_BRANCH.Where(x => x.DELETED == false).Select(x => new BranchViewModel
            {
                branchId = x.BRANCHID,
                stateId = x.STATEID,
                cityId = (int)x.CITYID,
                companyId = x.COMPANYID,
                stateName = x.TBL_STATE.STATENAME,
                cityName = context.TBL_CITY.FirstOrDefault(c => c.CITYID == x.CITYID).CITYNAME ?? string.Empty,
                branchName = x.BRANCHNAME,
                branchCode = x.BRANCHCODE,
                addressLine1 = x.ADDRESSLINE1,
                addressLine2 = x.ADDRESSLINE2,
                comment = x.COMMENT,
                deleted = x.DELETED,
            }).ToList();

            return branches;
        }

        public IEnumerable<BranchViewModel> GetAllBranchByCompanyId(int id)
        {
            var branches = context.TBL_BRANCH.Where(x => x.COMPANYID == id).Select(x => new BranchViewModel
            {
                branchId = x.BRANCHID,
                stateId = x.STATEID,
                companyId = x.COMPANYID,
                branchName = x.BRANCHNAME,
                stateName = x.TBL_STATE.STATENAME,
                cityId = (int)x.CITYID,
                cityName = context.TBL_CITY.FirstOrDefault(c=>c.CITYID==x.CITYID).CITYNAME,
                branchCode = x.BRANCHCODE,
                addressLine1 = x.ADDRESSLINE1,
                addressLine2 = x.ADDRESSLINE2,
                comment = x.COMMENT,
                dateTimeUpdated = x.DATETIMEUPDATED,
                deleted = x.DELETED,
            });

            return branches;
        }

        public async Task<bool> AddBranch(AddBranchViewModel model)
        {
            var branch = new TBL_BRANCH()
            {
                STATEID = model.stateId,
                CITYID = model.cityId,
                COMPANYID = model.companyId,
                BRANCHNAME = model.branchName,
                BRANCHCODE = model.branchCode,
                ADDRESSLINE1 = model.addressLine1,
                ADDRESSLINE2 = model.addressLine2,
                COMMENT = model.comment,
                CREATEDBY = model.createdBy,
                DELETED = model.deleted,
            };

            this.context.TBL_BRANCH.Add(branch);

            var response = await context.SaveChangesAsync();
            // Audit Section ---------------------------
            var audit = new TBL_AUDIT
            {
                AUDITTYPEID = (short)AuditTypeEnum.BranchAdded,
                STAFFID = (int)model.createdBy,
                BRANCHID = (short)model.userBranchId,
                DETAIL = $"Added branch: '{model.branchName}' with code: {model.branchCode} ",
                IPADDRESS = model.userIPAddress,
                URL = model.applicationUrl,
                APPLICATIONDATE = genSetup.GetApplicationDate(),
                SYSTEMDATETIME = DateTime.Now
            };
            //end of Audit section -------------------------------
            return response != 0;
        }

        public async Task<bool> UpdateBranch(BranchViewModel model, short id)
        {
            var response = 0;
            var branch = context.TBL_BRANCH.Find(id);

            if (branch != null)
            {
                //branch.BranchId = model.branchId;
                branch.STATEID = model.stateId;
                branch.CITYID = model.cityId;
                branch.COMPANYID = model.companyId;
                branch.BRANCHNAME = model.branchName;
                branch.BRANCHCODE = model.branchCode;
                branch.ADDRESSLINE1 = model.addressLine1;
                branch.ADDRESSLINE2 = model.addressLine2;
                branch.COMMENT = model.comment;
                branch.LASTUPDATEDBY = model.lastUpdatedBy;
                branch.DATETIMEUPDATED = DateTime.Now;

                response = await context.SaveChangesAsync();

                // Audit Section ---------------------------
                var audit = new TBL_AUDIT
                {
                    AUDITTYPEID = (short)AuditTypeEnum.BranchUpdated,
                    STAFFID = (int)model.lastUpdatedBy,
                    BRANCHID = (short)model.userBranchId,
                    DETAIL = $"Updated branch: '{model.branchName}' with code: {model.branchCode} ",
                    IPADDRESS = model.userIPAddress,
                    URL = model.applicationUrl,
                    APPLICATIONDATE = genSetup.GetApplicationDate(),
                    SYSTEMDATETIME = DateTime.Now
                };
                //end of Audit section -------------------------------
            }

            return response != 0;
        }

        public async Task<bool> DeleteBranch(short id, UserInfo user)
        {
            var response = 0;
            var branch = context.TBL_BRANCH.Find(id);

            if (branch != null)
            {
                branch.DELETED = true;
                response = await context.SaveChangesAsync();
                // Audit Section ---------------------------
                var audit = new TBL_AUDIT
                {
                    AUDITTYPEID = (short)AuditTypeEnum.BranchDeleted,
                    STAFFID = (int)user.staffId,
                    BRANCHID = (short)user.BranchId,
                    DETAIL = $"Deleted branch: '{branch.BRANCHNAME}' with code: {branch.BRANCHCODE} ",
                    IPADDRESS = user.userIPAddress,
                    URL = user.applicationUrl,
                    APPLICATIONDATE = genSetup.GetApplicationDate(),
                    SYSTEMDATETIME = DateTime.Now
                };
                //end of Audit section -------------------------------
            }

            return response != 0;
        }

        #endregion tbl_Branch Setup

        //#region tbl_Branch Limit Setup

        //public BranchLimitViewModel GetBranchLimit(short id)
        //{
        //    var model = context.TblBranchLimit.Find(id);

        //    if (model != null)
        //    {
        //        return new BranchLimitViewModel()
        //        {
        //            branchLimitId = model.BranchLimitId,
        //            branchId = model.BranchId,
        //            limitType = model.LimitType,
        //            limitAmount = model.LimitAmount,
        //            limitPercentage = model.LimitPercentage,
        //            limitCount = model.LimitCount,
        //        };
        //    }

        //    return new BranchLimitViewModel();
        //}

        //public IEnumerable<BranchLimitViewModel> GetAllBranchLimit()
        //{
        //    var branchelimits = context.TblBranchLimit.Select(model => new BranchLimitViewModel
        //    {
        //        branchLimitId = model.BranchLimitId,
        //        branchId = model.BranchId,
        //        limitType = model.LimitType,
        //        limitAmount = model.LimitAmount,
        //        limitPercentage = model.LimitPercentage,
        //        limitCount = model.LimitCount,
        //    });

        //    return branchelimits;
        //}

        //public IEnumerable<BranchLimitViewModel> GetAllBranchLimitByBranchId(int id)
        //{
        //    var branchelimits = context.TblBranchLimit.Where(x => x.BranchId == id).Select(model => new BranchLimitViewModel
        //    {
        //        branchLimitId = model.BranchLimitId,
        //        branchId = model.BranchId,
        //        limitType = model.LimitType,
        //        limitAmount = model.LimitAmount,
        //        limitPercentage = model.LimitPercentage,
        //        limitCount = model.LimitCount,
        //    });

        //    return branchelimits;
        //}

        //public async Task<bool> AddBranchLimit(BranchLimitViewModel model)
        //{
        //    var branch = new TblBranchLimit()
        //    {
        //        //BranchLimitId = model.branchLimitId,
        //        BranchId = model.branchId,
        //        LimitType = model.limitType,
        //        LimitAmount = model.limitAmount,
        //        LimitPercentage = model.limitPercentage,
        //        LimitCount = model.limitCount,
        //    };

        //    this.context.TblBranchLimit.Add(branch);

        //    var response = await context.SaveChangesAsync();
        //    return response != 0;
        //}

        //public async Task<bool> UpdateBranchLimit(BranchLimitViewModel model)
        //{
        //    var response = 0;
        //    var branchlimit = context.TblBranchLimit.Find(model.branchId);

        //    if (branchlimit != null)
        //    {
        //        //branchlimit.BranchLimitId = model.branchLimitId;
        //        branchlimit.BranchId = model.branchId;
        //        branchlimit.LimitType = model.limitType;
        //        branchlimit.LimitAmount = model.limitAmount;
        //        branchlimit.LimitPercentage = model.limitPercentage;
        //        branchlimit.LimitCount = model.limitCount;

        //        response = await context.SaveChangesAsync();
        //    }

        //    return response != 0;
        //}

        //public async Task<bool> DeleteBranchLimit(short id)
        //{
        //    var response = 0;
        //    var branchlimit = context.TblBranchLimit.Find(id);

        //    if (branchlimit != null)
        //    {
        //        //branch.Deleted = true; // TODO: not sure if we want this exposed
        //        response = await context.SaveChangesAsync();
        //    }

        //    return response != 0;
        //}

        //#endregion tbl_Branch Limit Setup
    }
}