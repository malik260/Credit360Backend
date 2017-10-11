using FintrakBanking.Common.Enum;
using FintrakBanking.ViewModels;
using FintrakBanking.ViewModels.Credit;
using FintrakBanking.ViewModels.Customer;
using FintrakBanking.ViewModels.Finance;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
//using FintrakBanking.ViewModels.Operations;

namespace FintrakBanking.Interfaces.Credit
{
    public interface ILoanOperationsRepository
    {
        bool AddCollateralSearchLien(CasaLienViewModel model);
        decimal GetCollateralSearchChargeAmount(int stateId);
        bool AddOperationReview(LoanReviewOperationViewModel model);
        IEnumerable<LoanOperationTypeViewModel> GetOperationType();

        IEnumerable<DailyInterestAccrualViewModel> GetDailyTeamLoansInterestAccrual(DateTime applicationDate);
        IEnumerable<DailyInterestAccrualViewModel> GetDailyAuthorisedOverdraftInterestAccrual(DateTime applicationDate);
        IEnumerable<DailyInterestAccrualViewModel> GetDailyUnauthorisedOverdraftInterestAccrual(DateTime applicationDate);
        IEnumerable<DailyInterestAccrualViewModel> GetDailyPastDueInterestAccrual(DateTime applicationDate);
        IEnumerable<DailyInterestAccrualViewModel> GetDailyPastDuePrincipalAccrual(DateTime applicationDate);
        // IEnumerable<LoanOperationTypeViewModel> GetOperationTypeByLoanId(int scheduleId);
        IEnumerable<LoanOperationTypeViewModel> GetOperationTypeByLoanId(LoanProductTypeEnum productTypeId, LoanScheduleTypeEnum scheduleTypeId);
        IEnumerable<LoanReviewOperationViewModel> GetLoanOperationAwaitingApproval(int staffId, int companyId);
        bool LoanCancellation(int loanId, DateTime applicationDate, int staffId);
        void OverdraftTopUp(int loanId, decimal amount);
        IEnumerable<LimitSuspensionViewModel> NPLByBranchSuspension();
        IEnumerable<LoanRepaymentViewModel> BuildLoanRepaymentPostingForceDebit(DateTime applicationDate);
        IEnumerable<LoanRepaymentViewModel> BuildLoanRepaymentPostingPastDue(DateTime applicationDate);
        IEnumerable<DailyInterestAccrualViewModel> InterestSuspension(int loanId, DateTime applicationDate, int staffId);
    }
}