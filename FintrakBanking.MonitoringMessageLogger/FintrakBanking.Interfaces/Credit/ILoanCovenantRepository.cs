using FintrakBanking.ViewModels;
using FintrakBanking.ViewModels.Credit;
using FintrakBanking.ViewModels.Finance;
using FintrakBanking.ViewModels.Setups;
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
        bool AddLoanCovenantDetail(LoanCovenantDetailViewModel entity);
        Task<bool> DeleteLoanCovenantDetail(int loanCovenantDetailId, UserInfo user );
        Task<bool> UpdateLoanCovenantDetail(int loanCovenantDetailId, LoanCovenantDetailViewModel entity);
        IEnumerable<LoanCovenantDetailViewModel> GetLoanCovenantDetailByCovenantType(int covenantTypeId, int companyId);
        IEnumerable<LoanCovenantDetailViewModel> GetLoanCovenantDetailByloanId(int loanId, int companyId);
        IEnumerable<LoanCovenantTypeViewModel> GetLoanCovenantDetailById(int covenantDetailId, int companyId);

       bool DeleteLoanApplicationCovenant(int covenantId, UserInfo user);
       bool AddLoanApplicationCovenant(LoanCovenantDetailViewModel entity);
        IEnumerable<LoanCovenantDetailViewModel> GetLoanApplicationCovenant(int applicationId);
        IEnumerable<LoanCovenantDetailViewModel> GetLoanApplicationDetailCovenant(int applicationDetailId);
        //bool UpdateLoanApplicationCovenant(DateTime date);
        bool UpdateLoanApplicationCovenant(DateTime date, int companyId, int staffId, out string transactionReferenceNo);
        DateTime GetFrequencyDate(int frequencyTypeId, DateTime date);

        #endregion Loan Covenant Detail

        #region Loan Covenant Type
        Task<bool> AddLoanCovenantType(LoanCovenantTypeViewModel entity);         
        Task<bool> UpdateLoanCovenantType(short loanCovenantTypeId, LoanCovenantTypeViewModel entity);
        IEnumerable<LoanCovenantTypeViewModel> GetLoanCovenantType(int companyId);

        IEnumerable<LoanCovenantDetailViewModel> GetLoanApplicationCovenantLms(int id);
        bool AddLoanApplicationCovenantLms(LoanCovenantDetailViewModel entity);
        bool DeleteLoanApplicationCovenantLms(int id, UserInfo user);


        #endregion Loan Covenant Detail
    }
}
