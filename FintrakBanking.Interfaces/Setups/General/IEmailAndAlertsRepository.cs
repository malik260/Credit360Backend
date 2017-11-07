using FintrakBanking.ViewModels.Setups.General;
using System.Collections.Generic;

namespace FintrakBanking.Interfaces.Setups.General
{
    public interface IEmailAndAlertsRepository
    {
        void SendAlertsForCovenantsApproachingDueDate();

        void SendAlertsForCovenantsOverDue();

        void SendAlertsForCollateralPropertyRevaluation();

        void SendAlertsForLoanNplMonitoring();

        void SendAlertsOnSelfLiquidatingLoanExpiry();

        void SendEmailAlertsForWorkflow(string[] emails, string operation, bool group, string link);

        bool CreateEmailMessageAndSend(MessageLogViewModel model);

        IEnumerable<MessageLogViewModel> GetMailingList();

        IEnumerable<MessageLogViewModel> GetEmailMailingList();

        IEnumerable<MessageLogViewModel> GetSmsMailingList();

        bool UpdateMailDeliveryStatus(int messageId, short statusId);
        void SendAlertsOnOverDraftLoansAlmostDue();
    }
}