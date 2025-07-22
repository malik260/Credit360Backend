using FintrakBanking.ViewModels.Setups.General;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FintrakBanking.ViewModels.Credit
{
    public class OfferLetterViewModel : GeneralEntity
    {
        public string purpose;
        public decimal amount;
        public int tenor;
        public string loanType;
        public string pricing;

        public int customerId { get; set; }
        public string customerName { get; set; }
        public string customerAddress { get; set; }
        public DateTime applicationDate { get; set; }
        public string customerGroupName { get; set; }
        public string customerEmailAddress { get; set; }
        public string customerPhoneNumber { get; set; }
        public bool? isFinal  { get; set; }
        public string final { get; set; }
        public int producyClassProcessId { get; set; }
        public int loanApplicationDetailId { get; set; }
        public string operationName { get; set; }
        public string managementPosition { get; set; }
        public string offerLetterTitle { get; set; }
        public string offerLetterSalutation { get; set; }
        public string offerLetterClauses { get; set; }
        public string offerLetteracceptance { get; set; }
        public int loanApplicationId { get; set; }
        public bool isLMS { get; set; }
        public string title { get; set; }
        public int documentTemplate { get; set; }
        public string customerName2 { get; set; }
        public double? apr { get; set; }
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

        public List<ProductFeeViewModel> feesList { get; set; }

        public string fees { get; set; }
        
        public int tenor { get; set; }
        public double interestRate { get; set; }
        public string customerGroupName { get; set; }
        public string loanTypeName { get; set; }
        public string repaymentTerms { get; set; }
        public string repaymentSchedule { get; set; }
        public string purpose { get; set; }
        public short currencyId { get; set; }
        public string productPriceIndex { get; set; }
        public DateTime? approvedDate { get; set; }
        public string approvedAmountCurrency { get; set; }
        public int loanApplicationDetailId { get; set; }
        public string interestRateAndFees { get; set; }
        public decimal approvedAmount { get; set; }
        public short operarionId { get; set; }
        public string offerLetterIntroduction { get; set; }
        public bool isRenewal { get; set; }
        public short approvedProductId { get; set; }
        public short? productClassId { get; set; }
        public short? productTypeId { get; set; }
        public DateTime newApplicationDate;
        public string approvedTenorString
        {
            get
            {
                var units = tenor == 1 ? " day" : " days";
                if (tenor < 15) return tenor.ToString() + units;
                var months = Math.Ceiling((Math.Floor(tenor / 15.00)) / 2);
                units = months == 1 ? " month" : " months";
                return months.ToString() + " " + units;
            }
        }

        public string feeName { get; set; }
        public decimal rateValue { get; set; }
        public double? apr { get; set; }
    }

    public class OfferLetterConditionPrecidentViewModel : GeneralEntity
    {
        public int loanApplicationId { get; set; }
        public string conditionPrecident { get; set; }
        public bool isExternal { get; set; }
        public string productName { get; set; }
        public int SN { get; set; }
        public int? conditionId { get; set; }
        public string sortOrder { get; set; }
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
        public int loanApplicationId { get; set; }
    }

    public class Form3800ViewModel: GeneralEntity
    {
        public string documentTemplate { get; set; }
    }

    public class LeaseFacility
    {
        public int qty { get; set; }
        public string model { get; set; }
        public decimal unitCost { get; set; }
        public decimal totalCost { get; set; }
        public string dealer { get; set; }
    }

}
