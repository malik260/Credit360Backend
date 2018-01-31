using System;

namespace FintrakBanking.ViewModels.Setups.General
{
    public class LoanRecoverySetupViewModel  : GeneralEntity
    {
        public int recoveryPlanId { get; set; }
        public string loanId  { get; set; }
        public string loanRefNo { get; set; }
        public short productTypeId{ get; set; }
        public string productTypeName { get; set; }
        public int casaAccountId  { get; set; }
        public string casaAccountName { get; set; }
        public int agentId { get; set; }
        public string agentName  { get; set; }
        public decimal amountOwed  { get; set; }
        public decimal writeOffAmount { get; set; }
        public decimal paymentAmount{ get; set; }
        public DateTime paymentDate{ get; set; }
        public int recoveryPaymentPlanId{ get; set; }
        public int paymentloanId { get; set; }

    }

   
}