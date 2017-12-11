using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FintrakBanking.ViewModels.Credit
{
    public class LoanPrincipalViewModel : GeneralEntity
    {
        public int principalId { get; set; }

        public string principalsRegNumber { get; set; }

        public string name { get; set; }

        public string accountNumber { get; set; }

        public string emailAddress { get; set; }

        public string phoneNumber { get; set; }

        public string address { get; set; }
    }
}
