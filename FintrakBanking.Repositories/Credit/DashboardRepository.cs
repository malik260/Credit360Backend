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
        public List<DashboardViewModel> LoanApplicationsBySector(DateTime startDate, DateTime endDate, int companyId)
        {
            var result = from x in context.TBL_LOAN_APPLICATION_DETAIL
                         join l in context.TBL_LOAN_APPLICATION on x.LOANAPPLICATIONID equals l.LOANAPPLICATIONID
                         join sb in context.TBL_SUB_SECTOR on x.SUBSECTORID equals sb.SUBSECTORID
                         join s in context.TBL_SECTOR on sb.SECTORID equals s.SECTORID
                         where l.APPLICATIONSTATUSID !=  (int)LoanApplicationStatusEnum.CancellationInProgress
                                && l.APPLICATIONSTATUSID != (int)LoanApplicationStatusEnum.CancellationCompleted
                                && x.STATUSID == (int)ApprovalStatusEnum.Approved 
                                && l.COMPANYID==companyId
                                & x.DATETIMECREATED >= startDate && x.DATETIMECREATED <= endDate
                         group x by new { s.SECTORID,s.NAME } into gg
                         select new DashboardViewModel
                         {
                             loanCount = gg.Count(),
                             sumOfProposedAmount=gg.Sum(g=>g.APPROVEDAMOUNT),
                             sectorName = gg.Key.NAME
                         };
            return result.ToList();
        }

        public List<DashboardReportItem> LoanPerformance(DateTime startDate, DateTime endDate, int companyId)
        {
            int count = 0;
            var loanDetails = (from a in context.TBL_LOAN
                                              where a.APPROVALSTATUSID == (int)ApprovalStatusEnum.Approved
                                              && a.EFFECTIVEDATE >= startDate && a.EFFECTIVEDATE <= endDate && a.COMPANYID == companyId
                                              group a by new { a.USER_PRUDENTIAL_GUIDE_STATUSID} into gg
                                              select new DashboardReportItem
                                              {
                                                  id=count+1,
                                                  hoursSpent = gg.Count(),
                                                  name = context.TBL_LOAN_PRUDENT_GUIDE_TYPE.Where(x => x.PRUDENTIALGUIDELINETYPEID == gg.Key.USER_PRUDENTIAL_GUIDE_STATUSID).Select(x => x.PRUDENTIALGUIDELINETYPENAME).FirstOrDefault(),
                                              });

            return loanDetails.ToList();
        }

        public List<DashboardViewModel> LoanOnThePipeline(DateTime startDate, DateTime endDate, int companyId)
        {
            int[] applicationStatus =  { (int)LoanApplicationStatusEnum.CancellationInProgress,
                (int)LoanApplicationStatusEnum.CancellationInProgress,
                (int)LoanApplicationStatusEnum.LoanBookingInProgress,
                (int)LoanApplicationStatusEnum.LoanBookingCompleted };
            var result = from x in context.TBL_LOAN_APPLICATION_DETAIL
                         join l in context.TBL_LOAN_APPLICATION on x.LOANAPPLICATIONID equals l.LOANAPPLICATIONID
                         where !applicationStatus.Contains(l.APPLICATIONSTATUSID)
                                && x.STATUSID == (int)ApprovalStatusEnum.Approved
                                && l.COMPANYID == companyId
                                && x.DATETIMECREATED >= startDate && x.DATETIMECREATED <= endDate
                         group x by new { l.COMPANYID } into gg
                         select new DashboardViewModel
                         {
                             loanCount = gg.Count(),
                             sumOfProposedAmount = gg.Sum(g => g.APPROVEDAMOUNT),
                         };
            return result.ToList();
        }

        public List<DashboardViewModel> ExpotureByRiskRating(DateTime startDate, DateTime endDate, int companyId)
        {
          
            var result = from x in context.TBL_LOAN_APPLICATION_DETAIL
                         join l in context.TBL_LOAN_APPLICATION on x.LOANAPPLICATIONID equals l.LOANAPPLICATIONID
                         join a in context.TBL_LOAN on x.LOANAPPLICATIONDETAILID equals a.LOANAPPLICATIONDETAILID
                         where  x.STATUSID == (int)ApprovalStatusEnum.Approved
                                && l.COMPANYID == companyId
                                && x.DATETIMECREATED >= startDate && x.DATETIMECREATED <= endDate
                         group x by new { l.RISKRATINGID } into gg
                         select new DashboardViewModel
                         {
                             loanCount = gg.Count(),
                             riskRating = context.TBL_CUSTOMER_RISK_RATING.Where(y => y.RISKRATINGID == gg.Key.RISKRATINGID).Select(y => y.RISKRATING).FirstOrDefault(),
                         };
            return result.ToList();
        }
        public List<DashboardViewModel> CollateralCoverage(DateTime startDate, DateTime endDate, int companyId)
        {
            var termLoan = from x in context.TBL_LOAN
                         join c in context.TBL_LOAN_COLLATERAL_MAPPING on x.TERMLOANID equals c.LOANID
                         where x.APPROVALSTATUSID == (int)ApprovalStatusEnum.Approved
                                && x.COMPANYID == companyId
                                && c.LOANSYSTEMTYPEID == (int)LoanSystemTypeEnum.TermDisbursedFacility
                                && x.EFFECTIVEDATE >= startDate && x.EFFECTIVEDATE <= endDate
                         group x by new { c.COLLATERALCUSTOMERID } into gg
                         select new DashboardViewModel
                         {
                             collateralCustomerId = gg.Key.COLLATERALCUSTOMERID,
                             loanCount = gg.Count(),
                             facilityAmount = gg.Sum(x=>x.PRINCIPALAMOUNT),
                             name = "Term Loan"
                         };
            var contingent = from x in context.TBL_LOAN_CONTINGENT
                           join c in context.TBL_LOAN_COLLATERAL_MAPPING on x.CONTINGENTLOANID equals c.LOANID
                           where x.APPROVALSTATUSID == (int)ApprovalStatusEnum.Approved
                                  && x.COMPANYID == companyId
                                  && c.LOANSYSTEMTYPEID == (int)LoanSystemTypeEnum.ContingentLiability
                                  && x.EFFECTIVEDATE >= startDate && x.EFFECTIVEDATE <= endDate
                           group x by new { c.COLLATERALCUSTOMERID } into gg
                           select new DashboardViewModel
                           {
                               collateralCustomerId = gg.Key.COLLATERALCUSTOMERID,
                               loanCount = gg.Count(),
                               facilityAmount = gg.Sum(x => x.CONTINGENTAMOUNT),
                               name = "Contingent"
                           };
            var overdraft = from x in context.TBL_LOAN_REVOLVING
                             join c in context.TBL_LOAN_COLLATERAL_MAPPING on x.REVOLVINGLOANID equals c.LOANID
                             where x.APPROVALSTATUSID == (int)ApprovalStatusEnum.Approved
                             && c.ISRELEASED==false
                                    && x.COMPANYID == companyId
                                    && c.LOANSYSTEMTYPEID == (int)LoanSystemTypeEnum.OverdraftFacility
                                    && x.EFFECTIVEDATE >= startDate && x.EFFECTIVEDATE <= endDate
                             group x by new { c.COLLATERALCUSTOMERID } into gg
                             select new DashboardViewModel
                             {
                                 collateralCustomerId = gg.Key.COLLATERALCUSTOMERID,
                                 loanCount = gg.Count(),
                                 facilityAmount = gg.Sum(x => x.OVERDRAFTLIMIT),
                                 name = "Overdraft"
                             };

            var collaterals = termLoan.Union(contingent).Union(overdraft).ToList();

            var facilityCollateralSub = from x in collaterals
                                     group x by new { x.collateralCustomerId , x.facilityAmount} into  aa
                                     select new DashboardViewModel
                                     {
                                         collateralCustomerId = collaterals.Where(x=>x.collateralCustomerId==aa.Key.collateralCustomerId).Select(x=>x.collateralCustomerId).FirstOrDefault(),
                                         facilityAmount = aa.Sum(a=>a.facilityAmount)
                                     };
            var facilityCollateral = from x in facilityCollateralSub
                                         join a in context.TBL_COLLATERAL_CUSTOMER on x.collateralCustomerId equals a.COLLATERALCUSTOMERID
                                     group new { x.facilityAmount, a.COLLATERALVALUE, a.HAIRCUT } by 1 into aa
                                     select new DashboardViewModel
                                     {
                                         facilityAmount = aa.Sum(x => x.facilityAmount),
                                         collateralValue = aa.Sum(x => (x.COLLATERALVALUE * (decimal) ((1 - x.HAIRCUT)/100.00)  ) ) 
                                     };

            return facilityCollateral.ToList();

        }
        public List<DashboardViewModel> ApprovedLoan(DateTime startDate, DateTime endDate, int companyId)
        {
            var termLaon = from x in context.TBL_LOAN_APPLICATION_DETAIL
                         join l in context.TBL_LOAN_APPLICATION on x.LOANAPPLICATIONID equals l.LOANAPPLICATIONID
                         join a in context.TBL_LOAN on x.LOANAPPLICATIONDETAILID equals a.LOANAPPLICATIONDETAILID
                         where x.STATUSID == (int)ApprovalStatusEnum.Approved
                                && l.COMPANYID == companyId
                                && x.DATETIMECREATED >= startDate && x.DATETIMECREATED <= endDate
                         group x by new { l.RISKRATINGID } into gg
                         select new DashboardViewModel
                         {
                             loanCount = gg.Count(),
                             riskRating = context.TBL_CUSTOMER_RISK_RATING.Where(y => y.RISKRATINGID == gg.Key.RISKRATINGID).Select(y => y.RISKRATING).FirstOrDefault(),
                         };
            var OD = from x in context.TBL_LOAN_APPLICATION_DETAIL
                         join l in context.TBL_LOAN_APPLICATION on x.LOANAPPLICATIONID equals l.LOANAPPLICATIONID
                         join a in context.TBL_LOAN_REVOLVING on x.LOANAPPLICATIONDETAILID equals a.LOANAPPLICATIONDETAILID
                         where x.STATUSID == (int)ApprovalStatusEnum.Approved
                                && l.COMPANYID == companyId
                                && x.DATETIMECREATED >= startDate && x.DATETIMECREATED <= endDate
                         group x by new { l.RISKRATINGID } into gg
                         select new DashboardViewModel
                         {
                             loanCount = gg.Count(),
                             riskRating = context.TBL_CUSTOMER_RISK_RATING.Where(y => y.RISKRATINGID == gg.Key.RISKRATINGID).Select(y => y.RISKRATING).FirstOrDefault(),
                         };
            return termLaon.ToList();
        }
    }
}
