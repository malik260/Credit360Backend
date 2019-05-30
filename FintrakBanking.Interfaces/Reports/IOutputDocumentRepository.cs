using FintrakBanking.ViewModels.Report;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FintrakBanking.Interfaces.Reports
{
   public interface IOutputDocumentRepository
    {
        IEnumerable<OutPutDocumentFeeViewModel> GetFee(int loanApplicationId);
        IEnumerable<OutPutDocumentMonthActivitySignViewModel> MonthActivitySignature(int loanApplicationId);
        IEnumerable<OutPutDocumentApprovalViewModel> GetApplicationApproval(int loanApplicationId);
        IEnumerable<OutPutDocumentChecklistViewModel> GetChecklist(int loanApplicationId);
        IEnumerable<OutPutDocumentCollateralViewModel> GetCollateral(int loanApplicationId);
        IEnumerable<OutPutDocumentConcurrencesViewModel> GetConcurrences(int loanApplicationId);
        IEnumerable<OutPutDocumentMonthsActivityViewModel> GetMonthsActivity(int loanApplicationId);
        IEnumerable<OutPutDocumentCustomerFacilitiesViewModel> GetCustomerFacilities(int loanApplicationId);
        IEnumerable<OutPutDocumentCustomerInformationViewModel> GetCustomerInformation(int loanApplicationId);
    }
}
