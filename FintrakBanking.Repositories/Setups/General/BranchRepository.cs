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
using FintrakBanking.Common.CustomException;
using FintrakBanking.Common;

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
        #region Region Setup
        public IEnumerable<BranchRegionViewModel> GetAllRegion()
        {
            var regions = context.TBL_BRANCH_REGION.Where(x => x.DELETED == false).Select(x => new BranchRegionViewModel
            {
                regionId = x.REGIONID,
                regionName = x.REGION_NAME,
                companyId = x.COMPANYID,
                companyName = x.TBL_COMPANY.NAME,
                houStaffId = x.CAM_HOU_STAFFID,
                regionTypeId = x.REGIONTYPEID,
                regionTypeName = context.TBL_BRANCH_REGION_STAFF_TYPE.FirstOrDefault(a=>a.REGIONSTAFFTYPEID == x.REGIONTYPEID).REGIONSTAFFTYPENAME,
                houStaffName = x.TBL_STAFF.FIRSTNAME + " " + x.TBL_STAFF.LASTNAME
            }).ToList();

            return regions;
        }

        public bool AddUpdateBranchRegion(BranchRegionViewModel entity)
        {
            if (entity != null)
            {
                try
                {
                    TBL_BRANCH_REGION region;
                    if (entity.regionId != 0 || entity.regionId < 0)
                    {
                        region = context.TBL_BRANCH_REGION.Find(entity.regionId);
                        if (region != null)
                        {
                            region.REGION_NAME = entity.regionName;
                            region.CAM_HOU_STAFFID = entity.houStaffId;
                            region.LASTUPDATEDBY = entity.createdBy;
                            region.REGIONTYPEID = entity.regionTypeId;
                            region.DATETIMEUPDATED = DateTime.Now;
                        }
                    }
                    else
                    {
                        region = new TBL_BRANCH_REGION();

                        region.REGION_NAME = entity.regionName;
                        region.CAM_HOU_STAFFID = entity.houStaffId;
                        region.REGIONTYPEID = entity.regionTypeId;
                        region.COMPANYID = entity.companyId;
                        region.DELETED = false;
                        region.CREATEDBY = entity.createdBy;
                        region.DATETIMECREATED = DateTime.Now;

                        context.TBL_BRANCH_REGION.Add(region);
                    }

                    // Audit Section ---------------------------
                    var audit = new TBL_AUDIT
                    {
                        AUDITTYPEID = (short)AuditTypeEnum.BranchAdded,
                        STAFFID = entity.createdBy,
                        BRANCHID = (short)entity.userBranchId,
                        DETAIL = "Added Branch Region with Name : " + entity.regionName,
                        IPADDRESS = CommonHelpers.GetLocalIpAddress(),
                        URL = entity.applicationUrl,
                        APPLICATIONDATE = DateTime.Now,
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

        public IEnumerable<BranchRegionStaffViewModel> GetAllRegionStaff(int regionId)
        {
            var regions = context.TBL_BRANCH_REGION_STAFF.Where(x => x.DELETED == false && x.REGIONID == regionId).Select(x => new BranchRegionStaffViewModel
            {
                staffRegionId= x.STAFFREGIONID,
                regionId = x.REGIONID,
                houStaffId = x.STAFFID,
                regionStaffTypeId = x.REGIONSTAFFTYPEID,
                regionStaffTypeName = context.TBL_BRANCH_REGION_STAFF_TYPE.Where(a => a.REGIONSTAFFTYPEID == x.REGIONSTAFFTYPEID).Select(b => b.REGIONSTAFFTYPENAME).FirstOrDefault(),
                houStaffName = context.TBL_STAFF.Where(a=>a.STAFFID == x.STAFFID).Select(b=>b.FIRSTNAME + " " + b.LASTNAME + "-" +"("+ b.STAFFCODE+")").FirstOrDefault(),
            }).ToList();

            return regions;
        }

        public bool AddUpdateBranchRegionStaff(BranchRegionStaffViewModel entity)
        {
            if (entity != null)
            {
                try
                {
                    TBL_BRANCH_REGION_STAFF regionStaff;
                    if (entity.staffRegionId != 0 || entity.staffRegionId < 0)
                    {
                        regionStaff = context.TBL_BRANCH_REGION_STAFF.Find(entity.staffRegionId);
                        if (regionStaff != null)
                        {
                            regionStaff.STAFFID = entity.houStaffId;
                            regionStaff.REGIONSTAFFTYPEID = entity.regionStaffTypeId;
                            regionStaff.LASTUPDATEDBY = entity.createdBy;
                            regionStaff.DATETIMEUPDATED = DateTime.Now;
                        }
                    }
                    else
                    {
                        var exist = context.TBL_BRANCH_REGION_STAFF.Where(x=>x.STAFFID == entity.houStaffId && x.REGIONID == entity.regionId).FirstOrDefault();
                        if (exist!=null)
                        {
                            regionStaff = exist;
                            regionStaff.DELETED = false;
                            regionStaff.LASTUPDATEDBY = entity.createdBy;
                            regionStaff.DATETIMEUPDATED = DateTime.Now;

                        }
                        else
                        {
                            regionStaff = new TBL_BRANCH_REGION_STAFF();
                            regionStaff.REGIONID = entity.regionId;
                            regionStaff.STAFFID = entity.houStaffId;
                            regionStaff.REGIONSTAFFTYPEID = entity.regionStaffTypeId;
                            regionStaff.DELETED = false;
                            regionStaff.CREATEDBY = entity.createdBy;
                            regionStaff.DATETIMECREATED = DateTime.Now;

                            context.TBL_BRANCH_REGION_STAFF.Add(regionStaff);
                        }

                    }

                    // Audit Section ---------------------------
                    var region = context.TBL_BRANCH_REGION.Find(entity.regionId);
                    var audit = new TBL_AUDIT
                    {
                        AUDITTYPEID = (short)AuditTypeEnum.BranchAdded,
                        STAFFID = entity.createdBy,
                        BRANCHID = (short)entity.userBranchId,
                        DETAIL = "Added Branch Region Staff For with Region : " + region.REGION_NAME,
                        IPADDRESS = CommonHelpers.GetLocalIpAddress(),
                        URL = entity.applicationUrl,
                        APPLICATIONDATE = DateTime.Now,
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
        public IEnumerable<LookupViewModel> GetAllRegionStaffType()
        {
            var regions = context.TBL_BRANCH_REGION_STAFF_TYPE.Select(x => new LookupViewModel
            {
                lookupId = (short)x.REGIONSTAFFTYPEID,
                lookupName = x.REGIONSTAFFTYPENAME,
            }).ToList();

            return regions;
        }

        public async Task<bool> DeleteBranchRegionStaff(short id, UserInfo user)
        {
            var response = 0;
            var regionstaff = context.TBL_BRANCH_REGION_STAFF.Find(id);
            var staff = context.TBL_STAFF.Find(regionstaff.STAFFID);
            var region = context.TBL_BRANCH_REGION.Find(regionstaff.REGIONID);

            if (regionstaff != null)
            {
                regionstaff.DELETED = true;
                regionstaff.DELETEDBY = user.staffId;
                regionstaff.DATETIMEDELETED = DateTime.Now;

                response = await context.SaveChangesAsync();
                // Audit Section ---------------------------
                var audit = new TBL_AUDIT
                {
                    AUDITTYPEID = (short)AuditTypeEnum.RegionDeleted,
                    STAFFID = (int)user.staffId,
                    BRANCHID = (short)user.BranchId,
                    DETAIL = $"Deleted region staff: '{staff.FIRSTNAME}' '{staff.LASTNAME}' in region: {region.REGION_NAME} ",
                    IPADDRESS = CommonHelpers.GetLocalIpAddress(),
                    URL = user.applicationUrl,
                    APPLICATIONDATE = genSetup.GetApplicationDate(),
                    SYSTEMDATETIME = DateTime.Now,
                    DEVICENAME = CommonHelpers.GetDeviceName(),
                    OSNAME = CommonHelpers.FriendlyName(),
                };
                //end of Audit section -------------------------------
            }

            return response != 0;
        }




        public bool ValidateRegionName(string regionName)
        {
            return context.TBL_BRANCH_REGION.Where(x => x.REGION_NAME == regionName).Any();
        }
        #endregion
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
                    regionId = branch.REGIONID,
                    regionName = branch.TBL_BRANCH_REGION.REGION_NAME,
                    branchCode = branch.BRANCHCODE,
                    addressLine1 = branch.ADDRESSLINE1,
                    addressLine2 = branch.ADDRESSLINE2,
                    comment = branch.COMMENT_,
                };
            }

            return new BranchViewModel();
        }

        public IEnumerable<BranchViewModel> GetAllBranch()
        {
            var branches = from x in context.TBL_BRANCH
                           join r in context.TBL_BRANCH_REGION on x.REGIONID equals r.REGIONID
                           where x.DELETED == false
                           select new BranchViewModel
                           {
                               branchId = x.BRANCHID,
                               stateId = x.STATEID,
                               cityId = x.CITYID,
                               companyId = x.COMPANYID,
                               stateName = x.TBL_STATE.STATENAME,
                               cityName = context.TBL_CITY.FirstOrDefault(c => c.CITYID == x.CITYID).CITYNAME ?? "",
                               branchName = x.BRANCHNAME,
                               branchCode = x.BRANCHCODE,
                               addressLine1 = x.ADDRESSLINE1,
                               addressLine2 = x.ADDRESSLINE2,
                               comment = x.COMMENT_,
                               branchLimit = x.NPL_LIMIT,
                               deleted = x.DELETED,
                               regionId = x.REGIONID,
                               regionName = r.REGION_NAME
                               
                           };

            return branches.ToList();
        }

        public IEnumerable<BranchViewModel> GetSearchedBranch(string search)
        {

            var branches = from x in context.TBL_BRANCH
                           where x.DELETED == false
                           && x.BRANCHCODE.Contains(search.ToUpper())
                           || x.BRANCHNAME.Contains(search.ToUpper())
                           select new BranchViewModel
                           {
                               branchId = x.BRANCHID,
                               stateName = x.TBL_STATE.STATENAME,
                               branchName = x.BRANCHNAME,
                               branchCode = x.BRANCHCODE,
                               addressLine1 = x.ADDRESSLINE1,
                               addressLine2 = x.ADDRESSLINE2,
                           };

            return branches.ToList();
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
                regionId = x.REGIONID,
                regionName = x.TBL_BRANCH_REGION.REGION_NAME,
                cityId = (int)x.CITYID,
                cityName = context.TBL_CITY.FirstOrDefault(c => c.CITYID == x.CITYID).CITYNAME,
                branchCode = x.BRANCHCODE,
                addressLine1 = x.ADDRESSLINE1,
                addressLine2 = x.ADDRESSLINE2,
                comment = x.COMMENT_,
                branchLimit = x.NPL_LIMIT,
                dateTimeUpdated = x.DATETIMEUPDATED,
                deleted = x.DELETED,
            });

            return branches;
        }

        public async Task<bool> AddBranch(AddBranchViewModel model)
        {
            var response = 0;

            try
            {
                var branch = new TBL_BRANCH()
                {
                    STATEID = model.stateId,
                    CITYID = model.cityId,
                    REGIONID = model.regionId,
                    COMPANYID = model.companyId,
                    BRANCHNAME = model.branchName,
                    BRANCHCODE = model.branchCode,
                    ADDRESSLINE1 = model.addressLine1,
                    ADDRESSLINE2 = model.addressLine2,
                    COMMENT_ = model.comment,
                    CREATEDBY = model.createdBy,
                    DELETED = model.deleted,
                };

                this.context.TBL_BRANCH.Add(branch);

                response = await context.SaveChangesAsync();


                // Audit Section ---------------------------
                var audit = new TBL_AUDIT
                {
                    AUDITTYPEID = (short)AuditTypeEnum.BranchAdded,
                    STAFFID = (int)model.createdBy,
                    BRANCHID = (short)model.userBranchId,
                    DETAIL = $"Added branch: '{model.branchName}' with code: {model.branchCode} ",
                    IPADDRESS = CommonHelpers.GetLocalIpAddress(),
                    URL = model.applicationUrl,
                    APPLICATIONDATE = genSetup.GetApplicationDate(),
                    SYSTEMDATETIME = DateTime.Now,
                    DEVICENAME = CommonHelpers.GetDeviceName(),
                    OSNAME = CommonHelpers.FriendlyName(),
                };
                //end of Audit section -------------------------------
            }
            catch (Exception ex) { }
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
                branch.REGIONID = model.regionId;
                branch.BRANCHNAME = model.branchName;
                branch.BRANCHCODE = model.branchCode;
                branch.ADDRESSLINE1 = model.addressLine1;
                branch.ADDRESSLINE2 = model.addressLine2;
                branch.COMMENT_ = model.comment;
                branch.NPL_LIMIT = model.branchLimit;
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
                    IPADDRESS = CommonHelpers.GetLocalIpAddress(),
                    URL = model.applicationUrl,
                    APPLICATIONDATE = genSetup.GetApplicationDate(),
                    SYSTEMDATETIME = DateTime.Now,
                    DEVICENAME = CommonHelpers.GetDeviceName(),
                    OSNAME = CommonHelpers.FriendlyName(),
                };
                //end of Audit section -------------------------------
            }

            return response != 0;
        }


        public bool UpdateBranches(BranchViewModel model, short id)
        {
            var response = 0;
            var branch = context.TBL_BRANCH.Find(id);

            if (branch != null)
            {
                branch.BRANCHID = id;
                branch.BRANCHNAME = model.branchName;
                branch.BRANCHCODE = model.branchCode;
                branch.NPL_LIMIT = model.branchLimit;
            }
                // Audit Section ---------------------------
                var audit = new TBL_AUDIT
                {
                    AUDITTYPEID = (short)AuditTypeEnum.BranchUpdated,
                    STAFFID = (int)model.createdBy,
                    BRANCHID = (short)model.userBranchId,
                    DETAIL = $"Updated branch: '{model.branchName}' with code: {model.branchCode} ",
                    IPADDRESS = CommonHelpers.GetLocalIpAddress(),
                    URL = model.applicationUrl,
                    APPLICATIONDATE = genSetup.GetApplicationDate(),
                    SYSTEMDATETIME = DateTime.Now,
                    DEVICENAME = CommonHelpers.GetDeviceName(),
                    OSNAME = CommonHelpers.FriendlyName(),
                };
            //end of Audit section -------------------------------
            this.auditTrail.AddAuditTrail(audit);

            return context.SaveChanges() > 0;
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
                    IPADDRESS = CommonHelpers.GetLocalIpAddress(),
                    URL = user.applicationUrl,
                    APPLICATIONDATE = genSetup.GetApplicationDate(),
                    SYSTEMDATETIME = DateTime.Now,
                    DEVICENAME = CommonHelpers.GetDeviceName(),
                    OSNAME = CommonHelpers.FriendlyName(),
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


        public IEnumerable<CollectionsRetailCronSetupViewModel> GetCollectionRetailCronJobSetup()
        {
            var retailCollection = from x in context.TBL_COLLECTION_RETAIL_CRON_SETUP
                                   where x.DELETED == false
                                   select new CollectionsRetailCronSetupViewModel
                                   {
                                       cronJobId = x.CRONJOBID,
                                       startDate = x.STARTDATE,
                                       startTime = x.STARTTIME,
                                       endDate = x.ENDDATE,
                                       endTime = x.ENDTIME,
                                       cronNature = x.CRONNATURE,
                                       createdBy = x.CREATEDBY,
                                       dateTimeCreated = x.DATETIMECREATED,
                                   };

            return retailCollection.ToList();
        }

        public IEnumerable<CollectionsRetailComputationVariableSetupViewModel> GetCollectionRetailComputationVariablesSetup()
        {
            var retailCollection = from x in context.TBL_COLLECTION_COMPUTATION_VARIABLES_SETUP
                                   where x.DELETED == false
                                   select new CollectionsRetailComputationVariableSetupViewModel
                                   {
                                       computationVariableId = x.COMPUTATIONVARIABLEID,
                                       vat = x.VAT,
                                       wht = x.WHT,
                                       commissionPayable = x.COMMISSIONPAYABLE,
                                       commissionPayableLimit = x.COMMISSIONPAYABLELIMIT,
                                       commissionRateExternal = x.COMMISSIONRATEEXTERNAL,
                                       createdBy = x.CREATEDBY,
                                       dateTimeCreated = x.DATETIMECREATED,
                                       recoveredAmountAbove = x.RECOVEREDAMOUNTABOVE,
                                       recoveredAmountBelow = x.RECOVEREDAMOUNTBELOW,
                                       commissionRateExternal2 = x.COMMISSIONRATEEXTERNALTWO,
                                       deleted = x.DELETED,
                                       recoveredAmountExternalAbove = x.RECOVEREDAMOUNTEXTERNALABOVE,
                                       recoveredAmountExternalBelow = x.RECOVEREDAMOUNTEXTERNALBELOW,
                                   };

            return retailCollection.ToList();
        }

        public async Task<bool> AddRetailCollectionCronJobAsync(CollectionsRetailCronSetupViewModel model)
        {
            var response = 0;

            try
            {
                var setup = new TBL_COLLECTION_RETAIL_CRON_SETUP()
                {
                    STARTDATE = model.startDate,
                    STARTTIME = model.startTime,
                    ENDDATE = model.endDate,
                    ENDTIME = model.endTime,
                    CRONNATURE = model.cronNature,
                    CREATEDBY = model.createdBy,
                    DATETIMECREATED = DateTime.Now,
                };

                this.context.TBL_COLLECTION_RETAIL_CRON_SETUP.Add(setup);

                response = await context.SaveChangesAsync();

            }
            catch (Exception ex) { }
            return response != 0;
        }

        public async Task<bool> AddRetailCollectionComputationVariableAsync(CollectionsRetailComputationVariableSetupViewModel model)
        {
            var response = 0;

            try
            {
                var setup = new TBL_COLLECTION_COMPUTATION_VARIABLES_SETUP()
                {
                    VAT = model.vat,
                    WHT = model.wht,
                    COMMISSIONPAYABLE = model.commissionPayable,
                    COMMISSIONPAYABLELIMIT = model.commissionPayableLimit,
                    COMMISSIONRATEEXTERNAL = model.commissionRateExternal,
                    RECOVEREDAMOUNTBELOW = model.recoveredAmountBelow,
                    RECOVEREDAMOUNTABOVE = model.recoveredAmountAbove,
                    COMMISSIONRATEEXTERNALTWO = model.commissionRateExternal2,
                    CREATEDBY = model.createdBy,
                    DATETIMECREATED = DateTime.Now,
                    DELETED = false,
                    RECOVEREDAMOUNTEXTERNALBELOW = model.recoveredAmountExternalBelow,
                    RECOVEREDAMOUNTEXTERNALABOVE = model.recoveredAmountExternalAbove,
                };

                this.context.TBL_COLLECTION_COMPUTATION_VARIABLES_SETUP.Add(setup);

                response = await context.SaveChangesAsync();

            }
            catch (Exception ex) { }
            return response != 0;
        }

        public bool UpdateCollectionRetailCronJob(CollectionsRetailCronSetupViewModel model, short id)
        {
            var setup = context.TBL_COLLECTION_RETAIL_CRON_SETUP.Find(id);

            if (setup != null)
            {
                setup.CRONJOBID = id;
                setup.STARTDATE = model.startDate;
                setup.STARTTIME = model.startTime;
                setup.ENDDATE = model.endDate;
                setup.ENDTIME = model.endTime;
                setup.CRONNATURE = model.cronNature;
            }
           
            return context.SaveChanges() > 0;
        }

        public bool UpdateCollectionRetailComputationVariables(CollectionsRetailComputationVariableSetupViewModel model, short id)
        {
            var setup = context.TBL_COLLECTION_COMPUTATION_VARIABLES_SETUP.Find(id);

            if (setup != null)
            {
                setup.VAT = model.vat;
                setup.WHT = model.wht;
                setup.COMMISSIONPAYABLE = model.commissionPayable;
                setup.COMMISSIONPAYABLELIMIT = model.commissionPayableLimit;
                setup.COMMISSIONRATEEXTERNAL = model.commissionRateExternal;
                setup.RECOVEREDAMOUNTBELOW = model.recoveredAmountBelow;
                setup.RECOVEREDAMOUNTABOVE = model.recoveredAmountAbove;
                setup.COMMISSIONRATEEXTERNALTWO = model.commissionRateExternal2;
                setup.RECOVEREDAMOUNTEXTERNALBELOW = model.recoveredAmountExternalBelow;
                setup.RECOVEREDAMOUNTEXTERNALABOVE = model.recoveredAmountExternalAbove;
            }

            return context.SaveChanges() > 0;
        }


        public async Task<bool> DeleteRetailCollectionCronJobAsync(short id, UserInfo user)
        {
            var response = 0;
            var setup = context.TBL_COLLECTION_RETAIL_CRON_SETUP.Find(id);

            if (setup != null)
            {
                setup.DELETED = true;
                setup.DATETIMEDELETED = DateTime.Now;
                response = await context.SaveChangesAsync();
            }

            return response != 0;
        }

        public async Task<bool> DeleteRetailCollectionComputationVariablesAsync(short id, UserInfo user)
        {
            var response = 0;
            var setup = context.TBL_COLLECTION_COMPUTATION_VARIABLES_SETUP.Find(id);

            if (setup != null)
            {
                setup.DELETED = true;
                response = await context.SaveChangesAsync();
            }

            return response != 0;
        }



    }
}