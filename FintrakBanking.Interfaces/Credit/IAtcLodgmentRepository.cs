using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FintrakBanking.ViewModels;
using FintrakBanking.ViewModels.credit;

namespace FintrakBanking.Interfaces.credit
{
    public interface IAtcLodgmentRepository
    {
        AtcLodgmentViewModel GetAtcLodgment(int id);

        IEnumerable<AtcLodgmentViewModel> GetAtcLodgments();

        IEnumerable<AtcLodgmentViewModel> GetAtcLodgmentForRelease();

        bool AddAtcLodgment(AtcLodgmentViewModel model);

        bool UpdateAtcLodgment(AtcLodgmentViewModel model, int id, UserInfo user);

        bool DeleteAtcLodgment(int id, UserInfo user);

        IEnumerable<AtcLodgmentViewModel> GetAtcType();

        //IEnumerable<AtcReleaseViewModel> GetAtcRelease(int id);
        AtcReleaseViewModel GetAtcRelease(int id);

        bool AddAtcRelease(IEnumerable <AtcReleaseViewModel> model);

        bool DeleteAtcRelease(int id, UserInfo user);

        IEnumerable<AtcLodgmentViewModel> GetAtcLodgmentForApproval(int staffId);

        bool SubmitApproval(AtcReleaseViewModel model);

        IEnumerable<AtcLodgmentViewModel> GetAtcReleaseForApproval(int staffId);

        bool SubmitLodgementApproval(AtcLodgmentViewModel model);
        bool AddAtcType(AtcTypeViewModel model);
        bool DeleteAtcType(int id, UserInfo user);
        IEnumerable<AtcReleaseViewModel> GetAtcLodgmentForReleaseList();
    }
}
