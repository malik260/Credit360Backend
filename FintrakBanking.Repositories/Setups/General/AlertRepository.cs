using FintrakBanking.Common;
using FintrakBanking.Common.CustomException;
using FintrakBanking.Common.Enum;
using FintrakBanking.Entities.AlertReportingModels;
using FintrakBanking.Entities.Models;
using FintrakBanking.Entities.StagingModels;
using FintrakBanking.Interfaces.Admin;
using FintrakBanking.Interfaces.Credit;
using FintrakBanking.Interfaces.Setups.General;
using FintrakBanking.ViewModels;
using FintrakBanking.ViewModels.Notification;
using FintrakBanking.ViewModels.Setups.General;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.Entity;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FintrakBanking.Repositories.Setups.General
{
    public class AlertRepository : IAlertRepository
    {
        private IExternalAlertRepository externalAlertRepository;
        private FinTrakBankingContext context;
        private FinTrakBankingStagingContext context2;
        private IAuditTrailRepository audit;
        private IGeneralSetupRepository general;
        public AlertRepository(FinTrakBankingContext _context, IAuditTrailRepository _audit, IGeneralSetupRepository _general,
                                FinTrakBankingStagingContext _context2, IExternalAlertRepository _externalAlertRepository)
        {
            this.context = _context;
            this.context2 = _context2;
            this.audit = _audit;
            this.general = _general;
            this.externalAlertRepository = _externalAlertRepository; 
        }

        #region other code logic
        public AlertRepository() {}

        public IEnumerable<AlertTitleViewModel> GetAllAlerts()
        {
            var alerts = (from a in context.TBL_ALERT_TITLE                  
                          select new AlertTitleViewModel
                          {
                              alertTitleId = a.ALERTTITLEID,
                              title = a.TITLE,
                              template = a.TEMPLATE,
                              templateType = a.TEMPLATETYPE,
                              businessOwner = a.BUSINESSOWNER,
                              senderEmail = a.SENDEREMAIL,
                              senderName = a.SENDERNAME,
                              templateTypeName = a.TEMPLATETYPE=="1"? "EMAIL":"SMS",
                              defaultEmail = a.DEFAULTEMAIL,
                              lastSentDate = a.LASTSENTDATE,
                              actionStatus = a.ACTIONSTATUS,
                              bindingMethod = a.BINDINGMETHOD,
                          });
            return alerts;
        }

        public IEnumerable<StaffRoleViewModel> GetAllStaffRoles()
        {
            var staffRoles = (from a in context.TBL_STAFF_ROLE
                          select new StaffRoleViewModel
                          {
                              staffRoleId = a.STAFFROLEID,
                              staffRoleCode = a.STAFFROLECODE,
                              staffRoleName = a.STAFFROLENAME,
                          });
            return staffRoles;
        }

        public IEnumerable<AlertTitleViewModel> GetAlerts()
        {
            var alerts = (from a in context.TBL_ALERT_TITLE
                          select new AlertTitleViewModel
                          {
                              alertTitleId = a.ALERTTITLEID,
                              title = a.TITLE,
                              template = a.TEMPLATE,
                              templateType = a.TEMPLATETYPE,
                              businessOwner = a.BUSINESSOWNER,
                              senderEmail = a.SENDEREMAIL,
                              senderName = a.SENDERNAME,
                              templateTypeName = a.TEMPLATETYPE == "1" ? "EMAIL" : "SMS",
                              defaultEmail = a.DEFAULTEMAIL,
                              lastSentDate = a.LASTSENTDATE,
                              actionStatus = a.ACTIONSTATUS,
                              bindingMethod = a.BINDINGMETHOD,
                          });
            return alerts;
        }

        public AlertTitleViewModel GetAlertById(int id)
        {
            var alert = (from a in context.TBL_ALERT_TITLE.Where(x => x.ALERTTITLEID == id)
                         select new AlertTitleViewModel
                         {
                             alertTitleId = a.ALERTTITLEID,
                             title = a.TITLE,
                             template = a.TEMPLATE,
                             templateType = a.TEMPLATETYPE,
                             businessOwner = a.BUSINESSOWNER,
                             senderEmail = a.SENDEREMAIL,
                             senderName = a.SENDERNAME,
                             templateTypeName = a.TEMPLATETYPE == "1" ? "EMAIL" : "SMS",
                             defaultEmail = a.DEFAULTEMAIL,
                             lastSentDate = a.LASTSENTDATE,
                             actionStatus = a.ACTIONSTATUS,
                             bindingMethod = a.BINDINGMETHOD,
                         }).FirstOrDefault();
            return alert;
        }

        public bool AddAlertTitle(AlertTitleViewModel model)
        {
            var entity = new TBL_ALERT_TITLE
            {
                TITLE = model.title,
                TEMPLATE = model.template,
                BUSINESSOWNER = model.businessOwner,
                SENDEREMAIL = model.senderEmail,
                SENDERNAME = model.senderName,
                TEMPLATETYPE = model.templateType,
                DEFAULTEMAIL = model.defaultEmail,
                BINDINGMETHOD = model.bindingMethod,
                //LASTSENTDATE = general.GetApplicationDate(),
                ACTIONSTATUS = 1,
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

        public bool UpdateAlertTitle(int id, AlertTitleViewModel model, UserInfo user)
        {
            var entity = this.context.TBL_ALERT_TITLE.Find(id);
            entity.TITLE = model.title;
            entity.TEMPLATE = model.template;
            entity.BUSINESSOWNER = model.businessOwner;
            entity.SENDERNAME = model.senderName;
            entity.SENDEREMAIL = model.senderEmail;
            entity.TEMPLATETYPE = model.templateType;
            entity.DEFAULTEMAIL = model.defaultEmail;
            entity.BINDINGMETHOD = model.bindingMethod;
            //entity.LASTSENTDATE = general.GetApplicationDate();
            entity.ACTIONSTATUS = 1;

            var auditStaff = (context.TBL_STAFF.Where(x => x.STAFFID == user.createdBy).Select(x => x.STAFFCODE));
            // Audit Section ---------------------------
            this.audit.AddAuditTrail(new TBL_AUDIT
            {
                AUDITTYPEID = (short)AuditTypeEnum.AlertTitleUpdated,
                STAFFID = user.createdBy,
                BRANCHID = (short)user.BranchId,
                DETAIL = $"TBL_ALERT_TITLE'{entity.TITLE}' was updated by {auditStaff}",
                IPADDRESS = CommonHelpers.GetLocalIpAddress(),
                URL = user.applicationUrl,
                APPLICATIONDATE = general.GetApplicationDate(),
                SYSTEMDATETIME = DateTime.Now,
                TARGETID = entity.ALERTTITLEID,
                DEVICENAME = CommonHelpers.GetDeviceName(),
                OSNAME = CommonHelpers.FriendlyName(),
            });
            // Audit Section end ------------------------

            return context.SaveChanges() != 0;
        }

        public bool DeleteAlertTitle(int id, UserInfo user)
        {
            var entity = this.context.TBL_ALERT_TITLE.Find(id);
            context.TBL_ALERT_TITLE.Remove(entity);
            var auditStaff = (context.TBL_STAFF.Where(x => x.STAFFID == user.createdBy).Select(x => x.STAFFCODE));
            // Audit Section ---------------------------
            this.audit.AddAuditTrail(new TBL_AUDIT
            {
                AUDITTYPEID = (short)AuditTypeEnum.AlertTitleDeleted,
                STAFFID = user.createdBy,
                BRANCHID = (short)user.BranchId,
                DETAIL = $"TBL_ALERT_TITLE '{entity.ToString()}' was deleted by {auditStaff}",
                IPADDRESS = CommonHelpers.GetLocalIpAddress(),
                URL = user.applicationUrl,
                APPLICATIONDATE = general.GetApplicationDate(),
                SYSTEMDATETIME = DateTime.Now,
                TARGETID = entity.ALERTTITLEID,
                DEVICENAME = CommonHelpers.GetDeviceName(),
                OSNAME = CommonHelpers.FriendlyName(),
            });
            // Audit Section end ------------------------

            return context.SaveChanges() != 0;
        }


        public IEnumerable<AlertSetupViewModel> GetAllAlertSetup()
        {
            var alerts = (from a in context.TBL_ALERT_SETUP
                          join c in context.TBL_ALERT_ROLE_GROUP on a.LEVELGROUPID equals c.ALERTLEVELGROUPID
                          join x in context.TBL_ALERT_CONDITION on (int)a.CONDITIONID equals x.ALERTCONDITIONID
                          join f in context.TBL_OPERATIONS on (int)x.OPERATIONID equals f.OPERATIONID
                          select new AlertSetupViewModel
                          {
                              alertSetupId = a.ALERTSETUPID,
                              titleId = a.TITLEID,
                              levelGroupId = a.LEVELGROUPID,
                              frequencyId = a.FREQUENCYID,
                              operationName = f.OPERATIONNAME,
                              formular = x.FORMULAR,
                              title = context.TBL_ALERT_TITLE.Where(at => at.ALERTTITLEID == a.TITLEID).Select(at => at.TITLE).FirstOrDefault() == null ? "N/A" : context.TBL_ALERT_TITLE.Where(at => at.ALERTTITLEID == a.TITLEID).Select(at => at.TITLE).FirstOrDefault(),
                              levelGroupName = context.TBL_ALERT_ROLE_GROUP.Where(g => g.ALERTLEVELGROUPID == a.LEVELGROUPID).Select(at => at.LEVELGROUPNAME).FirstOrDefault() == null ? "N/A" : context.TBL_ALERT_ROLE_GROUP.Where(g => g.ALERTLEVELGROUPID == a.LEVELGROUPID).Select(at => at.LEVELGROUPNAME).FirstOrDefault()
                          });
            return alerts;
        }

        public AlertSetupViewModel GetAlertSetupById(int id)
        {
            var alert = (from a in context.TBL_ALERT_SETUP.Where(x => x.ALERTSETUPID == id)
                         select new AlertSetupViewModel
                         {
                             alertSetupId = a.ALERTSETUPID,
                             titleId = a.TITLEID,
                             levelGroupId = a.LEVELGROUPID,
                             frequencyId = a.FREQUENCYID,
                             conditionId = (short)a.CONDITIONID
                         }).FirstOrDefault();
            return alert;
        }

        public bool AddAlertSetup(AlertSetupViewModel model)
        {
            var entity = new TBL_ALERT_SETUP
            {
                LEVELGROUPID = model.levelGroupId,
                TITLEID = model.titleId,
                FREQUENCYID = model.frequencyId,
                CONDITIONID = model.conditionId
            };

            context.TBL_ALERT_SETUP.Add(entity);

            var auditStaff = (context.TBL_STAFF.Where(x => x.STAFFID == model.createdBy).Select(x => x.STAFFCODE));
            // Audit Section ---------------------------
            this.audit.AddAuditTrail(new TBL_AUDIT
            {
                AUDITTYPEID = (short)AuditTypeEnum.AlertSetupAdded,
                STAFFID = model.createdBy,
                BRANCHID = (short)model.userBranchId,
                DETAIL = $"TBL_ALERT_SETUP '{entity.ToString()}' created by {auditStaff}",
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

        public bool UpdateAlertSetup(int id, AlertSetupViewModel model, UserInfo user)
        {
            var entity = this.context.TBL_ALERT_SETUP.Find(id);
            entity.TITLEID = model.titleId;
            entity.FREQUENCYID = model.frequencyId;
            entity.LEVELGROUPID = model.levelGroupId;
            entity.CONDITIONID = model.conditionId;

            var auditStaff = (context.TBL_STAFF.Where(x => x.STAFFID == user.createdBy).Select(x => x.STAFFCODE));
            // Audit Section ---------------------------
            this.audit.AddAuditTrail(new TBL_AUDIT
            {
                AUDITTYPEID = (short)AuditTypeEnum.AlertSetupUpdated,
                STAFFID = user.createdBy,
                BRANCHID = (short)user.BranchId,
                DETAIL = $"TBL_ALERT_SETUP '{entity}' was updated by {auditStaff}",
                IPADDRESS = CommonHelpers.GetLocalIpAddress(),
                URL = user.applicationUrl,
                APPLICATIONDATE = general.GetApplicationDate(),
                SYSTEMDATETIME = DateTime.Now,
                TARGETID = entity.ALERTSETUPID,
                DEVICENAME = CommonHelpers.GetDeviceName(),
                OSNAME = CommonHelpers.FriendlyName(),
            });
            // Audit Section end ------------------------

            return context.SaveChanges() != 0;
        }

        public bool DeleteAlertSetup(int id, UserInfo user)
        {
            var entity = this.context.TBL_ALERT_SETUP.Find(id);
            context.TBL_ALERT_SETUP.Remove(entity);
            var auditStaff = (context.TBL_STAFF.Where(x => x.STAFFID == user.createdBy).Select(x => x.STAFFCODE));
            // Audit Section ---------------------------
            this.audit.AddAuditTrail(new TBL_AUDIT
            {
                AUDITTYPEID = (short)AuditTypeEnum.AlertSetupDeleted,
                STAFFID = user.createdBy,
                BRANCHID = (short)user.BranchId,
                DETAIL = $"TBL_ALERT_SETUP '{entity.ToString()}' was deleted by {auditStaff}",
                IPADDRESS = CommonHelpers.GetLocalIpAddress(),
                URL = user.applicationUrl,
                APPLICATIONDATE = general.GetApplicationDate(),
                SYSTEMDATETIME = DateTime.Now,
                TARGETID = entity.ALERTSETUPID,
                DEVICENAME = CommonHelpers.GetDeviceName(),
                OSNAME = CommonHelpers.FriendlyName(),
            });
            // Audit Section end ------------------------

            return context.SaveChanges() != 0;
        }


        public IEnumerable<LevelGroupMappingViewModel> GetAllAlertLevelGroupMapping()
        {
            var alerts = (from a in context.TBL_ALERT_ROLE_GRP_MAPPING
                          select new LevelGroupMappingViewModel
                          {
                              alertLevelGroupMapId = a.ALERTLEVELGROUPMAPID,
                              levelGroupId = a.LEVELGROUPID,
                              levelCode = a.LEVELCODE,
                              levelGroupName = context.TBL_ALERT_ROLE_GROUP.Where(t => t.ALERTLEVELGROUPID == a.LEVELGROUPID).Select(t => t.LEVELGROUPNAME).FirstOrDefault() == null ? "N/A" : context.TBL_ALERT_ROLE_GROUP.Where(t => t.ALERTLEVELGROUPID == a.LEVELGROUPID).Select(t => t.LEVELGROUPNAME).FirstOrDefault()
                          });
            return alerts;
        }

        public LevelGroupMappingViewModel GetAlertLevelGroupMappingById(int id)
        {
            var alert = (from a in context.TBL_ALERT_ROLE_GRP_MAPPING.Where(x => x.ALERTLEVELGROUPMAPID == id)
                         select new LevelGroupMappingViewModel
                         {
                             alertLevelGroupMapId = a.ALERTLEVELGROUPMAPID,
                             levelGroupId = a.LEVELGROUPID,
                             levelCode = a.LEVELCODE
                         }).FirstOrDefault();
            return alert;
        }

        public bool AddAlertLevelGroupMapping(LevelGroupMappingViewModel model)
        {
            var entity = new TBL_ALERT_ROLE_GRP_MAPPING
            {
                LEVELCODE = model.levelCode,
                LEVELGROUPID = model.levelGroupId
            };

            context.TBL_ALERT_ROLE_GRP_MAPPING.Add(entity);

            var auditStaff = (context.TBL_STAFF.Where(x => x.STAFFID == model.createdBy).Select(x => x.STAFFCODE));
            // Audit Section ---------------------------
            this.audit.AddAuditTrail(new TBL_AUDIT
            {
                AUDITTYPEID = (short)AuditTypeEnum.AlertLevelGroupMappingAdded,
                STAFFID = model.createdBy,
                BRANCHID = (short)model.userBranchId,
                DETAIL = $"TBL_ALERT_TITLE '{entity.ToString()}' created by {auditStaff}",
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

        public bool UpdateAlertLevelGroupMapping(int id, LevelGroupMappingViewModel model, UserInfo user)
        {
            var entity = this.context.TBL_ALERT_ROLE_GRP_MAPPING.Find(id);
            entity.LEVELGROUPID = model.levelGroupId;
            entity.LEVELCODE = model.levelCode;

            var auditStaff = (context.TBL_STAFF.Where(x => x.STAFFID == user.createdBy).Select(x => x.STAFFCODE));
            // Audit Section ---------------------------
            this.audit.AddAuditTrail(new TBL_AUDIT
            {
                AUDITTYPEID = (short)AuditTypeEnum.AlertLevelGroupMappingUpdated,
                STAFFID = user.createdBy,
                BRANCHID = (short)user.BranchId,
                DETAIL = $"TBL_ALERT_TITLE'{entity}' was updated by {auditStaff}",
                IPADDRESS = CommonHelpers.GetLocalIpAddress(),
                URL = user.applicationUrl,
                APPLICATIONDATE = general.GetApplicationDate(),
                SYSTEMDATETIME = DateTime.Now,
                TARGETID = entity.ALERTLEVELGROUPMAPID,
                DEVICENAME = CommonHelpers.GetDeviceName(),
                OSNAME = CommonHelpers.FriendlyName(),
            });
            // Audit Section end ------------------------

            return context.SaveChanges() != 0;
        }

        public bool DeleteAlertLevelGroupMapping(int id, UserInfo user)
        {
            var entity = this.context.TBL_ALERT_ROLE_GRP_MAPPING.Find(id);
            context.TBL_ALERT_ROLE_GRP_MAPPING.Remove(entity);
            var auditStaff = (context.TBL_STAFF.Where(x => x.STAFFID == user.createdBy).Select(x => x.STAFFCODE));
            // Audit Section ---------------------------
            this.audit.AddAuditTrail(new TBL_AUDIT
            {
                AUDITTYPEID = (short)AuditTypeEnum.AlertLevelGroupMappingDeleted,
                STAFFID = user.createdBy,
                BRANCHID = (short)user.BranchId,
                DETAIL = $"TBL_ALERT_TITLE '{entity.ToString()}' was deleted by {auditStaff}",
                IPADDRESS = CommonHelpers.GetLocalIpAddress(),
                URL = user.applicationUrl,
                APPLICATIONDATE = general.GetApplicationDate(),
                SYSTEMDATETIME = DateTime.Now,
                TARGETID = entity.ALERTLEVELGROUPMAPID,
                DEVICENAME = CommonHelpers.GetDeviceName(),
                OSNAME = CommonHelpers.FriendlyName(),
            });
            // Audit Section end ------------------------

            return context.SaveChanges() != 0;
        }


        public IEnumerable<AlertLevelGroupViewModel> GetAllAlertLevelGroup()
        {
            var alerts = (from a in context.TBL_ALERT_ROLE_GROUP
                          select new AlertLevelGroupViewModel
                          {
                              alertLevelGroupId = a.ALERTLEVELGROUPID,
                              levelGroupName = a.LEVELGROUPNAME,
                              description = a.DESCRIPTION
                          });
            return alerts;
        }

        public AlertLevelGroupViewModel GetAlertLevelGroupById(int id)
        {
            var alert = (from a in context.TBL_ALERT_ROLE_GROUP.Where(x => x.ALERTLEVELGROUPID == id)
                         select new AlertLevelGroupViewModel
                         {
                             alertLevelGroupId = a.ALERTLEVELGROUPID,
                             levelGroupName = a.LEVELGROUPNAME,
                             description = a.DESCRIPTION
                         }).FirstOrDefault();
            return alert;
        }

        public bool AddAlertLevelGroup(AlertLevelGroupViewModel model)
        {
            var entity = new TBL_ALERT_ROLE_GROUP
            {
                LEVELGROUPNAME = model.levelGroupName,
                DESCRIPTION = model.description
            };

            context.TBL_ALERT_ROLE_GROUP.Add(entity);

            var auditStaff = (context.TBL_STAFF.Where(x => x.STAFFID == model.createdBy).Select(x => x.STAFFCODE));
            // Audit Section ---------------------------
            this.audit.AddAuditTrail(new TBL_AUDIT
            {
                AUDITTYPEID = (short)AuditTypeEnum.AlertLevelGroupAdded,
                STAFFID = model.createdBy,
                BRANCHID = (short)model.userBranchId,
                DETAIL = $"TBL_ALERT_TITLE '{entity.ToString()}' created by {auditStaff}",
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

        public bool UpdateAlertLevelGroup(int id, AlertLevelGroupViewModel model, UserInfo user)
        {
            var entity = this.context.TBL_ALERT_ROLE_GROUP.Find(id);
            entity.LEVELGROUPNAME = model.levelGroupName;
            entity.DESCRIPTION = model.description;

            var auditStaff = (context.TBL_STAFF.Where(x => x.STAFFID == user.createdBy).Select(x => x.STAFFCODE));
            // Audit Section ---------------------------
            this.audit.AddAuditTrail(new TBL_AUDIT
            {
                AUDITTYPEID = (short)AuditTypeEnum.AlertLevelGroupUpdated,
                STAFFID = user.createdBy,
                BRANCHID = (short)user.BranchId,
                DETAIL = $"TBL_ALERT_TITLE'{entity}' was updated by {auditStaff}",
                IPADDRESS = CommonHelpers.GetLocalIpAddress(),
                URL = user.applicationUrl,
                APPLICATIONDATE = general.GetApplicationDate(),
                SYSTEMDATETIME = DateTime.Now,
                TARGETID = entity.ALERTLEVELGROUPID,
                DEVICENAME = CommonHelpers.GetDeviceName(),
                OSNAME = CommonHelpers.FriendlyName(),
            });
            // Audit Section end ------------------------

            return context.SaveChanges() != 0;
        }

        public bool DeleteAlertLevelGroup(int id, UserInfo user)
        {
            var entity = this.context.TBL_ALERT_ROLE_GROUP.Find(id);
            context.TBL_ALERT_ROLE_GROUP.Remove(entity);
            var auditStaff = (context.TBL_STAFF.Where(x => x.STAFFID == user.createdBy).Select(x => x.STAFFCODE));
            // Audit Section ---------------------------
            this.audit.AddAuditTrail(new TBL_AUDIT
            {
                AUDITTYPEID = (short)AuditTypeEnum.AlertLevelGroupDeleted,
                STAFFID = user.createdBy,
                BRANCHID = (short)user.BranchId,
                DETAIL = $"TBL_ALERT_TITLE '{entity.ToString()}' was deleted by {auditStaff}",
                IPADDRESS = CommonHelpers.GetLocalIpAddress(),
                URL = user.applicationUrl,
                APPLICATIONDATE = general.GetApplicationDate(),
                SYSTEMDATETIME = DateTime.Now,
                TARGETID = entity.ALERTLEVELGROUPID,
                DEVICENAME = CommonHelpers.GetDeviceName(),
                OSNAME = CommonHelpers.FriendlyName(),
            });
            // Audit Section end ------------------------

            return context.SaveChanges() != 0;
        }


        public IEnumerable<AlertLevelViewModel> GetAllAlertLevel()
        {
            var alerts = (from a in context.TBL_ALERT_STAFF_ROLE 
                          join b in context.TBL_STAFF_ROLE on a.STAFFROLEID equals b.STAFFROLEID
                          join c in context.TBL_ALERT_TITLE on a.ALERTTITLEID equals c.ALERTTITLEID
                          select new AlertLevelViewModel
                          {
                              alertStaffRoleId = a.ALERTSTAFFROLEID,
                              alertTitleId = a.ALERTTITLEID,
                              staffRoleId = a.STAFFROLEID,
                              title = c.TITLE == null ? "N/A" : c.TITLE,
                              staffRoleCode = b.STAFFROLECODE == null ? "N/A" : b.STAFFROLECODE,
                              staffRoleName = b.STAFFROLENAME == null ? "N/A" : b.STAFFROLENAME,
                          });
            return alerts;
        }

        public bool AddAlertStaffRole(AlertLevelViewModel model)
        {
            var entity = new TBL_ALERT_STAFF_ROLE
            {
                ALERTTITLEID = model.alertTitleId,
                STAFFROLEID = model.staffRoleId,
            };

            context.TBL_ALERT_STAFF_ROLE.Add(entity);

            var auditStaff = (context.TBL_STAFF.Where(x => x.STAFFID == model.createdBy).Select(x => x.STAFFCODE));
            // Audit Section ---------------------------
            this.audit.AddAuditTrail(new TBL_AUDIT
            {
                AUDITTYPEID = (short)AuditTypeEnum.AlertLevelAdded,
                STAFFID = model.createdBy,
                BRANCHID = (short)model.userBranchId,
                DETAIL = $"TBL_ALERT_TITLE '{entity.ToString()}' created by {auditStaff}",
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

        public bool UpdateAlertLevel(int id, AlertLevelViewModel model, UserInfo user)
        {
            var entity = this.context.TBL_ALERT_STAFF_ROLE.Find(id);
            entity.ALERTTITLEID = model.alertTitleId;
            entity.STAFFROLEID = model.staffRoleId;

            var auditStaff = (context.TBL_STAFF.Where(x => x.STAFFID == user.createdBy).Select(x => x.STAFFCODE));
            // Audit Section ---------------------------
            this.audit.AddAuditTrail(new TBL_AUDIT
            {
                AUDITTYPEID = (short)AuditTypeEnum.AlertLevelUpdated,
                STAFFID = user.createdBy,
                BRANCHID = (short)user.BranchId,
                DETAIL = $"TBL_ALERT_TITLE'{entity}' was updated by {auditStaff}",
                IPADDRESS = CommonHelpers.GetLocalIpAddress(),
                URL = user.applicationUrl,
                APPLICATIONDATE = general.GetApplicationDate(),
                SYSTEMDATETIME = DateTime.Now,
                TARGETID = entity.ALERTSTAFFROLEID,
                DEVICENAME = CommonHelpers.GetDeviceName(),
                OSNAME = CommonHelpers.FriendlyName(),
            });
            // Audit Section end ------------------------

            return context.SaveChanges() != 0;
        }

        public bool DeleteAlertLevel(int id, UserInfo user)
        {
            var entity = this.context.TBL_ALERT_STAFF_ROLE.Find(id);
            context.TBL_ALERT_STAFF_ROLE.Remove(entity);
            var auditStaff = (context.TBL_STAFF.Where(x => x.STAFFID == user.createdBy).Select(x => x.STAFFCODE));
            // Audit Section ---------------------------
            this.audit.AddAuditTrail(new TBL_AUDIT
            {
                AUDITTYPEID = (short)AuditTypeEnum.AlertLevelDeleted,
                STAFFID = user.createdBy,
                BRANCHID = (short)user.BranchId,
                DETAIL = $"TBL_ALERT_TITLE '{entity.ToString()}' was deleted by {auditStaff}",
                IPADDRESS = CommonHelpers.GetLocalIpAddress(),
                URL = user.applicationUrl,
                APPLICATIONDATE = general.GetApplicationDate(),
                SYSTEMDATETIME = DateTime.Now,
                TARGETID = entity.ALERTSTAFFROLEID,
                DEVICENAME = CommonHelpers.GetDeviceName(),
                OSNAME = CommonHelpers.FriendlyName(),
            });
            // Audit Section end ------------------------

            return context.SaveChanges() != 0;
        }


        public IEnumerable<AlertMisViewModel> GetAllUserMisCode()
        {
            var alerts = (from a in context2.STG_USER_MIS
                          select new AlertMisViewModel
                          {
                              loginId = a.LOGINID,
                              userMisId = a.USERMISID,
                              profitCenterDefinitionCode = a.PROFITCENTERDEFINITIONCODE,
                              profitCenterMisCode = a.PROFITCENTERMISCODE
                          }).GroupBy(a => a.profitCenterDefinitionCode).Select(a => a.FirstOrDefault());
            return alerts;
        }

        public IEnumerable<AlertFrequencyViewModel> GetAllFrequency()
        {
            var frequency = (from a in context.TBL_ALERT_FREQUENCY
                             select new AlertFrequencyViewModel
                             {
                                 alertFrequencyId = a.ALERTFREQUENCYID,
                                 frequencyMode = a.FREQUENCYMODE,
                                 description = a.DESCRIPTION
                             }).ToList();
            return frequency;
        }

        public IEnumerable<AlertConditionViewModel> GetAllConditions()
        {
            var condition = (from a in context.TBL_ALERT_CONDITION
                             select new AlertConditionViewModel
                             {
                                 alertConditionId = a.ALERTCONDITIONID,
                                 triggerSource = a.TRIGGERSOURCE,
                                 type = a.TYPE,
                                 formular = a.FORMULAR,
                                 operationId = a.OPERATIONID,
                                 operationName = context.TBL_OPERATIONS.Where(o => o.OPERATIONID == a.OPERATIONID).Select(o => o.OPERATIONNAME).FirstOrDefault() == null ? "N/A" : context.TBL_OPERATIONS.Where(o => o.OPERATIONID == a.OPERATIONID).Select(o => o.OPERATIONNAME).FirstOrDefault(),
                                 lastRunDate = a.LASTRUNDATE,
                                 alertInterval = a.ALERTINTERVAL,
                                 nextRunDate = (DateTime)a.NEXTRUNDATE,
                                 title = a.TITLE,
                                 actionForTrigger = a.ACTIONFORTRIGGER,
                                 titleName = context.TBL_ALERT_TITLE.Where(b => b.ALERTTITLEID == a.TITLE).Select(b => b.TITLE).FirstOrDefault() == null ? "N/A" : context.TBL_ALERT_TITLE.Where(b => b.ALERTTITLEID == a.TITLE).Select(b => b.TITLE).FirstOrDefault()
                             });
            return condition;
        }

        public AlertConditionViewModel GetAlertConditionById(int id)
        {
            var alert = (from a in context.TBL_ALERT_CONDITION.Where(x => x.ALERTCONDITIONID == id)
                         select new AlertConditionViewModel
                         {
                             alertConditionId = a.ALERTCONDITIONID,
                             triggerSource = a.TRIGGERSOURCE,
                             type = a.TYPE,
                             formular = a.FORMULAR,
                             operationId = a.OPERATIONID,
                             operationName = context.TBL_OPERATIONS.Where(o => o.OPERATIONID == a.OPERATIONID).Select(o => o.OPERATIONNAME).FirstOrDefault() == null ? "N/A" : context.TBL_OPERATIONS.Where(o => o.OPERATIONID == a.OPERATIONID).Select(o => o.OPERATIONNAME).FirstOrDefault(),
                             lastRunDate = a.LASTRUNDATE,
                             alertInterval = a.ALERTINTERVAL,
                             nextRunDate = (DateTime)a.NEXTRUNDATE,
                             title = a.TITLE,
                             actionForTrigger = a.ACTIONFORTRIGGER,
                             titleName = context.TBL_ALERT_TITLE.Where(b => b.ALERTTITLEID == a.TITLE).Select(b => b.TITLE).FirstOrDefault() == null ? "N/A" : context.TBL_ALERT_TITLE.Where(b => b.ALERTTITLEID == a.TITLE).Select(b => b.TITLE).FirstOrDefault()
                         }).FirstOrDefault();
            return alert;
        }

        public bool AddAlertCondition(AlertConditionViewModel model)
        {
            var entity = new TBL_ALERT_CONDITION
            {
                TRIGGERSOURCE = model.triggerSource,
                TYPE = model.type,
                FORMULAR = model.formular,
                OPERATIONID = model.operationId,
                LASTRUNDATE = model.lastRunDate,
                ALERTINTERVAL = model.alertInterval,
                NEXTRUNDATE = model.nextRunDate,
                TITLE = model.title,
                ACTIONFORTRIGGER = model.actionForTrigger

            };

            context.TBL_ALERT_CONDITION.Add(entity);

            var auditStaff = (context.TBL_STAFF.Where(x => x.STAFFID == model.createdBy).Select(x => x.STAFFCODE));
            // Audit Section ---------------------------
            this.audit.AddAuditTrail(new TBL_AUDIT
            {
                AUDITTYPEID = (short)AuditTypeEnum.AlertConditionAdded,
                STAFFID = model.createdBy,
                BRANCHID = (short)model.userBranchId,
                DETAIL = $"TBL_ALERT_CONDITION '{entity.ToString()}' created by {auditStaff}",
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

        public bool UpdateAlertCondition(int id, AlertConditionViewModel model, UserInfo user)
        {
            var entity = this.context.TBL_ALERT_CONDITION.Find(id);
            entity.TRIGGERSOURCE = model.triggerSource;
            entity.LASTRUNDATE = model.lastRunDate;
            entity.NEXTRUNDATE = model.nextRunDate;
            entity.FORMULAR = model.formular;
            entity.OPERATIONID = model.operationId;
            entity.TYPE = model.type;
            entity.ALERTINTERVAL = model.alertInterval;
            entity.TITLE = model.title;
            entity.ACTIONFORTRIGGER = model.actionForTrigger;

            var auditStaff = (context.TBL_STAFF.Where(x => x.STAFFID == user.createdBy).Select(x => x.STAFFCODE));
            // Audit Section ---------------------------
            this.audit.AddAuditTrail(new TBL_AUDIT
            {
                AUDITTYPEID = (short)AuditTypeEnum.AlertConditionUpdated,
                STAFFID = user.createdBy,
                BRANCHID = (short)user.BranchId,
                DETAIL = $"TBL_ALERT_CONDITION'{entity.ToString()}' was updated by {auditStaff}",
                IPADDRESS = CommonHelpers.GetLocalIpAddress(),
                URL = user.applicationUrl,
                APPLICATIONDATE = general.GetApplicationDate(),
                SYSTEMDATETIME = DateTime.Now,
                TARGETID = entity.ALERTCONDITIONID,
                DEVICENAME = CommonHelpers.GetDeviceName(),
                OSNAME = CommonHelpers.FriendlyName(),
            });
            // Audit Section end ------------------------

            return context.SaveChanges() != 0;
        }

        public bool DeleteAlertCondition(int id, UserInfo user)
        {
            var entity = this.context.TBL_ALERT_CONDITION.Find(id);
            context.TBL_ALERT_CONDITION.Remove(entity);
            var auditStaff = (context.TBL_STAFF.Where(x => x.STAFFID == user.createdBy).Select(x => x.STAFFCODE));
            // Audit Section ---------------------------
            this.audit.AddAuditTrail(new TBL_AUDIT
            {
                AUDITTYPEID = (short)AuditTypeEnum.AlertConditionDeleted,
                STAFFID = user.createdBy,
                BRANCHID = (short)user.BranchId,
                DETAIL = $"TBL_ALERT_TITLE '{entity.ToString()}' was deleted by {auditStaff}",
                IPADDRESS = CommonHelpers.GetLocalIpAddress(),
                URL = user.applicationUrl,
                APPLICATIONDATE = general.GetApplicationDate(),
                SYSTEMDATETIME = DateTime.Now,
                TARGETID = entity.ALERTCONDITIONID,
                DEVICENAME = CommonHelpers.GetDeviceName(),
                OSNAME = CommonHelpers.FriendlyName(),
            });
            // Audit Section end ------------------------

            return context.SaveChanges() != 0;
        }

        public IEnumerable<TblOperationsViewModel> GetAllOperations()
        {
            var opeartion = (from a in context.TBL_OPERATIONS
                             select new TblOperationsViewModel
                             {
                                 operationId = a.OPERATIONID,
                                 operationName = a.OPERATIONNAME,
                             });
            return opeartion;
        }

        #endregion end of code logic

        public void validateAlertCheck()
        {
            GetLoanExpirationReminder(); 
            GetUnpaidObligationReminder();
            GetLoanRepaymentReminder();
            GetImminentMaturitiesAlertEmail();

            GetImminentMaturities();
            GetCreditCardMaturingObligations();
            GetExpiringFacilityReport();
            GetUnAuthorizedOverdraftReport();
            GetOverlineMonitoringReport();
            GetCreditCardDelinquencyMonitoringReport();
            GetPastDueObligationsReminder();
            GetRiskAssetsReportNotification();
            GetDashboardReportNotification();
            GetCACReport();
            GetOverlineCreditCardPosition();
            GetPastDueFacilitiesNotification();
            GetExpiredFacilityNotification();
            GetLoanExpirationReminderAccountOfficer();
            GetLoanRepaymentReminderAccountOfficer();
            GetUnpaidObligationReminderAccountOfficer();
            GetOverlineReminder();
            GetMaturingObligationsReport();
            GetOverlineFacilityNotification();
            GetImminentObligationMaturityFacilityNotification();
            GetNplOnCreditPortfolio();
        }

        private string GetBusinessUsersEmails(string accountOfficerMIsCode)
        {
            string emailList = "";

            var accountOfficer = context.TBL_STAFF.Where(x => x.MISCODE.ToLower() == accountOfficerMIsCode.ToLower()).FirstOrDefault();
            if (accountOfficer != null)
            {
                emailList = accountOfficer.EMAIL;
                if (accountOfficer.SUPERVISOR_STAFFID != null)
                {
                    var relationshipManager = context.TBL_STAFF.Where(x => x.STAFFID == accountOfficer.SUPERVISOR_STAFFID).FirstOrDefault();
                    if (relationshipManager != null)
                    {
                        emailList = emailList + ";" + relationshipManager.EMAIL;
                        if (relationshipManager.SUPERVISOR_STAFFID != null)
                        {
                            var zonalHead = context.TBL_STAFF.Where(x => x.STAFFID == relationshipManager.SUPERVISOR_STAFFID).FirstOrDefault();
                            if (zonalHead != null)
                            {
                                emailList = emailList + ";" + zonalHead.EMAIL;

                                var groupHead = context.TBL_STAFF.Where(x => x.STAFFID == zonalHead.SUPERVISOR_STAFFID).FirstOrDefault();

                                if (groupHead != null)
                                {
                                    emailList = emailList + ";" + groupHead.EMAIL;
                                }
                            }
                        }
                    }
                }

            }

            return emailList;
        }
        public void GetImminentMaturities()
        {
                // GetImminentMaturities method
                var staffList = externalAlertRepository.GetAccountOfficersWithImminentMaturities();
                var alertTitleInfo = context.TBL_ALERT_TITLE.Where(a => a.BINDINGMETHOD == "GetImminentMaturities").FirstOrDefault();

                var defaultEmail = "";
                if (alertTitleInfo.DEFAULTEMAIL != null)
                {
                    defaultEmail = ";" + alertTitleInfo.DEFAULTEMAIL;
                }
            if (staffList != null && staffList.Count() > 0)
            {

                List<AlertsViewModel> alerts = new List<AlertsViewModel>();
                foreach (var staff in staffList)
                {
                    AlertsViewModel alert = new AlertsViewModel();
                    var alertTitle = alertTitleInfo.TITLE;
                    var alertTemplate = alertTitleInfo.TEMPLATE;
                    string emailList = "";
                    var staffFullName = context.TBL_GLOBAL_EXPOSURE.Where(b => b.ACCOUNTOFFICERCODE == staff.misCode).Select(b => b.ACCOUNTOFFICERNAME).FirstOrDefault();

                    emailList = GetBusinessUsersEmails(staff.misCode);

                    List<int> days = new List<int> { 60, 90, 30, 21, 14, 7, 3, 1 };
                    var loanInformation = context.TBL_GLOBAL_EXPOSURE.Where(d => days.Contains(DbFunctions.DiffDays(DateTime.UtcNow, d.MATURITYDATE).Value) && d.ACCOUNTOFFICERCODE == staff.misCode).ToList();
                    var n = 0;
                    var result = $@"
                     <table cellpadding='0' cellspacing='0' border='1' width='800px'>
                        <tr>
                            <td><b>S/N</b></td>
                            <td><b>Customer Name</b></td>
                            <td><b>Reference Number</b></td>
                            <td><b>Amount</b></td>
                            <td><b>Maturity Date</b></td>
                        </tr>
                     ";
                    foreach (var t in loanInformation)
                    {
                        n++;

                        var amount = string.Format("{0:#,##.00}", Convert.ToDecimal(t.PRINCIPALOUTSTANDINGBALLCY));
                        var maturityDate = t.MATURITYDATE.ToString("dd-MM-yyyy");
                        result = result + $@"
                        <tr>
                            <td>{n}</td>
                            <td>{t.CUSTOMERNAME}</td>
                            <td>{t.REFERENCENUMBER}</td>
                            <td>{$"{amount}"}</td>
                            <td>{$"{maturityDate}"}</td>
                        </tr>
                        ";
                    }
                    result = result + $"</table>";

                    alertTemplate = alertTemplate.Replace("@{{accountOfficerName}}", staffFullName);
                    alertTemplate = alertTemplate.Replace("@{{accountNumbers}}", result);

                    var emailList2 = "benjamin.gbaaikye@fintraksoftware.com"; //emailList + GetAllStaffRoleEmails(alertTitleInfo.ALERTTITLEID) + defaultEmail;

                    alert.receiverEmailList.Add(emailList2);
                    alert.template = alertTemplate;
                    alert.alertTitle = alertTitle;
                    alert.canFire = true;

                    alerts.Add(alert);
                }

                SendAlertNotification(alerts);
            }
        }
        public void GetCreditCardMaturingObligations()
        {
            // GetCreditCardMaturingObligations method
            var staffCreditCardMaturingObligations = externalAlertRepository.GetCreditCardMaturingObligations();
            var alertTitleInfo = context.TBL_ALERT_TITLE.Where(a => a.BINDINGMETHOD == "GetCreditCardMaturingObligations").FirstOrDefault();

            var defaultEmail = "";
            if (alertTitleInfo.DEFAULTEMAIL != null)
            {
                defaultEmail = ";" + alertTitleInfo.DEFAULTEMAIL;
            }

            if (staffCreditCardMaturingObligations != null && staffCreditCardMaturingObligations.Count() > 0)
            {

                List<AlertsViewModel> alerts = new List<AlertsViewModel>();
                foreach (var staff in staffCreditCardMaturingObligations)
                {
                    AlertsViewModel alert = new AlertsViewModel();
                    var alertTitle = alertTitleInfo.TITLE;
                    var alertTemplate = alertTitleInfo.TEMPLATE;
                    string emailList = "";
                    var staffFullName = context.TBL_GLOBAL_EXPOSURE.Where(b => b.ACCOUNTOFFICERCODE == staff.misCode).Select(b => b.ACCOUNTOFFICERNAME).FirstOrDefault();

                    emailList = GetBusinessUsersEmails(staff.misCode);

                    List<int> days = new List<int> { 60, 89 };
                    var accountNumbers = context.TBL_GLOBAL_EXPOSURE.Where(d => days.Contains(DbFunctions.DiffDays(DateTime.UtcNow, d.MATURITYDATE).Value) && d.ACCOUNTOFFICERCODE == staff.misCode).ToList();
                    var n = 0;
                    var result = $@"
                      <table cellpadding='0' cellspacing='0' border='1' width='800px'>
                        <tr>
                            <td><b>S/N</b></td>
                            <td><b>Customer Name</b></td>
                            <td><b>Reference Number</b></td>
                            <td><b>Amount</b></td>
                             <td><b>Maturity Date</b></td>
                        </tr>
                     ";
                    foreach (var t in accountNumbers)
                    {
                        n++;
                        var amount = string.Format("{0:#,##.00}", Convert.ToDecimal(t.PRINCIPALOUTSTANDINGBALLCY));
                        var maturityDate = t.MATURITYDATE.ToString("dd-MM-yyyy");

                        result = result + $@"
                        <tr>
                            <td>{n}</td>
                            <td>{t.CUSTOMERNAME}</td>
                            <td>{t.REFERENCENUMBER}</td>
                            <td>{amount}</td>
                            <td>{maturityDate}</td>
                        </tr>
                        ";
                    }
                    result = result + $"</table>";

                    alertTemplate = alertTemplate.Replace("@{{accountOfficerName}}", staffFullName);
                    alertTemplate = alertTemplate.Replace("@{{accountNumbers}}", result);

                    emailList = emailList + GetAllStaffRoleEmails(alertTitleInfo.ALERTTITLEID) + defaultEmail;
                    alert.receiverEmailList.Add(emailList);
                    alert.template = alertTemplate;
                    alert.alertTitle = alertTitle;
                    alert.canFire = true;

                    alerts.Add(alert);
                }
                SendAlertNotification(alerts);
            }
        }
        public void GetUnpaidObligationReminder()
        {
            // GetUnpaidObligationReminder method
            var unpaidObligationReminder = externalAlertRepository.GetUnpaidObligationReminder();
            var alertTitleInfo = context.TBL_ALERT_TITLE.Where(a => a.BINDINGMETHOD == "GetUnpaidObligationReminder").FirstOrDefault();

            var defaultEmail = "";
            if (alertTitleInfo.DEFAULTEMAIL != null)
            {
                defaultEmail = ";" + alertTitleInfo.DEFAULTEMAIL;
            }
            if (unpaidObligationReminder != null && unpaidObligationReminder.Count() > 0)
            {
                List<AlertsViewModel> alerts = new List<AlertsViewModel>();
                foreach (var i in unpaidObligationReminder)
                {
                    AlertsViewModel alert = new AlertsViewModel();
                    var alertTitle = alertTitleInfo.TITLE;
                    var alertTemplate = alertTitleInfo.TEMPLATE;

                    string emailList = "";

                    emailList = GetBusinessUsersEmails(i.accountOfficerCode);

                    alertTemplate = alertTemplate.Replace("@{{customerName}}", i.customerName);
                    alertTemplate = alertTemplate.Replace("@{{facilityName}}", i.adjFacilityType);
                    alertTemplate = alertTemplate.Replace("@{{referenceNumber}}", i.referenceNumber);
                    
                    emailList = emailList + GetAllStaffRoleEmails(alertTitleInfo.ALERTTITLEID)+ defaultEmail;
                    alert.receiverEmailList.Add(emailList);
                    alert.template = alertTemplate;
                    alert.alertTitle = alertTitle;
                    alert.canFire = true;

                    alerts.Add(alert);
                }
                SendAlertNotification(alerts);
            }
        }
        public void GetExpiringFacilityReport()
        {
            // GetExpiringFacilityReport method
            var expiringFacilityReport = externalAlertRepository.GetExpiringFacilityReport();
            var alertTitleInfo = context.TBL_ALERT_TITLE.Where(a => a.BINDINGMETHOD == "GetExpiringFacilityReport").FirstOrDefault();

            var defaultEmail = "";
            if (alertTitleInfo.DEFAULTEMAIL != null)
            {
                defaultEmail = ";" + alertTitleInfo.DEFAULTEMAIL;
            }
            if (expiringFacilityReport != null && expiringFacilityReport.Count() > 0)
            {

                List<AlertsViewModel> alerts = new List<AlertsViewModel>();
                foreach (var i in expiringFacilityReport)
                {
                    AlertsViewModel alert = new AlertsViewModel();
                    var alertTitle = alertTitleInfo.TITLE;
                    var alertTemplate = alertTitleInfo.TEMPLATE;
                    var staffFullName = context.TBL_GLOBAL_EXPOSURE.Where(b => b.ACCOUNTOFFICERCODE == i.misCode).Select(b => b.ACCOUNTOFFICERNAME).FirstOrDefault();

                    string emailList = "";
                    emailList = GetBusinessUsersEmails(i.misCode);
                    List<int> days = new List<int> { 90 };
                    var accountNumbers = context.TBL_GLOBAL_EXPOSURE.Where(d => days.Contains(DbFunctions.DiffDays(DateTime.UtcNow, d.MATURITYDATE).Value) && d.ACCOUNTOFFICERCODE == i.misCode).ToList();
                    var n = 0;
                    var result = $@"
                      <table cellpadding='0' cellspacing='0' border='1' width='800px'>
                        <tr>
                            <td><b>S/N</b></td>
                            <td><b>Customer Name</b></td>
                            <td><b>Product</b></td>
                            <td><b>Facility</b></td>
                        </tr>
                     ";
                    foreach (var t in accountNumbers)
                    {
                        n++;
                        result = result + $@"
                        <tr>
                            <td>{n}</td>
                            <td>{t.CUSTOMERNAME}</td>
                            <td>{t.PRODUCTNAME}</td>
                            <td>{t.ADJFACILITYTYPE}</td>
                        </tr>
                        ";
                    }
                    result = result + $"</table>";

                    alertTemplate = alertTemplate.Replace("@{{accountOfficerName}}", i.misCode);
                    alertTemplate = alertTemplate.Replace("@{{facilityName}}", result);
                    
                    emailList = emailList + GetAllStaffRoleEmails(alertTitleInfo.ALERTTITLEID)+ defaultEmail;
                    alert.receiverEmailList.Add(emailList);
                    alert.template = alertTemplate;
                    alert.alertTitle = alertTitle;
                    alert.canFire = true;

                    alerts.Add(alert);
                }
                SendAlertNotification(alerts);
            }
        }
        public void GetLoanExpirationReminder()
        {
            // GetLoanExpirationReminder method
            var loanExpirationReminder = externalAlertRepository.GetLoanExpirationReminder();
            var alertTitleInfo = context.TBL_ALERT_TITLE.Where(a => a.BINDINGMETHOD == "GetLoanExpirationReminder").FirstOrDefault();

            var defaultEmail = "";
            if (alertTitleInfo.DEFAULTEMAIL != null)
            {
                defaultEmail = ";" + alertTitleInfo.DEFAULTEMAIL;
            }

            if (loanExpirationReminder != null && loanExpirationReminder.Count() > 0)
            {

                List<AlertsViewModel> alerts = new List<AlertsViewModel>();
                foreach (var i in loanExpirationReminder)
                {
                    AlertsViewModel alert = new AlertsViewModel();
                    var alertTitle = alertTitleInfo.TITLE;
                    var alertTemplate = alertTitleInfo.TEMPLATE;

                    string emailList = "";
                    alertTemplate = alertTemplate.Replace("@{{customerName}}", i.customerName);

                    emailList = i.customerName+defaultEmail;
                    alert.receiverEmailList.Add(emailList);
                    alert.template = alertTemplate;
                    alert.alertTitle = alertTitle;
                    alert.canFire = true;

                    alerts.Add(alert);
                }
                SendAlertNotification(alerts);
            }
        }
        public void GetLoanExpirationReminderAccountOfficer()
        {
            // GetLoanExpirationReminderAccountOfficer method
            var loanExpirationReminderAccountOfficer = externalAlertRepository.GetLoanExpirationReminderAccountOfficer();
            var alertTitleInfo = context.TBL_ALERT_TITLE.Where(a => a.BINDINGMETHOD == "GetLoanExpirationReminderAccountOfficer").FirstOrDefault();

            var defaultEmail = "";
            if (alertTitleInfo.DEFAULTEMAIL != null)
            {
                defaultEmail = ";" + alertTitleInfo.DEFAULTEMAIL;
            }

            if (loanExpirationReminderAccountOfficer != null && loanExpirationReminderAccountOfficer.Count() > 0)
            {

                List<AlertsViewModel> alerts = new List<AlertsViewModel>();
                foreach (var staff in loanExpirationReminderAccountOfficer)
                {
                    AlertsViewModel alert = new AlertsViewModel();
                    var alertTitle = alertTitleInfo.TITLE;
                    var alertTemplate = alertTitleInfo.TEMPLATE;
                    string emailList = "";
                    var staffFullName = context.TBL_GLOBAL_EXPOSURE.Where(b => b.ACCOUNTOFFICERCODE == staff.misCode).Select(b => b.ACCOUNTOFFICERNAME).FirstOrDefault();

                    emailList = GetBusinessUsersEmails(staff.misCode);

                    List<int> days = new List<int> { 30 };
                    var accountNumbers = context.TBL_GLOBAL_EXPOSURE.Where(d =>days.Contains(DbFunctions.DiffDays(DateTime.UtcNow, d.MATURITYDATE).Value) 
                                         && d.ACCOUNTOFFICERCODE == staff.misCode).ToList();
                   
                    var n = 0;
                    var result = $@"
                      <table cellpadding='0' cellspacing='0' border='1' width='800px'>
                        <tr>
                            <td><b>S/N</b></td>
                            <td><b>Customer Name</b></td>
                            <td><b>Reference Number</b></td>
                            <td><b>Amount</b></td>
                             <td><b>Maturity Date</b></td>
                        </tr>
                     ";
                    foreach (var t in accountNumbers)
                    {
                        n++;
                        var amount = string.Format("{0:#,##.00}", Convert.ToDecimal(t.AMOUNTDUE));
                        var maturityDate = t.MATURITYDATE.ToString("dd-MM-yyyy");
                        result = result + $@"
                        <tr>
                            <td>{n}</td>
                            <td>{t.CUSTOMERNAME}</td>
                            <td>{t.REFERENCENUMBER}</td>
                            <td>{amount}</td>
                            <td>{maturityDate}</td>
                        </tr>
                        ";
                    }
                    result = result + $"</table>";

                    alertTemplate = alertTemplate.Replace("@{{accountOfficerName}}", staffFullName);
                    alertTemplate = alertTemplate.Replace("@{{accountNumbers}}", result);

                    var emailList2 = "benjamin.gbaaikye@fintraksoftware.com";// emailList + GetAllStaffRoleEmails(alertTitleInfo.ALERTTITLEID) + defaultEmail;
                    alert.receiverEmailList.Add(emailList2);
                    alert.template = alertTemplate;
                    alert.alertTitle = alertTitle;
                    alert.canFire = true;

                    alerts.Add(alert);
                }
                SendAlertNotification(alerts);
            }
        }
        public void GetUnAuthorizedOverdraftReport()
        {
            // GetUnAuthorizedOverdraftReport method
            var unAuthorizedOverdraftReport = externalAlertRepository.GetUnAuthorizedOverdraftReport();
            var alertTitleInfo = context.TBL_ALERT_TITLE.Where(a => a.BINDINGMETHOD == "GetUnAuthorizedOverdraftReport").FirstOrDefault();

            var defaultEmail = "";
            if (alertTitleInfo.DEFAULTEMAIL != null)
            {
                defaultEmail = ";" + alertTitleInfo.DEFAULTEMAIL;
            }

            if (unAuthorizedOverdraftReport != null && unAuthorizedOverdraftReport.Count() > 0)
            {

                List<AlertsViewModel> alerts = new List<AlertsViewModel>();
                foreach (var i in unAuthorizedOverdraftReport)
                {
                    AlertsViewModel alert = new AlertsViewModel();
                    var alertTitle = alertTitleInfo.TITLE;
                    var alertTemplate = alertTitleInfo.TEMPLATE;

                    string emailList = "";
                    emailList = GetBusinessUsersEmails(i.accountOfficerCode);

                    alertTemplate = alertTemplate.Replace("@{{customerName}}", i.customerName);
                    alertTemplate = alertTemplate.Replace("@{{accountNumber}}", i.accountNumber);

                    emailList = emailList + GetAllStaffRoleEmails(alertTitleInfo.ALERTTITLEID) + defaultEmail;
                    alert.receiverEmailList.Add(emailList);
                    alert.template = alertTemplate;
                    alert.alertTitle = alertTitle;
                    alert.canFire = true;

                    alerts.Add(alert);
                }
                SendAlertNotification(alerts);
            }
        }
        public void GetOverlineMonitoringReport()
        {
            // GetOverlineMonitoringReport method
            var overlineMonitoringReport = externalAlertRepository.GetOverlineMonitoringReport();
            var alertTitleInfo = context.TBL_ALERT_TITLE.Where(a => a.BINDINGMETHOD == "GetOverlineMonitoringReport").FirstOrDefault();

            var defaultEmail = "";
            if (alertTitleInfo.DEFAULTEMAIL != null)
            {
                defaultEmail = ";" + alertTitleInfo.DEFAULTEMAIL;
            }
            if (overlineMonitoringReport != null && overlineMonitoringReport.Count() > 0)
            {

                List<AlertsViewModel> alerts = new List<AlertsViewModel>();
                foreach (var staff in overlineMonitoringReport)
                {
                    AlertsViewModel alert = new AlertsViewModel();
                    var alertTitle = alertTitleInfo.TITLE;
                    var alertTemplate = alertTitleInfo.TEMPLATE;
                    string emailList = "";
                    var staffFullName = context.TBL_GLOBAL_EXPOSURE.Where(b => b.ACCOUNTOFFICERCODE == staff.misCode).Select(b => b.ACCOUNTOFFICERNAME).FirstOrDefault();

                    emailList = GetBusinessUsersEmails(staff.misCode);

                    var loanInformation = context.TBL_GLOBAL_EXPOSURE.Where(d => d.ADJFACILITYTYPE == "OVERDRAFT" && DbFunctions.TruncateTime(d.MATURITYDATE) == DbFunctions.TruncateTime(d.BOOKINGDATE)
                                         && DbFunctions.DiffDays(DateTime.UtcNow, d.MATURITYDATE).Value > 90 && d.ACCOUNTOFFICERCODE == staff.misCode).ToList();
                    var n = 0;
                    var result = $@"
                     <table cellpadding='0' cellspacing='0' border='1' width='800px'>
                        <tr>
                            <td><b>S/N</b></td>
                            <td><b>Customer Name</b></td>
                            <td><b>Reference Number</b></td>
                            <td><b>Amount</b></td>
                            <td><b>Maturity Date</b></td>
                        </tr>
                     ";
                    foreach (var t in loanInformation)
                    {
                        n++;

                        var amount = string.Format("{0:#,##.00}", Convert.ToDecimal(t.PRINCIPALOUTSTANDINGBALLCY));
                        var maturityDate = t.MATURITYDATE.ToString("dd-MM-yyyy");
                        result = result + $@"
                        <tr>
                            <td>{n}</td>
                            <td>{t.CUSTOMERNAME}</td>
                            <td>{t.REFERENCENUMBER}</td>
                            <td>{amount}</td>
                            <td>{maturityDate}</td>
                        </tr>
                        ";
                    }
                    result = result + $"</table>";

                    alertTemplate = alertTemplate.Replace("@{{accountOfficerName}}", staffFullName);
                    alertTemplate = alertTemplate.Replace("@{{accountNumbers}}", result);

                    emailList = emailList + GetAllStaffRoleEmails(alertTitleInfo.ALERTTITLEID) + defaultEmail;

                    alert.receiverEmailList.Add(emailList);
                    alert.template = alertTemplate;
                    alert.alertTitle = alertTitle;
                    alert.canFire = true;

                    alerts.Add(alert);
                }

                SendAlertNotification(alerts);
            }
        }
        public void GetCreditCardDelinquencyMonitoringReport()
        {
            // GetCreditCardDelinquencyMonitoringReport method
            var creditCardDelinquencyMonitoringReport = externalAlertRepository.GetCreditCardDelinquencyMonitoringReport();
            var alertTitleInfo = context.TBL_ALERT_TITLE.Where(a => a.BINDINGMETHOD == "GetCreditCardDelinquencyMonitoringReport").FirstOrDefault();

            var defaultEmail = "";
            if (alertTitleInfo.DEFAULTEMAIL != null)
            {
                defaultEmail = ";" + alertTitleInfo.DEFAULTEMAIL;
            }
            if (creditCardDelinquencyMonitoringReport != null && creditCardDelinquencyMonitoringReport.Count() > 0)
            {

                List<AlertsViewModel> alerts = new List<AlertsViewModel>();
                foreach (var staff in creditCardDelinquencyMonitoringReport)
                {
                    AlertsViewModel alert = new AlertsViewModel();
                    var alertTitle = alertTitleInfo.TITLE;
                    var alertTemplate = alertTitleInfo.TEMPLATE;
                    string emailList = "";
                    var staffFullName = context.TBL_GLOBAL_EXPOSURE.Where(b => b.ACCOUNTOFFICERCODE == staff.misCode).Select(b => b.ACCOUNTOFFICERNAME).FirstOrDefault();

                    emailList = GetBusinessUsersEmails(staff.misCode);

                    var loanInformation = context.TBL_GLOBAL_EXPOSURE.Where(d => d.UNPODAYSOVERDUE > 0 && d.ACCOUNTOFFICERCODE == staff.misCode).ToList();
                    var n = 0;
                    var result = $@"
                     <table cellpadding='0' cellspacing='0' border='1' width='800px'>
                        <tr>
                            <td><b>S/N</b></td>
                            <td><b>Customer Name</b></td>
                            <td><b>Reference Number</b></td>
                            <td><b>Amount</b></td>
                            <td><b>Maturity Date</b></td>
                        </tr>
                     ";
                    foreach (var t in loanInformation)
                    {
                        n++;
                        var amount = string.Format("{0:#,##.00}", Convert.ToDecimal(t.PRINCIPALOUTSTANDINGBALLCY));
                        var maturityDate = t.MATURITYDATE.ToString("dd-MM-yyyy");
                        result = result + $@"
                        <tr>
                            <td>{n}</td>
                            <td>{t.CUSTOMERNAME}</td>
                            <td>{t.REFERENCENUMBER}</td>
                            <td>{amount}</td>
                            <td>{maturityDate}</td>
                        </tr>
                        ";
                    }
                    result = result + $"</table>";

                    alertTemplate = alertTemplate.Replace("@{{accountOfficerName}}", staffFullName);
                    alertTemplate = alertTemplate.Replace("@{{accountNumbers}}", result);

                    emailList = emailList + GetAllStaffRoleEmails(alertTitleInfo.ALERTTITLEID) + defaultEmail;

                    alert.receiverEmailList.Add(emailList);
                    alert.template = alertTemplate;
                    alert.alertTitle = alertTitle;
                    alert.canFire = true;

                    alerts.Add(alert);
                }

                SendAlertNotification(alerts);
            }
        }
        public void GetPastDueObligationsReminder()
        {
            // GetPastDueObligationsReminder method
            var pastDueObligationsReminder = externalAlertRepository.GetPastDueObligationsReminder();
            var alertTitleInfo = context.TBL_ALERT_TITLE.Where(a => a.BINDINGMETHOD == "GetPastDueObligationsReminder").FirstOrDefault();

            var defaultEmail = "";
            if (alertTitleInfo.DEFAULTEMAIL != null)
            {
                defaultEmail = ";" + alertTitleInfo.DEFAULTEMAIL;
            }
            if (pastDueObligationsReminder != null && pastDueObligationsReminder.Count() > 0)
            {

                List<AlertsViewModel> alerts = new List<AlertsViewModel>();
                foreach (var staff in pastDueObligationsReminder)
                {
                    AlertsViewModel alert = new AlertsViewModel();
                    var alertTitle = alertTitleInfo.TITLE;
                    var alertTemplate = alertTitleInfo.TEMPLATE;
                    string emailList = "";
                    var staffFullName = context.TBL_GLOBAL_EXPOSURE.Where(b => b.ACCOUNTOFFICERCODE == staff.misCode).Select(b => b.ACCOUNTOFFICERNAME).FirstOrDefault();

                    emailList = GetBusinessUsersEmails(staff.misCode);

                    var loanInformation = context.TBL_GLOBAL_EXPOSURE.Where(d => d.UNPODAYSOVERDUE > 0 && d.ACCOUNTOFFICERCODE == staff.misCode).ToList();
                    var n = 0;
                    var result = $@"
                     <table cellpadding='0' cellspacing='0' border='1' width='800px'>
                        <tr>
                            <td><b>S/N</b></td>
                            <td><b>Customer Name</b></td>
                            <td><b>Reference Number</b></td>
                            <td><b>Amount</b></td>
                            <td><b>Number Of Days</b></td>
                        </tr>
                     ";
                    foreach (var t in loanInformation)
                    {
                        n++;
                        var amount = string.Format("{0:#,##.00}", Convert.ToDecimal(t.AMOUNTDUE));
                        result = result + $@"
                        <tr>
                            <td>{n}</td>
                            <td>{t.CUSTOMERNAME}</td>
                            <td>{t.REFERENCENUMBER}</td>
                            <td>{amount}</td>
                            <td>{t.UNPODAYSOVERDUE}</td>
                        </tr>
                        ";
                    }
                    result = result + $"</table>";

                    alertTemplate = alertTemplate.Replace("@{{accountOfficerName}}", staffFullName);
                    alertTemplate = alertTemplate.Replace("@{{accountNumbers}}", result);

                    emailList = emailList + GetAllStaffRoleEmails(alertTitleInfo.ALERTTITLEID) + defaultEmail;

                    alert.receiverEmailList.Add(emailList);
                    alert.template = alertTemplate;
                    alert.alertTitle = alertTitle;
                    alert.canFire = true;

                    alerts.Add(alert);
                }

                SendAlertNotification(alerts);
            }
        }
        public void GetRiskAssetsReportNotification()
        {
            // GetRiskAssetsReportNotification method
            var riskAssetsReportNotification = externalAlertRepository.GetRiskAssetsReportNotification();
            TBL_ALERT_TITLE alertTitleInfo;
            if (riskAssetsReportNotification == false)
            {
                 alertTitleInfo = context.TBL_ALERT_TITLE.Where(a => a.BINDINGMETHOD == "GetRiskAssetsReportNotification").FirstOrDefault();
            }
            else
            {
                alertTitleInfo = context.TBL_ALERT_TITLE.Where(a => a.BINDINGMETHOD == "GetRiskAssetsReportReminder").FirstOrDefault();

            }
            var defaultEmail = "";
            if (alertTitleInfo.DEFAULTEMAIL != null)
            {
                defaultEmail = ";" + alertTitleInfo.DEFAULTEMAIL;
            }
            
                List<AlertsViewModel> alerts = new List<AlertsViewModel>();
                
                    AlertsViewModel alert = new AlertsViewModel();
                    var alertTitle = alertTitleInfo.TITLE;
                    var alertTemplate = alertTitleInfo.TEMPLATE;
                                  

                    var emailList =  GetAllStaffRoleEmails(alertTitleInfo.ALERTTITLEID) + defaultEmail;

                    alert.receiverEmailList.Add(emailList);
                    alert.template = alertTemplate;
                    alert.alertTitle = alertTitle;
                    alert.canFire = true;

                    alerts.Add(alert);
                
                SendAlertNotification(alerts);
        }
        public void GetDashboardReportNotification()
        {
            // GetDashboardReportNotification method
            var dashboardReportNotification = externalAlertRepository.GetDashboardReportNotification();
            TBL_ALERT_TITLE alertTitleInfo;
            if (dashboardReportNotification == false)
            {
                alertTitleInfo = context.TBL_ALERT_TITLE.Where(a => a.BINDINGMETHOD == "GetDashboardReportNotification").FirstOrDefault();
            }
            else
            {
                alertTitleInfo = context.TBL_ALERT_TITLE.Where(a => a.BINDINGMETHOD == "GetDashboardReportReminder").FirstOrDefault();

            }
            var defaultEmail = "";
            if (alertTitleInfo.DEFAULTEMAIL != null)
            {
                defaultEmail = ";" + alertTitleInfo.DEFAULTEMAIL;
            }

            List<AlertsViewModel> alerts = new List<AlertsViewModel>();

            AlertsViewModel alert = new AlertsViewModel();
            var alertTitle = alertTitleInfo.TITLE;
            var alertTemplate = alertTitleInfo.TEMPLATE;


            var emailList = GetAllStaffRoleEmails(alertTitleInfo.ALERTTITLEID) + defaultEmail;

            alert.receiverEmailList.Add(emailList);
            alert.template = alertTemplate;
            alert.alertTitle = alertTitle;
            alert.canFire = true;

            alerts.Add(alert);

            SendAlertNotification(alerts);
        }
        public void GetCACReport()
        {
            // GetDashboardReportNotification method
            var CACReport = externalAlertRepository.GetCACReport();
            TBL_ALERT_TITLE alertTitleInfo;
            if (CACReport == false)
            {
                alertTitleInfo = context.TBL_ALERT_TITLE.Where(a => a.BINDINGMETHOD == "GetCACReport").FirstOrDefault();
            }
            else
            {
                alertTitleInfo = context.TBL_ALERT_TITLE.Where(a => a.BINDINGMETHOD == "GetCACReportReminder").FirstOrDefault();

            }
            var defaultEmail = "";
            if (alertTitleInfo.DEFAULTEMAIL != null)
            {
                defaultEmail = ";" + alertTitleInfo.DEFAULTEMAIL;
            }

            List<AlertsViewModel> alerts = new List<AlertsViewModel>();

            AlertsViewModel alert = new AlertsViewModel();
            var alertTitle = alertTitleInfo.TITLE;
            var alertTemplate = alertTitleInfo.TEMPLATE;


            var emailList = GetAllStaffRoleEmails(alertTitleInfo.ALERTTITLEID) + defaultEmail;

            alert.receiverEmailList.Add(emailList);
            alert.template = alertTemplate;
            alert.alertTitle = alertTitle;
            alert.canFire = true;

            alerts.Add(alert);

            SendAlertNotification(alerts);
        }
        public void GetOverlineCreditCardPosition()
        {
            // GetOverlineCreditCardPosition method
            var overlineCreditCardPosition = externalAlertRepository.GetOverlineCreditCardPosition();
            var alertTitleInfo = context.TBL_ALERT_TITLE.Where(a => a.BINDINGMETHOD == "GetOverlineCreditCardPosition").FirstOrDefault();

            var defaultEmail = "";
            if (alertTitleInfo.DEFAULTEMAIL != null)
            {
                defaultEmail = ";" + alertTitleInfo.DEFAULTEMAIL;
            }

            if (overlineCreditCardPosition != null && overlineCreditCardPosition.Count() > 0)
            {

                List<AlertsViewModel> alerts = new List<AlertsViewModel>();
                foreach (var staff in overlineCreditCardPosition)
                {
                    AlertsViewModel alert = new AlertsViewModel();
                    var alertTitle = alertTitleInfo.TITLE;
                    var alertTemplate = alertTitleInfo.TEMPLATE;
                    string emailList = "";
                    var staffFullName = context.TBL_GLOBAL_EXPOSURE.Where(b => b.ACCOUNTOFFICERCODE == staff.misCode).Select(b => b.ACCOUNTOFFICERNAME).FirstOrDefault();

                    emailList = GetBusinessUsersEmails(staff.misCode);

                    var accountNumbers = context.TBL_GLOBAL_EXPOSURE.Where(d => DbFunctions.TruncateTime(d.MATURITYDATE) == DbFunctions.TruncateTime(d.BOOKINGDATE)
                                         && d.ADJFACILITYTYPE == "OVERDRAFT" && d.ACCOUNTOFFICERCODE == staff.misCode).ToList();
                    var n = 0;
                    var result = $@"
                      <table cellpadding='0' cellspacing='0' border='1' width='800px'>
                        <tr>
                            <td><b>S/N</b></td>
                            <td><b>Customer Name</b></td>
                            <td><b>Reference Number</b></td>
                            <td><b>Amount</b></td>
                             <td><b>Maturity Date</b></td>
                        </tr>
                     ";
                    foreach (var t in accountNumbers)
                    {
                        n++;
                        var amount = string.Format("{0:#,##.00}", Convert.ToDecimal(t.PRINCIPALOUTSTANDINGBALLCY));
                        var maturityDate = t.MATURITYDATE.ToString("dd-MM-yyyy");

                        result = result + $@"
                        <tr>
                            <td>{n}</td>
                            <td>{t.CUSTOMERNAME}</td>
                            <td>{t.REFERENCENUMBER}</td>
                            <td>{amount}</td>
                            <td>{maturityDate}</td>
                        </tr>
                        ";
                    }
                    result = result + $"</table>";

                    alertTemplate = alertTemplate.Replace("@{{accountOfficerName}}", staffFullName);
                    alertTemplate = alertTemplate.Replace("@{{accountNumbers}}", result);

                    emailList = emailList + GetAllStaffRoleEmails(alertTitleInfo.ALERTTITLEID) + defaultEmail;
                    alert.receiverEmailList.Add(emailList);
                    alert.template = alertTemplate;
                    alert.alertTitle = alertTitle;
                    alert.canFire = true;

                    alerts.Add(alert);
                }
                SendAlertNotification(alerts);
            }
        }
        public void GetPastDueFacilitiesNotification()
         {
             // GetPastDueFacilitiesNotification method
             var pastDueFacilitiesNotification = externalAlertRepository.GetPastDueFacilitiesNotification();
             var alertTitleInfo = context.TBL_ALERT_TITLE.Where(a => a.BINDINGMETHOD == "GetPastDueFacilitiesNotification").FirstOrDefault();

             var defaultEmail = "";
             if (alertTitleInfo.DEFAULTEMAIL != null)
             {
                 defaultEmail = ";" + alertTitleInfo.DEFAULTEMAIL;
             }

             if (pastDueFacilitiesNotification != null && pastDueFacilitiesNotification.Count() > 0)
             {

                 List<AlertsViewModel> alerts = new List<AlertsViewModel>();
                 foreach (var staff in pastDueFacilitiesNotification)
                 {
                     AlertsViewModel alert = new AlertsViewModel();
                     var alertTitle = alertTitleInfo.TITLE;
                     var alertTemplate = alertTitleInfo.TEMPLATE;
                     string emailList = "";
                     var staffFullName = context.TBL_GLOBAL_EXPOSURE.Where(b => b.ACCOUNTOFFICERCODE == staff.misCode).Select(b => b.ACCOUNTOFFICERNAME).FirstOrDefault();

                     emailList = GetBusinessUsersEmails(staff.misCode);

                     var accountNumbers = context.TBL_GLOBAL_EXPOSURE.Where(d => d.EXPIRYBANDID > 0 && d.ACCOUNTOFFICERCODE == staff.misCode).ToList();
                     decimal totalPastDue = 0;
                     var n = 0;
                     var result = $@"
                      <table cellpadding='0' cellspacing='0' border='1' width='800px'>
                         < tr>
                             <td><b>S/N</b></td>
                             <td><b>Customer Name</b></td>
                             <td><b>Reference Number</b></td>
                             <td><b>Amount</b></td>
                              <td><b>Expiring Band</b></td>
                         </tr>
                      ";
                     foreach (var t in accountNumbers)
                     {
                         n++;
                         totalPastDue = totalPastDue+ Convert.ToDecimal(t.AMOUNTDUE);
                         var amount = string.Format("{0:#,##.00}", Convert.ToDecimal(t.AMOUNTDUE));
                        result = result + $@"
                         <tr>
                             <td>{n}</td>
                             <td>{t.CUSTOMERNAME}</td>
                             <td>{t.REFERENCENUMBER}</td>
                             <td>{amount}</td>
                             <td>{t.EXPIRINGBAND}</td>
                         </tr>
                         ";
                     }
                     result = result + $"</table>";

                     alertTemplate = alertTemplate.Replace("@{{accountOfficerName}}", staffFullName);
                     alertTemplate = alertTemplate.Replace("@{{accountNumbers}}", result);
                     alertTemplate = alertTemplate.Replace("@{{totalPastDueFacility}}", totalPastDue.ToString());

                     emailList = emailList + GetAllStaffRoleEmails(alertTitleInfo.ALERTTITLEID) + defaultEmail;
                     alert.receiverEmailList.Add(emailList);
                     alert.template = alertTemplate;
                     alert.alertTitle = alertTitle;
                     alert.canFire = true;

                     alerts.Add(alert);
                 }
                 SendAlertNotification(alerts);
             }
         }
        public void GetExpiredFacilityNotification()
         {
             // GetExpiredFacilityNotification method
             var expiredFacilityNotification = externalAlertRepository.GetExpiredFacilityNotification();
             var alertTitleInfo = context.TBL_ALERT_TITLE.Where(a => a.BINDINGMETHOD == "GetExpiredFacilityNotification").FirstOrDefault();

             var defaultEmail = "";
             if (alertTitleInfo.DEFAULTEMAIL != null)
             {
                 defaultEmail = ";" + alertTitleInfo.DEFAULTEMAIL;
             }

             if (expiredFacilityNotification != null && expiredFacilityNotification.Count() > 0)
             {

                 List<AlertsViewModel> alerts = new List<AlertsViewModel>();
                 foreach (var staff in expiredFacilityNotification)
                 {
                     AlertsViewModel alert = new AlertsViewModel();
                     var alertTitle = alertTitleInfo.TITLE;
                     var alertTemplate = alertTitleInfo.TEMPLATE;
                     string emailList = "";
                     var staffFullName = context.TBL_GLOBAL_EXPOSURE.Where(b => b.ACCOUNTOFFICERCODE == staff.misCode).Select(b => b.ACCOUNTOFFICERNAME).FirstOrDefault();

                     emailList = GetBusinessUsersEmails(staff.misCode);

                     var accountNumbers = context.TBL_GLOBAL_EXPOSURE.Where(d => d.EXPIRYBANDID >= 4 && d.ACCOUNTOFFICERCODE == staff.misCode).ToList();
                     var n = 0;
                     var result = $@"
                       <table cellpadding='0' cellspacing='0' border='1' width='800px'>
                         <tr>
                             <td><b>S/N</b></td>
                             <td><b>Customer Name</b></td>
                             <td><b>Reference Number</b></td>
                             <td><b>Amount</b></td>
                              <td><b>Expiring Band</b></td>
                         </tr>
                      ";
                     foreach (var t in accountNumbers)
                     {
                         n++;
                         var amount = string.Format("{0:#,##.00}", Convert.ToDecimal(t.AMOUNTDUE));

                         result = result + $@"
                         <tr>
                             <td>{n}</td>
                             <td>{t.CUSTOMERNAME}</td>
                             <td>{t.REFERENCENUMBER}</td>
                             <td>{amount}</td>
                             <td>{t.EXPIRINGBAND}</td>
                         </tr>
                         ";
                     }
                     result = result + $"</table>";

                     alertTemplate = alertTemplate.Replace("@{{accountOfficerName}}", staffFullName);
                     alertTemplate = alertTemplate.Replace("@{{accountNumbers}}", result);

                     emailList = emailList + GetAllStaffRoleEmails(alertTitleInfo.ALERTTITLEID) + defaultEmail;
                     alert.receiverEmailList.Add(emailList);
                     alert.template = alertTemplate;
                     alert.alertTitle = alertTitle;
                     alert.canFire = true;

                     alerts.Add(alert);
                 }
                 SendAlertNotification(alerts);
             }
         }
        public void GetLoanRepaymentReminder()
         {
             // GetLoanRepaymentReminder method
             var loanRepaymentReminder = externalAlertRepository.GetLoanRepaymentReminder();
             var alertTitleInfo = context.TBL_ALERT_TITLE.Where(a => a.BINDINGMETHOD == "GetLoanRepaymentReminder").FirstOrDefault();

             var defaultEmail = "";
             if (alertTitleInfo.DEFAULTEMAIL != null)
             {
                 defaultEmail = ";" + alertTitleInfo.DEFAULTEMAIL;
             }

             if (loanRepaymentReminder != null && loanRepaymentReminder.Count() > 0)
             {

                 List<AlertsViewModel> alerts = new List<AlertsViewModel>();
                 foreach (var i in loanRepaymentReminder)
                 {
                     AlertsViewModel alert = new AlertsViewModel();
                     var alertTitle = alertTitleInfo.TITLE;
                     var alertTemplate = alertTitleInfo.TEMPLATE;

                     string emailList = "";
                     alertTemplate = alertTemplate.Replace("@{{customerName}}", i.customerName);
                     alertTemplate = alertTemplate.Replace("@{{maturityBand}}", i.customerName);

                     emailList = i.customerName + defaultEmail;
                     alert.receiverEmailList.Add(emailList);
                     alert.template = alertTemplate;
                     alert.alertTitle = alertTitle;
                     alert.canFire = true;

                     alerts.Add(alert);
                 }
                 SendAlertNotification(alerts);
             }
         }
        public void GetLoanRepaymentReminderAccountOfficer()
         {
             // GetLoanRepaymentReminderAccountOfficer method
             var loanRepaymentReminderAccountOfficer = externalAlertRepository.GetLoanRepaymentReminderAccountOfficer();
             var alertTitleInfo = context.TBL_ALERT_TITLE.Where(a => a.BINDINGMETHOD == "GetLoanExpirationReminderAccountOfficer").FirstOrDefault();

             var defaultEmail = "";
             if (alertTitleInfo.DEFAULTEMAIL != null)
             {
                 defaultEmail = ";" + alertTitleInfo.DEFAULTEMAIL;
             }

             if (loanRepaymentReminderAccountOfficer != null && loanRepaymentReminderAccountOfficer.Count() > 0)
             {

                 List<AlertsViewModel> alerts = new List<AlertsViewModel>();
                 foreach (var staff in loanRepaymentReminderAccountOfficer)
                 {
                     AlertsViewModel alert = new AlertsViewModel();
                     var alertTitle = alertTitleInfo.TITLE;
                     var alertTemplate = alertTitleInfo.TEMPLATE;
                     string emailList = "";
                     var staffFullName = context.TBL_GLOBAL_EXPOSURE.Where(b => b.ACCOUNTOFFICERCODE == staff.misCode).Select(b => b.ACCOUNTOFFICERNAME).FirstOrDefault();

                     emailList = GetBusinessUsersEmails(staff.misCode);
                     var accountNumbers = context.TBL_GLOBAL_EXPOSURE.Where(d => d.UNPODAYSOVERDUE > 0 && d.ACCOUNTOFFICERCODE == staff.misCode).ToList();

                     var n = 0;
                     var result = $@"
                       <table cellpadding='0' cellspacing='0' border='1' width='800px'>
                         <tr>
                             <td><b>S/N</b></td>
                             <td><b>Customer Name</b></td>
                             <td><b>Reference Number</b></td>
                             <td><b>Amount</b></td>
                              <td><b>Schedule Day</b></td>
                         </tr>
                      ";
                     foreach (var t in accountNumbers)
                     {
                         n++;
                         var amount = string.Format("{0:#,##.00}", Convert.ToDecimal(t.AMOUNTDUE));
                         var scheduleDueDate = t.SCHEDULEDUEDATE.ToString("dd-MM-yyyy");
                         result = result + $@"
                         <tr>
                             <td>{n}</td>
                             <td>{t.CUSTOMERNAME}</td>
                             <td>{t.REFERENCENUMBER}</td>
                             <td>{amount}</td>
                             <td>{scheduleDueDate}</td>
                         </tr>
                         ";
                     }
                     result = result + $"</table>";

                     alertTemplate = alertTemplate.Replace("@{{accountOfficerName}}", staffFullName);
                     alertTemplate = alertTemplate.Replace("@{{accountNumber}}", result);

                     emailList = emailList + GetAllStaffRoleEmails(alertTitleInfo.ALERTTITLEID) + defaultEmail;
                     alert.receiverEmailList.Add(emailList);
                     alert.template = alertTemplate;
                     alert.alertTitle = alertTitle;
                     alert.canFire = true;

                     alerts.Add(alert);
                 }
                 SendAlertNotification(alerts);
             }
         }
        public void GetOverlineReminder()
        {
            // GetOverlineReminder method
            var overlineReminder = externalAlertRepository.GetOverlineReminder();
            var alertTitleInfo = context.TBL_ALERT_TITLE.Where(a => a.BINDINGMETHOD == "GetOverlineReminder").FirstOrDefault();

            var defaultEmail = "";
            if (alertTitleInfo.DEFAULTEMAIL != null)
            {
                defaultEmail = ";" + alertTitleInfo.DEFAULTEMAIL;
            }
            if (overlineReminder != null && overlineReminder.Count() > 0)
            {

                List<AlertsViewModel> alerts = new List<AlertsViewModel>();
                foreach (var staff in overlineReminder)
                {
                    AlertsViewModel alert = new AlertsViewModel();
                    var alertTitle = alertTitleInfo.TITLE;
                    var alertTemplate = alertTitleInfo.TEMPLATE;
                    string emailList = "";
                    var staffFullName = context.TBL_GLOBAL_EXPOSURE.Where(b => b.ACCOUNTOFFICERCODE == staff.misCode).Select(b => b.ACCOUNTOFFICERNAME).FirstOrDefault();

                    emailList = GetBusinessUsersEmails(staff.misCode);

                    var loanInformation = context.TBL_GLOBAL_EXPOSURE.Where(d => DbFunctions.TruncateTime(d.MATURITYDATE) == DbFunctions.TruncateTime(d.BOOKINGDATE) && d.ADJFACILITYTYPE == "OVERDRAFT"
                                          && d.ACCOUNTOFFICERCODE == staff.misCode).ToList();
                    var n = 0;
                    var result = $@"
                     <table cellpadding='0' cellspacing='0' border='1' width='800px'>
                        <tr>
                            <td><b>S/N</b></td>
                            <td><b>Customer Name</b></td>
                            <td><b>Reference Number</b></td>
                            <td><b>Facility</b></td>
                        </tr>
                     ";
                    foreach (var t in loanInformation)
                    {
                        n++;

                        var amount = string.Format("{0:#,##.00}", Convert.ToDecimal(t.LOANAMOUNYLCY));
                        //var maturityDate = t.MATURITYDATE.ToString("dd-MM-yyyy");
                        result = result + $@"
                        <tr>
                            <td>{n}</td>
                            <td>{t.CUSTOMERNAME}</td>
                            <td>{t.REFERENCENUMBER}</td>
                            <td>{t.ADJFACILITYTYPE}</td>
                        </tr>
                        ";
                    }
                    result = result + $"</table>";

                    alertTemplate = alertTemplate.Replace("@{{accountOfficeName}}", staffFullName);
                    alertTemplate = alertTemplate.Replace("@{{accountNumbers}}", result);

                    emailList = emailList + GetAllStaffRoleEmails(alertTitleInfo.ALERTTITLEID) + defaultEmail;

                    alert.receiverEmailList.Add(emailList);
                    alert.template = alertTemplate;
                    alert.alertTitle = alertTitle;
                    alert.canFire = true;

                    alerts.Add(alert);
                }

                SendAlertNotification(alerts);
            }
        }
        public void GetUnpaidObligationReminderAccountOfficer()
        {
            // GetUnpaidObligationReminderAccountOfficer method
            var unpaidObligationReminderAccountOfficer = externalAlertRepository.GetUnpaidObligationReminderAccountOfficer();
            var alertTitleInfo = context.TBL_ALERT_TITLE.Where(a => a.BINDINGMETHOD == "GetUnpaidObligationReminderAccountOfficer").FirstOrDefault();

            var defaultEmail = "";
            if (alertTitleInfo.DEFAULTEMAIL != null)
            {
                defaultEmail = ";" + alertTitleInfo.DEFAULTEMAIL;
            }

            if (unpaidObligationReminderAccountOfficer != null && unpaidObligationReminderAccountOfficer.Count() > 0)
            {

                List<AlertsViewModel> alerts = new List<AlertsViewModel>();
                foreach (var staff in unpaidObligationReminderAccountOfficer)
                {
                    AlertsViewModel alert = new AlertsViewModel();
                    var alertTitle = alertTitleInfo.TITLE;
                    var alertTemplate = alertTitleInfo.TEMPLATE;
                    string emailList = "";
                    var staffFullName = context.TBL_GLOBAL_EXPOSURE.Where(b => b.ACCOUNTOFFICERCODE == staff.misCode).Select(b => b.ACCOUNTOFFICERNAME).FirstOrDefault();

                    emailList = GetBusinessUsersEmails(staff.misCode);

                    List<int> days = new List<int> { 30 };
                    var accountNumbers = context.TBL_GLOBAL_EXPOSURE.Where(d => d.TOTALUNPAIDOBLIGATION > 0 && d.ACCOUNTOFFICERCODE == staff.misCode).ToList();

                    var n = 0;
                    var result = $@"
                      <table cellpadding='0' cellspacing='0' border='1' width='800px'>
                        <tr>
                            <td><b>S/N</b></td>
                            <td><b>Customer Name</b></td>
                            <td><b>Reference Number</b></td>
                            <td><b>Amount</b></td>
                        </tr>
                     ";
                    foreach (var t in accountNumbers)
                    {
                        n++;
                        var amount = string.Format("{0:#,##.00}", Convert.ToDecimal(t.TOTALUNPAIDOBLIGATION));
                        result = result + $@"
                        <tr>
                            <td>{n}</td>
                            <td>{t.CUSTOMERNAME}</td>
                            <td>{t.REFERENCENUMBER}</td>
                            <td>{amount}</td>
                        </tr>
                        ";
                    }
                    result = result + $"</table>";

                    alertTemplate = alertTemplate.Replace("@{{accountOfficerNumber}}", staffFullName);
                    alertTemplate = alertTemplate.Replace("@{{accountNumbers}}", result);

                    emailList = emailList + GetAllStaffRoleEmails(alertTitleInfo.ALERTTITLEID) + defaultEmail;
                    alert.receiverEmailList.Add(emailList);
                    alert.template = alertTemplate;
                    alert.alertTitle = alertTitle;
                    alert.canFire = true;

                    alerts.Add(alert);
                }
                SendAlertNotification(alerts);
            }
        }
        public void GetMaturingObligationsReport()
        {
            // GetMaturingObligationsReport method
            var maturingObligationsReport = externalAlertRepository.GetMaturingObligationsReport();
            var alertTitleInfo = context.TBL_ALERT_TITLE.Where(a => a.BINDINGMETHOD == "GetMaturingObligationsReport").FirstOrDefault();

            var defaultEmail = "";
            if (alertTitleInfo.DEFAULTEMAIL != null)
            {
                defaultEmail = ";" + alertTitleInfo.DEFAULTEMAIL;
            }

            if (maturingObligationsReport != null && maturingObligationsReport.Count() > 0)
            {

                List<AlertsViewModel> alerts = new List<AlertsViewModel>();
                foreach (var staff in maturingObligationsReport)
                {
                    AlertsViewModel alert = new AlertsViewModel();
                    var alertTitle = alertTitleInfo.TITLE;
                    var alertTemplate = alertTitleInfo.TEMPLATE;
                    string emailList = "";
                    var staffFullName = context.TBL_GLOBAL_EXPOSURE.Where(b => b.ACCOUNTOFFICERCODE == staff.misCode).Select(b => b.ACCOUNTOFFICERNAME).FirstOrDefault();

                    emailList = GetBusinessUsersEmails(staff.misCode);

                    List<int> days = new List<int> { 90 };
                    var accountNumbers = context.TBL_GLOBAL_EXPOSURE.Where(d => days.Contains(DbFunctions.DiffDays(DateTime.UtcNow, d.MATURITYDATE).Value) && d.ACCOUNTOFFICERCODE == staff.misCode).ToList();
                    var n = 0;
                    var result = $@"
                      <table cellpadding='0' cellspacing='0' border='1' width='800px'>
                        <tr>
                            <td><b>S/N</b></td>
                            <td><b>Customer Name</b></td>
                            <td><b>Reference Number</b></td>
                            <td><b>Amount</b></td>
                             <td><b>Maturity Date</b></td>
                        </tr>
                     ";
                    foreach (var t in accountNumbers)
                    {
                        n++;
                        var amount = string.Format("{0:#,##.00}", Convert.ToDecimal(t.PRINCIPALOUTSTANDINGBALLCY));
                        var maturityDate = t.MATURITYDATE.ToString("dd-MM-yyyy");

                        result = result + $@"
                        <tr>
                            <td>{n}</td>
                            <td>{t.CUSTOMERNAME}</td>
                            <td>{t.REFERENCENUMBER}</td>
                            <td>{amount}</td>
                            <td>{maturityDate}</td>
                        </tr>
                        ";
                    }
                    result = result + $"</table>";

                    alertTemplate = alertTemplate.Replace("@{{accountOfficerName}}", staffFullName);
                    alertTemplate = alertTemplate.Replace("@{{accountNumbers}}", result);

                    emailList = emailList + GetAllStaffRoleEmails(alertTitleInfo.ALERTTITLEID) + defaultEmail;
                    alert.receiverEmailList.Add(emailList);
                    alert.template = alertTemplate;
                    alert.alertTitle = alertTitle;
                    alert.canFire = true;

                    alerts.Add(alert);
                }
                SendAlertNotification(alerts);
            }
        }
        public void GetOverlineFacilityNotification()
        {
            // GetOverlineFacilityNotification method
            var overlineFacilityNotification = externalAlertRepository.GetOverlineFacilityNotification();
            var alertTitleInfo = context.TBL_ALERT_TITLE.Where(a => a.BINDINGMETHOD == "GetOverlineFacilityNotification").FirstOrDefault();

            var defaultEmail = "";
            if (alertTitleInfo.DEFAULTEMAIL != null)
            {
                defaultEmail = ";" + alertTitleInfo.DEFAULTEMAIL;
            }

            if (overlineFacilityNotification != null && overlineFacilityNotification.Count() > 0)
            {

                List<AlertsViewModel> alerts = new List<AlertsViewModel>();
                foreach (var staff in overlineFacilityNotification)
                {
                    AlertsViewModel alert = new AlertsViewModel();
                    var alertTitle = alertTitleInfo.TITLE;
                    var alertTemplate = alertTitleInfo.TEMPLATE;
                    string emailList = "";
                    var staffFullName = context.TBL_GLOBAL_EXPOSURE.Where(b => b.ACCOUNTOFFICERCODE == staff.misCode).Select(b => b.ACCOUNTOFFICERNAME).FirstOrDefault();
                    
                    emailList = GetBusinessUsersEmails(staff.misCode);

                    var accountNumbers = context.TBL_GLOBAL_EXPOSURE.Where(d => DbFunctions.TruncateTime(d.MATURITYDATE) == DbFunctions.TruncateTime(d.BOOKINGDATE) && d.ADJFACILITYTYPE == "OVERDRAFT" && d.ACCOUNTOFFICERCODE == staff.misCode).ToList();

                    var n = 0;
                    decimal totalAmount = 0;

                    var result = $@"
                      <table cellpadding='0' cellspacing='0' border='1' width='800px'>
                        <tr>
                            <td><b>S/N</b></td>
                            <td><b>Customer Name</b></td>
                            <td><b>Reference Number</b></td>
                            <td><b>Amount</b></td>
                             <td><b>Maturity Date</b></td>
                        </tr>
                     ";
                    foreach (var t in accountNumbers)
                    {
                        n++;
                        totalAmount = totalAmount + Convert.ToDecimal(t.PRINCIPALOUTSTANDINGBALLCY);
                        var amount = string.Format("{0:#,##.00}", Convert.ToDecimal(t.PRINCIPALOUTSTANDINGBALLCY));
                        var maturityDate = t.MATURITYDATE.ToString("dd-MM-yyyy");
                       
                        result = result + $@"
                        <tr>
                            <td>{n}</td>
                            <td>{t.CUSTOMERNAME}</td>
                            <td>{t.REFERENCENUMBER}</td>
                            <td>{amount}</td>
                            <td>{maturityDate}</td>
                        </tr>
                        ";
                    }
                    result = result + $"</table>";

                    alertTemplate = alertTemplate.Replace("@{{accountOfficerName}}", staffFullName);
                    alertTemplate = alertTemplate.Replace("@{{total}}", totalAmount.ToString());
                    alertTemplate = alertTemplate.Replace("@{{accountNumbers}}", result);

                    emailList = emailList + GetAllStaffRoleEmails(alertTitleInfo.ALERTTITLEID) + defaultEmail;
                    alert.receiverEmailList.Add(emailList);
                    alert.template = alertTemplate;
                    alert.alertTitle = alertTitle;
                    alert.canFire = true;

                    alerts.Add(alert);
                }
                SendAlertNotification(alerts);
            }
        }
        public void GetImminentObligationMaturityFacilityNotification()
        {
            // GetImminentObligationMaturityFacilityNotification method
            var imminentObligationMaturityFacilityNotification = externalAlertRepository.GetImminentObligationMaturityFacilityNotification();
            var alertTitleInfo = context.TBL_ALERT_TITLE.Where(a => a.BINDINGMETHOD == "GetImminentObligationMaturityFacilityNotification").FirstOrDefault();

            var defaultEmail = "";
            if (alertTitleInfo.DEFAULTEMAIL != null)
            {
                defaultEmail = ";" + alertTitleInfo.DEFAULTEMAIL;
            }

            if (imminentObligationMaturityFacilityNotification != null && imminentObligationMaturityFacilityNotification.Count() > 0)
            {

                List<AlertsViewModel> alerts = new List<AlertsViewModel>();
                foreach (var staff in imminentObligationMaturityFacilityNotification)
                {
                    AlertsViewModel alert = new AlertsViewModel();
                    var alertTitle = alertTitleInfo.TITLE;
                    var alertTemplate = alertTitleInfo.TEMPLATE;
                    string emailList = "";
                    var staffFullName = context.TBL_GLOBAL_EXPOSURE.Where(b => b.ACCOUNTOFFICERCODE == staff.misCode).Select(b => b.ACCOUNTOFFICERNAME).FirstOrDefault();

                    emailList = GetBusinessUsersEmails(staff.misCode);

                    var accountNumbers = context.TBL_GLOBAL_EXPOSURE.Where(d => d.MATURITYBANDID <= 4 && d.ACCOUNTOFFICERCODE == staff.misCode).ToList();
                    var n = 0;
                    var result = $@"
                      <table cellpadding='0' cellspacing='0' border='1' width='800px'>
                        <tr>
                            <td><b>S/N</b></td>
                            <td><b>Customer Name</b></td>
                            <td><b>Reference Number</b></td>
                            <td><b>Amount</b></td>
                            <td><b>Maturity Band</b></td>
                        </tr>
                     ";
                    foreach (var t in accountNumbers)
                    {
                        n++;
                        var amount = string.Format("{0:#,##.00}", Convert.ToDecimal(t.PRINCIPALOUTSTANDINGBALLCY));
                        var maturityDate = t.MATURITYDATE.ToString("dd-MM-yyyy");

                        result = result + $@"
                        <tr>
                            <td>{n}</td>
                            <td>{t.CUSTOMERNAME}</td>
                            <td>{t.REFERENCENUMBER}</td>
                            <td>{amount}</td>
                            <td>{t.MATURITYBAND}</td>
                        </tr>
                        ";
                    }
                    result = result + $"</table>";

                    alertTemplate = alertTemplate.Replace("@{{acoountOfficerName}}", staffFullName);
                    alertTemplate = alertTemplate.Replace("@{{accountNumbers}}", result);

                    emailList = emailList + GetAllStaffRoleEmails(alertTitleInfo.ALERTTITLEID) + defaultEmail;
                    alert.receiverEmailList.Add(emailList);
                    alert.template = alertTemplate;
                    alert.alertTitle = alertTitle;
                    alert.canFire = true;

                    alerts.Add(alert);
                }
                SendAlertNotification(alerts);
            }
        }
        public void GetNplOnCreditPortfolio()
        {
            // GetNplOnCreditPortfolio method
            var nplOnCreditPortfolio = externalAlertRepository.GetNplOnCreditPortfolio();
            var alertTitleInfo = context.TBL_ALERT_TITLE.Where(a => a.BINDINGMETHOD == "GetNplOnCreditPortfolio").FirstOrDefault();

            var defaultEmail = "";
            if (alertTitleInfo.DEFAULTEMAIL != null)
            {
                defaultEmail = ";" + alertTitleInfo.DEFAULTEMAIL;
            }
            if (nplOnCreditPortfolio != null && nplOnCreditPortfolio.Count() > 0)
            {
                decimal overallTotal = Convert.ToDecimal(context.TBL_GLOBAL_EXPOSURE.Where(d => d.NPL > 0).Sum(d => d.NPL));
                List<AlertsViewModel> alerts = new List<AlertsViewModel>();
                foreach (var staff in nplOnCreditPortfolio)
                {
                    AlertsViewModel alert = new AlertsViewModel();
                    var alertTitle = alertTitleInfo.TITLE;
                    var alertTemplate = alertTitleInfo.TEMPLATE;
                    string emailList = "";
                    var staffFullName = context.TBL_GLOBAL_EXPOSURE.Where(b => b.ACCOUNTOFFICERCODE == staff.misCode).Select(b => b.ACCOUNTOFFICERNAME).FirstOrDefault();

                    emailList = GetBusinessUsersEmails(staff.misCode);

                    decimal sumTotal = 0;
                    string percentage = "";
                    string innerPercent = "";
                    var loanInformation = context.TBL_GLOBAL_EXPOSURE.Where(d => d.NPL > 0 && d.ACCOUNTOFFICERCODE == staff.misCode).ToList();
                    var n = 0;
                    var result = $@"
                     <table cellpadding='0' cellspacing='0' border='1' width='800px'>
                        <tr>
                            <td><b>S/N</b></td>
                            <td><b>Customer Name</b></td>
                            <td><b>Reference Number</b></td>
                            <td><b>Amount</b></td>
                        </tr>
                     ";
                    foreach (var t in loanInformation)
                    {
                        n++;
                        sumTotal = sumTotal + Convert.ToDecimal(t.NPL);
                        innerPercent = (Convert.ToDecimal((t.NPL / sumTotal) * 100)).ToString("0.00%");
                        percentage = ((sumTotal / overallTotal) * 100).ToString("0.00%");

                        result = result + $@"
                        <tr>
                            <td>{n}</td>
                            <td>{t.CUSTOMERNAME}</td>
                            <td>{t.REFERENCENUMBER}</td>
                            <td>{t.NPL}</td>
                        </tr>
                        ";
                    }
                    result = result + $"</table>";
                    
                    alertTemplate = alertTemplate.Replace("@{{accountOfficerName}}", staffFullName);
                    alertTemplate = alertTemplate.Replace("@{{accountNumbers}}", result);
                    alertTemplate = alertTemplate.Replace("@{{percentage}}", percentage);

                    emailList = emailList + GetAllStaffRoleEmails(alertTitleInfo.ALERTTITLEID) + defaultEmail;

                    alert.receiverEmailList.Add(emailList);
                    alert.template = alertTemplate;
                    alert.alertTitle = alertTitle;
                    alert.canFire = true;

                    alerts.Add(alert);
                }

                SendAlertNotification(alerts);
            }
        }
        public void GetImminentMaturitiesAlertEmail()
        {
            // GetImminentMaturitiesAlertEmail method
            var imminentMaturitiesAlertEmail = externalAlertRepository.GetImminentMaturitiesAlertEmail();
            var alertTitleInfo = context.TBL_ALERT_TITLE.Where(a => a.BINDINGMETHOD == "GetImminentMaturitiesAlertEmail").FirstOrDefault();

            var defaultEmail = "";
            if (alertTitleInfo.DEFAULTEMAIL != null)
            {
                defaultEmail = ";" + alertTitleInfo.DEFAULTEMAIL;
            }

            if (imminentMaturitiesAlertEmail != null && imminentMaturitiesAlertEmail.Count() > 0)
            {

                List<AlertsViewModel> alerts = new List<AlertsViewModel>();
                foreach (var i in imminentMaturitiesAlertEmail)
                {
                    AlertsViewModel alert = new AlertsViewModel();
                    var alertTitle = alertTitleInfo.TITLE;
                    var alertTemplate = alertTitleInfo.TEMPLATE;

                    string emailList = "";
                    alertTemplate = alertTemplate.Replace("@{{customerName}}", i.customerName);
                    alertTemplate = alertTemplate.Replace("@{{amountDue}}", Convert.ToDecimal(i.amountDue).ToString("#,##.00"));
                    alertTemplate = alertTemplate.Replace("@{{scheduleDate}}", i.scheduleDueDate.ToString("dd-MM-yyyy"));
                    alertTemplate = alertTemplate.Replace("@{{dueDate}}", i.scheduleDueDate.ToString("dd-MM-yyyy"));

                    emailList = i.customerName + defaultEmail;
                    alert.receiverEmailList.Add(emailList);
                    alert.template = alertTemplate;
                    alert.alertTitle = alertTitle;
                    alert.canFire = true;

                    alerts.Add(alert);
                }
                SendAlertNotification(alerts);
            }
        }

        #region other code logic
        //public void validateAlertCheck()
        //{
        //    List<AlertsViewModel> alerts = new List<AlertsViewModel>();

        //    var alertSuject = context.TBL_ALERT_TITLE;
        //    var alertSetup = context.TBL_ALERT_SETUP;
        //    foreach (var i in alertSetup)
        //    {
        //        AlertsViewModel alert = new AlertsViewModel();

        //        var alertcategory = context.TBL_ALERT_TITLE.Where(x => x.ALERTTITLEID == i.TITLEID).FirstOrDefault();
        //        if (alertcategory != null)
        //        {
        //            alert.alertTitle = alertcategory.TITLE;
        //            // alert.template = externalAlertRepository.GetAccountDeferralReport();

        //            if (validateConditionTrigger(i.FREQUENCYID, i.CONDITIONID ?? 0))
        //            {
        //                alert.canFire = true;
        //                var levels = GetReceivergroup(i.LEVELGROUPID);
        //                foreach (var level in levels)
        //                {
        //                    var levelRecord = context.TBL_ALERT_STAFF_ROLE.Where(x => x.LEVELCODE == level.levelCode).ToList();
        //                    alert.receiverEmailList.AddRange(levelRecord.Select(x => x.EMAILLIST));
        //                }
        //            }
        //            else { alert.canFire = false; }
        //        }

        //        alerts.Add(alert);
        //    }
        //    postAlertNotification(alerts);
        //}

        private void SendAlertNotification(List<AlertsViewModel> alerts)
        {
           
                foreach (var alert in alerts)
                {
                    if (alert.canFire) LogEmailAlert(alert.template, alert.alertTitle, alert.receiverEmailList, "100442", 0);
                }
            
        }

        private bool validateConditionTrigger(short frequencyId, short conditionId)
        {
            if (ValidateFrequency(frequencyId, conditionId))
            {
                return true;
            }
            else { return false; }
        }

        private bool ValidateFrequency(short frequencyId, short conditionId)
        {
            var frequency = context.TBL_ALERT_FREQUENCY.Find(frequencyId);
            var systemDate = general.GetApplicationDate();

            if (frequencyId == (short)AlertFrequencyEnum.DATE)
            {
                var condition = context.TBL_ALERT_CONDITION.Find(conditionId);
                if(condition != null)
                {
                    if(systemDate.Date == condition.NEXTRUNDATE) return true; else  return false;
                }
            }

            if (frequencyId == (short)AlertFrequencyEnum.DAILY)
            {
                var condition = context.TBL_ALERT_CONDITION.Find(conditionId);
                if (condition != null)
                {
                    if (systemDate.Date > condition.LASTRUNDATE && systemDate.Date <= condition.NEXTRUNDATE) return true; else return false;
                }
            }

            if (frequencyId == (short)AlertFrequencyEnum.DAYCOUNT)
            {
                var condition = context.TBL_ALERT_CONDITION.Find(conditionId);
                if (condition != null)
                {
                    var count = condition.ALERTINTERVAL ?? 0; 
                    if (condition.LASTRUNDATE.AddDays(count) == systemDate) return true; else return false;
                }
            }

            if (frequencyId == (short)AlertFrequencyEnum.WEEKLY)
            {
                var condition = context.TBL_ALERT_CONDITION.Find(conditionId);
                if (condition != null)
                {
                    if (condition.LASTRUNDATE.AddDays(7) == systemDate) return true; else return false;
                }
            }

            if (frequencyId == (short)AlertFrequencyEnum.EVENT)
            {
                var condition = context.TBL_ALERT_CONDITION.Find(conditionId);
                if (condition != null)
                {
                    //if (condition.LASTRUNDATE.AddDays(7) == systemDate) return true; else return false;
                }
            }

            return false;
        }

        //private List<AlertLevelViewModel> GetReceivergroup(int levelGroupId)
        //{
        //    var levels = (from g in context.TBL_ALERT_ROLE_GROUP
        //                  join m in context.TBL_ALERT_ROLE_GRP_MAPPING on g.ALERTLEVELGROUPID equals m.LEVELGROUPID
        //                  join l in context.TBL_ALERT_STAFF_ROLE on m.LEVELCODE equals l.STAFFROLEID
        //                  where g.ALERTLEVELGROUPID == levelGroupId
        //                  select new AlertLevelViewModel
        //                  {
        //                      levelCode = l.LEVELCODE,
        //                      levelGroupId = l.LEVELGROUPID
        //                  }).ToList();

        //    return levels;
        //}


        public string GetAllStaffRoleEmails(int alerttitleId)
        {
            var list = "";
            var roleEmail = (from r in context.TBL_STAFF_ROLE
                            join s in context.TBL_STAFF on r.STAFFROLEID equals s.STAFFROLEID
                            join t in context.TBL_ALERT_STAFF_ROLE on r.STAFFROLEID equals t.STAFFROLEID
                            where t.ALERTTITLEID == alerttitleId
                              select new simpleStaffModel
                              {
                                   staffCode= s.STAFFCODE,
                                   staffRoleId = s.STAFFROLEID,
                                   email = s.EMAIL,
                              }).ToList();

            foreach (var t in roleEmail)
            {
                list = list+";"+t.email;
            }
            return list;
        }

        public void LogEmailAlert(string messageBody, string alertSubject, List<string> recipients, string referenceCode, int targetId)
        {
            try
            {
                string recipient = string.Join("", recipients.ToArray());
                string messageSubject = alertSubject +" TESTING ALERT SYSTEM";
                string messageContent = messageBody;
                //string templateUrl = context.TBL_ALERT_GENERAL_TEMPLATE.Find(1).TEMPLATEBODY; //"~/EmailTemp/Monitoring.html";
                //string mailBody = templateUrl.Replace("{Description}", messageContent);  //EmailHelpers.PopulateBody(messageContent, templateUrl); 
                MessageLogViewModel messageModel = new MessageLogViewModel
                {
                    MessageSubject = messageSubject,
                    MessageBody = messageContent,
                    MessageStatusId = 1,
                    MessageTypeId = 1,
                    FromAddress = ConfigurationManager.AppSettings["SupportEmailAddr"],
                    ToAddress = $"{recipient}",
                    DateTimeReceived = DateTime.Now,
                    SendOnDateTime = DateTime.Now,
                    ReferenceCode = referenceCode,
                    targetId = targetId,
                };
                SaveMessageDetails(messageModel);
            }
            catch (Exception ex)
            {
                new SecureException(ex.ToString());
            }
        }

        private void SaveMessageDetails(MessageLogViewModel model)
        {
            var message = new TBL_MESSAGE_LOG()
            {
                //MessageId = model.MessageId,
                MESSAGESUBJECT = model.MessageSubject,
                MESSAGEBODY = model.MessageBody,
                MESSAGESTATUSID = model.MessageStatusId,
                MESSAGETYPEID = model.MessageTypeId,
                FROMADDRESS = model.FromAddress,
                TOADDRESS = model.ToAddress,
                DATETIMERECEIVED = model.DateTimeReceived,
                SENDONDATETIME = model.SendOnDateTime,
                ATTACHMENTCODE = model.ReferenceCode,
                ATTACHMENTTYPEID = (short)AttachementTypeEnum.JobRequest,
                TARGETID = (int)model.targetId
            };

            context.TBL_MESSAGE_LOG.Add(message);
            context.SaveChanges();

        }

# endregion logic codes

    }
}
