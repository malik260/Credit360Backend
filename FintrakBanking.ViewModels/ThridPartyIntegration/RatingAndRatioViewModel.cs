using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FintrakBanking.ViewModels.ThridPartyIntegration
{
    public class RatingAndRatioViewModel : GeneralEntity
    {
        public int customerId { get; set; }
        public string description { get; set; }
        public string value { get; set; }
        public bool inUse { get; set; }
        public string indicatorvalue { get; set; }
        public string indicatorname { get; set; }
    }

    public class CutomerRatingViewModel : GeneralEntity
    {
        public string customerCode { get; set; }
        public string companYRating { get; set; }
    }
}
