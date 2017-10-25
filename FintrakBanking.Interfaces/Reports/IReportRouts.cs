using FintrakBanking.ViewModels.Reports;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace  FintrakBanking.Interfaces.Reports
{
    public  interface IReportRoutes
    {
        string GetWorkflowSLA(int loanApplicationId, int companyId);
        string GetLoanScheduleReport(int tearmLoanId, int companyId);
        string GetSectorLimitMonitoringReport(int companyId);
        string GetBranchLoanAmountLimit(int branchId, int companyId);
        string GetWorkflowDefinition(int operationId, int companyId);
        string GetDisburstLoans(DateRange dateRange, int companyId);
        string GetLoanCommercialReport(DateRange dateRange, int companyId);
        string GetTeamAndRevolving(DateRange dateRange, int companyId);
        string GetEarnedUnearnedInterest(DateRange dateRange, int companyId);
       string GetPostedTransactions(ReportSearchEntity searchEntity, int companyId);
    }
}
