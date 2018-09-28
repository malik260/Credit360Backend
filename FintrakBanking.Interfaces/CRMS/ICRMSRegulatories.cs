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
        List<CRMSRegulatoryViewModel> GetAllLoansForCRMS(CRMSViewModel data);
        CRMSRecord GenerateCBNReport(CRMSViewModel param);
        List<LoansCount> LoanCountsByLegalStatus(List<CRMSRegulatoryViewModel> loans);
    }
}
