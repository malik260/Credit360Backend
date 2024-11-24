using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FintrakBanking.ViewModels.External.Customer
{
    public class AffordabilityViewModel : GeneralEntity
    {
        public DateTime? dateOfEmployment { get; set; }
        public string nhfAccount { get; set; }
        public string customerName { get; set; }
        public int customerId { get; set; }
        public DateTime? dateOfBirth { get; set; }
        public decimal? monthlyIncome { get; set; }
        public string sortCode { get; set; }
        public int quotient { get; set; }
        public int age { get; set; }
        public int yearsToClockSixty { get; set; }
        public int yearsInService { get; set; }
        public int yearsToClockThirtyFive { get; set; }
        public int minYearsInServiceYearsToClockSixty { get; set; }
        public int repaymentPeriod { get; set; }
        public double presentValue { get; set; }
        public double affordableAmount { get; set; }
        public double monthlyRepayment { get; set; }
        public double profitability { get; set; }
        public int casaAccountId { get; set; }
        public double rate { get; set; }
        public double amountRequested { get; set; }
        public int productId { get; set; }
        public bool? tenorOverride { get; set; }
        public int proposedTenor { get; set; }
        public int loanAffordabilityId { get; set; }
        public int loanApplicationDetailId { get; set; }
        public string productName { get; set; }
        
    }


}


