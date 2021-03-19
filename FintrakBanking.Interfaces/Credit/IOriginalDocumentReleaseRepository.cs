using FintrakBanking.Interfaces.WorkFlow;
using FintrakBanking.ViewModels.Credit;
using FintrakBanking.ViewModels.Setups.General;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FintrakBanking.Interfaces.Credit
{
    public interface IOriginalDocumentReleaseRepository
    {
        IEnumerable<CollateralCashReleaseViewModel> GetCashSecurityReleaseSearch(string searchString);
        WorkflowResponse SubmitCashSecurityReleaseApproval(CollateralCashReleaseViewModel model);
        IEnumerable<CollateralCashReleaseViewModel> GetRejectedAndReferredCashSecurityRelease(int staffId);
        IEnumerable<CollateralCashReleaseViewModel> GetCashSecurityReleaseForApproval(int staffId);
        WorkflowResponse GoForGuaranteeCashApproval(CollateralCashReleaseViewModel entity);
        IEnumerable<OriginalDocumentReleaseViewModel> GetSecurityReleaseSearch(string searchString);
        bool AddOriginalDocumentGuaranteeRelease(IEnumerable<OriginalDocumentReleaseViewModel> model);
        WorkflowResponse GoForGuaranteeApproval(IEnumerable<OriginalDocumentReleaseViewModel> entity);
        //OriginalDocumentReleaseViewModel AddOriginalDocumentRelease(OriginalDocumentReleaseViewModel model);
        //OriginalDocumentReleaseViewModel GetOriginalDocmentReleaseById(int id);
        bool AddOriginalDocumentRelease(IEnumerable<OriginalDocumentReleaseViewModel> model);
        IEnumerable<OriginalDocumentReleaseViewModel> GetOriginalAllDocmentRelease(int id);
        bool saveChanges();

        IEnumerable<OriginalDocumentReleaseViewModel> GetLeaseDocumentForApproval(int staffId);

        WorkflowResponse GoForApproval(IEnumerable<OriginalDocumentReleaseViewModel> model);

        WorkflowResponse SubmitApproval(OriginalDocumentReleaseViewModel model);

        bool UpdateOriginalDocumentRelease(OriginalDocumentReleaseViewModel mod);

        IEnumerable<OriginalDocumentReleaseViewModel> GetRejectedAndReferredSecurityRelease(int staffId);
        bool reinitiateSecurityRelease(int id, int staffId, int companyId);
        IEnumerable<DocumentUploadViewModel> GetReleasedDocUploadIds(int operationId, int targetId, int staffId);
        IEnumerable<DocumentUploadViewModel> GetAvailableDocumentsForReleease(int operationId, int targetId, int staffId);
    }
}
