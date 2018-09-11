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
        public string customerGroupName { get; set; }
        public string customerEmailAddress { get; set; }
        public string customerPhoneNumber { get; set; }
        public bool isFinal  { get; set; }
        public string final { get; set; }
        public int producyClassProcessId { get; set; }
        public int loanApplicationDetailId { get; set; }
    }

    public class OfferLetterDetailViewModel : GeneralEntity
    {
        public string loanApplicationId { get; set; }
        public string productName { get; set; }
        public string customerName { get; set; }
        public string currencyName { get; set; }
        public decimal loanAmount { get; set; }
        public double exchangeRate { get; set; }
        public string customerAddress { get; set; }
        public string customerEmailAddress { get; set; }
        public string customerPhoneNumber { get; set; }
        public DateTime applicationDate { get; set; }
        public string applicationReferenceNumber { get; set; }

        public decimal baseCurrencyLoanAmount
        {
            get { return this.loanAmount * (decimal)this.exchangeRate; }
        }

        public int tenor { get; set; }
        public double interestRate { get; set; }
        public string customerGroupName { get; set; }
        public string loanTypeName { get; set; }
        public string repaymentTerms { get; set; }
        public string repaymentSchedule { get; set; }
        public string purpose { get; set; }
        public short currencyId { get; set; }
        public string productPriceIndex { get; set; }
    }

    public class OfferLetterConditionPrecidentViewModel : GeneralEntity
    {
        public int loanApplicationId { get; set; }
        public string conditionPrecident { get; set; }
        public bool isExternal { get; set; }
        public string productName { get; set; }
    }

    public class OfferLetterTemplateViewModel: GeneralEntity
    {
        public string documentTemplate { get; set; }
        public int documentId { get; set; }
        public string applicationReferenceNumber { get; set; }
        public short? productId { get; set; }
        public string comments { get; set; }
        public bool isAccepted { get; set; }
        public decimal approvedAmount { get; set; }
        public bool isFinal { get; set; }
        public bool saveOnly { get; set; }
    }

    public class Form3800ViewModel: GeneralEntity
    {
        public string documentTemplate { get; set; }
    }

}
