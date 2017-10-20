namespace FintrakBanking.Interfaces.Setups.General
{
    public interface IEmailAndAlertsRepository
    {
        void SendAlertsForCovenantsApproachingDueDate();

        void SendAlertsForCovenantsOverDue();

        void SendAlertsForCollateralPropertyRevaluation();

        void SendAlertsForLoanNplMonitoring();

        void SendAlertsOnSelfLiquidatingLoanExpiry();
    }
}