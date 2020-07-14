using System.Collections.Generic;
using System.Text;

namespace FintrakBanking.ViewModels.Setups.Credit
{
    public class CollateralLocationsViewModel : GeneralEntity
    {
        public int collateralLocationId { get; set; }
        public int collateralTypeId { get; set; }
        public int cityId { get; set; }
        public new int countryId { get; set; }
        public string country { get; set; }
        public string city { get; set; }
        public string collateralTypeName { get; set; }
        
    }
}
