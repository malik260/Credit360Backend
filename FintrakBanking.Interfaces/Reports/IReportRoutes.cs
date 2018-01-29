using FintrakBanking.ViewModels.Reports;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FintrakBanking.Interfaces.Reports
{
    public interface IReportRoutes
    {
        string GetWorkflowSLA(int loanApplicationId, int companyId);
        string GetLoanScheduleReport(int tearmLoanId, int companyId);
        string GetSectorLimitMonitoringReport(int companyId);
        string GetBranchLoanAmountLimit(int branchId, int companyId);
        string GetWorkflowDefinition(int operationId, int companyId);
        string GetDisburstLoans(DateRange dateRange, int companyId);
        string GetLoanStatement(int companyId, int loanId);
        string GetLoanAnniversery(DateRange dateRange, int companyId);
        string GetLoanDocumentWaived(int companyId, DateRange dateRange);
        string GetLoanDocumentDeferrals(int companyId, DateRange dateRange);
        string GetLoanDocumentDeferralsMCC(int companyId, DateRange dateRange);
        string GetCollateralEstimated(int companyId, string collateralCode);
        string GetFCYScheuledLoan(int companyId, int loanId);
        string GetRuningLoansByLoanType(ReportSearchEntity searchEntity, int companyId);
        string GetLoanInterestReceivableAndPayable(ReportSearchEntity searchEntity, int companyId);
        #region Offer Letter Generation & Loan Monitoring Reports

        string GetGeneratedOfferLetter(string applicationRefNumber);

        string GetCovenantsApproachingDueDateReport(int companyId);
        string GetCollateralPropertyRevaluationReport(int companyId,int value);
        string GetExpiredSelfLiquidatingLoansReport(int companyId);
        string GetNonPerformingLoansReport(int companyId);
        string GetExpiredOverdraftLoansReport(int companyId);

        #endregion Offer Letter Generation & Loan Monitoring Reports
        string GetLoanCommercialReport(DateRange dateRange, int companyId);
        string GetTeamAndRevolving(DateRange dateRange, int companyId);
        string GetEarnedUnearnedInterest(DateRange dateRange, int companyId);
        string GetPostedTransactions(ReportSearchEntity searchEntity, int companyId);
        string GetAccountWithLein(int staffId, short? branchId, string customerName, int companyId);
        string GetStakeholdersOnExpirationOfFTP(ReportSearchEntity searchEntity, int companyId);
        string GetAuditTrail(DateRange dateRange, int companyId);
        string GetFacilityApprovedNotUtilized(ReportSearchEntity searchEntity, int companyId);

    }
}
