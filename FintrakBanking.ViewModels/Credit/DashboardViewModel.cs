using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FintrakBanking.ViewModels.Credit
{
    public class DashboardViewModel
    {
        public string sectorName { get; set; }
        public int loanCount { get; set; }
        public double sumOfProposedAmount { get; set; }
        public string prodentialType { get; set; }
        public string loanSystemType { get; set; }

        public int id { get; set; }
        public string name { get; set; }
        public int hoursSpent { get; set; }
        public string riskRating { get; set; }
        public int collateralCustomerId { get; set; }
        public decimal facilityAmount { get; set; }
        public double hairCut { get; set; }
        public decimal collateralValue { get; set; }
    }
    public class DashboardReportItem
    {
        public int id { get; set; }
        public string name { get; set; }
        public int hoursSpent { get; set; }
    }

    public class LoanDisburseByType
    {
        public int count { get; set; }
        public int typeId { get; set; }
        public string type { get; set; }
    }
}
