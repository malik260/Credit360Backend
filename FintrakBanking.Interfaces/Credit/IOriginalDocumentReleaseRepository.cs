using FintrakBanking.ViewModels.Credit;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FintrakBanking.Interfaces.Credit
{
    public interface IOriginalDocumentReleaseRepository
    {
        //OriginalDocumentReleaseViewModel AddOriginalDocumentRelease(OriginalDocumentReleaseViewModel model);
        //OriginalDocumentReleaseViewModel GetOriginalDocmentReleaseById(int id);
        bool AddOriginalDocumentRelease(IEnumerable<OriginalDocumentReleaseViewModel> model);
        IEnumerable<OriginalDocumentReleaseViewModel> GetOriginalAllDocmentRelease(int id);
        bool saveChanges();

        IEnumerable<OriginalDocumentReleaseViewModel> GetLeaseDocumentForApproval(int staffId);

        bool GoForApproval(IEnumerable<OriginalDocumentReleaseViewModel> model);
    }
}
