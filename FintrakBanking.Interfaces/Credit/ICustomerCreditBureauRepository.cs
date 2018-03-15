using FintrakBanking.ViewModels.Credit;
using FintrakBanking.ViewModels.Customer;
using FintrakBanking.ViewModels.ThridPartyIntegration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FintrakBanking.Interfaces.Credit
{
    public interface ICustomerCreditBureauRepository
    {
        #region CREDIT BUREAU REPORT
        IEnumerable<CustomerViewModels> GetCreditBureauCustomerDetailsByCustomerId(int customerId);

        int AddCustomerCreditBureauCharge(LoanCreditBereauViewModel entity);
        int AddCustomerCreditBureauUpload(LoanCreditBereauViewModel entity, LoanDocumentViewModel docModel, byte[] file);

        bool UpdateCreditBureauCustomerReportStatus(bool status, LoanCreditBereauViewModel model);

        bool UpdateMultipleCreditBureauCustomerReportStatus(bool status, List<LoanCreditBereauViewModel> model);

        IEnumerable<CreditBereauViewModel> GetCreditBureauInformation();

        List<LoanCreditBereauViewModel> GetCustomerCreditBureauReportLog(int customerId, int? companyDirectorId);

        #endregion

        #region Integration
        List<string> GetCustomerCreditMatch(CreditBureauSearchViewModel searchInfoList);

        byte[] GetFullSearchResultInPDF(SearchInput searchInput);
        #endregion
    }
}
