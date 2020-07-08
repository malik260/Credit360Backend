using FintrakBanking.Finance.ViewModels;
using FintrakBanking.ViewModels.Credit;
using FintrakBanking.ViewModels.Report;
using FintrakBanking.ViewModels.Reports;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FintrakBanking.Interfaces.Reports
{
    public interface IFinanceTransactionsReport
    {
        IEnumerable<dynamic> PostTransactionsByStaffByDate(DateRange dateItem, int companyId);
        IEnumerable<dynamic> PostTransactionsByBranchByDate(DateRange dateItem, int companyId);
        List<DailyAccrualViewModel> GetAllDailyAccrualCategories();
        List<DailyAccrualViewModel> GetAllLoanTransactionType();
        List<LoanOperationTypeViewModel> Operations();
    }
}
