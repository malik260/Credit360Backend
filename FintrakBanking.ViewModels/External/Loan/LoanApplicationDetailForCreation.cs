using FintrakBanking.Common.Enum;
using FintrakBanking.ViewModels.External.Customer;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FintrakBanking.ViewModels.External.Loan
{
    public class LoanApplicationDetailForCreation
    {
        public string customerCode { get; set; }
        public int proposedProductId { get; set; }
        public int proposedTenor { get; set; }
        public int proposedRate { get; set; }
        public decimal proposedAmount { get; set; }
        public short subSectorId { get; set; }
        public string loanPurpose { get; set; }
        public string operatingAccountNo { get; set; }
        public string repaymentDate { get; set; }
        public decimal requestedAmount { get; set; }
        public decimal? creditScore { get; set; }
        public string creditRating { get; set; }
        public AffordabilityViewModel affordabilityDetails { get; set; }
        // public int crmsFundingSourceId { get; set; }
        //public int crmsPaymentSourceId { get; set; }
        //public TraderLoanForCreation traderLoan { get; set; }
        //public InvoiceDetailForCreation invoiceDetail { get; set; }


        // PROPERTIES THAT WILL BE AUTO GENERATED 
        public int tenorModeId { get; set; } = (int)TenorMode.Yearly;

        public int? businessUnit { get; set; }

        public short currencyId { get; set; } = 1;
        public double exchangeRate { get; set; } = 1;
        public decimal exchangeAmount { get { return (decimal)exchangeRate * proposedAmount; } }

        public bool isUnAdviced { get; set; } = false;
        public bool isLineFacility { get; set; } = false;
        public bool esgmsRequired { get; set; } = false;
        public short? propertyTypeId { get; set; }
        public string propertyTitle { get; set; }
        public decimal? propertyPrice { get; set; }
        public decimal? downPayment { get; set; }

    }
}
