using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FintrakBanking.ViewModels.External.Customer
{
    public class DirectorInfoForCustomerCreation
    {
        public string surname { get; set; }
        public string firstname { get; set; }
        public string middlename { get; set; }
        public string customerNIN { get; set; }
        public double numberOfShares { get; set; }
        public string bankVerificationNumber { get; set; }
        public string address { get; set; }
        public string phoneNumber { get; set; }
        public string email { get; set; }
    }
}
