using FintrakBanking.ViewModels;
using FintrakBanking.ViewModels.Credit;
using System;
using System.Collections.Generic;
using System.Text;

namespace FintrakBanking.Interfaces.Credit
{
    public interface ILimitRepository
    {
        //#region Limits
        //IEnumerable<LimitViewModel> GetAllLimit(int companyId);
        //List<LimitViewModel> GetLimitById(int LimitId);
        //bool AddLimit(LimitViewModel model);
        //bool UpdateLimit(int LimitId, LimitViewModel model);
        //bool DeleteLimit(int LimitId, UserInfo user);
        //#endregion

        //#region Limits Details
        //IEnumerable<LimitDetailViewModel> GetAllLimitDetail(int id);
        //List<LimitDetailViewModel> GetLimitDetailById(int limitDetailId);
        //bool AddLimitDetail(LimitDetailViewModel model);
        //bool UpdateLimitDetail(int limitDetailId, LimitDetailViewModel model);
        //bool DeleteLimitDetail(int limitDetailId, UserInfo user);
        //bool AddMultipleLimitDetail(List<LimitDetailViewModel> model);
        //#endregion

        //#region Limits Metric
        //IEnumerable<LimitMetricViewModel> GetAllLimitMetric();
        //#endregion

        //#region Limits Type
        //IEnumerable<LimitTypeViewModel> GetAllLimitType();
        //#endregion

        //#region Limits Value Type
        //IEnumerable<LimitValueTypeViewModel> GetAllLimitValueType();
        //#endregion

        //#region Frequency Type
        //IEnumerable<FrequencyTypeViewModel> GetAllFrequencyType();
        //#endregion

        //IEnumerable<ObligorLimitViewModel> GetAllObligorLimit();
        //bool AddUpdateRiskRating(ObligorLimitViewModel entity);
        //bool ValidateRiskRating(string riskRating);
    }
}
