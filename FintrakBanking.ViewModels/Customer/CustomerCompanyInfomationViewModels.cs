namespace FintrakBanking.ViewModels.Customer
{
    public class CustomerCompanyInfomationViewModels : GenaralEntity
    {
        public int companyInfomationId { get; set; }
        public int customerId { get; set; }
        public string registrationNumber { get; set; }
        public string companyName { get; set; }
        public string companyWebsite { get; set; }
        public string companyEmail { get; set; }
        public string registeredOffice { get; set; }
        public string annualTurnOver { get; set; }
        public string corporateBusinessCategory { get; set; }
        public string creditRating { get; set; }
        public string previousCreditRating { get; set; }
    }


}