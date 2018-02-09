using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FintrakBanking.ViewModels.ThridPartyIntegration
{
    public  class CreditBureauSearchViewModel
    {
    }
    public class IndividualSearchViewModel
    {
        public string DataTicket { get; set; }
        public string EnquiryReason { get; set; }
        public string ConsumerName { get; set; }
        public string DateOfBirth { get; set; }
        public string Identification { get; set; }
        public string AccountNumber { get; set; }
        public string ProductID { get; set; }
    }

}
