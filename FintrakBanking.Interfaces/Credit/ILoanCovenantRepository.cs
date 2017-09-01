using FintrakBanking.ViewModels;
using FintrakBanking.ViewModels.Credit;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;


namespace FintrakBanking.Interfaces.Customer
{
    public interface ILoanCovenantRepository
    {
        #region Loan Covenant Detail
        Task<int> AddMultipleLoanCovenantDetail(List<LoanCovenantDetailViewModel> covenantModel);
        Task<bool> AddLoanCovenantDetail(LoanCovenantDetailViewModel entity);
        Task<bool> DeleteLoanCovenantDetail(int loanCovenantDetailId, UserInfo user );
        Task<bool> UpdateLoanCovenantDetail(int loanCovenantDetailId, LoanCovenantDetailViewModel entity);
       // IEnumerable<LoanCovenantDetailViewModel> GetLoanCovenantDetail(int companyId);
        IEnumerable<LoanCovenantDetailViewModel> GetLoanCovenantDetailByCovenantType(int covenantTypeId, int companyId);
        IEnumerable<LoanCovenantDetailViewModel> GetLoanCovenantDetailByloanId(int loanId, int companyId);
        IEnumerable<LoanCovenantTypeViewModel> GetLoanCovenantDetailById(int covenantDetailId, int companyId);
        #endregion Loan Covenant Detail

        #region Loan Covenant Type
        Task<bool> AddLoanCovenantType(LoanCovenantTypeViewModel entity);         
        Task<bool> UpdateLoanCovenantType(short loanCovenantTypeId, LoanCovenantTypeViewModel entity);
        IEnumerable<LoanCovenantTypeViewModel> GetLoanCovenantType(int companyId);


        #endregion Loan Covenant Detail
    }
}
