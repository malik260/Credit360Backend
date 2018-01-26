using FintrakBanking.Common;
using FintrakBanking.Common.Enum;
using FintrakBanking.Entities.Models;
using FintrakBanking.Interfaces.Reports;
using FintrakBanking.ViewModels.Reports;
using System;
using System.Linq;
namespace FintrakBanking.ReportObjects.ReportCalls
{
    public class ReportRoutes : IReportRoutes
    {
        string reportPath = CommonHelpers.ReportPath;
        private IQueryable<TBL_LOAN_APPLICATION> LoanApplication(int companyId)
        {
            IQueryable<TBL_LOAN_APPLICATION> data;
            using (FinTrakBankingContext context = new FinTrakBankingContext())
            {
                data = context.TBL_LOAN_APPLICATION.Where(c => c.COMPANYID == companyId);
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

        public string GetLoanScheduleReport(int tearmLoanId, int companyId)
        {
            string path = string.Empty;
            path = reportPath + "ReportViews/LoanRepaymentSchedule.aspx?companyId=" + companyId.ToString() + "&tearmLoanId=" + tearmLoanId.ToString();
            return path;
        }

        public string GetSectorLimitMonitoringReport(int companyId)
        {
            string path = string.Empty;
            path = reportPath + "ReportViews/SectorialLimitMonitoring.aspx?companyId=" + companyId.ToString();
            return path;
        }

        public string GetBranchLoanAmountLimit(int branchId, int companyId)
        {
            string path = string.Empty;
            path = reportPath + "ReportViews/BranchLimitMonitoring.aspx?companyId=" + companyId.ToString() + "&branchId=" + branchId.ToString();
            return path;
        }
        public string GetWorkflowDefinition(int operationId, int companyId)
        {
            string path = string.Empty;
            path = reportPath + "ReportViews/Workflow.aspx?companyId=" + companyId.ToString() + "&operationId=" + operationId.ToString();
            return path;
        }
        public string GetDisburstLoans(DateRange dateRange, int companyId)
        {
            string path = string.Empty;
            path = reportPath + "ReportViews/DisbursedLoans.aspx?companyId=" + companyId.ToString() + "&startDate=" + dateRange.startDate.ToShortDateString() + "&endDate=" + dateRange.endDate.ToShortDateString() + "&loanRefNo="+ dateRange.loanRefNo + "&branchId="+ dateRange.branchId + "&productClassId="+ dateRange.productClassId;
            return path;
        }

        public string GetLoanStatement(int companyId, int loanId)
        {
            string path = string.Empty;
            path = reportPath + "ReportViews/LoanStatement.aspx?companyId=" + companyId.ToString() + "&loanId=" + loanId.ToString();
            return path;
        }

        public string GetLoanAnniversery(DateRange dateRange, int companyId)
        {
            string path = string.Empty;
            path = reportPath + "ReportViews/LoanAnniversery.aspx?companyId=" + companyId.ToString() + "&startDate=" + dateRange.startDate.ToShortDateString() + "&endDate=" + dateRange.endDate.ToShortDateString();
            return path;
        }
        public string GetLoanDocumentWaived(int companyId, DateRange dateRange)
        {
            string path = string.Empty;
            path = reportPath + "ReportViews/LoanDocumentWaived.aspx?companyId=" + companyId+ "&startDate=" + dateRange.startDate.ToShortDateString() + "&endDate=" + dateRange.endDate.ToShortDateString() + "&branchId="+ dateRange.branchId;
            return path;
        }
        public string GetLoanDocumentDeferrals(int companyId, DateRange dateRange)
        {
            string path = string.Empty;
            path = reportPath + "ReportViews/LoanDocumentDeferral.aspx?companyId=" + companyId + "&startDate=" + dateRange.startDate.ToShortDateString() + "&endDate=" + dateRange.endDate.ToShortDateString() + "&branchId=" + dateRange.branchId;
            return path;
        }
        public string GetCollateralEstimated(int companyId, string collateralCode)
        {
            string path = string.Empty;
            path = reportPath + "ReportViews/CollateralEstimated.aspx?companyId=" + companyId.ToString() + "&collateralCode=" + collateralCode.ToString();
            return path;
        }

        public string GetFCYScheuledLoan(int companyId, int loanId)
        {
            string path = string.Empty;
            path = reportPath + "ReportViews/FCYScheuledLoan.aspx?companyId=" + companyId.ToString() + "&loanId=" + loanId.ToString();
            return path;
        }


        #region Offer Letter Generation

        public string GetGeneratedOfferLetter(string applicationRefNumber)
        {
            try
            {
                string path = string.Empty;
                path = reportPath + "Credit/TestReport.aspx?applicationRefNumber=" + applicationRefNumber;
                return path;
            }
            catch (Exception ex)
            {

                throw ex;
            }
        }

        #endregion Offer Letter Generation

        #region Loan Monitoring Reports

        public string GetCovenantsApproachingDueDateReport(int companyId)
        {
            try
            {
                string path = string.Empty;
                path = reportPath + "Credit/Monitoring/CovenantsApproachingDueDate.aspx?companyId=" + companyId.ToString();
                return path;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public string GetCollateralPropertyRevaluationReport(int companyId)
        {
            try
            {
                string path = string.Empty;
                path = reportPath + "Credit/Monitoring/CollateralPropertyRevaluation.aspx?companyId=" + companyId.ToString();
                return path;
            }
            catch (Exception ex)
            {

                throw ex;
            }
        }

        public string GetExpiredSelfLiquidatingLoansReport(int companyId)
        {
            try
            {
                string path = string.Empty;
                path = reportPath + "Credit/Monitoring/ExpiredSelfLiquidatingLoans.aspx?companyId=" + companyId.ToString();
                return path;
            }
            catch (Exception ex)
            {

                throw ex;
            }
        }

        public string GetNonPerformingLoansReport(int companyId)
        {
            try
            {
                string path = string.Empty;
                path = reportPath + "Credit/Monitoring/NonPeformingLoans.aspx?companyId=" + companyId.ToString();
                return path;
            }
            catch (Exception ex)
            {

                throw ex;
            }
        }

        public string GetExpiredOverdraftLoansReport(int companyId)
        {
            try
            {
                string path = string.Empty;
                path = reportPath + "Credit/Monitoring/ExpiredOverdraftLoans.aspx?companyId=" + companyId.ToString();
                return path;
            }
            catch (Exception ex)
            {

                throw ex;
            }
        }

        #endregion Loan Monitoring Reports

        public string GetLoanCommercialReport(DateRange dateRange, int companyId)
        {
            throw new NotImplementedException();
        }

        public string GetTeamAndRevolving(DateRange dateRange, int companyId)
        {
            throw new NotImplementedException();
        }

        public string GetEarnedUnearnedInterest(DateRange dateRange, int companyId)
        {
            throw new NotImplementedException();
        }

        public string GetPostedTransactions(ReportSearchEntity searchEntity, int companyId)
        {
            string path = string.Empty;
            path = reportPath + "ReportViews/GrantedFacilities.aspx?companyId=" + companyId.ToString() + "&startDate=" + searchEntity.startDate.ToShortDateString() + 
                "&endDate=" + searchEntity.endDate.ToShortDateString() + "&staffId=" + searchEntity.staffId + "&excludeSystem=" + searchEntity.excludeSystem + "&branchId=" + searchEntity.branchId;
            return path;
        }

        public string GetAccountWithLein(int staffId, short? branchId, string customerName, int companyId)
        {
            string path = string.Empty;
            path = reportPath + "ReportViews/LoanCASAaccountWithLein.aspx?companyId=" + companyId.ToString() + "&branchId=" + branchId + "&customerName=" + customerName
                + "&staffId=" + staffId;
            return path;
        }

        public string GetStakeholdersOnExpirationOfFTP(ReportSearchEntity searchEntity, int companyId)
        {
            string path = string.Empty;
            path = reportPath + "ReportViews/GetStakeholdersOnExpirationOfFTP.aspx?companyId=" + companyId.ToString() + "&branchId=" + searchEntity.branchId + "&customerName=" + searchEntity.customerName + "&startDate="+ searchEntity.startDate;
            return path;
        }
    }

}
