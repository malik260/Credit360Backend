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

        IEnumerable<DocumentUploadViewModel> GetDocumentUploads();

        bool AddDocumentUpload(DocumentUploadViewModel model, byte[] buffer);

        bool UpdateDocumentUpload(DocumentUploadViewModel model, int id, UserInfo user);

        bool DeleteDocumentUpload(int id, UserInfo user);
    }
}
