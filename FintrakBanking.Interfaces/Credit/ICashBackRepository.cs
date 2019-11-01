using FintrakBanking.ViewModels;
using FintrakBanking.ViewModels.Credit;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FintrakBanking.Interfaces.Credit
{
  public interface ICashBackRepository
    {
        IEnumerable<CashBackViewModel> GetCashbackSectionByApplicationDetailId(int loanApplicationDetailId);
        bool AddCashbackSection(CashBackViewModel model);
        bool UpdateCashbackSection(int id, CashBackViewModel model, UserInfo user);
    }
}
