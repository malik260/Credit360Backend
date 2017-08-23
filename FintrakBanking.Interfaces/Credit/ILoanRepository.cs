
using FintrakBanking.Common.Enum;
using FintrakBanking.ViewModels;
using FintrakBanking.ViewModels.Credit;
using FintrakBanking.ViewModels.Setups.Credit;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FintrakBanking.Interfaces.Credit
{
    public interface ILoanRepository
    {
        IEnumerable<LookupViewModel> GetAllLoanTypes();

        string AddLoanBooking(LoanViewModel entity);

        IEnumerable<LoanViewModel> GetLoanByCustomer(int customerId);

        LoanViewModel GetLoan(int loanId);
        
        IEnumerable<LoanViewModel> FindLoan(string referenceNumberOrName, int companyId);

        IEnumerable<LoanViewModel> LoanSearch(int companyId, LoanSearchViewModel searchModel);

        IEnumerable<LoanViewModel> GetLoanByCustomerGroup(int customerGroupId);
    }
}