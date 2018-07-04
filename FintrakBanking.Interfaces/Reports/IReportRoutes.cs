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
        string GetWorkflowSLA(int loanApplicationId, int companyId, int staffId);
        string GetLoanScheduleReport(int tearmLoanId, int companyId, int staffId);
        string GetSectorLimitMonitoringReport(int companyId, int staffId);
        string GetBranchLoanAmountLimit(int branchId, int companyId, int staffId);
        string GetWorkflowDefinition(int operationId, int companyId, int staffId);
        string GetDisburstLoans(DateRange dateRange, int companyId, int staffId);
        string GetLoanStatement(int companyId, int loanId, int staffId);
        string GetLoanAnniversery(DateRange dateRange, int companyId, int staffId);
        string GetLoanDocumentWaived(int companyId, DateRange dateRange, int staffId);
        string GetLoanDocumentDeferrals(int companyId, DateRange dateRange, int staffId);
        string GetLoanDocumentDeferralsMCC(int companyId, DateRange dateRange, int staffId);
        string GetCollateralEstimated(int companyId, string collateralCode, int staffId);
        string GetFCYScheuledLoan(int companyId, int loanId, int staffId);
        string GetRuningLoansByLoanType(ReportSearchEntity searchEntity, int companyId, int staffId);
        string GetLoanInterestReceivableAndPayable(ReportSearchEntity searchEntity, int companyId, int staffId);
        #region Offer Letter Generation & Loan Monitoring Reports

        string GetGeneratedOfferLetter(string applicationRefNumber);

        string GetCovenantsApproachingDueDateReport(int companyId, int staffId, DateRange dateRange);
        string GetCollateralPropertyRevaluationReport(int companyId,DateRange dateRange, int staffId);
        string GetSelfLiquidatingLoansReport(DateRange dateRange,int companyId, int staffId);
        string GetNonPerformingLoansReport(DateRange dateRange, int companyId, int staffId);
        string GetExpiredOverdraftLoansReport(DateRange dateRange, int companyId, int staffId);

        #endregion Offer Letter Generation & Loan Monitoring Reports
        string GetLoanCommercialReport(DateRange dateRange, int companyId, int staffId);
        string GetTeamAndRevolving(DateRange dateRange, int companyId, int staffId);
        string GetEarnedUnearnedInterest(DateRange dateRange, int companyId, int staffId);
        string GetPostedTransactions(ReportSearchEntity searchEntity, int companyId);
        string GetStakeholdersOnExpirationOfFTP(ReportSearchEntity searchEntity, int companyId, int staffId);
        string GetAuditTrail(DateRange dateRange, int companyId, int staffId);
        string GetFacilityApprovedNotUtilized(ReportSearchEntity searchEntity, int companyId, int staffId);
        string GetCollateralPropertyDueForVisitationReport(int companyId, DateRange dateRange, int staffId);
        string GetBondAndGuaranteeReport(DateRange dateRange, int companyId, int staffId);
        string GetGeneratedOfferLetterLMS(string refNumber);
        string GetCollateralInsuranceReport(DateRange dateRange, int companyId, int staffId);
        string GetTurnoverCovenantReport(DateRange dateRange, int companyId, int staffId);
        string GetWorkflowSLAMonitoring(int companyId, DateRange dateRange);
        string GetBlacklist(ReportSearchEntity searchEntity);
        string GetDailyAccrual(ReportSearchEntity searchEntity);
        string GetRepayment(ReportSearchEntity searchEntity);
        string GetCustomeFacilityRepayment(ReportSearchEntity searchEntity);
        string AccountWithLein(ReportSearchEntity searchEntity);
    }
}
