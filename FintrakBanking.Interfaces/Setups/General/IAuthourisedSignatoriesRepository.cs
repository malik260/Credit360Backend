using FintrakBanking.ViewModels.Setups.General;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FintrakBanking.Interfaces.Setups.General
{
   public interface IAuthourisedSignatoriesRepository
    {
     
        AuthourisedSignatoriesViewModel GetSignatoryName(int id);
        IEnumerable<AuthourisedSignatoriesViewModel> GetSignatories();
        bool AddSignatory(AuthourisedSignatoriesViewModel model);
        bool DeleteSignatory(AuthourisedSignatoriesViewModel model);
   
    }
}
