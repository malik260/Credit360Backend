
using FintrakBanking.ViewModels.Credit;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FintrakBanking.Interfaces.Credit
{
    public interface ICRMSRegulatories
    {
        string AddCRMSCode(CRMSViewModel code);
        List<CRMSRegulatoryViewModel> GetAllLoansWithCRMSCode(CRMSViewModel data);
        //byte[] GenerateExportTemplate(CRMS300TemplateViewModel loanInput);
        byte[] GenerateCRMS300Template(CRMSViewModel param);
    }
}
