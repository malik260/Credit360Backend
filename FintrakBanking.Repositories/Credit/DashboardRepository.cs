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
        public List<DashboardViewModel> LoanApplicationsBySector(DateTime startDate, DateTime endDate)
        {
            var result = from x in context.TBL_LOAN_APPLICATION_DETAIL
                         join l in context.TBL_LOAN_APPLICATION on x.LOANAPPLICATIONID equals l.LOANAPPLICATIONID
                         join sb in context.TBL_SUB_SECTOR on x.SUBSECTORID equals sb.SUBSECTORID
                         join s in context.TBL_SECTOR on sb.SECTORID equals s.SECTORID
                         where l.APPLICATIONSTATUSID !=  (int)LoanApplicationStatusEnum.CancellationInProgress
                                && l.APPLICATIONSTATUSID != (int)LoanApplicationStatusEnum.CancellationCompleted
                                && x.STATUSID == (int)ApprovalStatusEnum.Approved
                         group x by new { s.SECTORID,s.NAME } into gg
                         select new DashboardViewModel
                         {
                             loanCount = gg.Count(),
                             sumOfProposedAmount=gg.Sum(g=>g.APPROVEDAMOUNT),
                             sectorName = gg.Key.NAME
                         };
            return result.ToList();
        }
    }
}
