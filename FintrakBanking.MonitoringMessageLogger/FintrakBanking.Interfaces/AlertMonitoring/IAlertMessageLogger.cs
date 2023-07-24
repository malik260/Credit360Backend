using FintrakBanking.Entities.Models;
using FintrakBanking.ViewModels.Credit;
using FintrakBanking.ViewModels.Setups.General;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FintrakBanking.Interfaces.AlertMonitoring
{
    public interface IAlertMessageLogger
    {
        bool SendAlertsForCovenantsApproachingDueDate(string title, string messageBody, List<TBL_MONITORING_ALERT_SETUP> alertSetups);
        void SendAlertsForCovenantsApproachingDueDateToRM(List<LoanCovenantDetailViewModel> loanDetails, string title);
        void SendAlertsForCovenantsApproachingDueDateToMonitoringTeam(List<LoanCovenantDetailViewModel> loanDetails, TBL_MONITORING_ALERT_SETUP alertSetups);

        bool SendAlertsForExpiredBG(string title, string messageBody, List<TBL_MONITORING_ALERT_SETUP> alertSetups);

        bool SendAlertsForCovenantsOverDue(string title, string messageBody, List<TBL_MONITORING_ALERT_SETUP> alertSetups);
        void SendAlertsForCovenantsOverDueRM(List<LoanCovenantDetailViewModel> loanDetails, string title);
        void SendAlertsForCovenantsOverDueMonitoringTeam(List<LoanCovenantDetailViewModel> loanDetails, TBL_MONITORING_ALERT_SETUP alertSetups);


        bool SendAlertsForCollateralPropertyApproachingRevaluation(string title, string messageBody, List<TBL_MONITORING_ALERT_SETUP> alertSetups);
        void SendAlertsForCollateralPropertyApproachingRevaluationRM(List<CollateralViewModel> loanDetails, string title);
        void SendAlertsForCollateralPropertyApproachingRevaluationMonitoringTeam(List<CollateralViewModel> loanDetails, TBL_MONITORING_ALERT_SETUP alertSetups);


        bool SendAlertsForCollateralPropertyDueForRevaluation(string title, string messageBody, List<TBL_MONITORING_ALERT_SETUP> alertSetups);
        void SendAlertsForCollateralPropertyDueForRevaluationRM(List<CollateralViewModel> loanDetails, string title);
        void SendAlertsForCollateralPropertyDueForRevaluationMonitoringTeam(List<CollateralViewModel> loanDetails, TBL_MONITORING_ALERT_SETUP alertSetups);


        bool SendAlertsForLoanNplMonitoring(string title, string messageBody, List<TBL_MONITORING_ALERT_SETUP> alertSetups);
        void SendAlertsForLoanNplMonitoringRM(List<LoanViewModel> loanDetails, string title);
        void SendAlertsForLoanNplMonitoringMonitoringTeam(List<LoanViewModel> loanDetails, TBL_MONITORING_ALERT_SETUP alertSetups);


        bool SendAlertsOnSelfLiquidatingLoanExpiry(string title, string messageBody, List<TBL_MONITORING_ALERT_SETUP> alertSetups);
        void SendAlertsOnSelfLiquidatingLoanExpiryRM(List<LoanViewModel> loanDetails, string title);
        void SendAlertsOnSelfLiquidatingLoanExpiryMonitoringTeam(List<LoanViewModel> loanDetails, TBL_MONITORING_ALERT_SETUP alertSetups);


        bool SendAlertsOnOverDraftLoansAlmostDue(string title, string messageBody, List<TBL_MONITORING_ALERT_SETUP> alertSetups);
        void SendAlertsOnOverDraftLoansAlmostDueRM(List<LoanViewModel> loanDetails, string title);
        void SendAlertsOnOverDraftLoansAlmostDueMonitoringTeam(List<LoanViewModel> loanDetails, TBL_MONITORING_ALERT_SETUP alertSetups);


        bool SendAlertsOnLoanCASAwithPND(string title, string messageBody, List<TBL_MONITORING_ALERT_SETUP> alertSetups);
        void SendAlertsOnLoanCASAwithPNDrm(List<LoanViewModel> loanDetails, string title);
        void SendAlertsOnLoanCASAwithPNDmonitoringTeam(List<LoanViewModel> loanDetails, TBL_MONITORING_ALERT_SETUP alertSetups);


        bool SendAlertsOnInActiveBondAndGuarantee(string title, string messageBody, List<TBL_MONITORING_ALERT_SETUP> alertSetups);
        void SendAlertsOnInActiveBondAndGuaranteeRM(List<LoanViewModel> loanDetails, string title);
        void SendAlertsOnInActiveBondAndGuaranteeMonitoringTeam(List<LoanViewModel> loanDetails, TBL_MONITORING_ALERT_SETUP alertSetups);



        bool SendAlertsOnExpiredActiveBondAndGuarantee(string title, string messageBody, List<TBL_MONITORING_ALERT_SETUP> alertSetups);
        void ExpiredActiveBondAndGuaranteeRM(List<LoanViewModel> loanDetails, string title);
        void SendAlertsOnExpiredActiveBondAndGuaranteeMonitoringTeam(List<LoanViewModel> loanDetails, TBL_MONITORING_ALERT_SETUP alertSetups);


        bool SendAlertOnAccountWithExeption_Overdrawn(string title, string messageBody, List<TBL_MONITORING_ALERT_SETUP> alertSetups);
        bool SendAlertOnAccountWithExeption_Watchist(string title, string messageBody, List<TBL_MONITORING_ALERT_SETUP> alertSetups);
        void SendAlertsOnAccountWithExeptionRM(List<LoanViewModel> loanDetails, string title);
        bool SendAlertOnAccountWithExeption_Unauthorized(string title, string messageBody, List<TBL_MONITORING_ALERT_SETUP> alertSetups);
        void SendAlertsOnAccountWithExeptionMonitoringTeam(List<LoanViewModel> loanDetails, TBL_MONITORING_ALERT_SETUP alertSetups);



        bool SendAlertOnPastDueObligationAccounts(string title, string messageBody, List<TBL_MONITORING_ALERT_SETUP> alertSetups);
        void SendAlertsOnPastDueObligationAccountsRM(List<LoanViewModel> loanDetails, string title);
        void SendAlertsOnPastDueObligationAccountsMonitoringTeam(List<LoanViewModel> loanDetails, TBL_MONITORING_ALERT_SETUP alertSetups);



        bool SendAlertOnInsuranceApprochingExpiration(string title, string messageBody, List<TBL_MONITORING_ALERT_SETUP> alertSetups);
        void SendAlertOnInsuranceApprochingExpirationRM(List<CollateralViewModel> loanDetails, string title);
        void SendAlertOnInsuranceApprochingExpirationMonitoringTeam(List<CollateralViewModel> loanDetails, TBL_MONITORING_ALERT_SETUP alertSetups);



        bool SendAlertForExpiredInsurance(string title, string messageBody, List<TBL_MONITORING_ALERT_SETUP> alertSetups);
        void SendAlertOnExpiredInsuranceRM(List<CollateralViewModel> loanDetails, string title);
        void SendAlertOnExpiredInsuranceMonitoringTeam(List<CollateralViewModel> loanDetails, TBL_MONITORING_ALERT_SETUP alertSetups);


        bool SendAlertOnTurnoverCovenant(string title, string messageBody, List<TBL_MONITORING_ALERT_SETUP> alertSetups);
        void SendAlertOnTurnoverCovenantRM(List<LoanCovenantDetailViewModel> loanDetails, string title);
        void SendAlertOnTurnoverCovenantMonitoringTeam(List<LoanCovenantDetailViewModel> loanDetails, TBL_MONITORING_ALERT_SETUP alertSetups);


        bool SendAlertsForCollateralPropertyDueForVisitation(string title, string messageBody, List<TBL_MONITORING_ALERT_SETUP> alertSetups);
        void SendAlertsForCollateralPropertyDueForVisitationRM(List<CollateralViewModel> loanDetails, string title);
        void SendAlertsForCollateralPropertyDueForVisitationMonitoringTeam(List<CollateralViewModel> loanDetails, TBL_MONITORING_ALERT_SETUP alertSetups);

        bool SendAlertsForLoanRepayment(string title, string messageBody, List<TBL_MONITORING_ALERT_SETUP> alertSetups);
        void SendAlertsForLoanRepaymentRM(List<LoanPaymentSchedulePeriodicViewModel> loanDetails, string title);
        void SendAlertsForLoanRepaymentTeam(List<LoanPaymentSchedulePeriodicViewModel> loanDetails, TBL_MONITORING_ALERT_SETUP alertSetups);

        bool SendAlertToCustomerForLoanRepaymentApproachingDueDate(string title, string messageBody, List<TBL_MONITORING_ALERT_SETUP> alertSetups);

        int SaveMessageDetails(MessageLogViewModel model);

        List<TBL_MONITORING_ALERT_SETUP> getAlertMessageSetting();

        void LogSLAApprovalNotification();
    }
}
