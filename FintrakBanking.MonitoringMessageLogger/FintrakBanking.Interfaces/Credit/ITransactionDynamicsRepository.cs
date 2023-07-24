using FintrakBanking.ViewModels;
using FintrakBanking.ViewModels.Credit;
using System.Collections.Generic;

namespace FintrakBanking.Interfaces.Credit
{
    public interface ITransactionDynamicsRepository
    {
        IEnumerable<TransactionDynamicsViewModel> GetTransactionDynamicsByDetailId(int detailId);

        IEnumerable<TransactionDynamicsViewModel> GetAllTransactionDynamics();

        bool AddTransactionDynamics(TransactionDynamicsViewModel model);

        IEnumerable<TransactionDynamicsViewModel> GetTransactionDynamicsTemplate();

        bool AddTransactionDynamicsTemplate(TransactionDynamicsViewModel model);

        bool UpdateTransactionDynamicsTemplate(TransactionDynamicsViewModel model, int conditionPrecedentId);

        bool RemoveLoanTransactionDynamics(int id, UserInfo user);

        bool EditLoanTransactionDynamics(int id, TransactionDynamicsViewModel entity);

        List<TransactionDynamicsViewModel> GetTransactionDynamicsDefaultByDetailId(int detailId);

        List<TransactionDynamicsViewModel> AddSelectedTransactionDynamics(SelectedIdsViewModel entity);

        // LMS approval
        List<TransactionDynamicsViewModel> AddSelectedTransactionDynamicsLms(SelectedIdsViewModel entity);
        bool RemoveLoanTransactionDynamicsLms(int id, UserInfo user);
        bool EditLoanTransactionDynamicsLms(int id, TransactionDynamicsViewModel entity);
        bool AddTransactionDynamicsLms(TransactionDynamicsViewModel entity);
        IEnumerable<TransactionDynamicsViewModel> GetTransactionDynamicsByDetailIdLms(int detailId);
        List<TransactionDynamicsViewModel> GetTransactionDynamicsDefaultByDetailIdLms(int detailId);
        List<TransactionDynamicsViewModel> GetTransactionDynamicsDefaultByApplicationIdAndOperationLms(int detailId, int? operationId);

        // SUGGESTED conditions
        bool AddSuggestedConditions(SuggestedConditionsViewModel entity);
        List<SuggestedConditionsViewModel> GetSuggestedConditions(int applicationId);
        List<SuggestedConditionsViewModel> GetSuggestedConditionsByApplicationId(int applicationId);
        bool UpdateSuggestedConditions(int id, SuggestedConditionsViewModel entity);
        bool RemoveSuggestedConditions(int id, UserInfo user);

    }
}