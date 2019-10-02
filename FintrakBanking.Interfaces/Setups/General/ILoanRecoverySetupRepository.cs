using FintrakBanking.ViewModels.Credit;
using FintrakBanking.ViewModels.Setups.General;
using FintrakBanking.ViewModels.WorkFlow;
using System.Collections.Generic;
using System.Linq;

namespace FintrakBanking.Interfaces.Setups.General
{
    public interface ILoanRecoverySetupRepository
    {
        IEnumerable<LoanRecoverySetupViewModel> GetAllLoanRecoverySetup();

        bool AddLoanRecoverySetup(LoanRecoverySetupViewModel entity);

        bool UpdateLoanRecoverySetup(int LoanRecoverySetupId, LoanRecoverySetupViewModel entity);

        LoanRecoverySetupViewModel GetLoanRecoverySetup(int recoveryPlanId);

        IEnumerable<LoanRecoverySetupViewModel> GetAllCasa();

        IEnumerable<LoanRecoverySetupViewModel> GetAllAgent();

        IEnumerable<LoanRecoverySetupViewModel> GetAllProductType();

        bool AddLoanRecoveryPaymentPlan(LoanRecoverySetupViewModel entity);

        LoanRecoverySetupViewModel GetLoanRecoveryPaymentPlan(int recoveryPaymentPlanId);

        bool UpdateLoanRecoveryPaymentPlan(int recoveryPaymentPlanId, LoanRecoverySetupViewModel entity);

        IEnumerable<LoanRecoverySetupViewModel> GetDistinctLoanRecoveryPaymentPlan();

        IEnumerable<LoanRecoverySetupViewModel> GetAllLoanRecoveryPaymentPlan();
        List<LoanRecoveryPaymentViewModel> LoanRecoveryPaymentWaitingForApproval(int staffId, int companyId);
        bool RecoveryPaymentGoForApproval(LoanRecoveryPaymentViewModel entity);
        IEnumerable<LoanRecoveryPaymentViewModel> GetLoanRecoveryPayment(string searchQuery);
        IEnumerable<LoanRecoveryPaymentViewModel> GetRecoveryPaymentSchedule(int loanReviewOperationId);
        LoanRecoveryPaymentViewModel GetTotalRecoveryPayments(int loanReviewOperationId);
        bool AddLaonRecoveryPayment(LoanRecoveryPaymentViewModel model);
    }
}