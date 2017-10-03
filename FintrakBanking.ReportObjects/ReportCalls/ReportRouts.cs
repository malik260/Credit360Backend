using FintrakBanking.Common.Enum;
using FintrakBanking.Entities.Models;
using FintrakBanking.ReportObjects.InterfaceReporting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;
namespace FintrakBanking.ReportObjects.ReportCalls
{
  public  class ReportRouts : IReportRouts
    {
        string reportPath = "http://localhost:51336/Reports/";
        private IQueryable<tbl_Loan_Application> LoanApplication(int companyId)
        {
            IQueryable<tbl_Loan_Application> data;
            using (FinTrakBankingContext context = new FinTrakBankingContext())
            {
             data=   context.tbl_Loan_Application.Where(c => c.CompanyId == companyId );
            }
            return data;
        }
        public string GetWorkflowSLA(int loanApplicationId, int companyId)
        { 
            string path = string.Empty;
            int operationId = (int)OperationsEnum.CAM;
            path = reportPath + "ReportViews/ApprovalTrailWithSLA.aspx?companyId=" + companyId.ToString() + "&operationId=" + operationId + "&loanApplicationId=" + loanApplicationId.ToString();
            return path;
        }

        public string  GetLoanScheduleReport(int tearmLoanId, int companyId)
        {
            string path = string.Empty;
            path = reportPath + "ReportViews/LoanRepaymentSchedule.aspx?companyId=" + companyId.ToString() + "&tearmLoanId="+ tearmLoanId.ToString();
            return path;
        }

        public string GetSectorLimitMonitoringReport(int companyId)
        {
            string path = string.Empty;
            path = reportPath + "ReportViews/SectorialLimitMonitoring.aspx?companyId=" + companyId.ToString();
            return path;
        }

        public string GetBranchLoanAmountLimit(int branchId,int companyId)
        {
            string path = string.Empty;
            path = reportPath + "ReportViews/BranchLimitMonitoring.aspx?companyId=" + companyId.ToString() + "&branchId=" + branchId.ToString();
            return path;
        }

    }

}
