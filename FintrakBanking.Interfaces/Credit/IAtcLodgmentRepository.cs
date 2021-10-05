using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FintrakBanking.Interfaces.WorkFlow;
using FintrakBanking.ViewModels;
using FintrakBanking.ViewModels.credit;

namespace FintrakBanking.Interfaces.credit
{
    public interface IAtcLodgmentRepository
    {
        AtcLodgmentViewModel GetAtcLodgment(int id);

        IEnumerable<AtcLodgmentViewModel> GetAtcLodgments(int staffId);

        IEnumerable<AtcLodgmentViewModel> GetAtcLodgmentForRelease();

        bool AddAtcLodgment(AtcLodgmentViewModel model);


        bool UpdateAtcLodgment(AtcLodgmentViewModel model, int id, UserInfo user);

        bool DeleteAtcLodgment(int id, UserInfo user);

        IEnumerable<AtcLodgmentViewModel> GetAtcType();

        //IEnumerable<AtcReleaseViewModel> GetAtcRelease(int id);
        AtcReleaseViewModel GetAtcRelease(int id);

        WorkflowResponse AddAtcRelease(IEnumerable <AtcReleaseViewModel> model);

        bool DeleteAtcRelease(int id, UserInfo user);

        IEnumerable<AtcLodgmentViewModel> GetAtcLodgmentForApproval(int staffId);

        Tuple<WorkflowResponse, int> SubmitApproval(IEnumerable<AtcReleaseViewModel> model);

        IEnumerable<AtcLodgmentViewModel> GetAtcReleaseForApproval(int staffId);

        WorkflowResponse SubmitLodgementApproval(AtcLodgmentViewModel model);
        bool AddAtcType(AtcTypeViewModel model);
        bool DeleteAtcType(int id, UserInfo user);
        IEnumerable<AtcLodgmentViewModel> GetAtcLodgmentForReleaseList(int staffId);
        bool atclodgeApproval(AtcLodgmentViewModel model);
        WorkflowResponse AtclodgmentApproval(IEnumerable<AtcLodgmentViewModel> model);
        bool SaveEditedATCRelease(AtcReleaseViewModel model, int id);
        IEnumerable<AtcLodgmentViewModel> GetAtcLodgmentsByCustomerId(int customerId);
        WorkflowResponse SubmitReferredAtcBackIntoWorkflow(AtcReleaseViewModel model);
    }
}
