using FintrakBanking.ViewModels.CASA;
using FintrakBanking.ViewModels.Finance;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FintrakBanking.Interfaces.CASA
{
    public interface ICasaLienRepository
    {
        string PlaceLien(CasaLienViewModel model, TwoFactorAutheticationViewModel twoFADetails = null);
        bool ReleaseLien(CasaLienViewModel model, TwoFactorAutheticationViewModel twoFADetails = null, bool require2FA = true);

    }
}
