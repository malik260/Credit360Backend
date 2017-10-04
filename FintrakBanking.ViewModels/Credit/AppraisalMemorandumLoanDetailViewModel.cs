namespace FintrakBanking.ViewModels.Credit
{
    public class AppraisalMemorandumLoanDetailViewModel
    {
        public int appraisalMemorandumLoanDetailId { get; set; }

        public int appraisalMemorandumId { get; set; }

        public decimal principalAmount { get; set; }

        public double interestRate { get; set; }

        public int tenor { get; set; }

    }

}
