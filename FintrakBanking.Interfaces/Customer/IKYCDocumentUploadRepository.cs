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
        CheckListDocumentUploadViewModel CheckListDocumentUploadViewModel(int definitionId, int statusId, int detailId, bool isProductBased);
        IEnumerable<CustomerDocumentUploadViewModel> GetKYCDocumentUploadByCustomerId(int customerId);
        bool KYCDocumentUpload(CustomerDocumentUploadViewModel model, byte[] file);
        bool CheckListDocumentUpload(CheckListDocumentUploadViewModel model, byte[] file);
    }
}
