using FintrakBanking.ViewModels;
using FintrakBanking.ViewModels.Setups.Credit;
using FintrakBanking.ViewModels.ThridPartyIntegration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FintrakBanking.Interfaces.Setups.Credit
{
   public interface IFXAccountCreationRepository
    {
        FXAccountCreationListViewModel GetListOfFSCode();
        List<FXAccountCreationViewModel> GetAllGLSubHead(string schemeCode);
        string ForeignCurrencyAccountCreation(CreateAccountViewModel entity, UserInfo user);
    }
}
