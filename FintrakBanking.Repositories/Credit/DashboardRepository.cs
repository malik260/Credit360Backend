using FintrakBanking.Common.Enum;
using FintrakBanking.Entities.Models;
using FintrakBanking.Interfaces.Admin;
using FintrakBanking.Interfaces.Credit;
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
        public DashboardRepository(FinTrakBankingContext context, IGeneralSetupRepository general, IAuditTrailRepository audit, IWorkflow workflow)
        {
            this.context = context;
            this.general = general;
            this.audit = audit;
            this.workflow = workflow;
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
                         select new { x, l, sb, s }).ToList();

            if (staff.TBL_STAFF_ROLE.STAFFROLECODE =="RM")
            {
                data = data.Where(o => o.l.RELATIONSHIPMANAGERID == staffId).ToList();

            }
            else if(staff.TBL_STAFF_ROLE.STAFFROLECODE == "BM")
            {
                data = data.Where(o => o.l.BRANCHID == staff.BRANCHID).ToList();
            }
   
            var result = (from res in data
            group res by new { res.s.SECTORID, res.s.NAME } into gg
            select new DashboardViewModel
            {
                loanCount = gg.Count(),
                sumOfProposedAmount = gg.Sum(g => (double)g.x.APPROVEDAMOUNT * g.x.EXCHANGERATE),
                sectorName = gg.Key.NAME
            }).ToList();

            return result;
        }

        public List<DashboardReportItem> LoanPerformance(DateTime startDate, DateTime endDate, int companyId, int staffId)
        {
            int count = 0;

            var staff = context.TBL_STAFF.Where(o => o.STAFFID == staffId).Select(o => o).FirstOrDefault();

            var data = (from a in context.TBL_LOAN
                                              where a.APPROVALSTATUSID == (int)ApprovalStatusEnum.Approved
                                              && a.EFFECTIVEDATE >= startDate && a.EFFECTIVEDATE <= endDate && a.COMPANYID == companyId
                        select new { a }).ToList();

            if (staff.TBL_STAFF_ROLE.STAFFROLECODE == "RM")
            {
                data = data.Where(o => o.a.RELATIONSHIPMANAGERID == staffId).ToList();

            }
            else if (staff.TBL_STAFF_ROLE.STAFFROLECODE == "BM")
            {
                data = data.Where(o => o.a.BRANCHID == staff.BRANCHID).ToList();
            }

            var loanDetails = (from rec in data

            group rec by new { rec.a.USER_PRUDENTIAL_GUIDE_STATUSID } into gg
            select new DashboardReportItem
            {
                id = count + 1,
                hoursSpent = gg.Count(),
                name = context.TBL_LOAN_PRUDENT_GUIDE_TYPE.Where(x => x.PRUDENTIALGUIDELINETYPEID == gg.Key.USER_PRUDENTIAL_GUIDE_STATUSID).Select(x => x.PRUDENTIALGUIDELINETYPENAME).FirstOrDefault(),
            }).ToList();

            return loanDetails;
        }
        public List<LoanDisburseByType> LoanDisbursedByType(DateTime startDate, DateTime endDate, int companyId, int staffId)
        {
            int count = 0;
            var staff = context.TBL_STAFF.Where(o => o.STAFFID == staffId).Select(o => o).FirstOrDefault();

            var loanDetails = (from a in context.TBL_LOAN
                               where
                                a.EFFECTIVEDATE >= startDate && a.EFFECTIVEDATE <= endDate && a.COMPANYID == companyId
                               select new LoanViewModel { relationshipManagerId= a.RELATIONSHIPMANAGERID, loanSystemTypeId=a.LOANSYSTEMTYPEID, branchId=a.BRANCHID,}).ToList();

            var od = (from a in context.TBL_LOAN_REVOLVING
                      where
                       a.EFFECTIVEDATE >= startDate && a.EFFECTIVEDATE <= endDate && a.COMPANYID == companyId
                      select new LoanViewModel { relationshipManagerId = a.RELATIONSHIPMANAGERID, loanSystemTypeId = a.LOANSYSTEMTYPEID, branchId = a.BRANCHID, }).ToList();

            var contingent = (from a in context.TBL_LOAN_CONTINGENT
                              where 
                               a.EFFECTIVEDATE >= startDate && a.EFFECTIVEDATE <= endDate && a.COMPANYID == companyId
                              select new LoanViewModel { relationshipManagerId = a.RELATIONSHIPMANAGERID, loanSystemTypeId = a.LOANSYSTEMTYPEID, branchId = a.BRANCHID, }).ToList();

            var data = loanDetails.Union(od).Union(contingent);

            if (staff.TBL_STAFF_ROLE.STAFFROLECODE == "RM")
            {
                data = data.Where(o => o.relationshipManagerId == staffId).ToList();
            }
            else if (staff.TBL_STAFF_ROLE.STAFFROLECODE == "BM")
            {
                data = data.Where(o => o.branchId == staff.BRANCHID).ToList();
            }

            var result = (from rec in data
            group rec by new { rec.loanSystemTypeId } into gg
            select new LoanDisburseByType
            {
                count = gg.Count(),
                typeId = gg.Key.loanSystemTypeId,
                type = context.TBL_LOAN_SYSTEM_TYPE.Where(x => x.LOANSYSTEMTYPEID == gg.Key.loanSystemTypeId).Select(x => x.LOANSYSTEMTYPENAME).FirstOrDefault()
            }).ToList();

            return result.ToList();
        }

        public List<DashboardViewModel> LoanOnThePipeline(DateTime startDate, DateTime endDate, int companyId, int staffId)
        {
            var staff = context.TBL_STAFF.Where(o => o.STAFFID == staffId).Select(o => o).FirstOrDefault();

            int[] applicationStatus =  { (int)LoanApplicationStatusEnum.CancellationInProgress,
                (int)LoanApplicationStatusEnum.CancellationInProgress,
                (int)LoanApplicationStatusEnum.LoanBookingInProgress,
                (int)LoanApplicationStatusEnum.LoanBookingCompleted };

            var data = (from x in context.TBL_LOAN_APPLICATION_DETAIL
                         join l in context.TBL_LOAN_APPLICATION on x.LOANAPPLICATIONID equals l.LOANAPPLICATIONID
                         where !applicationStatus.Contains(l.APPLICATIONSTATUSID)
                                && x.STATUSID == (int)ApprovalStatusEnum.Approved
                                && l.COMPANYID == companyId
                                && x.DATETIMECREATED >= startDate && x.DATETIMECREATED <= endDate
                         select new { x, l }).ToList();

            if (staff.TBL_STAFF_ROLE.STAFFROLECODE == "RM")
            {
                data = data.Where(o => o.l.RELATIONSHIPMANAGERID == staffId).ToList();

            }
            else if (staff.TBL_STAFF_ROLE.STAFFROLECODE == "BM")
            {
                data = data.Where(o => o.l.BRANCHID == staff.BRANCHID).ToList();
            }

            var result = (from rec in data
            group rec by new { rec.l.COMPANYID } into gg
            select new DashboardViewModel
            {
                loanCount = gg.Count(),
                sumOfProposedAmount = gg.Sum(g => (double)g.x.APPROVEDAMOUNT * g.x.EXCHANGERATE),
            }).ToList();
            return result.ToList();
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
                                select new {x,l,a}).ToList();

            if (staff.TBL_STAFF_ROLE.STAFFROLECODE == "RM")
            {
                data = data.Where(o => o.l.RELATIONSHIPMANAGERID == staffId).ToList();

            }
            else if (staff.TBL_STAFF_ROLE.STAFFROLECODE == "BM")
            {
                data = data.Where(o => o.l.BRANCHID == staff.BRANCHID).ToList();
            }

            var result = (from rec in data 
            group rec by new { rec.l.RISKRATINGID } into gg
            select new DashboardViewModel
            {
                loanCount = gg.Count(),
                riskRating = context.TBL_CUSTOMER_RISK_RATING.Where(y => y.RISKRATINGID == gg.Key.RISKRATINGID).Select(y => y.RISKRATING).FirstOrDefault(),
            }).ToList();
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
                           select new LoanCollateralMappingViewModel { amount = x.PRINCIPALAMOUNT, exchangeRate = x.EXCHANGERATE, collateralCustomerId = c.COLLATERALCUSTOMERID, note="Term Loan" , userBranchId=x.BRANCHID}).ToList();
                         
            var contingent = (from x in context.TBL_LOAN_CONTINGENT
                           join c in context.TBL_LOAN_COLLATERAL_MAPPING on x.CONTINGENTLOANID equals c.LOANID
                           where x.APPROVALSTATUSID == (int)ApprovalStatusEnum.Approved
                                  && x.COMPANYID == companyId
                                  && c.LOANSYSTEMTYPEID == (int)LoanSystemTypeEnum.ContingentLiability
                                  && x.EFFECTIVEDATE >= startDate && x.EFFECTIVEDATE <= endDate
                             select new LoanCollateralMappingViewModel { amount = x.CONTINGENTAMOUNT, exchangeRate = x.EXCHANGERATE, collateralCustomerId = c.COLLATERALCUSTOMERID, note = "Contingent", userBranchId = x.BRANCHID }).ToList();


            var overdraft = (from x in context.TBL_LOAN_REVOLVING
                             join c in context.TBL_LOAN_COLLATERAL_MAPPING on x.REVOLVINGLOANID equals c.LOANID
                             where x.APPROVALSTATUSID == (int)ApprovalStatusEnum.Approved
                             && c.ISRELEASED==false
                                    && x.COMPANYID == companyId
                                    && c.LOANSYSTEMTYPEID == (int)LoanSystemTypeEnum.OverdraftFacility
                                    && x.EFFECTIVEDATE >= startDate && x.EFFECTIVEDATE <= endDate
                            select new LoanCollateralMappingViewModel { amount = x.OVERDRAFTLIMIT, exchangeRate = x.EXCHANGERATE, collateralCustomerId = c.COLLATERALCUSTOMERID , note = "Overdraft" , userBranchId = x.BRANCHID }).ToList();

            var collaterals = termLoan.Union(contingent).Union(overdraft).ToList();

            if (staff.TBL_STAFF_ROLE.STAFFROLECODE == "RM")
            {
                collaterals = collaterals.Where(o => o.relationshipManagerId == staffId).ToList();

            }
            else if (staff.TBL_STAFF_ROLE.STAFFROLECODE == "BM")
            {
                collaterals = collaterals.Where(o => o.userBranchId == staff.BRANCHID).ToList();
            }

            var result = (from rec in collaterals
            group rec by new { rec.collateralCustomerId,rec.note} into gg
            select new DashboardViewModel
            {
                collateralCustomerId = gg.Key.collateralCustomerId,
                loanCount = gg.Count(),
                facilityAmount = gg.Sum(x => x.amount * (decimal)x.exchangeRate),
                name = gg.Key.note
            }).ToList();

            var facilityCollateralSub = (from x in result
                                        group x by new { x.collateralCustomerId , x.facilityAmount} into  aa
                                     select new DashboardViewModel
                                     {
                                         collateralCustomerId = collaterals.Where(x=>x.collateralCustomerId==aa.Key.collateralCustomerId).Select(x=>x.collateralCustomerId).FirstOrDefault(),
                                         facilityAmount = aa.Sum(a=>a.facilityAmount)
                                     }).ToList();
            var facilityCollateral = (from x in facilityCollateralSub
                                         join a in context.TBL_COLLATERAL_CUSTOMER on x.collateralCustomerId equals a.COLLATERALCUSTOMERID
                                     group new { x.facilityAmount, a.COLLATERALVALUE, a.HAIRCUT } by 1 into aa
                                     select new DashboardViewModel
                                     {
                                         facilityAmount = aa.Sum(x => x.facilityAmount),
                                         collateralValue = aa.Sum(x => (x.COLLATERALVALUE * (decimal) ((1 - x.HAIRCUT)/100.00)  ) ) 
                                     }).ToList();

            return facilityCollateral;

        }
        public List<DashboardViewModel> ApprovedLoan(DateTime startDate, DateTime endDate, int companyId, int staffId)
        {
            var staff = context.TBL_STAFF.Where(o => o.STAFFID == staffId).Select(o => o).FirstOrDefault();

            var data = (from l in context.TBL_LOAN_APPLICATION_DETAIL
                           join a in context.TBL_LOAN_APPLICATION on l.LOANAPPLICATIONID equals a.LOANAPPLICATIONID
                           where l.STATUSID == (int)ApprovalStatusEnum.Approved
                                && a.COMPANYID == companyId
                                && a.DATETIMECREATED >= startDate && a.DATETIMECREATED <= endDate
                           select new { l, a }).ToList();

            if (staff.TBL_STAFF_ROLE.STAFFROLECODE == "RM")
            {
                data = data.Where(o => o.a.RELATIONSHIPMANAGERID == staffId).ToList();

            }
            else if (staff.TBL_STAFF_ROLE.STAFFROLECODE == "BM")
            {
                data = data.Where(o => o.a.BRANCHID == staff.BRANCHID).ToList();
            }
       
            var termLaon = (from rec in data
            group rec by new { rec.a.COMPANYID } into gg
                         select new DashboardViewModel
                         {
                             loanCount = gg.Count(),
                             sumOfProposedAmount =gg.Sum(x=> (double)x.l.APPROVEDAMOUNT * x.l.EXCHANGERATE)
                         }).ToList();

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
                           ).ToList();

            var OD = (from x in context.TBL_LOAN_REVOLVING
                     where x.APPROVALSTATUSID == (int)ApprovalStatusEnum.Approved
                            && x.COMPANYID == companyId
                            && x.DATETIMECREATED >= startDate && x.DATETIMECREATED <= endDate
                     select new LoanViewModel { relationshipManagerId = x.RELATIONSHIPMANAGERID, approvedAmount = x.OVERDRAFTLIMIT, exchangeRate = x.EXCHANGERATE, branchId = x.BRANCHID, companyId = x.COMPANYID }
                     ).ToList();

          var data =  termLaon = termLaon.Union(OD).ToList();

            if (staff.TBL_STAFF_ROLE.STAFFROLECODE == "RM")
            {
                data = data.Where(o => o.relationshipManagerId == staffId).ToList();

            }
            else if (staff.TBL_STAFF_ROLE.STAFFROLECODE == "BM")
            {
                data = data.Where(o => o.branchId == staff.BRANCHID).ToList();
            }

            var result = (from rec in data
            group rec by new { rec.companyId} into gg
                     select new DashboardViewModel
                     {
                         loanCount = gg.Count(),
                         sumOfProposedAmount = gg.Sum(x => (double)x.approvedAmount * x.exchangeRate)
                     }).ToList();
            

            return result;
        }
    }
}
