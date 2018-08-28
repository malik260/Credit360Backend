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
        IEnumerable<CustomerViewModels> GetCreditBureauCustomerDetailsByCustomerId(int customerId, bool isExternal);

        bool VerifyCustomerValidCreditBureau(int customerId);
        int AddCustomerCreditBureauCharge(LoanCreditBureauViewModel entity);
        int AddCustomerCreditBureauUpload(LoanCreditBureauViewModel entity, LoanDocumentViewModel docModel, byte[] file);

        bool UpdateCreditBureauCustomerReportStatus(bool status, LoanCreditBureauViewModel model);

        bool UpdateMultipleCreditBureauCustomerReportStatus(bool status, List<LoanCreditBureauViewModel> model);

        IEnumerable<CreditBereauViewModel> GetCreditBureauInformation();

        IEnumerable<CRCBureauFacilityViewModel> GetCRCBureauFacilities();

        List<LoanCreditBureauViewModel> GetCustomerCreditBureauReportLog(int customerId, int? companyDirectorId);

        #endregion

        #region Integration
        XDSSearchResult GetCustomerXDSCreditMatch(CreditBureauSearchViewModel searchInfoList);

        XDSSearchResult GetXDSFullSearchResultInPDF(SearchInput searchInput);

        byte[] GetCRCFullCreditMergeReport(MultiHitRequestViewModel request);

        CRCSearchResult GetCustomerCRCCreditMatch(CRCRequestViewModel searchInfo);
        #endregion
    }
}
