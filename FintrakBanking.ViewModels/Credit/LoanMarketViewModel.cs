using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FintrakBanking.ViewModels.Credit
{
    public class LoanMarketViewModel : GeneralEntity
    {
        public int marketId { get; set; }
        public string marketName { get; set; }
        public string accountNumber { get; set; }
        public string emailAddress { get; set; }
        public string phoneNumber { get; set; }
        public int cityId { get; set; }
        public string address { get; set; }
        public int stateId { get; set; }
        public string cityName { get; set; }
    }
}
