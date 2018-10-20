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

    }
}
