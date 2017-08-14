
using FintrakBanking.ViewModels;
using FintrakBanking.ViewModels.Credit;
using System.Collections.Generic;

namespace FintrakBanking.Interfaces.Credit
{
    public interface IAppraisalMemorandumRepository
    {
        AppraisalMemorandumViewModel GetAppraisalMemorandumByLoanApplicationId(int applicationId);

        AppraisalMemorandumViewModel AddAppraisalMemorandum(AppraisalMemorandumViewModel model);

        IEnumerable<AppraisalMemorandumViewModel> GetAllAppraisalMemorandum();

        bool AppendTemplate(AppraisalMemorandumViewModel model, int appraisalMemorandumId, int userId);

        bool UpdateAppraisalMemorandum(AppraisalMemorandumViewModel model, int appraisalMemorandumId);
        
    }
}
