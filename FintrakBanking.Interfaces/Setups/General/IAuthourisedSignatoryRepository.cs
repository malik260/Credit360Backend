using FintrakBanking.ViewModels.Setups.General;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FintrakBanking.ViewModels;

namespace FintrakBanking.Interfaces.Setups.General
{
   public interface IAuthourisedSignatoryRepository
    {
     
        AuthorisedSignatoryViewModel GetSignatory(int id);
        IEnumerable<AuthorisedSignatoryViewModel> GetSignatories();
        bool AddSignatory(AuthorisedSignatoryViewModel model);
        bool DeleteSignatory(int id, UserInfo user);
        bool UpdateSignatory(AuthorisedSignatoryViewModel model, int id, UserInfo user);
    }
}
