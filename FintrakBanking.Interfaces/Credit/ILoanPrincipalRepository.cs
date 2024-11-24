using FintrakBanking.ViewModels.Credit;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FintrakBanking.Interfaces.Credit
{
    public interface ILoanPrincipalRepository
    {
        IEnumerable<LoanPrincipalViewModel> GetLoanPrincipal(int companyId);

        LoanPrincipalViewModel GetLoanPrincipal(int principalId, int companyId);

        string AddLoanPrincipal(LoanPrincipalViewModel loanPrincipal);

        string UpdateLoanPrincipal(LoanPrincipalViewModel loanPrincipal);

        string DeleteLoanPrincipal(LoanPrincipalViewModel loanPrincipal);
        List<LoanPrincipalViewModel> GetLoanPrincipal();
    }
}
