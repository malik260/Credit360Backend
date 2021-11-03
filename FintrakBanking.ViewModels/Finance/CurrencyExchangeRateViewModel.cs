using System;
using System.Collections.Generic;

namespace FintrakBanking.ViewModels.Finance
{
 
    public partial class CurrencyExchangeRateViewModel
    {        
 
        public short currencyId { get; set; }

        public DateTime date { get; set; }

        public double buyingRate { get; set; }

        public double sellingRate { get; set; }

        public short baseCurrencyId { get; set; }

        public bool isBaseCurrency { get; set; }

        public string webRequestStatus { get; set; }
        public int companyId { get; set; }
        public string fromCurrencyCode { get; set; }
        public string toCurrencyCode { get; set; }
        public double exchangeRate { get; set; }
    }

    public partial class ExchangeRateViewModel
    {
        public string responseCode { get; set; }
        public string responseMessage { get; set; }
        public CurrencyExchangeRateViewModel data { get; set; }
    }

    public partial class CloseMannualBookingViewModel
    {
        public string channel_code { get; set; }
        public string loan_accountno { get; set; }
    }


    public class CloseMannualBookingResponseDetailViewModel
    {
        public DateTime repayment_duedate { get; set; }
        public string product_description { get; set; }
        public string product_category { get; set; }
        public string branch_code { get; set; }
        public string currency_code { get; set; }
        public string customer_name { get; set; }
        public string customer_no { get; set; }
        public string loan_accountno { get; set; }
        public DateTime book_date { get; set; }
        public DateTime value_date { get; set; }
        public DateTime maturity_date { get; set; }
        public decimal amount_disbursed { get; set; }
        public decimal amount_financed { get; set; }
        public string user_referenceno { get; set; }
    }

    public class CloseMannualBookingResponseViewModel
    {
        public string response_code { get; set; }
        public string response_message { get; set; }
        public List<CloseMannualBookingResponseDetailViewModel> loandetailsresp { get; set; }

    }

}