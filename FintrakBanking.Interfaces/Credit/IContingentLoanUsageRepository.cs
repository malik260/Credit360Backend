using FintrakBanking.ViewModels.Credit;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FintrakBanking.Interfaces.Credit
{
    public interface IContingentLoanUsageRepository
    {
        IEnumerable<ContingentLoansViewModel> GetAllContingentLoans(int staffId, int companyId);

        bool SaveContigentLoans(ContingentLoanUsageViewModel entity);

        IEnumerable<ContingentLoansViewModel> GetPendingRequest(int staffId);

        bool SaveContigentLoansUsageApproval(ApproveAPSRequestViewModel entity);

        List<ContingentLoansViewModel> GetContingentUsage(int loanId);

        List<CollateralDocumentViewModel> GetContingentUsageDocument(int loanId);
    }
}
