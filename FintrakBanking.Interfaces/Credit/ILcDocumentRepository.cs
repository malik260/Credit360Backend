using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using FintrakBanking.ViewModels.credit;
using FintrakBanking.ViewModels;

namespace FintrakBanking.Interfaces.credit
{
    public interface ILcDocumentRepository
    {
        LcDocumentViewModel GetLcDocument(int id);

        IEnumerable<LcDocumentViewModel> GetLcDocuments();

        IEnumerable<LcDocumentViewModel> GetLcDocumentsBylcIssuanceId(int lcIssuanceId);

        bool AddLcDocument(LcDocumentViewModel model);

        bool UpdateLcDocument(LcDocumentViewModel model, int id, UserInfo user);

        bool DeleteLcDocument(int id, UserInfo user);
    }
}
