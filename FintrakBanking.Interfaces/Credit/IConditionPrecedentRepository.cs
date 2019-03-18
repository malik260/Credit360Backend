using FintrakBanking.ViewModels;
using FintrakBanking.ViewModels.Credit;
using System.Collections.Generic;

namespace FintrakBanking.Interfaces.Credit
{
    public interface IConditionPrecedentRepository

    {
        IEnumerable<ConditionPrecedentViewModel> GetConditionPrecedentByDetailId(int detailId);

        IEnumerable<ConditionPrecedentViewModel> GetAllConditionPrecedent();

        bool AddConditionPrecedent(ConditionPrecedentViewModel model);

        IEnumerable<ConditionPrecedentViewModel> GetConditionPrecedentTemplate();

        bool AddConditionPrecedentTemplate(ConditionPrecedentViewModel model);

        bool UpdateConditionPrecedentTemplate(ConditionPrecedentViewModel model, int conditionPrecedentId);

        bool RemoveLoanConditionPrecedent(int id, UserInfo user);

        bool EditLoanConditionPrecedent(int id, ConditionPrecedentViewModel entity);

        IEnumerable<ComplianceTimelineViewModel> GetComplianceTimelineTemplate();

        bool AddComplianceTimelineTemplate(ComplianceTimelineViewModel model);

        bool UpdateComplianceTimelineTemplate(ComplianceTimelineViewModel model, int timelineId);
        bool RemoveComplianceTimelineTemplate(UserInfo user, int id);

        List<ConditionPrecedentViewModel> GetConditionPrecedentDefaultByDetailId(int detailId);

        List<ConditionPrecedentViewModel> AddSelectedConditionPrecedent(SelectedIdsViewModel entity);

        // LMS approval
        bool RemoveLoanConditionPrecedentLms(int id, UserInfo user);
        bool EditLoanConditionPrecedentLms(int id, ConditionPrecedentViewModel entity);
        List<ConditionPrecedentViewModel> AddSelectedConditionPrecedentLms(SelectedIdsViewModel entity);
        bool AddConditionPrecedentLms(ConditionPrecedentViewModel entity);
        IEnumerable<ConditionPrecedentViewModel> GetConditionPrecedentByDetailIdLms(int detailid);
        List<ConditionPrecedentViewModel> GetConditionPrecedentDefaultByDetailIdLms(int detailId);
        bool DeleteConditionPrecedentTemplate(UserInfo user, int id);

        // additional comment condition
        List<AdditionalCommentViewModel> GetAdditionalComment(int applicationId, int callerId, int userId);
        bool AddAdditionalComment(AdditionalCommentViewModel entity);
        bool EditAdditionalComment(int id, AdditionalCommentViewModel entity);
        bool RemoveAdditionalComment(int id, UserInfo user);

        List<ConditionPrecedentViewModel> GetConditionPrecedentDefaultByApplicationIdAndOperationLms(int detailId, int? operationId);
    }
}
