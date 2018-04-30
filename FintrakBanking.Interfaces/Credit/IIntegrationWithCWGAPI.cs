using FintrakBanking.ViewModels.ThridPartyIntegration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FintrakBanking.Interfaces.Credit
{
   public  interface IIntegrationWithCWGAPI
    {
        OverdraftResponseViewModel OverDraftNormal(OverDraftNormalViewModel model);
        OverdraftResponseViewModel OverDraftTopUp(OverDraftTopUpViewModel model);
        OverdraftResponseViewModel OverDraftExtend(OverDraftExtendViewModel model);
        
        OverdraftResponseViewModel TemporaryOverDraftNormal(TemporaryOverDraftViewModel model);
        OverdraftResponseViewModel TemporaryOverDraftRunning(TemporaryOverDraftViewModel model);
        OverdraftResponseViewModel TemporaryOverDraftSingle(TemporaryOverDraftViewModel model);
    }
}
