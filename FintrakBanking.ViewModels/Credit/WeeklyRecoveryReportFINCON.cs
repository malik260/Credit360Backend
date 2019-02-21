using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FintrakBanking.ViewModels.Credit
{
    public class WeeklyRecoveryReportFINCON
    {
        public string loanReferenceNumber { get; set; }
        public decimal principal { get; set; }
        public decimal interest { get; set; }
        public string status { get; set; }
        public DateTime effectiveDate { get; set; }
        public DateTime maturityDate { get; set; }
        public string businessDevelopmentManager { get; set; }
        public int MyProperty { get; set; }
        public string customerName { get; set; }
        public string account { get; set; }
        public string nplAccount { get; set; }
        public string camsolAccount { get; set; }
        public string branchName { get; set; }
        public string netPosition { get; set; }
        public decimal suspense { get; set; }
        public decimal outStandingAmount { get; set; }
        public decimal amountReceived { get; set; }
        public DateTime dateGranted { get; set; }
        public DateTime dateCreated { get; set; }
        public string classification { get; set; }
        public string collateral { get; set; }
        public string source { get; set; }




    }
}
