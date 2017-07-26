using FintrakBanking.ViewModels;
using FintrakBanking.ViewModels.Setups;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace FintrakBanking.Interfaces.Setups.Risk
{
    public interface IRiskSetupRepository
    {
        Task<bool> AddRiskRating(RiskRatingViewModel entity);
        Task<bool> DeleteRiskRating(int ratingId, RiskRatingViewModel entity);
        Task<bool> UpdateRiskRating(int ratingId, RiskRatingViewModel entity);
        IEnumerable<RiskRatingViewModel> GetRiskRatingByCompanyId(int companyId);
        IEnumerable<RiskRatingViewModel> GetRiskRatingByProductId(int ratingId);
        IEnumerable<RiskRatingViewModel> GetRiskRating();


        Task<bool> AddRiskAssessmentIndexs(RiskAssessmentIndexViewModels entity);
        Task<bool> UpdateRiskAssessmentIndex(int riskId, RiskAssessmentIndexViewModels entity);
        Task<bool> DeleteRiskAssessmentIndex(int riskId, UserInfo user);
        IEnumerable<RiskAssessmentIndexViewModels> GetRiskAssessmentIndexByRiskTitle(int riskTitleId, int companyId);
        RiskAssessmentIndexViewModels GetRiskAssessmentIndexById(int riskId, int companyId);

        IEnumerable<RiskAssessmentIndexViewModels> GetRiskAssessmentIndexByParent(int parentId, int companyId);
        IEnumerable<RiskAssessmentIndexViewModels> GetRiskAssessmentIndexByItemLevel(int levelId, int companyId);


        Task<bool> AddRiskAssessmentTitle(RiskAssessmentTitleViewModels entity);
        Task<bool> UpdateRiskAssessmentTitle(int riskAssessmentTitleId, RiskAssessmentTitleViewModels entity);
        Task<bool> DeleteRiskAssessmentTitle(int riskAssessmentTitleId, UserInfo user);

        IEnumerable<RiskAssessmentTitleViewModels> GetRiskAssessmentTitleByProductId(int productId, int companyId);
        IEnumerable<RiskAssessmentTitleViewModels> GetRiskAssessmentTitleByRiskType(int riskTypeId, int companyId);
        RiskAssessmentTitleViewModels GetRiskAssessmentTitleById(int riskAssessmentTitleId, int companyId);
        IEnumerable<RiskAssessmentTitleViewModels> GetRiskAssessmentTitle(int companyId);
    }
}