using FintrakBanking.Finance.ViewModels;
using FintrakBanking.ViewModels.Admin;
using FintrakBanking.ViewModels.Reports;
using FintrakBanking.ViewModels.Setups.Credit;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FintrakBanking.Interfaces.Reports
{
    public interface IReportRoutes
    {
        string GetOutstandingDocumentDeferredList();
        string GetAllContingentsReport(DateRange dateRange, int companyId, int staffId);
        string GetBondsAndGuaranteeReport(DateRange dateRange, int companyId, int staffId);
        string GetComputationForInternalAgentsReport(DateTime startDate, DateTime endDate);
        string GetRecoveryCollectionReport(DateTime startDate, DateTime endDate);
        string GetComputationForExternalAgentsReport(DateTime startDate, DateTime endDate);
        string GetPaydayLoanRecoveryCollectionReport(DateTime startDate, DateTime endDate);
        string GetRecoveryDelinquentAccountsReport(DateTime startDate, DateTime endDate, int dpd, decimal amount);
        string GetOutOfCourtSettlement(DateTime startDate, DateTime endDate);
        string GetCollateralSales(DateTime startDate, DateTime endDate);
        string GetRecoveryAgentUpdate(DateTime startDate, DateTime endDate);
        string GetRecoveryCommission(DateTime startDate, DateTime endDate);
        string GetRecoveryAgentPerformance(DateTime startDate, DateTime endDate);
        string GetLitigationRecoveries(DateTime startDate, DateTime endDate);
        string GetRevalidationOfFullAndFinalSettlement(DateTime startDate, DateTime endDate);
        string GetIdleAssetsSales(DateTime startDate, DateTime endDate);
        string GetFullAndFinalSettlementAndWaivers(DateTime startDate, DateTime endDate);
        int GetLoanApplicationIdByReferenceNumberLMS(string applicationRefNumber);
        string GetProductSpecificTemplateCFL(short? productClassProcessId, short? productClassId, string applicationRefNumber,
        string StatusCode, string RequestId, string WorkflowStage, string ReasonForRejection, string ActionByName);
        string DeferralWaiverReport(int staffId, int operationId, int targetId, int loanApplicationDetailId);
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
        string InsiderRelatedLoansReport(DateRange dateRange);
        string GetLoanStatusReport(DateRange dateRange, int companyId, int staffId);
        string GetEarnedUnearnedInterest(DateRange dateRange, int companyId, int staffId);
        string RiskAssets(DateTime runDate, string level, string misCode, string exposureType, string divisionName, string groupName, string branchName, string regionName);
        string CbnTeam(DateTime runDate, string level, string misCode, string exposureType, string divisionName, string groupName, string branchName, string regionName);
        string GetCollateralRevaluation(int companyId, int value);
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
        int GetLoanApplicationIdByReferenceNumber(string applicationRefNumber);
        int GetLmsrApplicationIdByReferenceNumber(string applicationRefNumber);
        //List<LoadedDocumentSectionViewModel> GetGeneratedFORM3800BLOS(string applicationRefNumber, int staffId);
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
        string GetAnalystReport(DateRange dateRange, int companyId, int staffId);
        string GetCollateralValuationReport(DateRange dateRange);
        string GetLoanClassificationReport(DateRange dateRange);

        string GetAgeAnalysisReport(DateRange dateRange);

        string GetCreditScheduleReport(DateRange dateRange);

        string GetSanctionLimitReport(DateRange dateRange);
        string GetImpairedWatchListReport(DateRange dateRange);

        string GetInsuranceReport(int companyId);
        string GetExpiredReport(DateRange dateRange);
        string GetExcessReport(DateRange dateRange);
        string GetUnutilizedFacilityReport(int companyId);

        string GetLoanDocumentDeferred(int companyId, DateRange dateRange, int staffId);
        string GetRuniningLoanReport(DateRange dateRange);

        string GetDisbursalCreditTurnover(DateRange dateRange);

        string GetLoanBookingReport(DateRange dateRange); 
        string Form3800BApprovedFacility(DateRange dateRange);

        string GetOutPutDocument(int loanApplicationId);

        string SubmissionOfOriginalDocument(DateRange dateRange);

        string PSR(int psrReportTypeId, int projectSiteReportId);

        string ContigentReport (DateTime runDate, string level, string misCode, string exposureType, string divisionName, string groupName, string branchName, string regionName);
        string ExpiredFacilityReport (DateTime runDate, string level, string misCode, string exposureType, string divisionName, string groupName, string branchName, string regionName);
        string OverLineReport (DateTime runDate, string level, string misCode, string exposureType, string divisionName, string groupName, string branchName, string regionName);
        string LargeExposureReport (DateTime runDate, string level, string misCode, string exposureType, string divisionName, string groupName, string branchName, string regionName);

        string Overline(DateTime runDate, string level, string misCode, string exposureType, string divisionName, string groupName, string branchName, string regionName);

        string ExtensionReport(DateTime runDate, string level, string misCode, string exposureType, string divisionName, string groupName, string branchName, string regionName);
        string MaturityReport(DateTime runDate, string level, string misCode, string exposureType, string divisionName, string groupName, string branchName, string regionName);
        string IfrsClassificationReport(DateTime runDate, string level, string misCode, string exposureType, string divisionName, string groupName, string branchName, string regionName);
        string RiskAssetByIFRSClassificationReport(DateTime runDate, string level, string misCode, string exposureType, string divisionName, string groupName, string branchName, string regionName);
        string RiskAssetByVarianceReport(DateTime runDate, string level, string misCode, string exposureType, string divisionName, string groupName, string branchName, string regionName);
        string RiskAssetCombinedReport(DateTime runDate, string level, string misCode, string exposureType, string divisionName, string groupName, string branchName, string regionName);
        string RiskAssetDistributionBySectorReport(DateTime runDate, string level, string misCode, string exposureType, string divisionName, string groupName, string branchName, string regionName);
        string RiskAssetMainReport(DateTime runDate, string level, string misCode, string exposureType, string divisionName, string groupName, string branchName, string regionName);
        string RiskAssetTeamReport(DateTime runDate, string level, string misCode, string exposureType, string divisionName, string groupName, string branchName, string regionName);
        string UnpaidObligationReport(DateTime runDate, string level, string misCode, string exposureType, string divisionName, string groupName, string branchName, string regionName);
        string RiskAssetContigentReportMain(DateTime runDate, string level, string misCode, string exposureType, string divisionName, string groupName, string branchName, string regionName);
        string RiskAssetMain1Report(DateTime runDate, string level, string misCode, string exposureType, string divisionName, string groupName, string branchName, string regionName);
        string DrawdownReport(string referenceNumber);
        string RiskAssetByCbnNplClassification(DateTime runDate, string level, string misCode, string exposureType, string divisionName, string groupName, string branchName, string regionName);
        string ContigentLiabilityReportMain(DateTime runDate, string level, string misCode, string exposureType, string divisionName, string groupName, string branchName, string regionName);
        string ContigentLiabilityReport(DateTime runDate, string level, string misCode, string exposureType, string divisionName, string groupName, string branchName, string regionName);
        string ContigentLiabilityReportMain1(DateTime runDate, string level, string misCode, string exposureType, string divisionName, string groupName, string branchName, string regionName);
        string CopyOfRiskAssetMain(DateTime runDate, string level, string misCode, string exposureType, string divisionName, string groupName, string branchName, string regionName);
        string RiskAssetCalcCombinedReportTeam(DateTime runDate, string level, string misCode, string exposureType, string divisionName, string groupName, string branchName, string regionName);
        string RiskAssetCalcCombinedReport(DateTime runDate, string level, string misCode, string exposureType, string divisionName, string groupName, string branchName, string regionName);
        string RiskAssetsContigentReport(DateTime runDate, string level, string misCode, string exposureType, string divisionName, string groupName, string branchName, string regionName);
        string CopyOfRiskAssetByIfrsClassification(DateTime runDate, string level, string misCode, string exposureType, string divisionName, string groupName, string branchName, string regionName);

        string GetGeneratedCFLOfferLetter(string applicationRefNumber, string ActionByName);
        string GetTrialBalanceReport(int glAccountId, int currencyCode, int companyId, int staffId);
        string GetInterestIncome(DateTime startDate, DateTime endDate);
        string GetCreditBureauReport(DateRange dateRange);
        string GetCollateralPerfection(DateRange dateRange);
        string CorporateLoansReport(DateRange dateRange);
        string GetCollateralRegister(DateRange dateRange);
        string GetCollateralAdequacy(DateRange dateRange);
        string GetFixedDepositCollaterals(int companyId, string collateralCode, int staffId);
        string GetValidCollaterals(DateTime startDate, DateTime endDate);
        string GetJobRequestReport(DateRange dateRange, int companyId, int staffId);
        string GetDisbursedFacilityCollateralReport(DateRange param);
        string SubmissionOfOriginalDocuments(DateRange param);
        string SecurityReleaseReport(DateRange param);
        string CorporateCustomerCreation(DateRange param);
        string InsuranceSpoolReport(DateRange param);

        string GetAvailmentUtilizationTicketReport(int customerId);


    }


}
