using FintrakBanking.ViewModels.ThridPartyIntegration;
using System.Threading.Tasks;

namespace ThirdPartyIntegration
{
    public interface ILoanPrepayment
    {
        Task<MainResponseLoanPrepaymentViewModel> GetTodayRepaymentLoans(LoanPrepaymentViewModel model);
        Task<MainResponseLoanPrepaymentViewModel> GetTodayLoanRepaymentByRefNo(LoanPrepaymentViewModel model);
        Task<MainResponseLoanPrepaymentViewModel> GetTodayLoanSumRepaymentByRefNo(LoanPrepaymentViewModel model);
        Task<ResponseLoanPrepaymentViewModel> GetOverdraftRepayment(LoanPrepaymentViewModel model);
        void GetRepaymentEntriesToStaging();
    }
}