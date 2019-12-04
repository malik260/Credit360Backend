using FintrakBanking.Common;
using FintrakBanking.Common.Enum;
using FintrakBanking.Entities.Models;
using FintrakBanking.Entities.StagingModels;
using FintrakBanking.Interfaces.Admin;
using FintrakBanking.Interfaces.Credit;
using FintrakBanking.Interfaces.Setups.General;
using FintrakBanking.ViewModels;
using FintrakBanking.ViewModels.Credit;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FintrakBanking.Repositories.Credit
{
    public class CashBackRepository : ICashBackRepository
    {
        private FinTrakBankingContext context;
        private FinTrakBankingStagingContext context2;
        private IAuditTrailRepository audit;
        private IGeneralSetupRepository general;
        public CashBackRepository(FinTrakBankingContext _context, IAuditTrailRepository _audit,
                                FinTrakBankingStagingContext _context2, IGeneralSetupRepository _general)
        {
            this.context = _context;
            this.context2 = _context2;
            this.audit = _audit;
            this.general = _general;
        }

        public IEnumerable<CashBackViewModel> GetCashbackSectionByApplicationDetailId(int loanApplicationDetailId)
        {
            var alerts = (from a in context.TBL_CASHBACK.Where(a => a.LOANAPPLICATIONDETAILID == loanApplicationDetailId)
                          select new CashBackViewModel
                          {
                              cashBackId = a.CASHBACKID,
                              background = a.BACKGROUND,
                              issues = a.ISSUES,
                              request = a.REQUEST,
                              loanApplicationDetailId = a.LOANAPPLICATIONDETAILID,
                              operationId = a.OPERATIONID,
                          });
            return alerts;
        }

        public bool AddCashbackSection(CashBackViewModel model)
        {
            var entity = new TBL_CASHBACK
            {
                BACKGROUND = model.background,
                ISSUES = model.issues,
                REQUEST = model.request,
                LOANAPPLICATIONDETAILID = model.loanApplicationDetailId,
                OPERATIONID = model.operationId,
            };

            context.TBL_CASHBACK.Add(entity);

            var auditStaff = (context.TBL_STAFF.Where(x => x.STAFFID == model.createdBy).Select(x => x.STAFFCODE));
            // Audit Section ---------------------------
            this.audit.AddAuditTrail(new TBL_AUDIT
            {
                AUDITTYPEID = (short)AuditTypeEnum.CashbackSectionAdded,
                STAFFID = model.createdBy,
                BRANCHID = (short)model.userBranchId,
                DETAIL = $"TBL_CASHBACK '{entity.ToString()}' created by {auditStaff}",
                IPADDRESS = CommonHelpers.GetLocalIpAddress(),
                URL = model.applicationUrl,
                APPLICATIONDATE = general.GetApplicationDate(),
                SYSTEMDATETIME = DateTime.Now,
                DEVICENAME = CommonHelpers.GetDeviceName(),
                OSNAME = CommonHelpers.FriendlyName(),
            });
            // Audit Section end ------------------------

            return context.SaveChanges() != 0;
        }

        public bool UpdateCashbackSection(int id, CashBackViewModel model, UserInfo user)
        {
            var entity = this.context.TBL_CASHBACK.Find(id);
            entity.BACKGROUND = model.background;
            entity.ISSUES = model.issues;
            entity.REQUEST = model.request;
            entity.LOANAPPLICATIONDETAILID = model.loanApplicationDetailId;
            entity.OPERATIONID = model.operationId;

            var auditStaff = (context.TBL_STAFF.Where(x => x.STAFFID == user.createdBy).Select(x => x.STAFFCODE));
            // Audit Section ---------------------------
            this.audit.AddAuditTrail(new TBL_AUDIT
            {
                AUDITTYPEID = (short)AuditTypeEnum.CashbackSectionUpdated,
                STAFFID = user.createdBy,
                BRANCHID = (short)user.BranchId,
                DETAIL = $"TBL_CASHBACK'{entity.ToString()}' was updated by {auditStaff}",
                IPADDRESS = CommonHelpers.GetLocalIpAddress(),
                URL = user.applicationUrl,
                APPLICATIONDATE = general.GetApplicationDate(),
                SYSTEMDATETIME = DateTime.Now,
                TARGETID = entity.CASHBACKID,
                DEVICENAME = CommonHelpers.GetDeviceName(),
                OSNAME = CommonHelpers.FriendlyName(),
            });
            // Audit Section end ------------------------

            return context.SaveChanges() != 0;
        }

      }
    }
