using FintrakBanking.Common.Enum;
using FintrakBanking.Entities.Models;
using FintrakBanking.Interfaces.Admin;
using FintrakBanking.Interfaces.Credit;
using FintrakBanking.Interfaces.Setups.Approval;
using FintrakBanking.Interfaces.Setups.General;
using FintrakBanking.Interfaces.WorkFlow;
using FintrakBanking.ViewModels.Credit;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FintrakBanking.Repositories.Credit
{
    public class DashboardRepository : IDashboardRepository
    {
        private FinTrakBankingContext context;
        private IGeneralSetupRepository general;
        private IAuditTrailRepository audit;
        private IWorkflow workflow;
        private IApprovalLevelStaffRepository levelStaffRepo;

        public DashboardRepository(FinTrakBankingContext context, IGeneralSetupRepository general, 
                                    IAuditTrailRepository audit, IWorkflow workflow, IApprovalLevelStaffRepository _levelStaffRepo)
        {
            this.context = context;
            this.general = general;
            this.audit = audit;
            this.workflow = workflow;
            this.levelStaffRepo = _levelStaffRepo;
        }

        public List<DashboardViewModel> LoanApplicationsBySector(DateTime startDate, DateTime endDate, int companyId, int staffId)
        {
            var staff = context.TBL_STAFF.Where(o => o.STAFFID == staffId).Select(o => o).FirstOrDefault();

            var data = (from x in context.TBL_LOAN_APPLICATION_DETAIL
                         join l in context.TBL_LOAN_APPLICATION on x.LOANAPPLICATIONID equals l.LOANAPPLICATIONID
                         join sb in context.TBL_SUB_SECTOR on x.SUBSECTORID equals sb.SUBSECTORID
                         join s in context.TBL_SECTOR on sb.SECTORID equals s.SECTORID
                         where l.APPLICATIONSTATUSID != (int)LoanApplicationStatusEnum.CancellationInProgress
                                && l.APPLICATIONSTATUSID != (int)LoanApplicationStatusEnum.CancellationCompleted
                                && x.STATUSID == (int)ApprovalStatusEnum.Approved
                                && l.COMPANYID == companyId
                                && x.DATETIMECREATED >= startDate && x.DATETIMECREATED <= endDate
                         select new { x, l, sb, s })?.ToList();

            if (staff?.TBL_STAFF_ROLE.STAFFROLECODE =="RM")
            {
                data = data.Where(o => o.l.RELATIONSHIPMANAGERID == staffId)?.ToList();

            }
            else if(staff?.TBL_STAFF_ROLE.STAFFROLECODE == "BM")
            {
                data = data.Where(o => o.l.BRANCHID == staff.BRANCHID)?.ToList();
            }

            List<DashboardViewModel> result = new List<DashboardViewModel>();
            if (data != null && data.Count() > 0)
            {
                result = (from res in data
                          group res by new { res.s.SECTORID, res.s.NAME } into gg
                          select new DashboardViewModel
                          {
                              loanCount = gg?.Count() ?? 0,
                              sumOfProposedAmount = gg?.Sum(g => (double)g.x.APPROVEDAMOUNT * g.x.EXCHANGERATE) ?? 0,
                              sectorName = gg.Key.NAME
                          })?.ToList();
            }
            return result;
        }

        public List<DashboardReportItem> LoanPerformance(DateTime startDate, DateTime endDate, int companyId, int staffId)
        {
            int count = 0;

            var staff = context.TBL_STAFF.Where(o => o.STAFFID == staffId).Select(o => o).FirstOrDefault();

            var data = (from a in context.TBL_LOAN
                                              where a.APPROVALSTATUSID == (int)ApprovalStatusEnum.Approved
                                              && a.EFFECTIVEDATE >= startDate && a.EFFECTIVEDATE <= endDate && a.COMPANYID == companyId
                        select new { a })?.ToList();

            if (staff?.TBL_STAFF_ROLE.STAFFROLECODE == "RM")
            {
                data = data.Where(o => o.a.RELATIONSHIPMANAGERID == staffId)?.ToList();

            }
            else if (staff?.TBL_STAFF_ROLE.STAFFROLECODE == "BM")
            {
                data = data.Where(o => o.a.BRANCHID == staff.BRANCHID)?.ToList();
            }

            List<DashboardReportItem> loanDetails = new List<DashboardReportItem>();
            if (data != null && data.Count() > 0)
            {
                loanDetails = (from rec in data
                               group rec by new { rec.a.USER_PRUDENTIAL_GUIDE_STATUSID } into gg
                               select new DashboardReportItem
                               {
                                   id = count + 1,
                                   hoursSpent = gg?.Count() ?? 0,
                                   name = context.TBL_LOAN_PRUDENT_GUIDE_TYPE.Where(x => x.PRUDENTIALGUIDELINETYPEID == gg.Key.USER_PRUDENTIAL_GUIDE_STATUSID).Select(x => x.PRUDENTIALGUIDELINETYPENAME).FirstOrDefault(),
                               })?.ToList();
            }
            return loanDetails;
        }

        public List<LoanDisburseByType> LoanDisbursedByType(DateTime startDate, DateTime endDate, int companyId, int staffId)
        {
            int count = 0;
            var staff = context.TBL_STAFF.Where(o => o.STAFFID == staffId).Select(o => o).FirstOrDefault();

            var loanDetails = (from a in context.TBL_LOAN
                               where
                                a.EFFECTIVEDATE >= startDate && a.EFFECTIVEDATE <= endDate && a.COMPANYID == companyId
                               select new LoanViewModel { relationshipManagerId= a.RELATIONSHIPMANAGERID, loanSystemTypeId=a.LOANSYSTEMTYPEID, branchId=a.BRANCHID,})?.ToList();

            var od = (from a in context.TBL_LOAN_REVOLVING
                      where
                       a.EFFECTIVEDATE >= startDate && a.EFFECTIVEDATE <= endDate && a.COMPANYID == companyId
                      select new LoanViewModel { relationshipManagerId = a.RELATIONSHIPMANAGERID, loanSystemTypeId = a.LOANSYSTEMTYPEID, branchId = a.BRANCHID, })?.ToList();

            var contingent = (from a in context.TBL_LOAN_CONTINGENT
                              where 
                               a.EFFECTIVEDATE >= startDate && a.EFFECTIVEDATE <= endDate && a.COMPANYID == companyId
                              select new LoanViewModel { relationshipManagerId = a.RELATIONSHIPMANAGERID, loanSystemTypeId = a.LOANSYSTEMTYPEID, branchId = a.BRANCHID, })?.ToList();

            var data = loanDetails.Union(od).Union(contingent)?.ToList();

            if (staff?.TBL_STAFF_ROLE.STAFFROLECODE == "RM")
            {
                data = data.Where(o => o.relationshipManagerId == staffId)?.ToList();
            }
            else if (staff?.TBL_STAFF_ROLE.STAFFROLECODE == "BM")
            {
                data = data.Where(o => o.branchId == staff.BRANCHID)?.ToList();
            }

            List<LoanDisburseByType> result = new List<LoanDisburseByType>();
            if (data != null && data.Count() > 0)
            {
                 result = (from rec in data
                              group rec by new { rec.loanSystemTypeId } into gg
                              select new LoanDisburseByType
                              {
                                  count = gg?.Count() ?? 0,
                                  typeId = gg.Key.loanSystemTypeId,
                                  type = context.TBL_LOAN_SYSTEM_TYPE.Where(x => x.LOANSYSTEMTYPEID == gg.Key.loanSystemTypeId).Select(x => x.LOANSYSTEMTYPENAME).FirstOrDefault()
                              })?.ToList();
            }
            return result;
        }

        public List<DashboardViewModel> LoanOnThePipeline(DateTime startDate, DateTime endDate, int companyId, int staffId)
        {
            var staff = context.TBL_STAFF.Where(o => o.STAFFID == staffId).Select(o => o).FirstOrDefault();
            var approvalLevel = levelStaffRepo.GetAllAssignedApprovalLevelStaff(companyId).Where(c => c.staffId == staffId || c.staffRoleId == staff.STAFFROLEID).ToList();
            var staffApprovalLevels = approvalLevel.Select(x => x.approvalLevelId).Distinct();
            var reliefStaff = general.GetStaffRlieved(staffId);
            List<int> ExclusiveOperations = (from flow in context.TBL_LOAN_APPLICATN_FLOW_CHANGE select flow.OPERATIONID).ToList();

            //int[] applicationStatus =  { (int)LoanApplicationStatusEnum.CancellationInProgress,
            //    (int)LoanApplicationStatusEnum.CancellationInProgress,
            //    (int)LoanApplicationStatusEnum.LoanBookingInProgress,
            //    (int)LoanApplicationStatusEnum.LoanBookingCompleted
            //};

            var data = (from x in context.TBL_LOAN_APPLICATION_DETAIL
                        join l in context.TBL_LOAN_APPLICATION on x.LOANAPPLICATIONID equals l.LOANAPPLICATIONID
                        join a in context.TBL_APPROVAL_TRAIL on x.LOANAPPLICATIONID equals a.TARGETID
                        where a.APPROVALSTATEID != (int)ApprovalState.Ended && l.DELETED == false 
                        && l.APPLICATIONSTATUSID != (int)LoanApplicationStatusEnum.CancellationInProgress
                        //&& !applicationStatus.Contains(l.APPLICATIONSTATUSID) //&& x.STATUSID == (int)ApprovalStatusEnum.Approved
                        && l.APPLICATIONSTATUSID != (int)LoanApplicationStatusEnum.CancellationCompleted 
                        && l.APPROVALSTATUSID != (int)ApprovalStatusEnum.Approved
                        && a.APPROVALSTATUSID != (int) ApprovalStatusEnum.Approved
                        && l.COMPANYID == companyId
                        && x.DATETIMECREATED >= startDate && x.DATETIMECREATED <= endDate
                        && (a.TOSTAFFID == staff.STAFFID || a.TOSTAFFID == null)
                        && a.RESPONSESTAFFID == null
                        //&& levelIds.Contains((int) a.TOAPPROVALLEVELID)
                        && l.ISADHOCAPPLICATION != true
                        && (a.TOSTAFFID == null || reliefStaff.Contains((int)a.TOSTAFFID))
                        && (ExclusiveOperations.Contains(a.OPERATIONID) || ExclusiveOperations.Contains(a.DESTINATIONOPERATIONID ?? 0))
                        && staffApprovalLevels.ToList().Contains((int)a.TOAPPROVALLEVELID)
                        select new { x, l })?.ToList();


            //if (staff?.TBL_STAFF_ROLE.STAFFROLECODE == "RM")
            //{
            //    data = data.Where(o => o.l.RELATIONSHIPMANAGERID == staffId)?.ToList();

            //}
            //else if (staff?.TBL_STAFF_ROLE.STAFFROLECODE == "BM")
            //{
            //    data = data.Where(o => o.l.BRANCHID == staff.BRANCHID)?.ToList();
            //}

            List<DashboardViewModel> result = new List<DashboardViewModel>();

            if (data != null && data.Count() > 0)
            {
                result = (from rec in data
                          group rec by new { rec.l.COMPANYID } into gg
                          select new DashboardViewModel
                          {
                              loanCount = gg?.Select(O => O.x.LOANAPPLICATIONID).Distinct().Count() ?? 0,
                              sumOfProposedAmount = gg?.Sum(g => (double)g.x.APPROVEDAMOUNT * g.x.EXCHANGERATE) ?? 0,
                          })?.ToList();
            }
            return result;
        }

        public List<DashboardViewModel> SubsLoanOnThePipeline(DateTime startDate, DateTime endDate, int companyId, int staffId)
        {
            var staff = context.TBL_STAFF.Where(o => o.STAFFID == staffId).Select(o => o).FirstOrDefault();
            var staffRole = context.TBL_STAFF_ROLE.Where(o => o.STAFFROLEID == staff.STAFFROLEID).Select(o => o).FirstOrDefault();
            
            var reliefStaff = general.GetStaffRlieved(staffId);
            List<int> ExclusiveOperations = (from flow in context.TBL_LOAN_APPLICATN_FLOW_CHANGE select flow.OPERATIONID).ToList();

            

            var data = (from x in context.TBL_SUB_BASICTRANSACTION
                        
                        where x.DELETED == false
                        && x.APPLICATIONSTATUSID != (int)LoanApplicationStatusEnum.CancellationInProgress
                        && x.APPLICATIONSTATUSID != (int)LoanApplicationStatusEnum.CancellationCompleted
                        && x.APPROVALSTATUSID != (int)ApprovalStatusEnum.Approved
                        && x.APPROVALSTATUSID != (int)ApprovalStatusEnum.Approved
                        && x.SYSTEMARRIVALDATETIME >= startDate && x.SYSTEMARRIVALDATETIME <= endDate
                        && (x.STAFFROLECODE == staffRole.STAFFROLECODE)
                        && x.ACTEDON == false
                        select new { x })?.ToList();

            List<DashboardViewModel> result = new List<DashboardViewModel>();

            if (data != null && data.Count() > 0)
            {
                result = (from rec in data
                          group rec by new { rec.x.STAFFROLECODE } into gg
                          select new DashboardViewModel
                          {
                              loanCount = gg?.Select(O => O.x.LOANAPPLICATIONID).Distinct().Count() ?? 0,
                              sumOfProposedAmount = gg?.Sum(g => (double)g.x.TOTALEXPOSUREAMOUNT ) ?? 0,
                          })?.ToList();
            }
            return result;
        }


        public DashboardViewModel GetLoanInThePipelineLms(int operationId, int staffId, int companyId, int branchId, int? classId)
        {
            var staff = context.TBL_STAFF.FirstOrDefault(O => O.STAFFID == staffId);

            var approvalOperations = context.TBL_OPERATIONS.Where(x => x.OPERATIONTYPEID == (short)OperationTypeEnum.LoanReviewApplication)
                .Select(x => x.OPERATIONID).ToList();

            bool ignoreBranch = true; // rm = false, ho = true
            if (operationId == 47) if (ProcessInitiator(staffId, operationId, classId, 2)) ignoreBranch = false;
            if (approvalOperations.Contains(operationId)) if (ProcessInitiator(staffId, operationId, classId, 1)) ignoreBranch = false;
            //if (camOperationIds.Contains(operationId)) if (ProcessInitiator(staffId, operationId, classId, 1)) ignoreBranch = false;

            List<int> operationIds = new List<int>();
            operationIds.Add(operationId);
            operationIds.AddRange(approvalOperations);

            IQueryable<LoanReviewApplicationViewModel> applications = null;
            var levelIds = general.GetStaffApprovalLevelIds(staffId, operationId);

            var query = context.TBL_LMSR_APPLICATION.Where(x => x.BRANCHID == branchId || ignoreBranch)
                        .Join(context.TBL_BRANCH, a => a.BRANCHID, b => b.BRANCHID, (a, b) => new { a, b })
                        .Join(context.TBL_CUSTOMER, ab => ab.a.CUSTOMERID, c => c.CUSTOMERID, (ab, c) => new { ab, c, b = ab.b })
                        .Join(context.TBL_APPROVAL_TRAIL.Where(x => operationIds.Contains(x.OPERATIONID)
                            // && x.APPROVALSTATEID != (int)ApprovalState.Ended
                            && (x.APPROVALSTATUSID == (short)ApprovalStatusEnum.Pending
                            || x.APPROVALSTATUSID == (short)ApprovalStatusEnum.Processing
                            || x.APPROVALSTATUSID == (short)ApprovalStatusEnum.Authorised
                            || x.APPROVALSTATUSID == (short)ApprovalStatusEnum.Referred)
                                && x.RESPONSESTAFFID == null
                                && ((levelIds.Contains((int)x.TOAPPROVALLEVELID) && x.TOSTAFFID == null) || (levelIds.Contains((int)x.TOAPPROVALLEVELID) && x.TOSTAFFID == staffId)
                                || (!levelIds.Contains((int)x.TOAPPROVALLEVELID)) && (x.TOSTAFFID == staffId))
                        ),
                            alaba => alaba.ab.a.LOANAPPLICATIONID,
                            trail => trail.TARGETID,
                            (alaba, trail) => new { application = alaba.ab.a, trail, branch = alaba.b, customer = alaba.c })
                        .Select(x => new LoanReviewApplicationViewModel
                        {
                            approvalTrailId = x.trail == null ? 0 : x.trail.APPROVALTRAILID,
                            loanReviewApplicationId = x.application.LOANAPPLICATIONID,     
                            approvedAmount = x.application.APPROVEDAMOUNT == null ? 0 : x.application.APPROVEDAMOUNT,
                            dateTimeCreated = x.application.DATETIMECREATED,
                
                        }).GroupBy(d => d.loanReviewApplicationId).ToList();

            applications = query.AsQueryable().Select(g => g.OrderByDescending(b => b.approvalTrailId).FirstOrDefault()).OrderByDescending(x => x.loanReviewApplicationId);

            var result = (new DashboardViewModel
            {
                loanCount = applications.Count(),
                sumOfProposedAmount = applications.Sum(O => (double) O.approvedAmount)
            });

            return result;
        }

        public DashboardViewModel GetApprovedLoansLms(int companyId, int staffId)
        {
            var staff = context.TBL_STAFF.Where(o => o.STAFFID == staffId).Select(o => o).FirstOrDefault();
            var approvalLevel = levelStaffRepo.GetAllAssignedApprovalLevelStaff(companyId).Where(c => c.staffId == staffId || c.staffRoleId == staff.STAFFROLEID).ToList();
            var staffApprovalLevels = approvalLevel.Select(x => x.approvalLevelId).Distinct();

            var data = (from l in context.TBL_LMSR_APPLICATION_DETAIL
                        join a in context.TBL_LMSR_APPLICATION on l.LOANAPPLICATIONID equals a.LOANAPPLICATIONID
                        join t in context.TBL_APPROVAL_TRAIL on l.LOANAPPLICATIONID equals t.TARGETID
                        where l.DELETED == false //a.APPROVALSTATEID != (int)ApprovalState.Ended &&
                        && a.APPLICATIONSTATUSID != (int)LoanApplicationStatusEnum.CancellationInProgress
                        && a.APPLICATIONSTATUSID != (int)LoanApplicationStatusEnum.CancellationCompleted
                        && a.COMPANYID == companyId
                        //&& l.DATETIMECREATED >= startDate && l.DATETIMECREATED <= endDate
                        //&& a.TOSTAFFID == staff.STAFFID
                        && t.RESPONSESTAFFID == staff.STAFFID
                        //&& levelIds.Contains((int) t.TOAPPROVALLEVELID)
                        && staffApprovalLevels.ToList().Contains((int)t.TOAPPROVALLEVELID)
                        select new { l, a })?.ToList();
            
            var result = new DashboardViewModel
            {
                loanCount = data?.Select(O => O.l.LOANAPPLICATIONID).Distinct().Count() ?? 0,
                sumOfProposedAmount = data?.Sum(x => (double) x.l.APPROVEDAMOUNT) ?? 0
            };

            return result;
        }

        private bool ProcessInitiator(int staffId, int operationId, int? productClassId, int position)
        {
            var staff = context.TBL_STAFF.Find(staffId);

            var levels = context.TBL_APPROVAL_GROUP_MAPPING.Where(x => x.TBL_OPERATIONS.OPERATIONTYPEID == 11 && x.PRODUCTCLASSID == productClassId)
                    .Join(context.TBL_APPROVAL_GROUP, m => m.GROUPID, g => g.GROUPID, (m, g) => new { m, g })
                    .Join(context.TBL_APPROVAL_LEVEL.Where(x => x.ISACTIVE == true),
                        mg => mg.g.GROUPID, l => l.GROUPID, (mg, l) => new
                        {
                            groupPosition = mg.m.POSITION,
                            levelPosition = l.POSITION,
                            levelId = l.APPROVALLEVELID,
                            levelName = l.LEVELNAME,
                            staffRoleId = l.STAFFROLEID,
                        })
                        .OrderBy(x => x.groupPosition)
                        .ThenBy(x => x.levelPosition)
                        .ToList()
                        ;

            var staffRoleLevels = levels.Where(x => x.staffRoleId == staff.STAFFROLEID);
            var staffRoleLevelIds = staffRoleLevels.Select(x => x.levelId);
            var staffRoleLevelId = staffRoleLevelIds.FirstOrDefault();

            int index = levels.FindIndex(x => x.levelId == staffRoleLevelId);

            return index == (position - 1);
        }

        public List<DashboardViewModel> ExpotureByRiskRating(DateTime startDate, DateTime endDate, int companyId, int staffId)
        {
            var staff = context.TBL_STAFF.Where(o => o.STAFFID == staffId).Select(o => o).FirstOrDefault();

            var data = (from x in context.TBL_LOAN_APPLICATION_DETAIL
                         join l in context.TBL_LOAN_APPLICATION on x.LOANAPPLICATIONID equals l.LOANAPPLICATIONID
                         join a in context.TBL_LOAN on x.LOANAPPLICATIONDETAILID equals a.LOANAPPLICATIONDETAILID
                         where  x.STATUSID == (int)ApprovalStatusEnum.Approved
                                && l.COMPANYID == companyId
                                && x.DATETIMECREATED >= startDate && x.DATETIMECREATED <= endDate
                                select new {x,l,a})?.ToList();

            if (staff?.TBL_STAFF_ROLE.STAFFROLECODE == "RM")
            {
                data = data.Where(o => o.l.RELATIONSHIPMANAGERID == staffId)?.ToList();

            }
            else if (staff?.TBL_STAFF_ROLE.STAFFROLECODE == "BM")
            {
                data = data.Where(o => o.l.BRANCHID == staff.BRANCHID)?.ToList();
            }

            List<DashboardViewModel> result = new List<DashboardViewModel>();
            if (data != null && data.Count()>0)
            {
                result = (from rec in data
                          group rec by new { rec.l.RISKRATINGID } into gg
                          select new DashboardViewModel
                          {
                              loanCount = gg?.Count() ?? 0,
                              riskRating = context.TBL_CUSTOMER_RISK_RATING.Where(y => y.RISKRATINGID == gg.Key.RISKRATINGID).Select(y => y.RISKRATING).FirstOrDefault(),
                          })?.ToList();
            }
            return result;
        }

        public List<DashboardViewModel> CollateralCoverage(DateTime startDate, DateTime endDate, int companyId, int staffId)
        {
            var staff = context.TBL_STAFF.Where(o => o.STAFFID == staffId).Select(o => o).FirstOrDefault();

            var termLoan = (from x in context.TBL_LOAN
                           join c in context.TBL_LOAN_COLLATERAL_MAPPING on x.TERMLOANID equals c.LOANID
                           where x.APPROVALSTATUSID == (int)ApprovalStatusEnum.Approved
                                  && x.COMPANYID == companyId
                                  && c.LOANSYSTEMTYPEID == (int)LoanSystemTypeEnum.TermDisbursedFacility
                                  && x.EFFECTIVEDATE >= startDate && x.EFFECTIVEDATE <= endDate
                           select new LoanCollateralMappingViewModel { amount = x.PRINCIPALAMOUNT, exchangeRate = x.EXCHANGERATE, collateralCustomerId = c.COLLATERALCUSTOMERID, note="Term Loan" , userBranchId=x.BRANCHID})?.ToList();
                         
            var contingent = (from x in context.TBL_LOAN_CONTINGENT
                           join c in context.TBL_LOAN_COLLATERAL_MAPPING on x.CONTINGENTLOANID equals c.LOANID
                           where x.APPROVALSTATUSID == (int)ApprovalStatusEnum.Approved
                                  && x.COMPANYID == companyId
                                  && c.LOANSYSTEMTYPEID == (int)LoanSystemTypeEnum.ContingentLiability
                                  && x.EFFECTIVEDATE >= startDate && x.EFFECTIVEDATE <= endDate
                             select new LoanCollateralMappingViewModel { amount = x.CONTINGENTAMOUNT, exchangeRate = x.EXCHANGERATE, collateralCustomerId = c.COLLATERALCUSTOMERID, note = "Contingent", userBranchId = x.BRANCHID })?.ToList();


            var overdraft = (from x in context.TBL_LOAN_REVOLVING
                             join c in context.TBL_LOAN_COLLATERAL_MAPPING on x.REVOLVINGLOANID equals c.LOANID
                             where x.APPROVALSTATUSID == (int)ApprovalStatusEnum.Approved
                             && c.ISRELEASED==false
                                    && x.COMPANYID == companyId
                                    && c.LOANSYSTEMTYPEID == (int)LoanSystemTypeEnum.OverdraftFacility
                                    && x.EFFECTIVEDATE >= startDate && x.EFFECTIVEDATE <= endDate
                            select new LoanCollateralMappingViewModel { amount = x.OVERDRAFTLIMIT, exchangeRate = x.EXCHANGERATE, collateralCustomerId = c.COLLATERALCUSTOMERID , note = "Overdraft" , userBranchId = x.BRANCHID })?.ToList();

            var collaterals = termLoan.Union(contingent).Union(overdraft)?.ToList();

            if (staff?.TBL_STAFF_ROLE.STAFFROLECODE == "RM")
            {
                collaterals = collaterals.Where(o => o.relationshipManagerId == staffId)?.ToList();

            }
            else if (staff?.TBL_STAFF_ROLE.STAFFROLECODE == "BM")
            {
                collaterals = collaterals.Where(o => o.userBranchId == staff.BRANCHID)?.ToList();
            }

            List<DashboardViewModel> result = new List<DashboardViewModel>();
            if (collaterals != null && collaterals.Count() > 0)
            {
                result = (from rec in collaterals
                          group rec by new { rec.collateralCustomerId, rec.note } into gg
                          select new DashboardViewModel
                          {
                              collateralCustomerId = gg.Key.collateralCustomerId,
                              loanCount = gg?.Count() ?? 0,
                              facilityAmount = gg?.Sum(x => x.amount * (decimal)x.exchangeRate) ?? 0,
                              name = gg.Key.note
                          })?.ToList();
            }
                var facilityCollateralSub = (from x in result
                                             group x by new { x.collateralCustomerId, x.facilityAmount } into aa
                                             select new DashboardViewModel
                                             {
                                                 collateralCustomerId = collaterals.Where(x => x.collateralCustomerId == aa.Key.collateralCustomerId).Select(x => x.collateralCustomerId).FirstOrDefault(),
                                                 facilityAmount = aa?.Sum(a => a.facilityAmount) ?? 0
                                             })?.ToList();

                var facilityCollateral = (from x in facilityCollateralSub
                                          join a in context.TBL_COLLATERAL_CUSTOMER on x.collateralCustomerId equals a.COLLATERALCUSTOMERID
                                          group new { x.facilityAmount, a.COLLATERALVALUE, a.HAIRCUT } by 1 into aa
                                          select new DashboardViewModel
                                          {
                                              facilityAmount = aa?.Sum(x => x.facilityAmount) ?? 0,
                                              collateralValue = aa?.Sum(x => (x.COLLATERALVALUE * (decimal)((1 - x.HAIRCUT) / 100.00))) ?? 0
                                          })?.ToList();
            
            return facilityCollateral;

        }

        public List<DashboardViewModel> ApprovedLoan(DateTime startDate, DateTime endDate, int companyId, int staffId)
        {
            var staff = context.TBL_STAFF.Where(o => o.STAFFID == staffId).Select(o => o).FirstOrDefault();
            var approvalLevel = levelStaffRepo.GetAllAssignedApprovalLevelStaff(companyId).Where(c => c.staffId == staffId || c.staffRoleId == staff.STAFFROLEID).ToList();
            var staffApprovalLevels = approvalLevel.Select(x => x.approvalLevelId).Distinct();

            var data = (from l in context.TBL_LOAN_APPLICATION_DETAIL
                        join a in context.TBL_LOAN_APPLICATION on l.LOANAPPLICATIONID equals a.LOANAPPLICATIONID
                        join t in context.TBL_APPROVAL_TRAIL on l.LOANAPPLICATIONID equals t.TARGETID
                        where l.DELETED == false //a.APPROVALSTATEID != (int)ApprovalState.Ended &&
                        && a.APPLICATIONSTATUSID != (int)LoanApplicationStatusEnum.CancellationInProgress
                        && a.APPLICATIONSTATUSID != (int)LoanApplicationStatusEnum.CancellationCompleted
                        && a.COMPANYID == companyId
                        && l.DATETIMECREATED >= startDate && l.DATETIMECREATED <= endDate
                        //&& a.TOSTAFFID == staff.STAFFID
                        && t.RESPONSESTAFFID == staff.STAFFID
                        //&& levelIds.Contains((int) t.TOAPPROVALLEVELID)
                        && staffApprovalLevels.ToList().Contains((int) t.TOAPPROVALLEVELID)
                        select new { l, a })?.ToList();

            if (staff?.TBL_STAFF_ROLE.STAFFROLECODE == "RM")
            {
                data = data.Where(o => o.a.RELATIONSHIPMANAGERID == staffId)?.ToList();

            }
            else if (staff?.TBL_STAFF_ROLE.STAFFROLECODE == "BM")
            {
                data = data.Where(o => o.a.BRANCHID == staff.BRANCHID)?.ToList();
            }

            List<DashboardViewModel> termLaon = new List<DashboardViewModel>();
            if (data != null && data.Count() > 0)
            {
                termLaon = (from rec in data
                            group rec by new { rec.a.COMPANYID } into gg
                            select new DashboardViewModel
                            {
                                loanCount = gg?.Select(O => O.l.LOANAPPLICATIONID).Distinct().Count() ?? 0,
                                sumOfProposedAmount = gg?.Sum(x => (double)x.l.APPROVEDAMOUNT * x.l.EXCHANGERATE) ?? 0
                            })?.ToList();
            }
            return termLaon;
        }

        public List<DashboardViewModel> TotalRiskExposure(DateTime startDate, DateTime endDate, int companyId, int staffId)
        {
            var staff = context.TBL_STAFF.Where(o => o.STAFFID == staffId).Select(o => o).FirstOrDefault();

            var termLaon = (from x in context.TBL_LOAN
                           where x.APPROVALSTATUSID == (int)ApprovalStatusEnum.Approved
                                && x.COMPANYID == companyId
                                && x.DATETIMECREATED >= startDate && x.DATETIMECREATED <= endDate
                           select new LoanViewModel { relationshipManagerId = x.RELATIONSHIPMANAGERID, approvedAmount = x.PRINCIPALAMOUNT, exchangeRate = x.EXCHANGERATE, branchId = x.BRANCHID,companyId=x.COMPANYID }
                           )?.ToList();

            var OD = (from x in context.TBL_LOAN_REVOLVING
                     where x.APPROVALSTATUSID == (int)ApprovalStatusEnum.Approved
                            && x.COMPANYID == companyId
                            && x.DATETIMECREATED >= startDate && x.DATETIMECREATED <= endDate
                     select new LoanViewModel { relationshipManagerId = x.RELATIONSHIPMANAGERID, approvedAmount = x.OVERDRAFTLIMIT, exchangeRate = x.EXCHANGERATE, branchId = x.BRANCHID, companyId = x.COMPANYID }
                     )?.ToList();

          var data = termLaon.Union(OD).ToList();

            if (staff?.TBL_STAFF_ROLE.STAFFROLECODE == "RM")
            {
                data = data.Where(o => o.relationshipManagerId == staffId)?.ToList();

            }
            else if (staff?.TBL_STAFF_ROLE.STAFFROLECODE == "BM")
            {
                data = data.Where(o => o.branchId == staff.BRANCHID)?.ToList();
            }

            List<DashboardViewModel> result = new List<DashboardViewModel>();
            if (data != null && data.Count() > 0)
            {
                result = (from rec in data
                          group rec by new { rec.companyId } into gg
                          select new DashboardViewModel
                          {
                              loanCount = gg?.Count() ?? 0,
                              sumOfProposedAmount = gg?.Sum(x => (double)x.approvedAmount * x.exchangeRate) ?? 0
                          })?.ToList();
            }

            return result;
        }

    }
}
