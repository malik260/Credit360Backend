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



    public class Exposure 
    {
        public short facilityName { get; set; }
        public short currency { get; set; }
        public decimal outstandingExpo { get; set; }
        public decimal impact { get; set; }
        public decimal approvedAmount { get; set; }
        public string productName { get; set; }
        public string currencyCode { get; set; }
        public string currencyName { get; set; }
    }
}
