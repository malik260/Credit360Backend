using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FintrakBanking.ViewModels.Credit
{
    public class OfferLetterViewModel : GeneralEntity
    {
        public int customerId { get; set; }
        public string customerName { get; set; }
        public string customerAddress { get; set; }

        public DateTime applicationDate { get; set; }
    }

    public class OfferLetterDetailViewModel : GeneralEntity
    {
        public int loanApplicationId { get; set; }
        public string productName { get; set; }
        public string customerName { get; set; }
        public string currencyName { get; set; }
        public decimal loanAmount { get; set; }
        public double exchangeRate { get; set; }

        public decimal baseCurrencyLoanAmount
        {
            get { return this.loanAmount * (decimal)this.exchangeRate; }
        }

        public int tenor { get; set; }
        public double interestRate { get; set; }

    }

    public class OfferLetterConditionPrecidentViewModel : GeneralEntity
    {
        public int loanApplicationId { get; set; }
        public string conditionPrecident { get; set; }
    }

}
