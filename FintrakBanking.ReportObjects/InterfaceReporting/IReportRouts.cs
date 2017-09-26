using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FintrakBanking.ReportObjects.InterfaceReporting
{
    public  interface IReportRouts
    {
        string GetWorkflowSLA(int loanApplicationId, int companyId);
        string GetLoanScheduleReport(int tearmLoanId, int companyId);
        string GetSectorLimitMonitoringReport(int companyId);
    }
}
