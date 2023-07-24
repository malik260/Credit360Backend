using FintrakBanking.ViewModels.Credit;
using FintrakBanking.ViewModels.Reports;
using FintrakBanking.ViewModels.Setups.General;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FintrakBanking.Interfaces.CRMS
{
    public interface ICRMSRegulatories
    {
        string GetCRMSCode(CRMSViewModel code);
        bool ResetCrmsCode(CRMSViewModel code);
        string AddCRMSCode(CRMSViewModel code);
        List<CRMSRegulatoryViewModel> GetAllLoansForCRMS(CRMSViewModel data);
        CRMSRecord GenerateCBNReport(List<CRMSViewModel> paramx);
        CRMSRecord GenerateCBNReportByLoanAppId(List<CRMSViewModel> paramx);

        List<LoansCount> LoanCountsByLegalStatus(List<CRMSRegulatoryViewModel> loans);
        CRMSRecord GenerateBatchPosting(DateRange model);

        bool GenerateCRMSCodes(List<int> loanBookingRequestIds, UserViewModel model);
        bool GenerateCRMSCode(int loanBookingRequestId, UserViewModel model);

    }
}
