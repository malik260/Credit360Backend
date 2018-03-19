using FintrakBanking.Entities.Models;
using FintrakBanking.ViewModels.Setups.General;
using System.Collections.Generic;

namespace FintrakBanking.Interfaces.Setups.General
{
    public interface IEmailAndAlertsRepository
    {

        void SendAlertsForCovenantsApproachingDueDate(string title, string messageBody, List<TBL_MONITORING_ALERT_SETUP> alertSetups);

        void SendAlertsForCollateralPropertyRevaluation(string title, string messageBody);

        void SendAlertsForLoanNplMonitoring(string title, string messageBody);

        void SendAlertsOnSelfLiquidatingLoanExpiry(string title, string messageBody);

        bool CreateEmailMessageAndSend(MessageLogViewModel model);

        IEnumerable<MessageLogViewModel> GetMailingList();

        IEnumerable<MessageLogViewModel> GetEmailMailingList();

        IEnumerable<MessageLogViewModel> GetSmsMailingList();

        bool UpdateMailDeliveryStatus(int messageId, short statusId);

        void SendAlertsOnOverDraftLoansAlmostDue(string title, string messageBody);

        void SendAlertsOnLoanForInActiveBondAndGuarantee(string title, string messageBody);
    }
}