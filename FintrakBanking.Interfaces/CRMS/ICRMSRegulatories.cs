using FintrakBanking.ViewModels.Credit;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FintrakBanking.Interfaces.CRMS
{
    public interface ICRMSRegulatories
    {
        string AddCRMSCode(CRMSViewModel code);
        List<CRMSRegulatoryViewModel> GetAllLoansWithCRMSCode(CRMSViewModel data);
        List<CRMSRecord> GenerateCBNReport(CRMSViewModel param);

    }
}
