using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FintrakBanking.ViewModels.Credit
{
  public  class DailyAccrualDetailViewModel
    {
        public string REFERENCENUMBER { get; set; }
        public string BASEREFERENCENUMBER { get; set; }
        public string CATEGORY { get; set; }
        public string TRANSACTIONTYPE { get; set; }
        public string PRODUCT { get; set; }
        public string BRANCH { get; set; }
        public string CURRENCY { get; set; }
        public double EXCHANGERATE { get; set; }
        public decimal MAINAMOUNT { get; set; }
        public double INTERESTRATE { get; set; }
        public string DAYCOUNTCONVENTION { get; set; }
        public decimal DAILYACCURALAMOUNT { get; set; }
        public string REPAYMENTPOSTEDSTATUS { get; set; }
        public decimal SYSTEMDATETIME { get; set; }
        public DateTime DATE { get; set; }
        public string CHARGEFEE { get; set; }
        public DateTime? DEMANDDATE { get; set; }
        public decimal DAILYACCURALAMOUNT2 { get; set; }
    }
}
