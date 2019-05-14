using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FintrakBanking.ViewModels;
using FintrakBanking.ViewModels.Credit;

namespace FintrakBanking.Interfaces.Credit
{
    public interface ITermSheetRepository
    {
        TermSheetViewModel GetTermSheet(int id);

        IEnumerable<TermSheetViewModel> GetTermSheets();

        IEnumerable<LookupViewModel> GetCustomerTermSheets(int customerId);

        bool AddTermSheet(TermSheetViewModel model);

        bool UpdateTermSheet(TermSheetViewModel model, int id, UserInfo user);

        bool DeleteTermSheet(int id, UserInfo user);
    }
}