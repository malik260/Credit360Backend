using FintrakBanking.Entities.Models;
using FintrakBanking.Interfaces.Setups.Finance;
using FintrakBanking.ViewModels.Setups.Finance;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FintrakBanking.Repositories.Setups.Finance
{
    public class ChargeRepository : IChargeRepository
    {
        private FinTrakBankingContext context;

        public ChargeRepository(FinTrakBankingContext _context)
        {
            context = _context;
        }
        #region Charges
        public async Task<bool> AddCharge(ChargeVeiwModel entity)
        {
            var data = new tbl_Charges()
            {
                ApplyVAT = entity.applyVAT,
                ApplyWHT = entity.applyWHT,
                ChargeName = entity.chargeName,
                CompanyId = entity.companyId,
                Frequency = entity.frequency,
                CreatedBy = entity.createdBy,
                DateTimeCreated = entity.dateTimeCreated,
                GLAccountId = entity.gLAccountId,
                OperationId = entity.operationId,
                SetValue = entity.setValue
            };
            context.tbl_Charges.Add(data);
            return await context.SaveChangesAsync() != 0;
        }
        public async Task<bool> UpdateCharge(ChargeVeiwModel entity)
        {
            var data = context.tbl_Charges.Find(entity.chargeId);
            if (data != null)
            {
                data.ApplyVAT = entity.applyVAT;
                data.ApplyWHT = entity.applyWHT;
                data.ChargeName = entity.chargeName;
                data.CompanyId = entity.companyId;
                data.Frequency = entity.frequency;
                data.LastUpdatedBy = entity.lastUpdatedBy;
                data.DateTimeUpdated = entity.dateTimeUpdated;
                data.GLAccountId = entity.gLAccountId;
                data.OperationId = entity.operationId;
                data.SetValue = entity.setValue;
            }
            return await context.SaveChangesAsync() != 0;
        }
        public async Task<bool> DeleteCharge(ChargeVeiwModel entity)
        {
            var data = context.tbl_Charges.Find(entity.chargeId);
            if (data != null)
            {
                data.Deleted = false;
                data.DateTimeDeleted = entity.dateTimeDeleted;
                data.DeletedBy = (int)entity.deletedBy;
            }
            return await context.SaveChangesAsync() != 0;
        }
        private IQueryable<ChargeVeiwModel> GetCharges(int companyId)
        {
            var data = context.tbl_Charges.Where(c => c.CompanyId == companyId);
            return (IQueryable<ChargeVeiwModel>)data;
        }
        public IEnumerable<ChargeVeiwModel> GetAllCharges(int companyId)
        {
            return GetCharges(companyId).ToList().AsEnumerable();
        }
        public ChargeVeiwModel  GetAllCharges(int companyId, int chargeId)
        {
            return GetCharges(companyId).FirstOrDefault(c => c.chargeId == chargeId);
        }
        public IEnumerable<ChargeVeiwModel> GetAllChargeByOperation(int companyId, int operationId)
        {
            return GetCharges(companyId).Where(d => d.operationId == operationId).ToList().AsEnumerable();
        }
        #endregion Charge End

        #region Charges Value Source
        public async Task<bool> AddChargeValueSource(ChargesValueSourceVeiwModel entity)
        {
            var data = new tbl_Charges_ValueSource()
            {
                IsAbsolute = entity.IsAbsolute,
                IsFixed = entity.isFixed,
                ValueSourceName = entity.valueSourceName,
                CompanyId = entity.companyId,
                CreatedBy = entity.createdBy,
                DateTimeCreated = entity.dateTimeCreated
            };
            context.tbl_Charges_ValueSource.Add(data);
            return await context.SaveChangesAsync() != 0;
        }
        public async Task<bool> UpdateChargeValueSource(ChargesValueSourceVeiwModel entity)
        {
            var data = context.tbl_Charges_ValueSource.Find(entity.valueSourceId);
            if (data != null)
            {
                    data.IsAbsolute = entity.IsAbsolute;
                data.IsFixed = entity.isFixed;
                data.ValueSourceName = entity.valueSourceName;
                data.CompanyId = entity.companyId; 
                data.LastUpdatedBy = entity.lastUpdatedBy;
                data.DateTimeUpdated = entity.dateTimeUpdated; 
            }
            return await context.SaveChangesAsync() != 0;
        }
        public async Task<bool> DeleteChargeValueSource(ChargesValueSourceVeiwModel  entity)
        {
            var data = context.tbl_Charges_ValueSource.Find(entity.valueSourceId);
            if (data != null)
            {
                data.Deleted = false;
                data.DateTimeDeleted = entity.dateTimeDeleted;
                data.DeletedBy = (int)entity.deletedBy;
            }
            return await context.SaveChangesAsync() != 0;
        }
        private IQueryable<ChargesValueSourceVeiwModel> GetChargesValueSource(int companyId)
        {
            var data = context.tbl_Charges_ValueSource.Where(c => c.CompanyId == companyId);
            return (IQueryable<ChargesValueSourceVeiwModel>)data;
        }
        public IEnumerable<ChargesValueSourceVeiwModel> GetAllChargesValueSource(int companyId)
        {
            return GetChargesValueSource(companyId).ToList().AsEnumerable();
        }
        public ChargesValueSourceVeiwModel GetChargesValueSourceById(int companyId, int valueSourceId)
        {
            return GetChargesValueSource(companyId).FirstOrDefault(c => c.valueSourceId == valueSourceId);
        }

        #endregion Charges Value Source


        #region Charge Range
        public async Task<bool> AddChargeRange(ChargeRangeVeiwModel entity)
        {
            var data = new tbl_Charge_Range()
            {
                ChargeId = entity.chargeId,
                Rate = entity.rate,
                MaximumAmount = entity.maximumAmount,
                MinimumAmount = entity.minimumAmount,
                CompanyId = entity.companyId,
                CreatedBy = entity.createdBy,
                DateTimeCreated = entity.dateTimeCreated
            };
            context.tbl_Charge_Range.Add(data);
            return await context.SaveChangesAsync() != 0;
        }
        public async Task<bool> UpdateChargeRange(ChargeRangeVeiwModel entity)
        {
            var data = context.tbl_Charge_Range.Find(entity.rangeId);
            if (data != null)
            {
                data.ChargeId = entity.chargeId;
                data.Rate  = entity.rate;
                data.MaximumAmount = entity.maximumAmount;
                data.MinimumAmount = entity.minimumAmount; 
                data.CompanyId = entity.companyId;
                data.LastUpdatedBy = entity.lastUpdatedBy;
                data.DateTimeUpdated = entity.dateTimeUpdated;
                
            }
            return await context.SaveChangesAsync() != 0;
        }
        public async Task<bool> DeleteChargeRange(ChargeRangeVeiwModel entity)
        {
            var data = context.tbl_Charge_Range.Find(entity.chargeId);
            if (data != null)
            {
                data.Deleted = false;
                data.DateTimeDeleted = entity.dateTimeDeleted;
                data.DeletedBy = (int)entity.deletedBy;
            }
            return await context.SaveChangesAsync() != 0;
        }
        private IQueryable<ChargeRangeVeiwModel> GetChargesRange(int companyId)
        {
            var data = context.tbl_Charge_Range.Where(c => c.CompanyId == companyId);
            return (IQueryable<ChargeRangeVeiwModel>)data;
        }
        public IEnumerable<ChargeRangeVeiwModel> GetAllChargesRange(int companyId)
        {
            return GetChargesRange(companyId).ToList().AsEnumerable();
        }
        public ChargeRangeVeiwModel GetAllChargeRanges(int companyId, int rangeId)
        {
            return GetChargesRange(companyId).FirstOrDefault(c => c.rangeId == rangeId);
        }

        #endregion Charge Range
    }
}
