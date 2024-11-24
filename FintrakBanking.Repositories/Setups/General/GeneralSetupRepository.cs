using FintrakBanking.Common;
using FintrakBanking.Common.CustomException;
using FintrakBanking.Common.Enum;
using FintrakBanking.Entities.Models;
using FintrakBanking.Interfaces.Helper;
using FintrakBanking.Interfaces.Setups.General;
using FintrakBanking.ViewModels;
using FintrakBanking.ViewModels.Setups.General;
using FintrakBanking.ViewModels.WorkFlow;
using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;

namespace FintrakBanking.Repositories.Setups.General
{ 
    public class GeneralSetupRepository : IGeneralSetupRepository
    {
        private FinTrakBankingContext context;
        //private GeneralSetupRepository genSetup;
        public GeneralSetupRepository(FinTrakBankingContext _context)
        {
            this.context = _context;
             //this.genSetup = _genSetup;
        }

        public DateTime GetApplicationDate()
        {
            return this.context.TBL_FINANCECURRENTDATE.FirstOrDefault().CURRENTDATE;            
        }


        public DateTime GetApplicationEODLastRefreshedDate()
        {
            return this.context.TBL_FINANCECURRENTDATE.FirstOrDefault().LASTEODREFRESHDATETIME;
        }

        public DateTime CalculateMaturityDate(DateTime effectiveDate, TenorModeEnum tenorModeId, int tenor)
        {
            DateTime output = DateTime.Now;

            if (tenorModeId == TenorModeEnum.Days)
                output = effectiveDate.AddDays(tenor);
            else if (tenorModeId == TenorModeEnum.Months)
                output = effectiveDate.AddMonths(tenor);
            else if (tenorModeId == TenorModeEnum.Years)
                output = effectiveDate.AddYears(tenor);


            return output;
        }

        public IEnumerable<LookupViewModel> GetAllTenorMode()
        {
            return (from data in context.TBL_TENOR_MODE
                    select new LookupViewModel()
                    {
                        lookupId = data.TENORMODEID,
                        lookupName = data.TENORMODENAME
                    });
        }

        public IEnumerable<LookupViewModel> GetRegionByType(int regionTypeId)
        {
            var regions = (from data in context.TBL_BRANCH_REGION.Where(x => x.REGIONTYPEID == regionTypeId)
                    select new LookupViewModel()
                    {
                        lookupId = (short)data.REGIONID,
                        lookupName = data.REGION_NAME
                    }).ToList();

            return regions;
        }

        public int  GetLoanApplicationRef()
        {
            return CommonHelpers.GetLoanReferanceNumber();
        }


        /// <summary>
        /// Returns All tbl_Product group
        /// </summary>
        /// <returns>IEnumerable of ProductGroup</returns>
        public IEnumerable<LookupViewModel> GetAllCurrency()
        {
            return (from data in context.TBL_CURRENCY
                    where data.INUSE == true
                    select new LookupViewModel()
                    {
                        lookupId = data.CURRENCYID,
                        lookupName = data.CURRENCYCODE + " -- " + data.CURRENCYNAME,
                         lookupTypeName = data.CURRENCYCODE
                    });
        }
        

        public IEnumerable<LookupViewModel> GetSector() {
            return (from   data in context.TBL_SECTOR
                    select new LookupViewModel()
                    {
                        lookupId = data.SECTORID ,
                        lookupName = data.NAME  
                    }).OrderBy(l => l.lookupName).ToList();
        }

        //public IEnumerable<LookupViewModel> GetAllSectors ()
        //{
        //    return (from data in context.TBL_SECTOR
        //            select new LookupViewModel()
        //            {
        //                lookupId = data.SECTORID,
        //                lookupName = data.NAME,
        //                lookupCode = data.CODE,
        //                lookupLoanLimit = data.LOAN_LIMIT
        //            });
        //}

        public IEnumerable<LookupViewModel> GetSubsector()
        {
            return (from data in context.TBL_SUB_SECTOR 
                    select new LookupViewModel()
                    {
                        lookupId = data.SUBSECTORID,
                        lookupName = data.NAME
                    }).OrderBy(l => l.lookupName);
        }

        public IEnumerable<LookupViewModel> GetAllCustomerType()
        {
            return (from data in context.TBL_CUSTOMER_TYPE
                    select new LookupViewModel()
                    {
                        lookupId = data.CUSTOMERTYPEID,
                        lookupName = data.NAME
                    });
        }

        public IEnumerable<LookupViewModel> GetAllDealClassificationType()
        {
            return (from data in context.TBL_DEAL_CLASSIFICATION
                    select new LookupViewModel()
                    {
                        lookupId = data.DEALCLASSIFICATIONID,
                        lookupName = data.CLASSIFICATION
                    });
        }

        public IEnumerable<LookupViewModel> GetAllDayCount()
        {
            return (from data in context.TBL_DAY_COUNT_CONVENTION
                    where data.INUSE == true
                    select new LookupViewModel()
                    {
                        lookupId = data.DAYCOUNTCONVENTIONID,
                        lookupName = data.DAYCOUNTCONVENTIONNAME
                    });
        }

        public IEnumerable<LookupViewModel> GetAllFeeAmortisationType()
        {
            return (from data in context.TBL_FEE_AMORTISATION_TYPE
                    select new LookupViewModel()
                    {
                        lookupId = data.FEEAMORTISATIONTYPEID,
                        lookupName = data.FEEAMORTISATIONTYPENAME
                    });
        }

        public IEnumerable<LookupViewModel> GetAllDealTypes()
        {
            return (from data in context.TBL_DEAL_TYPE
                    select new LookupViewModel()
                    {
                        lookupId = data.DEALTYPEID,
                        lookupName = data.DEALTYPENAME
                    });
        }

        public IEnumerable<LookupViewModel> GetAllFSTypes()
        {
            return (from data in context.TBL_FINANCIAL_STATEMENT_TYPE
                    select new LookupViewModel()
                    {
                        lookupId = data.FSTYPEID,
                        lookupName = data.FSTYPENAME
                    });
        }

        public IEnumerable<LookupViewModel> GetAllFrequencyTypes()
        {
            return (from data in context.TBL_FREQUENCY_TYPE
                    select new LookupViewModel()
                    {
                        lookupId = data.FREQUENCYTYPEID,
                        lookupName = data.MODE,
                        isVisible = data.ISVISIBLE,
                        value = data.VALUE,
                    });
        }

        public IEnumerable<LookupViewModel> GetAllOperationTypes()
        {
            return (from data in context.TBL_OPERATIONS_TYPE
                    orderby data.OPERATIONTYPENAME ascending
                    where data.INUSE == true
                    select new LookupViewModel()
                    {
                        lookupId = data.OPERATIONTYPEID,
                        lookupName = data.OPERATIONTYPENAME
                    });
        }

        public IEnumerable<LookupViewModel> GetAllOperations()
        {
            var data = (from a in context.TBL_OPERATIONS
                        orderby a.OPERATIONNAME ascending
                        where a.ISDISABLED == false
                        select new LookupViewModel()
                        {
                            lookupId = (short)a.OPERATIONID,
                            lookupName = a.OPERATIONNAME,
                            lookupTypeId = a.OPERATIONTYPEID,
                            lookupTypeName = a.TBL_OPERATIONS_TYPE.OPERATIONTYPENAME
                        }).OrderBy(l => l.lookupName).ToList();

            return data;
        }
      
        public IEnumerable<LookupViewModel> GetOperations(short operationTypeId)
        {
            var operations = (from data in context.TBL_OPERATIONS
                    where data.OPERATIONTYPEID == operationTypeId && data.ISDISABLED == false
                    select new LookupViewModel()
                    {
                        lookupId = (short)data.OPERATIONID,
                        lookupName = data.OPERATIONNAME,
                        lookupTypeId = data.OPERATIONTYPEID,
                        lookupTypeName = data.TBL_OPERATIONS_TYPE.OPERATIONTYPENAME
                    }).ToList();

            var operations2 = (from data in context.TBL_OPERATIONS
                              join op in context.TBL_OPERATIONS_TYPE on data.OPERATIONTYPEID equals op.BINDINGTYPEID
                              where data.OPERATIONTYPEID == op.BINDINGTYPEID && op.OPERATIONTYPEID == operationTypeId && data.ISDISABLED == false

                               select new LookupViewModel()
                              {
                                  lookupId = (short)data.OPERATIONID,
                                  lookupName = data.OPERATIONNAME,
                                  lookupTypeId = data.OPERATIONTYPEID,
                                  lookupTypeName = data.TBL_OPERATIONS_TYPE.OPERATIONTYPENAME
                              }).ToList();
            return operations.Union(operations2).OrderBy(l => l.lookupName).ToList();
        }

        /// <summary>
        /// Adds new product group
        /// </summary>
        /// <param name="group"></param>
        /// <returns>true/false</returns>

        //public async Task<bool> SaveProductGroup(ProductGroupViewModel group)
        //{
        //    var productGroup = new TblProductGroup()
        //    {
        //        ProductGroupName = group.productGroupName,
        //        ProductGroupCode = group.productGroupCode
        //    };

        //    this.context.TblProductGroup.Add(productGroup);

        //    var response = await this.context.SaveChangesAsync();
        //    return response != 0;
        //}

        //   public IEnumerable<LoanCovenantDetailViewModel>  Get


        public IEnumerable<SectorViewModel> GetAllSectors()
        {
            var data = (from cs in context.TBL_SECTOR
                        select new SectorViewModel()
                        {
                            sectorId = cs.SECTORID,
                            sectorName = cs.NAME,
                            sectorCode = cs.CODE,
                            sectorLimit =cs.LOAN_LIMIT,
                        });

            return data;
        }

        public IEnumerable<GlobalSectorViewModel> GetAllGlobalSectors()
        {
            var data = (from cs in context.TBL_SECTOR_GLOBAL_LIMIT
                        select new GlobalSectorViewModel()
                        {
                            id = cs.ID,
                            date = cs.DATE,
                            cbnSector = cs.CBNSECTOR,
                            cbnSectorId = cs.CBNSECTORID,
                            totalExposureLcy = cs.TOTALEXPOSURELCY,
                            percentageExposures = cs.EXPOSURES,
                            percentageSectorLimit = cs.SECTORLIMIT
                        });

            return data;
        }



        public IEnumerable<SectorViewModel> GetAllSubSectors()
        {
            var data = (from cs in context.TBL_SUB_SECTOR
                        select new SectorViewModel()
                        {
                            subSectorId = cs.SUBSECTORID,
                            sectorId = cs.TBL_SECTOR.SECTORID,
                            sectorName = cs.NAME,
                            sectorCode = cs.CODE,
                        }).Distinct().OrderBy(s => s.sectorName);

            return data;
        }


        public IEnumerable<SectorViewModel> GetSectorsBySubSectorId(short ssId)
        {
            var data = (from s in context.TBL_SUB_SECTOR
                        where s.SUBSECTORID == ssId
                        select new SectorViewModel()
                        {
                            subSectorId = s.SUBSECTORID,
                            sectorId = s.TBL_SECTOR.SECTORID,
                            sectorName = s.NAME,
                            sectorCode = s.CODE
                        });

            return data;
        }

        public List<SectorViewModel> GetSubSectorsBySector(int sectorId)
        {
            using (FinTrakBankingContext context = new FinTrakBankingContext())
            {
                var data = (from cs in context.TBL_SUB_SECTOR
                            where cs.SECTORID == sectorId
                            orderby cs.NAME ascending
                            select new SectorViewModel()
                            {
                                subSectorId = (short)cs.SUBSECTORID,
                                sectorId = (short)cs.TBL_SECTOR.SECTORID,
                                sectorName = cs.NAME,
                                sectorCode = cs.CODE,
                            }).Distinct();

                return data.ToList();
            }
        }
        public void ValidateAgainstCompanyLimit(int companyId, decimal? limit)
        {
            var company = context.TBL_COMPANY.Find(companyId);
            var currCode = context.TBL_CURRENCY.Find(company.CURRENCYID).CURRENCYCODE;
            if (limit > company.SHAREHOLDERSFUND)
            {
                throw new SecureException("Limit cannot be greater than Company Limit of " + currCode + String.Format("{0:0,0.00}", company.SHAREHOLDERSFUND));
            }
        }

        public bool AddSector(SectorViewModel model)
        {
            var sectors = context.TBL_SECTOR.ToList();
            var limit = sectors.Sum(s => s.LOAN_LIMIT) + model.sectorLimit;
            ValidateAgainstCompanyLimit(model.companyId, limit);
                var response = 0;
                context.TBL_SECTOR.Add(new TBL_SECTOR()
                {
                    CODE = model.sectorCode,
                    NAME = model.sectorName,
                    LOAN_LIMIT = model.sectorLimit
                });



            // Audit Section ---------------------------
            var audit = new TBL_AUDIT
                {
                    AUDITTYPEID = (short)AuditTypeEnum.SectorAdded,
                    STAFFID = (int)model.createdBy,
                    BRANCHID = (short)model.userBranchId,
                    DETAIL = $"Added Sector: '{model.sectorName}' with code: {model.sectorCode} ",
                    IPADDRESS = CommonHelpers.GetLocalIpAddress(),
                    URL = model.applicationUrl,
                    APPLICATIONDATE = DateTime.Now,
                    SYSTEMDATETIME = DateTime.Now,
                     DEVICENAME = CommonHelpers.GetDeviceName(),
                    OSNAME = CommonHelpers.FriendlyName(),
            };
            context.TBL_AUDIT.Add(audit);
            //end of Audit section -------------------------------
            response = context.SaveChanges();
            return response != 0;
        }

        public bool UpdateSector (SectorViewModel model, short id)
        {
            ValidateAgainstCompanyLimit(model.companyId, model.sectorLimit);
            var response = 0;
            var sector  = context.TBL_SECTOR.Find(id);

            if (sector != null)
            {
                sector.CODE = model.sectorCode;
                sector.NAME = model.sectorName;
                sector.LOAN_LIMIT = model.sectorLimit;


                // Audit Section ---------------------------
                var audit = new TBL_AUDIT
                {
                    AUDITTYPEID = (short)AuditTypeEnum.SectorUpdated,
                    STAFFID = (int)model.lastUpdatedBy,
                    BRANCHID = (short)model.userBranchId,
                    DETAIL = $"Updated sector: '{model.sectorName}' with code: {model.sectorCode} ",
                    IPADDRESS = CommonHelpers.GetLocalIpAddress(),
                    URL = model.applicationUrl,
                    APPLICATIONDATE = DateTime.Now,
                    SYSTEMDATETIME = DateTime.Now,
                    DEVICENAME = CommonHelpers.GetDeviceName(),
                    OSNAME = CommonHelpers.FriendlyName(),
                };
                context.TBL_AUDIT.Add(audit);
                //end of Audit section -------------------------------
                response = context.SaveChanges();
            }

            return response != 0;
        }

        public bool UpdateGlobalSector(GlobalSectorViewModel model, int id)
        {
            var response = false;

            var computeThreshold = context.TBL_SECTOR_GLOBAL_LIMIT.Where(x => x.CBNSECTORID != null).Sum(x => x.SECTORLIMIT);
            var oldThreshold = computeThreshold - (decimal)model.percentageSectorLimit;
            var currentThreshold = oldThreshold + (decimal)model.percentageSectorLimit;
            if (computeThreshold != null && oldThreshold < (decimal)1.2000 && currentThreshold > (decimal)1.2000)
            {
                throw new SecureException("The limit entered has exceeded the cumulative threshold of 120 percent");
            }
            var sector = context.TBL_SECTOR_GLOBAL_LIMIT.Find(id);

            if (sector != null)
            {
                sector.SECTORLIMIT = (decimal)model.percentageSectorLimit;
                
                    // Audit Section ---------------------------
                    var audit = new TBL_AUDIT
                    {
                        AUDITTYPEID = (short)AuditTypeEnum.SectorUpdated,
                        STAFFID = model.createdBy,
                        TARGETID = sector.ID,
                        BRANCHID = model.userBranchId,
                        DETAIL = $"Updated global sector: '{sector.CBNSECTOR}' with code: {sector.CBNSECTORID} ",
                        IPADDRESS = CommonHelpers.GetLocalIpAddress(),
                        URL = model.applicationUrl,
                        APPLICATIONDATE = DateTime.Now,
                        SYSTEMDATETIME = DateTime.Now,
                        DEVICENAME = CommonHelpers.GetDeviceName(),
                        OSNAME = CommonHelpers.FriendlyName(),
                    };
                    context.TBL_AUDIT.Add(audit);
                    //end of Audit section -------------------------------
                    if (context.SaveChanges() > 0)
                    {
                        return response = true;
                    }
                
            }

            return response;
        }

        public bool DeleteSector(int id, UserInfo user)
        {
            var response = 0;
            var sector = context.TBL_SECTOR.Find(id);

            if (sector != null)
            {
                context.TBL_SECTOR.Remove(sector);


                // Audit Section ---------------------------
                var audit = new TBL_AUDIT
                {
                    AUDITTYPEID = (short)AuditTypeEnum.SectorDeleted,
                    STAFFID = user.staffId,
                    BRANCHID = (short)user.BranchId,
                    DETAIL = $"Deleted sector: '{sector.ToString()} ",
                    IPADDRESS = CommonHelpers.GetLocalIpAddress(),
                    URL = user.applicationUrl,
                    APPLICATIONDATE = GetApplicationDate(),
                    SYSTEMDATETIME = DateTime.Now,
                    DEVICENAME = CommonHelpers.GetDeviceName(),
                    OSNAME = CommonHelpers.FriendlyName(),
                };
                context.TBL_AUDIT.Add(audit);
                //end of Audit section -------------------------------
                response = context.SaveChanges();
            }

            return response != 0;
        }

        public IEnumerable<int> GetRelievedStaffApprovalLevelIds(int staffId, int operationId)
        {
            var now = DateTime.Now;

            var staffIds = context.TBL_STAFF_RELIEF
                .Where(x => x.DELETED == false
                    && x.RELIEFSTAFFID == staffId
                    && x.STARTDATE <= now
                    && x.ENDDATE >= now
                    && x.ISACTIVE == true
                ).Select(x => x.STAFFID).Distinct().ToList();

            var staff = context.TBL_STAFF.Where(x => staffIds.Contains(x.STAFFID));
            var roleids = staff.Select(x => x.STAFFROLEID).ToList();

            var roleLevelIds = context.TBL_APPROVAL_LEVEL
                .Where(x => x.DELETED == false && roleids.Contains((int)x.STAFFROLEID))
                .Select(x => x.APPROVALLEVELID)
                .Distinct();

            var allLevels = context.TBL_APPROVAL_GROUP_MAPPING
                .Where(x => x.OPERATIONID == operationId)
                .Select(g => g.TBL_APPROVAL_GROUP)
                .SelectMany(x => x.TBL_APPROVAL_LEVEL
                .Where(l => l.ISACTIVE == true));

            var staffWorkflow = allLevels.SelectMany(l => l.TBL_APPROVAL_LEVEL_STAFF).Where(x => x.DELETED == false && staffIds.Contains(x.STAFFID));

            var staffLevels = staffWorkflow.Select(x => x.APPROVALLEVELID).Distinct();

            return staffLevels.Union(roleLevelIds);
        }

        public IEnumerable<int> GetStaffApprovalLevelIds(int staffId, int operationId)
        {
            var relievedLevelids = GetRelievedStaffApprovalLevelIds(staffId, operationId).ToList(); // for approval delegation

            var staff = context.TBL_STAFF.Find(staffId);

            var roleLevelIds = context.TBL_APPROVAL_LEVEL
                .Where(x => x.DELETED == false && x.STAFFROLEID == staff.STAFFROLEID)
                .Select(x => x.APPROVALLEVELID)
                .Distinct().ToList();

            int scope = (int)ProcessViewScopeEnum.Level; // default 1

            var allLevels = context.TBL_APPROVAL_GROUP_MAPPING
                .Where(x => x.OPERATIONID == operationId)
                .Select(g => g.TBL_APPROVAL_GROUP)
                .SelectMany(x => x.TBL_APPROVAL_LEVEL
                .Where(l => l.DELETED == false && l.ISACTIVE == true)).ToList();

            if (operationId == 0)
            {
                allLevels = context.TBL_APPROVAL_GROUP_MAPPING
                .Where(x => x.DELETED == false)
                .Select(g => g.TBL_APPROVAL_GROUP)
                .SelectMany(x => x.TBL_APPROVAL_LEVEL
                .Where(l => l.DELETED == false && l.ISACTIVE == true)).ToList();
            }

            var staffWorkflow = allLevels.SelectMany(l => l.TBL_APPROVAL_LEVEL_STAFF).Where(x => x.STAFFID == staffId && x.DELETED == false).ToList();
                
            if (staffWorkflow.Count() > 0) scope = staffWorkflow.Max(x => x.PROCESSVIEWSCOPEID);

            if (scope == 3) return allLevels.Select(x => x.APPROVALLEVELID).Distinct().Union(roleLevelIds).Union(relievedLevelids);

            var approvalStaffLevels = staffWorkflow.Select(x => x.APPROVALLEVELID).Distinct();

            if (scope == 2)
            {
                var groups = context.TBL_APPROVAL_LEVEL.Where(x => x.DELETED == false && approvalStaffLevels.Contains(x.APPROVALLEVELID)).Select(x => x.GROUPID).Distinct();
                return context.TBL_APPROVAL_LEVEL
                    .Where(x => groups.Contains(x.GROUPID))
                    .Select(x => x.APPROVALLEVELID)
                    .Distinct()
                    .Union(roleLevelIds)
                    .Union(relievedLevelids);
            }

            //return staffLevels.Union(roleLevelIds); // without relief code
            return approvalStaffLevels.Union(roleLevelIds).Union(relievedLevelids);
        }

        public IEnumerable<int> GetStaffApprovalLevelIdsWithoutRelief(int staffId, int operationId)
        {
            var staff = context.TBL_STAFF.Find(staffId);

            var roleLevelIds = context.TBL_APPROVAL_LEVEL
                .Where(x => x.DELETED == false && x.STAFFROLEID == staff.STAFFROLEID)
                .Select(x => x.APPROVALLEVELID)
                .Distinct().ToList();

            int scope = (int)ProcessViewScopeEnum.Level; // default 1

            var allLevels = context.TBL_APPROVAL_GROUP_MAPPING
                .Where(x => x.OPERATIONID == operationId)
                .Select(g => g.TBL_APPROVAL_GROUP)
                .SelectMany(x => x.TBL_APPROVAL_LEVEL
                .Where(l => l.DELETED == false && l.ISACTIVE == true)).ToList();

            var staffWorkflow = allLevels.SelectMany(l => l.TBL_APPROVAL_LEVEL_STAFF).Where(x => x.STAFFID == staffId).ToList();

            if (staffWorkflow.Count() > 0) scope = staffWorkflow.Max(x => x.PROCESSVIEWSCOPEID);

            if (scope == 3) return allLevels.Select(x => x.APPROVALLEVELID).Distinct().Union(roleLevelIds);

            var staffLevels = staffWorkflow.Select(x => x.APPROVALLEVELID).Distinct();

            if (scope == 2)
            {
                var groups = context.TBL_APPROVAL_LEVEL.Where(x => x.DELETED == false && staffLevels.Contains(x.APPROVALLEVELID)).Select(x => x.GROUPID).Distinct();
                return context.TBL_APPROVAL_LEVEL
                    .Where(x => groups.Contains(x.GROUPID))
                    .Select(x => x.APPROVALLEVELID)
                    .Distinct()
                    .Union(roleLevelIds);
            }

            //return staffLevels.Union(roleLevelIds); // without relief code
            return staffLevels.Union(roleLevelIds);
        }

        public IEnumerable<int> GetStaffApprovalLevelIdsForAdhoc(int staffId, int operationId = 0)
        {
            var relievedLevelids = GetRelievedStaffApprovalLevelIds(staffId, operationId); // for approval delegation

            var staff = context.TBL_STAFF.Find(staffId);

            var roleLevelIds = context.TBL_APPROVAL_LEVEL
                .Where(x => x.DELETED == false && x.STAFFROLEID == staff.STAFFROLEID)
                .Select(x => x.APPROVALLEVELID)
                .Distinct();

            int scope = (int)ProcessViewScopeEnum.Level; // default 1

            var allLevels = context.TBL_APPROVAL_GROUP_MAPPING
                .Where(x => x.OPERATIONID == operationId)
                .Select(g => g.TBL_APPROVAL_GROUP)
                .SelectMany(x => x.TBL_APPROVAL_LEVEL
                .Where(l => l.DELETED == false && l.ISACTIVE == true)).ToList();

            var staffWorkflow = allLevels.SelectMany(l => l.TBL_APPROVAL_LEVEL_STAFF).Where(x => x.STAFFID == staffId).ToList();

            if (staffWorkflow.Count() > 0) scope = staffWorkflow.Max(x => x.PROCESSVIEWSCOPEID);

            if (scope == 3) return allLevels.Select(x => x.APPROVALLEVELID).Distinct().Union(roleLevelIds).Union(relievedLevelids);

            var staffLevels = staffWorkflow.Select(x => x.APPROVALLEVELID).Distinct();

            if (scope == 2)
            {
                var groups = context.TBL_APPROVAL_LEVEL.Where(x => x.DELETED == false && staffLevels.Contains(x.APPROVALLEVELID)).Select(x => x.GROUPID).Distinct();
                return context.TBL_APPROVAL_LEVEL
                    .Where(x => groups.Contains(x.GROUPID))
                    .Select(x => x.APPROVALLEVELID)
                    .Distinct()
                    .Union(roleLevelIds)
                    .Union(relievedLevelids);
            }

            //return staffLevels.Union(roleLevelIds); // without relief code
            return staffLevels.Union(roleLevelIds).Union(relievedLevelids);
        }

        public List<int> GetRouteLevels(int operationId, int depth)
        {
            var levels = context.TBL_APPROVAL_GROUP_MAPPING.Where(x => x.OPERATIONID == operationId)
                    .Join(context.TBL_APPROVAL_GROUP,
                        m => m.GROUPID, g => g.GROUPID, (m, g) => new { m, g })
                    .Join(context.TBL_APPROVAL_LEVEL.Where(x => x.ISACTIVE == true && x.DELETED == false),
                        mg => mg.g.GROUPID, l => l.GROUPID, (mg, l) => new ApprovalLevelInfo
                        {
                            groupId = l.GROUPID,
                            groupPosition = mg.m.POSITION,
                            levelPosition = l.POSITION,
                            levelId = l.APPROVALLEVELID,
                            levelName = l.LEVELNAME,
                            staffRoleId = l.STAFFROLEID,
                            levelTypeId = l.LEVELTYPEID,
                        })
                        .OrderBy(x => x.groupPosition)
                        .ThenBy(x => x.levelPosition)
                        .ToList();

            var routeLevels = levels.Where(x => x.levelTypeId == 2);

            IEnumerable <ApprovalLevelInfo> cls = null;
            List<ApprovalLevelInfo> controlLevels = new List<ApprovalLevelInfo>();
            foreach (var routeLevel in routeLevels)
            {
                cls = levels.Where(x => x.groupId == routeLevel.groupId 
                        && x.levelPosition >= routeLevel.levelPosition && x.levelPosition <= (routeLevel.levelPosition + depth)
                      );
                controlLevels.AddRange(cls.ToList());
            }

            return controlLevels.Select(x => x.levelId).Distinct().ToList();
        }

        public IEnumerable<ProfileBusinessUnitViewModel> GetProfileBusinessUnits()
        {
            return context.TBL_PROFILE_BUSINESS_UNIT
               .Select(x => new ProfileBusinessUnitViewModel
               {
                   businessUnitId = x.BUSINESSUNITID,
                   businessUnitName = x.BUSINESSUNITNAME,
               })
               .ToList();
        }

        public List<int> GetStaffRlieved(int staffId)
        {
            List<int> staffs = new List<int>();
            var now = DateTime.Now;
            staffs.Add(staffId);
            var staffIds = context.TBL_STAFF_RELIEF
                .Where(x => x.DELETED == false
                    && x.RELIEFSTAFFID == staffId
                    && x.STARTDATE <= now
                    && x.ENDDATE >= now
                    && x.ISACTIVE == true
                ).Select(x => x.STAFFID).Distinct();
            if (staffIds.Any())
            {
                foreach (var rec in staffIds)
                {
                    staffs.Add(rec);
                }
            }

            return staffs;
        }

    }

}