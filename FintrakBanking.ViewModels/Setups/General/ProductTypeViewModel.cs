namespace FintrakBanking.ViewModels.Setups.General
{
    public class ProductTypeViewModel : GeneralEntity
    {
        public bool requirePrincipalGl2;

        public short productTypeId { get; set; }
        public string productTypeName { get; set; }
        public short productGroupId { get; set; }
        public string productGroupName { get; set; }
        public bool requirePrincipalGl { get; set; }
        public bool requireInterestIncomeExpenseGl { get; set; }
        public bool requireInterestReceivablePayableGl { get; set; }
        public bool requireDormantGl { get; set; }
        public bool requirePremiumDiscountGl { get; set; }
        public bool requireOverdrawnGL { get; set; }
        public short dealClassificationId { get; set; }
        public bool requireRate { get; set; }
        public bool requireTenor { get; set; }
        public bool requireScheduleType { get; set; }
    }
}