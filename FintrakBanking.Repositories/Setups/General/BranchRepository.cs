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
            var branch = context.tbl_Branch.Find(id);

            if (branch != null)
            {
                return new BranchViewModel()
                {
                    branchId = branch.BranchId,
                    stateId = branch.StateId,
                    companyId = branch.CompanyId,
                    branchName = branch.BranchName,
                    branchCode = branch.BranchCode,
                    addressLine1 = branch.AddressLine1,
                    addressLine2 = branch.AddressLine2,
                    comment = branch.Comment,
                };
            }

            return new BranchViewModel();
        }

        public IEnumerable<BranchViewModel> GetAllBranch()
        {
            var branches = context.tbl_Branch.Where(x => x.Deleted == false).Select(x => new BranchViewModel
            {
                branchId = x.BranchId,
                stateId = x.StateId,
                cityId = (int)x.CityId,
                companyId = x.CompanyId,
                stateName = x.tbl_State.StateName,
                cityName = context.tbl_City.FirstOrDefault(c => c.CityId == x.CityId).CityName ?? string.Empty,
                branchName = x.BranchName,
                branchCode = x.BranchCode,
                addressLine1 = x.AddressLine1,
                addressLine2 = x.AddressLine2,
                comment = x.Comment,
                deleted = x.Deleted,
            }).ToList();

            return branches;
        }

        public IEnumerable<BranchViewModel> GetAllBranchByCompanyId(int id)
        {
            var branches = context.tbl_Branch.Where(x => x.CompanyId == id).Select(x => new BranchViewModel
            {
                branchId = x.BranchId,
                stateId = x.StateId,
                companyId = x.CompanyId,
                branchName = x.BranchName,
                stateName = x.tbl_State.StateName,
                cityId = (int)x.CityId,
                cityName = context.tbl_City.First(c=>c.CityId==x.CityId).CityName,
                branchCode = x.BranchCode,
                addressLine1 = x.AddressLine1,
                addressLine2 = x.AddressLine2,
                comment = x.Comment,
                dateTimeUpdated = x.DateTimeUpdated,
                deleted = x.Deleted,
            });

            return branches;
        }

        public async Task<bool> AddBranch(AddBranchViewModel model)
        {
            var branch = new tbl_Branch()
            {
                StateId = model.stateId,
                CityId = model.cityId,
                CompanyId = model.companyId,
                BranchName = model.branchName,
                BranchCode = model.branchCode,
                AddressLine1 = model.addressLine1,
                AddressLine2 = model.addressLine2,
                Comment = model.comment,
                CreatedBy = model.createdBy,
                Deleted = model.deleted,
            };

            this.context.tbl_Branch.Add(branch);

            var response = await context.SaveChangesAsync();
            // Audit Section ---------------------------
            var audit = new tbl_Audit
            {
                AuditTypeId = (short)AuditTypeEnum.BranchAdded,
                StaffId = (int)model.createdBy,
                BranchId = (short)model.userBranchId,
                Detail = $"Added branch: '{model.branchName}' with code: {model.branchCode} ",
                IPAddress = model.userIPAddress,
                Url = model.applicationUrl,
                ApplicationDate = genSetup.GetApplicationDate(),
                SystemDateTime = DateTime.Now
            };
            //end of Audit section -------------------------------
            return response != 0;
        }

        public async Task<bool> UpdateBranch(BranchViewModel model, short id)
        {
            var response = 0;
            var branch = context.tbl_Branch.Find(id);

            if (branch != null)
            {
                //branch.BranchId = model.branchId;
                branch.StateId = model.stateId;
                branch.CityId = model.cityId;
                branch.CompanyId = model.companyId;
                branch.BranchName = model.branchName;
                branch.BranchCode = model.branchCode;
                branch.AddressLine1 = model.addressLine1;
                branch.AddressLine2 = model.addressLine2;
                branch.Comment = model.comment;
                branch.LastUpdatedBy = model.lastUpdatedBy;
                branch.DateTimeUpdated = model.dateTimeUpdated;
                branch.Deleted = model.deleted;

                response = await context.SaveChangesAsync();
                // Audit Section ---------------------------
                var audit = new tbl_Audit
                {
                    AuditTypeId = (short)AuditTypeEnum.BranchAdded,
                    StaffId = (int)model.createdBy,
                    BranchId = (short)model.userBranchId,
                    Detail = $"Updated branch: '{model.branchName}' with code: {model.branchCode} ",
                    IPAddress = model.userIPAddress,
                    Url = model.applicationUrl,
                    ApplicationDate = genSetup.GetApplicationDate(),
                    SystemDateTime = DateTime.Now
                };
                //end of Audit section -------------------------------
            }

            return response != 0;
        }

        public async Task<bool> DeleteBranch(short id, UserInfo user)
        {
            var response = 0;
            var branch = context.tbl_Branch.Find(id);

            if (branch != null)
            {
                branch.Deleted = true;
                response = await context.SaveChangesAsync();
                // Audit Section ---------------------------
                var audit = new tbl_Audit
                {
                    AuditTypeId = (short)AuditTypeEnum.BranchDeleted,
                    StaffId = (int)user.staffId,
                    BranchId = (short)user.BranchId,
                    Detail = $"Deleted branch: '{branch.BranchName}' with code: {branch.BranchCode} ",
                    IPAddress = user.userIPAddress,
                    Url = user.applicationUrl,
                    ApplicationDate = genSetup.GetApplicationDate(),
                    SystemDateTime = DateTime.Now
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