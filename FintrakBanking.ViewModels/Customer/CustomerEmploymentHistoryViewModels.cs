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

    }


}