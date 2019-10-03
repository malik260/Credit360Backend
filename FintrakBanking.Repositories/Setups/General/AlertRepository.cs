using FintrakBanking.Common.Enum;
using FintrakBanking.Entities.Models;
using FintrakBanking.Interfaces.Admin;
using FintrakBanking.Interfaces.Setups.General;
using FintrakBanking.ViewModels;
using FintrakBanking.ViewModels.Setups.General;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FintrakBanking.Repositories.Setups.General
{
   public class AlertRepository : IAlertRepository
    {
        private FinTrakBankingContext context;
        private IAuditTrailRepository audit;
        private IGeneralSetupRepository general;
        public AlertRepository(FinTrakBankingContext _context, IAuditTrailRepository _audit, IGeneralSetupRepository _general)
        {
            this.context = _context;
            this.audit = _audit;
            this.general = _general;
        }

        public IEnumerable<AlertViewModel> GetAllAlerts()
        {
            var alerts = (from a in context.TBL_ALERT_TITLE
                                      select new AlertViewModel
                                      {
                                          alertTitleId = a.ALERTTITLEID,
                                          title = a.TITLE,
                                          template = a.TEMPLATE
                                      });
            return alerts;
        }

        public AlertViewModel GetAlertById(int id)
        {
            var alert = (from a in context.TBL_ALERT_TITLE.Where(x=>x.ALERTTITLEID == id)
                          select new AlertViewModel
                          {
                              alertTitleId = a.ALERTTITLEID,
                              title = a.TITLE,
                              template = a.TEMPLATE
                          }).FirstOrDefault();
            return alert;
        }

        public bool AddAlertTitle(AlertViewModel model)
        {
            var entity = new TBL_ALERT_TITLE
            {
                TITLE = model.title,
                TEMPLATE = model.template
            };

            context.TBL_ALERT_TITLE.Add(entity);

            var auditStaff = (context.TBL_STAFF.Where(x => x.STAFFID == model.createdBy).Select(x => x.STAFFCODE));
            // Audit Section ---------------------------
            this.audit.AddAuditTrail(new TBL_AUDIT
            {
                AUDITTYPEID = (short)AuditTypeEnum.AlertTitleAdded,
                STAFFID = model.createdBy,
                BRANCHID = (short)model.userBranchId,
                DETAIL = $"TBL_ALERT_TITLE '{entity.ToString()}' created by {auditStaff}",
                IPADDRESS = model.userIPAddress,
                URL = model.applicationUrl,
                APPLICATIONDATE = general.GetApplicationDate(),
                SYSTEMDATETIME = DateTime.Now
            });
            // Audit Section end ------------------------

            return context.SaveChanges() != 0;
        }

        public bool UpdateLcCondition(AlertViewModel model, int id, UserInfo user)
        {
            var entity = this.context.TBL_ALERT_TITLE.Find(id);
            entity.TITLE = model.title;
            entity.TEMPLATE = model.template;
           
            var auditStaff = (context.TBL_STAFF.Where(x => x.STAFFID == user.createdBy).Select(x => x.STAFFCODE));
            // Audit Section ---------------------------
            this.audit.AddAuditTrail(new TBL_AUDIT
            {
                AUDITTYPEID = (short)AuditTypeEnum.AlertTitleUpdated,
                STAFFID = user.createdBy,
                BRANCHID = (short)user.BranchId,
                DETAIL = $"TBL_ALERT_TITLE '{entity.ToString()}' was updated by {auditStaff}",
                IPADDRESS = user.userIPAddress,
                URL = user.applicationUrl,
                APPLICATIONDATE = general.GetApplicationDate(),
                SYSTEMDATETIME = DateTime.Now,
                TARGETID = entity.ALERTTITLEID
            });
            // Audit Section end ------------------------

            return context.SaveChanges() != 0;
        }

        public bool DeleteLcCondition(int id, UserInfo user)
        {
            var entity = this.context.TBL_ALERT_TITLE.Find(id);
            
            var auditStaff = (context.TBL_STAFF.Where(x => x.STAFFID == user.createdBy).Select(x => x.STAFFCODE));
            // Audit Section ---------------------------
            this.audit.AddAuditTrail(new TBL_AUDIT
            {
                AUDITTYPEID = (short)AuditTypeEnum.AlertTitleDeleted,
                STAFFID = user.createdBy,
                BRANCHID = (short)user.BranchId,
                DETAIL = $"TBL_Lc Condition '{entity.ToString()}' was deleted by {auditStaff}",
                IPADDRESS = user.userIPAddress,
                URL = user.applicationUrl,
                APPLICATIONDATE = general.GetApplicationDate(),
                SYSTEMDATETIME = DateTime.Now,
                TARGETID = entity.ALERTTITLEID
            });
            // Audit Section end ------------------------

            return context.SaveChanges() != 0;
        }
    }
}
