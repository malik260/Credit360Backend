using FintrakBanking.Finance.ViewModels;
using FintrakBanking.ViewModels.Admin;
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
        string GetRunningFacilities(DateRange dateRange, int companyId, int staffId);
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

        string GetCovenantsApproachingDueDateReport(int staffId, DateRange dateRange,int companyId);
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
        IEnumerable<AuditViewModel> AuditType(string searchValue);
        List<GLAccountSearchViewModel> GLAccount(string searchValue);
        string GetGeneratedFORM3800BLOS(string applicationRefNumber);
        string GetGeneratedFORM3800BLMS(string applicationRefNumber);
        string GetStalledPerfection(DateRange dateRange);
        string GetCollateralPerfectionYetToCommence(DateRange dateRange);
        string GetAllCommercialLoanReport(DateRange dateRange);
        string GetUnearnedLoanInterestReport(DateRange dateRange);
        string GetReceivableInterestReport(DateRange dateRange);
        string GetCashBackedReport(DateRange dateRange);
        string GetCashBackedBondAndGuarantee(DateRange dateRange);
        string GetweeklyRecoveryReportforFINCON(DateRange dateRange);
        string GetCashCollaterizedCredits(DateRange dateRange);
        string GetStaffPrivilegeChangeReport(DateRange dateRange);
        string GetUserGroupChangeReport(DateRange dateRange);
        string GetProfileActivityReport(DateRange dateRange);
        string GetStaffRoleProfileGroupReport(DateRange dateRange);
        string GetStaffRoleProfileActivityReport(DateRange dateRange);
        string GetInActiveContigentLiabilityReport(DateRange dateRange);
        string GetLoggingStatus(DateRange dateRange);
        string GetMiddleOfficeReport(DateRange dateRange);
        string GetCollateralValuationReport(DateRange dateRange);
        string GetLoanClassificationReport(DateRange dateRange);

        string GetAgeAnalysisReport(DateRange dateRange);

        string GetCreditScheduleReport(DateRange dateRange);

        string GetSanctionLimitReport(DateRange dateRange);
        string GetImpairedWatchListReport(DateRange dateRange);

        string GetInsuranceReport(DateRange dateRange);
        string GetExpiredReport(DateRange dateRange);
        string GetExcessReport(DateRange dateRange);

        string GetLoanDocumentDeferred(int companyId, DateRange dateRange, int staffId);

        string GetLoanBookingReport(DateRange dateRange);
    }
}
