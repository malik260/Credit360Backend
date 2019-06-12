using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using FintrakBanking.ViewModels.Risk;

namespace FintrakBanking.Interfaces.Risk
{
    public interface ICreditOfficerRiskRepository
    {
        MatrixGrid GetCreditOfficerRiskRating(string username);
        IEnumerable<RatingPeriodViewModel> GetRatingPeriods();
        bool AddRatingPeriod(RatingPeriodViewModel model);
        bool AddOfficerRating(OfficerRatingViewModel model);

        List<CreditOfficerRatingViewModel> GetCreditOfficerSearch(CreditOfficerSearchViewModel model);
        KeyIndicatorAssessmentParametersViewModel GetKeyIndicatorAssessmentParameters();
        CreditOfficerRiskRatingDetail GetCurrentCreditOfficerRiskRating(int id);
    }
}
