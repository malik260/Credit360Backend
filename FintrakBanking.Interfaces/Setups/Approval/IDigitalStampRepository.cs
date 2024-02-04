using FintrakBanking.ViewModels;
using FintrakBanking.ViewModels.Setups.Approval;
using FintrakBanking.ViewModels.WorkFlow;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FintrakBanking.Interfaces.Setups.Approval
{
    public interface IDigitalStampRepository
    {
        IEnumerable<DigitalStampViewModel> GetAllDigitalStamp();
        bool AddDigitalStamp(DigitalStampViewModel model, byte[] buffer);
        bool UpdateDigitalStamp(int digitalStampid, DigitalStampViewModel model, byte[] buffer);
        bool DeleteDigitalStamp(int digitalStampid, UserInfo user);
        IEnumerable<DigitalStampViewModel> GetDigitalStampByApprovalLevel(int approvalLevelId);

    }
}
