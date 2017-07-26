using System;
using System.Collections.Generic;
using System.Text;

namespace FintrakBanking.ViewModels.CASA
{
    public class CustomerSearchVM
    {
        public int customerId { get; set; }
        public string firstName { get; set; }
        public string customerCode { get; set; }
        public string middleName { get; set; }
        public string lastName { get; set; }
        public string accountNumber { get; set; }
        public string currency { get; set; }
        public int relationshipOfficerId { get; set; }
        public int relationshipManagerId { get; set; }
        public string customerName { get { return $"{this.firstName} {this.lastName}"; } }
    }
}
