using FintrakBanking.Common.Enum;
using FintrakBanking.ViewModels.CASA;
using FintrakBanking.ViewModels.Credit;
using FintrakBanking.ViewModels.WorkFlow;
using System;
using System.Collections.Generic;
//using FintrakBanking.ViewModels.Operations;

namespace FintrakBanking.Interfaces.Credit
{
    public interface ILoanOperationsRepository
    {
        bool DoesChargeFeeExist(int loanId, int operationTypeId, int chargeFeeId);
        bool DoesOperationExist(int loanId, int operationTypeId);
        bool GoForApproval(ApprovalViewModel entity);
        bool AddCollateralSearchLien(CasaLienViewModel model);
        decimal GetCollateralSearchChargeAmount(int stateId);
        bool AddOperationReview(LoanReviewOperationViewModel model);
        IEnumerable<LoanOperationTypeViewModel> GetOperationType();

        IEnumerable<DailyInterestAccrualViewModel> ProcessDailyTeamLoansInterestAccrual(DateTime applicationDate);
        IEnumerable<DailyInterestAccrualViewModel> ProcessDailyAuthorisedOverdraftInterestAccrual(DateTime applicationDate);
        IEnumerable<DailyInterestAccrualViewModel> ProcessDailyUnauthorisedOverdraftInterestAccrual(DateTime applicationDate);
        IEnumerable<DailyInterestAccrualViewModel> ProcessDailyPastDueInterestAccrual(DateTime applicationDate);
        IEnumerable<DailyInterestAccrualViewModel> ProcessDailyPastDuePrincipalAccrual(DateTime applicationDate);
        IEnumerable<LoanPastDueViewModel> ProcessUnauthorisedOverdraftInterestRepaymentPostingPastDue(DateTime applicationDate);
        IEnumerable<LoanOperationTypeViewModel> GetOperationTypeByLoanId(LoanProductTypeEnum productTypeId, LoanScheduleTypeEnum scheduleTypeId);
        IEnumerable<LoanReviewOperationApprovalViewModel> GetLoanOperationAwaitingApproval(int staffId, int companyId);
        IEnumerable<LoanReviewOperationApprovalViewModel> GetApprovedLoanOperationReview();
        IEnumerable<ApprovalTrailDetailsViewModel> GetApprovalDetails(int loanId, int OperationId);
        IEnumerable<LoanCovenantDetailViewModel> ProcessIDFExpiryAndlocking(DateTime applicationDate);
        IEnumerable<LoanCovenantDetailViewModel> ProcessOverdraftBalanceSuspensionBaseOnCovenant(DateTime applicationDate);
        IEnumerable<LoanCovenantDetailViewModel> ProcessOverdraftBalanceSuspensionBaseOnCleanUp(DateTime applicationDate);
        IEnumerable<LoanViewModel> ProcessIntervalFeeandCommissionPosting(DateTime applicationDate);
        IEnumerable<LoanCovenantDetailViewModel> ProcessCFFExpiryAndlocking(DateTime applicationDate);
        IEnumerable<LoanCovenantDetailViewModel> ProcessLPOExpiryAndlocking(DateTime applicationDate);
        IEnumerable<LoanPastDueViewModel> ProcessUnauthorisedOverdraftPrincipalRepaymentPostingPastDue(DateTime applicationDate);
        bool LoanCancellation(int loanId, DateTime applicationDate, int staffId);
        void OverdraftTopUp(int loanId, decimal amount);
        IEnumerable<LimitSuspensionViewModel> ProcessNPLByBranchSuspension();
        IEnumerable<LoanRepaymentViewModel> ProcessLoanRepaymentPostingForceDebit(DateTime applicationDate);
        IEnumerable<LoanRepaymentViewModel> ProcessLoanRepaymentPostingPastDue (DateTime applicationDate);
        IEnumerable<DailyInterestAccrualViewModel> InterestSuspension(int loanId, DateTime applicationDate, int staffId);
        LoanViewModel ArchiveLoan(int loanId, int operationId);
        IEnumerable<LoanViewModel> BulkArchiveLoan();
        IEnumerable<LoanPaymentSchedulePeriodicViewModel> ArchivePeriodicSchedule(int loanId);
        IEnumerable<LoanPaymentScheduleDailyViewModel> ArchiveDailySchedule(int loanId);
        IEnumerable<LoanPaymentSchedulePeriodicViewModel> MergePeriodicSchedule (int loanId, DateTime applicationDate);
        bool LoanRephasementProcess(short loanReviewOperationsId, int loanId, int staffId);
        IEnumerable<LoanRepaymentViewModel> ProcessLoanRepaymentPostingPastDueForInterestReview(DateTime applicationDate, int loanId);
        IEnumerable<LoanRepaymentViewModel> ProcessLoanRepaymentPostingPastDueForBulkInterestReview(DateTime applicationDate);
        IEnumerable<LoanRepaymentViewModel> ProcessAuthorisedOverdraftRepaymentPostingForceDebit(DateTime applicationDate);
        bool BulkRateReview(short priceindexId, double newRate, DateTime applicationDate, int staffId, int operationId);
        IEnumerable<LoanViewModel> GetLoanRateCustomerExcemptions(int companyId);
        bool addBulkRateLoanExcemptions(LoanViewModel model);
        bool addInterestRateChange(LoanBulkInterestReviewViewModel model);
        IEnumerable<LoanBulkInterestReviewViewModel> GetNewInterestRateReviews(int companyId);
        IEnumerable<LoanClassificationViewModel> CalLoanClassification(DateTime applicationDate);
        IEnumerable<DailyInterestAccrualViewModel> ProcessDailyCommercialPaperInterestAccrual(DateTime applicationDate);
        // IEnumerable<LoanViewModel> GetRunningLoans(int companyId, string refNo);
        LoanViewModel GetRunningLoans(int companyId, string refNo);
        IEnumerable<LoanOperationTypeViewModel> GetOperationTypeByOD();
        IEnumerable<LoanOperationTypeViewModel> GetRemedialOperationType();
        List<LoanReviewOperationParentChildViewModel> GetMaturedCommercialLoansParent(int companyId);
        IEnumerable<LoanReviewOperationApprovalViewModel> GetRunningCommercialLoans(int companyId, string loanReferenceNumber);
        List<LoanReviewOperationApprovalViewModel> GetMaturedCommercialLoans(int companyId, int loanApplicationDetailID);
        IEnumerable<LoanFeeOperationViewModel> GetLoanChargeFeeByLoanId(int loanId);
        IEnumerable<MaturityIntructionTypeViewModel> GetMaturityInstructionType();
    }
}