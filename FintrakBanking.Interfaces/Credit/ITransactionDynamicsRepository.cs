using FintrakBanking.ViewModels;
using FintrakBanking.ViewModels.Credit;
using System.Collections.Generic;

namespace FintrakBanking.Interfaces.Credit
{
    public interface ITransactionDynamicsRepository
    {
        IEnumerable<TransactionDynamicsViewModel> GetTransactionDynamicsByApplicationId(int applicationId);

        IEnumerable<TransactionDynamicsViewModel> GetAllTransactionDynamics();

        bool AddTransactionDynamics(TransactionDynamicsViewModel model);

        IEnumerable<TransactionDynamicsViewModel> GetTransactionDynamicsTemplate();

        bool AddTransactionDynamicsTemplate(TransactionDynamicsViewModel model);

        bool UpdateTransactionDynamicsTemplate(TransactionDynamicsViewModel model, int conditionPrecedentId);

        bool RemoveLoanTransactionDynamics(int id, UserInfo user);

        bool EditLoanTransactionDynamics(int id, TransactionDynamicsViewModel entity);
    }
}