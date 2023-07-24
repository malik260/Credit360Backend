using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FintrakBanking.ViewModels;
using FintrakBanking.ViewModels.Credit;

namespace FintrakBanking.Interfaces.Media
{
    public interface IDocumentTypeRepository
    {
        DocumentTypeViewModel GetDocumentType(int id);

        IEnumerable<DocumentTypeViewModel> GetDocumentTypes();

        bool AddDocumentType(DocumentTypeViewModel model);

        bool UpdateDocumentType(DocumentTypeViewModel model, int id, UserInfo user);

        bool DeleteDocumentType(int id, UserInfo user);
    }
}
