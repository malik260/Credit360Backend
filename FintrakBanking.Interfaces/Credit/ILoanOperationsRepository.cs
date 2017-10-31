using FintrakBanking.Common.Enum;
using FintrakBanking.ViewModels;
using FintrakBanking.ViewModels.Credit;
using FintrakBanking.ViewModels.Customer;
using FintrakBanking.ViewModels.Finance;
using FintrakBanking.ViewModels.WorkFlow;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
//using FintrakBanking.ViewModels.Operations;

namespace FintrakBanking.Interfaces.Credit
{
    public interface ILoanOperationsRepository
    {
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
        // IEnumerable<LoanOperationTypeViewModel> GetOperationTypeByLoanId(int scheduleId);
        IEnumerable<LoanOperationTypeViewModel> GetOperationTypeByLoanId(LoanProductTypeEnum productTypeId, LoanScheduleTypeEnum scheduleTypeId);
        IEnumerable<LoanReviewOperationApprovalViewModel> GetLoanOperationAwaitingApproval(int staffId, int companyId);
        IEnumerable<LoanReviewOperationApprovalViewModel> GetApprovedLoanOperationReview();
        IEnumerable<ApprovalTrailDetailsViewModel> GetApprovalDetails(int loanId, int OperationId);
       // IEnumerable<LoanReviewOperationViewModel> GetLoanOperationAwaitingApproval(int staffId, int companyId);
        bool LoanCancellation(int loanId, DateTime applicationDate, int staffId);
        void OverdraftTopUp(int loanId, decimal amount);
        IEnumerable<LimitSuspensionViewModel> ProcessNPLByBranchSuspension();
        IEnumerable<LoanRepaymentViewModel> ProcessLoanRepaymentPostingForceDebit(DateTime applicationDate);
        IEnumerable<LoanRepaymentViewModel> ProcessLoanRepaymentPostingPastDue (DateTime applicationDate);
        IEnumerable<DailyInterestAccrualViewModel> InterestSuspension(int loanId, DateTime applicationDate, int staffId);
        IEnumerable<LoanViewModel> ArchiveLoan(int loanId, int operationId);
        IEnumerable<LoanViewModel> BulkArchiveLoan();
        IEnumerable<LoanPaymentSchedulePeriodicViewModel> ArchivePeriodicSchedule(int loanId);
        IEnumerable<LoanPaymentScheduleDailyViewModel> ArchiveDailySchedule(int loanId);
        IEnumerable<LoanPaymentSchedulePeriodicViewModel> MergePeriodicSchedule (int loanId, DateTime applicationDate);
        bool LoanRephasementProcess(short loanReviewOperationsId, int loanId, int staffId);
        IEnumerable<LoanRepaymentViewModel> ProcessLoanRepaymentPostingPastDueForInterestReview(DateTime applicationDate, int loanId);
        IEnumerable<LoanRepaymentViewModel> ProcessLoanRepaymentPostingPastDueForBulkInterestReview(DateTime applicationDate);
        IEnumerable<LoanRepaymentViewModel> ProcessAuthorisedOverdraftRepaymentPostingForceDebit(DateTime applicationDate);
        bool BulkRateReview(short priceindexId, double newRate, DateTime applicationDate, int staffId);
        IEnumerable<LoanViewModel> GetLoanRateCustomerExcemptions(int companyId);
        bool addBulkRateLoanExcemptions(LoanViewModel model);
        bool addInterestRateChange(LoanBulkInterestReviewViewModel model);
        IEnumerable<LoanBulkInterestReviewViewModel> GetNewInterestRateReviews(int companyId);




    }
}