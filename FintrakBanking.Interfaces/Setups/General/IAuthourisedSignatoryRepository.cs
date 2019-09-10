using FintrakBanking.ViewModels.Setups.General;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FintrakBanking.ViewModels;

namespace FintrakBanking.Interfaces.Setups.General
{
   public interface IAuthourisedSignatoriesRepository
    {
     
        AuthourisedSignatoryViewModel GetSignatoryName(int id);
        IEnumerable<AuthourisedSignatoryViewModel> GetSignatories();
        bool AddSignatory(AuthourisedSignatoryViewModel model);
        bool DeleteSignatory(AuthourisedSignatoryViewModel model);
        bool DeleteSignatory(int id, UserInfo user);
        bool UpdateSignatory(AuthourisedSignatoryViewModel model, int id, UserInfo user);
    }
}
