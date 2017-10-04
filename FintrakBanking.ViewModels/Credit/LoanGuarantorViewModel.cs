namespace FintrakBanking.ViewModels.Credit
{
    public class LoanGuarantorViewModel
    {
    public short loanGuarantorId { get; set; }
    public int loanId { get; set; }
    public string fullName { get; set; }
    public string firstname { get; set; }
    public string lastname { get; set; }
    public string middlename { get; set; }
    public string phoneNumber1 { get; set; }
    public string phoneNumber2 { get; set; }
    public string address { get; set; }
    public string relationship { get; set; }
    public int? relationshipDuration { get; set; }
    public string emailAddress { get; set; }
    public string bvn { get; set; }

    }

}
