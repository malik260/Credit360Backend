using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FintrakBanking.ViewModels.Credit
{
    public class DisbursalCreditTurnoverViewModel: ImpairedWatchListViewModel
    {
        public string bdo { get; set; }
        public string groupName { get; set; }
        public string branches { get; set; }
        public string schemeCode { get; set; }
        public string customerName { get; set; }
        public string operativeAcct { get; set; }
        //public string sanctionLimit { get; set; }
        public DateTime? dateDisbursed { get; set; }
        public DateTime? expiryDate { get; set; }
        public int daysPastDue { get; set; }
        //public string totalExposure { get; set; }
        public string status { get; set; }
        public Decimal? currentBalance { get; set; }
        public string excessAboveLimit { get; set; }
        public string crTurnover { get; set; }
        public int custId { get; set; }








    }
}


