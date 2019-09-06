using FintrakBanking.ViewModels;
using FintrakBanking.ViewModels.Setups.Credit;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FintrakBanking.Interfaces.Setups.Credit
{
    public interface IBulkDisbursementPackageRepository
    {
        // for scheme interfaces
        IEnumerable<BulkDisbursementSetupSchemeViewModel> GetAllBulkDisburseSchemeByApplicationReferenceNumber(string referenceNumber);
        IEnumerable<BulkDisbursementSetupSchemeViewModel> GetAllBulkDisbursementScheme();
        IEnumerable<BulkDisbursementSetupSchemeViewModel> GetAllBulkDisbursementSchemeByProductId(int productId);
        IEnumerable<BulkDisbursementSetupSchemeViewModel> GetAllBulkDisbursementSchemeByDisburseSchemeId(int disburseSchemeId);
        bool AddBulkDisbursementScheme(BulkDisbursementSetupSchemeViewModel model);
        bool AddMultipleBulkDisbursementScheme(List<BulkDisbursementSetupSchemeViewModel> models);
        bool UpdateBulkDisbursementScheme(int disbursementSchemeId, BulkDisbursementSetupSchemeViewModel model);
        bool DeleteBulkDisbursementScheme(int disbursementPackageId, UserInfo user);
        IEnumerable<BulkDisbursementSetupSchemeViewModel> SearchLoanApplicationDetails(int companyId, string searchQuery);
        IEnumerable<BulkDisbursementSetupSchemeViewModel> SchemeSearch(string searchString);
    }
}
