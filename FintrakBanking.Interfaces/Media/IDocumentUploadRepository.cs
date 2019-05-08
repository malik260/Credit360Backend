using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FintrakBanking.ViewModels;
using FintrakBanking.ViewModels.Credit;
using FintrakBanking.ViewModels.Setups.General;

namespace FintrakBanking.Interfaces.Media
{
    public interface IDocumentUploadRepository
    {
        DocumentUploadViewModel GetDocumentUpload(int id);

        IEnumerable<DocumentUploadViewModel> GetDocumentUploads(int staffId);

        int AddDocumentUpload(DocumentUploadViewModel model, byte[] buffer);

        bool UpdateDocumentUpload(DocumentUploadViewModel model, int id, UserInfo user);

        bool DeleteDocumentUpload(int id, UserInfo user);
        DocumentUploadViewModel GetDocument(int documentId);
        IEnumerable<DocumentUploadViewModel> GetDocumentUploads(int getStaffId, int operationId, int targetId);
        IEnumerable<DocumentCategoryViewModel> GetDocumentCategories();
        IEnumerable<DocumentTypeViewModel> GetDocumentTypes(int id);
        CustomerDocumentSearchViewModel GetCustomerDocuments(DocumentUploadViewModel model, UserInfo user);
        //DocumentUploadViewModel GetUploadedDocument(DocumentUploadViewModel model);
    }
}
