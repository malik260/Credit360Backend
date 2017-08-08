using System;
using System.Collections.Generic;
using FintrakBanking.Entities.Models;
using FintrakBanking.Interfaces.Admin;
using FintrakBanking.Interfaces.Setups.Finance;
using FintrakBanking.Interfaces.Setups.General;
using FintrakBanking.ViewModels;
using FintrakBanking.ViewModels.Setups.Finance;
using FintrakBanking.Common.Enum;
using System.Linq;

namespace FintrakBanking.Repositories.Finance
{
    public class TaxRepository : ITaxRepository
    {
        private FinTrakBankingContext context;
        private IGeneralSetupRepository general;
        private IAuditTrailRepository audit;

        public TaxRepository(FinTrakBankingContext context, IGeneralSetupRepository general, IAuditTrailRepository audit)
        {
            this.context = context;
            this.general = general;
            this.audit = audit;
        }

        public bool AddTax(TaxViewModel model)
        {
            var data = new tbl_Tax
            {
                TaxId = model.taxId,
                TaxName = model.taxName,
                Amount = model.amount,
                Rate = model.rate,
                GLAccountId = model.gLAccountId,
                UseAmount = model.useAmount,
                CompanyId = model.companyId,
                CreatedBy = (int)model.createdBy,
                DateTimeCreated = general.GetApplicationDate()
            };

            context.tbl_Tax.Add(data);

            // Audit Section ---------------------------
            var audit = new tbl_Audit
            {
                AuditTypeId = (short)AuditTypeEnum.TaxAdded,
                StaffId = model.createdBy,
                BranchId = (short)model.userBranchId,
                Detail = $"Added Tax '{ data.TaxName }' ",
                IPAddress = model.userIPAddress,
                Url = model.applicationUrl,
                ApplicationDate = general.GetApplicationDate(),
                SystemDateTime = DateTime.Now
            };
            this.audit.AddAuditTrail(audit);
            // End of Audit Section ---------------------

            return context.SaveChanges() != 0;
        }

        public bool UpdateTax(TaxViewModel model, int taxId)
        {
            var data = this.context.tbl_Tax.Find(taxId);
            if (data == null)
            {
                return false;
            }

            data.TaxId = model.taxId;
            data.TaxName = model.taxName;
            data.Amount = model.amount;
            data.Rate = model.rate;
            data.GLAccountId = model.gLAccountId;
            data.UseAmount = model.useAmount;

            data.LastUpdatedBy = model.lastUpdatedBy;
            data.DateTimeUpdated = general.GetApplicationDate();

            // Audit Section ---------------------------
            var audit = new tbl_Audit
            {
                AuditTypeId = (short)AuditTypeEnum.TaxUpdated,
                StaffId = model.lastUpdatedBy,
                BranchId = (short)model.userBranchId,
                Detail = $"Updated Tax '{ data.TaxName }' ",
                IPAddress = model.userIPAddress,
                Url = model.applicationUrl,
                ApplicationDate = general.GetApplicationDate(),
                SystemDateTime = DateTime.Now
            };
            this.audit.AddAuditTrail(audit);
            // End of Audit Section ---------------------

            return context.SaveChanges() != 0;
        }

        public IEnumerable<TaxViewModel> GetAllTax()
        {
            return this.context.tbl_Tax.Where(x => x.Deleted == false).Select(x => new TaxViewModel
            {
                taxId = x.TaxId,
                taxName = x.TaxName,
                amount = x.Amount,
                rate = x.Rate,
                gLAccountId = x.GLAccountId,
                useAmount = x.UseAmount,
                companyId = x.CompanyId,
            });
        }

        public TaxViewModel GetTax(int taxId)
        {
            var data = this.context.tbl_Tax.Find(taxId);

            if (data == null)
            {
                return null;
            }

            return new TaxViewModel
            {
                taxId = data.TaxId,
                taxName = data.TaxName,
                amount = data.Amount,
                rate = data.Rate,
                gLAccountId = data.GLAccountId,
                useAmount = data.UseAmount,
                companyId = data.CompanyId,
            };
        }

        public IEnumerable<TaxViewModel> GetAllTaxByCompanyId(int companyId)
        {
            return this.GetAllTax().Where(x => x.companyId == companyId);
        }

        public bool DeleteTax(int taxId, UserInfo user)
        {
            var data = this.context.tbl_Tax.Find(taxId);
            if (data == null)
            {
                return false;
            }

            data.Deleted = true;
            data.DateTimeUpdated = general.GetApplicationDate();

            // Audit Section ---------------------------
            var audit = new tbl_Audit
            {
                AuditTypeId = (short)AuditTypeEnum.TaxDeleted,
                StaffId = user.createdBy,
                BranchId = (short)user.BranchId,
                Detail = $"Deleted Tax '{ data.TaxName }' ",
                IPAddress = user.userIPAddress,
                Url = user.applicationUrl,
                ApplicationDate = general.GetApplicationDate(),
                SystemDateTime = DateTime.Now
            };
            this.audit.AddAuditTrail(audit);
            // End of Audit Section ---------------------

            return context.SaveChanges() != 0;
        }
    }
}
