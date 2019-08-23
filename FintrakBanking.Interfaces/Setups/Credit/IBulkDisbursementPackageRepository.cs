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
        IEnumerable<BulkDisbursementSetupPackageViewModel> GetAllBulkDisbursementPackageByGroupCustomerId(int groupCustomerId);
        IEnumerable<BulkDisbursementSetupPackageViewModel> GetAllBulkDisbursementPackageByCompany();
        IEnumerable<BulkDisbursementSetupPackageViewModel> GetBulkDisbursementPackageById(int disbursementPackageId);
        bool AddMultipleBulkDisbursementPackage(List<BulkDisbursementSetupPackageViewModel> models);
        bool UpdateBulkDisbursementPackage(int disbursementPackageId, BulkDisbursementSetupPackageViewModel model);
        bool DeleteBulkDisbursementPackage(int disbursementPackageId, UserInfo user);
        bool AddBulkDisbursementPackage(BulkDisbursementSetupPackageViewModel model);

        // for scheme interfaces
        IEnumerable<BulkDisbursementSetupSchemeViewModel> GetAllBulkDisbursementSchemeByPackageId(int disbursementPackageId);
        IEnumerable<BulkDisbursementSetupSchemeViewModel> GetAllBulkDisbursementScheme(int companyId);
        IEnumerable<BulkDisbursementSetupSchemeViewModel> GetAllBulkDisbursementSchemeByProductId(int productId);
        IEnumerable<BulkDisbursementSetupSchemeViewModel> GetAllBulkDisbursementSchemeByDisburseSchemeId(int disburseSchemeId);
        bool AddBulkDisbursementScheme(BulkDisbursementSetupSchemeViewModel model);
        bool AddMultipleBulkDisbursementScheme(List<BulkDisbursementSetupSchemeViewModel> models);
        bool UpdateBulkDisbursementScheme(int disbursementSchemeId, BulkDisbursementSetupSchemeViewModel model);
        bool DeleteBulkDisbursementScheme(int disbursementPackageId, UserInfo user);

        // for scheme fees interfaces
        IEnumerable<BulkDisbursementSetupSchemeFeesViewModel> GetAllBulkDisbursementSchemeFeesDisburseSchemeId(int disburseSchemeId);
        IEnumerable<BulkDisbursementSetupSchemeFeesViewModel> GetAllBulkDisbursementSchemeFees();
        IEnumerable<BulkDisbursementSetupSchemeFeesViewModel> GetBulkDisbursementSchemeFeesById(int schemeFeeId);
        bool AddBulkDisbursementSchemeFees(BulkDisbursementSetupSchemeFeesViewModel model);
        bool AddMultipleBulkDisbursementSchemeFees(List<BulkDisbursementSetupSchemeFeesViewModel> models);
        bool UpdateBulkDisbursementSchemeFees(int schemeFeeId, BulkDisbursementSetupSchemeFeesViewModel model);
        bool DeleteBulkDisbursementSchemeFees(int schemeFeeId, UserInfo user);
    }
}
