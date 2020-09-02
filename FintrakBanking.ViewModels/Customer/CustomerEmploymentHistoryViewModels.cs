using System;

namespace FintrakBanking.ViewModels.Customer
{
    public class CustomerEmploymentHistoryViewModels : GeneralEntity
    {
        public int placeOfWorkId { get; set; }
        public string employerName { get; set; }
        public string employerAddress { get; set; }
        public int? employerStateId { get; set; }
        public string employerState { get; set; }
        public int employerCountryId { get; set; }
        public string officePhone { get; set; }
        public DateTime employDate { get; set; }
        public string previousEmployer { get; set; }
        public int customerId { get; set; }
        public bool active { get; set; }
        public int? yearOfEmployment { get; set; }
        public int? totalWorkingExperience { get; set; }
        public int? yearsOfCurrentEmployment { get; set; }
        public decimal? terminalBenefits { get; set; }
        public decimal? annualIncome { get; set; }
        public decimal? monthlyIncome { get; set; }
        public decimal? expenditure { get; set; }
        public int? approvedEmployerId { get; set; }
        public bool isEmployerRelated { get; set; }

        public int? employerId { get; set; }
    }


}