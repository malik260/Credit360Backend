using FintrakBanking.ViewModels.Setups.Finance;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FintrakBanking.Interfaces.Setups.Finance
{
    public interface IChargeRepository
    {
        #region Charge
        Task<bool> AddCharge(ChargeVeiwModel entity);
        Task<bool> UpdateCharge(ChargeVeiwModel entity);
        Task<bool> DeleteCharge(ChargeVeiwModel entity);
        IEnumerable<ChargeVeiwModel> GetAllCharges(int companyId);
        ChargeVeiwModel GetAllCharges(int companyId, int chargeId);
        IEnumerable<ChargeVeiwModel> GetAllChargeByOperation(int companyId, int operationId);
        #endregion Charge End

        #region Charges Value Source

        Task<bool> AddChargeValueSource(ChargesValueSourceVeiwModel entity);
        Task<bool> UpdateChargeValueSource(ChargesValueSourceVeiwModel entity);
        Task<bool> DeleteChargeValueSource(ChargesValueSourceVeiwModel entity);
        IEnumerable<ChargesValueSourceVeiwModel> GetAllChargesValueSource(int companyId);
        ChargesValueSourceVeiwModel GetChargesValueSourceById(int companyId, int valueSourceId);

        #endregion Charges Value Source

        #region Charge Range

        Task<bool> AddChargeRange(ChargeRangeVeiwModel entity);
        Task<bool> UpdateChargeRange(ChargeRangeVeiwModel entity);
        Task<bool> DeleteChargeRange(ChargeRangeVeiwModel entity);
        IEnumerable<ChargeRangeVeiwModel> GetAllChargesRange(int companyId);
        ChargeRangeVeiwModel GetAllChargeRanges(int companyId, int rangeId);

        #endregion Charge Range

    }
}
