using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FintrakBanking.ViewModels.Credit
{
    public class CollateralPerfectionyettoCommenceViewModel
    {
        public int loanId { get; set; }

      //  public short loanSystemTypeId { get; set; }
        public string subHead { get; set; }
        public string customername { get; set; }

        public decimal outstandingBalance { get; set; }

        public decimal outstandingInterest { get; set; }

        public string collateralType { get; set; }
        public DateTime facilityGrantDate { get; set; }

        public DateTime startDate { get; set; }
        public DateTime endDate { get; set; }
        public string staffCode { get; set; }
        public decimal total { get; set; }
         

    }


    public class SubHead
    {
        public string subHead { get; set; }
        public string staffCode { get; set; }
        public string teamUnit { get; set; }
        public string region { get; set; }
        public string firstName { get; set; }
        public string lastName { get; set; }
        public string middleName { get; set; }
        public string businessDevelopmentManger { get; set; }
        public string businessUnit { get; set; }
    }



    public class StaffMis
    {
        public int staffId { get; set; }
        public string staffCode { get; set; }
        public string subhead { get; set; }
    }
}
