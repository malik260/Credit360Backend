using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FintrakBanking.ViewModels;
using FintrakBanking.ViewModels.Credit;

namespace FintrakBanking.Interfaces.Media
{
    public interface IDocumentCategoryRepository
    {
        DocumentCategoryViewModel GetDocumentCategory(int id);
        IEnumerable<DocumentCategoryViewModel> GetDocumentCategorys();

        bool AddDocumentCategory(DocumentCategoryViewModel model);

        bool UpdateDocumentCategory(DocumentCategoryViewModel model, int id, UserInfo user);

        bool DeleteDocumentCategory(int id, UserInfo user);
    }
}
