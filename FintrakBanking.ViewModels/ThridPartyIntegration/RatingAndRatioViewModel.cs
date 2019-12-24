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
        public int loanApplicationId { get; set; }

    }

    public class SubGroupRatingAndRatioViewModel : GeneralEntity
    {
        public string financial_Period { get; set; }
        public List<RatingAndRatioViewModel> ratio_List { get; set; }
    }

    public class GroupRatingAndRatioViewModel : GeneralEntity
    {
        public string ratioHeaderId { get; set; }
        public string ratioHeader { get; set; }
        public List<RatingAndRatioViewModel> ratio { get; set; }

    }

    public class MainGroupRatingAndRatioViewModel : GeneralEntity
    {
        public string financial_Period { get; set; }
        public List<GroupRatingAndRatioViewModel> ratio_List { get; set; }
    }

    public class CutomerRatingViewModel : GeneralEntity
    {
        //public string companYRating { get; set; }
        public string companY_RATING { get; set; }
        public string customeR_ID { get; set; }

    }

    public class FacilityRatingViewModel : GeneralEntity
    {
        public int loanApplicationDetailId { get; set; }
        public string customer_ID { get; set; }
        public string probability_of_Default { get; set; }
        public string remark { get; set; }

    }
}
