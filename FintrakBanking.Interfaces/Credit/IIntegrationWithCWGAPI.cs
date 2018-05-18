using FintrakBanking.ViewModels.ThridPartyIntegration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FintrakBanking.Interfaces.Credit
{
    public interface IIntegrationWithCWGAPI
    {
        ResponseMessageViewModel OverDraftNormal(OverDraftNormalViewModel model);
        ResponseMessageViewModel OverDraftTopUp(OverDraftTopUpAndRenewViewModel model);
        ResponseMessageViewModel OverDraftExtend(OverDraftExtendViewModel model);
        ResponseMessageViewModel OverDraftRenew(OverDraftTopUpAndRenewViewModel model);

        ResponseMessageViewModel TemporaryOverDraftNormal(TemporaryOverDraftViewModel model);
        ResponseMessageViewModel TemporaryOverDraftRunning(TemporaryOverDraftViewModel model);
        ResponseMessageViewModel TemporaryOverDraftSingle(TemporaryOverDraftViewModel model);

        bool GetExposePersonStatus(string customerCode);
        BVNCustomerDetailsViewModel BVNCustomerDetails(string customerCode);
        GLAccountDetailsViewModel ValidateGLNumber(string glNumber);
        TDAccountRecordViewModel ValidateTDAccountNumber(string teamDepositAccountNumber);
    }
}
