using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FintrakBanking.ViewModels;
using FintrakBanking.ViewModels.Credit;

namespace FintrakBanking.Interfaces.Media
{
    public interface IDocumentCategoryTypeRepository
    {
        DocumentCategoryTypeViewModel GetDocumentCategoryType(int id);

        IEnumerable<DocumentCategoryTypeViewModel> GetDocumentCategoryTypes();

        bool AddDocumentCategoryType(DocumentCategoryTypeViewModel model);

        bool UpdateDocumentCategoryType(DocumentCategoryTypeViewModel model, int id, UserInfo user);

        bool DeleteDocumentCategoryType(int id, UserInfo user);
    }
}
