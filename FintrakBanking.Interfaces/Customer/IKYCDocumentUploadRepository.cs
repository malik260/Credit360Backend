using FintrakBanking.ViewModels.Customer;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FintrakBanking.Interfaces.Customer
{
   public interface IKYCDocumentUploadRepository
    {
      
        IEnumerable<CustomerDocumentUploadViewModel> GetKYCDocumentUploadByCustomerId(int customerId);
        bool KYCDocumentUpload(CustomerDocumentUploadViewModel model, byte[] file);

        CheckListDocumentUploadViewModel CheckListDocumentUploadViewModel(int definitionId, int statusId, int detailId, bool isProductBased);
        bool CheckListDocumentUpload(CheckListDocumentUploadViewModel model, byte[] file);
        bool RemoveCheckListDocument(int definitionId, int statusId, int detailId, bool isProductBased);

        ConditionsPrecedentUploadViewModel GetLoanConditionDocumentBydocumentId(int documentId);
        IEnumerable<ConditionsPrecedentUploadViewModel> GetLoanConditionDocumentByContionId(int conditionId);
        bool ConditionsPrecedentDocumentUpload(ConditionsPrecedentUploadViewModel model, byte[] file);
    }
}
