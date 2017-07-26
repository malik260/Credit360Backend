using System.Text;

namespace FintrakBanking.ViewModels.Risk
{
    public class RiskLoanApplicationViewModel
    {
        public int loanApplicationId { get; set; }
        public string customerCode { get; set; }
        public string customerName { get; set; }
        public decimal amount { get; set; }
        public string sector { get; set; }
        public string natureOfBusiness { get; set; }
        public string loanType { get; set; }
    }

    public class RiskType
    {
        public int loanApplicationId { get; set; }
        public int riskTypeId { get; set; }
        public int productId { get; set; }
    }
}
