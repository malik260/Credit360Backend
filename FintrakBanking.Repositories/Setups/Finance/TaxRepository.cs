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
using FintrakBanking.Common;

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
            var data = new TBL_TAX
            {
                TAXID = model.taxId,
                TAXNAME = model.taxName,
                AMOUNT = model.amount,
                RATE = model.rate,
                GLACCOUNTID = model.gLAccountId,
                USEAMOUNT = model.useAmount,
                COMPANYID = model.companyId,
                CREATEDBY = (int)model.createdBy,
                DATETIMECREATED = general.GetApplicationDate()
            };

            context.TBL_TAX.Add(data);

            // Audit Section ---------------------------
            var audit = new TBL_AUDIT
            {
                AUDITTYPEID = (short)AuditTypeEnum.TaxAdded,
                STAFFID = model.createdBy,
                BRANCHID = (short)model.userBranchId,
                DETAIL = $"Added Tax '{ data.TAXNAME }' ",
                IPADDRESS = CommonHelpers.GetLocalIpAddress(),
                URL = model.applicationUrl,
                APPLICATIONDATE = general.GetApplicationDate(),
                SYSTEMDATETIME = DateTime.Now,
                DEVICENAME = CommonHelpers.GetDeviceName(),
                OSNAME = CommonHelpers.FriendlyName()
            };
            this.audit.AddAuditTrail(audit);
            // End of Audit Section ---------------------

            return context.SaveChanges() != 0;
        }

        public bool UpdateTax(TaxViewModel model, int taxId)
        {
            var data = this.context.TBL_TAX.Find(taxId);
            if (data == null)
            {
                return false;
            }

            data.TAXNAME = model.taxName;
            data.AMOUNT = model.amount;
            data.RATE = model.rate;
            data.GLACCOUNTID = model.gLAccountId;
            data.USEAMOUNT = model.useAmount;

            data.LASTUPDATEDBY = model.lastUpdatedBy;
            data.DATETIMEUPDATED = general.GetApplicationDate();

            // Audit Section ---------------------------
            var audit = new TBL_AUDIT
            {
                AUDITTYPEID = (short)AuditTypeEnum.TaxUpdated,
                STAFFID = model.staffId,
                BRANCHID = (short)model.userBranchId,
                DETAIL = $"Updated Tax '{ data.TAXNAME }' ",
                IPADDRESS = CommonHelpers.GetLocalIpAddress(),
                URL = model.applicationUrl,
                APPLICATIONDATE = general.GetApplicationDate(),
                SYSTEMDATETIME = DateTime.Now,
                DEVICENAME = CommonHelpers.GetDeviceName(),
                OSNAME = CommonHelpers.FriendlyName()
            };
            this.audit.AddAuditTrail(audit);
            // End of Audit Section ---------------------

            return context.SaveChanges() != 0;
        }

        public IEnumerable<TaxViewModel> GetAllTax()
        {
            return this.context.TBL_TAX.Where(x => x.DELETED == false).Select(x => new TaxViewModel
            {
                taxId = x.TAXID,
                taxName = x.TAXNAME,
                amount = x.AMOUNT,
                rate = x.RATE,
                gLAccountId = x.GLACCOUNTID,
                useAmount = x.USEAMOUNT,
                companyId = x.COMPANYID,
            });
        }

        public TaxViewModel GetTax(int taxId)
        {
            var data = this.context.TBL_TAX.Find(taxId);

            if (data == null)
            {
                return null;
            }

            return new TaxViewModel
            {
                taxId = data.TAXID,
                taxName = data.TAXNAME,
                amount = data.AMOUNT,
                rate = data.RATE,
                gLAccountId = data.GLACCOUNTID,
                useAmount = data.USEAMOUNT,
                companyId = data.COMPANYID,
            };
        }

        public IEnumerable<TaxViewModel> GetAllTaxByCompanyId(int companyId)
        {
            return this.GetAllTax().Where(x => x.companyId == companyId);
        }

        public bool DeleteTax(int taxId, UserInfo user)
        {
            var data = this.context.TBL_TAX.Find(taxId);
            if (data == null)
            {
                return false;
            }

            data.DELETED = true;
            data.DATETIMEUPDATED = general.GetApplicationDate();

            // Audit Section ---------------------------
            var audit = new TBL_AUDIT
            {
                AUDITTYPEID = (short)AuditTypeEnum.TaxDeleted,
                STAFFID = user.createdBy,
                BRANCHID = (short)user.BranchId,
                DETAIL = $"Deleted Tax '{ data.TAXNAME }' ",
                IPADDRESS = CommonHelpers.GetLocalIpAddress(),
                URL = user.applicationUrl,
                APPLICATIONDATE = general.GetApplicationDate(),
                SYSTEMDATETIME = DateTime.Now,
                DEVICENAME = CommonHelpers.GetDeviceName(),
                OSNAME = CommonHelpers.FriendlyName()
            };
            this.audit.AddAuditTrail(audit);
            // End of Audit Section ---------------------

            return context.SaveChanges() != 0;
        }
    }
}
