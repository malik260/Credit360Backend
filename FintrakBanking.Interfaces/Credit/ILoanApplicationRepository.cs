using FintrakBanking.Common;
using FintrakBanking.Common.Enum;
using FintrakBanking.ViewModels;
using FintrakBanking.ViewModels.WorkFlow;
using FintrakBanking.ViewModels.Credit;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;


namespace FintrakBanking.Interfaces.Credit
{
    public interface ILoanApplicationRepository
    {

        IEnumerable<LoanApplicationViewModel> GetAllLoanApplications(int companyId);

        IEnumerable<LoanApplicationViewModel> GetLoanApplicationJobs(int companyId, int level, int scope);

        IEnumerable<LoanApplicationViewModel> GetLoanApplicationById(int loanApplicationId, int companyId);

        IEnumerable<ProductClassViewModel> GetProductClass();

        //string AddLoanApplication(LoanApplicationViewModel entity);

        IEnumerable<LoanApplicationViewModel> FindLoanApplication(string referenceNumberOrName, int companyId);

         bool ApprovalOperation(ApprovalViewModel approval);

        Task<bool> UpdateApprovalStatus(ApprovalViewModel entity);

        Task<bool> AddLoanApplication(LoanApplicationViewModel loan);

    }
}
