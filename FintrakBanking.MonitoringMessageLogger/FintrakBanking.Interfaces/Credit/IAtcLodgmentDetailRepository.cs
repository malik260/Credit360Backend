using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FintrakBanking.ViewModels;
using FintrakBanking.ViewModels.credit;

namespace FintrakBanking.Interfaces.credit
{
    public interface IAtcLodgmentDetailRepository
    {
        IEnumerable<AtcLodgmentDetailViewModel> GetAtcLodgmentDetail(int id);

        bool AddAtcLodgmentDetail(AtcLodgmentDetailViewModel model);

        bool DeleteAtcLodgmentDetail(int id, UserInfo user);
    }
}
