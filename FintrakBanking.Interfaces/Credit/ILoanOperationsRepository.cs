using FintrakBanking.Common.Enum;
using FintrakBanking.ViewModels;
using FintrakBanking.ViewModels.CASA;
using FintrakBanking.ViewModels.Credit;
using FintrakBanking.ViewModels.Finance;
using FintrakBanking.ViewModels.ThridPartyIntegration;
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
        int GoForApproval(ApprovalViewModel entity);
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
        bool OverdraftTopUp(TwoFactorAutheticationViewModel twoFactorAuth, int loanId, decimal amount);
        //bool OverdraftTopUp(int loanId, decimal amount);
        IEnumerable<LimitSuspensionViewModel> ProcessNPLByBranchSuspension();
        IEnumerable<LoanRepaymentViewModel> ProcessLoanRepaymentPostingForceDebit(DateTime applicationDate);
        IEnumerable<LoanRepaymentViewModel> ProcessLoanRepaymentPostingPastDue (DateTime applicationDate);
        IEnumerable<DailyInterestAccrualViewModel> InterestSuspension(int loanId, DateTime applicationDate, int staffId);
        LoanViewModel ArchiveLoan(int loanId, int operationId, string archiveBatchCode);
        IEnumerable<LoanViewModel> BulkArchiveLoan();
        IEnumerable<LoanPaymentSchedulePeriodicViewModel> ArchivePeriodicSchedule(int loanId, string archiveBatchCode);
        IEnumerable<LoanPaymentScheduleDailyViewModel> ArchiveDailySchedule(int loanId, string archiveBatchCode);
        IEnumerable<LoanPaymentSchedulePeriodicViewModel> MergePeriodicSchedule (int loanId, DateTime applicationDate);
        bool LoanRephasementProcess(TwoFactorAutheticationViewModel twoFactorAuth, short loanReviewOperationsId, int loanId, int staffId);
        IEnumerable<LoanRepaymentViewModel> ProcessLoanRepaymentPostingPastDueForInterestReview(DateTime applicationDate, int loanId);
        IEnumerable<LoanRepaymentViewModel> ProcessLoanRepaymentPostingPastDueForBulkInterestReview(DateTime applicationDate);
        IEnumerable<LoanRepaymentViewModel> ProcessAuthorisedOverdraftRepaymentPostingForceDebit(DateTime applicationDate);
        bool BulkRateReview(short priceindexId, double newRate, DateTime applicationDate, int staffId, int operationId);
        IEnumerable<LoanViewModel> GetLoanRateCustomerExcemptions(int companyId);
        bool addBulkRateLoanExcemptions(LoanViewModel model);
        bool addInterestRateChange(LoanBulkInterestReviewViewModel model);
        IEnumerable<LoanBulkInterestReviewViewModel> GetNewInterestRateReviews(int companyId); 
         IEnumerable<LoanClassificationViewModel> CalculateLoanClassification(DateTime applicationDate);
        LoanViewModel GetRunningLoans(int companyId, string refNo);
        LoanViewModel GetRunningFXLoans(int companyId, string refNo);
        IEnumerable<LoanOperationTypeViewModel> GetOperationTypeByOD();
        IEnumerable<LoanOperationTypeViewModel> GetRemedialOperationType();
        IEnumerable<LoanFeeOperationViewModel> GetLoanChargeFeeByLoanId(int loanId);
       
        IEnumerable<LoanClassificationViewModel> CalculateOverdraftClassification(DateTime applicationDate);
        IEnumerable<LoanViewModel> LoanHistory();
        IEnumerable<RevolvingLoanViewModel> OverDraftHistory();

        #region COMMERCIAL PAPER LOANS
        bool CommercialPaperSubAllocation(List<subAllocationViewModel> models);
        IEnumerable<MaturityIntructionViewModel> GetMaturityInstructionType();
        bool addMaturityInstruction(MaturityIntructionViewModel model);
        IEnumerable<MaturityIntructionViewModel> GetLoanMaturityInstructions();
        bool ProcessCommercialPaperManualRollOver(MaturityIntructionViewModel model, string refNo);
        //void CommercialPaperManualRollOver(DateTime applicationDate);
        bool addCommercialPaperTenorReview(TenorExtionViewModel userModel);
        List<LoanReviewOperationParentChildViewModel> GetRunningCommercialLoanLines(int companyId);
        bool CommercialPaperRateReview(InterestReviewViewModel userModel);
        List<LoanReviewOperationParentChildViewModel> GetCommercialLoansLines(int companyId);
        List<LoanReviewOperationApprovalViewModel> GetDueCommercialLoans(int companyId);
        List<LoanReviewOperationApprovalViewModel> GetDueCommercialLoansByApplicationDetailId(int companyId, int loanApplicationDetailID);
        //IEnumerable<DailyInterestAccrualViewModel> ProcessDailyCommercialPaperInterestAccrual(DateTime applicationDate);
        void CommercialPaperChangeOperativeAccount(int casaPayAccountId, int newCasaPayAccountId);
        bool CommercialPaperDetailsCancellation(string refNo, DateTime applicationDate, int staffId);
        loanPrepaymentViewModel addCommercialLoanPrepayment(string refNo, loanPrepaymentViewModel model);
        IEnumerable<LoanReviewOperationApprovalViewModel> GetRunningCommercialLoans(int companyId, string loanReferenceNumber);
        bool addCommercialPaperLineTenorReview(int loanAplicationDetailId, int newTenor);
        bool GetRepaymentFromStaging();
        #endregion

        #region
        List<ItemValue> FlowTypes();
        #endregion
    }
}