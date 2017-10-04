namespace FintrakBanking.ViewModels.Credit
{
    public class CamProcessedLoanViewModel3 : LoanApplicationViewModel
    {

        public string loanDetails { get; set; }
        public string camReference { get; set; }
        public int appraisalMemorandumId { get; set; }
        public string customerCode { get; set; }
        public int casaAccountId { get; set; }
        public string loanStatusName { get; set; }
        public short productTypeId { get; set; }
        public string productTypeName { get; set; }
        public int customerSensitivityLevelId { get; set; }
        public string camDocumentation { get; set; }
        public short applicationStatusId { get; set; }
    }

}
