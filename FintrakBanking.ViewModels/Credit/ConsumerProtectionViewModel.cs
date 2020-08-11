using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FintrakBanking.ViewModels.Credit
{
    public class ConsumerProtectionViewModel: GeneralEntity
    {
        public int consumerProtectionId { get; set; }

        public double annualInterestRate { get; set; }
        public double totalFees { get; set; }
        public double insurance { get; set; }
        public double loanAPR { get; set; }
        public short termOfLoanInYears { get; set; }
        public short branchId { get; set; }

        public decimal loanAmount { get; set; }

        public decimal monthlyPayment { get; set; }
        public decimal totalFeesAndCharges { get; set; }
        public decimal actualAmountBorrowed { get; set; }
    }
}
