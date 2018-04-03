using FintrakBanking.ViewModels.Credit;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FintrakBanking.Interfaces.Credit
{
    public interface ICollateralDocumentRepository
    {
        CollateralDocumentViewModel GetCollateralDocument(int documentId);

        IEnumerable<CollateralDocumentViewModel> GetAllCollateralDocument();

        IEnumerable<CollateralDocumentViewModel> GetCustomerCollateralDocument(int collateralId);

        bool AddCollateralDocument(CollateralDocumentViewModel model, byte[] file);

        bool UpdateCollateralDocument(CollateralDocumentViewModel model, int documentId);

        bool AddCollateralVisitation(CollateralDocumentViewModel model, byte[] file);

        CollateralVisitationDocumentViewModel GetCollateralVisitationDocument(int documentId);
    }
}
