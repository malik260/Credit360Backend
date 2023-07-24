using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FintrakBanking.ViewModels;
using FintrakBanking.ViewModels.Credit;

namespace FintrakBanking.Interfaces.Media
{
    public interface IDocumentUsageRepository
    {
        DocumentUsageViewModel GetDocumentUsage(int id);

        IEnumerable<DocumentUsageViewModel> GetDocumentUsages();

        bool AddDocumentUsage(DocumentUsageViewModel model);

        bool UpdateDocumentUsage(DocumentUsageViewModel model, int id, UserInfo user);

        bool DeleteDocumentUsage(int id, UserInfo user);

        IEnumerable<DocumentUsageViewModel> SearchDocumentUsage(string parameter);
    }
}
