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
    }
}
