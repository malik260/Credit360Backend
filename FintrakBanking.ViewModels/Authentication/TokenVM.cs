namespace FintrakBanking.ViewModels.Authentication
{
    public class TokenVM
    {
        public string username { get; set; }
        public string password { get; set; }
        public string encodedToken { get; set; }
        public string validTo { get; set; }
    }
}