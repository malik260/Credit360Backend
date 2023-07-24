using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using FintrakBanking.ViewModels.credit;
using FintrakBanking.ViewModels;

namespace FintrakBanking.Interfaces.credit
{
    public interface ILcShippingRepository
    {
        LcShippingViewModel GetLcShipping(int id);

        IEnumerable<LcShippingViewModel> GetLcShippings();

        IEnumerable<LcShippingViewModel> GetLcShippingsByIssuanceId(int lcIssuanceId);

        bool AddLcShipping(LcShippingViewModel model);

        bool UpdateLcShipping(LcShippingViewModel model, int id, UserInfo user);

        bool DeleteLcShipping(int id, UserInfo user);
    }
}
