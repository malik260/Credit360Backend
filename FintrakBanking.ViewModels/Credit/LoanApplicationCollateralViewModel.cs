namespace FintrakBanking.ViewModels.Credit
{
    public partial class LoanApplicationCollateralViewModel : GeneralEntity
    { 
        public int customerCollateralId { get; set; }
        public string certificateOfOwnership { get; set; }
        public int? cityId { get; set; }
        public string city { get; set; }
        public int collateralTypeId { get; set; }
        public string collateralType  { get; set; }
        public string documentTitle { get; set; }
        public double? latitude { get; set; }
        public double? longitude { get; set; }
        public string locationAddress { get; set; }
        public string nearestBusStop { get; set; }
        public string nearestLandmark { get; set; }
        public string otherInformations { get; set; }
        public int loanApplicationId { get; set; }
        public int ApplicationReferanceNumber{ get; set; }
    }

}
