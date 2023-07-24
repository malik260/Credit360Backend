using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FintrakBanking.ViewModels.Setups.Credit;

namespace FintrakBanking.Interfaces.Setups.Credit
{
    public interface ICrmsRegulatoryRepository
    {
        #region CRMS CREDIT TYPE PRODUCT
        IEnumerable<CrmsRegulatoryViewModel> GetAllRegulatorySetup();
        IEnumerable<CrmsRegulatoryTypeViewModel> GetAllRegulatoryType();
        bool AddRegulatory(CrmsRegulatoryViewModel model);
        bool UpdateRegulatory(CrmsRegulatoryViewModel model, int regulatoryId);
        IEnumerable<CrmsRegulatoryViewModel> GetRegulatoryByTypeId(int crmsTypeId, int companyId);

        bool DeleteRegulatory(int regulatoryId, short userBranchId, int companyId, int lastUpdatedBy, string applicationUrl, string userIPAddress);
        #endregion
    }
}
