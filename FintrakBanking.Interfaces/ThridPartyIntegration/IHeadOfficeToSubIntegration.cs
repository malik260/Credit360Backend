using FintrakBanking.ViewModels.Credit;
using FintrakBanking.ViewModels.ThridPartyIntegration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FintrakBanking.Interfaces.ThridPartyIntegration
{
    public interface IHeadOfficeToSubIntegration
    {
        PostingResult PostFacilityApprovalToSubnputs(ForwardViewModel model);
    }
}
