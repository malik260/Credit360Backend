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
            return (from data in context.TBL_BRANCH_REGION.Where(x => x.REGIONTYPEID == regionTypeId)
                    select new LookupViewModel()
                    {
                        lookupId = (short)data.REGIONID,
                        lookupName = data.REGION_NAME
                    });
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
                    });
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

        public IEnumerable<LookupViewModel> GetSubsector( )
        {
            return (from data in context.TBL_SUB_SECTOR 
                    select new LookupViewModel()
                    {
                        lookupId = data.SUBSECTORID,
                        lookupName = data.NAME
                    });
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
                    select new LookupViewModel()
                    {
                        lookupId = data.OPERATIONTYPEID,
                        lookupName = data.OPERATIONTYPENAME
                    });
        }

        public IEnumerable<LookupViewModel> GetAllOperations()
        {
            var data = (from a in context.TBL_OPERATIONS
                        select new LookupViewModel()
                        {
                            lookupId = (short)a.OPERATIONID,
                            lookupName = a.OPERATIONNAME,
                            lookupTypeId = a.OPERATIONTYPEID,
                            lookupTypeName = a.TBL_OPERATIONS_TYPE.OPERATIONTYPENAME
                        }).ToList();

            return data;
        }
      
        public IEnumerable<LookupViewModel> GetOperations(short operationTypeId)
        {
            return (from data in context.TBL_OPERATIONS
                    where data.OPERATIONTYPEID == operationTypeId
                    select new LookupViewModel()
                    {
                        lookupId = (short)data.OPERATIONID,
                        lookupName = data.OPERATIONNAME,
                        lookupTypeId = data.OPERATIONTYPEID,
                        lookupTypeName = data.TBL_OPERATIONS_TYPE.OPERATIONTYPENAME
                    });
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

        public IEnumerable<SectorViewModel> GetAllSubSectors()
        {
            var data = (from cs in context.TBL_SUB_SECTOR
                        select new SectorViewModel()
                        {
                            subSectorId = cs.SUBSECTORID,
                            sectorId = cs.TBL_SECTOR.SECTORID,
                            sectorName = cs.NAME,
                            sectorCode = cs.CODE,
                        }).Distinct();

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


        public bool Updatesector (SectorViewModel model, short id)
        {
            var response = 0;
            var sector  = context.TBL_SECTOR.Find(id);

            if (sector != null)
            {
                sector.CODE = model.sectorCode;
                sector.NAME = model.sectorName;
                sector.LOAN_LIMIT = model.sectorLimit;

                response = context.SaveChanges();

                // Audit Section ---------------------------
                var audit = new TBL_AUDIT
                {
                    AUDITTYPEID = (short)AuditTypeEnum.SectorUpdated,
                    STAFFID = (int)model.lastUpdatedBy,
                    BRANCHID = (short)model.userBranchId,
                    DETAIL = $"Updated branch: '{model.sectorName}' with code: {model.sectorCode} ",
                    IPADDRESS = model.userIPAddress,
                    URL = model.applicationUrl,
                    APPLICATIONDATE = DateTime.Now,
                    SYSTEMDATETIME = DateTime.Now
                };
                //end of Audit section -------------------------------
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
                ).Select(x => x.STAFFID).Distinct();

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
                .Where(l => l.DELETED == false && l.ISACTIVE == true));

            var staffWorkflow = allLevels.SelectMany(l => l.TBL_APPROVAL_LEVEL_STAFF).Where(x => x.STAFFID == staffId);

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

            return controlLevels.Select(x => x.levelId).ToList();
        }

    }

}