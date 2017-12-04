using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FintrakBanking.ViewModels.Report
{
    class CollateralInformation
    {
    }

    public class CollateralEstimatedViewModel
    {
        public string firstName { get; set; }
        public string lastName { get; set; }
        public string middleName { get; set; }
        public string customerName { get { return lastName + " " + middleName + " " + firstName; } }
        public decimal? facilityAmount { get; set; }
        public string companyName { get; set; }
        public int customerId { get; set; }
        public string facilityName { get; set; }
        public string collateralType { get; set; }
        public string collateralDetail { get; set; }
        public string collateralCode { get; set; }
        public double hairCut { get; set; }
        public decimal collateralValue { get; set; }

        public double securityValue { get { return (double)collateralValue - ((double)hairCut * (double)collateralValue * 0.01); } }

        public decimal amountInUse { get; set; }
        public string loanRefrenceNumber { get; set; }

    }

    public class CollateralSearchEntity
    {
        public string collateralCode { get; set; }
        public string acctNumber { get; set; }
    }
}
