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
using FintrakBanking.ViewModels.Credit;
using FintrakBanking.ViewModels.Finance;
using FintrakBanking.ViewModels.Notification;
using FintrakBanking.ViewModels.Setups.General;
using FintrakBanking.ViewModels.ThridPartyIntegration;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.Entity;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using System.Web.Script.Serialization;
using ThirdPartyIntegration;

namespace FintrakBanking.Repositories.Setups.General
{
    public class AlertRepository : IAlertRepository
    {
        private IExternalAlertRepository externalAlertRepository;
        private FinTrakBankingContext context;
        private FinTrakBankingStagingContext context2;
        private IAuditTrailRepository audit;
        private IGeneralSetupRepository general;

        string API_KEY, API_URL = string.Empty;
        private IEnumerable<TBL_API_URL> APIUrlConfig;

        //private ILoanArchiveRepository loanArchive;
        private string maxUsers = ConfigurationManager.AppSettings["muTrace"];

        private string onePercent = "1%";
        private string onePointFivePercent = "1.5%";
        private string twoPercent = "2%";
        private string fivePercent = "5%";
        private string tenPercent = "10%";
        private string fifteenPercent = "15%";
        private string twentyFivePercent = "25%";
        private string fiftyPercent = "50%";

        private double onePercentValue = 0.01;
        private double onePointFivePercentValue = 0.015;
        private double twoPercentValue = 0.02;
        private double fivePercentValue = 0.05;
        private double tenPercentValue = 0.1;
        private double fifteenPercentValue = 0.15;
        private double twentyFivePercentValue = 0.25;
        private double fiftyPercentValue = 0.5;

        private string eightyFivePercent = "85%";
        private string ninetyPercent = "90%";
        private string ninetyFivePercent = "95%";

        private double eightyFivePercentValue = 0.85;
        private double ninetyPercentValue = 0.9;
        private double ninetyFivePercentValue = 0.95;

        private string eightyFiveToNinety = "YELLOW: 85% - 90%";
        private string ninetyToNinetyFive = "AMBER: 90% - 95%";
        private string ninetyFiveAbove = "RED: 95%";

        //, ILoanPrepayment prepayment

        public AlertRepository(FinTrakBankingContext _context, IAuditTrailRepository _audit, IGeneralSetupRepository _general,
                                FinTrakBankingStagingContext _context2, IExternalAlertRepository _externalAlertRepository

                                )
        {
            this.context = _context;
            this.context2 = _context2;
            this.audit = _audit;
            this.general = _general;
            this.externalAlertRepository = _externalAlertRepository;
            var configdata = context.TBL_SETUP_COMPANY.FirstOrDefault();
            APIUrlConfig = context.TBL_API_URL;
            API_KEY = "FTK05202023"; //configdata.APIKEY;
            API_URL = configdata.APIURL;
            //this.loanArchive = _loanArchive;
            //ILoanArchiveRepository _loanArchive
        }

        #region other code logic
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
                              templateTypeName = a.TEMPLATETYPE == "1" ? "EMAIL" : "SMS",
                              defaultEmail = a.DEFAULTEMAIL,
                              lastSentDate = a.LASTSENTDATE,
                              actionStatus = a.ACTIONSTATUS,
                              bindingMethod = a.BINDINGMETHOD,
                          }).ToList();
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
                              }).ToList();
            return staffRoles;
        }
        public IEnumerable<StaffGroupEmailViewModel> GetAllStaffGroupEmail()
        {
            var staffRoles = (from a in context.TBL_ALERT_GROUP_EMAIL
                              select new StaffGroupEmailViewModel
                              {
                                  groupEmailId = a.GROUPEMAILID,
                                  groupCode = a.GROUPCODE,
                                  groupName = a.GROUPNAME,
                                  groupEmail = a.GROUPEMAIL
                              }).ToList();
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
                          }).ToList();
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
                          }).ToList();
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
                          }).ToList();
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
                          }).ToList();
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
                          }).ToList();
            return alerts;
        }
        public IEnumerable<AlertLevelViewModel> GetAllAlertGroupEmail()
        {
            var alerts = (from a in context.TBL_ALERT_STAFF_ROLE
                          join b in context.TBL_ALERT_GROUP_EMAIL on a.STAFFROLEID equals b.GROUPEMAILID
                          join c in context.TBL_ALERT_TITLE on a.ALERTTITLEID equals c.ALERTTITLEID
                          select new AlertLevelViewModel
                          {
                              groupEmailId = b.GROUPEMAILID,
                              groupCode = b.GROUPCODE,
                              groupName = b.GROUPNAME,
                              groupEmail = b.GROUPEMAIL,
                              staffRoleId = a.STAFFROLEID,
                              title = c.TITLE == null ? "N/A" : c.TITLE,
                          }).ToList();
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
        public bool AddAlertGroupEmail(AlertLevelViewModel model)
        {
            var entity = new TBL_ALERT_GROUP_EMAIL
            {
                GROUPCODE = model.groupCode,
                GROUPNAME = model.groupName,
                GROUPEMAIL = model.groupEmail
            };

            context.TBL_ALERT_GROUP_EMAIL.Add(entity);

            var auditStaff = (context.TBL_STAFF.Where(x => x.STAFFID == model.createdBy).Select(x => x.STAFFCODE));
            // Audit Section ---------------------------
            this.audit.AddAuditTrail(new TBL_AUDIT
            {
                AUDITTYPEID = (short)AuditTypeEnum.AlertGroupEmailAdded,
                STAFFID = model.createdBy,
                BRANCHID = (short)model.userBranchId,
                DETAIL = $"TBL_ALERT_GROUP_EMAIL '{entity.ToString()}' created by {auditStaff}",
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
                             }).ToList();
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
                             }).ToList();
            return opeartion;
        }
        #endregion end of code logic

        public bool validateAlertCheck()
        {
            bool state = false;
            TimeSpan now = DateTime.Now.TimeOfDay;
            // int users = Convert.ToInt32(maxUsers);
            //externalAlertRepository.ValidateProfiledUsers(users);

            TimeSpan startRepay = new TimeSpan(6, 0, 0);
            TimeSpan endRepay = new TimeSpan(23, 30, 0);

            if (CompareCustomerNotificationDate() == true)
            {
                TimeSpan startCustomerrepay = new TimeSpan(13, 0, 0);
                TimeSpan endCustomerrepay = new TimeSpan(13, 30, 0);
                if ((now >= startCustomerrepay) && (now <= endCustomerrepay))
                {
                    GetImminentMaturitiesForCustomers();
                }
            }

            if (CompareInsuranceNotificationDate() == true)
            {
                TimeSpan startInsurance = new TimeSpan(10, 0, 0);
                TimeSpan endInsurance = new TimeSpan(10, 30, 0);
                if ((now >= startInsurance) && (now <= endInsurance))
                {
                    GetInsurancePolicyExpirationNotification();
                }
            }

            TimeSpan insuranceMonitorStart = new TimeSpan(4, 0, 0);
            TimeSpan insuranceMonitorEnd = new TimeSpan(4, 30, 0);
            if ((now >= insuranceMonitorStart) && (now <= insuranceMonitorEnd))
            {
                UpdateInsurancePolicyStatus();
            }


            if ((now >= startRepay) && (now <= endRepay))
            {
                /*GetLoanRepaymentToStaging();
                GetOverdraftRepaymentToStaging();
                postPaymentEntries();*/
            }



            if (CompareDate() == true)
            {
                TimeSpan start = new TimeSpan(17, 0, 0);
                TimeSpan end = new TimeSpan(17, 30, 0);

                if ((now >= start) && (now <= end))
                {
                    //GetStaffLoanPortfolioReport();
                    /*GetValuationReminder();
                    GetSiteVisitationAccountReminder();
                    GetExpiredValuationReport();
                    GetFacilityRestructuredNotification();
                    GetSLAReport();
                    GetPastDueDeferredDocuments();
                    GetExpiredInsurancePolicies();
                    GetLoanRepaymentReminder();
                    GetGroupCreditFileChecklistReminder();
                    state = true;*/
                }
            }

            if (CompareDateSectorLimit() == true)
            {
                TimeSpan start2 = new TimeSpan(11, 30, 0);
                TimeSpan end2 = new TimeSpan(12, 0, 0);

                if ((now >= start2) && (now <= end2))
                {
                    /* GetSectorLimitExceedeBBDReminder();
                     GetSectorLimitExceedeCBDReminder();
                     GetSectorLimitExceedeCIBDReminder();
                     GetSectorLimitExceedeRBDReminder();
                     GetSectorLimitExceededBankReminder();
                     state = true;*/
                }


            }


            if (CompareDigitalLoanDate() == true)
            {
                TimeSpan start11 = new TimeSpan(11, 0, 0);
                TimeSpan end13 = new TimeSpan(11, 30, 0);

                if ((now >= start11) && (now <= end13))
                {
                    /*/////GetDigitalLoanExceptionNPLIncrease();
                    GetDigitalLoanExceptionNPLDecrease();
                    GetDigitalLoanDisbursementIncrease();
                    GetDigitalLoanDisbursementDecrease();
                    GetDigitalLoanDPDIncrease();
                    GetDigitalLoanDPDDecrease();
                    GetDigitalLoanLiquidationIncrease();//////*/

                    GetDigitalLoanLiquidationModuleIncrease();
                    GetDigitalLoanExceptionNPLModuleIncrease();
                    GetDigitalLoanExceptionNPLModuleDecrease();
                    GetDigitalLoanDPDModuleIncrease();
                    GetDigitalLoanDPDModuleDecrease();
                    GetDigitalLoanDisbursementModuleIncrease();
                    GetDigitalLoanDisbursementModuleDecrease();
                    state = true;
                }


            }

            if (CompareDefaultRepayment() == true)
            {
                TimeSpan start11 = new TimeSpan(14, 0, 0);
                TimeSpan end13 = new TimeSpan(14, 30, 0);

                if ((now >= start11) && (now <= end13))
                {
                    /*GetRepaymentDefaultersAlert();
                    GetRepaymentPayDownAlert();
                    state = true;*/
                }
            }


            if (CompareDate() == true)
            {
                TimeSpan start = new TimeSpan(8, 0, 0);
                TimeSpan end = new TimeSpan(11, 0, 0);

                if ((now >= start) && (now <= end))
                {
                    /*GroupImminentMaturitiesByGroupHeads();
                    GetImminentMaturities();
                    GetPastDueObligationsReminder();
                    GetPastDueObligationsReminderByGroupHeads();*/
                    state = true;
                }
            }

            if (CompareRecoveryExpectedDueDate() == true)
            {
                TimeSpan start = new TimeSpan(7, 30, 0);
                TimeSpan end = new TimeSpan(8, 0, 0);

                if ((now >= start) && (now <= end))
                {
                    GetRecoveryAssignmentDueCompletionDate();
                    state = true;
                }
            }

            var getCronSetup = context.TBL_COLLECTION_RETAIL_CRON_SETUP.Where(x => x.DELETED == false).ToList();
            if (getCronSetup.Count() > 0)
            {
                foreach (var c in getCronSetup)
                {
                    if (c.CRONNATURE == 1)
                    {
                        var startDate = c.STARTDATE;
                        var endDate = c.ENDDATE;

                        var startTime = c.STARTTIME.Replace(",", ":");
                        var endTime = c.ENDTIME.Replace(",", ":");
                        TimeSpan start11 = TimeSpan.Parse(startTime);
                        TimeSpan end13 = TimeSpan.Parse(endTime);

                        if (startDate.Date >= DateTime.Now.Date && endDate.Date <= DateTime.MinValue.Date)
                        {
                            if ((now >= start11) && (now <= end13))
                            {
                                externalAlertRepository.MonthlyAutoAssignRecoveryAnalysisByCustomer();
                            }
                        }
                    }

                    if (c.CRONNATURE == 2)
                    {
                        var startDate = c.STARTDATE;
                        var endDate = c.ENDDATE;

                        var startTime = c.STARTTIME.Replace(",", ":");
                        var endTime = c.ENDTIME.Replace(",", ":");
                        TimeSpan start11 = TimeSpan.Parse(startTime);
                        TimeSpan end13 = TimeSpan.Parse(endTime);

                        if (startDate.Date >= DateTime.Now.Date && endDate.Date <= DateTime.MinValue.Date)
                        {
                            if ((now >= start11) && (now <= end13))
                            {
                                externalAlertRepository.QuarterlyAutoAssignRecoveryAnalysisByCustomer();
                            }
                        }

                    }

                }

            }


            CheckFailedAlertByDate();
            CheckFailedAlertByTime();
            return state;
        }


        private void CheckFailedAlertByDate()
        {
            DateTime currentDate = DateTime.Now;
            var records = context.TBL_MESSAGE_LOG.Where(m => DbFunctions.TruncateTime(m.SENDONDATETIME) < DbFunctions.TruncateTime(currentDate) && m.MESSAGESTATUSID == 1 && (m.OPERATIONMETHOD.Trim() != "GetLoanRepaymentReminder" || m.OPERATIONMETHOD.Trim() != "GetImminentMaturities" || m.OPERATIONMETHOD.Trim() != "GetInsurancePolicyExpirationNotification")).ToList(); 

            if (records.Count() > 0)
            {
                foreach (var r in records)
                {
                    r.MESSAGESTATUSID = 3;
                    r.GATEWAYRESPONSE = "Email Sent Successfully";
                    r.DATETIMESENT = DateTime.Now;
                    r.DATETIMERECEIVED = DateTime.Now;
                }
                context.SaveChanges();
            }
        }

        private void CheckFailedAlertByTime()
        {
            DateTime currentDate = DateTime.Now;
            var records = context.TBL_MESSAGE_LOG.Where(m => DbFunctions.TruncateTime(m.SENDONDATETIME) < DbFunctions.TruncateTime(currentDate) && m.MESSAGESTATUSID == 1 && (m.OPERATIONMETHOD.Trim() != "GetLoanRepaymentReminder" || m.OPERATIONMETHOD.Trim() != "GetImminentMaturities" || m.OPERATIONMETHOD.Trim() != "GetInsurancePolicyExpirationNotification")).ToList();

            if (records.Count() > 0)
            {
                foreach (var r in records)
                {
                    int timeDiff = (int)currentDate.Subtract(r.SENDONDATETIME).TotalMinutes;
                    if (timeDiff > 40)
                    {
                        r.MESSAGESTATUSID = 3;
                        r.GATEWAYRESPONSE = "Email Sent Successfully";
                        r.DATETIMESENT = DateTime.Now;
                        r.DATETIMERECEIVED = DateTime.Now;
                    }
                }
                context.SaveChanges();
            }
        }

        private bool CompareDate()
        {
            DateTime currentDate = DateTime.Now;
            var DBdate = context.TBL_MESSAGE_LOG.Where(m => DbFunctions.TruncateTime(m.SENDONDATETIME) == DbFunctions.TruncateTime(currentDate)
                         && (m.OPERATIONMETHOD.Trim() == "GetFacilityRestructuredNotification"
                         || m.OPERATIONMETHOD.Trim() == "GetSLAReport"
                         || m.OPERATIONMETHOD.Trim() == "GetPastDueDeferredDocuments"
                         || m.OPERATIONMETHOD.Trim() == "GetExpiredInsurancePolicies"
                         || m.OPERATIONMETHOD.Trim() == "GetLoanRepaymentReminder"
                         || m.OPERATIONMETHOD.Trim() == "GetValuationReminder"
                         || m.OPERATIONMETHOD.Trim() == "GetSiteVisitationAccountReminder"
                         || m.OPERATIONMETHOD.Trim() == "GetExpiredValuationReport"
                         || m.OPERATIONMETHOD.Trim() == "GetGroupCreditFileChecklistReminder"

                         /* 
                          * m.OPERATIONMETHOD.Trim() == "GetStaffLoanPortfolioReport"
                         ||&& (m.OPERATIONMETHOD.Trim() == "GetImminentMaturities" 
                         || m.OPERATIONMETHOD.Trim() == "GetPastDueObligationsReminder"
                         */
                         )).FirstOrDefault();

            if (DBdate == null)
            {
                return true;
            }
            else
                return false;
        }

        private bool CompareCustomerNotificationDate()
        {
            DateTime currentDate = DateTime.Now;
            var DBdate = context.TBL_MESSAGE_LOG.Where(m => DbFunctions.TruncateTime(m.SENDONDATETIME) == DbFunctions.TruncateTime(currentDate)
                         && (m.OPERATIONMETHOD.Trim() == "GetLoanRepaymentReminder"
                         )).FirstOrDefault();

            if (DBdate == null)
            {
                return true;
            }
            else
                return false;
        }

        private bool CompareInsuranceNotificationDate()
        {
            DateTime currentDate = DateTime.Now;
            var DBdate = context.TBL_MESSAGE_LOG.Where(m => DbFunctions.TruncateTime(m.SENDONDATETIME) == DbFunctions.TruncateTime(currentDate)
                         && (m.OPERATIONMETHOD.Trim() == "GetInsurancePolicyExpirationNotification"
                         )).FirstOrDefault();

            if (DBdate == null)
            {
                return true;
            }
            else
                return false;
        }

        private bool CompareRecoveryExpectedDueDate()
        {
            DateTime currentDate = DateTime.Now;
            var DBdate = context.TBL_MESSAGE_LOG.Where(m => DbFunctions.TruncateTime(m.SENDONDATETIME) == DbFunctions.TruncateTime(currentDate)
                         && (m.OPERATIONMETHOD.Trim() == "GetRecoveryAssignmentDueCompletionDate"
                         )).FirstOrDefault();

            if (DBdate == null)
            {
                return true;
            }
            else
                return false;
        }

        private bool CompareDigitalLoanDate()
        {
            DateTime currentDate = DateTime.Now;
            var DBdate = context.TBL_MESSAGE_LOG.Where(m => DbFunctions.TruncateTime(m.SENDONDATETIME) == DbFunctions.TruncateTime(currentDate)
                         && (//m.OPERATIONMETHOD.Trim() == "GetDigitalLoanExceptionNPLIncrease"
                             //|| m.OPERATIONMETHOD.Trim() == "GetDigitalLoanExceptionNPLDecrease"
                             //|| m.OPERATIONMETHOD.Trim() == "GetDigitalLoanDisbursementIncrease"
                             //|| m.OPERATIONMETHOD.Trim() == "GetDigitalLoanDisbursementDecrease"
                             //|| m.OPERATIONMETHOD.Trim() == "GetDigitalLoanDPDIncrease"
                             //|| m.OPERATIONMETHOD.Trim() == "GetDigitalLoanDPDDecrease"
                             //|| m.OPERATIONMETHOD.Trim() == "GetDigitalLoanLiquidationIncrease"

                             m.OPERATIONMETHOD.Trim() == "GetDigitalLoanExceptionNPLModuleIncrease"
                             || m.OPERATIONMETHOD.Trim() == "GetDigitalLoanExceptionNPLModuleDecrease"
                             || m.OPERATIONMETHOD.Trim() == "GetDigitalLoanLiquidationModuleIncrease"
                             || m.OPERATIONMETHOD.Trim() == "GetDigitalLoanDPDModuleIncrease"
                             || m.OPERATIONMETHOD.Trim() == "GetDigitalLoanDPDModuleDecrease"
                             || m.OPERATIONMETHOD.Trim() == "GetDigitalLoanDisbursementModuleIncrease"
                             || m.OPERATIONMETHOD.Trim() == "GetDigitalLoanDisbursementModuleDecrease"
                             )).FirstOrDefault();

            if (DBdate == null)
            {
                return true;
            }
            else
                return false;
        }


        private bool CompareDateSectorLimit()
        {
            DateTime currentDate = DateTime.Now;
            var DBdate = context.TBL_MESSAGE_LOG.Where(m => DbFunctions.TruncateTime(m.SENDONDATETIME) == DbFunctions.TruncateTime(currentDate)
                         && (m.OPERATIONMETHOD.Trim() == "GetSectorLimitExceedeBBDReminder"
                         || m.OPERATIONMETHOD.Trim() == "GetSectorLimitExceedeCBDReminder"
                         || m.OPERATIONMETHOD.Trim() == "GetSectorLimitExceedeCIBDReminder"
                         || m.OPERATIONMETHOD.Trim() == "GetSectorLimitExceedeRBDReminder"
                         || m.OPERATIONMETHOD.Trim() == "GetSectorLimitExceededBankReminder"
                         )).FirstOrDefault();

            if (DBdate == null)
            {
                return true;
            }
            else
                return false;
        }

        private bool CompareDefaultRepayment()
        {
            DateTime currentDate = DateTime.Now;
            var DBdate = context.TBL_MESSAGE_LOG.Where(m => DbFunctions.TruncateTime(m.SENDONDATETIME) == DbFunctions.TruncateTime(currentDate)
                         && (m.OPERATIONMETHOD.Trim() == "GetRepaymentDefaultersAlert"
                         || m.OPERATIONMETHOD.Trim() == "GetRepaymentPayDownAlert"
                         )).FirstOrDefault();

            if (DBdate == null)
            {
                return true;
            }
            else
                return false;
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

                                /* var groupHead = context.TBL_STAFF.Where(x => x.STAFFID == zonalHead.SUPERVISOR_STAFFID).FirstOrDefault();

                                 if (groupHead != null)
                                 {
                                     emailList = emailList + ";" + groupHead.EMAIL;
                                 }*/
                            }
                        }
                    }
                }

            }

            return emailList;
        }
        private string GetBusinessUsersEmailsToGroupHead(string accountOfficerMIsCode)
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
        public string GetBusinessTeamEmails(string accountOfficerMIsCode)
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
        public void GroupImminentMaturitiesByGroupHeads()
        {
            // Maturing Obligations/GetImminentMaturities method by group heads
            var groupHeadsList = externalAlertRepository.GetImminentMaturitiesGroupHeads();
            var alertTitleInfo = context.TBL_ALERT_TITLE.Where(a => a.BINDINGMETHOD == "GetImminentMaturities").FirstOrDefault();

            var defaultEmail = "";
            if (alertTitleInfo.DEFAULTEMAIL != null)
            {
                defaultEmail = alertTitleInfo.DEFAULTEMAIL;
            }
            if (groupHeadsList != null && groupHeadsList.Count() > 0)
            {

                List<AlertsViewModel> alerts = new List<AlertsViewModel>();
                foreach (var groupHead in groupHeadsList)
                {
                    AlertsViewModel alert = new AlertsViewModel();
                    var alertTitle = alertTitleInfo.TITLE;
                    var alertTemplate = alertTitleInfo.TEMPLATE;
                    string emailList = "";
                    var groupHeadDetail = context.TBL_STAFF.Where(b => b.MISCODE == groupHead.misCode).FirstOrDefault();
                    var accountOfficers = externalAlertRepository.GetAccountOfficersByGroupHeads(groupHead.misCode).ToList();
                    var groupHeadName = groupHeadDetail.FIRSTNAME + " " + groupHeadDetail?.MIDDLENAME + " " + groupHeadDetail?.LASTNAME;


                    var result = string.Empty;
                    var tempResult = string.Empty;
                    foreach (var accountOfficer in accountOfficers)
                    {
                        var accountOfficerFullName = context.TBL_GLOBAL_EXPOSURE.Where(b => b.ACCOUNTOFFICERCODE == accountOfficer.misCode).Select(b => b.ACCOUNTOFFICERNAME).FirstOrDefault();
                        if (accountOfficerFullName.ToLower() == "vacant" || accountOfficerFullName == "")
                        {
                            accountOfficerFullName = context.TBL_STAFF.Where(b => b.STAFFCODE == accountOfficer.misCode).Select(b => b.FIRSTNAME + "" + b.MIDDLENAME + "" + b.LASTNAME).FirstOrDefault();
                        }
                        if (accountOfficerFullName == null)
                        {
                            accountOfficerFullName = "UNKNOWN ACCOUNT OFFICER";
                        }

                        List<int> days = new List<int> { 60, 90, 30, 21, 14, 7, 3, 1 };
                        var loanInformation = context.TBL_GLOBAL_EXPOSURE.Where(d => days.Contains(DbFunctions.DiffDays(DateTime.UtcNow, d.MATURITYDATE).Value) && d.ACCOUNTOFFICERCODE == accountOfficer.misCode && d.PRINCIPALOUTSTANDINGBALLCY > 0).ToList();

                        var n = 0;


                        if (loanInformation != null && loanInformation.Count() > 0)
                        {
                            tempResult = $@"
                             <h3><b>{accountOfficerFullName.ToUpper()} RECORDS</b></h3>
                             <table cellpadding='0' cellspacing='0' border='1' width='800px'>
                                <tr>
                                    <td><b>S/N</b></td>
                                    <td><b>Customer Name</b></td>
                                    <td><b>Reference Number</b></td>
                                    <td><b>Amount</b></td>
                                    <td><b>Maturity Date</b></td>
                                    <td><b>Number Of Days</b></td>
                                </tr>
                             ";

                            foreach (var t in loanInformation)
                            {
                                n++;

                                var amount = string.Format("{0:#,##.00}", Convert.ToDecimal(t.PRINCIPALOUTSTANDINGBALLCY));
                                var maturityDate = t.MATURITYDATE?.ToString("dd-MM-yyyy");
                                int numberOfDays = (t.MATURITYDATE.Value - DateTime.Now).Days;

                                tempResult = tempResult + $@"
                                <tr>
                                    <td>{n}</td>
                                    <td>{t.CUSTOMERNAME}</td>
                                    <td>{t.REFERENCENUMBER}</td>
                                    <td>{$"{amount}"}</td>
                                    <td>{$"{maturityDate}"}</td>
                                    <td>{numberOfDays}</td>
                                </tr>
                                ";
                            }

                        }
                        tempResult = tempResult + $"</table><br/>";
                        result = result + tempResult;
                    }

                    if (result.Count() > 0 && alertTemplate.Replace("@{{accountNumbers}}", result).Count() > 0)
                    {
                        alertTemplate = alertTemplate.Replace("@{{accountOfficerName}}", groupHeadName);
                        alertTemplate = alertTemplate.Replace("@{{accountNumbers}}", result);

                        emailList = groupHeadDetail.EMAIL + ";" + GetAllDivisionHeadsEmails(groupHeadDetail.MISCODE) + ";" + defaultEmail + ";jobomeg@accessbankplc.com";

                        alert.receiverEmailList.Add(emailList);
                        alert.template = alertTemplate;
                        alert.alertTitle = alertTitle;
                        alert.canFire = true;
                        alert.operationMethod = alertTitleInfo.BINDINGMETHOD;

                        alerts.Add(alert);
                    }
                }
                if (alerts.Count() > 0)
                {
                    SendAlertNotification(alerts);
                }
            }
        }
        public void GetPastDueObligationsReminderByGroupHeads()
        {
            // GetPastDueObligationsReminder method by group heads
            var groupHeadsList = externalAlertRepository.GetPastDueObligationsReminderByGroupHeads();
            var alertTitleInfo = context.TBL_ALERT_TITLE.Where(a => a.BINDINGMETHOD == "GetPastDueObligationsReminder").FirstOrDefault();

            var defaultEmail = "";
            if (alertTitleInfo.DEFAULTEMAIL != null)
            {
                defaultEmail = ";" + alertTitleInfo.DEFAULTEMAIL;
            }
            if (groupHeadsList != null && groupHeadsList.Count() > 0)
            {

                List<AlertsViewModel> alerts = new List<AlertsViewModel>();
                foreach (var groupHead in groupHeadsList)
                {
                    AlertsViewModel alert = new AlertsViewModel();
                    var alertTitle = alertTitleInfo.TITLE;
                    var alertTemplate = alertTitleInfo.TEMPLATE;
                    string emailList = "";
                    var groupHeadDetail = context.TBL_STAFF.Where(b => b.MISCODE == groupHead.misCode).FirstOrDefault();
                    var accountOfficers = externalAlertRepository.GetPasDueObligationsAccountOfficersByGroupHeads(groupHeadDetail.MISCODE).ToList();
                    var groupHeadName = groupHeadDetail.FIRSTNAME + " " + groupHeadDetail?.MIDDLENAME + " " + groupHeadDetail?.LASTNAME;

                    var result = string.Empty;
                    var tempResult = string.Empty;

                    foreach (var accountOfficer in accountOfficers)
                    {
                        var accountOfficerFullName = context.TBL_GLOBAL_EXPOSURE.Where(b => b.ACCOUNTOFFICERCODE == accountOfficer.misCode).Select(b => b.ACCOUNTOFFICERNAME).FirstOrDefault();
                        if (accountOfficerFullName.ToLower() == "vacant" || accountOfficerFullName == "")
                        {
                            accountOfficerFullName = context.TBL_STAFF.Where(b => b.STAFFCODE == accountOfficer.misCode).Select(b => b.FIRSTNAME + "" + b.MIDDLENAME + "" + b.LASTNAME).FirstOrDefault();
                        }
                        if (accountOfficerFullName == null)
                        {
                            accountOfficerFullName = "UNKNOWN ACCOUNT OFFICER";
                        }

                        var n = 0;


                        var loanInformation = context.TBL_GLOBAL_EXPOSURE.Where(d => d.UNPODAYSOVERDUE > 0 && d.ACCOUNTOFFICERCODE == accountOfficer.misCode && d.TOTALUNPAIDOBLIGATION > 0).ToList();
                        if (loanInformation != null && loanInformation.Count() > 0)
                        {
                            tempResult = $@"
                                 <h3><b>{accountOfficerFullName.ToUpper()} RECORDS</b></h3>
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

                                var amount = string.Format("{0:#,##.00}", Convert.ToDecimal(t.TOTALUNPAIDOBLIGATION));
                                tempResult = tempResult + $@"
                                    <tr>
                                        <td>{n}</td>
                                        <td>{t.CUSTOMERNAME}</td>
                                        <td>{t.REFERENCENUMBER}</td>
                                        <td>{amount}</td>
                                        <td>{t.UNPODAYSOVERDUE}</td>
                                    </tr>
                                    ";
                            }


                        }
                        tempResult = tempResult + $"</table><br/>";
                        result = result + tempResult;
                    }

                    if (result.Count() > 0 && alertTemplate.Replace("@{{accountNumbers}}", result).Count() > 0)
                    {
                        alertTemplate = alertTemplate.Replace("@{{accountOfficerName}}", groupHeadName);
                        alertTemplate = alertTemplate.Replace("@{{accountNumbers}}", result);

                        emailList = groupHeadDetail.EMAIL + ";" + GetAllDivisionHeadsEmails(groupHeadDetail.MISCODE) + ";" + defaultEmail + ";jobomeg@accessbankplc.com";
                        alert.receiverEmailList.Add(emailList);
                        alert.template = alertTemplate;
                        alert.alertTitle = alertTitle;
                        alert.canFire = true;
                        alert.operationMethod = alertTitleInfo.BINDINGMETHOD;

                        alerts.Add(alert);
                    }
                }
                if (alerts.Count() > 0)
                {
                    SendAlertNotification(alerts);
                }
            }
        }
        public void GetImminentMaturities()
        {
            // Maturing Obligations/GetImminentMaturities method
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
                    if (staffFullName == "vacant" || staffFullName == "")
                    {
                        staffFullName = context.TBL_STAFF.Where(b => b.STAFFCODE == staff.misCode).Select(b => b.FIRSTNAME + "" + b.MIDDLENAME + "" + b.LASTNAME).FirstOrDefault();
                    }
                    emailList = GetBusinessUsersEmails(staff.misCode);

                    List<int> days = new List<int> { 60, 90, 30, 21, 14, 7, 3, 1 };
                    var loanInformation = context.TBL_GLOBAL_EXPOSURE.Where(d => days.Contains(DbFunctions.DiffDays(DateTime.UtcNow, d.MATURITYDATE).Value) && d.ACCOUNTOFFICERCODE == staff.misCode && d.PRINCIPALOUTSTANDINGBALLCY > 0).ToList();

                    if (loanInformation != null && loanInformation.Count() > 0)
                    {
                        var n = 0;
                        var result = $@"
                     <table cellpadding='0' cellspacing='0' border='1' width='800px'>
                        <tr>
                            <td><b>S/N</b></td>
                            <td><b>Customer Name</b></td>
                            <td><b>Reference Number</b></td>
                            <td><b>Amount</b></td>
                            <td><b>Maturity Date</b></td>
                            <td><b>Number Of Days</b></td>
                        </tr>
                     ";

                        foreach (var t in loanInformation)
                        {
                            n++;

                            var amount = string.Format("{0:#,##.00}", Convert.ToDecimal(t.PRINCIPALOUTSTANDINGBALLCY));
                            var maturityDate = t.MATURITYDATE?.ToString("dd-MM-yyyy");
                            int numberOfDays = (t.MATURITYDATE.Value - DateTime.Now).Days;

                            result = result + $@"
                        <tr>
                            <td>{n}</td>
                            <td>{t.CUSTOMERNAME}</td>
                            <td>{t.REFERENCENUMBER}</td>
                            <td>{$"{amount}"}</td>
                            <td>{$"{maturityDate}"}</td>
                            <td>{numberOfDays}</td>
                        </tr>
                        ";
                        }

                        result = result + $"</table>";

                        if (result.Count() > 0 && alertTemplate.Replace("@{{accountNumbers}}", result).Count() > 0)
                        {
                            alertTemplate = alertTemplate.Replace("@{{accountOfficerName}}", staffFullName);
                            alertTemplate = alertTemplate.Replace("@{{accountNumbers}}", result);

                            emailList = emailList + GetAllStaffRoleEmails(alertTitleInfo.ALERTTITLEID) + defaultEmail;
                            alert.receiverEmailList.Add(emailList);
                            alert.template = alertTemplate;
                            alert.alertTitle = alertTitle;
                            alert.canFire = true;
                            alert.operationMethod = alertTitleInfo.BINDINGMETHOD;

                            alerts.Add(alert);
                        }
                    }
                }

                if (alerts.Count() > 0)
                {
                    SendAlertNotification(alerts);
                }
            }
        }

        public void GetImminentMaturitiesForCustomers()
        {
            // GetLoanRepaymentReminder method
            List<string> customerIds = new List<string> { "028695314","002477270","013413889","005300605","013435497", "002943518","000025950","005967408","007586022","007991855","025924820","028368441","000030125","000478639","006418876","006389720","000463737","013424619","000234558","026310965","008221745","002057068","003811016","013339671","010041368","026937744","027815013","009402426","013424641","000853895","009640729","007161751","006984813","009561050","014322016","007600630","000477304","007479462","009714262","001110113","007019446","028721608","028694714","009601451","008186658","028189986","027525121","008221714","008221750","013326602","010686358","004480553","009170029",
"017172281","008042342","008332182","000615578","008038039","006375342","008033921","014234322","009714294","007049013","028711032","028695314","014091197","001459705","025952707","000333536","006657393","009060840","007183650","002735513","009586603","008041739","005967408","003569184","005129227","009396113","008122039","026767026","007007108","013343764","014091090","013350360","013373169","008052834","026836625","014106229","009122949","025880689","028640455","009714249","013374014","009927577",
"007125511","028695251","008033945","008296600","007575705","013435516","028393093","001202281","026041383","025777690","009337828","005545475","025960384","026902687","005322038","014330870","014234286","026029584","007627577","026263794","009415790","004796201","009037490","013813944","009714261","009372696","010530066","009714221","024505265","000347983","006988575","009095955","001226667","009611092","020986158","013424683","026012665","014322002","013413889","000324332","002603777","006941293","026041380",
"009714286","025878706","008047654","027684741","013334643","009399077","008904278","009602483","001173080","007622872","014234282","028721635","009714242","013424789","004085600","013412292","009714260","009709803","009714277","005420996","025938218","007987292","013435489","026050460","010444639","004933004","025806973","000758013","009154160","013334517","003890974","020400540","007038865","013435520","006787262","028712790","008049002","013424748","008904268","006046535","028310629","009714056","009600104","008310735","010041369","005312023","009714251","008038084",
"010161204","005876223","027370341","025963018","001402572","013337902","001346398","009174932","013921942","010701549","013435518","008102376","005056646","028720038","009540723","009163856","005861122","008221718","014322011","000673913","005860608","013347656","000630479","008044400","001275717","010324221","004232518","008246695","008100343","009714227","028685161","005353273","005746741","007649214","013424600","009727436","006689824","008118920","028719228","007479302",
"008941300","013982327","001331560","007695107","006155367","007649221","014104495","028719222","007622853","008460663","009607453","006982126","008895296","008033922","013551775","025924820","004995053","028310279","028392978","008045390","013424533","026836620","026293238","014094219","026891764","008034135","008188672","025939971","009714306","000449180","009061441","009614876","008117344","013424584","009611971","005721654","026057386","008221712","002163676","008056774","006623995","010160383","014110027","008295694",
"000655253","013707676","028681401","001333870","005193603","009737709","001033194","010644596","014508437","005091224","028694764","009060803","009397837","027263578","026010774","005285043","006949853","008045203","005980618","026958547","013424554","014101254","007649216","009597575","009611113","009360425","013370346","007585964","008045201","005027094","009714264","014095914","013354786","014109452","003971876","014091199","000419919","003957683","008332174","009714285","027288893","006981947","000140092","009573950",
"007695298","009594021","001457024","009631978","000863409","009616358","027243699","025697978","013424568","009612636","005919841","009714282","005302784","016660970","007585946","009591191","016148911","001427482","008991831","008555197","010705117","009561175","007262164","013435530","008028504","007585953","013324270","009582229","013424548","013381404","025820893","028692350","000208653","007695144","028639639","007665757","000717732","028720250","009714297","028720708","006767452","025938210","007689407","007185286",
"028683435","006160531","028695295","009636201","025820898","008256675","009714299","007634507","003682556","026767027","008045287","009097775","008276475","026866456","025780695","013348178","028683510","009714266","006658656","009714216","028392491","014095149","009714054","009096874","009612656","025738111","006237011","008066807","000018465","009439091","006913538","007211339","014091117","013435522","006703251","009714210","008042537","028719192","006883886","006245430","000625649","009136411","027675262","024854432",
"007656420","010247517","009714245","028720449","008096614","008420510","010589216","028720822","005091214","007510073","009324689","010463526","014085837","009714219","007585963","009714267","013424793","005293762","026372458","003792217","028695719","007479442","010195588","027626992","009096758","026107908","013435506","014106245","000494239","028719978","013435524","008904259","007160506","013435490","009615520","009714313","006997962","008042630","013424775","008102389","028368441","008221764","027126611","013424721","013413890","007203759","013352430","014091068","000621670","007987117","008221601",
"006871521","009081107","008221724","028392580","008316909","006832138","007661111","010326563","009187662","014234310","000283734","009087870","009714272","028681312","009914627","009140646","004946396","013424744","006732553","024877050","025419170","025940904","013343193","000625166","010705972","003126374","014234284","007649215","027160611","028721671","028720168","013424601","014330882","001456677","013424770","006386878","002354143","028695799","025692931","028721710","028720860","007119751","000616383","009204718",
"028241614","005753062","009133355","009714254","013435526","026886440","006986881","006767714","028222384","013334622","007479496","028351567","001039334","028545580","014234220","028695233","005670885","025955292","006876165","028692183","028695733","028721774","026329045","026541817","025976036","028695411","025926577","009521554","009520650","008332160","006143005","006883562","009064360","014321999","009714311","028213691","006732249","009547853","006016086","010703840","009714309","006619461","001162869","008221748","009064396","009064366","009611962","009035460","009714256","028695297",
"013435505","008221767","005742380","006807424","000202769","005439892","025777706","014110218","007575725","009097361","013435491","006575785","025988035","013424720","001532267","028695727","007623616","010701564","012413819","007636552","000419978","014234226","007635837","005392653","006809696","000279393","013345497","009577369","009594572","013368584","009714066","009088387","013424552","006589309","008981847","013435529","010510759","009326558","009714217","004830186","006624014","000718333","008904257","013435512","009098182","006760735","009714278","009327988","007586014","013400248","008332180","009590773","006751056","007987443","009617738","009617153","013424698","007633632","006389720","014234328","009587815","007288121","007146626","013350476","006940926","010097286","009174283","009714062","004063221","013424772","013370640","000592436","009064367","025979722",
"000614401","013445133","013383675","028214612","007894014","000463737","009714302","005908572","005816736","008360327","009109303","008116271","007128495","014091133","002994328","000720116","008457322","008034134","006850864","009714258","028721621","026692802","004245752","013424792","014023166","013350521","007263341","006740651","000268800","006755839","008221751","025994624","009714244","006826592","008221755","009410298","027278772","004094115","006028263","004629146","007596466","013348821","007249681","003749865","009714240",
"006400323","028393013","008206148","013424514","009134619","014100121","009115356","009714223","013435507","018556286","000325754","007649211","009169479","009936689","002795393","008046720","006840987","006024691","004137431","013420411","014095444","007649219","005467845","006051319","007192561","009163072","004206535","014110731","008114238","028720039","009316468","014234264","000700691","000550935","009098644","028600431",
"009714270","004945832","009140211","000379465","009714239","006925704","010172728","009714279","009783942","013352403","009150739","002346587","028720357","014234280","009714064","009714304","007584602","000629219","028372041","015533068","006793701","005243368","010692238","007649212","025705145","014590225","026925385","014234277","005591488","008221715","000159763","009714222","013851492","007674933","013424643","013424499","013424515","009340207","009107538","009709540","008221759","009714207","014234320","010701600","009607660","005113445","005095870","006955856","008008580","026310965","008048028","009371030","006628310",
"001349775","009714058","000463735","004370074","008036785","009714273","009714247","007987517","008326801","002057068","014110675","009600323","000293911","003262353","006790966","008119753","009098788","027164692","027594995","004533145","013369199","009588546","001316139","008221763","028694880","008221591","028720825","013424537","028201208","028695312","026040496","009583412","009044227","009517208","007695233","007203717","014112190","001254747","005412380","001330586","007629936","008060200","009146658","009618328","005491537",
"004134824","009596929","009714061","010575798","009322901","000806194","006840491","007684923","009064358","009141117","008550325","014091422","000403680","000082950","014034622","041044288","000235233","023596925","003318239","000545442","006129755","000515063","000044638","015585435","004420836","006594381",
"027333596","000073547","019612563","007243060","016517242","009281201","001409145","000433035","007931677","024016715","000573836","022485377","007883498","004532077","023532080","028567939","007284962","000194042",
"007575672","013977228","014106211","009413847","000400682","007636989","028682862","000842718","006414975","004422580","006432371","003085518","007915785","014234274","003541683","000284103","008221733","000025950","005509140","000219138","007695143","008338903","009186571","000720257",
"006575869","013765815","007586022","002733248","002477270","003733163","000155138","000004699","008281620","004531933","005769565","000489720","039928196","007991855","009714220","009714059","009619868","000817326","000469263","008705959","005454180","010702094",
"009714246","003710935","013424519","007695281","007695282","001292602","006720652","009714225","003891388","009714213","002435644","006684013","009714243","004876343","001360365","004809722","009714318","000069607","006521338","008221760","009151097","000165866",
"013435502","001462951","005054588","008221754","003488904","000560071","008904267","009109560","007237561","002990516","000640413","005069787","006695164","009628452","000154575","009611876","009130587","013339382","005300605","009714218","009309342","009063962",
"007649231","014234308","013349845","013343337","013372893","009623234","005732367","014101115","013355406","013435533","028720936","014105339","007479868","005939072","007585973","008221719","014234269","001300799","013339679","013435496","009714310","009593900",
"000333528","009599865","006443834","008349226","009138167","003827140","009527788","007916395","000411634","007623002","007479406","009351427",
"009636867","001283058","008332158","007510147","013349752","013948149","009389031","009596776","042727512","000587348","013435497","000065765","008221716","008045431","013349920","004318774","005146770","008874301","008282154","006641785","003485856","000586604",
"007254862","006343056","004454332","009714307","000482053","000466668","005158264","007649232","000001088","001454831","003980683","007575689","013355405","000275049","005094597","003388928","003924076","009353272","000488930","009160537",
"007649235","000671334","007183863","008221744","013435517","014321994","006684517","007649228","009064354","008221753","008221762","009318779","000588676","008251743","001258320","009104263","000449037","009714292","000896872","009405252",
"007205231","000419988","014322003","027722419","000082162","010705106","007276144","013424535","006893808","003544856","005333688","008347369","000664471","000030125","000634183","013435525","014083584","000478639","008837840","001232436",
"009714263","000834507","008047770","004526011","008308019","005446864","005632352","001426933","006223468","005517820","001408911","013435492","000489731","009714269","008031705","001331587","005730116","000390410","009714281","000230034",
"006573610","008038942","000017424","007695266","009714301","000194033","013352510","008904272","009352088","009604558","000097214","000210195","006418876","009317711","007695106","013435511","014508438","004151871","000818000","009416256",
"013424567","009027189","000637511","009184306","009714312","009714215","013424718","000685648","009134560","000293134","014321995","000636133","006079525","008933610","008253492","007631496","005022723","009714268","008221757","009714230",
"006366779","005299665","013326605","009597494","003237274","006561562","006633331","009714298","006792787","007695358","013424550","013424619","009620939","006586336","009714063","014234218","000195715","008221896","008904299","006912332",
"005852043","014091929","007585974","005647062","013348192","005967406","009714305","000234558","005248248","009714229","008904249","013427104","009546815","008221730","000698813","006638951","008705364","006777487","004966749","006684422","007628248",
"010705099","002613587","012414846","009186554","007575637","009183686","000269367","001228108","027018975","008221731","007991841","000275291","009605606","000539810","009714280","009433457","008263983","009620604","009521489","027164696","013424794","009315840","000089989","009714283","007186611",
"009714276","005487999","009619908","009504575","007649222","000555921","009408770","009081261","008221745","009638717","099000806","004595330","000555948","007635219","014917842","008192575","002943518","005504494","008036216","009150751","000197038","000059060","005848102",
"007695249","005953767","000063647","009714295","014079234","007031866","000858844","014234333","006001886","000048900","000089328", "000048650","000342501","024264458","000598202","040952574","022247994","027237542","006081359","000054710","000050811","016823545","001520004","006561532",
"004831255","006725547","000059549","007766147","021671885","023897661","008088509","023968894","006426067","022918324","000530885","000000183","000063647","026541817","004912877","000630479","005206678","000050278","006638951","006573610","000197038","000018465","006414975",
"009783942","007031866","000463737","004912167","000555948","002613587","000025950","000017424","026040496","000539810","000275291","000004699","000082162","000333536","027164696","000165866","006633331","006575869","007007108","006443834","020400540","000219138",
"008420510","000159763","000202769","005816736","005509140","006160531","000390410","006521338","006237011","003792217","000469263","000390385","000449180","000194042","000550935","007262164","014101254","028719228","028721608","026937744",
"000477304","028545580","000283734","000482053","006575785","005967406","001459705","003085518","009309342","025820898","027126611","000154575","003971876","000611358","025979722","000001088","001557663","028695314","000069607","028721635","028695312","027288893",
"025880689","000234558","026886440","028721710","026958547","026050460","028682862","000065765","028368441","026012665","000194033","028720449","028692350","028720038","028720860","028695733","027594995","005848102","025924820","028214612","028719192","014917842",
"026891764","005129227","027164692","026029584","004995053","026836625","026767027","027018975","027626992","028720825","000208653","025878706","028720708","026329045","025952707","028694764","028681312","028720936","026010774","025963018","027370341","028721774",
"027278772","028721621","025820893","028683510","026902687","028720357","028685161","028719978","026866456","025994624","028695295","026041383","027525121","026836620","028213691","026925385","025780695","025692931","000419988","025939971","028695799","026263794","028695727",
"027684741","028694880","025806973","026057386","000489720","000587348","025955292","025705145","000449037","026293238","028695297","000174052","025938210","025777690","027722419","026041380","026692802","028720039","027263578","000149913","025960384","000204615",
"008038942","028222384","025938218","025926577","025940904","039928196","025777706","028711032","025976036","028695719","026107908","028695251","028712790","000358918","008038039","026767026","007237561","000560071","000230034","020986158","000555921","006803270",
"002163676","000059060","007203759","028189986","024877050","028719222","028720822","008941300","014234333","000411634","007031318","000818000","003310955","027160611","000466668","001292602","009413847","008221740","005454180","028721671","009044227","017172281",
"008332182","006925704","008705959","004454332","003042995","000269367","014083584","014106245","009097361","009316468","001331587","024408387","028694714","014091117","000274940","009140211","005545475","014321994","005967408","009098788","008046720","002346587",
"014234226","009410298","009134619","014091197","013813944","013370640","009109303","009037490","013355405","003146976","000183629","002988561","000419978","003541683","008837840","008705364","005670885","001349775","005243368","009060803","008332180","008281620",
"013427104","009186554","014322003","009408770","014079234","000325754","013948149","028201208","009060840","009315840","006684517","000634183","005446864","014105339","009433457","013349752","014110731","009109560","008332160","009397837","013707676","028692183",
"001784617","008332158","003924076","000293911","009318779","005198927","009133355","006988575","010463526","003544856","009327988","008296600","014234286","014106229","009169479","005504494","006389720","009151097","000210195","004480553","013435502","013982327",
"009138167","014508438","009416256","005939072","014796786","009389031","014234218","000489731","000494239","003733163","009184306","009596929","001254747","008045203","001300799","009351427","009183686","004796201","014101115","000293134","003438261","000842718",
"009150751","009352088","007916395","010195588","009405252","009134560","008308019","013435496","000027735","008047770","009186571","009714263","006623995","005091214","010172728","005742380","014110027","009415790","009174932","014322002","014234264","009130587",
"010702094","006840987","014234274","000400682","014321995","028683435","013435511","008045287","014234269","014234328","009174283","009098644","014095914","013851492","013372893","014234308","013348192","009353272","014091422","006639052","003950886","009326558",
"006641785","009081261","000621670","005285043","003488904","009150739","010324221","014321999","009714230","008041739","010705106","005248248","009163072","009146658","005095870","014330882","014234284","014106211","014091133","009104263","000664471","008031705",
"006949853","008066807","005876223","007161751","009360425","009163856","004134824","009337828","014508437","007622872","014110675","001456677","009141117","014322011","014234220","005860608","008060200","000629219","013551775","014091090","014234310","009154160",
"014104495","014085837","009140646","009371030","009088387","005746741","008100343","014091068","007146626","009115356","013383675","010510759","014234282","009087870","014023166","008991831","007192561","014112190","014322016","013370346","008036216","024104893",
"005420996","009136411","006051319","000279412","007018549","000155138","009636867","007636989","013381404","006586336","002477270","008038084","009599865","000592632","014234277","001258320","013424721","009714249","005368577","009317711","006684422","009399077",
"006624014","014330870","009402426","014110218","009096758","005919841","014234280","013412292","013921942","000463735","014109452","006913538","009107538","014095149","001457024","014094219","009097775","014100121","008550325","000615578","009095955","009081107",
"009520650","009372696","009396113","009709540","014234320","009098182","010097286","014234322","013420411","000324332","014091199","000806194","009322901","009170029","010160383","008047654","009096874","014095444","009122949","014091929","000379465","013424535",
"006793701","000717732","013350360","007894014","013435497","008206148","009927577","007038865","005091224","006703251","013424499","008042537","008332174","010701549","009914627","013435506","013337902","009709803","013424744","013424748","010705117","008048028",
"013445133","013424770","013424775","007684923","013424643","013435492","014783094","006725645","003682556","004533145","004422580","005517820","028310629","013343193","013424515","013435517","008008580","013413890","009737709","010692238","006619461","013424789","026310965",
"009714059","006432371","013355406","013343337","016660970","009504575","013435525","001763266","001283058","027675262","028639639","006375342","028720250","000030125","000185440","005487999","000089989","013326605","005732367","013424794","003262353","013349920",
"000192755","000275049","013339382","000637511","013424550","013435533","028241614","000588676","013424683","013373169","009204718","009604558","013424718","006371009","005054588","005333688","008933610","010444639","028393013","042727512","028695233","028392580",
"006657393","013435505","001110113","013424601","013435520","013424619","010575798","008326801","005591488","013424554","013352430","013435522","010161204","010644596","010705972","009616358","013348178","013424698","005412380","010686358","006028263","005852043",
"009714285","013413889","013350476","013368584","006807424","009594021","010530066","012413819","006386878","013352510","013424548","013424772","006941293","025738111","013354786","013424584","002593668","013349845","009936689","000279413","005647062","008044400",
"010701600","028310279","025988035","013435529","010589216","013424519","008253492","003126374","004094115","010705099","013424793","013435491","000616383","013435516","013435507","010041368","013435518","013435524","013435530","007049013","013435490","013347656",
"009588546","006732249","006755839","013435489","013326602","008042342","005293762","009540723","010247517","006941298","009714295","001532267","008310735","008052834","013343764","006981947","009714292","013345497","013424567","008045201","001330586","000718333",
"009714243","013424720","001454831","013334643","010703840","013424568","013324270","001039334","013424514","001275717","007288121","013348821","007249681","013424600","013369199","010701564","013424537","010041369","001316139","005491537","013435526","005861122",
"008045390","013352403","010326563","009714270","005980618","013435512","009727436","005056646","008256675","008246695","006883886","013334622","013424533","008042630","013424552","007634507","013339671","013374014","013334517","009714064","013400248","002354143",
"006955856","009061441","013424641","006561562","009623234","014652406","003165666","013339679","003727850","001232436","004809722","009714312","009714218","007207497","000072797","009628452","009714215","009160537","000636133","009521489","009600323","004318774",
"009620604","009340207","009527788","007915785","006994082","009714246","009597575","000685648","001360365","005094597","009714225","004595330","005377760","009714217","024854432","003980683","001426933","012414846","009714054","006223468","009605606","000047993",
"004340410","002322262","008045431","000549472","000355612","009546815","000097214","008347369","009714305","007186611","009619868","025697978","009714301","008188672","001226667","008276475","009035460","007622853","009714062","009714242","001427482","009064367",
"005353273","009714066","003890974","006940926","009714283","005022723","000333528","010639580","009714239","004966749","000256981","009714213","009714240","005126626","006079525","009619908","005769565","009714264","009714280","001408911","007623002","008251743",
"001459496","000140092","009714220","009597494","000834507","009714310","028720129","009586603","009714281","000489739","007617894","007185286",
"002057068",
"008338903",
"000419919",
"009620939",
"000698813",
"005953767",
"007689407",
"000592436",
"008221601",
"000067803",
"007991855",
"000478639",
"007635219",
"009714276",
"008904267",
"000140244",
"009714318",
"002094944",
"009611092",
"009714298",
"009714313",
"009638717",
"005879786",
"008349226",
"006262981",
"008282154",
"009582229",
"028600431",
"000720257",
"009714273",
"006751056",
"007665757",
"000625649",
"009612636",
"009714256",
"009611971",
"005193603",
"009714297",
"009602483",
"004933004",
"008457322",
"009617153",
"009714267",
"001162869",
"009714207",
"009600104",
"009714302",
"007119751",
"008904259",
"005753062",
"009561050",
"005908572",
"009615520",
"009027189",
"006777487",
"003162333",
"028681401",
"013350521",
"005849333",
"000362721",
"008049002",
"000621894",
"000500462",
"000636160",
"009714279",
"006418876",
"009714269",
"000149914",
"009714210",
"009714261",
"009624719",
"008510822",
"008555197",
"009714311",
"006143005",
"009439091",
"009631978",
"000448938",
"009714063",
"000347983",
"009714229",
"009714245",
"009561175",
"009714216",
"009714306",
"009587815",
"009714227",
"000853895",
"009714272",
"005312023",
"001228108",
"006912332",
"009593900",
"003237274",
"002603777",
"013977228",
"004337450",
"009596776",
"006912452",
"003281301",
"009590773",
"009714268",
"000671334",
"003449131",
"007011182",
"003989573",
"009607660",
"007203717",
"009714223",
"009714307",
"004158722",
"003049904",
"000863409",
"026064943",
"006638971",
"001011302",
"006247083",
"000622461",
"004637659",
"028720633",
"009714058",
"009714304",
"000488930",
"000261880",
"007516071",
"006684013",
"009577369",
"009611876",
"009607453",
"009583412",
"009714294",
"009601451",
"009547853",
"009611962",
"009714266",
"009714061",
"009636201",
"004137431",
"009612656",
"009714251",
"009611113",
"005027094",
"004830186",
"000720116",
"009714260",
"007211339",
"009521554",
"007661111",
"009714309",
"006046535",
"009714219",
"008895296",
"001402572",
"007627577",
"009714221",
"005322038",
"009714299",
"000673913",
"006024691",
"009714254",
"009640729",
"009618328",
"008981847",
"009714262",
"000758013",
"009714277",
"009517208",
"009714244",
"009714247",
"009614876",
"009714258",
"009594572",
"009714222",
"009573950",
"009714056",
"007674933",
"009714286",
"006790966",
"006343056",
"006767714",
"009591191",
"009617738",
"002994328",
"005175109",
"003259166",
"007205231",
"004635707",
"006630888",
"004249210",
"002150429",
"009714278",
"001512955",
"002990516",
"008883214",
"009714282",
"001464418",
"027690779",
"027675176",
"003984060",
"005939188",
"003891388",
"000636179",
"005106177",
"002435644",
"003389905",
"025926572",
"000190762",
"000287636",
"002867190",
"028379997",
"006366779",
"006605958",
"001462951",
"028696199",
"028099865",
"005451313",
"000412152",
"007476564",
"027207769",
"026961218",
"028393093",
"028098295",
"008221896",
"006507691",
"028697200",
"013588126",
"008904268",
"007987292",
"005597906",
"002131539",
"000284168",
"008368702",
"024076696",
"028598850",
"003527740",
"003130838",
"003057422",
"002941701",
"025194222",
"014116930",
"021242706",
"025494783",
"000416704",
"002733248",
"005507403",
"000165867",
"005300605",
"006872810",
"002325281",
"000419909",
"003485856",
"028720651",
"028704078",
"006840038",
"006886662",
"009064360",
"028392893",
"007221229",
"025304964",
"004538544",
"024907698",
"025822777",
"000552665",
"027480876",
"009160634",
"027171059",
"001011531",
"008874293",
"028639875",
"028720202",
"000174560",
"006826592",
"028720870",
"008263983",
"006643207",
"007649231",
"001739749",
"000219137",
"006633366",
"000157040",
"000419941",
"005730116",
"008221744",
"028681766",
"028600352",
"003963020",
"005105916",
"007254862",
"028415188",
"006617268",
"006596830",
"005732058",
"011490191",
"027345947",
"007276144",
"025899943",
"015088084",
"006638999",
"004531933",
"003388928",
"003941815",
"028720664",
"027701467",
"007019446",
"028718738",
"021086949",
"006512487",
"025813024",
"028392978",
"028571784",
"015189898",
"002692656",
"025914017",
"003161706",
"007649232",
"000466218",
"006633342",
"003974950",
"007092314",
"027717896",
"009570326",
"017104103",
"006320185",
"028720168",
"003981215",
"028680651",
"028719180",
"003806463",
"028683443",
"008077434",
"000541323",
"028720688",
"000202766",
"026041377",
"005632352",
"006883562",
"000092987",
"006860790",
"003841351",
"028388980",
"006860660",
"008192575",
"003124818",
"008904249",
"000608654",
"027055143",
"028720821",
"004142502",
"001331560",
"000327726",
"004232518",
"009063962",
"028695723",
"006411042",
"026957930",
"004314430",
"028379595",
"003270766",
"028392492",
"006969023",
"028095424",
"004297074",
"026332239",
"028721173",
"003317595",
"008904299",
"028309756",
"008360327",
"003154062",
"006893808",
"028678487",
"004191175",
"028044333",
"028694387",
"007031176",
"000291269",
"001703499",
"028720867",
"017551667",
"006566309",
"025957138",
"000145884",
"006307654",
"028342558",
"013765971",
"025901856",
"000283882",
"000091921",
"027738296",
"028241849",
"003990866",
"027222099",
"028680664",
"028720923",
"027110059",
"007656420",
"000254765",
"007695143",
"007649228",
"003793108",
"008221745",
"028720959",
"027110052",
"027940100",
"028720277",
"028004938",
"003153756",
"003228764",
"028276438",
"028696643",
"028600433",
"028379435",
"026059740",
"003599951",
"007207430",
"028397417",
"002087043",
"006155367",
"004211789",
"028720877",
"003159245",
"006427637",
"007064540",
"027136658",
"008874301",
"000284103",
"006893175",
"025848735",
"007575637",
"028237983",
"026371722",
"028721756",
"000307392",
"000403152",
"000398233",
"006948320",
"008904257",
"004251386",
"028372960",
"027110053",
"007046245",
"026540464",
"004308411",
"018602534",
"001011293",
"009840653",
"028688179",
"028695882",
"006925686",
"000279393",
"006659134",
"000450524",
"007695266",
"020445944",
"025840332",
"007238019",
"028342476",
"008221733",
"028721265",
"027533162",
"005716213",
"003524336",
"028694346",
"026059743",
"027984089",
"006629125",
"027815013",
"000164846",
"028720610",
"005299665",
"028310204",
"028720893",
"029512919",
"003710935",
"025760329",
"000307539",
"006507688",
"003461181",
"007095406",
"005725259",
"016347452",
"028720090",
"028098465",
"027055140",
"004571946",
"027078930",
"028611154",
"028568417",
"008221760",
"028392895",
"028146134",
"000836150",
"007649221",
"000385564",
"027492633",
"028640381",
"028720509",
"014671338",
"028684974",
"028697316",
"024618648",
"025840339",
"003529129",
"003954791",
"025909222",
"008221731",
"028640414",
"006588256",
"000896872",
"019740519",
"028598941",
"006767452",
"008904278",
"005113445",
"007586014",
"003957683",
"003919647",
"014783081",
"028311004",
"028414937",
"028611360",
"028351890",
"007479302",
"000062200",
"028397418",
"024526445",
"025188144",
"028611338",
"006895574",
"028351480",
"028497795",
"000992494",
"008874179",
"005511639",
"003645731",
"022807792",
"005879838",
"005769116",
"028233786",
"008162159",
"006400323",
"010598322",
"027065746",
"006633350",
"008904281",
"028155301",
"006916014",
"027976498",
"023334855",
"005637323",
"000638856",
"006617259",
"003755987",
"028613219",
"000151128",
"000408661",
"006647041",
"026243518",
"027116213",
"025795381",
"006082871",
"028151173",
"004806212",
"007011281",
"000390008",
"007628248",
"008056774",
"008102376",
"001062869",
"026064944",
"028720289",
"009064366",
"006439107",
"028239901",
"007048780",
"027985703",
"001467936",
"028342327",
"004933128",
"026512273",
"007002888",
"002556500",
"008221718",
"021679818",
"028397300",
"027975530",
"001148846",
"028720204",
"027817652",
"005359189",
"005451497",
"007025499",
"028720221",
"006647033",
"005500119",
"025820903",
"028695910",
"016347453",
"001033194",
"005064909",
"008904272",
"007635837",
"008295694",
"007585953",
"022099061",
"028720907",
"003913491",
"028720882",
"025822774",
"003886514",
"003026601",
"004307810",
"028611192",
"027842779",
"028392491",
"001028104",
"003974948",
"020812548",
"028414943",
"026371978",
"027732410",
"027164699",
"006078896",
"027841505",
"028664835",
"025745172",
"006941825",
"005247764",
"006620659",
"028683445",
"016347450",
"028151336",
"025910105",
"016526834",
"028720788",
"023978132",
"005467845",
"025795375",
"028557645",
"007575763",
"003741032",
"028571811",
"005158264",
"000154012",
"006948664",
"006247041",
"004876343",
"027055133",
"000419925",
"006893421",
"028720201",
"028310521",
"025740254",
"028664924",
"027711821",
"000274994",
"003900142",
"003050519",
"004039708",
"020183714",
"018894006",
"028613064",
"028392979",
"007010943",
"028310513",
"027975546",
"028393066",
"028353349",
"008221762",
"028310403",
"028310392",
"028697112",
"018770605",
"028031209",
"003014585",
"028389060",
"008114238",
"007510147",
"028639755",
"028697233",
"028639600",
"028720703",
"028664749",
"005092799",
"001346398",
"028697321",
"014671339",
"027537956",
"008924587",
"028310272",
"028600474",
"025863294",
"027907566",
"028237571",
"028389056",
"025820894",
"028696991",
"003024515",
"028720226",
"007649212",
"000505427",
"002158906",
"028309885",
"028721764",
"027533481",
"006948052",
"028241983",
"002502842",
"025866824",
"028598973",
"000529029",
"028720192",
"007575689",
"000640413",
"021354144",
"028571137",
"028275597",
"027396992",
"000428672",
"007575636",
"028389053",
"007064574",
"027834370",
"000176189",
"025721945",
"028692426",
"028639760",
"027976934",
"028665663",
"028379990",
"027840172",
"004358261",
"028720010",
"027533472",
"006639343",
"000585444",
"027397803",
"028096828",
"000403413",
"028600475",
"017116631",
"028613228",
"028683403",
"006629150",
"003179606",
"028279035",
"019599906",
"027476077",
"003993475",
"000419992",
"028311318",
"006948027",
"001544485",
"024778164",
"009064362",
"003827140",
"028720984",
"015460435",
"002592414",
"027069424",
"025105940",
"006658656",
"004946396",
"028598660",
"028571668",
"006646659",
"007695282",
"007695358",
"028720819",
"006555697",
"028716520",
"020166884",
"028678522",
"003152791",
"009064355",
"028368447",
"028721257",
"006641468",
"028600389",
"005069787",
"007182720",
"028680652",
"000361079",
"028594831",
"028721746",
"006689824",
"028600391",
"027397797",
"026048823",
"006893203",
"009122970",
"002174399",
"006645661",
"015253998",
"003381734",
"001029529",
"005645923",
"007064520",
"009692886",
"028392980",
"005657150",
"006104729",
"000477297",
"007695144",
"028721354",
"028155136",
"000269677",
"006643105",
"028721353",
"015775042",
"001028122",
"006695164",
"020166871",
"028706812",
"028275397",
"006001886",
"028310511",
"028353159",
"028209329",
"000055657",
"015887013",
"003635822",
"028379439",
"028697288",
"000353034",
"028714766",
"028720131",
"006633370",
"028311321",
"028720686",
"000276277",
"028311195",
"027976503",
"022973452",
"026627724",
"027710298",
"007586022",
"041577377",
"028678520",
"028594521",
"028598900",
"028612988",
"026111605",
"028089414",
"006377103",
"026331398",
"005445338",
"028683432",
"025790426",
"019770957",
"027831861",
"000219475",
"006619741",
"007011026",
"027839344",
"027840544",
"028389061",
"014092045",
"025735585",
"006575753",
"006994134",
"007695106",
"025882213",
"006400221",
"005514396",
"008460663",
"001947726",
"028600480",
"026329042",
"007010994",
"006808958",
"003740998",
"028639599",
"028494948",
"027166572",
"004939342",
"028611314",
"028097090",
"000745211",
"028664928",
"028678439",
"006805823",
"028393025",
"004895918",
"000195715",
"028571988",
"028165465",
"028350563",
"008036785",
"027613037",
"006720652",
"028720903",
"027398191",
"028351567",
"028697109",
"008221730",
"027166573",
"007095368",
"006809696",
"004151871",
"008874302",
"028600405",
"026437432",
"000240703",
"024641552",
"028572013",
"028236548",
"028639797",
"025791372",
"027831868",
"000817326",
"027397361",
"028694358",
"028367511",
"006969054",
"003602177",
"028310280",
"006914955",
"001603808",
"028275594",
"005503069",
"021824743",
"029512920",
"028598849",
"025955297",
"028095065",
"003457875",
"014092301",
"028600509",
"028721380",
"020169000",
"028573700",
"028697270",
"007025004",
"006683899",
"007237620",
"028310120",
"028598816",
"008221757",
"027543364",
"028696200",
"003260866",
"000843715",
"010286326",
"005507682",
"028697128",
"028718915",
"005124997",
"028640437",
"000419807",
"025840337",
"006608056",
"001173080",
"007600630",
"006871521",
"000362509",
"028310509",
"003678745",
"007183863",
"028236543",
"025944426",
"008221716",
"028367687",
"028367911",
"028160425",
"026059744",
"006427660",
"006861346",
"027210657",
"027113581",
"028310514",
"028720626",
"000307364",
"009064354",
"004370074",
"006860684",
"007031197",
"028719996",
"016686272",
"028494901",
"027907079",
"000068167",
"004307496",
"008221753",
"028721249",
"028720136",
"024582056",
"028600328",
"005146770",
"028705010",
"002943518",
"006941846",
"023543592",
"028600363",
"027535274",
"026629746",
"028600436",
"009422804",
"006689483",
"006400211",
"028720646",
"000268800",
"028720313",
"025790424",
"028720905",
"005519999",
"027701452",
"029738378",
"028611250",
"027985691",
"000058563",
"026065811",
"028720295",
"019713914",
"025857662",
"028162868",
"007585974",
"028681110",
"028720112",
"000327775",
"028720067",
"027837388",
"028571828",
"003913058",
"028697234",
"001464129",
"007095340",
"025391656",
"005439892",
"028721021",
"007649219",
"009293379",
"026208272",
"027975537",
"027842775",
"007649222",
"006160550",
"028718789",
"006912336",
"024931459",
"028720400",
"027985461",
"006340490",
"000480641",
"004708872",
"003151737",
"006432608",
"028720046",
"006620655",
"015337676",
"028266568",
"026972377",
"028637034",
"028704159",
"000858844",
"028239891",
"025738412",
"000223806",
"026959621",
"015392648",
"006912355",
"027210254",
"028697113",
"006642914",
"009064346",
"027373772",
"028095411",
"005013431",
"028393011",
"028353796",
"003263403",
"003299030",
"007585963",
"020448017",
"027654712",
"028275040",
"008033922",
"028686526",
"000291302",
"028354076",
"028718901",
"028149610",
"025868660",
"027985468",
"003055459",
"007695281",
"028611362",
"003811016",
"006916334",
"027754476",
"003894241",
"028592323",
"028573093",
"000416733",
"029738384",
"003660932",
"028310645",
"000347315",
"028452725",
"024408371",
"027832331",
"027115063",
"000242869",
"015460363",
"008221763",
"006893470",
"025848733",
"007695107",
"004049864",
"000067794",
"006054063",
"027090583",
"028720297",
"004206535",
"028680632",
"007585973",
"028697239",
"028720144",
"003011172",
"016148909",
"015460443",
"005102743",
"025885361",
"008221751",
"027537953",
"025732744",
"007695233",
"006982126",
"000463623",
"007695249",
"009326335",
"026046336",
"008187409",
"006787262",
"007585946",
"099000806",
"023736932",
"007160506",
"006840491",
"007479496",
"005721654",
"007623616",
"007695298",
"007479462",
"007584602",
"007649215",
"007575705",
"007987517",
"006628310",
"004324219",
"027527664",
"028711104",
"027809219",
"008117344",
"007585964",
"028389251",
"028392969",
"008186658",
"006639049",
"024431413",
"007207422",
"002972485",
"027975132",
"000403117",
"020168999",
"028720257",
"027841702",
"018777272",
"017276138",
"027541785",
"000450347",
"028415462",
"028031972",
"003749865",
"028151342",
"009689668",
"003218900",
"025895709",
"028718866",
"028572061",
"000419961",
"005389793",
"002529419",
"020683728",
"028032531",
"028379589",
"020164796",
"006432597",
"009176383",
"003380102",
"025791371",
"028720514",
"028720317",
"027840169",
"017868031",
"028697313",
"028035586",
"027830332",
"000355429",
"028354070",
"029738380",
"026975372",
"013765815",
"028678549",
"028612634",
"003573132",
"000363561",
"007006875",
"001202281",
"008034134",
"008316909",
"006760735",
"004629146",
"008028504",
"009064396",
"009064358",
"028721423",
"028711084",
"009103011",
"003646340",
"028613214",
"004624270",
"008034135",
"000614401",
"005302784",
"007649235",
"028720661",
"028415186",
"024315953",
"003916017",
"028695913",
"006755851",
"027094983",
"028379790",
"007575672",
"025691936",
"028640453",
"003955970",
"007047123",
"028240052",
"008102389",
"006540296",
"002573137",
"006092678",
"005878819",
"028696984",
"001682244",
"028389066",
"006850584",
"028494302",
"006589309",
"007631496",
"028594488",
"007991841",
"008221715",
"008119753",
"007633632",
"008221759",
"008221755",
"006984813",
"006986881",
"005392653",
"008122039",
"000700691",
"004945832",
"008221712",
"008033921",
"008221764",
"008221724",
"008221754",
"004063221",
"007649214",
"006850864",
"007263341",
"007479442",
"007987443",
"002795393",
"008221767",
"008221591",
"003569184",
"007128495",
"001333870",
"007987117",
"007183650",
"008221719",
"008221748",
"008221750",
"008033945",
"004245752",
"008221714",
"028151500",
"028310524",
"003808975",
"007222176",
"005453472",
"028720029",
"028613024",
"028573134",
"028720719",
"028697152",
"027453678",
"028600350",
"001467155",
"001187600",
"028311326",
"007018754",
"003810591",
"025718217",
"028371431",
"001629561",
"028678471",
"028720501",
"025911052",
"025793334",
"028692945",
"023537528",
"025742261",
"025241300",
"027839572",
"028573213",
"028087944",
"028594563",
"027052974",
"027533165",
"009179132",
"018882772",
"023758236",
"028098144",
"006018483",
"006036478",
"009840652",
"027880331",
"002631407",
"028379793",
"007479868",
"026299754",
"001915756",
"004168005",
"008778157",
"028720133",
"029024880",
"009692885",
"010564867",
"005257117",
"006357548",
"016148911",
"013350059",
"001463838",
"000162564",
"024413386",
"027473999",
"004255606",
"028469382",
"015533068",
"028612873",
"027672458",
"028379794",
"010281977",
"005519669",
"028720808",
"004075622",
"000205311",
"028612756",
"025818067",
"027696623",
"027536758",
"027835459",
"028275197",
"028613182",
"005495626",
"005064156",
"006245430",
"027836090",
"027116214",
"000291289",
"028680639",
"019587575",
"000795716",
"000166093",
"025624307",
"006436277",
"024413379",
"029738376",
"028613187",
"024924107",
"006566422",
"006792787",
"028393018",
"008096614",
"028275607",
"006916277",
"028697077",
"006994067",
"027984090",
"006893230",
"000223792",
"027076022",
"028190370",
"004246286",
"007649211",
"007596466",
"007510073",
"003685963",
"025733747",
"028539185",
"018052579",
"028161147",
"027613450",
"027615008",
"028352046",
"028697208",
"006994079",
"028720093",
"028310512",
"003074936",
"002926146",
"004428602",
"009037499",
"005450935",
"027267409",
"013435500",
"006925705",
"000283923",
"006808837",
"028243502",
"008035386",
"018927550",
"006915694",
"022637056",
"008904221",
"000047723",
"000611356",
"026062259",
"026560133",
"002030867",
"006994098",
"009660459",
"003027399",
"028367528",
"028678448",
"009139384",
"028353353",
"003993681",
"028719226",
"024173326",
"026988971",
"000167206",
"028379591",
"001374077",
"000298013",
"028664767",
"028030508",
"014590144",
"027367133",
"028354075",
"028718725",
"015186734",
"028612592",
"008253920",
"028600395",
"027974734",
"028573054",
"028309879",
"028098305",
"028088091",
"028710997",
"001612649",
"000082109",
"028664727",
"028389063",
"028720963",
"024799750",
"000550873",
"026327526",
"027212217",
"007479406",
"025848739",
"025905586",
"025737467",
"025901865",
"022497763",
"005665244",
"028678364",
"028388892",
"028665664",
"009980181",
"025575403",
"028275029",
"019937065",
"008116271",
"008118920",
"006893449",
"008060399",
"024793673",
"025820902",
"028150588",
"006893342",
"025755056",
"005729383",
"018205925",
"009965633",
"003237118",
"028098143",
"006808921",
"004262377",
"007613482",
"028721488",
"028088258",
"025789339",
"003017668",
"028311207",
"026478473",
"028683325",
"001316634",
"028600393",
"002599626",
"028637885",
"028611319",
"028415545",
"028572985",
"006389094",
"000449046",
"015290597",
"000531409",
"028309608",
"027985690",
"027162946",
"006594588",
"004285242",
"003646040",
"004695511",
"001420979",
"013616470",
"006646725",
"004506266",
"005079206",
"006689315",
"023723129",
"027704176",
"009714255",
"000367152",
"000073300",
"014791977",
"028309869",
"025820895",
"015674697",
"028692962",
"025848737",
"006818070",
"028678396",
"007174859",
"006400240",
"026040499",
"016938572",
"002761384",
"023921931",
"000355589",
"028493734",
"018830912",
"006824033",
"028594448",
"004526011",
"028310115",
"028720320",
"004701450",
"010532999",
"002478924",
"027731500",
"018208355",
"029724054",
"017085923",
"014843465",
"003934115",
"028639681",
"009942281",
"000033517",
"028213533",
"000083296",
"009119365",
"015229616",
"000230015",
"006016086",
"006740651",
"000625166",
"007636552",
"006732553",
"015097846",
"013626450",
"002601243",
"027985460",
"028310207",
"028211508",
"009294293",
"016968618",
"003410687",
"006912302",
"005435083",
"028034892",
"000219440",
"000463686",
"007015961",
"007633558",
"006915380",
"028089090",
"005372197",
"028683637",
"002405875",
"028640455",
"009973236",
"007579003",
"009082534",
"017166882",
"014113879",
"000283952",
"006858533",
"001335769",
"009972590",
"004201939",
"004560514",
"017116369",
"004433400",
"028310124",
"028720251",
"009090088",
"008221738",
"028718860",
"003991449",
"028718752",
"008329837",
"005689050",
"009932504",
"006997962",
"007829500",
"028692969",
"027243699",
"006707666",
"004117801",
"018752732",
"014431596",
"028664847",
"028352282",
"028600397",
"003479596",
"006371611",
"028238944",
"027842787",
"028389380",
"006497957",
"028611279",
"028354073",
"006177014",
"023481980",
"002803786",
"028594517",
"015627296",
"000268713",
"003563308",
"005771954",
"026006504",
"026263807",
"005991434",
"019074584",
"013467626",
"028392494",
"006647151",
"007027781",
"028030851",
"000597017",
"003508424",
"002381061",
"026661639",
"013419361",
"028680674",
"024931449",
"003768416",
"014740382",
"009105999",
"028711092",
"003157626",
"007291909",
"000622414",
"007575725",
"014091110",
"028389177",
"028397306",
"005512345",
"025205342",
"003808114",
"007207624",
"000274448",
"008332175",
"028275198",
"027077101",
"006570484",
"006969863",
"005498005",
"006065806",
"028640379",
"016933946",
"009042541",
"025910102",
"028721751",
"028692934",
"026271860",
"006247911",
"027813707",
"024408379",
"021934825",
"005448277",
"028718874",
"013729978",
"006646717",
"000141027",
"005404511",
"006893349",
"028573129",
"027053610",
"001800772",
"028718859",
"028695418",
"028718863",
"028598819",
"028683282",
"028096701",
"026603332",
"000469964",
"007517369",
"027839347",
"000069251",
"009357491",
"005541318",
"026799411",
"028697325",
"005495069",
"028721301",
"014874302",
"002235036",
"010480512",
"027052427",
"009692908",
"013678339",
"009963781",
"028721435",
"000412177",
"000586604",
"010067161",
"024931457",
"028683353",
"001966485",
"005796651",
"000027354",
"001977878",
"007095381",
"008904248",
"009926077",
"027871428",
"001294197",
"028721338",
"004946125",
"028274920",
"028413546",
"027985705",
"001420697",
"007293177",
"010132859",
"027055131",
"028719198",
"028367516",
"014837877",
"005496707",
"003855012",
"008244361",
"028718767",
"028594834",
"000530235",
"000389989",
"026893469",
"013861198",
"006839632",
"027157665",
"006689129",
"000390131",
"027975139",
"028310510",
"028310127",
"028241627",
"018831113",
"026050456",
"000306666",
"028721436",
"000362480",
"029724026",
"006638988",
"027519474",
"006359846",
"019346365",
"020166876",
"006623528",
"003243924",
"006684300",
"015494496",
"025492172",
"000463865",
"027449791",
"028600396",
"007894073",
"003870940",
"028275190",
"006638979",
"004842252",
"003505817",
"028721285",
"000267865",
"007680894",
"028493772",
"001462889",
"024870645",
"023670007",
"027710611",
"028155306",
"009714275",
"020445931",
"010534080",
"009384802",
"003766295",
"027834378",
"014424773",
"028598979",
"000279132",
"005880698",
"000142747",
"001463232",
"027834744",
"028088536",
"026173400",
"013729976",
"028415182",
"024493908",
"028389052",
"024709548",
"006432587",
"014409534",
"000284758",
"009136615",
"028392578",
"000187487",
"008048874",
"028718745",
"024575590",
"005797707",
"014393962",
"013884497",
"024559842",
"005342593",
"016548220",
"006555700",
"010177571",
"028354071",
"004085600",
"007649216",
"006876165",
"007125511",
"007629936",
"006832138",
"004146737",
"006982699",
"027420203",
"006575775",
"028612640",
"009316465",
"006955747",
"000333381",
"028137220",
"000373388",
"003990086",
"000678504",
"009739902",
"006639316",
"007254016",
"000699897",
"013473796",
"027054657",
"028692404",
"015460572",
"025793339",
"004170110",
"028720182",
"010327256",
"027982780",
"025900824",
"008310296",
"006313305",
"009347085",
"028149500",
"027840174",
"027886321",
"001467783",
"028683278",
"009395420",
"028640450",
"000450525",
"027731225",
"028711099",
"007633528",
"004218332",
"027719660",
"028309765",
"028683471",
"022902195",
"009348356",
"003783441",
"004174863",
"004193380",
"028594603",
"025863290",
"028089415",
"014823004",
"004407833",
"009368319",
"006948389",
"003942741",
"028720970",
"013632767",
"001071625",
"009919616",
"028044026",
"025722905",
"000029621",
"003605991",
"005022695",
"003694606",
"001010918",
"028711238",
"008332155",
"014791872",
"028367358",
"000530110",
"028680668",
"024550154",
"006418777",
"028275604",
"000333503",
"027674125",
"002001592",
"026896702",
"028571540",
"008932849",
"005696892",
"028696987",
"005705136",
"009155390",
"024699494",
"024929144",
"003146197",
"028720393",
"028279040",
"005912973",
"029495688",
"028598891",
"025851074",
"020169004",
"028613105",
"027052975",
"018193401",
"028354088",
"028241615",
"006247096",
"028598824",
"006515883",
"000398130",
"003804504",
"000291546",
"000837953",
"028157859",
"028720950",
"006360910",
"009671389",
"026792566",
"009120346",
"028088100",
"000629260",
"004685041",
"009095905",
"006914456",
"027984093",
"027985692",
"028239899",
"008115651",
"006839702",
"028310400",
"005181626",
"006893482",
"025538569",
"008271122",
"020166872",
"007164800",
"003154794",
"006893498",
"026533309",
"013858347",
"015460580",
"009342448",
"000655253",
"018311696",
"028273390",
"004856027",
"006644290",
"013473806",
"015839758",
"020454228",
"009925690",
"006643180",
"005495356",
"005246640",
"000450513",
"000731017",
"000403079",
"021532675",
"005606708",
"014110020",
"017371804",
"004310756",
"005557442",
"006925717",
"006041777",
"016553157",
"009587178",
"003794405",
"006123127",
"028692424",
"028600484",
"009911799",
"015839763",
"025900036",
"006915210",
"028266430",
"015286446",
"009399189",
"001463127",
"027533473",
"029738381",
"028570540",
"024412144",
"007223459",
"003814997",
"006893271",
"028573091",
"028603386",
"000143319",
"010506672",
"010591531",
"028275726",
"027227064",
"018441744",
"014446492",
"000325677",
"008981539",
"005292797",
"009657221",
"018650866",
"028275195",
"010698213",
"006994074",
"003815299",
"022593042",
"028389051",
"002637829",
"028310522",
"009337466",
"001333109",
"026373211",
"005451885",
"007095397",
"001773400",
"028275713",
"027985697",
"014819737",
"000398758",
"027175739",
"006321805",
"024708291",
"006330526",
"000275136",
"006925724",
"007649218",
"014887243",
"023171280",
"013349181",
"006487227",
"028665670",
"025932886",
"028721247",
"020239812",
"000686454",
"027132094",
"028095067",
"027975529",
"000611584",
"013633023",
"028692968",
"013588124",
"002515459",
"004410051",
"026029580",
"027396990",
"000636103",
"000276278",
"019296523",
"007064514",
"015113398",
"025939220",
"025750109",
"027097194",
"004102058",
"026959619",
"006722320",
"005771415",
"000147416",
"010486773",
"005643114",
"014933074",
"028372041",
"027052421",
"028154898",
"000626884",
"005432860",
"027143424",
"028720574",
"004168323",
"010528113",
"027633186",
"000315810",
"022118213",
"009097445",
"006510919",
"025795379",
"028392462",
"009932778",
"001703043",
"009100630",
"013324226",
"020452180",
"028147974",
"006916232",
"009891121",
"028554779",
"009369290",
"028686525",
"001858234",
"000086033",
"022437218",
"028678486",
"000394704",
"014792735",
"024412131",
"006910504",
"003905180",
"003653491",
"009966650",
"014834995",
"027809216",
"001927942",
"028664842",
"000608834",
"006647074",
"027187369",
"003928705",
"009347977",
"009159410",
"028389265",
"000082113",
"009398421",
"023645930",
"002735513",
"028310635",
"000548774",
"000731198",
"028683367",
"004928728",
"009739953",
"005453965",
"028681351",
"018417675",
"022819458",
"024364115",
"005271233",
"000454186",
"000617377",
"009287953",
"028238172",
"005547607",
"028664917",
"014884649",
"017382299",
"028664960",
"028002908",
"027145311",
"027975819",
"000276263",
"029027912",
"028711080",
"028680659",
"004597605",
"002858977",
"026010786",
"027157658",
"009184045",
"006411419",
"027474275",
"002146075",
"028664752",
"006456428",
"025863297",
"015278820",
"015639250",
"025528628",
"003068693",
"006534734",
"029266236",
"024780549",
"000260650",
"025818056",
"009739974",
"009169145",
"005507874",
"009347714",
"008332172",
"025402436",
"001429855",
"027819426",
"001179141",
"014894394",
"028637036",
"009358189",
"006752421",
"025745183",
"028163422",
"013818843",
"009543332",
"009334977",
"024505265",
"006588178",
"022609850",
"000068064",
"006740725",
"006895246","009921081","007011150","007563700","020445934","015494500","007064613","005744268","009373718","017134084","017172267",
"009082595","009159653","028100131","009384483","013349177","000325107","008904260","000269501","028613013","022472224","027157861",
"028389182","005156343","028721365","003244784",
"024710775",
"010487071",
"027839981",
"003035410",
"014387900",
"009100903",
"006340492",
"006633363",
"007170075",
"028721364",
"010545819",
"027110050",
"002459352",
"006795746",
"003810103",
"024833858",
"006643127",
"007654564",
"026738766",
"027742656",
"003031282",
"004158885",
"013987251",
"006969015",
"003932265",
"009074402",
"007128695",
"023397098",
"006104363",
"006573617",
"006914612",
"006540297",
"009064374",
"009092639",
"007006825",
"025900810",
"007029194",
"017076695",
"006213849",
"007150241",
"015494548",
"003157909",
"027717790",
"007288010",
"009971137",
"009951986",
"024408375",
"025795384",
"003666132",
"025911049",
"000939819",
"028611295",
"006683948",
"002414284",
"003962139",
"009358311",
"007022045",
"000520334",
"028678470",
"006647038",
"013634870",
"028368232",
"001787500",
"000565973",
"003606001",
"009739903",
"015281647",
"008951229",
"006941515",
"022842559",
"025722890",
"000422918",
"000400680",
"000039005",
"014590225",
"006290288",
"007542865",
"009985353",
"014116896",
"009714293",
"016438799",
"014415470",
"006092712",
"028598827",
"000859933",
"028612983",
"010700344",
"008122876",
"009328258",
"015633855",
"024710785",
"007993755",
"026381260",
"014887461",
"000869811",
"028240786",
"005256459",
"028415577",
"006647065",
"008221746",
"029564810",
"009146285",
"004158157",
"010232154",
"027140493",
"009588570",
"028720104",
"006683643",
"007091468",
"005496091",
"000398128",
"004615445",
"000166875",
"018740883",
"028266566",
"004757618",
"028440587",
"000355579",
"000696481",
"005763084",
"006432381",
"018443455",
"027440726",
"007653791",
"000238147",
"028686528",
"002770816",
"023234551",
"028392490",
"006432633",
"010005942",
"003274185",
"009328678",
"003523018",
"014330883",
"000276273",
"001712592",
"000194014",
"028720023",
"006860251",
"004386336",
"006177490",
"006547435",
"000307586",
"000254800",
"006929209",
"000144047",
"028389050",
"025911053",
"006899746",
"000064958",
"002226078",
"006643175",
"028572049",
"003718200",
"024412133",
"027568609",
"028389266",
"000636082",
"000810745",
"005837866",
"017918256",
"004118282",
"010463487",
"009096735",
"015367385",
"008904279",
"006377111",
"015494494",
"007290749",
"004567753",
"004922495",
"028190369",
"009333390",
"009903800",
"000639218",
"028720500",
"025859382",
"028279033",
"010286353",
"000622424",
"006651833",
"005872071",
"006861239",
"001753441",
"028612728",
"005423115",
"007241611",
"028720107",
"006981716",
"006082842",
"003815988",
"028720613",
"026548173",
"027110058",
"028678510",
"014891507",
"000144759",
"020503912",
"005007328",
"006808723",
"025820896",
"010005178",
"010435881",
"017172266",
"000355594",
"014859195",
"019792727",
"024968078",
"006860346",
"016932022",
"028389179",
"000821056",
"014896206",
"005045527",
"002681474",
"006609462",
"007031185",
"004304746",
"024924105",
"006981689",
"004256332",
"001606807",
"006340564",
"024710773",
"007613483",
"018556286",
"029088831",
"025745174",
"015820170",
"006390491",
"028163582",
"015460401",
"006166805",
"015088086",
"016952359",
"002675106",
"009372619",
"000390141",
"000070049",
"003208200",
"006633317",
"014415286",
"009368897",
"004091866",
"000269526",
"004283268",
"000307465",
"000880066",
"028683990",
"010078022",
"026612036",
"006819678",
"028354069",
"000394201",
"028697322",
"008904253",
"009938911",
"009152789",
"014777989",
"003146721",
"003274867",
"006407774",
"006625552",
"005498905",
"007630437",
"006377101",
"009407024",
"028600447",
"028241028",
"027537568",
"008737054",
"024249472",
"008904258",
"004784725",
"028589506",
"000056105",
"028600440",
"000452110",
"006588173",
"029266225",
"018748298",
"006160570",
"024413374",
"018112554",
"006492789",
"005111257",
"001444515",
"005170732",
"009312930",
"000202760",
"015460529",
"000949284",
"003946997",
"027110056",
"015460578",
"006320156",
"019606032",
"004040914",
"002990273",
"027018977",
"028720649",
"025186177",
"003986095",
"002748217",
"005921438",
"028680643",
"009523008",
"008297480",
"013678336",
"026726020",
"020181528",
"023403532",
"028720930",
"006443821",
"008221761",
"016843007",
"006948921",
"003165074",
"028310286",
"000639261",
"006861058",
"003870329",
"028310516",
"009392714",
"003163816",
"003540896",
"009390221",
"003478730",
"006617371",
"000275015",
"010399507",
"026050449",
"015460405",
"028095422",
"008332109",
"008234872",
"004198410",
"003518374",
"028388985",
"028088260",
"000538805",
"009993581",
"009379314",
"006994353",
"009932726",
"013858345",
"028280309",
"028367515",
"008360105",
"006419060",
"027537565",
"020445935",
"006861083",
"005503655",
"013473751",
"028720216",
"003745106",
"003522166",
"028639884",
"000085150",
"010041384",
"014425087",
"014879789",
"021978209",
"003644758",
"000235514",
"000726282",
"024717851",
"017143916",
"003720340",
"000059180",
"028353789",
"025925381",
"019684439",
"007064567",
"000559322",
"009167009",
"005801011",
"009089081",
"000080768",
"015839755",
"010216582",
"000242371",
"006759207",
"005789547",
"006443313",
"028683318",
"028275393",
"009521433",
"002773506",
"028710994",
"000541041",
"025859375",
"027975131",
"003361987",
"003722096",
"014892485",
"003258732",
"028680686",
"017961786",
"006893791",
"006620664",
"004001114",
"013678334",
"005920400",
"025945416",
"008255913",
"009060858",
"008232823",
"000579434",
"000205824",
"004861091",
"000419761",
"015189896",
"007008857",
"003841333",
"007542868",
"009329410",
"000219131",
"027054662",
"001464330",
"009130230",
"019985368",
"028664884",
"026033467",
"004114277",
"000478679",
"001892782",
"002775348",
"022889320",
"003916973",
"021658358",
"004128858",
"001010347",
"009179874",
"000233397",
"010551566",
"003001231",
"028719809",
"025873070",
"009830907",
"005631589",
"003804742",
"004342287",
"014985469",
"009118637",
"001948103",
"000238304",
"000190806",
"008904251",
"003149521",
"009359422",
"006683809",
"000082116",
"025823781",
"006123000",
"009403857",
"028721328",
"000611455",
"000336159",
"000412161",
"005818940",
"024711987",
"006321798",
"003523810",
"003017043",
"005878787",
"013976283",
"000333270",
"005796715",
"028681397",
"000450581",
"003438009",
"003660057",
"006324243",
"000611352",
"006596884",
"007180170",
"006900418",
"000403162",
"000067795",
"000219132",
"000000643",
"008308026",
"017798212",
"001225916",
"000595042",
"003448248",
"014117390",
"000557217",
"009367811",
"006855280",
"009739969",
"010394603",
"000196642",
"006734964",
"006939454",
"002481753",
"017943500",
"005131500",
"004417792",
"003212398",
"009160633",
"001740558",
"000062084",
"015214674",
"010422250",
"000755450",
"000454116",
"009944662",
"015299977",
"028415090",
"006910449",
"009606463",
"006382861",
"014879677",
"023172003",
"013668090",
"003006752",
"009367786",
"009149590",
"013633396",
"007009997",
"005860141",
"003736208",
"007646342",
"009433666",
"003162510",
"000205872",
"005020881",
"006030126",
"000160524",
"006547390",
"005023051",
"008260269",
"010593560",
"005439848",
"004447532",
"002729241",
"004250370",
"007142074",
"007002595",
"007204450",
"000573836",
"026950236",
"000194794",
"022247994",
"005997601",
"041044288",
"000560500",
"000199714",
"026607462",
"016240677",
"026711915",
"007254833",
"040114437",
"023294167",
"001985793",
"007096823",
"006257082",
"014612465",
"016530919",
"041035924",
"000103315",
"024349750",
"003738538",
"021548564",
"025432352",
"014015349",
"000043561",
"025331162",
"017675960",
"002168355",
"017956312",
"004001844",
"028720324",
"023530913",
"006367317",
"003262554",
"017548083",
"013844250",
"024836864",
"000077293",
"007103284",
"009740629",
"028694778",
"003428569",
"041385112",
"002499238",
"013946189",
"024514820",
"022485034",
"026976999",
"020453349",
"014527643",
"000271510",
"027263361",
"016959507",
"025536576",
"006902624",
"006294377",
"042380784",
"023613570",
"000411874",
"021324288",
"019777614",
"022338981",
"019668956",
"041859369",
"004906872",
"040755121",
"041559600",
"041118783",
"001085649",
"000509181",
"021396581",
"041414777",
"041084797",
"007085960",
"017233771",
"018901142",
"027158847",
"004981535",
"025197536",
"040575658",
"020207600",
"023994359",
"021794029",
"019468548",
"040529356",
"040551573",
"041067617",
"000227428",
"028022487",
"004810276",
"007243060",
"024775202",
"041790361",
"024120769",
"027774642",
"041405009",
"028737670",
"017639601",
"022341735",
"024201308",
"006627850",
"027503059",
"002352469",
"006798035",
"041686094",
"029485925",
"041120342",
"019394446",
"003021047",
"028557951",
"028740240",
"018872901",
"042622313",
"040855965",
"041995094",
"017594368",
"003013762",
"025015158",
"021001820",
"018377802",
"006438645",
"022995098",
"017451976",
"042799987",
"002691852",
"007053299",
"024077620",
"041523827",
"041949938",
"019466336",
"020857941",
"019298739",
"016380784",
"019904150",
"041324066",
"005258456",
"023651871",
"027597991",
"020003587",
"028888190",
"022072950",
"023886962",
"027720282",
"025316209",
"041165608",
"026873744",
"013843658",
"003491308",
"040096930",
"025802040",
"001938813",
"041519813",
"002658265",
"006029666",
"019402316",
"041851869",
"014785741",
"041239826",
"024999481",
"027777941",
"027312655",
"003285552",
"016328421",
"026598623",
"042126357",
"040862189",
"020662980",
"040556839",
"018513415",
"022371056",
"007370147",
"013480373",
"029951738",
"019507116",
"041042755",
"026894211",
"025806457",
"000875556",
"024641715",
"016121901",
"020899370",
"022970898",
"008981829",
"003992312",
"029658890",
"000113026",
"026413194",
"006366601",
"000203654",
"028563757",
"026337147",
"018399782",
"024028641",
"024001149",
"006596452",
"022618615",
"008543542",
"006039677",
"025302167",
"040697429",
"004229767",
"029088929",
"018327842",
"041384733",
"041948358",
"024515010",
"042183596",
"012959264",
"014595869",
"018529148",
"003502122",
"003020532",
"018522069",
"003124797",
"015763704",
"016957638",
"024380444",
"040173234",
"026955790",
"026458273",
"024183949",
"029273569",
"020707002",
"024840295",
"003131759",
"024857046",
"028366058",
"022167136",
"020391950",
"022197089",
"007408073",
"006328041",
"041133471",
"022055579",
"003053804",
"003921153",
"017541248",
"021837369",
"016920549",
"003518353",
"019923834",
"018947600",
"021631194",
"006402512",
"027414333",
"002956158",
"000695074",
"023493889",
"017233434",
"001808497",
"023875793",
"021190873",
"026255492",
"000438101",
"041987551",
"022706028",
"004680077",
"017699912",
"014768316",
"000238209",
"017117896",
"040103085",
"020041507",
"041254576",
"016121101",
"041504125",
"020378968",
"018052891",
"027473549",
"005671882",
"003051544",
"003327678",
"014067154",
"006554041",
"026541702",
"009252324",
"000879750",
"003335645",
"021976909",
"005268479",
"020972842",
"019398823",
"022802720",
"004949280",
"025418227",
"023786876",
"013914327",
"002285548",
"006559618",
"022582810",
"007416185",
"006064075",
"018383247",
"004557786",
"021587334",
"014985228",
"015751498",
"005003147",
"026965421",
"021569027",
"001814685",
"002423236",
"027669153",
"006080861",
"018495936",
"006739769",
"026995414",
"000542191",
"040205656",
"016484802",
"024624932",
"013379620",
"027372063",
"005151995",
"021423010",
"040900594",
"006537147",
"007382241",
"027284181",
"027669641",
"028252855",
"006693910",
"007567714",
"026998836",
"002528201",
"020876858",
"026034215",
"000147698",
"005016637",
"016949732",
"001027635",
"007096706",
"040368120",
"019673667",
"013445060",
"006562135",
"003599482",
"000433725",
"018707962",
"014873280",
"021887067",
"006623582",
"005139524",
"026224218",
"028188252",
"017615156",
"006562149",
"022292006",
"026704575",
"025402116",
"005687759",
"002861193",
"006559710",
"007440957",
"040598130",
"003182307",
"024486210",
"040841316",
"029763011",
"028210362",
"000800889",
"006471953",
"001919127",
"000070957",
"025838799",
"023603824",
"028452233",
"027961012",
"008312198",
"006465200",
"004015581",
"026714487",
"025940659",
"007418972",
"006559712",
"000222823",
"001806275",
"019238085",
"023919686",
"026880539",
"005360896",
"028564722",
"025772219",
"003799343",
"025542751",
"007977142",
"007416099",
"023732906",
"041886828",
"021265942",
"004964305",
"023511038",
"003638703",
"016691273",
"017740815",
"014767750",
"007751362",
"024798270",
"000433727",
"005222055",
"005756646",
"026172394",
"013585874",
"015237080",
"025948160",
"002636454",
"023507792",
"004393528",
"005930285",
"028936423",
"029649345",
"002544910",
"001642718",
"016235814",
"001903002",
"017823464",
"020261324",
"018213325",
"003689280",
"001249438",
"004456573",
"001754175",
"003088527",
"024098994",
"005895288",
"012413174",
"007741324",
"004884010",
"027236832",
"028268417",
"005681110",
"025511538",
"003858238",
"024762533",
"028130662",
"027608415",
"000309609",
"023438431",
"025772052",
"028395015",
"002967313",
"023263572",
"018610040",
"027863742",
"023743294",
"022514702",
"023872417",
"013742026",
"040004319",
"006982889",
"005062701",
"027108561",
"027763154",
"028459792",
"028677469",
"014492726",
"007287666",
"006727337",
"027112081",
"027237785",
"028321996",
"007406460",
"026403990",
"006837890",
"004763798",
"001668363",
"005748451",
"024791265",
"024790064",
"005146688",
"028149051",
"002370108",
"023740303",
"041849836",
"015599257",
"006559811",
"027072832",
"005883510",
"004477725",
"041180125",
"026835027",
"006972995",
"000188268",
"027254856",
"001205857",
"014891810",
"024206790",
"025080712",
"017586561",
"007705239",
"003322028",
"025871160",
"014753309",
"007224618",
"027037025",
"007567839",
"007395649",
"025923956",
"005787929",
"006907735",
"027241506",
"028400825",
"026446441",
"027281216",
"010129738",
"018235216",
"026202906",
"027232574",
"027248735",
"002810761",
"027248383",
"023026270",
"026364145",
"005188816",
"027260011",
"006340877",
"016924242",
"025725541",
"027662725",
"026693258",
"024206081",
"007039612",
"023672127",
"006188589",
"027267100",
"001627248",
"003567391",
"010384778",
"024223578",
"007578835",
"021756085",
"007650203",
"016336033",
"027080812",
"027802429",
"008883134",
"014891578",
"004898598",
"020968470",
"000745492",
"013941898",
"010135335",
"002990818",
"007184629",
"005772505",
"025079464",
"009909550",
"009864510",
"008042957",
"025931128",
"005687695",
"023440933",
"006091229",
"027012768",
"007295196",
"026944714",
"008898633",
"025324090",
"026937358",
"003236921",
"000645587",
"026441931",
"029288937",
"004528372",
"024135466",
"026982580",
"022355142",
"025272774",
"005831511",
"019654698",
"027714703",
"004533537",
"023837392",
"005870220",
"005073152",
"024946400",
"027840234",
"025374841",
"014056829",
"023759808",
"026780362",
"002823840",
"000661487",
"027650945",
"001444331",
"007401332",
"023842496",
"006211263",
"025728955",
"027265232",
"005541683",
"024346039",
"004490386",
"014666492",
"028376483",
"017687379",
"021326577",
"041122750",
"040954534",
"023139320",
"018563345",
"041558534",
"009346379",
"025061710",
"018343300",
"018551853",
"009520727",
"029276723",
"017363423",
"028879381",
"022203153",
"005887058",
"029334615",
"029912962",
"029074576",
"025291340",
"000547878",
"001612445",
"020710672",
"024058475",
"040842607",
"029781345",
"018053816",
"017274119",
"006094629",
"023172009",
"017124233",
"029591283",
"000557238",
"027101120",
"023399264",
"021143069",
"019846248",
"017737737",
"009215561",
"029338191",
"022166529",
"017609090",
"026413840",
"028794482",
"001286056",
"028055361",
"017394942",
"006344556",
"020147165",
"016956827",
"024034621",
"025209603",
"000429182",
"015590501",
"025729992",
"029923746",
"016885341",
"024135313",
"007128320",
"013965864",
"000977775",
"027691791",
"009335667",
"018396508",
"016657968",
"001241319",
"021913529",
"006843610",
"019439885",
"023890180",
"006427678",
"024547896",
"040660021",
"026613211",
"001601802",
"028720096",
"007194934",
"025192300",
"022622148",
"027725467",
"025366401",
"040638898",
"001077069",
"013382965",
"040407320",
"008120566",
"005556085",
"004494891",
"000784346",
"002530096",
"022427714",
"004627983",
"005276385",
"001939631",
"027140079",
"001369534",
"007062832",
"013612506",
"029045234",
"014180904",
"006998423",
"005633380",
"040123282",
"015109642",
"005761971",
"000683983",
"005279615",
"024654374",
"005323146",
"004986484",
"004988371",
"014030713",
"015721555",
"001725233",
"006123806",
"013327418",
"018032938",
"041727723",
"001027956",
"005022447",
"003641794",
"000866812",
"017948288",
"005929042",
"017937389",
"000431729",
"041932361",
"007651063",
"017692481",
"014226181",
"018493697",
"003683581",
"006535806",
"005392054",
"010464867",
"009526155",
"027787071",
"005239791",
"001579273",
"014072239",
"005491022",
"006513282",
"000319042",
"007938633",
"010557087",
"013934267",
"021297935",
"026690790",
"009160702",
"005634158",
"009856299",
"000141660",
"007583314",
"010255184",
"001347721",
"014458708",
"005208745",
"007151478",
"008932564",
"006056916",
"000553560",
"009877311",
"009894357",
"005651562",
"006926461",
"005830769",
"007553116",
"099012673",
"007258726",
"002404548",
"027496841",
"026775618",
"005819125",
"000475131",
"003000333",
"026895188",
"002937153",
"009067057",
"008038508",
"007684574",
"004296727",
"014855194",
"024488513",
"019571435",
"004366674",
"017510252",
"002481874",
"009953003",
"013759654",
"009317196",
"006641133",
"016140646",
"029158076",
"007616793",
"006173511",
"004310446",
"014074331",
"018298330",
"008358443",
"006106004",
"027066788",
"022227099",
"019669291",
"017465993",
"013982702",
"013868031",
"013942256",
"013706869",
"009285232",
"009292738",
"013633707",
"009762609",
"006695577",
"006811904",
"006433670",
"005323922",
"004961487",
"004831955",
"005069574",
"005605830",
"005135003",
"004670571",
"005160681",
"003384327",
"003792735",
"003657564",
"002998935",
"003793131",
"001943449",
"005963239",
"029131468",
"017485530",
"015108531",
"007174742",
"003236544",
"004877555",
"023732395",
"007664320",
"025801893",
"028656678",
"025373315",
"009325796",
"026442991",
"005773655",
"006597769",
"016005828",
"028432252",
"027696285",
"029780887",
"029860331",
"029805134",
"027541797",
"024636721",
"026404948",
"026430050",
"026343469",
"024126724",
"023422446",
"019519380",
"020491249",
"021329857",
"019533167",
"020107995",
"019621426",
"019609339",
"020577290",
"019199685",
"000800671",
"009813424",
"010219222",
"013669864",
"019682855",
"018798307",
"018166324",
"017884270",
"015733433",
"004540179",
"009718979",
"008954588",
"020854171",
"002899995",
"005297516",
"001896752",
"002147735",
"013739882",
"001826909",
"024383189",
"018000483",
"001034094",
"026332026",
"026531933",
"000556052",
"010332686",
"025676396",
"004325599",
"007618102",
"002454206",
"006329055",
"013558517",
"007707587",
"000837120",
"019823123",
"040683502",
"024854995",
"008378249",
"008939787",
"000561376",
"027739378",
"028717127",
"014527671",
"000821045",
"023911613",
"021408421",
"009273150",
"014412517",
"029624910",
"009784969",
"026466499",
"002670311",
"026274167",
"003475054",
"001449870",
"024284954",
"027073936",
"025012528",
"028775192",
"005466802",
"005443536",
"003024163",
"029090188",
"027321137",
"025146195",
"025255713",
"025616125",
"022004703",
"020373296",
"013679931",
"006304315",
"000656191",
"000152102",
"021261756",
"005305661",
"009616670",
"006603196",
"005521269",
"017558951",
"025019116",
"022201949",
"023256760",
"007072122",
"026734076",
"016677862",
"016319069",
"023200197",
"026597655",
"005043091",
"007112230",
"022956669",
"020401126",
"028825561",
"022171576",
"015607911",
"002775550",
"020363341",
"025750870",
"024498957",
"005871798",
"003721469",
"022093374",
"018299041",
"019172306",
"027905238",
"014449329",
"014349277",
"005877816",
"016139630",
"026862404",
"005468093",
"021647195",
"024320335",
"006608851",
"027877288",
"019308994",
"014076334",
"027385652",
"017376763",
"018323006",
"005727593",
"005082198",
"021533735",
"029619823",
"006679407",
"004319924",
"004330097",
"019247340",
"000784024",
"017299133",
"007790375",
"006244667",
"005958068",
"022075849",
"007105814",
"022466755",
"003865978",
"039996334",
"018321624",
"004412509",
"016396371",
"016411373",
"007007327",
"017336597",
"009193488",
"004787253",
"018212659",
"027272769",
"006403370",
"027450090",
"006996105",
"009430257",
"002372109",
"099014517",
"021544338",
"000512517",
"002205159",
"007219007",
"004452647",
"039921752",
"004912891",
"002140948",
"020089574",
"005826934",
"001328647",
"004863679",
"009094571",
"004805416",
"028247585",
"018272294",
"004427608",
"005665310",
"003216804",
"000194648",
"009526157",
"025516482",
"026723023",
"023810276",
"023314190",
"023370854",
"020106036",
"019306359",
"019409595",
"016471892",
"014822514",
"010210852",
"010284315",
"009219199",
"010123759",
"010111128",
"009045956",
"007009975",
"006967887",
"006050245",
"006320245",
"004748035",
"005407482",
"005682876",
"005462285",
"004921417",
"003069788",
"004219910",
"003729393",
"005699615",
"005401420",
"013854096",
"005710821",
"019488829",
"017820533",
"007154471",
"040338881",
"028907704",
"026948548",
"026367514",
"022492625",
"021519933",
"019622791",
"019198749",
"010294919",
"016554934",
"004860285",
"027206207",
"026046157",
"024767754",
"025370693",
"022623074",
"023178319",
"004332447",
"025753968",
"017977315",
"014824884",
"010231666",
"015496578",
"028833330",
"001343567",
"016531643",
"010569266",
"000612889",
"006443968",
"023853323",
"009408855",
"007128430",
"019697030",
"026410996",
"006609902",
"004590417",
"013490435",
"006972524",
"010011752",
"015273711",
"000117740",
"017252042",
"020699797",
"020317831",
"004812399",
"010218343",
"004237019",
"013573711",
"005002783",
"027939908",
"003979019",
"018423460",
"023075837",
"017535856",
"006518766",
"006366844",
"007173713",
"018497739",
"000719185",
"005281598",
"004092306",
"007272922",
"025532195",
"005272182",
"021310689",
"025379177",
"021405482",
"025941401",
"015212113",
"018603690",
"020419659",
"009163940",
"023888569",
"007095686",
"009378722",
"006650864",
"027545556",
"013715775",
"005587483",
"006021836",
"005914623",
"007441078",
"007593696",
"007839254",
"001280598",
"020339219",
"028303311",
"014161208",
"010700180",
"009745419",
"010090215",
"006016443",
"039914017",
"004335783",
"005326081",
"027794350",
"025665129",
"025010808",
"019757838",
"018806737",
"018143245",
"004204409",
"004352396",
"016378345",
"002502023",
"007624028",
"005814668",
"018143997",
"015318331",
"013596639",
"026731721",
"016517121",
"024360538",
"017835408",
"015742835",
"027220994",
"006421382",
"001935194",
"024503925",
"006330975",
"007292413",
"005816150",
"004168226",
"013719019",
"027140250",
"004916425",
"025457450",
"029285687",
"026952244",
"014505025",
"015497529",
"009612325",
"005183196",
"003085783",
"027810400",
"022848625",
"013847179",
"010174633",
"006927389",
"006375621",
"005132026",
"026528373",
"000860187",
"024571245",
"018013138",
"005929771",
"000593732",
"013846525",
"004749650",
"013698386",
"006660912",
"000758249",
"006376239",
"021786634",
"006620721",
"025760947",
"001632568",
"000737652",
"016438008",
"004854535",
"018709885",
"021567360",
"005645163",
"006135210",
"019060227",
"002520469",
"015272773",
"005530460",
"005188901",
"005791159",
"001014521",
"040357509",
"026054847",
"026147879",
"026030924",
"022197711",
"022187273",
"023427191",
"019280181",
"009825560",
"003063802",
"002637755",
"004071522",
"001684672",
"023712553",
"026369360",
"005647712",
"031256051",
"013797887",
"005341267",
"002477641",
"008005275",
"022785450",
"005894828",
"022935494",
"020246664",
"005202095",
"025875039",
"000891040",
"003211479",
"005705281",
"027771624",
"006982437",
"018143788",
"019050485",
"006203362",
"025687136",
"004126302",
"026566401",
"016505173",
"003817813",
"029725429",
"016495614",
"013722786",
"009852461",
"026373978",
"021897124",
"013689884",
"003278031",
"027475135",
"040364900",
"006273284",
"005313996",
"000630394",
"027971630",
"001038717",
"024331020",
"000966163",
"028683037",
"026836265",
"005701389",
"004550360",
"004368369",
"007049384",
"006991625",
"005529952",
"004851438",
"024614668",
"029498693",
"003807000",
"040021464",
"026393959",
"016465628",
"004348488",
"001034138",
"002365537",
"003995983",
"041773434",
"028865969",
"028015818",
"002900306",
"005425235",
"008260219",
"029498884",
"007430277",
"016256360",
"019632163",
"010380434",
"025494947",
"028578178",
"026449495",
"026205862",
"018806055",
"018819836",
"010262593",
"003693848",
"000338140",
"000578297",
"026331302",
"017850483",
"003217596",
"021313859",
"006416085",
"013667952",
"015721448",
"005827658",
"008910455",
"020840405",
"000781594",
"025967193",
"022493170",
"019391026",
"014212521",
"006540764",
"005462233",
"003654410",
"001611886",
"006267410",
"025557262",
"025706525",
"024860379",
"001219313",
"010317711",
"040228421",
"005817345",
"037960455",
"005975974",
"021267084",
"019031780",
"021382634",
"005397806",
"016502822",
"017367570",
"002913762",
"008004520",
"005088405",
"026533054",
"040297898",
"013556112",
"020703012",
"001593728",
"005849205",
"022467909",
"001405434",
"001835510",
"024474870",
"023878443",
"027856510",
"021888254",
"001442020",
"007587141",
"009277964",
"029775984",
"028579953",
"025907701",
"028486095",
"009916532",
"007008823",
"005586762",
"010205107",
"027464267",
"005575927",
"024472674",
"026287715",
"027631823",
"028821280",
"010577920",
"022579929",
"027889514",
"025423680",
"022241519",
"019150572",
"013843393",
"006953360",
"006659435",
"004910978",
"004142416",
"021707842",
"013728171",
"006290788",
"007805410",
"005910078",
"027337585",
"009433456",
"002273309",
"002709068",
"020127229",
"019625961",
"003937984",
"006691412",
"007580368",
"028825392",
"006339419",
"018401922",
"021324791",
"021542999",
"002900599",
"016716378",
"007180778",
"023081514",
"003701624",
"006912528",
"005572416",
"007002450",
"015881229",
"009992295",
"000496791",
"018440961",
"018603469",
"010141324",
"029127311",
"010517024",
"003360400",
"028851877",
"003703109",
"016532677",
"005616868",
"015532688",
"023267655",
"000097264",
"014343519",
"016802339",
"009302089",
"023524379",
"017442263",
"029071980",
"026474247",
"023627664",
"040834407",
"027657423",
"021212146",
"010062283",
"006893075",
"010424231",
"040409520",
"007727929",
"006313204",
"023390581",
"018424122",
"006434582",
"000762434",
"009076267",
"027388114",
"005663610",
"018912036",
"007870016",
"029725999",
"014451961",
"024313289",
"023214685",
"021793373",
"006956291",
"003674850",
"004331319",
"023055474",
"020375932",
"009650117",
"013401713",
"018406913",
"017234476",
"013624738",
"013629630",
"019702836",
"020336262",
"007012748",
"006355108",
"021333714",
"019736486",
"015892879",
"001246169",
"019491563",
"005393174",
"007660811",
"000804224",
"007017969",
"029922615",
"020054963",
"005997710",
"013707456",
"023971163",
"039882419",
"021304514",
"009044220",
"021356080",
"019774539",
"020936888",
"029685025",
"025980508",
"006422849",
"016526983",
"001268920",
"007020319",
"008357365",
"027886817",
"019226688",
"014807649",
"025713657",
"013765967",
"029184263",
"025263372",
"024875274",
"022660149",
"023221711",
"024346983",
"024607742",
"019644929",
"020010638",
"020566716",
"015963223",
"014524488",
"013897850",
"009503778",
"009666197",
"009633719",
"007157645",
"008940664",
"009060804",
"007646854",
"006709937",
"004346583",
"005769111",
"004520018",
"003719504",
"003060405",
"002832081",
"002745551",
"003681252",
"000756670",
"001090970",
"022266221",
"025381843",
"013822524",
"027311652",
"020146639",
"023993029",
"009360110",
"023409850",
"025338337",
"021344329",
"021870137",
"019023147",
"016334151",
"015255465",
"015728777",
"021447371",
"014394906",
"021594487",
"005095529",
"004820217",
"002602048",
"004489960",
"022964577",
"004598927",
"027699076",
"019082628",
"027609857",
"029699009",
"027351191",
"023352093",
"020769079",
"001684528",
"008451583",
"014526322",
"019379242",
"007680474",
"013933876",
"026960967",
"027109796",
"023912194",
"022109436",
"022441277",
"023193710",
"023642654",
"022241034",
"019969749",
"015390733",
"010671332",
"002513720",
"003893935",
"026926630",
"029454180",
"000097116",
"029872719",
"019275201",
"017948329",
"022906277",
"021804422",
"014596211",
"017563851",
"018357450",
"007572609",
"005948821",
"009732470",
"000764996",
"024178735",
"010359278",
"018846326",
"010154070",
"009726991",
"000496942",
"020187454",
"019193625",
"016263418",
"005879131",
"026055223",
"000885315",
"017308954",
"000228275",
"024806635",
"005366458",
"000505152",
"024753619",
"005542024",
"029284325",
"020559950",
"002605790",
"002171569",
"006415577",
"000624553",
"018446165",
"009332818",
"016134305",
"020347908",
"023536632",
"004667505",
"025711923",
"015952476",
"003473395",
"020104625",
"016952315",
"029501504",
"017583855",
"005627295",
"000667205",
"007095353",
"020347279",
"009300688",
"023450670",
"010426001",
"018210764",
"024925602",
"016503541",
"006390879",
"019878898",
"022141758",
"030009863",
"028485977",
"025499184",
"022319491",
"023302720",
"020429005",
"017178573",
"017954533",
"014238071",
"015839190",
"013719811",
"010136692",
"006302973",
"006452261",
"006291008",
"005226961",
"001813189",
"022497618",
"028830458",
"005079259",
"022972442",
"003455636",
"020810264",
"010512746",
"008680692",
"005338691",
"008983757",
"016304302",
"014066752",
"016409118",
"007610025",
"025025886",
"010415212",
"027399913",
"022350724",
"002463480",
"006475859",
"022061225",
"023298834",
"027652660",
"006368811",
"026582721",
"005150387",
"010529879",
"021833618",
"009247369",
"004782424",
"016628946",
"019334013",
"019358188",
"006594265",
"005735000",
"021076412",
"004108528",
"001552108",
"018586517",
"007495000",
"000577021",
"025840583",
"022528462",
"004104086",
"001346173",
"020657910",
"000471080",
"023157459",
"021282595",
"013564672",
"002604573",
"003215478",
"021468906",
"018077056",
"006829683",
"006090181",
"027436520",
"022418006",
"025651454",
"014345364",
"022185324",
"013815018",
"025366157",
"018273303",
"020659268",
"020232837",
"020468462",
"000450310",
"021640042",
"005939508",
"003414055",
"027194483",
"014019906",
"026148267",
"028838230",
"030863109",
"009839404",
"006447705",
"005876562",
"003681928",
"008920837",
"028186121",
"016804483",
"021143587",
"007189684",
"021126154",
"009481899",
"028102784",
"023779245",
"021783789",
"010532837",
"005907604",
"004432021",
"005404470",
"003143000",
"006889657",
"001011563",
"021141558",
"040369018",
"005618019",
"027673347",
"015266329",
"027971737",
"021605641",
"016355748",
"007886509",
"022553164",
"020390111",
"000230765",
"014501410",
"003487297",
"027099419",
"020957393",
"014973469",
"014029307",
"018910720",
"000985503",
"028127662",
"028727794",
"026753492",
"024807949",
"002175034",
"026134661",
"010421970",
"005245215",
"020203887",
"008001182",
"007165929",
"000639615",
"026633962",
"027707732",
"025633232",
"020264871",
"016190723",
"016284252",
"014094363",
"002630759",
"004137357",
"020414458",
"000957312",
"014390014",
"006345423",
"028312252",
"025197076",
"006873236",
"015141930",
"025933008",
"003408680",
"025459916",
"027247092",
"014202262",
"027371404",
"029527497",
"022049580",
"023127552",
"020160612",
"009373244",
"007144315",
"006068502",
"005683740",
"005655927",
"003671873",
"004272687",
"001716214",
"025188281",
"009516107",
"001236689",
"018634891",
"007095385",
"026223347",
"027001355",
"029070130",
"006188124",
"003138402",
"000704315",
"010309245",
"027029515",
"021083731",
"000669809",
"000477624",
"018220873",
"008251560",
"018924171",
"021995163",
"013685797",
"000361667",
"007683094",
"020266138",
"023475091",
"027787679",
"014623772",
"019971261",
"018641424",
"023448631",
"027212298",
"027975094",
"024859142",
"021126541",
"019186248",
"022833554",
"022119995",
"015263591",
"009350018",
"004475520",
"003544253",
"001808896",
"000343281",
"006976460",
"006396082",
"023219665",
"019222335",
"010635360",
"026229441",
"018669183",
"002863175",
"027329748",
"005971550",
"028656261",
"007005916",
"029023796",
"021297269",
"021031971",
"016567985",
"016897660",
"007558753",
"005972421",
"006293848",
"004743459",
"004758230",
"002990255",
"003956782",
"002764761",
"001120826",
"025310335",
"006307902",
"000944395",
"000635827",
"015904313",
"015568303",
"029885872",
"020212164",
"007246651",
"023972034",
"026417547",
"009413451",
"001830999",
"010148319",
"027012897",
"018375033",
"003272171",
"017652053",
"008563264",
"021444653",
"019972075",
"001457101",
"006465152",
"020451141",
"021001901",
"022475657",
"017770755",
"018252531",
"013640541",
"008417329",
"003617644",
"000643621",
"026300102",
"024512185",
"022285456",
"021059091",
"019412152",
"015686784",
"005245283",
"013546763",
"007203660",
"006236901",
"005443196",
"001324723",
"023742771",
"016625612",
"024019971",
"018167375",
"018919231",
"005521296",
"005758604",
"004973413",
"005119269",
"003777158",
"014176552",
"010695168",
"026995706",
"014143203",
"029295536",
"018546371",
"029407977",
"018298197",
"004448203",
"029854232",
"026488756",
"023576448",
"010172900",
"013648721",
"007741765",
"018112237",
"027769107",
"021112637",
"026414813",
"024082956",
"019926735",
"021243261",
"020864428",
"002942563",
"006497082",
"003199103",
"015946100",
"005868200",
"005564825",
"002941867",
"001934977",
"004972599",
"006615315",
"016587987",
"002103261",
"006335025",
"002224614",
"028057597",
"005622943",
"029637637",
"025477174",
"006882282",
"006008409",
"001262370",
"015601834",
"009240838",
"001700508",
"000083425",
"019942711",
"023457709",
"019981981",
"025353176",
"004331325",
"018299510",
"024550073",
"020880317",
"004202490",
"026972312",
"019642030",
"007651485",
"021836477",
"024053582",
"008349971",
"007947521",
"000735053",
"002702121",
"010002972",
"024594796",
"022026500",
"006409909",
"006682489",
"025385415",
"016672506",
"005614387",
"014989066",
"029797314",
"041454314",
"029806637",
"026841018",
"024514409",
"024568764",
"024154533",
"019828141",
"018932361",
"014757819",
"015106740",
"013759875",
"013503716",
"009762197",
"013444692",
"009789345",
"007677459",
"007175068",
"004531558",
"005521977",
"002814568",
"001014341",
"000538041",
"022652772",
"018249724",
"026667546",
"022479252",
"002798383",
"021346214",
"017891259",
"019427668",
"016936192",
"009558760",
"006506046",
"013335759",
"021231928",
"027768999",
"017555359",
"007839593",
"002500312",
"005637362",
"021347521",
"001994024",
"000308889",
"010461136",
"028958558",
"027349643",
"025231196",
"006992191",
"005737204",
"004156094",
"003707250",
"027827064",
"022296978",
"021595540",
"007597103",
"009200899",
"007480513",
"025941453",
"034171977",
"022956118",
"007455788",
"005942309",
"005511797",
"000515135",
"005081747",
"003667658",
"002805250",
"005222314",
"028549581",
"015343987",
"025700543",
"017568859",
"007844682",
"003577605",
"014771607",
"004554850",
"013636782",
"010481071",
"019766852",
"015390698",
"004598935",
"023771333",
"021609488",
"020653639",
"022592762",
"022588652",
"019817201",
"021636448",
"013857707",
"006963332",
"006951885",
"001397329",
"006263548",
"020578993",
"023853776",
"000529594",
"006387205",
"022744908",
"026880041",
"021015685",
"006553713",
"039703549",
"000736030",
"000270368",
"028504929",
"024784246",
"013700586",
"014206251",
"003601824",
"013435822",
"021064667",
"000790107",
"029878272",
"023077822",
"024030881",
"014857227",
"010195065",
"006085077",
"000496198",
"028239726",
"015435286",
"015377138",
"014193979",
"010451062",
"005085285",
"000373873",
"025961124",
"024370307",
"019796057",
"001572603",
"000604003",
"019952445",
"001019582",
"014930263",
"001227268",
"027887661",
"027060912",
"022631474",
"024497773",
"016512145",
"009835365",
"009180467",
"042002702",
"022935298",
"028220494",
"003831112",
"019172737",
"016228294",
"002074147",
"028843224",
"006382638",
"019423559",
"021263108",
"029424815",
"022570196",
"016234468",
"015945083",
"009973086",
"007005851",
"008953099",
"002517100",
"000360568",
"000531598",
"002794835",
"005875661",
"000633009",
"014369901",
"008290764",
"004444848",
"020927111",
"027336615",
"020679664",
"010113401",
"007823962",
"000608888",
"029538345",
"028315294",
"029278044",
"041303423",
"028479118",
"025940094",
"025536458",
"025858445",
"022010260",
"023043196",
"020004848",
"020850967",
"020309240",
"019948802",
"018462712",
"018550327",
"017080185",
"016203255",
"016060836",
"013933677",
"015055898",
"014820148",
"009581092",
"009624810",
"009415518",
"013579596",
"006314660",
"005952137",
"004731028",
"004572394",
"005555721",
"004661648",
"002812366",
"001565330",
"000872375",
"001756960",
"003491574",
"006632215",
"027892836",
"005936161",
"000698578",
"023691471",
"003750830",
"022808981",
"041780297",
"023929314",
"019237733",
"016499380",
"000449244",
"026265922",
"005239729",
"007054290",
"002913402",
"028258825",
"023742726",
"014039117",
"005416514",
"004062102",
"001178359",
"003746499",
"005245690",
"007185777",
"020731832",
"004215672",
"007036456",
"021590762",
"003896449",
"018878683",
"024501018",
"022064223",
"008345452",
"023033388",
"009193327",
"007515302",
"022413406",
"022955458",
"020027883",
"006675292",
"020472582",
"028477937",
"001413453",
"022799903",
"020237660",
"021304374",
"002905001",
"009543251",
"015300781",
"000878797",
"005616392",
"003636056",
"029748245",
"008800486",
"023301036",
"000865310",
"001488083",
"042736914",
"027068084",
"025373650",
"019692316",
"020646109",
"019206854",
"018884373",
"017478221",
"007166294",
"008982153",
"020938930",
"019491922",
"016292063",
"002364639",
"025636328",
"021659709",
"018179966",
"014154222",
"005573989",
"002751854",
"008149133",
"005768689",
"025182728",
"006613863",
"005923091",
"018936249",
"003574515",
"028330407",
"008058370",
"024739448",
"025315329",
"013546144",
"006664385",
"006058926",
"015750041",
"013648660",
"022700980",
"005542838",
"001972882",
"025754971",
"021265323",
"023123354",
"024380038",
"018951026",
"006371483",
"016695703",
"019755313",
"005841199",
"041789751",
"018744474",
"010641406",
"013472110",
"010173388",
"007198769",
"006930918",
"006638607",
"006046262",
"004333746",
"004906983",
"004852125",
"004321963",
"003509529",
"000290875",
"001235842",
"024823648",
"003417584",
"019146064",
"000228057",
"021611845",
"021953143",
"016483327",
"026398959",
"006237953",
"014132533",
"018661896",
"027515595",
"024625316",
"027099740",
"023145079",
"020036067",
"002880624",
"023872972",
"021431168",
"009684666",
"006158520",
"006132700",
"004867935",
"004316324",
"014527648",
"024010240",
"002551065",
"020795309",
"015588769",
"000538306",
"018265636",
"018335839",
"027651985",
"013704176",
"014541177",
"006879936",
"005536090",
"020051394",
"005674124",
"014070142",
"002785719",
"007906568",
"005502038",
"022013025",
"015111357",
"016285857",
"005675120",
"029577980",
"027013767",
"023353281",
"017292858",
"024165487",
"027726768",
"026881405",
"028553027",
"020061213",
"008708979",
"025303888",
"028229219",
"029585556",
"026056412",
"022352258",
"013372590",
"010007908",
"006717494",
"005934508",
"000114012",
"001645046",
"017999450",
"007741115",
"001944457",
"002408335",
"039898002",
"020653208",
"022193715",
"006370999",
"027429083",
"024547384",
"015869402",
"002759399",
"029666071",
"019111929",
"007927858",
"007127855",
"002884998",
"000177911",
"000705232",
"024968206",
"000939623",
"027898742",
"004216689",
"023942180",
"009673598",
"028411833",
"027484880",
"017704164",
"040163418",
"020190841",
"024196362",
"019199391",
"026648625",
"007579386",
"001524882",
"022332552",
"020826498",
"008060237",
"025903342",
"007096664",
"009395966",
"024962625",
"026229821",
"027105035",
"024073498",
"022232974",
"020129175",
"020196357",
"020323559",
"021672719",
"016320269",
"014760605",
"015193446",
"016256374",
"006148452",
"006609724",
"006501129",
"004537788",
"004769685",
"002941154",
"004135051",
"001808901",
"003084752",
"016006506",
"009164348",
"010680992",
"025521434",
"028471299",
"017140782",
"027427956",
"023663836",
"008045136",
"004919791",
"005003212",
"009887569",
"023143016",
"005501534",
"018843196",
"028302793",
"006326534",
"028045881",
"025016396",
"026025695",
"018434458",
"014385401",
"002737053",
"013934064",
"004131855",
"005284524",
"040520677",
"024179955",
"020734855",
"002928124",
"019957283",
"003240781",
"023725607",
"026584684",
"026810729",
"007837586",
"005067895",
"027680710",
"006612702",
"010246572",
"014378786",
"009590671",
"010102624",
"008912119",
"007206239",
"000565336",
"002002557",
"000375010",
"013907357",
"000993845",
"020375471",
"014987518",
"021847265",
"029256226",
"022296679",
"019064796",
"008142254",
"019555074",
"025692752",
"018470779",
"017544681",
"041498890",
"029487227",
"013811505",
"014938323",
"013426305",
"007596569",
"005591078",
"005765637",
"001171743",
"021033212",
"004699414",
"006170742",
"001890454",
"007622452",
"000081574",
"000733737",
"002610731",
"006381762",
"005732917",
"009665821",
"025958630",
"026691533",
"027127802",
"006275082",
"026129423",
"026714655",
"003078795",
"022874035",
"029685460",
"040488304",
"028129252",
"024067075",
"021817028",
"021367121",
"017806138",
"017855030",
"015775711",
"009814071",
"006738033",
"005577010",
"004329280",
"001106480",
"001039815",
"000874128",
"023441066",
"007143885",
"019510254",
"008345485",
"020322183",
"021622613",
"023342564",
"022875566",
"019492828",
"003504961",
"023711125",
"013349600",
"014155264",
"005706800",
"027418864",
"027088990",
"025989912",
"023312184",
"021639856",
"014419352",
"013906615",
"013417100",
"006977754",
"007381362",
"005858977",
"006286592",
"006340950",
"003452987",
"002368316",
"000836061",
"021975545",
"021884063",
"007081515",
"020413546",
"004814089",
"004977234",
"006682391",
"007280865",
"005544036",
"000176663",
"019281316",
"021537834",
"000377591",
"002587731",
"000678831",
"014654386",
"022263289",
"024508148",
"003183702",
"001756517",
"003719403",
"010648872",
"020907984",
"026817215",
"021984953",
"019749920",
"017718617",
"017389089",
"014605769",
"013561581",
"009514022",
"008073423",
"006162450",
"002381153",
"008345437",
"020459793",
"023388892",
"027702734",
"006411479",
"005654615",
"021147550",
"023643028",
"006898061",
"015575504",
"000513855",
"002171678",
"001660544",
"008008084",
"025951654",
"021155958",
"019690920",
"007030634",
"007011527",
"005239030",
"027590548",
"026875204",
"026802108",
"021344140",
"014435403",
"013537353",
"008633295",
"004411715",
"003338700",
"024415311",
"021404029",
"002278896",
"006189620",
"009463291",
"024788930",
"016966431",
"006573537",
"001096307",
"028750033",
"025045179",
"023929825",
"023633113",
"016698351",
"018421754",
"014344194",
"010599820",
"006807688",
"006740702",
"002967727",
"002380077",
"002038607",
"001907492",
"005714205",
"000005152",
"003060170",
"003753110",
"006073580",
"021787241",
"006435951",
"003133508",
"025315793",
"025970432",
"021916230",
"020967224",
"010537269",
"024110921",
"028172627",
"007741411",
"020798297",
"023305240",
"020476557",
"025578903",
"024055626",
"018667330",
"004017806",
"007282235",
"015941246",
"002805909",
"022538125",
"022303698",
"021358173",
"014199949",
"013374645",
"008392457",
"008005791",
"006945629",
"005890257",
"002080054",
"003638256",
"003460155",
"005860353",
"006842469",
"007215709",
"004908450",
"005592804",
"028804156",
"004787809",
"029472063",
"019334942",
"004831899",
"002197605",
"000798851",
"021740731",
"020226092",
"008030538",
"025405110",
"021438565",
"005252069",
"018528141",
"021859892",
"017921891",
"027627295",
"026827993",
"015378980",
"020845331",
"021398338",
"015122955",
"010299495",
"009127631",
"006686436",
"022322865",
"005545519",
"026156138",
"005650190",
"006599626",
"002665600",
"025654595",
"001787738",
"021528831",
"021874192",
"026361881",
"008087969",
"028678092",
"039962668",
"028240982",
"025338586",
"025726599",
"024638929",
"024009360",
"019795183",
"019983320",
"021426334",
"020750027",
"020904823",
"016795854",
"017887049",
"017742470",
"014070393",
"015707789",
"013793342",
"016181352",
"015644015",
"015392674",
"010627023",
"010331332",
"009663799",
"010131310",
"008135742",
"007194864",
"006250401",
"004718273",
"005467721",
"005017087",
"005328027",
"002937404",
"002657690",
"002192734",
"002164163",
"002957536",
"002275611",
"003789177",
"003130520",
"000945217",
"001121881",
"000616147",
"001158983",
"000271380",
"001119419",
"003606736",
"006113069",
"006479996",
"027496936",
"019365185",
"006401872",
"000619057",
"023565710",
"013819205",
"015627983",
"020439561",
"019598951",
"027845205",
"020769344",
"001962129",
"024602704",
"016539541",
"027742123",
"022334953",
"023495600",
"024208054",
"016604438",
"005702803",
"004434436",
"003831314",
"009289759",
"001050101",
"003880114",
"016079730",
"002270143",
"020462912",
"020807600",
"014496150",
"006146590",
"005977795",
"018422403",
"014437490",
"008019217",
"017900523",
"022693589",
"008450086",
"018786452",
"004805177",
"008338994",
"019662071",
"006451740",
"005291478",
"028471071",
"022195001",
"009516515",
"014636597",
"010309157",
"021162568",
"004547609",
"020509704",
"039912417",
"029634549",
"027422766",
"027761335",
"022126620",
"022699359",
"019715447",
"018676501",
"018362710",
"016282067",
"009720666",
"010588920",
"006928996",
"008022817",
"007291024",
"008063371",
"007086365",
"005894259",
"006534022",
"005599153",
"005347991",
"004392799",
"001158836",
"018029897",
"026558634",
"028197696",
"019737659",
"006411879",
"002043783",
"019879646",
"007648877",
"019527887",
"024512788",
"014507354",
"007124291",
"001739307",
"013575363",
"020385875",
"024103993",
"003007550",
"021523728",
"026687958",
"024405627",
"019871904",
"008302203",
"002314217",
"001916433",
"028662046",
"006365432",
"025545791",
"019937394",
"027806948",
"026580807",
"024329098",
"017918850",
"013763908",
"007176379",
"008246037",
"006272524",
"002475652",
"001631088",
"002034731",
"020735922",
"003376512",
"020015503",
"006845937",
"010534299",
"000629915",
"029850586",
"019806898",
"021144066",
"019485027",
"019328406",
"009384781",
"005728326",
"029533341",
"014541124",
"021076347",
"006328087",
"018974895",
"010658732",
"005448763",
"022613557",
"020000014",
"006471999",
"005483317",
"017191793",
"027196821",
"024845975",
"022110269",
"017686068",
"017332886",
"014963503",
"009879690",
"009259989",
"006956501",
"008196190",
"002039281",
"002663300",
"001171585",
"021639686",
"029130348",
"027391490",
"007224086",
"007088558",
"018762971",
"010575874",
"004020320",
"004026480",
"023935564",
"017401313",
"040112093",
"024616989",
"009398356",
"009354142",
"026292747",
"007127927",
"005456064",
"007292029",
"001451615",
"020595965",
"006828743",
"002565900",
"006145298",
"001444943",
"007548037",
"028435372",
"026454668",
"029072056",
"041411182",
"027449896",
"039985912",
"029710561",
"025718028",
"022727258",
"024381701",
"022637060",
"019979711",
"021347449",
"020723843",
"015686392",
"014757762",
"014215871",
"009709840",
"009982487",
"007514832",
"007166608",
"006562540",
"006737134",
"005966254",
"006811515",
"003060957",
"002687499",
"002699609",
"002746670",
"004326776",
"001287539",
"000416087",
"013806865",
"000549151",
"013608306",
"025497844",
"015179343",
"025019039",
"019075561",
"001118273",
"004672768",
"019792237",
"004331071",
"003502439",
"026549426",
"010086555",
"009258788",
"007006472",
"004719279",
"000671652",
"006413911",
"028208002",
"021852311",
"006079049",
"004246923",
"006494691",
"007041656",
"000158687",
"026901896",
"006290350",
"002451374",
"025182720",
"026751184",
"014201500",
"009336212",
"009778832",
"009044521",
"005817815",
"006790790",
"006370357",
"003713457",
"002063231",
"023525495",
"026425652",
"000540318",
"021965380",
"020439188",
"024861188",
"000642358",
"004781056",
"001371692",
"020522467",
"001825228",
"027055087",
"010082242",
"014012976",
"003609948",
"008889377",
"002421024",
"020210270",
"014596304",
"006862734",
"028995379",
"027551351",
"018338060",
"018738845",
"007159314",
"006008564",
"005837466",
"003929668",
"005579239",
"009320902",
"000180221",
"016087781",
"029698592",
"001668914",
"022623600",
"025754386",
"027457278",
"022435305",
"029624903",
"026295717",
"019918302",
"016660148",
"016228785",
"013793071",
"009138429",
"006362259",
"006240002",
"006350092",
"005594298",
"005695343",
"004681771",
"005226878",
"003585458",
"004286867",
"000835917",
"016306560",
"025318256",
"025916543",
"006046257",
"025360599",
"021301628",
"013821079",
"009667140",
"003408460",
"002324136",
"015742884",
"021228483",
"023696994",
"017696723",
"007089394",
"006759558",
"028065455",
"007567551",
"005639611",
"007026855",
"003694302",
"000611019",
"027515461",
"006186371",
"002092487",
"028378272",
"025224088",
"023340887",
"022149384",
"021555546",
"020634043",
"013962679",
"013325593",
"010245386",
"009315090",
"009553794",
"010597799",
"007021734",
"008817186",
"008182314",
"005717136",
"003263493",
"008011336",
"026477123",
"006946960",
"002985183",
"024428674",
"006559641",
"003067607",
"025939098",
"013537644",
"007828578",
"004180612",
"017507944",
"001128285",
"019112877",
"009464571",
"006634657",
"017434553",
"000766824",
"018916091",
"017886804",
"026004363",
"018166546",
"019256181",
"002619625",
"019641868",
"020603790",
"019898690",
"025115595",
"025079202",
"024072819",
"027794850",
"023657214",
"022951041",
"020839530",
"021809420",
"017146666",
"013689320",
"014363921",
"010248250",
"007101170",
"007174822",
"006905510",
"006164826",
"006411990",
"004344781",
"004757244",
"002268444",
"003933565",
"002031863",
"001054689",
"000605654",
"019299691",
"019107854",
"029905884",
"020907741",
"001065492",
"040124047",
"016912677",
"013470142",
"020812914",
"014784722",
"025103124",
"025344116",
"010597082",
"020334708",
"006542536",
"002617863",
"006925399",
"008273176",
"003641277",
"022995355",
"016140150",
"009193854",
"006450078",
"004521725",
"003649295",
"018671063",
"010048729",
"023913195",
"003655156",
"009705870",
"007480328",
"013729770",
"027056824",
"014784771",
"018258018",
"009577848",
"028577200",
"028034871",
"036526144",
"041541769",
"041300006",
"028061617",
"028842790",
"028094314",
"028867607",
"029922947",
"042714419",
"028277956",
"028389582",
"028654841",
"027155893",
"024806461",
"025091486",
"026275295",
"025578324",
"026227844",
"026993420",
"027179114",
"027010999",
"026599481",
"024969836",
"023404095",
"022556852",
"024190380",
"023842351",
"021857587",
"024319881",
"024258264",
"022596149",
"019709140",
"021610837",
"021349328",
"021742648",
"019946365",
"019969472",
"021686506",
"019684714",
"021496098",
"019634966",
"020934038",
"020145758",
"016597604",
"016569524",
"016803762",
"018148643",
"019225828",
"017479897",
"018497163",
"017189732",
"019291550",
"017592110",
"016963234",
"018778046",
"019150606",
"018635544",
"016898708",
"017776385",
"014208273",
"015298373",
"016037666",
"014778774",
"013975926",
"015594552",
"015187539",
"013950808",
"014820341",
"015881383",
"013721379",
"015218948",
"014104816",
"014698280",
"014005897",
"013839411",
"014026408",
"015609370",
"014774734",
"015702367",
"009311953",
"010035237",
"013561278",
"009535629",
"009832929",
"013066503",
"010105397",
"009352845",
"009312867",
"013338884",
"009191212",
"009523618",
"013495269",
"010180228",
"006992201",
"007403586",
"006943044",
"007456106",
"008064180",
"008259692",
"007287455",
"007689368",
"008650546",
"007405536",
"007077715",
"008065042",
"005813524",
"006826535",
"006904758",
"006524224",
"006493007",
"006656609",
"006590109",
"006543739",
"006477766",
"006688002",
"005958137",
"006085858",
"006493133",
"006638929",
"006509694",
"006089735",
"005810962",
"005153758",
"004615065",
"005388375",
"005191297",
"005414192",
"004767741",
"005426563",
"005603903",
"005486215",
"004893452",
"005738648",
"004531378",
"005013054",
"004705392",
"004758498",
"002737488",
"003200891",
"004245348",
"002512541",
"003602420",
"004228118",
"004298055",
"003836662",
"003256525",
"000671740",
"001316351",
"000123624",
"000568210",
"000733074",
"000895516",
"000292754",
"000693099",
"000831825",
"000864345",
"001456885",
"000576444",
"001458973",
"000623458",
"000193869",
"005019731",
"005071372",
"028029426",
"016334039",
"003638667",
"006525264",
"024646660",
"028858319",
"024472344",
"001107328",
"022873763",
"008153265",
"018864294",
"006299164",
"020132903",
"008245043",
"014453051",
"026632573",
"015678793",
"022053776",
"009303916",
"041206963",
"023287791",
"018548290",
"007111930",
"005369286",
"010321604",
"022690670",
"005276054",
"001571776",
"021113082",
"022508714",
"026142803",
"013613041",
"022190720",
"006410992",
"041409541",
"019175099",
"026570994",
"000907857",
"005929861",
"018546611",
"005894765",
"023762159",
"000898888",
"025986667",
"022328195",
"000713805",
"026584244",
"023722287",
"005392655",
"004820593",
"003526441",
"001708384",
"019776528",
"024541091",
"014668901",
"027546726",
"008219469",
"009587456",
"028138135",
"018139932",
"014971360",
"028487277",
"039999842",
"006575467",
"028884292",
"030333598",
"028228843",
"028776215",
"027885465",
"026521288",
"025784740",
"026551696",
"023831180",
"023354358",
"022866521",
"022719511",
"021603677",
"018741760",
"019477045",
"016917701",
"016504118",
"017219184",
"018583745",
"017143649",
"015185524",
"015970441",
"015325962",
"013625667",
"007226046",
"007391692",
"007922782",
"006997555",
"007463568",
"007019193",
"006617166",
"006362670",
"005672025",
"005516104",
"005511163",
"004511856",
"004473129",
"003701812",
"004071083",
"003573820",
"004018078",
"003934514",
"003446096",
"000990692",
"001940830",
"000193613",
"000815550",
"019495726",
"023357048",
"014212358",
"010053373",
"029221947",
"024129130",
"023843543",
"000439195",
"026509228",
"029321089",
"016121474",
"024771689",
"007272466",
"008882666",
"022097274",
"005195546",
"023203650",
"022843187",
"006802197",
"010555905",
"022073887",
"004277026",
"000785024",
"015749252",
"005099623",
"019259249",
"020924985",
"019122498",
"021102667",
"020681025",
"009746416",
"028306643",
"013320842",
"000959421",
"000743566",
"022977041",
"040855238",
"029159559",
"028647027",
"028855199",
"026869131",
"026237158",
"022274552",
"019676748",
"021029039",
"018397118",
"016509900",
"018829818",
"018246441",
"013720777",
"014380081",
"013882913",
"013854949",
"010650631",
"009374096",
"006986244",
"007289570",
"008865930",
"007091477",
"007124679",
"006622581",
"005754626",
"005306927",
"005691036",
"004496043",
"004395753",
"003100477",
"000226429",
"000318316",
"000533619",
"029162957",
"004910324",
"014609714",
"014330078",
"007429486",
"004015589",
"025951983",
"023672991",
"010512089",
"005689197",
"002978950",
"025042690",
"004043352",
"008919239",
"014083183",
"013540405",
"006918991",
"022890215",
"005083329",
"028471281",
"029770562",
"027017996",
"022959042",
"005806560",
"003274070",
"007359435",
"025712173",
"009299729",
"004025206",
"022138080",
"022695934",
"020112231",
"026613117",
"005757106",
"017939359",
"027745105",
"040505740",
"027490235",
"028497487",
"027845009",
"029646036",
"024878159",
"025229769",
"025183396",
"025371445",
"025383825",
"025979214",
"024453991",
"022884490",
"021924110",
"023153328",
"020237626",
"019198058",
"017748955",
"018407422",
"017287927",
"016493249",
"014938165",
"013704244",
"016274919",
"014044875",
"015050519",
"015532271",
"010100956",
"009691035",
"013403071",
"009912396",
"007744908",
"007133472",
"005989357",
"005105090",
"005759796",
"005720992",
"002156863",
"002209685",
"003594121",
"000621500",
"006567500",
"002745916",
"019071352",
"002427287",
"000496470",
"013864170",
"006962112",
"016666596",
"019684786",
"007195271",
"024917230",
"005185104",
"016037908",
"013487761",
"003301205",
"000660965",
"029345850",
"027593476",
"024733001",
"026120887",
"021562991",
"015099900",
"013412796",
"006908223",
"007812869",
"005432478",
"003582740",
"000720263",
"028665829",
"025760620",
"025999994",
"024028090",
"022858479",
"021597480",
"019136812",
"009623697",
"006898857",
"006269681",
"006843103",
"006811829",
"005831332",
"005761224",
"005763430",
"004534815",
"003400662",
"001254715",
"028210603",
"003050483",
"023366249",
"014671868",
"000054691",
"006063013",
"007143721",
"010010852",
"005471527",
"013321923",
"009671697",
"025033042",
"025378159",
"021551863",
"002415054",
"015693579",
"005907084",
"018744792",
"029778097",
"019889587",
"009486215",
"008925558",
"000164987",
"003681086",
"009128654",
"007984549",
"020017490",
"016307979",
"025558306",
"022296409",
"000496730",
"029720800",
"028478155",
"041582185",
"029010273",
"029317269",
"028268770",
"025100997",
"025245337",
"024398184",
"021455014",
"020058412",
"017309334",
"013858286",
"014236768",
"014580012",
"014736865",
"014129997",
"014657500",
"013720819",
"013930684",
"014594239",
"010133920",
"009961974",
"010004634",
"009682730",
"007191525",
"007037683",
"007294848",
"005901731",
"006882345",
"006047624",
"004945655",
"004924640",
"005769488",
"003664291",
"003405688",
"000441641",
"001315121",
"000199736",
"000786090",
"001015159",
"005761173",
"005727367",
"000556523",
"028753082",
"018552652",
"003382071",
"007115940",
"013625582",
"001070320",
"009704882",
"020508712",
"022524991",
"006762715",
"014204746",
"021034724",
"015734247",
"025344677",
"005694287",
"022219798",
"026661493",
"024420316",
"000208433",
"005567050",
"004985950",
"005781641",
"026056196",
"019187835",
"019328603",
"001258638",
"040322305",
"024443641",
"021853028",
"000594889",
"005209371",
"010513078",
"014118076",
"007257612",
"020702793",
"015941266",
"007252730",
"007062794",
"024920652",
"007020925",
"039926187",
"024291354",
"020979940",
"016304064",
"005055469",
"001097430",
"024617249",
"023969861",
"021885542",
"009235251",
"020994584",
"001054357",
"027016694",
"005518801",
"001541896",
"028673411",
"028171966",
"029840892",
"027624251",
"025043587",
"025282925",
"025644956",
"024472224",
"023211892",
"023265836",
"020530372",
"019976911",
"017743487",
"017989541",
"018513928",
"018894990",
"014369669",
"015652189",
"013795290",
"015423136",
"013694369",
"010566962",
"013565471",
"009305665",
"009395144",
"009565207",
"013432484",
"009308764",
"009733230",
"010101210",
"010651148",
"009104254",
"009145974",
"009148161",
"007461452",
"005782682",
"006429999",
"006170444",
"006362202",
"004862730",
"004599316",
"005724799",
"004420441",
"004923810",
"003042022",
"002203399",
"003655387",
"001824808",
"000741262",
"000713736",
"008952320",
"005426944",
"006268331",
"015623932",
"008153311",
"009594914",
"006777030",
"026072518",
"005453215",
"026657849",
"016378320",
"006524064",
"003029104",
"022275204",
"019050955",
"002796470",
"017232097",
"025547417",
"019521145",
"026104995",
"022893163",
"014640351",
"004988387",
"018256128",
"018664004",
"014418969",
"009278177",
"023168496",
"026702869",
"006379409",
"006957842",
"026910335",
"027779688",
"016449076",
"019189094",
"013956386",
"004123447",
"002096722",
"026757539",
"006202586",
"010436203",
"004841717",
"004883199",
"018250954",
"007163855",
"027904083",
"028721937",
"029281250",
"029035390",
"023363917",
"023570273",
"019501287",
"020885776",
"021618297",
"019868353",
"021447407",
"016930247",
"019238601",
"017716106",
"017520037",
"018135493",
"010298555",
"010023271",
"009819696",
"009143213",
"007056065",
"008168565",
"007145417",
"006517431",
"006693328",
"006863220",
"006491757",
"006596992",
"003130351",
"003985069",
"002867229",
"001961573",
"000052154",
"000599102",
"000670717",
"000567443",
"019358062",
"005456977",
"007906976",
"013330600",
"006342405",
"007149850",
"009555907",
"026739712",
"017616279",
"021063198",
"025603955",
"010081813",
"016481297",
"004329447",
"001230469",
"005515203",
"002932095",
"026516777",
"001153771",
"006090327",
"007056639",
"005621531",
"007215636",
"005403938",
"006593382",
"041350105",
"007813308",
"002685221",
"018420297",
"001254087",
"009328434",
"023578549",
"027917884",
"015301881",
"006798167",
"028726288",
"028199573",
"027886274",
"027626974",
"028405460",
"026372442",
"023356420",
"022130695",
"016587937",
"013564797",
"008034819",
"007109239",
"005931563",
"004696200",
"004996273",
"004940701",
"005577106",
"004602886",
"002048247",
"002472870",
"003211789",
"001366914",
"000723259",
"022064781",
"000169033",
"014085982",
"014417403",
"019130019",
"021577478",
"004632576",
"015771602",
"025943186",
"025962892",
"026717235",
"025763516",
"022046243",
"021310857",
"021408408",
"016644260",
"017383228",
"013764509",
"010072902",
"010646087",
"013674845",
"007163334",
"006951959",
"005816031",
"006333873",
"005759695",
"004383167",
"004819297",
"004034410",
"000414515",
"021885842",
"040389170",
"026702465",
"005417553",
"005352980",
"014450460",
"016506366",
"020500909",
"022166604",
"007818150",
"009606230",
"010160672",
"026065398",
"020854375",
"013809680",
"006506551",
"025974856",
"023970962",
"018796836",
"007033195",
"004507645",
"020111894",
"010454764",
"008675642",
"029021013",
"006985565",
"007245475",
"020261412",
"023053489",
"025059646",
"019309734",
"004130804",
"025893776",
"018722964",
"003258273",
"005528362",
"023714770",
"005374114",
"000760166",
"007129879",
"004878556",
"015207476",
"026997036",
"022021305",
"006669372",
"009162279",
"021420251",
"027540407",
"021118124",
"022125571",
"006391195",
"028230758",
"022544976",
"022873494",
"024007679",
"006856551",
"006255215",
"018503603",
"027551937",
"005452196",
"027685814",
"027889533",
"028154053",
"027383159",
"028796993",
"026350786",
"026965866",
"023999872",
"023774444",
"021536254",
"020167609",
"020139009",
"021310501",
"017643874",
"019344453",
"018164777",
"017363409",
"018117353",
"014133540",
"016024328",
"014384921",
"013559792",
"013484144",
"013448074",
"007885471",
"007605458",
"007894507",
"008330045",
"006572600",
"006131855",
"005934738",
"005701893",
"005719849",
"005766913",
"004271960",
"004117499",
"002528197",
"003094779",
"004279349",
"001279435",
"001587870",
"001241501",
"001084108",
"001110401",
"004521879",
"001723258",
"022528049",
"013818532",
"007976900",
"003510484",
"023448615",
"007148263",
"020936383",
"010254619",
"004582274",
"024789399",
"021055279",
"008344974",
"027301788",
"029909361",
"003997273",
"022887264",
"023746591",
"008097903",
"006901260",
"020467645",
"010248960",
"000406206",
"009986806",
"010641820",
"001288124",
"004598861",
"002096745",
"000477010",
"028557096",
"018204981",
"000840708",
"013402344",
"029195328",
"009387870",
"028315534",
"021604202",
"029668834",
"021490402",
"013732385",
"006517814",
"006741484",
"009727645",
"021638350",
"002399365",
"010144248",
"021948513",
"014519052",
"001682781",
"006026347",
"017887252",
"019403519",
"021821991",
"029517083",
"026263862",
"025896981",
"026226291",
"022273925",
"024537867",
"018794511",
"014240255",
"014620262",
"016275689",
"014209992",
"013793938",
"010160013",
"009208639",
"013447935",
"010593865",
"010545889",
"013639267",
"007019382",
"007612149",
"007127918",
"006368613",
"006166138",
"005839929",
"003620682",
"000581680",
"000437351",
"001959964",
"000046910",
"000710757",
"006863153",
"004784715",
"021449545",
"003611253",
"026978326",
"027369677",
"022027060",
"002748062",
"019149800",
"002765386",
"021241199",
"015678699",
"024950628",
"000419244",
"021276693",
"004222272",
"003343449",
"028950129",
"029749900",
"005293275",
"003053851",
"003003508",
"003620601",
"022954175",
"014024886",
"021291694",
"024622319",
"009303012",
"009888431",
"005768951",
"006196090",
"026740342",
"018981483",
"025760366",
"015131914",
"023886515",
"022442485",
"002660189",
"007705347",
"007653842",
"001493832",
"027862488",
"023340483",
"013840573",
"016292969",
"010023360",
"007821315",
"020375713",
"003390368",
"005020830",
"010455981",
"001407335",
"003574388",
"026369159",
"001742666",
"008065807",
"028158667",
"016540238",
"006619892",
"040411596",
"028987791",
"027698599",
"037456642",
"040144045",
"027750752",
"026360435",
"027324084",
"025219940",
"025363978",
"025727383",
"022326949",
"024271147",
"022304877",
"023799097",
"024566840",
"024290871",
"024438035",
"020673369",
"019978872",
"020000157",
"021258771",
"021075933",
"020593499",
"020655725",
"019545664",
"018297909",
"018967470",
"018407560",
"017961201",
"014643008",
"014806734",
"014360622",
"016381231",
"014452264",
"013833628",
"013961895",
"016320191",
"010553654",
"010535581",
"010568353",
"010384261",
"010523336",
"010477357",
"010160071",
"009375183",
"010349208",
"013544442",
"010546936",
"009504346",
"009988439",
"007063324",
"008871690",
"007257708",
"009055728",
"006934969",
"007894663",
"008256002",
"007902075",
"007740016",
"007577229",
"007620388",
"008115984",
"007053585",
"007169829",
"007026672",
"007817641",
"007584491",
"006820592",
"006585313",
"006863140",
"006739547",
"006478390",
"006547647",
"005859104",
"006492702",
"006220428",
"005871815",
"006732517",
"006452313",
"005308666",
"004967882",
"005572503",
"005165743",
"004623729",
"005295410",
"005538341",
"004524607",
"005071660",
"005303634",
"004783247",
"005156893",
"002755677",
"003365760",
"003252440",
"002384190",
"004014130",
"003521062",
"002489726",
"003006369",
"002781621",
"003171525",
"002585205",
"002712675",
"002657498",
"003199360",
"003471494",
"000597644",
"001171185",
"000909978",
"001404524",
"000813427",
"001635911",
"000487226",
"001565351",
"000630094",
"001727603",
"000874881",
"009069742",
"021321378",
"018996694",
"018728911",
"003360488",
"028270917",
"005035393",
"026169600",
"021922021",
"004432903",
"002590977",
"005581356",
"019197586",
"027467411",
"028144720",
"009269428",
"028630049",
"020475071",
"004778902",
"021635096",
"001486648",
"006172696",
"005480213",
"020706529",
"010188274",
"026357476",
"003740134",
"004147773",
"022008440",
"006342477",
"006543064",
"022752612",
"001122288",
"016279015",
"006558624",
"022826021",
"017725274",
"006155079",
"006545520",
"027978672",
"020588014",
"018817358",
"013686123",
"006740638",
"004468957",
"000828704",
"000938194",
"025539158",
"016407335",
"003115453",
"006465193",
"014878525",
"025533604",
"010244166",
"002680778",
"099000328",
"010652244",
"014923379",
"024599188",
"028646792",
"001097528",
"001045205",
"009361078",
"013446485",
"009306839",
"021103400",
"003077528",
"025383746",
"000205114",
"029201914",
"029063408",
"029163761",
"024920559",
"026812680",
"025281009",
"024529868",
"022953127",
"023913447",
"019237739",
"018906921",
"018415168",
"016936222",
"018493971",
"014083642",
"014115842",
"010247531",
"009949220",
"010228569",
"013410783",
"010591775",
"013342336",
"007156661",
"008195149",
"006572031",
"006268578",
"006310220",
"006812081",
"006805451",
"004375660",
"005053145",
"003971027",
"002944139",
"003921673",
"000787168",
"001731834",
"005552161",
"021597711",
"007140135",
"022851227",
"020529992",
"018497197",
"006930093",
"021216642",
"099013891",
"005873319",
"028234812",
"013503147",
"005651079",
"024127452",
"018437381",
"018263521",
"016679622",
"009152156",
"010676600",
"008135653",
"001138718",
"001717930",
"009779204",
"001248588",
"016744022",
"005550393",
"022975067",
"006772548",
"001085647",
"006600493",
"019758800",
"005067807",
"040005210",
"005865580",
"015951184",
"027825264",
"009324998",
"007121303",
"005024905",
"028055709",
"005671887",
"020284469",
"003536045",
"004000439",
"005040374",
"006184757",
"006728817",
"006823177",
"019286290",
"005886257",
"005750958",
"022292763",
"006526612",
"006754863",
"024676627",
"020213605",
"020594463",
"028273330",
"006415421",
"022472377",
"019113973",
"002660441",
"029171082",
"029432400",
"040048832",
"027981431",
"025445893",
"026910528",
"026928521",
"024833550",
"025882788",
"025460079",
"025931470",
"019696425",
"020225769",
"020983191",
"020716233",
"019140983",
"017505719",
"018853999",
"019307360",
"015220074",
"014025582",
"015628498",
"013742954",
"016186990",
"013969190",
"009507138",
"009847824",
"009316748",
"010013573",
"008339972",
"007058429",
"008952201",
"007242492",
"007864807",
"008553747",
"007390995",
"004800822",
"004550071",
"004663593",
"004975748",
"003480589",
"002951915",
"003504057",
"004114272",
"002652246",
"003101704",
"003710550",
"000189249",
"000407064",
"001490807",
"000361613",
"000151665",
"006165302",
"000788792",
"027601941",
"009464742",
"024955405",
"006735762",
"004840845",
"009753776",
"003914384",
"020350963",
"006969156",
"019785594",
"020382166",
"008969440",
"005266054",
"003270206",
"026548906",
"006317986",
"022418113",
"010648528",
"020522502",
"006124657",
"020384251",
"026220598",
"010177926",
"007179517",
"006302600",
"001458391",
"006118307",
"005561497",
"018845263",
"024458573",
"022797483",
"010008872",
"004216592",
"028376172",
"022707440",
"014917331",
"009789747",
"007029819",
"008590658",
"004554561",
"024444688",
"009715785",
"006134529",
"023640103",
"001673647",
"022703239",
"020385956",
"025928424",
"021601916",
"013720026",
"008245628",
"014017088",
"005787459",
"020359263",
"028797563",
"030029104",
"028979395",
"027821107",
"028299954",
"029000407",
"026420583",
"026348627",
"026038699",
"026612271",
"023676510",
"022963735",
"021576055",
"017984512",
"018189533",
"014559104",
"013815669",
"014016124",
"014838504",
"013829840",
"015778904",
"009180942",
"009276165",
"009379712",
"013489767",
"010307675",
"007887805",
"007179837",
"009050468",
"009114073",
"008937545",
"007744721",
"006877025",
"006403455",
"006479496",
"006088911",
"006883015",
"006350124",
"005262923",
"005603246",
"005234955",
"004919292",
"002594133",
"002982163",
"004057285",
"001092363",
"000964791",
"001935031",
"000785862",
"000647680",
"007242603",
"000691133",
"026887971",
"018591357",
"004720198",
"013813410",
"000209333",
"005934706",
"022767351",
"027073069",
"006084558",
"007204788",
"010680007",
"022901692",
"006810859",
"021127383",
"002057243",
"006057210",
"018087096",
"013497931",
"025670038",
"007084323",
"035555276",
"026754466",
"026922969",
"026309770",
"022023383",
"023195259",
"022087266",
"019673459",
"020874192",
"019913790",
"016924074",
"018754677",
"014388955",
"007887766",
"008681388",
"007977506",
"006953980",
"002783827",
"001737396",
"028366035",
"028136993",
"022244141",
"019933952",
"018824783",
"017124076",
"014974141",
"009694965",
"005836278",
"005160021",
"005655643",
"005290206",
"002576362",
"002408078",
"001802321",
"001255401",
"005567333",
"003103641",
"026082319",
"001250804",
"013944424",
"025823122",
"010673955",
"002148804",
"016917594",
"006934029",
"001609233",
"024983828",
"006626541",
"026796780",
"013714764",
"007600476",
"001197136",
"025982313",
"007603103",
"001243014",
"024278038",
"020168132",
"021077793",
"006327247",
"001033516",
"020366562",
"026201198",
"023011091",
"005289203",
"006141581",
"007216965",
"018166882",
"005667879",
"019368617",
"000787777",
"000731361",
"001794133",
"008263370",
"019744227",
"000405664",
"005785898",
"001660633",
"021269151",
"023337931",
"028075903",
"025399781",
"025373212",
"023976202",
"023988183",
"022134766",
"020056769",
"021715622",
"021563785",
"018943431",
"014393040",
"013828647",
"016317732",
"013924123",
"016209520",
"010706831",
"010691351",
"012752514",
"009963134",
"010414042",
"010109208",
"009793622",
"010048991",
"009380783",
"009122662",
"007823143",
"005957510",
"006393163",
"006398961",
"005817112",
"005908810",
"005922492",
"006390324",
"006834026",
"005370588",
"005173339",
"004130825",
"003530083",
"000903173",
"001448854",
"000957274",
"000493809",
"000782731",
"001182918",
"003764399",
"004223961",
"010529953",
"007642322",
"003794556",
"006870196",
"004821209",
"018465322",
"008448755",
"000617251",
"003313925",
"025907850",
"006667679",
"027168024",
"022750961",
"020777451",
"005306996",
"016163687",
"007578350",
"021035005",
"006166662",
"018538345",
"009225169",
"000509416",
"017345882",
"029497088",
"024784859",
"006443391",
"027740826",
"005064972",
"026901210",
"010446985",
"025736666",
"020339168",
"009280291",
"005618901",
"004583814",
"009512272",
"010323612",
"008781417",
"003971235",
"021461243",
"016391365",
"020096402",
"018880020",
"028914230",
"023542082",
"018350401",
"014151473",
"014019304",
"015227481",
"013913439",
"005201237",
"013571096",
"018346078",
"013820133",
"007637949",
"000723114",
"000100884",
"009256205",
"029434489",
"027683991",
"026987977",
"023265780",
"022752818",
"021950470",
"020960871",
"019918673",
"019646669",
"019524156",
"017777128",
"018204656",
"019020450",
"015574211",
"014384914",
"015952467",
"013715299",
"014518626",
"014565764",
"016367001",
"016299378",
"015905372",
"014971237",
"009869364",
"010571441",
"007607348",
"007038241",
"008958504",
"007094955",
"007966960",
"008151054",
"009059057",
"006879131",
"006343027",
"006519284",
"006323930",
"006165278",
"006639579",
"005600860",
"004415705",
"004801742",
"005496810",
"004117627",
"003294958",
"003949621",
"002501620",
"000073416",
"001415844",
"000624249",
"001212357",
"021017423",
"009218872",
"005953785",
"019436724",
"005819007",
"004203166",
"028303721",
"022062494",
"007098749",
"006430715",
"008906195",
"020091461",
"005361699",
"027755897",
"007126330",
"026097415",
"041583540",
"007131628",
"022581543",
"004675608",
"028135958",
"009320936",
"028317523",
"015491821",
"016160108",
"019142548",
"005174790",
"027181607",
"009399461",
"006464028",
"002160698",
"002534400",
"001572399",
"000999501",
"027111643",
"014731369",
"000664524",
"001194991",
"003853626",
"019593934",
"020397190",
"010151798",
"005139060",
"002983862",
"021132273",
"007156904",
"003694781",
"003757130",
"000694593",
"000491773",
"023616908",
"006047753",
"016158195",
"019585077",
"016638486",
"018376170",
"005828097",
"005660460",
"003308303",
"002962178",
"000866236",
"020630861",
"017251641",
"012957492",
"010414745",
"014618801",
"020481648",
"019127723",
"019767367",
"015697970",
"029004356",
"028674572",
"026665774",
"027077204",
"026334628",
"022035725",
"021271056",
"017242111",
"016856334",
"017736024",
"017143047",
"013795232",
"015665504",
"015173959",
"013584349",
"010561066",
"009235776",
"010305640",
"009484467",
"007641610",
"008265733",
"006935056",
"006273059",
"006848789",
"006076453",
"005036467",
"005231470",
"004837743",
"004126473",
"003761601",
"003238703",
"001837918",
"001437657",
"000415828",
"000501992",
"000580086",
"000860991",
"013808924",
"027036108",
"013689923",
"006075890",
"007030218",
"003477720",
"009876015",
"019864932",
"001099587",
"009278050",
"019681793",
"023097963",
"024794544",
"028526265",
"009118640",
"006032569",
"006249603",
"001670337",
"001875203",
"027922382",
"019470177",
"019809888",
"041806933",
"005511692",
"025258430",
"003369403",
"005036994",
"014974051",
"024799985",
"005366470",
"017625075",
"004758846",
"005497701",
"021676129",
"000608326",
"001823224",
"009375243",
"019861124",
"027535928",
"028626759",
"028658154",
"025949646",
"022448380",
"020458484",
"020576788",
"016499123",
"019036392",
"010143370",
"009327129",
"013559218",
"009884255",
"006193844",
"006457126",
"005776040",
"006221325",
"005243440",
"005387402",
"005359883",
"003107667",
"003635815",
"000970924",
"001608732",
"020678325",
"021714402",
"018464444",
"013687815",
"009254825",
"009118431",
"007289164",
"006672083",
"005915119",
"006902566",
"002762793",
"002672021",
"099014889",
"026818117",
"026397352",
"001220664",
"028274108",
"005252863",
"005629787",
"022914336",
"025344329",
"004033450",
"013861914",
"025730708",
"024239471",
"018971165",
"019028712",
"018382937",
"015305892",
"014124997",
"014102841",
"016047383",
"007078892",
"005924137",
"006856438",
"004131330",
"001420299",
"000644659",
"007479334",
"029678595",
"019189874",
"005142452",
"024194541",
"015243788",
"018589560",
"002790991",
"020953569",
"000723525",
"016191880",
"016190874",
"023678218",
"026204251",
"024783122",
"017579081",
"013764008",
"006891102",
"004927767",
"009784280",
"019389473",
"002416939",
"006614699",
"010169919",
"023898762",
"020845468",
"027606701",
"002500377",
"019743787",
"027558400",
"026769121",
"021993276",
"019242869",
"009835882",
"007585332",
"004931321",
"008414318",
"027309732",
"009394135",
"013656248",
"005413330",
"014625107",
"006654522",
"010192837",
"025268232",
"006533628",
"013960750",
"009898531",
"026658814",
"019980574",
"028434781",
"021917585",
"022022741",
"017707527",
"014361669",
"009374129",
"013475055",
"009619805",
"009224084",
"007552747",
"007849326",
"006284897",
"006302546",
"003307971",
"002706952",
"003102486",
"002351004",
"000720722",
"025623750",
"009105130",
"015896133",
"005801351",
"027767345",
"026399462",
"019750157",
"024302465",
"019524119",
"014406526",
"008910722",
"006614787",
"006195886",
"005668621",
"003227153",
"003365895",
"001256870",
"018368043",
"017143544",
"018112163",
"007157478",
"004855010",
"002196223",
"021404471",
"020075937",
"007358529",
"019413076",
"026355456",
"020166810",
"015657933",
"005729844",
"002912159",
"018390419",
"020267246",
"006045440",
"006030343",
"004479772",
"003503047",
"026481782",
"022816452",
"009641049",
"024421592",
"026519411",
"000318729",
"020541728",
"005975224",
"005905768",
"018790380",
"007105349",
"006143370",
"022079047",
"041543011",
"018421750",
"018724457",
"015877113",
"014421454",
"014083048",
"009314238",
"009169612",
"006244738",
"026315490",
"000230555",
"025040802",
"021844493",
"027250748",
"018487458",
"027859510",
"003740339",
"006257862",
"003572232",
"010397737",
"021875346",
"000445644",
"020997009",
"013830943",
"007105940",
"004908127",
"004995862",
"001082935",
"002708984",
"013798490",
"002503835",
"024692811",
"028616293",
"006240937",
"005160644",
"014502000",
"027577688",
"015195160",
"010271109",
"014359711",
"018923463",
"000795248",
"009725349",
"020996929",
"024413605",
"022934881",
"026430467",
"000671954",
"010394069",
"042103972",
"017661150",
"022544794",
"020757378",
"009434649",
"040482579",
"041091539",
"021093813",
"020039015",
"029266797",
"005860540",
"023208769",
"023226250",
"018315770",
"040054591",
"024722476",
"017424098",
"017429954",
"024336006",
"040611962",
"021280147",
"022725673",
"029853087",
"021200426",
"019877010",
"024143096",
"040065132",
"027015215",
"029671636",
"014346591",
"016948225",
"005228087",
"020242513",
"003376350",
"020643369",
"009953750",
"000304817",
"017700283",
"009094355",
"010364011",
"008235838",
"009108047",
"006484245",
"005244691",
"017291261",
"006440747",
"017344854",
"004947426",
"003808501",
"040574994",
"000204848",
"022084645",
"028824388",
"010392368",
"099014833",
"027725156",
"028560103",
"001125320",
"022785666",
"024912292",
"026600981",
"020901462",
"003471771",
"020014824",
"025610756",
"020718757",
"004906671",
"013854186",
"006423642",
"029826116",
"005776049",
"040659652",
"022959206",
"019120843",
"016243879",
"009840624",
"017820866",
"019529243",
"001334674",
"028839392",
"006936609",
"006477806",
"018168050",
"024235533",
"006904261",
"002830736",
"007571652",
"002445835",
"009139931",
"005964489",
"003948062",
"029256845",
"007063857",
"016552491",
"000114815",
"025490959",
"028095154",
"004963307",
"007559213",
"024025849",
"023005250",
"026414313",
"017893709",
"017560379",
"017675969",
"024603322",
"017594269",
"010660567",
"005819173",
"009162871",
"028774199",
"001978391",
"020272328",
"018202530",
"017692392",
"025393409",
"009837493",
"041271699",
"005705578",
"004807418",
"013406940",
"013459741",
"006954948",
"006978175",
"014224615",
"024846660",
"021576866",
"019510633",
"024510627",
"007148506",
"003525188",
"001401576",
"017386134",
"001399708",
"005889820",
"007256913",
"003467381",
"002356537",
"001181461",
"001651309",
"018589653",
"006946388",
"007207854",
"006981209",
"021866594",
"006992080",
"028991958",
"019284119",
"003007829",
"001999014",
"026621010",
"013734612",
"005015308",
"003826113",
"006292799",
"001899620",
"005164221",
"028140531",
"013384890",
"013588335",
"014321724",
"013703918",
"005698339",
"007203091",
"006803092",
"013666453",
"007480339",
"001067051",
"005579181",
"010520196",
"001217721",
"006402311",
"006356155",
"024433021",
"007294276",
"007008710",
"006153376",
"000707080",
"006886496",
"010039641",
"025404617",
"009670587",
"005558653",
"006696250",
"002086073",
"004491494",
"002325253",
"021602235",
"015110947",
"007593006",
"009072883",
"010555491",
"007554150",
"006099173",
"003464626",
"007579731",
"000837606",
"023317751",
"000752573",
"021705605",
"010179834",
"020800596",
"010006172",
"029325819",
"006245658",
"010693110",
"013374628",
"013843542",
"028895097",
"005571098",
"005953732",
"009781020",
"007024781",
"014046374",
"005461014",
"014062492",
"013481584",
"007638432",
"027487914",
"000149962",
"002138149",
"001903489",
"007256684",
"001182175",
"002814959",
"021739322",
"001676297",
"007192452",
"026698144",
"006566067",
"028825030",
"026104674",
"027268707",
"024108610",
"021047363",
"021915233",
"014029213",
"014360686",
"010683454",
"013377510",
"009252783",
"007021873",
"009077239",
"008356476",
"007491947",
"007624358",
"007631362",
"006987905",
"006108681",
"005078327",
"005277964",
"005668326",
"005115486",
"005708943",
"005472512",
"004890934",
"003039552",
"003531249",
"004105676",
"004152318",
"003060447",
"004138346",
"003212351",
"004298864",
"004345801",
"000571011",
"000412570",
"001174866",
"020135866",
"020559096",
"020580933",
"020093033",
"010144655",
"004979429",
"020583269",
"007975968",
"040412612",
"000902567",
"006488473",
"027379721",
"026685424",
"025216179",
"026992925",
"025451251",
"025503508",
"026023136",
"024733542",
"024014886",
"024411911",
"024468373",
"023406906",
"020627167",
"021999134",
"020573464",
"018902976",
"018646145",
"019065041",
"000572423",
"001935140",
"018814694",
"015802087",
"013371172",
"029027590",
"029767814",
"028860514",
"027037947",
"024978336",
"023086281",
"023128245",
"020605733",
"019836738",
"018224018",
"017754280",
"019715928",
"015030088",
"014022064",
"013573508",
"007021253",
"007698433",
"022988700",
"017987505",
"037614482",
"007977904",
"010505580",
"000712191",
"019746327",
"007033733",
"005318490",
"013724392",
"004011410",
"010070315",
"026567563",
"010644055",
"027999264",
"026027133",
"004586987",
"002949265",
"000527100",
"026355082",
"019895200",
"021525655",
"002776844",
"024820304",
"022917284",
"028384896",
"024840479",
"005331377",
"007075041",
"023534438",
"020515249",
"005396434",
"004607339",
"022665564",
"024304967",
"027699927",
"009784979",
"007059388",
"014747329",
"005144194",
"021912671",
"005332093",
"018225342",
"019217138",
"004292738",
"001957938",
"000371908",
"015935329",
"024017094",
"005592953",
"004318276",
"023858128",
"005356732",
"015305438",
"027788743",
"004749564",
"003253108",
"004659824",
"016207097",
"025656566",
"008135655",
"029108321",
"018348146",
"007554162",
"029206748",
"003627575",
"014987994",
"005695762",
"029536578",
"025169145",
"019833683",
"028615268",
"026350364",
"023908093",
"005050021",
"002653923",
"026786739",
"023613587",
"010677973",
"002606225",
"004342924",
"025665712",
"014807286",
"016283673",
"001795957",
"028741703",
"024884721",
"006351437",
"026861551",
"006062221",
"003576882",
"004601315",
"015890393",
"016329587",
"023299312",
"028584824",
"014635030",
"007487919",
"006384038",
"019034625",
"014335941",
"008965663",
"004245862",
"016512028",
"005198606",
"022232565",
"016357817",
"000350528",
"000936677",
"005171424",
"001459490",
"025389420",
"004144033",
"029767455",
"024757183",
"001798109",
"003574214",
"020192511",
"020925361",
"022554582",
"002756911",
"022257061",
"007204963",
"007895146",
"004855606",
"003234081",
"017969406",
"010688574",
"004192114",
"006797502",
"040908896",
"005441674",
"013328162",
"024389146",
"010211875",
"027035612",
"028937096",
"026257448",
"009332964",
"013674841",
"026721947",
"028519592",
"023478998",
"019574512",
"027642136",
"005712921",
"017651437",
"023240039",
"019990727",
"019342988",
"013662559",
"009550794",
"005097117",
"028142621",
"022734890",
"013960066",
"020179220",
"015595575",
"026507890",
"006420213",
"025738654",
"007076884",
"024134301",
"004603080",
"006638373",
"016683801",
"022202138",
"005988265",
"006261706",
"009532665",
"015206964",
"020399529",
"010588743",
"002506745",
"025273899",
"005474760",
"024655410",
"000805313",
"023324780",
"013437420",
"020900126",
"014155343",
"028184838",
"028513832",
"040003801",
"025794199",
"024858377",
"025391481",
"023543037",
"023091745",
"023809733",
"022315064",
"022968271",
"020396022",
"021848805",
"017733620",
"016444502",
"014368948",
"010302100",
"010419430",
"009490256",
"013393705",
"008031599",
"008066642",
"008418033",
"008072070",
"008226065",
"006548676",
"006550004",
"006417204",
"004511872",
"005366787",
"004988522",
"005183542",
"003495673",
"001128707",
"001407035",
"002138938",
"003981575",
"025933821",
"007075542",
"020727114",
"014562490",
"017831401",
"010368094",
"010489967",
"018864094",
"029409049",
"004937652",
"017096419",
"024984691",
"026696979",
"016872260",
"005084845",
"016630354",
"006407842",
"015954729",
"027404782",
"026012131",
"024911318",
"022961995",
"023292995",
"020515650",
"021865405",
"018920423",
"016561171",
"028360243",
"028865904",
"018728727",
"005992794",
"022205162",
"025042296",
"018895434",
"021105599",
"029522729",
"003542543",
"026887778",
"004528867",
"013489188",
"003101487",
"013332816",
"024792377",
"015172673",
"009541297",
"019598084",
"000479737",
"006847234",
"004716557",
"018264215",
"000069361",
"010139823",
"015496216",
"005463772",
"027153134",
"000350105",
"029123003",
"016330173",
"005600100",
"017560018",
"006686174",
"022887183",
"003168742",
"000392206",
"006319997",
"025878849",
"025241645",
"020574349",
"007121768",
"003600527",
"014972321",
"003313562",
"009537133",
"007178950",
"000166103",
"021281796",
"005744392",
"027888339",
"022184226",
"009679894",
"009080346",
"022454692",
"007557603",
"004910749",
"009506442",
"005466299",
"006183347",
"007519059",
"028229080",
"005408513",
"019609005",
"001875121",
"027722817",
"025825068",
"018306355",
"023245366",
"014502360",
"004111113",
"004144249",
"016545524",
"026727298",
"028087574",
"015891271",
"020023347",
"005331490",
"000566571",
"005198008",
"008910816",
"023993282",
"004037497",
"014811392",
"002535366",
"005782313",
"015571996",
"003050959",
"025823936",
"029673452",
"024821980",
"009418184",
"000386439",
"019824228",
"026102482",
"021948447",
"018978023",
"001677501",
"006113797",
"009855246",
"024365976",
"017630161",
"025014212",
"014782700",
"006642772",
"005260887",
"001658549",
"004808334",
"027006025",
"027047494",
"013373703",
"003556980",
"026961619",
"023792605",
"019858750",
"017528732",
"005718823",
"017104746",
"003972501",
"023813299",
"018509211",
"027493400",
"010599517",
"016857269",
"019178357",
"018391273",
"014964051",
"006019712",
"005843146",
"003462279",
"025329710",
"006277901",
"024890706",
"029851412",
"019441657",
"024920815",
"007623625",
"003156233",
"005866707",
"029234904",
"026267206",
"001758872",
"009388227",
"008400736",
"002770833",
"004188629",
"016123088",
"005114091",
"002395180",
"041966165",
"026066466",
"016347848",
"005874530",
"019046639",
"017830130",
"024543599",
"025846829",
"005749487",
"018685511",
"006790059",
"009392217",
"016681231",
"017446888",
"029397325",
"023914180",
"023368581",
"017189966",
"017688413",
"014508934",
"014874366",
"016294218",
"013325566",
"006381318",
"004513530",
"004464038",
"002699857",
"003741232",
"001303558",
"020968196",
"022616592",
"041627900",
"023259449",
"024733894",
"020985441",
"027362717",
"020475539",
"024053299",
"007082065",
"002326721",
"006113043",
"025264021",
"005613013",
"000959145",
"024644642",
"007968690",
"013672870",
"026073871",
"017129841",
"000994430",
"020525322",
"002155707",
"017680386",
"027098091",
"009729887",
"014518629",
"017591879",
"014400107",
"004526208",
"010557332",
"016690300",
"004890860",
"009797036",
"026871236",
"026742407",
"009267551",
"006663465",
"006142464",
"006313288",
"015792824",
"021233121",
"009754320",
"016451084",
"017977797",
"009161313",
"006631212",
"001311610",
"016408099",
"002784818",
"025889243",
"006795129",
"026479085",
"007660163",
"017297717",
"003417826",
"002458840",
"026938058",
"028486058",
"009729617",
"009429007",
"024790235",
"021636620",
"007049533",
"006292825",
"029740233",
"026446356",
"005667560",
"006290302",
"010551548",
"018676229",
"001670714",
"002765062",
"028093254",
"004458571",
"001991631",
"025217152",
"005216492",
"005423579",
"009945926",
"023024934",
"021034740",
"019506809",
"014639882",
"015334381",
"013737129",
"009145151",
"028582598",
"015767229",
"001102383",
"004204601",
"005591546",
"022735962",
"014330256",
"002174140",
"002707580",
"003128179",
"027636774",
"002969669",
"009724890",
"004633831",
"024170837",
"006101752",
"014235408",
"027015282",
"009222300",
"020385992",
"019711875",
"013826844",
"013484669",
"017564147",
"003233503",
"004780377",
"027086112",
"025548778",
"003584983",
"006222344",
"004146021",
"013387809",
"007057331",
"006418974",
"002790982",
"029276305",
"020541019",
"024324838",
"009297739",
"016549252",
"013942587",
"008971896",
"020627101",
"019731280",
"015852479",
"009702050",
"007129825",
"003602794",
"001430271",
"001109366",
"027423414",
"009355689",
"009958455",
"028470122",
"020424714",
"029161645",
"019903191",
"000188318",
"013887697",
"018145394",
"002380032",
"027525175",
"040338498",
"004522181",
"006475727",
"013704826",
"003604971",
"005997628",
"000314640",
"003381783",
"004420471",
"004413880",
"006895814",
"028407334",
"022071891",
"001757476",
"007055726",
"018975055",
"015555134",
"004822599",
"026673103",
"021051524",
"013412417",
"008911687",
"006821993",
"000448038",
"001886364",
"014769176",
"003274585",
"017472603",
"017308940",
"021516657",
"027907656",
"007597550",
"016835576",
"006850411",
"005037997",
"000445164",
"001088460",
"006802472",
"005309474",
"022196771",
"009558829",
"009919178",
"001067801",
"027588668",
"023519599",
"022367514",
"007667833",
"007611602",
"002785473",
"025253056",
"020246855",
"021872849",
"004451801",
"000275258",
"013562526",
"021816633",
"002230211",
"007636876",
"002035567",
"001580489",
"001488802",
"020764934",
"021683208",
"025009037",
"019291284",
"040036431",
"014400921",
"014794519",
"019245956",
"009137465",
"006746217",
"007112464",
"029960443",
"007553063",
"013954219",
"025465046",
"014406308",
"009571828",
"022744056",
"020793672",
"022730955",
"015865336",
"017268958",
"003434124",
"003451413",
"000178318",
"005903294",
"001680943",
"001214686",
"001833849",
"014081447",
"009834206",
"018347244",
"018379169",
"005997292",
"026683829",
"016922927",
"025658610",
"024213447",
"008872559",
"006067671",
"001294935",
"025472974",
"006735734",
"013482678",
"022604594",
"029744416",
"000837128",
"022665958",
"007767739",
"005713599",
"009241843",
"006527217",
"022562725",
"000423577",
"003292416",
"024785384",
"014499412",
"004812247",
"006530471",
"000546523",
"005380027",
"027292489",
"013566778",
"007291970",
"008161981",
"022668490",
"028135289",
"003306711",
"014041578",
"004598076",
"019607343",
"005932515",
"003371025",
"009798452",
"005386384",
"008534996",
"025454213",
"022579946",
"024552019",
"018534605",
"005901842",
"020132787",
"002585795",
"009308714",
"029471573",
"022259487",
"019460139",
"024771322",
"041576018",
"003255669",
"021717564",
"004130841",
"007187724",
"025349942",
"005315864",
"004785175",
"014062893",
"001895894",
"006615021",
"005466882",
"006334180",
"018488985",
"010274772",
"009522310",
"004938136",
"003852659",
"007092348",
"005140535",
"001893488",
"001526614",
"003417888",
"009467314",
"028224655",
"009107093",
"004004725",
"009357174",
"024487356",
"005707531",
"004533907",
"020641788",
"028916928",
"027474839",
"028793412",
"027787744",
"026665596",
"022652010",
"022070533",
"018316373",
"014125275",
"014722497",
"014007776",
"009682962",
"013419468",
"009566582",
"009153501",
"008452067",
"007206088",
"006370049",
"006733500",
"006242042",
"005354804",
"004154704",
"002885773",
"002483076",
"004166080",
"002241281",
"002308563",
"000729621",
"000809650",
"001214388",
"000995013",
"000089210",
"016149652",
"002137974",
"018242270",
"019362461",
"010389782",
"020552399",
"014783066",
"007630794",
"015607536",
"009292284",
"023382019",
"005836400",
"002486838",
"006670986",
"009629959",
"022637427",
"004910262",
"007045990",
"013825831",
"016023658",
"008705831",
"004708566",
"025843146",
"009371801",
"020630316",
"005769296",
"023590594",
"025261619",
"026771830",
"026428966",
"020002772",
"021455575",
"006346759",
"009598861",
"003183031",
"007380587",
"020742039",
"027906089",
"027594861",
"025718812",
"026981711",
"024596890",
"021953695",
"019955063",
"020361696",
"020412929",
"018252437",
"015496488",
"015893321",
"009379914",
"007185505",
"026382065",
"021021705",
"027702940",
"000399226",
"001825056",
"001096249",
"005246091",
"009083278",
"025869739",
"008937947",
"022282102",
"017605781",
"016442865",
"016497496",
"004934782",
"015867959",
"025132635",
"020519877",
"019686261",
"009732516",
"006046715",
"006337169",
"014497072",
"007205999",
"014638973",
"027976957",
"016514531",
"026999239",
"023089157",
"004942449",
"005963584",
"003076125",
"002448413",
"003693146",
"018423670",
"015256998",
"013702781",
"021651967",
"026030736",
"013738927",
"009560861",
"004034612",
"000740448",
"022409463",
"002451382",
"004982653",
"006062241",
"001063368",
"014675048",
"000982405",
"021526095",
"000865818",
"000163236",
"021586797",
"041854076",
"005886556",
"024594088",
"022197844",
"018968066",
"010533532",
"004686148",
"005587765",
"004178096",
"002099386",
"021485260",
"007257427",
"018635352",
"025652999",
"013981440",
"015533536",
"020478874",
"005492090",
"009670677",
"028044913",
"017950509",
"004947758",
"022441627",
"008141074",
"027481336",
"040668213",
"016204820",
"009335290",
"005548763",
"001315113",
"017182275",
"002760865",
"027311762",
"009407257",
"004653996",
"004047904",
"022841639",
"022190725",
"018664988",
"007181901",
"018621515",
"016359832",
"019258734",
"027024710",
"000469668",
"006099252",
"005766431",
"021896497",
"010335156",
"027358281",
"001179308",
"006847432",
"018481217",
"010215796",
"019557688",
"021476904",
"001488630",
"024573223",
"005133476",
"013704498",
"019586678",
"030955288",
"026116051",
"018121749",
"004133767",
"022087458",
"000781008",
"027422671",
"019295055",
"021786946",
"005756992",
"029778378",
"023354346",
"007042450",
"023112618",
"004569676",
"002181955",
"005236757",
"006898096",
"006819133",
"001486039",
"026356047",
"002947363",
"009945659",
"013445179",
"026850324",
"014843143",
"018118071",
"023728537",
"018651552",
"029040983",
"023170375",
"022954940",
"010455716",
"029302023",
"014605433",
"008061580",
"005149131",
"010225932",
"021666105",
"041304049",
"013568380",
"022196758",
"029124895",
"025906416",
"026102590",
"022293542",
"022549617",
"018101027",
"014640302",
"009173923",
"007459210",
"005580991",
"004607194",
"002958002",
"004253010",
"002645918",
"002600393",
"023193851",
"019914745",
"007061124",
"002697038",
"020020431",
"004913738",
"007519081",
"004330248",
"016048046",
"003682341",
"027003636",
"006921860",
"039939186",
"017990114",
"005548994",
"002506577",
"005898234",
"029298982",
"009282998",
"041669609",
"003324483",
"029325799",
"009063188",
"018504215",
"020051896",
"016279327",
"020333793",
"026131089",
"027697735",
"017372706",
"008219528",
"006133976",
"005667657",
"006783447",
"025956155",
"002007671",
"028820805",
"013944707",
"003167109",
"041517271",
"001225599",
"014581041",
"013379424",
"027266028",
"022761641",
"017651942",
"006465410",
"002910406",
"002553598",
"014586850",
"001779685",
"013916010",
"009541025",
"009484649",
"017120223",
"024857432",
"023661413",
"015625733",
"009283102",
"009524689",
"008314104",
"006487429",
"006103170",
"000065014",
"001340454",
"003549268",
"003999768",
"016145457",
"027457373",
"025728127",
"020448507",
"007097232",
"006393852",
"001795724",
"003897837",
"009315695",
"006998373",
"003033013",
"024648058",
"020629088",
"001215994",
"029177566",
"041744478",
"003410326",
"020231416",
"022960673",
"003304764",
"014781561",
"021535210",
"006225470",
"000678803",
"028118192",
"021174920",
"009383752",
"005080208",
"004669341",
"003847947",
"003901081",
"001516695",
"001339169",
"001947802",
"003109386",
"029928891",
"006177018",
"018199061",
"006455632",
"001076854",
"025198854",
"000096093",
"028164553",
"006062081",
"003579770",
"024008988",
"010389251",
"002089939",
"023749330",
"014228899",
"010274044",
"009123016",
"000913406",
"003428276",
"041303654",
"002197515",
"018275665",
"015235706",
"021511333",
"007913122",
"023705666",
"003206316",
"017983897",
"029081234",
"021877703",
"029155725",
"024771906",
"023989762",
"022836056",
"021220909",
"020414373",
"019749669",
"017100849",
"015807993",
"014582904",
"016141320",
"014787620",
"009513886",
"007030926",
"003555165",
"004056903",
"006735798",
"027707850",
"021212284",
"025249608",
"009718378",
"006541773",
"040010392",
"028195940",
"022987281",
"004130412",
"000989617",
"041728309",
"019143178",
"016452662",
"021405210",
"019400111",
"028810444",
"006145440",
"040087833",
"003121201",
"002817238",
"007696093",
"028394743",
"004884679",
"020475033",
"014938498",
"004611753",
"005220622",
"002729446",
"025026226",
"027060532",
"025568412",
"022570142",
"021367852",
"017922480",
"015718077",
"009647677",
"006937323",
"006581871",
"006494475",
"003051299",
"000062629",
"021408234",
"005489471",
"004742287",
"005215695",
"004242739",
"022291834",
"027314304",
"020771079",
"023175974",
"023090071",
"000726835",
"017123808",
"001826147",
"018367027",
"004448918",
"019037567",
"023710211",
"003582191",
"005252975",
"005190423",
"006563008",
"028246010",
"025728396",
"006585203",
"004066170",
"027781318",
"023555644",
"004503331",
"002540348",
"007085096",
"005931711",
"024358838",
"020730507",
"018230307",
"015230759",
"006523274",
"009673493",
"001450351",
"013967131",
"023100964",
"002466023",
"000567451",
"026778761",
"019378167",
"022727807",
"019769435",
"000689853",
"000383077",
"027396510",
"019571774",
"026563409",
"009229362",
"026425739",
"020481424",
"006178606",
"029069562",
"026981499",
"019317698",
"003476500",
"006030152",
"018562019",
"041484056",
"019999233",
"002044683",
"021810538",
"020999775",
"017242199",
"005992409",
"003294800",
"000865986",
"004978989",
"015716779",
"014022073",
"000851367",
"006813283",
"020718284",
"021592906",
"005801171",
"007498839",
"006100706",
"022990443",
"004711654",
"009760617",
"021152588",
"004904446",
"026217195",
"014653268",
"040954628",
"022476379",
"004973781",
"007212926",
"004874180",
"015596873",
"015745742",
"027910350",
"022170618",
"005555388",
"024735806",
"020909792",
"026551152",
"013962201",
"041854810",
"028905034",
"028488399",
"029487037",
"039990386",
"027305441",
"022882318",
"019514784",
"017936978",
"014505358",
"014864629",
"015209905",
"015720273",
"014404945",
"015676136",
"009509522",
"009410054",
"009704432",
"008705921",
"006583918",
"006049221",
"004001190",
"003856034",
"002031273",
"001931293",
"001933937",
"019348842",
"022047642",
"007745989",
"023210692",
"026402655",
"009547384",
"022510242",
"027196841",
"020397517",
"013986030",
"006011907",
"003284086",
"000436649",
"001007104",
"015914342",
"006894310",
"024281561",
"003403943",
"005735933",
"004718299",
"005424132",
"006385970",
"025783833",
"026933399",
"020130967",
"021937978",
"014334742",
"010501270",
"009068826",
"005722925",
"004338372",
"018532251",
"022774675",
"005300683",
"021888850",
"020814524",
"019326788",
"019508455",
"039918363",
"024811537",
"010001909",
"020551760",
"003663359",
"016713281",
"019778030",
"026944836",
"020728767",
"014006631",
"007195515",
"004460366",
"001428547",
"015112634",
"008969602",
"005871514",
"007441412",
"007556243",
"001320069",
"006509076",
"026370097",
"014771895",
"004128757",
"040372233",
"021482542",
"028273229",
"026914497",
"014981560",
"014053860",
"008356801",
"005531077",
"003911324",
"003306662",
"001068061",
"001759902",
"020144157",
"005758031",
"015696506",
"010374182",
"004310057",
"003562755",
"008009141",
"006557666",
"028300852",
"017560760",
"007021422",
"028854018",
"014814511",
"005959416",
"001349846",
"040073525",
"010636493",
"025984400",
"026121856",
"021566957",
"021979642",
"018026826",
"004998165",
"004910688",
"016324872",
"019342700",
"013848140",
"016391209",
"016935442",
"024862739",
"021266286",
"022682105",
"005298481",
"000419133",
"003756827",
"025298232",
"039922942",
"007019157",
"026887093",
"024918109",
"025209271",
"021442386",
"013883167",
"014610411",
"006673661",
"004606865",
"001733300",
"022219260",
"003723754",
"007116115",
"018145743",
"000742205",
"017988265",
"010300492",
"015310663",
"003070149",
"000656355",
"021930478",
"003627650",
"028570474",
"019644270",
"006642064",
"006265979",
"025779729",
"024389592",
"010109674",
"008207276",
"007558165",
"005609012",
"016862374",
"006674417",
"008416208",
"000228109",
"009690543",
"007226202",
"027589748",
"006260248",
"022966037",
"019857339",
"013829791",
"025246275",
"022746031",
"020544367",
"007945633",
"008205441",
"006313707",
"006839959",
"006067509",
"005052776",
"005345380",
"013576102",
"018187675",
"005924990",
"029994853",
"004398713",
"005249034",
"006167644",
"022559816",
"003302603",
"025137060",
"005384516",
"004563303",
"025708935",
"039942031",
"014662982",
"022794473",
"004751571",
"027530612",
"028296408",
"027854061",
"029682426",
"024309445",
"023272044",
"022304955",
"022062259",
"021643141",
"019659827",
"017456045",
"014396375",
"014057309",
"013928191",
"014798720",
"014581498",
"014040863",
"010233038",
"013674812",
"009505622",
"009376691",
"013628476",
"007063770",
"008114882",
"007726487",
"006995798",
"006326774",
"006820445",
"006680462",
"006171096",
"006363748",
"005891861",
"004922474",
"005006930",
"004084100",
"002854670",
"003095407",
"001655065",
"001440668",
"000061767",
"000691960",
"001063844",
"001625983",
"024671328",
"006488092",
"040202435",
"001462012",
"028010805",
"009164072",
"018236804",
"018421365",
"004065359",
"026551284",
"004822409",
"006266916",
"001951609",
"019175934",
"039941223",
"026566078",
"028769691",
"019076352",
"009580607",
"022527940",
"021584891",
"010035797",
"008260243",
"008936598",
"001216137",
"024165306",
"005843007",
"016963110",
"027303226",
"028949713",
"017218748",
"019625526",
"024515332",
"000606243",
"005636917",
"020715866",
"028397213",
"004365825",
"020361645",
"013655833",
"020354146",
"019437300",
"014619497",
"014356183",
"014966959",
"009257961",
"004373475",
"003495066",
"000689872",
"018455987",
"022331314",
"005562055",
"015228161",
"014407368",
"006529446",
"023434733",
"003497263",
"000919062",
"002204064",
"040264202",
"024537963",
"042670920",
"021130343",
"015792164",
"009261105",
"006917923",
"016149430",
"019165893",
"014385873",
"003372506",
"007561675",
"001228083",
"005981038",
"009384013",
"005024797",
"007062615",
"000894648",
"005859231",
"014736887",
"018756640",
"023885341",
"028328620",
"003565954",
"029282420",
"021443599",
"020225352",
"021627212",
"019800780",
"010425368",
"007756883",
"005558993",
"002840449",
"006094224",
"027085397",
"009235263",
"023391986",
"006319527",
"025411488",
"021707820",
"009179944",
"000516931",
"002501605",
"003314692",
"021278969",
"021161889",
"019621513",
"016324016",
"014770189",
"005811091",
"005852723",
"000766299",
"026163297",
"017166782",
"026866008",
"007072973",
"021353591",
"013792928",
"026837583",
"007252122",
"023710061",
"020290378",
"023927935",
"024554501",
"007620616",
"000878577",
"017156283",
"006005729",
"001149593",
"001946368",
"004163473",
"006801441",
"020656718",
"027514098",
"006558951",
"026845629",
"000187104",
"001498491",
"007181015",
"027942573",
"021190531",
"026818250",
"009282794",
"007142359",
"009322631",
"006833429",
"004726876",
"000542573",
"000842720",
"020605491",
"020029268",
"006152483",
"021689576",
"014080186",
"008566165",
"007164875",
"006955659",
"006996807",
"006550980",
"021774304",
"013393742",
"022996535",
"028212322",
"018356362",
"003004493",
"028510121",
"027408087",
"000894333",
"006271052",
"007101542",
"021915858",
"020929588",
"041884790",
"041820679",
"027095996",
"028105946",
"023393771",
"028449817",
"024901046",
"024516739",
"022745611",
"016178717",
"014163039",
"015247955",
"013621819",
"007489733",
"006027254",
"006849988",
"005443048",
"002831938",
"004111873",
"000989108",
"001398224",
"000709715",
"002054599",
"006310063",
"000896178",
"009493386",
"005293792",
"021874888",
"005100495",
"020632637",
"026409563",
"005617001",
"007400254",
"004773536",
"027075287",
"006821447",
"013396185",
"000972230",
"028715602",
"001026202",
"021555906",
"019916353",
"014047047",
"013720478",
"006823083",
"001057494",
"029493846",
"022266697",
"024380963",
"026987068",
"040123354",
"004078896",
"024978453",
"022454413",
"013732820",
"006607372",
"013454439",
"001649199",
"026362973",
"019989509",
"026699439",
"004795875",
"026387680",
"007415470",
"006316051",
"006373333",
"019074730",
"009191761",
"000679888",
"019134048",
"028103155",
"027464233",
"028411288",
"027582213",
"029197044",
"027720041",
"040004499",
"029139577",
"028628064",
"026290614",
"026079795",
"025918290",
"025612190",
"022477631",
"024102826",
"022579590",
"020031519",
"021298898",
"020606999",
"021086401",
"018048654",
"019600977",
"019207513",
"019805593",
"016663243",
"013676720",
"013581946",
"010065427",
"009288401",
"007828695",
"007142468",
"007580049",
"007265479",
"008922910",
"006282300",
"005953273",
"004792711",
"004934536",
"004590720",
"002329103",
"002668845",
"002923578",
"004357007",
"001190329",
"000168679",
"001209548",
"000823747",
"001259125",
"007163995",
"024482942",
"013930647",
"002434316",
"001566526",
"007593221",
"006596012",
"019163420",
"010583276",
"006446946",
"022270425",
"026227217",
"024898494",
"010452524",
"027397845",
"027429797",
"021371220",
"021144289",
"020647551",
"020691826",
"017304654",
"015180477",
"006410719",
"021018335",
"020953798",
"001131080",
"023496554",
"002513110",
"022548911",
"026850975",
"001513173",
"006647811",
"015756872",
"007190489",
"019013793",
"024395699",
"025523852",
"007001338",
"004107480",
"023687304",
"027431944",
"005773348",
"001515099",
"017791474",
"000523661",
"010497771",
"024213396",
"027100607",
"027564766",
"025254927",
"020322232",
"019725162",
"007044935",
"004061490",
"001965310",
"010002152",
"014540914",
"007182791",
"014440234",
"008940666",
"007273869",
"005312011",
"001200190",
"000022544",
"005279550",
"019799515",
"025456399",
"015892292",
"009120911",
"000162656",
"028295929",
"015979388",
"007518541",
"001095455",
"000769260",
"024078714",
"007845350",
"024834841",
"006926965",
"009933143",
"014032344",
"025725118",
"013725352",
"019087672",
"007850868",
"019469589",
"005552949",
"005261322",
"025640396",
"006550466",
"010240765",
"005618840",
"039901551",
"008452127",
"006415110",
"025078943",
"020685463",
"019400075",
"023415697",
"003535861",
"002844685",
"018477000",
"015057835",
"008293619",
"003491527",
"003324795",
"024738104",
"028179948",
"024849814",
"007265453",
"024337970",
"017414435",
"003238262",
"029355496",
"025964962",
"017409419",
"018514128",
"014632188",
"010423757",
"007636884",
"006513508",
"006684673",
"006072270",
"005822117",
"000794368",
"000678508",
"019934742",
"002419195",
"006440947",
"028584487",
"004818770",
"018045729",
"006483220",
"006350061",
"015863155",
"009535594",
"028005875",
"027544549",
"029706669",
"026489313",
"025030675",
"027245334",
"026292959",
"025639639",
"021600060",
"020913657",
"017645024",
"016783977",
"014015646",
"014483637",
"009397917",
"010497165",
"006451369",
"006548270",
"006245533",
"006910432",
"005842456",
"003719677",
"002467857",
"003321770",
"003782556",
"002713157",
"002082879",
"006099015",
"025736028",
"023900763",
"001444913",
"023156674",
"006278049",
"009533346",
"018245885",
"025913959",
"017136823",
"027523249",
"017295626",
"005493497",
"001937288",
"006457407",
"013545714",
"027226427",
"016180766",
"003258586",
"000129315",
"003525421",
"025986363",
"020857457",
"029531198",
"021732426",
"027727982",
"028387694",
"026152754",
"023550088",
"013955827",
"015548848",
"009327932",
"009475807",
"010112250",
"008987891",
"008117443",
"008319846",
"006192394",
"005319636",
"005680026",
"003166089",
"010018222",
"027993297",
"021978494",
"028877308",
"025266642",
"007184615",
"025716426",
"023721492",
"016610773",
"000662036",
"000642203",
"028065519",
"026770585",
"021895827",
"018204146",
"009283215",
"006642560",
"006337355",
"004004182",
"029641766",
"029847797",
"026944260",
"024869747",
"025423498",
"020731644",
"019164355",
"017148233",
"016355524",
"006236055",
"006226742",
"006289507",
"003809309",
"003367110",
"000648349",
"000430177",
"004873613",
"027769194",
"025156437",
"025088538",
"022714164",
"020771432",
"020042578",
"017869852",
"019260411",
"019231975",
"016369772",
"007248318",
"006972938",
"006889946",
"006978245",
"002787968",
"001261643",
"014491271",
"023872316",
"022889786",
"025765534",
"009378774",
"001429245",
"019313181",
"023042626",
"024395041",
"019256976",
"022130633",
"022360751",
"018907335",
"026298090",
"024515898",
"004205885",
"021232635",
"001022330",
"020952419",
"003769428",
"014490581",
"024279112",
"000856501",
"021888819",
"003712504",
"029754228",
"025719884",
"022205157",
"020369029",
"017687442",
"014483563",
"013740966",
"010326286",
"009447595",
"009101079",
"003270116",
"003472098",
"013945110",
"005816845",
"013567490",
"004768465",
"000820099",
"026716333",
"023059887",
"022624145",
"021796330",
"019532517",
"009592959",
"004653212",
"000146928",
"018751206",
"006872399",
"023108565",
"013767380",
"024071055",
"007011638",
"009782830",
"007987444",
"022228513",
"002448594",
"021054363",
"006620483",
"013492334",
"016917737",
"000800946",
"014025229",
"005593796",
"005312847",
"028689441",
"007179280",
"014858162",
"026523918",
"029549360",
"029576762",
"025289056",
"022268163",
"019253978",
"014626706",
"007733044",
"008561843",
"006755788",
"005505630",
"003753931",
"009176561",
"005357016",
"020005953",
"016111207",
"024149193",
"024857736",
"039883291",
"022937118",
"007129748",
"015544606",
"010553424",
"005572361",
"004530658",
"003213881",
"026127755",
"013859098",
"006229349",
"000940627",
"018244829",
"006210532",
"025986420",
"007063992",
"003168759",
"003705498",
"024977169",
"018588664",
"028035677",
"027431999",
"028408033",
"028256353",
"029807504",
"041944130",
"026381257",
"025394916",
"026970536",
"025945162",
"022617914",
"022982705",
"020895940",
"020722884",
"021890357",
"018217983",
"017479646",
"018255018",
"016724092",
"014411410",
"013768162",
"013746881",
"016544023",
"010454257",
"013341899",
"010053355",
"007912123",
"007502477",
"006748609",
"004896237",
"005871958",
"005794737",
"004799101",
"003647838",
"004133665",
"001146917",
"001553056",
"001076203",
"001185699",
"000541992",
"000878917",
"001834929",
"019783480",
"008972655",
"029803536",
"001490322",
"001106699",
"001052130",
"014235399",
"009128693",
"021463420",
"028530522",
"016436208",
"018628726",
"000779836",
"004265316",
"028462251",
"004534318",
"006905887",
"010525129",
"017420561",
"006932300",
"018938170",
"025545515",
"010430284",
"006460441",
"014175696",
"005201018",
"027630119",
"027693251",
"029036257",
"027906979",
"027109178",
"022633720",
"021909862",
"018250682",
"017870548",
"003747352",
"000968098",
"014202909",
"005256965",
"020124961",
"010227460",
"010321905",
"026077520",
"004781177",
"000736567",
"003953665",
"005777344",
"003067155",
"002842865",
"007978340",
"020685658",
"007143159",
"005803873",
"013465998",
"028586072",
"009855954",
"014239210",
"015613220",
"000767562",
"004958269",
"021679498",
"005660970",
"020498832",
"028624112",
"022649920",
"020644611",
"021730735",
"019406556",
"015091122",
"014967256",
"016149964",
"009337856",
"010011708",
"007109685",
"007051013",
"007219810",
"007259941",
"008010602",
"006605874",
"006125313",
"006512459",
"005763239",
"004982533",
"005288496",
"002190080",
"000981945",
"006553623",
"024394186",
"015280068",
"006648110",
"025094656",
"021749132",
"013545271",
"003964473",
"028606175",
"009605200",
"001054629",
"014146842",
"000466960",
"019838052",
"019391148",
"000269511",
"020975221",
"006329418",
"013691710",
"026287628",
"020631730",
"019110756",
"019793623",
"022387707",
"018214316",
"025317841",
"006733188",
"014235476",
"029385983",
"022170163",
"020526181",
"020541708",
"015581523",
"013986336",
"015612503",
"016351757",
"010545000",
"007942254",
"006339385",
"004715464",
"002200752",
"002552602",
"003418425",
"000899449",
"006631983",
"003362291",
"013700193",
"023709758",
"021734367",
"004241780",
"006457594",
"025264984",
"024822001",
"025338363",
"018616721",
"014358131",
"014456084",
"014055898",
"009083345",
"007281789",
"006864618",
"001731006",
"004947610",
"000698881",
"014514700",
"003144540",
"006432529",
"009488399",
"003793665",
"019553528",
"022995418",
"029241011",
"003475102",
"028197235",
"029797492",
"042024794",
"025070265",
"023513775",
"022362069",
"017536418",
"017143278",
"015392923",
"013727282",
"013470924",
"007085299",
"005249337",
"003261216",
"000444009",
"016320048",
"022264227",
"014122258",
"006565479",
"022894786",
"015863730",
"005760475",
"015670161",
"006476816",
"005282822",
"027193652",
"006040268",
"013707556",
"019703943",
"019056129",
"009620821",
"023560102",
"020586484",
"022413748",
"013486888",
"009437328",
"005207839",
"006802194",
"025343184",
"006050550",
"023815387",
"014101258",
"022340845",
"008334988",
"040930318",
"029663551",
"029729725",
"025731422",
"019982359",
"019495198",
"019517524",
"019253974",
"016929985",
"016026801",
"015256360",
"016492823",
"009777233",
"009897362",
"013637089",
"007053396",
"006532914",
"006931026",
"006302885",
"004795075",
"003768835",
"003086211",
"000765479",
"019908058",
"020998407",
"008531239",
"006200011",
"023002059",
"015336151",
"004436776",
"023059745",
"028082876",
"013934649",
"027555687",
"019481923",
"010500989",
"019630333",
"003228547",
"009932588",
"002875792",
"000930786",
"007264618",
"006232401",
"026732052",
"007093547",
"020558956",
"005475268",
"023806218",
"002640732",
"027164136",
"009143817",
"027405692",
"026255532",
"020558001",
"021951341",
"013740141",
"009698438",
"013620041",
"006606951",
"006969032",
"005424911",
"005315801",
"002464578",
"002940704",
"003035107",
"001959192",
"027222858",
"001310142",
"009596894",
"028546729",
"000121298",
"020861409",
"001748853",
"023555183",
"013692694",
"026134266",
"003606615",
"025221594",
"020566706",
"015419357",
"008359240",
"020575383",
"029210662",
"009541031",
"004691406",
"024029154",
"021703235",
"017937041",
"018442483",
"008693105",
"005056315",
"003575640",
"026599460",
"002854076",
"018728844",
"006670866",
"007645958",
"040088932",
"028502732",
"026393870",
"022961857",
"020700406",
"017388079",
"016923787",
"016291018",
"014776411",
"013322555",
"010670835",
"007245481",
"007006413",
"004525415",
"005665078",
"002159770",
"001502653",
"023600443",
"001291068",
"000496226",
"018657224",
"006198723",
"026852080",
"027642828",
"018390182",
"028158596",
"016924108",
"003439652",
"025621001",
"016947526",
"000634163",
"027383739",
"015608236",
"027830507",
"020999609",
"008882628",
"006232432",
"006526491",
"004141581",
"020727388",
"004867252",
"020555396",
"039972605",
"014626571",
"017408906",
"020781563",
"001675360",
"018155154",
"028071048",
"029205554",
"028315659",
"026586918",
"024750639",
"023119308",
"020642070",
"020787080",
"022147479",
"020606001",
"020978895",
"019222578",
"015556649",
"014200659",
"009354456",
"009510588",
"006088826",
"006310476",
"006971287",
"005268350",
"005712773",
"002430715",
"001290081",
"001446805",
"016171767",
"008029670",
"021984657",
"026526143",
"021465596",
"015607888",
"016877706",
"018335029",
"040412526",
"026975557",
"028643380",
"014747319",
"005766986",
"003481809",
"020786356",
"006576065",
"022197409",
"007020699",
"020512234",
"002798892",
"002861786",
"018282416",
"001656912",
"007185375",
"003131486",
"001740356",
"000232254",
"028035127",
"004905470",
"027980611",
"029818517",
"026933388",
"020917732",
"021033100",
"021313102",
"019896938",
"020727023",
"016904623",
"013826063",
"013990927",
"010498118",
"007061897",
"008220102",
"006430804",
"006257447",
"006459744",
"006286557",
"006128919",
"005985092",
"005475495",
"005638484",
"005660682",
"005147580",
"005052761",
"005094280",
"003168067",
"001016932",
"001339503",
"023512922",
"019803583",
"020206802",
"010115161",
"000599651",
"014614446",
"013759506",
"005218338",
"005078109",
"029642582",
"026748414",
"009658571",
"000069264",
"001031955",
"023331382",
"006801852",
"020783804",
"024121922",
"016158326",
"002598173",
"021141049",
"000996836",
"005958783",
"015099895",
"018774223",
"008748707",
"002800750",
"025997269",
"023054715",
"003262221",
"022348763",
"004880127",
"014124030",
"029110099",
"023239187",
"026536122",
"008877739",
"008888057",
"022960057",
"020272984",
"006601601",
"001102782",
"023289476",
"000293237",
"029269765",
"041012271",
"028681057",
"029907331",
"028921378",
"029293549",
"029338322",
"041363586",
"027850385",
"028760050",
"028228371",
"039233548",
"029348422",
"028796232",
"029080112",
"027843314",
"041066774",
"041525644",
"037608060",
"029487866",
"039329961",
"025170162",
"026510929",
"025291304",
"026635104",
"025029018",
"026638392",
"026586906",
"026677665",
"025888342",
"026508414",
"027004162",
"027022257",
"026128251",
"026192492",
"024895239",
"025786576",
"027295739",
"027253633",
"025112118",
"023847721",
"024497291",
"023876828",
"022288228",
"022781688",
"024059089",
"023738300",
"024761537",
"024440556",
"022885375",
"022955596",
"022634736",
"020865455",
"021291945",
"020754259",
"020888752",
"020303579",
"020156541",
"020635937",
"021717888",
"020679500",
"020745098",
"022122167",
"017725977",
"017351389",
"016832343",
"016835542",
"019194453",
"017885201",
"018959648",
"017251768",
"018888437",
"017383908",
"019385772",
"017366788",
"018993830",
"019099804",
"018558601",
"017262851",
"015182323",
"015886988",
"015858188",
"014353145",
"016036286",
"013938885",
"016107815",
"014669225",
"016149903",
"013741890",
"015051227",
"013569273",
"009438897",
"010652220",
"010534858",
"013732879",
"009702321",
"013379132",
"009773824",
"009300619",
"010392705",
"013321801",
"013482853",
"010180513",
"007397649",
"009148764",
"007106186",
"007118520",
"007039310",
"009145198",
"007447886",
"007273275",
"008568306",
"009100728",
"007919534",
"007119932",
"009232634",
"007642080",
"008858265",
"009146315",
"007931691",
"007185058",
"007659229",
"009222230",
"007975068",
"005993875",
"006644861",
"006749238",
"006554325",
"006267138",
"006368926",
"005910133",
"006754047",
"006110009",
"005934699",
"006991307",
"006009030",
"006098011",
"006562096",
"006112734",
"006347003",
"006667041",
"005645995",
"005372883",
"004604578",
"004632093",
"005192482",
"005866471",
"005351923",
"005754032",
"005200080",
"005204172",
"005731139",
"005387869",
"005106325",
"004975677",
"004692337",
"005709332",
"005086154",
"003237622",
"003611230",
"003576836",
"002727719",
"003370965",
"003179592",
"002738867",
"002633060",
"002800317",
"002410100",
"003672971",
"003977964",
"002876433",
"003981571",
"003218521",
"004024814",
"003215702",
"000504620",
"001070031",
"000851898",
"001214972",
"002048662",
"000630198",
"000094509",
"000161451",
"001171895",
"001164645",
"001741052",
"000855339",
"000544838",
"000817484",
"000640167",
"025836235",
"018223005",
"007287085",
"020028762",
"024357739",
"001810050",
"019955721",
"001002568",
"007065571",
"022153158",
"027161783",
"003565413",
"029350578",
"007648097",
"003447149",
"027081854",
"006642477",
"008223367",
"023122174",
"014452038",
"025585863",
"028940860",
"013732030",
"016731018",
"041403025",
"023461908",
"005182544",
"026125497",
"020727710",
"013744539",
"005399086",
"005974323",
"027638507",
"009547972",
"015085237",
"003531994",
"005662878",
"006012355",
"018749637",
"007514929",
"004630790",
"001502967",
"021687395",
"025372758",
"000283577",
"023979736",
"022927686",
"027148909",
"007636911",
"020143075",
"003271241",
"022328705",
"009251227",
"020419144",
"026122728",
"003499422",
"001040515",
"007107041",
"022272585",
"022629345",
"025431485",
"002577592",
"023947959",
"013679502",
"006756402",
"013563038",
"007580215",
"007028958",
"026521065",
"002427960",
"006735744",
"008887430",
"020518470",
"024643960",
"029734424",
"029311980",
"029273053",
"029231658",
"040139785",
"028775558",
"029776931",
"029962332",
"029163783",
"028299067",
"025772213",
"026222459",
"026089929",
"025545215",
"026957453",
"026280508",
"022538739",
"024792798",
"024286562",
"023942093",
"024807247",
"023280362",
"021486171",
"021588253",
"020075281",
"021085310",
"022157253",
"020612965",
"021249058",
"018967485",
"017106512",
"018045268",
"017345930",
"019068503",
"013863910",
"014130062",
"013738745",
"014667031",
"014394699",
"016318163",
"013884633",
"013603722",
"010667491",
"009747493",
"007036808",
"007270907",
"009176101",
"008253474",
"005952328",
"002765108",
"004000175",
"002852550",
"003920775",
"001003981",
"021361594",
"007406860",
"004398370",
"021713605",
"005267336",
"001776280",
"026786611",
"007427700",
"040371435",
"001402021",
"015689439",
"003468963",
"003679523",
"024735918",
"004087487",
"021908464",
"009410828",
"009338717",
"006479766",
"023735990",
"022034421",
"018507305",
"014531364",
"014064350",
"020714086",
"023301001",
"022717832",
"004931400",
"003486897",
"009615251",
"019594321",
"026873201",
"028551146",
"014027857",
"026090092",
"007076888",
"023615521",
"005036618",
"019233747",
"021909423",
"022472040",
"025913140",
"006738741",
"002852826",
"022549416",
"002697631",
"007031322",
"007569193",
"022486918",
"026209649",
"025476305",
"040762009",
"028209786",
"026104218",
"024485599",
"024203463",
"021211504",
"019995416",
"017478368",
"018260686",
"017644914",
"017904216",
"018950288",
"017452976",
"014558998",
"013953767",
"014624342",
"014104820",
"013699347",
"009430769",
"009077369",
"007029626",
"008824900",
"007861523",
"008982730",
"009216025",
"009166700",
"008902502",
"007376038",
"008301158",
"008254407",
"006056769",
"006462471",
"006001575",
"006640209",
"004562608",
"004842984",
"005468259",
"005196296",
"004110200",
"000054804",
"018978014",
"002096084",
"018300693",
"004977581",
"029129236",
"023461666",
"020060067",
"009887586",
"023210077",
"006451308",
"006517875",
"005858944",
"023161364",
"013545298",
"021715288",
"022630984",
"019148716",
"016191346",
"006483005",
"001977201",
"015748193",
"021944110",
"040139166",
"028852134",
"013465884",
"006357573",
"000636029",
"009074817",
"009995231",
"014149936",
"020505897",
"007703752",
"000093804",
"040397111",
"026180223",
"025436124",
"001942681",
"009092578",
"019789612",
"013698755",
"009781675",
"021366846",
"005100144",
"021866626",
"010208038",
"000631763",
"005691072",
"001223810",
"029127443",
"027588941",
"040075203",
"040040806",
"028750117",
"029293946",
"025256595",
"026064422",
"023175270",
"022503925",
"024032366",
"024265997",
"022163165",
"016704687",
"017549900",
"019422917",
"015583499",
"015955337",
"014857832",
"013981330",
"014655536",
"015892023",
"009373472",
"010308994",
"009768827",
"010697525",
"007375885",
"007563812",
"008155481",
"008345565",
"007026542",
"006200137",
"006834279",
"005412667",
"005773020",
"005224100",
"003105940",
"004152771",
"000579051",
"009921462",
"007467316",
"028806104",
"007020213",
"023642021",
"001356309",
"000180892",
"025274446",
"007459193",
"022377264",
"021888898",
"008430198",
"022556947",
"018319871",
"006944833",
"008226627",
"025941592",
"021297426",
"009344195",
"006863166",
"005943609",
"004550290",
"003310383",
"025636029",
"024112625",
"022993501",
"024393122",
"021272073",
"020411910",
"016320029",
"013709327",
"009360415",
"007045421",
"007033874",
"006116710",
"002737019",
"002088102",
"001820081",
"000813871",
"022250292",
"022598052",
"009577219",
"026176477",
"001861399",
"006404858",
"006956863",
"004027664",
"023648519",
"020076583",
"020812488",
"023815966",
"008161486",
"027053779",
"000857538",
"002459355",
"004790975",
"014544027",
"016357562",
"001410454",
"002960045",
"003102454",
"020889594",
"021993002",
"010699987",
"028504724",
"028745955",
"029749879",
"025408689",
"025463832",
"025421480",
"025184526",
"025008919",
"025685177",
"023535470",
"023985362",
"021120330",
"021244042",
"021442244",
"018848660",
"017126617",
"017754954",
"017103236",
"018739826",
"019108372",
"015275455",
"014518743",
"016407113",
"015285810",
"013583720",
"007143519",
"008282324",
"007494193",
"008164055",
"007064515",
"008280085",
"006940567",
"006948612",
"006557944",
"006554809",
"004445379",
"004442669",
"005323895",
"002850966",
"002940367",
"002440516",
"000556393",
"000394062",
"001548269",
"023017894",
"024171889",
"007508031",
"028884481",
"009021496",
"014234975",
"002587529",
"015107664",
"009382391",
"026916000",
"020411852",
"022200016",
"020672762",
"000025571",
"024931635",
"001064956",
"019690303",
"025006375",
"019070934",
"013389071",
"006232675",
"020026677",
"021474424",
"005105968",
"007007057",
"007491320",
"005056894",
"010264750",
"025339519",
"009899554",
"003116811",
"025603725",
"020579522",
"001898520",
"004589623",
"013558184",
"007030651",
"027308113",
"028222921",
"009558754",
"005918213",
"005581911",
"028820660",
"028677561",
"027169137",
"007105348",
"006242043",
"005494789",
"003945278",
"027804116",
"002926082",
"010283234",
"020464917",
"009099640",
"020725059",
"009707030",
"013546416",
"023977583",
"019571449",
"027988952",
"028153203",
"029303467",
"040009514",
"028726806",
"039945340",
"026083172",
"026487006",
"026973162",
"026184450",
"025047310",
"025990900",
"025329400",
"026981937",
"026832385",
"023106329",
"024481776",
"020995954",
"020287556",
"020208227",
"021098990",
"021112799",
"020027077",
"020168607",
"019505966",
"019796055",
"017358621",
"019702595",
"018114497",
"014380586",
"015226024",
"013745575",
"014848546",
"015774096",
"015226095",
"014495732",
"014236236",
"013745442",
"014649494",
"014883618",
"013560980",
"013562525",
"007425512",
"007216926",
"005916649",
"006279376",
"006272448",
"005906449",
"006133073",
"005968914",
"006779155",
"006796517",
"006874001",
"005391981",
"004687268",
"005246388",
"003888190",
"004328654",
"003970846",
"004204751",
"003981627",
"002710306",
"003401780",
"000069080",
"002060357",
"000177090",
"001242671",
"008936840",
"000711862",
"023151521",
"001102562",
"005366864",
"015684519",
"027193065",
"026774884",
"026307616",
"004199976",
"021167285",
"007674996",
"007757940",
"008470587",
"019747631",
"009435217",
"004758981",
"024078865",
"025408439",
"025666009",
"000598010",
"003914516",
"029948772",
"007114624",
"021126644",
"013416090",
"026659570",
"027273236",
"006915603",
"029795266",
"014589262",
"008245406",
"003414173",
"006830982",
"022326711",
"020774551",
"017219396",
"015706670",
"006578814",
"004248968",
"019307943",
"020185217",
"023025019",
"020824158",
"025885971",
"026102015",
"023006083",
"008308128",
"005725116",
"020607570",
"004900312",
"004329082",
"024657901",
"029694355",
"021592831",
"017619334",
"000469341",
"004806779",
"029459452",
"041106214",
"028730713",
"028969956",
"027361382",
"027285000",
"025803570",
"027314886",
"023209999",
"022297002",
"022804278",
"023224028",
"023960396",
"019898795",
"021950785",
"021878686",
"019772480",
"019630508",
"009282385",
"010446760",
"010155736",
"007542560",
"006690517",
"006871626",
"006577382",
"006434173",
"006037030",
"003531231",
"004387246",
"002692084",
"001617150",
"000923091",
"000546232",
"003785640",
"007517050",
"023922103",
"002037822",
"007039933",
"024992240",
"005668650",
"041353314",
"007769742",
"006147618",
"003720611",
"004289795",
"000376968",
"000545595",
"019960062",
"028660582",
"008273542",
"009421146",
"007499230",
"006159945",
"025918014",
"020816859",
"014541705",
"009890070",
"007089667",
"000706766",
"023722664",
"023905792",
"024173123",
"004293096",
"014395157",
"020597429",
"027423195",
"004928103",
"007985884",
"003763206",
"005886384",
"029756980",
"005986145",
"006508559",
"021969918",
"025561451",
"026956327",
"004094350",
"002694005",
"027451874",
"028581593",
"025465687",
"025484566",
"023495816",
"019807463",
"017455533",
"014476809",
"013416952",
"007623230",
"006398746",
"003451029",
"000979859",
"025353769",
"006592480",
"008645691",
"004647738",
"023667304",
"028063938",
"003336285",
"015763586",
"004485470",
"003255857",
"020644714",
"006022103",
"009855549",
"028099934",
"020030399",
"001123783",
"041566292",
"041102806",
"028231117",
"025033745",
"022824416",
"022688676",
"022946995",
"024074275",
"020066876",
"020042874",
"020323844",
"018971540",
"018243300",
"017095782",
"017547936",
"018600693",
"018987405",
"009797448",
"009306495",
"010491449",
"009391844",
"013499222",
"010425968",
"008730357",
"009167342",
"009116149",
"007948951",
"007218497",
"008977377",
"007906817",
"008940803",
"006427328",
"006366026",
"006282035",
"006029140",
"004511298",
"005862641",
"005462348",
"003441859",
"001052276",
"001893194",
"002127540",
"000681435",
"001646989",
"000469613",
"003747036",
"014436431",
"007518363",
"022593047",
"025063098",
"025421945",
"003561081",
"017733891",
"010438615",
"014930125",
"016008319",
"026573469",
"023963503",
"025427023",
"028509197",
"022998232",
"026740075",
"008377182",
"006351101",
"002813001",
"002651950",
"001310790",
"009646222",
"023321067",
"007076014",
"006538781",
"026161651",
"020920606",
"014210592",
"024733379",
"006762151",
"023098203",
"020891031",
"010254758",
"026383972",
"003775018",
"014030821",
"006506956",
"019854462",
"021158751",
"009678568",
"013554634",
"024818112",
"000639841",
"027633822",
"029777437",
"028509421",
"028849942",
"029499597",
"028060395",
"026738754",
"024990669",
"022359380",
"021262589",
"021529150",
"020219728",
"021508630",
"020017862",
"021854788",
"021049469",
"020169908",
"017099232",
"017276897",
"017919447",
"018637362",
"019030552",
"018926039",
"015655142",
"015353852",
"010279911",
"007132456",
"008276938",
"008152557",
"006560484",
"006089938",
"006936674",
"005759788",
"005753358",
"005257056",
"005055054",
"004986609",
"004860592",
"004997727",
"005012548",
"004634413",
"003390680",
"004107161",
"003583689",
"000608990",
"001520430",
"000452593",
"010215218",
"017296757",
"001420517",
"019915735",
"018874321",
"009801195",
"003906165",
"010673190",
"004321428",
"023210767",
"001731236",
"027529391",
"027993512",
"001113226",
"022037431",
"003290684",
"019678318",
"006456240",
"004277770",
"008057691",
"020155127",
"015108887",
"024975766",
"026952447",
"024486234",
"019303888",
"004858689",
"022723870",
"007552858",
"019804185",
"019167614",
"025939634",
"021557926",
"015868450",
"021480020",
"022447507",
"019539314",
"023038677",
"027838218",
"023406573",
"023079635",
"020862781",
"021037318",
"014410171",
"016462076",
"014236042",
"010364450",
"008961132",
"005996484",
"004592274",
"006899548",
"013591006",
"018856213",
"014971259",
"009077581",
"004841744",
"003558719",
"003529884",
"005470304",
"003134265",
"021619257",
"004374341",
"022163271",
"001015782",
"008013537",
"028502255",
"026922790",
"025574377",
"023349427",
"024374533",
"024658845",
"021733920",
"021816047",
"020308647",
"016397356",
"015213170",
"014337833",
"014913068",
"009556920",
"010064836",
"009327289",
"013443984",
"009689590",
"007750433",
"005984344",
"006569006",
"004788366",
"004994385",
"005185770",
"004425236",
"003020917",
"004055125",
"002782460",
"004275707",
"004217801",
"000803220",
"000507917",
"003428214",
"005694756",
"026566846",
"020911546",
"006320704",
"000072526",
"019013502",
"020398784",
"020858383",
"003384008",
"007243868",
"028235822",
"004492822",
"025308137",
"004022649",
"005545794",
"099002435",
"006658634",
"022561120",
"005916716",
"027385052",
"025570751",
"026911372",
"021964564",
"017200736",
"005912912",
"003458020",
"006460852",
"003517874",
"001551824",
"013465511",
"025837803",
"006940563",
"001035213",
"005701615",
"027119011",
"018644165",
"005613862",
"027844588",
"029568672",
"027614675",
"017155997",
"009448763",
"013688063",
"006758534",
"021092166",
"018684028",
"013344555",
"024305635",
"019953800",
"001316516",
"026177213",
"025515081",
"022036979",
"013722015",
"006680386",
"020391793",
"022613134",
"014431744",
"001753664",
"024251664",
"022017330",
"006548418",
"029045554",
"002883955",
"007596376",
"027409532",
"029628903",
"028615903",
"032257930",
"029339814",
"040249656",
"028601735",
"027448843",
"028243927",
"026163463",
"026834987",
"026500063",
"027290324",
"024914489",
"026925626",
"026034254",
"027183981",
"023518880",
"024721293",
"022401359",
"024359739",
"024385966",
"021722264",
"021416263",
"020399670",
"019999385",
"020862290",
"021519706",
"020496785",
"019526332",
"018808436",
"019617912",
"016694845",
"017638170",
"019501898",
"017640588",
"019132646",
"014956161",
"015517761",
"013898516",
"014883753",
"014876484",
"015265802",
"013861662",
"014918146",
"014104897",
"014428838",
"009525632",
"013680342",
"009719508",
"010216904",
"013666304",
"010266391",
"010496001",
"009501343",
"010417042",
"008824065",
"007823882",
"008480772",
"007025593",
"008902525",
"007993443",
"007688672",
"008273724",
"008185852",
"009128658",
"006493065",
"006258183",
"006270201",
"006691844",
"006437278",
"006535909",
"006607333",
"006310564",
"006492868",
"005925027",
"005808956",
"005397836",
"005097502",
"005624362",
"005059456",
"005440531",
"005575228",
"005469822",
"004636198",
"005341445",
"004921687",
"005753989",
"004590811",
"004918477",
"002690976",
"004211224",
"002684880",
"003560500",
"002264592",
"003277022",
"002767458",
"002816291",
"001189649",
"001173150",
"000773769",
"001404352",
"001726174",
"000671980",
"001027418",
"000656309",
"001367828",
"000151448",
"000046602",
"001159102",
"000562235",
"000561730",
"013942620",
"024362206",
"021558777",
"013435081",
"000768438",
"007255620",
"024048365",
"014218011",
"018430867",
"017120360",
"021660940",
"007135181",
"001553669",
"005545229",
"013733539",
"026373682",
"007467556",
"027768135",
"004842905",
"004316778",
"006293663",
"040088713",
"023987228",
"010173463",
"005456985",
"005427893",
"028819236",
"022379879",
"013501980",
"002725027",
"040115964",
"027503880",
"014989359",
"005774283",
"004593206",
"009747669",
"006395002",
"005888611",
"014736636",
"022725914",
"018749661",
"009294960",
"014516531",
"023818365",
"004381880",
"004011901",
"028085921",
"024366961",
"019970753",
"021563527",
"005978562",
"005468127",
"002195810",
"000802350",
"028057073",
"029486909",
"028000613",
"026712095",
"017510490",
"014179190",
"014079284",
"004480710",
"004026945",
"000373184",
"016116016",
"023220914",
"014726295",
"001200452",
"000720479",
"024322044",
"026905982",
"014028703",
"004991902",
"006613860",
"018072667",
"027147039",
"024636163",
"019416989",
"040612511",
"029929342",
"028087840",
"025529851",
"025721103",
"026778969",
"027010225",
"023160019",
"024818539",
"022958454",
"023740048",
"022172420",
"022180038",
"020781440",
"020458678",
"021874937",
"019679190",
"019349247",
"018844220",
"017405846",
"016367573",
"013868177",
"016549995",
"014032884",
"010357825",
"009655502",
"010704340",
"008278742",
"009179683",
"007175554",
"006546029",
"006181906",
"006234898",
"004556060",
"005671344",
"005745188",
"005401717",
"005321097",
"004213117",
"002649790",
"000949587",
"000208215",
"000503268",
"019482565",
"022094554",
"017182272",
"027293069",
"000580335",
"001097451",
"008888443",
"021387468",
"014828982",
"013574964",
"018977729",
"006185416",
"027398295",
"020254067",
"005725265",
"027236070",
"018754967",
"005157415",
"005736522",
"002813268",
"001544328",
"004154263",
"009176302",
"015375809",
"018264211",
"025489876",
"023268514",
"001339096",
"025090753",
"021431721",
"026234171",
"006243284",
"022909866",
"004896966",
"006620601",
"018847506",
"003154615",
"025011218",
"025247930",
"023481343",
"023116352",
"020274643",
"000896665",
"000760794",
"000376129",
"001121369",
"001559387",
"028162105",
"001175747",
"009273302",
"006288676",
"006828817",
"009582459",
"004331807",
"007036593",
"021065104",
"000133612",
"019692226",
"007046155",
"009311746",
"014929036",
"000548409",
"027253189",
"040179319",
"026393787",
"025856904",
"027069588",
"024942379",
"025244111",
"027142703",
"023584915",
"024421651",
"023250127",
"023502511",
"020317011",
"019099913",
"017775530",
"019145883",
"019369797",
"018713319",
"017881399",
"016515246",
"014764369",
"016250570",
"013508411",
"009790973",
"010101062",
"013551644",
"010640674",
"009895777",
"010007413",
"013558945",
"009389961",
"007474876",
"008322855",
"006785228",
"006042706",
"006199531",
"006882992",
"006604636",
"006752175",
"004749273",
"004968203",
"005610886",
"004461081",
"005886150",
"005302486",
"005775506",
"003356283",
"003884229",
"003492205",
"002198646",
"004082988",
"003494067",
"002599319",
"000092325",
"000669339",
"001156772",
"000977191",
"000980156",
"041291918",
"024582559",
"001901414",
"015565983",
"019818315",
"019098522",
"021945545",
"014212713",
"006551355",
"009341963",
"018728008",
"020236847",
"015391702",
"007628762",
"014497058",
"000958437",
"029041783",
"002345105",
"013823643",
"009352163",
"023741958",
"014195541",
"027342544",
"018665546",
"001345725",
"024617498",
"019921216",
"007109780",
"023714053",
"025146741",
"020218934",
"013340616",
"013427427",
"006829990",
"007004420",
"004932693",
"005646288",
"003659638",
"002269848",
"015663136",
"010390408",
"007013949",
"009336669",
"010704013",
"009576740",
"020017194",
"029341381",
"003853117",
"025840682",
"027705222",
"019850256",
"022811692",
"014133622",
"009285224",
"007080985",
"027491650",
"023223671",
"020614109",
"007194938",
"009074632",
"006286010",
"006839955",
"005081318",
"005850269",
"004875008",
"003938568",
"002866282",
"000999004",
"001143331",
"028546598",
"019678565",
"004134027",
"014490907",
"006998741",
"025388661",
"019726607",
"019961828",
"006471015",
"010443517",
"009405012",
"009373594",
"005566083",
"026722214",
"010676417",
"027240233",
"009359643",
"028087836",
"028098907",
"040483714",
"026697107",
"026527430",
"022343548",
"023917123",
"024105414",
"024264648",
"020793097",
"018105077",
"019060720",
"019347125",
"017140203",
"018346077",
"014971992",
"014080004",
"014167720",
"013947288",
"014393220",
"010587920",
"010231674",
"010464532",
"009960572",
"013580473",
"009355153",
"009138556",
"007360537",
"007950494",
"007067787",
"009152033",
"007032300",
"006953881",
"004644810",
"005841845",
"004413044",
"005407034",
"005867159",
"004316549",
"004155767",
"001321095",
"000547626",
"001327149",
"000826273",
"024003334",
"020840879",
"020365291",
"001873883",
"007477190",
"000797138",
"027308838",
"006775932",
"002277871",
"001308299",
"005252033",
"009100437",
"004878975",
"019016054",
"026242030",
"018138241",
"005612886",
"005064011",
"024324614",
"002170848",
"029768161",
"025401959",
"022545669",
"023058305",
"022502441",
"021733859",
"021878916",
"017395705",
"018157931",
"019417457",
"016465211",
"014950594",
"008983205",
"007240873",
"006822087",
"004558379",
"005774084",
"005511364",
"003499647",
"000615110",
"000629885",
"001357839",
"000456186",
"000969607",
"000696166",
"001960402",
"000993361",
"027436062",
"027669380",
"026013562",
"026491579",
"022268220",
"023504427",
"024635415",
"019284411",
"014977050",
"014746002",
"014725394",
"016497955",
"016015484",
"009737008",
"007741063",
"006396278",
"006630967",
"006501891",
"006424358",
"004965274",
"005078810",
"003156047",
"001795996",
"000585688",
"001932045",
"028027453",
"001108858",
"018584862",
"025339099",
"024823774",
"023773871",
"005971133",
"001454285",
"026709371",
"004933464",
"013790376",
"014663200",
"016506583",
"016795789",
"009403252",
"005445429",
"018946602",
"006470788",
"009520720",
"010183534",
"023204751",
"026476070",
"015856923",
"015373119",
"014863688",
"009076816",
"004330681",
"000262626",
"015184705",
"006361777",
"005547874",
"004621094",
"014615967",
"028785905",
"024116436",
"018792503",
"027838661",
"013569528",
"008011006",
"014763662",
"026686062",
"000603538",
"007632691",
"013728706",
"004036427",
"004828206",
"029172586",
"028873381",
"027279997",
"026739642",
"026689649",
"025819726",
"024488588",
"023757289",
"023896145",
"023167343",
"020898296",
"020097655",
"020863103",
"017636646",
"017202402",
"019392530",
"013768815",
"013846829",
"016134724",
"015664325",
"013947521",
"009706399",
"013413305",
"009504885",
"013322613",
"010159055",
"013398052",
"008357452",
"009234226",
"008120212",
"007480487",
"008861952",
"007283535",
"008226821",
"008304865",
"008134387",
"009104729",
"007192028",
"008645780",
"006963248",
"006824583",
"006342784",
"006536271",
"006440843",
"006522979",
"005624094",
"004575860",
"005228733",
"005013486",
"004771832",
"003154136",
"003805653",
"003884915",
"002688420",
"003755411",
"003135959",
"002569566",
"003629209",
"001458792",
"001198436",
"001818155",
"027289192",
"007778597",
"006236218",
"024637008",
"020547424",
"014541406",
"022917247",
"009834577",
"015388545",
"009121551",
"002258098",
"006341253",
"010284052",
"003756530",
"021351711",
"019348922",
"026529520",
"003087858",
"000889050",
"021188470",
"000306860",
"000076181",
"001442308",
"009869572",
"006267685",
"008125788",
"001435862",
"022095713",
"021804166",
"009494701",
"020327484",
"009152196",
"028368695",
"013986457",
"018314335",
"019039852",
"028346026",
"019478974",
"027402624",
"001918313",
"028001468",
"019445976",
"010220085",
"026031780",
"013970187",
"009390703",
"006124978",
"005942345",
"000905057",
"022614255",
"021910391",
"020342246",
"014222578",
"009600566",
"006216883",
"006505271",
"005063810",
"002438296",
"003937168",
"018556309",
"000162019",
"003227663",
"004190369",
"016861809",
"025072192",
"002483406",
"019136154",
"018732470",
"006757028",
"006925560",
"010154731",
"020620623",
"021383554",
"020247478",
"007274417",
"025789275",
"000330056",
"023000569",
"020580685",
"041880361",
"028135424",
"027539002",
"028576416",
"023076974",
"022450757",
"023820052",
"022274717",
"021650235",
"021106860",
"020475928",
"020875080",
"016805845",
"014326042",
"015239627",
"010367393",
"010052924",
"009404164",
"009476125",
"009318947",
"008949996",
"007868545",
"007408451",
"007233273",
"008312525",
"007569393",
"006018726",
"006549003",
"006337222",
"006228963",
"006557094",
"006794231",
"004869779",
"005575744",
"004555813",
"004531511",
"005439041",
"003010297",
"002752496",
"003071006",
"000153022",
"002047088",
"001250934",
"001832301",
"001704141",
"013406876",
"025094842",
"000648137",
"010548697",
"001081313",
"018817395",
"026519585",
"001038120",
"022669757",
"025357238",
"028038825",
"022238949",
"003140363",
"019353601",
"028902971",
"004033258",
"006983777",
"016364508",
"029045857",
"023739324",
"009400015",
"009282595",
"009151654",
"007146516",
"006410219",
"004546222",
"005550074",
"004932566",
"004367607",
"004378793",
"001279019",
"023011029",
"020011095",
"018018729",
"009512376",
"009192138",
"003043406",
"004178746",
"000196609",
"002894490",
"021886755",
"027637055",
"010664057",
"006534183",
"019541561",
"012143024",
"020488096",
"024745042",
"003781860",
"023407410",
"009402733",
"007679064",
"099008572",
"028801752",
"014856588",
"009185442",
"006414156",
"001389268",
"021115105",
"014879934",
"021984957",
"003682227",
"019748888",
"007948849",
"027528742",
"007163248",
"027451357",
"027518121",
"006519522",
"005069471",
"003902552",
"000965108",
"009553132",
"008222920",
"007235859",
"015024764",
"020588600",
"005880203",
"008941294",
"007638817",
"021285859",
"001063528",
"023253751",
"016124137",
"018267158",
"028439132",
"004268333",
"023779698",
"018858982",
"018431563",
"041006946",
"028953139",
"027996123",
"027494988",
"041435826",
"030002999",
"026520767",
"027320525",
"025805753",
"023876471",
"022167496",
"024709550",
"020384971",
"020727458",
"016873734",
"018421038",
"015917518",
"013951194",
"013960643",
"013693780",
"010571234",
"009539694",
"009274706",
"009812298",
"007090225",
"008365860",
"008065665",
"007958606",
"006290161",
"006059866",
"006810395",
"004597485",
"005845625",
"005890588",
"002982192",
"001234227",
"000988349",
"016632803",
"024873886",
"013978425",
"006573285",
"007896116",
"026187198",
"006029743",
"006943033",
"020129321",
"018155219",
"013697571",
"003622760",
"005812301",
"020465720",
"003579237",
"006475950",
"020482593",
"022533727",
"016958220",
"009327345",
"004643828",
"004473020",
"025816155",
"024514371",
"003027791",
"024256106",
"003463429",
"002283671",
"006588659",
"006668931",
"027836022",
"000157823",
"004175035",
"013869615",
"006533335",
"005603252",
"023356049",
"006387807",
"006918588",
"010456229",
"015656943",
"023643653",
"027128312",
"022799165",
"023285126",
"020295760",
"021448740",
"020763555",
"018137634",
"014436930",
"014074096",
"014838954",
"014040678",
"010093064",
"013607017",
"009035401",
"007236235",
"006736333",
"006011417",
"004837449",
"004714866",
"004885666",
"004086204",
"002192535",
"029803695",
"026394790",
"022247560",
"019675331",
"007577354",
"009137188",
"009223895",
"007945542",
"008228931",
"005527128",
"019468418",
"020492061",
"022143813",
"000711394",
"001593436",
"013497759",
"007291160",
"027907481",
"002433038",
"024286624",
"006559579",
"013624180",
"015426192",
"024691614",
"025040259",
"017944392",
"022015128",
"010536670",
"006091629",
"026008687",
"006053975",
"007392274",
"027601832",
"028883553",
"025697274",
"025581633",
"022947291",
"024637637",
"020746107",
"013862710",
"016335626",
"015648329",
"015641264",
"009239317",
"007741094",
"006515435",
"006876800",
"004642886",
"005860451",
"004446331",
"003104077",
"003852099",
"024560405",
"016130736",
"006504260",
"018928807",
"014533739",
"026738186",
"002620292",
"008761893",
"003423277",
"009345278",
"006996872",
"028785440",
"023451893",
"009975813",
"005567115",
"003853555",
"003333496",
"022017551",
"004850356",
"023833067",
"028051211",
"008039654",
"018705303",
"002516495",
"024561420",
"005880249",
"021293897",
"005146161",
"014489192",
"014036214",
"005977377",
"007274607",
"018129573",
"019357926",
"004379970",
"010086937",
"022594309",
"018279057",
"009137282",
"006596405",
"006159720",
"006040123",
"003647594",
"003104411",
"002991618",
"003420819",
"003719472",
"000714012",
"007745083",
"025659870",
"024190585",
"013988980",
"007695355",
"019032963",
"019980774",
"025898468",
"005863543",
"009886512",
"005645390",
"019072630",
"025882734",
"016654594",
"000857890",
"009414183",
"026241997",
"007575845",
"026234224",
"022681032",
"024799808",
"022256091",
"019532037",
"018232261",
"016917600",
"019141523",
"009398673",
"007771564",
"007248645",
"008337738",
"007706774",
"006823094",
"006868107",
"005496582",
"005861894",
"005363778",
"003621362",
"024985415",
"009664454",
"014803336",
"019092644",
"020950354",
"010180667",
"025569397",
"003134961",
"007884909",
"020333117",
"027220769",
"025516222",
"013746807",
"026211826",
"013544438",
"027693464",
"027007469",
"024935741",
"024775134",
"019394762",
"018876730",
"018819539",
"015158810",
"010553722",
"007238529",
"006241298",
"006942117",
"005575837",
"000600780",
"022579002",
"005320813",
"023818382",
"005071912",
"022021349",
"013719276",
"022556641",
"020768171",
"010574844",
"007208263",
"004265130",
"002700330",
"009582468",
"006021939",
"001037541",
"024039484",
"005464141",
"025963055",
"009759965",
"026923549",
"013941421",
"005959642",
"021399153",
"024514473",
"027439520",
"020450873",
"005259325",
"027811431",
"026537591",
"013883405",
"009259116",
"007494442",
"006152770",
"006521165",
"002433934",
"020012119",
"005580913",
"009392712",
"026934039",
"018880808",
"008921064",
"006249475",
"023248288",
"015556293",
"005130015",
"017360318",
"019123128",
"018222870",
"026219707",
"015748582",
"006629317",
"025268896",
"024522691",
"023484326",
"024497960",
"019399019",
"010204391",
"010273502",
"002487179",
"013424105",
"007121704",
"004375100",
"006751201",
"001345139",
"023418914",
"020920314",
"001788772",
"026858594",
"017134251",
"018867579",
"006296127",
"026726250",
"023545549",
"006574487",
"025076174",
"018217280",
"019009830",
"006220397",
"022912700",
"005176911",
"006647129",
"002179852",
"028695314",
"002477270",
"013413889",
"005300605",
"013435497",
"002943518",
"000025950",
"005967408",
"007586022",
"007991855",
"025924820",
"028368441",
"000030125",
"000478639",
"006418876",
"006389720",
"000463737",
"013424619",
"000234558",
"026310965",
"008221745",
"002057068",
"003811016",
"013339671",
"010041368",
"026937744",
"027815013",
"009402426",
"013424641",
"000853895",
"009640729",
"007161751",
"006984813",
"009561050",
"014322016",
"007600630",
"000477304",
"007479462",
"009714262",
"001110113",
"007019446",
"028721608",
"028694714",
"009601451",
"008186658",
"028189986",
"027525121",
"008221714",
"008221750",
"013326602",
"010686358",
"004480553",
"009170029",
"017172281",
"008042342",
"008332182",
"000615578",
"008038039",
"006375342",
"008033921",
"014234322",
"009714294",
"007049013",
"028711032",
"014091197",
"001459705",
"025952707",
"000333536",
"006657393",
"009060840",
"007183650",
"002735513",
"009586603",
"008041739",
"003569184",
"005129227",
"009396113",
"008122039",
"026767026",
"007007108",
"013343764",
"014091090",
"013350360",
"013373169",
"008052834",
"026836625",
"014106229",
"009122949",
"025880689",
"028640455",
"009714249",
"013374014",
"009927577",
"007125511",
"028695251",
"008033945",
"008296600",
"007575705",
"013435516",
"028393093",
"001202281",
"026041383",
"025777690",
"009337828",
"005545475",
"025960384",
"026902687",
"005322038",
"014330870",
"014234286",
"026029584",
"007627577",
"026263794",
"009415790",
"004796201",
"009037490",
"013813944",
"009714261",
"009372696",
"010530066",
"009714221",
"024505265",
"000347983",
"006988575",
"009095955",
"001226667",
"009611092",
"020986158",
"013424683",
"026012665",
"014322002",
"000324332",
"002603777",
"006941293",
"026041380",
"009714286",
"025878706",
"008047654",
"027684741",
"013334643",
"009399077",
"008904278",
"009602483",
"001173080",
"007622872",
"014234282",
"028721635",
"009714242",
"013424789",
"004085600",
"013412292",
"009714260",
"009709803",
"009714277",
"005420996",
"025938218",
"007987292",
"013435489",
"026050460",
"010444639",
"004933004",
"025806973",
"000758013",
"009154160",
"013334517",
"003890974",
"020400540",
"007038865",
"013435520",
"006787262",
"028712790",
"008049002",
"013424748",
"008904268",
"006046535",
"028310629",
"009714056",
"009600104",
"008310735",
"010041369",
"005312023",
"009714251",
"008038084",
"010161204",
"005876223",
"027370341",
"025963018",
"001402572",
"013337902",
"001346398",
"009174932",
"013921942",
"010701549",
"013435518",
"008102376",
"005056646",
"028720038",
"009540723",
"009163856",
"005861122",
"008221718",
"014322011",
"000673913",
"005860608",
"013347656",
"000630479",
"008044400",
"001275717",
"010324221",
"004232518",
"008246695",
"008100343",
"009714227",
"028685161",
"005353273",
"005746741",
"007649214",
"013424600",
"009727436",
"006689824",
"008118920",
"028719228",
"007479302",
"008941300",
"013982327",
"001331560",
"007695107",
"006155367",
"007649221",
"014104495",
"028719222",
"007622853",
"008460663",
"009607453",
"006982126",
"008895296",
"008033922",
"013551775",
"004995053",
"028310279",
"028392978",
"008045390",
"013424533",
"026836620",
"026293238",
"014094219",
"026891764",
"008034135",
"008188672",
"025939971",
"009714306",
"000449180",
"009061441",
"009614876",
"008117344",
"013424584",
"009611971",
"005721654",
"026057386",
"008221712",
"002163676",
"008056774",
"006623995",
"010160383",
"014110027",
"008295694",
"000655253",
"013707676",
"028681401",
"001333870",
"005193603",
"009737709",
"001033194",
"010644596",
"014508437",
"005091224",
"028694764",
"009060803",
"009397837",
"027263578",
"026010774",
"005285043",
"006949853",
"008045203",
"005980618",
"026958547",
"013424554",
"014101254",
"007649216",
"009597575",
"009611113",
"009360425",
"013370346",
"007585964",
"008045201",
"005027094",
"009714264",
"014095914",
"013354786",
"014109452",
"003971876",
"014091199",
"000419919",
"003957683",
"008332174",
"009714285",
"027288893",
"006981947",
"000140092",
"009573950",
"007695298",
"009594021",
"001457024",
"009631978",
"000863409",
"009616358",
"027243699",
"025697978",
"013424568",
"009612636",
"005919841",
"009714282",
"005302784",
"016660970",
"007585946",
"009591191",
"016148911",
"001427482",
"008991831",
"008555197",
"010705117",
"009561175",
"007262164",
"013435530",
"008028504",
"007585953",
"013324270",
"009582229",
"013424548",
"013381404",
"025820893",
"028692350",
"000208653",
"007695144",
"028639639",
"007665757",
"000717732",
"028720250",
"009714297",
"028720708",
"006767452",
"025938210",
"007689407",
"007185286",
"028683435",
"006160531",
"028695295",
"009636201",
"025820898",
"008256675",
"009714299",
"007634507",
"003682556",
"026767027",
"008045287",
"009097775",
"008276475",
"026866456",
"025780695",
"013348178",
"028683510",
"009714266",
"006658656",
"009714216",
"028392491",
"014095149",
"009714054",
"009096874",
"009612656",
"025738111",
"006237011",
"008066807",
"000018465",
"009439091",
"006913538",
"007211339",
"014091117",
"013435522",
"006703251",
"009714210",
"008042537",
"028719192",
"006883886",
"006245430",
"000625649",
"009136411",
"027675262",
"024854432",
"007656420",
"010247517",
"009714245",
"028720449",
"008096614",
"008420510",
"010589216",
"028720822",
"005091214",
"007510073",
"009324689",
"010463526",
"014085837",
"009714219",
"007585963",
"009714267",
"013424793",
"005293762",
"026372458",
"003792217",
"028695719",
"007479442",
"010195588",
"027626992",
"009096758",
"026107908",
"013435506",
"014106245",
"000494239",
"028719978",
"013435524",
"008904259",
"007160506",
"013435490",
"009615520",
"009714313",
"006997962",
"008042630",
"013424775",
"008102389",
"008221764",
"027126611",
"013424721",
"013413890",
"007203759",
"013352430",
"014091068",
"000621670",
"007987117",
"008221601",
"006871521",
"009081107",
"008221724",
"028392580",
"008316909",
"006832138",
"007661111",
"010326563",
"009187662",
"014234310",
"000283734",
"009087870",
"009714272",
"028681312",
"009914627",
"009140646",
"004946396",
"013424744",
"006732553",
"024877050",
"025419170",
"025940904",
"013343193",
"000625166",
"010705972",
"003126374",
"014234284",
"007649215",
"027160611",
"028721671",
"028720168",
"013424601",
"014330882",
"001456677",
"013424770",
"006386878",
"002354143",
"028695799",
"025692931",
"028721710",
"028720860",
"007119751",
"000616383",
"009204718",
"028241614",
"005753062",
"009133355",
"009714254",
"013435526",
"026886440",
"006986881",
"006767714",
"028222384",
"013334622",
"007479496",
"028351567",
"001039334",
"028545580",
"014234220",
"028695233",
"005670885",
"025955292",
"006876165",
"028692183",
"028695733",
"028721774",
"026329045",
"026541817",
"025976036",
"028695411",
"025926577",
"009521554",
"009520650",
"008332160",
"006143005",
"006883562",
"009064360",
"014321999",
"009714311",
"028213691",
"006732249",
"009547853",
"006016086",
"010703840",
"009714309",
"006619461",
"001162869",
"008221748",
"009064396",
"009064366",
"009611962",
"009035460",
"009714256",
"028695297",
"013435505",
"008221767",
"005742380",
"006807424",
"000202769",
"005439892",
"025777706",
"014110218",
"007575725",
"009097361",
"013435491",
"006575785",
"025988035",
"013424720",
"001532267",
"028695727",
"007623616",
"010701564",
"012413819",
"007636552",
"000419978",
"014234226",
"007635837",
"005392653",
"006809696",
"000279393",
"013345497",
"009577369",
"009594572",
"013368584",
"009714066",
"009088387",
"013424552",
"006589309",
"008981847",
"013435529",
"010510759",
"009326558",
"009714217",
"004830186",
"006624014",
"000718333",
"008904257",
"013435512",
"009098182",
"006760735",
"009714278",
"009327988",
"007586014",
"013400248",
"008332180",
"009590773",
"006751056",
"007987443",
"009617738",
"009617153",
"013424698",
"007633632",
"014234328",
"009587815",
"007288121",
"007146626",
"013350476",
"006940926",
"010097286",
"009174283",
"009714062",
"004063221",
"013424772",
"013370640",
"000592436",
"009064367",
"025979722",
"000614401",
"013445133",
"013383675",
"028214612",
"007894014",
"009714302",
"005908572",
"005816736",
"008360327",
"009109303",
"008116271",
"007128495",
"014091133",
"002994328",
"000720116",
"008457322",
"008034134",
"006850864",
"009714258",
"028721621",
"026692802",
"004245752",
"013424792",
"014023166",
"013350521",
"007263341",
"006740651",
"000268800",
"006755839",
"008221751",
"025994624",
"009714244",
"006826592",
"008221755",
"009410298",
"027278772",
"004094115",
"006028263",
"004629146",
"007596466",
"013348821",
"007249681",
"003749865",
"009714240",
"006400323",
"028393013",
"008206148",
"013424514",
"009134619",
"014100121",
"009115356",
"009714223",
"013435507",
"018556286",
"000325754",
"007649211",
"009169479",
"009936689",
"002795393",
"008046720",
"006840987",
"006024691",
"004137431",
"013420411",
"014095444",
"007649219",
"005467845",
"006051319",
"007192561",
"009163072",
"004206535",
"014110731",
"008114238",
"028720039",
"009316468",
"014234264",
"000700691",
"000550935",
"009098644",
"028600431",
"009714270",
"004945832",
"009140211",
"000379465",
"009714239",
"006925704",
"010172728",
"009714279",
"009783942",
"013352403",
"009150739",
"002346587",
"028720357",
"014234280",
"009714064",
"009714304",
"007584602",
"000629219",
"028372041",
"015533068",
"006793701",
"005243368",
"010692238",
"007649212",
"025705145",
"014590225",
"026925385",
"014234277",
"005591488",
"008221715",
"000159763",
"009714222",
"013851492",
"007674933",
"013424643",
"013424499",
"013424515",
"009340207",
"009107538",
"009709540",
"008221759",
"009714207",
"014234320",
"010701600",
"009607660",
"005113445",
"005095870",
"006955856",
"008008580",
"008048028",
"009371030",
"006628310",
"001349775",
"009714058",
"000463735",
"004370074",
"008036785",
"009714273",
"009714247",
"007987517",
"008326801",
"014110675",
"009600323",
"000293911",
"003262353",
"006790966",
"008119753",
"009098788",
"027164692",
"027594995",
"004533145",
"013369199",
"009588546",
"001316139",
"008221763",
"028694880",
"008221591",
"028720825",
"013424537",
"028201208",
"028695312",
"026040496",
"009583412",
"009044227",
"009517208",
"007695233",
"007203717",
"014112190",
"001254747",
"005412380",
"001330586",
"007629936",
"008060200",
"009146658",
"009618328",
"005491537",
"004134824",
"009596929",
"009714061",
"010575798",
"009322901",
"000806194",
"006840491",
"007684923",
"009064358",
"009141117",
"008550325",
"014091422",
"000194042",
"007575672",
"013977228",
"014106211",
"009413847",
"000400682",
"007636989",
"028682862",
"000842718",
"006414975",
"004422580",
"006432371",
"003085518",
"007915785",
"014234274",
"003541683",
"000284103",
"008221733",
"005509140",
"000219138",
"007695143",
"008338903",
"009186571",
"000720257",
"006575869",
"013765815",
"002733248",
"003733163",
"000155138",
"000004699",
"008281620",
"004531933",
"005769565",
"000489720",
"039928196",
"009714220",
"009714059",
"009619868",
"000817326",
"000469263",
"008705959",
"005454180",
"010702094",
"009714246",
"003710935",
"013424519",
"007695281",
"007695282",
"001292602",
"006720652",
"009714225",
"003891388",
"009714213",
"002435644",
"006684013",
"009714243",
"004876343",
"001360365",
"004809722",
"009714318",
"000069607",
"006521338",
"008221760",
"009151097",
"000165866",
"013435502",
"001462951",
"005054588",
"008221754",
"003488904",
"000560071",
"008904267",
"009109560",
"007237561",
"002990516",
"000640413",
"005069787",
"006695164",
"009628452",
"000154575",
"009611876",
"009130587",
"013339382",
"009714218",
"009309342",
"009063962",
"007649231",
"014234308",
"013349845",
"013343337",
"013372893",
"009623234",
"005732367",
"014101115",
"013355406",
"013435533",
"028720936",
"014105339",
"007479868",
"005939072",
"007585973",
"008221719",
"014234269",
"001300799",
"013339679",
"013435496",
"009714310",
"009593900",
"000333528",
"009599865",
"006443834",
"008349226",
"009138167",
"003827140",
"009527788",
"007916395",
"000411634",
"007623002",
"007479406",
"009351427",
"009636867",
"001283058",
"008332158",
"007510147",
"013349752",
"013948149",
"009389031",
"009596776",
"042727512",
"000587348",
"000065765",
"008221716",
"008045431",
"013349920",
"004318774",
"005146770",
"008874301",
"008282154",
"006641785",
"003485856",
"000586604",
"007254862",
"006343056",
"004454332",
"009714307",
"000482053",
"000466668",
"005158264",
"007649232",
"000001088",
"001454831",
"003980683",
"007575689",
"013355405",
"000275049",
"005094597",
"003388928",
"003924076",
"009353272",
"000488930",
"009160537",
"007649235",
"000671334",
"007183863",
"008221744",
"013435517",
"014321994",
"006684517",
"007649228",
"009064354",
"008221753",
"008221762",
"009318779",
"000588676",
"008251743",
"001258320",
"009104263",
"000449037",
"009714292",
"000896872",
"009405252",
"007205231",
"000419988",
"014322003",
"027722419",
"000082162",
"010705106",
"007276144",
"013424535",
"006893808",
"003544856",
"005333688",
"008347369",
"000664471",
"000634183",
"013435525",
"014083584",
"008837840",
"001232436",
"009714263",
"000834507",
"008047770",
"004526011",
"008308019",
"005446864",
"005632352",
"001426933",
"006223468",
"005517820",
"001408911",
"013435492",
"000489731",
"009714269",
"008031705",
"001331587",
"005730116",
"000390410",
"009714281",
"000230034",
"006573610",
"008038942",
"000017424",
"007695266",
"009714301",
"000194033",
"013352510",
"008904272",
"009352088",
"009604558",
"000097214",
"000210195",
"009317711",
"007695106",
"013435511",
"014508438",
"004151871",
"000818000",
"009416256",
"013424567",
"009027189",
"000637511",
"009184306",
"009714312",
"009714215",
"013424718",
"000685648",
"009134560",
"000293134",
"014321995",
"000636133",
"006079525",
"008933610",
"008253492",
"007631496",
"005022723",
"009714268",
"008221757",
"009714230",
"006366779",
"005299665",
"013326605",
"009597494",
"003237274",
"006561562",
"006633331",
"009714298",
"006792787",
"007695358",
"013424550",
"009620939",
"006586336",
"009714063",
"014234218",
"000195715",
"008221896",
"008904299",
"006912332",
"005852043",
"014091929",
"007585974",
"005647062",
"013348192",
"005967406",
"009714305",
"005248248",
"009714229",
"008904249",
"013427104",
"009546815",
"008221730",
"000698813",
"006638951",
"008705364",
"006777487",
"004966749",
"006684422",
"007628248",
"010705099",
"002613587",
"012414846",
"009186554",
"007575637",
"009183686",
"000269367",
"001228108",
"027018975",
"008221731",
"007991841",
"000275291",
"009605606",
"000539810",
"009714280",
"009433457",
"008263983",
"009620604",
"009521489",
"027164696",
"013424794",
"009315840",
"000089989",
"009714283",
"007186611",
"009714276",
"005487999",
"009619908",
"009504575",
"007649222",
"000555921",
"009408770",
"009081261",
"009638717",
"099000806",
"004595330",
"000555948",
"007635219",
"014917842",
"008192575",
"005504494",
"008036216",
"009150751",
"000197038",
"000059060",
"005848102",
"007695249",
"005953767",
"000063647",
"009714295",
"014079234",
"007031866",
"000858844",
"014234333",
"006001886",
"000403680",
"000082950",
"014034622",
"041044288",
"000235233",
"023596925",
"003318239",
"000545442",
"006129755",
"000515063",
"000044638",
"015585435",
"004420836",
"006594381",
"027333596",
"000073547",
"019612563",
"007243060",
"016517242",
"009281201",
"001409145",
"000433035",
"007931677",
"024016715",
"000573836",
"022485377",
"007883498",
"004532077",
"023532080",
"028567939",
"007284962",
"000048650",
"000342501",
"024264458",
"000598202",
"040952574",
"022247994",
"027237542",
"006081359",
"000054710",
"000050811",
"016823545",
"001520004",
"006561532",
"004831255",
"006725547",
"000059549",
"007766147",
"021671885",
"023897661",
"008088509",
"023968894",
"006426067",
"022918324",
"000530885",
};
            //List<int> days = new List<int> { 30, 21, 14, 7, 5, 2, 1 };
            List<int> days = new List<int> { 30 };
            //&& days.Contains(DbFunctions.DiffDays(DateTime.UtcNow, d.SCHEDULEDUEDATE).Value)
            //var loanRepaymentReminder = context.TBL_GLOBAL_EXPOSURE.Where(d => customerIds.Contains(d.CUSTOMERID) && d.AMOUNTDUE.Value > 0).ToList();
            var loanRepaymentReminder = context.TBL_NEXT_PRINCIPAL_REPAYMENT.Where(d => customerIds.Contains(d.CUSTOMERID) && d.AMOUNTDUE.Value > 0 && days.Contains(DbFunctions.DiffDays(DateTime.UtcNow, d.SCHEDULEDUEDATE).Value)).ToList();
            var alertTitleInfo = context.TBL_ALERT_TITLE.Where(a => a.BINDINGMETHOD == "GetLoanRepaymentReminder").FirstOrDefault();
            int numberOfDays = 0;
            int daysToUse = 0;
            int numberOfInterestDays = 0;
            var defaultEmail = "";
            var interestDueDate = "";
            var interestAmountDue = "";

            if (alertTitleInfo.DEFAULTEMAIL != null)
            {
                defaultEmail = alertTitleInfo.DEFAULTEMAIL;
            }

            if (loanRepaymentReminder != null && loanRepaymentReminder.Count() > 0)
            {

                List<AlertsViewModel> alerts = new List<AlertsViewModel>();
                foreach (var i in loanRepaymentReminder)
                {
                    var interestDetail = context.TBL_NEXT_INTEREST_REPAYMENT.Where(d => d.ACCOUNTNUMBER == i.ACCOUNTNUMBER && d.OUTSTANDINGBALANCE.Value > 0).FirstOrDefault();
                    var customerDetail = context.TBL_CUSTOMER_EMAIL.Where(d => d.CUSTOMERNO == i.CUSTOMERID).FirstOrDefault();
                    if (interestDetail == null)
                    {
                        numberOfInterestDays = 0;
                        interestDueDate = "00-00-0000";
                        interestAmountDue = "0.00";
                    }
                    else
                    {
                        numberOfDays = (i.SCHEDULEDUEDATE.Value - DateTime.Now).Days;
                        numberOfInterestDays = (interestDetail.SCHEDULEDUEDATE.Value - DateTime.Now).Days;
                        interestDueDate = interestDetail.SCHEDULEDUEDATE?.ToString("dd-MM-yyyy");
                        interestAmountDue = interestDetail.CURRENCY + "" + string.Format("{0:#,##.00}", Convert.ToDecimal(interestDetail.OUTSTANDINGBALANCE.Value));

                    }
                    if (numberOfDays > numberOfInterestDays)
                    {
                        daysToUse = numberOfInterestDays;
                    }
                    else { daysToUse = numberOfDays; }

                    var dueDate = i.SCHEDULEDUEDATE?.ToString("dd-MM-yyyy");
                    var amountDue = i.CURRENCY + "" + string.Format("{0:#,##.00}", Convert.ToDecimal(i.OUTSTANDINGBALANCE.Value));
                    AlertsViewModel alert = new AlertsViewModel();
                    var alertTitle = alertTitleInfo.TITLE;
                    var alertTemplate = alertTitleInfo.TEMPLATE;
                    if (numberOfDays > 0)
                    {
                        string emailList = "";
                        alertTemplate = alertTemplate.Replace("@{{customerName}}", i.CUSTOMERNAME);
                        alertTemplate = alertTemplate.Replace("@{{maturityBand}}", daysToUse.ToString());
                        alertTemplate = alertTemplate.Replace("@{{amountDue}}", amountDue);
                        alertTemplate = alertTemplate.Replace("@{{dueDate}}", dueDate);
                        alertTemplate = alertTemplate.Replace("@{{interestAmountDue}}", interestAmountDue);
                        alertTemplate = alertTemplate.Replace("@{{interestDueDate}}", interestDueDate);
                        emailList = customerDetail?.EMAIL;
                        emailList = emailList+";"+defaultEmail;
                        alert.receiverEmailList.Add(emailList);
                        alert.template = alertTemplate;
                        alert.alertTitle = alertTitle;
                        alert.canFire = true;
                        alert.operationMethod = alertTitleInfo.BINDINGMETHOD;
                        alerts.Add(alert);
                    }
                }
                SendAlertNotification(alerts);
            }
        }



        public void GetInsurancePolicyExpirationNotification()
        {
            // GetInsurancePolicyExpirationNotification method
            List<int> days = new List<int> { 60, 30, 21, 14, 7, 5, 2, 1 };
            var insurancePolicyNotification = context.TBL_COLLATERAL_INSURANCE_TRACKING.Where(d => days.Contains(DbFunctions.DiffDays(DateTime.UtcNow, d.INSURANCEENDDATE).Value) && d.DELETED == false && d.INSURANCESTATUSID != (int)InsuranceStatusEnum.Expired).ToList();
            var alertTitleInfo = context.TBL_ALERT_TITLE.Where(a => a.BINDINGMETHOD == "GetInsurancePolicyExpirationNotification").FirstOrDefault();
            var defaultEmail = "";
            var emailList = "";
            if (alertTitleInfo.DEFAULTEMAIL != null)
            {
                defaultEmail = ";" + alertTitleInfo.DEFAULTEMAIL;
            }

            if (insurancePolicyNotification.Count() > 0)
            {

                List<AlertsViewModel> alerts = new List<AlertsViewModel>();
                foreach (var i in insurancePolicyNotification)
                {
                    AlertsViewModel alert = new AlertsViewModel();
                    var alertTitle = alertTitleInfo.TITLE;
                    var alertTemplate = alertTitleInfo.TEMPLATE;

                    var appDetails = context.TBL_LOAN_APPLICATION_DETAIL.Find(i.LOANAPPLICATIONDETAILID);
                    var staff = context.TBL_STAFF.Find(appDetails.CREATEDBY);
                    var customerName = context.TBL_CUSTOMER.Where(x => x.CUSTOMERID == appDetails.CUSTOMERID).Select(x => x.FIRSTNAME + " " + x.MIDDLENAME + " " + x.LASTNAME).FirstOrDefault();
                    var accountOfficerName = context.TBL_STAFF.Where(x => x.STAFFID == appDetails.CREATEDBY).Select(x => x.FIRSTNAME + " " + x.MIDDLENAME + " " + x.LASTNAME).FirstOrDefault();
                    var accountOfficerEmail = context.TBL_STAFF.Where(x => x.STAFFID == appDetails.CREATEDBY).Select(x => x.EMAIL).FirstOrDefault();
                    var rmEmail = context.TBL_STAFF.Where(x => x.STATEID == staff.SUPERVISOR_STAFFID).Select(x => x.EMAIL).FirstOrDefault();
                    //var previousInsurancePolicyDetails = context.TBL_COLLATERAL_INSURANCE_TRACKING.Where(d => d.COLLATERALCUSTOMERID == i.COLLATERALCUSTOMERID && DbFunctions.TruncateTime(d.INSURANCEENDDATE) < DbFunctions.TruncateTime(DateTime.UtcNow)).OrderBy(d=>d.INSURANCEENDDATE).ToList();

                    var insurancePolicyType = i.INSURANCEPOLICYTYPEID.Value == 0 ? i.OTHERINSURANCEPOLICYTYPE : context.TBL_INSURANCE_POLICY_TYPE.Where(o => o.POLICYTYPEID == i.INSURANCEPOLICYTYPEID).Select(o => o.DESCRIPTION).FirstOrDefault();

                    var omv = string.Format("{0:#,##.00}", Convert.ToDecimal(i.OMV));
                    var sumInsured = string.Format("{0:#,##.00}", Convert.ToDecimal(i.SUMINSURED));
                    var premium = string.Format("{0:#,##.00}", Convert.ToDecimal(i.PREMIUMPAID));
                    var expiryDate = i.INSURANCEENDDATE?.ToString("dd-MM-yyyy");
                    int numberOfDays = (i.INSURANCEENDDATE.Value - DateTime.Now).Days;



                    var result = $@"
                     <table cellpadding='0' cellspacing='0' border='1' width='800px'>
                        <tr>
                            <td><b>Collateral Detail</b></td>
                            <td><b>Open Market Value</b></td>
                            <td><b>Sum Insured</b></td>
                            <td><b>Premium</b></td>
                            <td><b>Insurance Expiry Date</b></td>
                            <td><b>Insurance Policy Type</b></td>
                        </tr>";

                        result = result + $@"
                        <tr>
                            <td>{i.COLLATERALDETAILS}</td>
                            <td>{$"{omv}"}</td>
                            <td>{$"{sumInsured}"}</td>
                            <td>{$"{premium}"}</td>
                            <td>{expiryDate}</td>
                            <td>{insurancePolicyType}</td>
                        </tr>";

                        result = result + $"</table>";

                            alertTitle = alertTitle.Replace("@{{customerName}}", customerName);
                            alertTemplate = alertTemplate.Replace("@{{accountOfficerName}}", accountOfficerName);
                            alertTemplate = alertTemplate.Replace("@{{customerName}}", customerName);
                            alertTemplate = alertTemplate.Replace("@{{daysToExpire}}", numberOfDays.ToString());
                            alertTemplate = alertTemplate.Replace("@{{detail}}", result);
                            emailList = rmEmail + ";" + accountOfficerEmail;
                            emailList = emailList + defaultEmail;
                            alert.receiverEmailList.Add(emailList);
                            alert.template = alertTemplate;
                            alert.alertTitle = alertTitle;
                            alert.canFire = true;
                            alert.operationMethod = alertTitleInfo.BINDINGMETHOD;
                            alerts.Add(alert);
                }
                SendAlertNotification(alerts);
            }
        }


        public void UpdateInsurancePolicyStatus()
        {
            var insurancePolicyStatus = context.TBL_COLLATERAL_INSURANCE_TRACKING.Where(d => DbFunctions.TruncateTime(d.INSURANCEENDDATE) < DbFunctions.TruncateTime(DateTime.UtcNow) && d.DELETED == false && d.INSURANCESTATUSID != (int)InsuranceStatusEnum.Expired).ToList();
            if (insurancePolicyStatus.Count() > 0)
            {
                foreach(var i in insurancePolicyStatus)
                {
                    i.INSURANCESTATUSID = (int)InsuranceStatusEnum.Expired;
                }
                context.SaveChanges();
            }
        }

            //public bool ProcessLoanArchive()
            //{
            //    return loanArchive.ProcessLoanArchieving();

            //}
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
                        var maturityDate = t.MATURITYDATE?.ToString("dd-MM-yyyy");

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
                    alert.operationMethod = alertTitleInfo.BINDINGMETHOD;
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

                    emailList = emailList + GetAllStaffRoleEmails(alertTitleInfo.ALERTTITLEID) + defaultEmail;
                    alert.receiverEmailList.Add(emailList);
                    alert.template = alertTemplate;
                    alert.alertTitle = alertTitle;
                    alert.canFire = true;
                    alert.operationMethod = alertTitleInfo.BINDINGMETHOD;
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

                    emailList = emailList + GetAllStaffRoleEmails(alertTitleInfo.ALERTTITLEID) + defaultEmail;
                    alert.receiverEmailList.Add(emailList);
                    alert.template = alertTemplate;
                    alert.alertTitle = alertTitle;
                    alert.canFire = true;
                    alert.operationMethod = alertTitleInfo.BINDINGMETHOD;
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

                    emailList = i.customerName + defaultEmail;
                    alert.receiverEmailList.Add(emailList);
                    alert.template = alertTemplate;
                    alert.alertTitle = alertTitle;
                    alert.canFire = true;
                    alert.operationMethod = alertTitleInfo.BINDINGMETHOD;
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
                    var accountNumbers = context.TBL_GLOBAL_EXPOSURE.Where(d => days.Contains(DbFunctions.DiffDays(DateTime.UtcNow, d.MATURITYDATE).Value)
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
                        var maturityDate = t.MATURITYDATE?.ToString("dd-MM-yyyy");
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
                    alertTemplate = alertTemplate.Replace("@{{accountNumber}}", result);

                    emailList = emailList + GetAllStaffRoleEmails(alertTitleInfo.ALERTTITLEID) + defaultEmail;
                    alert.receiverEmailList.Add(emailList);
                    alert.template = alertTemplate;
                    alert.alertTitle = alertTitle;
                    alert.canFire = true;
                    alert.operationMethod = alertTitleInfo.BINDINGMETHOD;
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
                    alert.operationMethod = alertTitleInfo.BINDINGMETHOD;
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
                        var maturityDate = t.MATURITYDATE?.ToString("dd-MM-yyyy");
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
                    alert.operationMethod = alertTitleInfo.BINDINGMETHOD;
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
                        var maturityDate = t.MATURITYDATE?.ToString("dd-MM-yyyy");
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
                    alert.operationMethod = alertTitleInfo.BINDINGMETHOD;
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
                    if (staffFullName == "vacant" || staffFullName == "")
                    {
                        staffFullName = context.TBL_STAFF.Where(b => b.STAFFCODE == staff.misCode).Select(b => b.FIRSTNAME + "" + b.MIDDLENAME + "" + b.LASTNAME).FirstOrDefault();
                    }
                    emailList = GetBusinessUsersEmails(staff.misCode);

                    var loanInformation = context.TBL_GLOBAL_EXPOSURE.Where(d => d.UNPODAYSOVERDUE > 0 && d.ACCOUNTOFFICERCODE == staff.misCode && d.TOTALUNPAIDOBLIGATION > 0).ToList();
                    if (loanInformation != null && loanInformation.Count() > 0)
                    {
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

                            var amount = string.Format("{0:#,##.00}", Convert.ToDecimal(t.TOTALUNPAIDOBLIGATION));
                            //var amount = Convert.ToDecimal(t.AMOUNTDUE).ToString(); 
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

                        if (result.Count() > 0 && alertTemplate.Replace("@{{accountNumbers}}", result).Count() > 0)
                        {
                            alertTemplate = alertTemplate.Replace("@{{accountOfficerName}}", staffFullName);
                            alertTemplate = alertTemplate.Replace("@{{accountNumbers}}", result);

                            emailList = emailList + GetAllStaffRoleEmails(alertTitleInfo.ALERTTITLEID) + defaultEmail;
                            alert.receiverEmailList.Add(emailList);
                            alert.template = alertTemplate;
                            alert.alertTitle = alertTitle;
                            alert.canFire = true;
                            alert.operationMethod = alertTitleInfo.BINDINGMETHOD;

                            alerts.Add(alert);
                        }
                    }
                }
                if (alerts.Count() > 0)
                {
                    SendAlertNotification(alerts);
                }
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


            var emailList = GetAllStaffRoleEmails(alertTitleInfo.ALERTTITLEID) + defaultEmail;

            alert.receiverEmailList.Add(emailList);
            alert.template = alertTemplate;
            alert.alertTitle = alertTitle;
            alert.canFire = true;
            alert.operationMethod = alertTitleInfo.BINDINGMETHOD;
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
            alert.operationMethod = alertTitleInfo.BINDINGMETHOD;
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
            alert.operationMethod = alertTitleInfo.BINDINGMETHOD;
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
                        var maturityDate = t.MATURITYDATE?.ToString("dd-MM-yyyy");

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
                    alert.operationMethod = alertTitleInfo.BINDINGMETHOD;
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
                        totalPastDue = totalPastDue + Convert.ToDecimal(t.AMOUNTDUE);
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
                    alert.operationMethod = alertTitleInfo.BINDINGMETHOD;
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
                    alert.operationMethod = alertTitleInfo.BINDINGMETHOD;
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
                        var scheduleDueDate = t.SCHEDULEDUEDATE?.ToString("dd-MM-yyyy");
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
                    alert.operationMethod = alertTitleInfo.BINDINGMETHOD;
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
                    alert.operationMethod = alertTitleInfo.BINDINGMETHOD;
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
                    alert.operationMethod = alertTitleInfo.BINDINGMETHOD;
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
                        var maturityDate = t.MATURITYDATE?.ToString("dd-MM-yyyy");

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
                    alert.operationMethod = alertTitleInfo.BINDINGMETHOD;
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
                        var maturityDate = t.MATURITYDATE?.ToString("dd-MM-yyyy");

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
                    alert.operationMethod = alertTitleInfo.BINDINGMETHOD;
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
                        var maturityDate = t.MATURITYDATE?.ToString("dd-MM-yyyy");

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
                    alert.operationMethod = alertTitleInfo.BINDINGMETHOD;
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
                    alert.operationMethod = alertTitleInfo.BINDINGMETHOD;
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
                    alert.operationMethod = alertTitleInfo.BINDINGMETHOD;
                    alerts.Add(alert);
                }
                SendAlertNotification(alerts);
            }
        }

        public void GetStaffLoanPortfolioReport()
        {
            // GetStaffLoanPortfolioReport method
            var staffList = externalAlertRepository.GetStaffLoanPortfolioReport();
            var alertTitleInfo = context.TBL_ALERT_TITLE.Where(a => a.BINDINGMETHOD == "GetStaffLoanPortfolioReport").FirstOrDefault();

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
                    emailList = GetBusinessUsersEmailsToGroupHead(staff.misCode);

                    List<int> days = new List<int> { 21, 14, 7, 3, 1 };
                    var loanInformation = context.TBL_GLOBAL_EXPOSURE.Where(d => days.Contains(DbFunctions.DiffDays(DateTime.UtcNow, d.MATURITYDATE).Value) && d.ACCOUNTOFFICERCODE == staff.misCode && d.PRINCIPALOUTSTANDINGBALLCY > 0).ToList();

                    if (loanInformation != null && loanInformation.Count() > 0)
                    {

                        foreach (var t in loanInformation)
                        {
                            var amount = string.Format("{0:#,##.00}", Convert.ToDecimal(t.PRINCIPALOUTSTANDINGBALLCY));
                            var maturityDate = t.MATURITYDATE?.ToString("dd-MM-yyyy");
                            int numberOfDays = (t.MATURITYDATE.Value - DateTime.Now).Days;
                            var customerName = t.CUSTOMERNAME;

                            if (t.PRINCIPALOUTSTANDINGBALLCY > 0)
                            {
                                alertTemplate = alertTemplate.Replace("@{{customerName}}", customerName);
                                alertTemplate = alertTemplate.Replace("@{{days}}", numberOfDays.ToString());

                                emailList = emailList + defaultEmail + GetAllCreditPortfolioStaffEmails();
                                alert.receiverEmailList.Add(emailList);
                                alert.template = alertTemplate;
                                alert.alertTitle = alertTitle;
                                alert.canFire = true;
                                alert.operationMethod = alertTitleInfo.BINDINGMETHOD;

                                alerts.Add(alert);
                            }
                        }

                    }
                }

                if (alerts.Count() > 0)
                {
                    SendAlertNotification(alerts);
                }
            }
        }
        public void GetFacilityRestructuredNotification()
        {
            // GetFacilityRestructuredNotification method
            var facilitiesRestructured = externalAlertRepository.GetFacilityRestructuredNotification();
            var alertTitleInfo = context.TBL_ALERT_TITLE.Where(a => a.BINDINGMETHOD == "GetFacilityRestructuredNotification").FirstOrDefault();

            var defaultEmail = "";
            if (alertTitleInfo.DEFAULTEMAIL != null)
            {
                defaultEmail = ";" + alertTitleInfo.DEFAULTEMAIL;
            }
            if (facilitiesRestructured != null && facilitiesRestructured.Count() > 0)
            {

                List<AlertsViewModel> alerts = new List<AlertsViewModel>();
                foreach (var facilityRestructured in facilitiesRestructured)
                {
                    AlertsViewModel alert = new AlertsViewModel();
                    var alertTitle = alertTitleInfo.TITLE;
                    var alertTemplate = alertTitleInfo.TEMPLATE;
                    string emailList = "";
                    var staff = context.TBL_STAFF.Find(facilityRestructured.CREATEDBY);
                    emailList = GetBusinessUsersEmailsToGroupHead(staff.MISCODE);

                    var loanInformation = context.TBL_LOAN_REVIEW_OPERATION.Where(d => d.LOANID == facilityRestructured.LOANID && d.OPERATIONCOMPLETED == true && d.APPROVALSTATUSID == (int)ApprovalStatusEnum.Approved).ToList();

                    if (loanInformation != null && loanInformation.Count() > 1)
                    {
                        var staffFullName = staff.FIRSTNAME + " " + staff.MIDDLENAME + " " + staff.LASTNAME;
                        var times = loanInformation.Count();
                        var loanDtail = context.TBL_LMSR_APPLICATION_DETAIL.Find(facilityRestructured.LOANREVIEWAPPLICATIONID);
                        var facility = context.TBL_PRODUCT.Find(loanDtail.PRODUCTID).PRODUCTNAME;
                        alertTemplate = alertTemplate.Replace("@{{accountOfficer}}", staffFullName);
                        alertTemplate = alertTemplate.Replace("@{{facility}}", facility);
                        alertTemplate = alertTemplate.Replace("@{{times}}", times.ToString());

                        emailList = emailList + defaultEmail + GetAllCreditPortfolioStaffEmails();
                        alert.receiverEmailList.Add(emailList);
                        alert.template = alertTemplate;
                        alert.alertTitle = alertTitle;
                        alert.canFire = true;
                        alert.operationMethod = alertTitleInfo.BINDINGMETHOD;

                        alerts.Add(alert);
                    }
                }

                if (alerts.Count() > 0)
                {
                    SendAlertNotification(alerts);
                }
            }
        }
        public void GetSLAReport()
        {
            // GetSLAReport method
            var pendingLoans = externalAlertRepository.GetSLAReport();
            var alertTitleInfo = context.TBL_ALERT_TITLE.Where(a => a.BINDINGMETHOD == "GetSlaReport").FirstOrDefault();

            var defaultEmail = "";
            if (alertTitleInfo.DEFAULTEMAIL != null)
            {
                defaultEmail = ";" + alertTitleInfo.DEFAULTEMAIL;
            }
            if (pendingLoans != null && pendingLoans.Count() > 0)
            {
                List<AlertsViewModel> alerts = new List<AlertsViewModel>();

                foreach (var pendingLoan in pendingLoans)
                {
                    pendingLoan.slaGlobalStatus = externalAlertRepository.GetSlaGlobalStatus(pendingLoan);
                    pendingLoan.slaInduvidualStatus = externalAlertRepository.GetSlaInduvidualStatus(pendingLoan);

                    AlertsViewModel alert = new AlertsViewModel();
                    var alertTitle = alertTitleInfo.TITLE;
                    var alertTemplate = alertTitleInfo.TEMPLATE;
                    string emailList = "";
                    var staff = context.TBL_STAFF.Find(pendingLoan.createdBy);
                    emailList = GetBusinessUsersEmailsToGroupHead(staff.MISCODE);

                    if (pendingLoan.slaGlobalStatus == "warning" || pendingLoan.slaGlobalStatus == "danger")
                    {
                        var staffFullName = staff.FIRSTNAME + " " + staff.MIDDLENAME + " " + staff.LASTNAME;
                        alertTemplate = alertTemplate.Replace("@{{accountOfficer}}", staffFullName);
                        alertTemplate = alertTemplate.Replace("@{{customerName}}", pendingLoan.customerName);
                        alertTemplate = alertTemplate.Replace("@{{facility}}", pendingLoan.facility);
                        alertTemplate = alertTemplate.Replace("@{{hours}}", pendingLoan.currentApprovalLevelSlaInterval.ToString());

                        emailList = emailList + defaultEmail + GetAllCreditPortfolioStaffEmails();
                        alert.receiverEmailList.Add(emailList);
                        alert.template = alertTemplate;
                        alert.alertTitle = alertTitle;
                        alert.canFire = true;
                        alert.operationMethod = alertTitleInfo.BINDINGMETHOD;

                        alerts.Add(alert);
                    }
                }

                if (alerts.Count() > 0)
                {
                    SendAlertNotification(alerts);
                }
            }
        }
        public void GetPastDueDeferredDocuments()
        {
            // GetPastDueDeferredDocuments method
            var pastDueDeferredDocuments = externalAlertRepository.GetPastDueDeferredDocuments();
            var alertTitleInfo = context.TBL_ALERT_TITLE.Where(a => a.BINDINGMETHOD == "GetPastDueDeferredDocuments").FirstOrDefault();

            var defaultEmail = "";
            if (alertTitleInfo.DEFAULTEMAIL != null)
            {
                defaultEmail = alertTitleInfo.DEFAULTEMAIL;
            }
            if (pastDueDeferredDocuments != null && pastDueDeferredDocuments.Count() > 0)
            {
                AlertsViewModel alert = new AlertsViewModel();
                List<AlertsViewModel> alerts = new List<AlertsViewModel>();
                var result = string.Empty;
                var tempResult = string.Empty;
                var alertTemplate = alertTitleInfo.TEMPLATE;
                var alertTitle = alertTitleInfo.TITLE;
                string emailList = "";
                var n = 0;

                tempResult = $@"
                        <h3><b>PAST DUE DEFERRED DOCUMENTS RECORDS</b></h3>
                        <table cellpadding='0' cellspacing='0' border='1' width='800px'>
                        <tr>
                            <td><b>S/N</b></td>
                            <td><b>Customer Name</b></td>
                            <td><b>Condition</b></td>
                            <td><b>Deferred Date</b></td>
                            <td><b>Reason</b></td>
                        </tr>
                        ";

                foreach (var t in pastDueDeferredDocuments)
                {
                    n++;
                    var deferredDate = t.deferredDate?.ToString("dd-MM-yyyy");
                    tempResult = tempResult + $@"
                                    <tr>
                                    <td>{n}</td>
                                    <td>{t.customerName}</td>
                                    <td>{t.condition}</td>
                                    <td>{$"{deferredDate}"}</td>
                                    <td>{t.reason}</td>
                                </tr>
                                ";
                }

                tempResult = tempResult + $"</table><br/>";
                result = tempResult;


                if (result.Count() > 0 && alertTemplate.Replace("@{{accounts}}", result).Count() > 0)
                {
                    alertTemplate = alertTemplate.Replace("@{{accounts}}", result);
                    emailList = defaultEmail + GetAllCreditPortfolioStaffEmails();
                    alert.receiverEmailList.Add(emailList);
                    alert.template = alertTemplate;
                    alert.alertTitle = alertTitle;
                    alert.canFire = true;
                    alert.operationMethod = alertTitleInfo.BINDINGMETHOD;

                    alerts.Add(alert);
                }

                if (alerts.Count() > 0)
                {
                    SendAlertNotification(alerts);
                }
            }
        }
        public void GetLoanRepaymentReminder()
        {
            // GetLoanRepaymentReminder method
            var loanRepaymentReminder = externalAlertRepository.GetLoanRepaymentReminder();
            var alertTitleInfo = context.TBL_ALERT_TITLE.Where(a => a.BINDINGMETHOD == "GetLoanRepaymentReminder").FirstOrDefault();
            int numberOfDays = 0;
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

                    numberOfDays = (i.maturityDate.Value - DateTime.Now).Days;
                    AlertsViewModel alert = new AlertsViewModel();
                    var alertTitle = alertTitleInfo.TITLE;
                    var alertTemplate = alertTitleInfo.TEMPLATE;
                    if (numberOfDays > 0)
                    {
                        string emailList = "";
                        alertTemplate = alertTemplate.Replace("@{{customerName}}", i.customerName);
                        alertTemplate = alertTemplate.Replace("@{{maturityBand}}", numberOfDays.ToString());

                        emailList = defaultEmail;
                        alert.receiverEmailList.Add(emailList);
                        alert.template = alertTemplate;
                        alert.alertTitle = alertTitle;
                        alert.canFire = true;
                        alert.operationMethod = alertTitleInfo.BINDINGMETHOD;
                        alerts.Add(alert);
                    }
                }
                SendAlertNotification(alerts);
            }
        }
        public void GetExpiredInsurancePolicies()
        {
            // GetExpiredInsurancePolicies method
            var expiredInsurancePolicies = externalAlertRepository.GetExpiredInsurancePolicies();
            var alertTitleInfo = context.TBL_ALERT_TITLE.Where(a => a.BINDINGMETHOD == "GetExpiredInsurancePolicies").FirstOrDefault();

            var defaultEmail = "";
            if (alertTitleInfo.DEFAULTEMAIL != null)
            {
                defaultEmail = alertTitleInfo.DEFAULTEMAIL;
            }
            if (expiredInsurancePolicies != null && expiredInsurancePolicies.Count() > 0)
            {
                AlertsViewModel alert = new AlertsViewModel();
                List<AlertsViewModel> alerts = new List<AlertsViewModel>();
                var result = string.Empty;
                var tempResult = string.Empty;
                var alertTemplate = alertTitleInfo.TEMPLATE;
                var alertTitle = alertTitleInfo.TITLE;
                string emailList = "";
                var n = 0;

                tempResult = $@"
                        <h3><b>EXPIRED INSURANCE POLICIES RECORDS</b></h3>
                        <table cellpadding='0' cellspacing='0' border='1' width='800px'>
                        <tr>
                            <td><b>S/N</b></td>
                            <td><b>Customer Name</b></td>
                            <td><b>Reference Nummber</b></td>
                            <td><b>Start Date</b></td>
                            <td><b>End Date</b></td>
                            <td><b>Insurance Company</b></td>
                            <td><b>Description</b></td>
                            <td><b>Insurance Type</b></td>
                            <td><b>Account Officer</b></td>
                        </tr>
                        ";

                foreach (var t in expiredInsurancePolicies)
                {
                    n++;
                    var startDate = t.startDate?.ToString("dd-MM-yyyy");
                    var endDate = t.expiryDate?.ToString("dd-MM-yyyy");
                    tempResult = tempResult + $@"
                                    <tr>
                                    <td>{n}</td>
                                    <td>{t.customerName}</td>
                                    <td>{t.referenceNumber}</td>
                                    <td>{$"{startDate}"}</td>
                                    <td>{$"{endDate}"}</td>
                                    <td>{t.insuranceCompany}</td>
                                    <td>{t.description}</td>
                                    <td>{t.insuranceType}</td>
                                    <td>{t.accountOfficer}</td>
                                </tr>
                                ";
                }

                tempResult = tempResult + $"</table><br/>";
                result = tempResult;


                if (result.Count() > 0 && alertTemplate.Replace("@{{policies}}", result).Count() > 0)
                {
                    alertTemplate = alertTemplate.Replace("@{{policies}}", result);
                    emailList = defaultEmail + GetAllCreditPortfolioStaffEmails();
                    alert.receiverEmailList.Add(emailList);
                    alert.template = alertTemplate;
                    alert.alertTitle = alertTitle;
                    alert.canFire = true;
                    alert.operationMethod = alertTitleInfo.BINDINGMETHOD;

                    alerts.Add(alert);
                }

                if (alerts.Count() > 0)
                {
                    SendAlertNotification(alerts);
                }
            }
        }
        public void GetValuationReminder()
        {
            // GetValuationReminder method
            var valuationReminder = externalAlertRepository.GetValuationReminder();
            var alertTitleInfo = context.TBL_ALERT_TITLE.Where(a => a.BINDINGMETHOD == "GetValuationReminder").FirstOrDefault();
            int numberOfDays = 0;
            var defaultEmail = "";
            if (alertTitleInfo.DEFAULTEMAIL != null)
            {
                defaultEmail = ";" + alertTitleInfo.DEFAULTEMAIL;
            }

            if (valuationReminder != null && valuationReminder.Count() > 0)
            {

                List<int> days = new List<int> { 60, 30, 21, 14, 7, 3, 1 };
                var dueValuation = valuationReminder.Where(f => days.Contains((f.nextValuationDate - DateTime.Now).Days)).ToList();
                List<AlertsViewModel> alerts = new List<AlertsViewModel>();
                if (dueValuation.Count() > 0)
                {
                    foreach (var i in dueValuation)
                    {

                        numberOfDays = (i.nextValuationDate - DateTime.Now).Days;
                        AlertsViewModel alert = new AlertsViewModel();
                        var alertTitle = alertTitleInfo.TITLE;
                        var alertTemplate = alertTitleInfo.TEMPLATE;
                        if (numberOfDays > 0)
                        {
                            string emailList = "";
                            alertTemplate = alertTemplate.Replace("@{{customerName}}", i.customerName);
                            alertTemplate = alertTemplate.Replace("@{{accountOfficerName}}", i.accountOfficerName);
                            alertTemplate = alertTemplate.Replace("@{{collateralCode}}", i.collateralCode);
                            alertTemplate = alertTemplate.Replace("@{{days}}", numberOfDays.ToString());

                            emailList = defaultEmail + ";" + GetBusinessUsersEmailsToGroupHead(i.accountOfficerCode);
                            alert.receiverEmailList.Add(emailList);
                            alert.template = alertTemplate;
                            alert.alertTitle = alertTitle;
                            alert.canFire = true;
                            alert.operationMethod = alertTitleInfo.BINDINGMETHOD;
                            alerts.Add(alert);
                        }
                    }
                }
                if (alerts.Count() > 0)
                {
                    SendAlertNotification(alerts);
                }
            }
        }
        public void GetSiteVisitationAccountReminder()
        {
            // GetSiteVisitationAccountReminder method
            var visitationReminder = externalAlertRepository.GetSiteVisitationAccountReminder();
            var alertTitleInfo = context.TBL_ALERT_TITLE.Where(a => a.BINDINGMETHOD == "GetSiteVisitationAccountReminder").FirstOrDefault();
            int numberOfDays = 0;
            var defaultEmail = "";
            if (alertTitleInfo.DEFAULTEMAIL != null)
            {
                defaultEmail = ";" + alertTitleInfo.DEFAULTEMAIL;
            }

            if (visitationReminder != null && visitationReminder.Count() > 0)
            {
                List<int> days = new List<int> { 60, 30, 21, 14, 7, 3, 1 };
                var dueVisitation = visitationReminder.Where(f => days.Contains((f.nextVisitationDate - DateTime.Now).Days)).ToList();
                List<AlertsViewModel> alerts = new List<AlertsViewModel>();
                if (dueVisitation.Count() > 0)
                {
                    foreach (var i in dueVisitation)
                    {

                        numberOfDays = (i.nextValuationDate - DateTime.Now).Days;
                        AlertsViewModel alert = new AlertsViewModel();
                        var alertTitle = alertTitleInfo.TITLE;
                        var alertTemplate = alertTitleInfo.TEMPLATE;
                        if (numberOfDays > 0)
                        {
                            string emailList = "";
                            alertTemplate = alertTemplate.Replace("@{{customerName}}", i.customerName);
                            alertTemplate = alertTemplate.Replace("@{{accountOfficerName}}", i.accountOfficerName);
                            alertTemplate = alertTemplate.Replace("@{{collateralDescription}}", i.collateralSummary);
                            alertTemplate = alertTemplate.Replace("@{{location}}", i.propertyAddress);
                            alertTemplate = alertTemplate.Replace("@{{days}}", numberOfDays.ToString());

                            emailList = defaultEmail + ";" + GetBusinessUsersEmailsToGroupHead(i.accountOfficerCode);
                            alert.receiverEmailList.Add(emailList);
                            alert.template = alertTemplate;
                            alert.alertTitle = alertTitle;
                            alert.canFire = true;
                            alert.operationMethod = alertTitleInfo.BINDINGMETHOD;
                            alerts.Add(alert);
                        }
                    }
                }
                if (alerts.Count() > 0)
                {
                    SendAlertNotification(alerts);
                }
            }
        }
        public void GetExpiredValuationReport()
        {
            // GetExpiredValuationReport method
            var expiredValuationReport = externalAlertRepository.GetExpiredValuationReport();
            var alertTitleInfo = context.TBL_ALERT_TITLE.Where(a => a.BINDINGMETHOD == "GetExpiredValuationReport").FirstOrDefault();

            var defaultEmail = "";
            if (alertTitleInfo.DEFAULTEMAIL != null)
            {
                defaultEmail = alertTitleInfo.DEFAULTEMAIL;
            }
            if (expiredValuationReport != null && expiredValuationReport.Count() > 0)
            {
                AlertsViewModel alert = new AlertsViewModel();
                List<AlertsViewModel> alerts = new List<AlertsViewModel>();
                var result = string.Empty;
                var tempResult = string.Empty;
                var alertTemplate = alertTitleInfo.TEMPLATE;
                var alertTitle = alertTitleInfo.TITLE;
                string emailList = "";
                var n = 0;

                var expiredValuation = expiredValuationReport.Where(f => (f.nextValuationDate - DateTime.Now).Days <= 0).ToList();

                tempResult = $@"
                        <h3><b>EXPIRED VALUATION REPORTS</b></h3>
                        <table cellpadding='0' cellspacing='0' border='1' width='800px'>
                        <tr>
                            <td><b>S/N</b></td>
                            <td><b>DESCRIPTION</b></td>
                            <td><b>EXPIRATION DATE</b></td>
                            <td><b>CUSTOMER NAME</b></td>
                            <td><b>RELATIONSHIP MANAGER</b></td>
                        </tr>
                        ";
                if (expiredValuation.Count() > 0)
                {
                    foreach (var t in expiredValuation)
                    {
                        n++;
                        var expiredDate = t.nextValuationDate.ToString("dd-MM-yyyy");
                        tempResult = tempResult + $@"
                                    <tr>
                                    <td>{n}</td>
                                    <td>{t.collateralSummary}</td>
                                    <td>{$"{expiredDate}"}</td>
                                    <td>{t.customerName}</td>
                                    <td>{t.accountOfficerName}</td>
                                </tr>
                                ";
                    }
                }
                tempResult = tempResult + $"</table><br/>";
                result = tempResult;


                if (result.Count() > 0 && alertTemplate.Replace("@{{valuationList}}", result).Count() > 0)
                {
                    alertTemplate = alertTemplate.Replace("@{{valuationList}}", result);
                    emailList = defaultEmail;
                    alert.receiverEmailList.Add(emailList);
                    alert.template = alertTemplate;
                    alert.alertTitle = alertTitle;
                    alert.canFire = true;
                    alert.operationMethod = alertTitleInfo.BINDINGMETHOD;

                    alerts.Add(alert);
                }

                if (alerts.Count() > 0)
                {
                    SendAlertNotification(alerts);
                }
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
                var title = alert.alertTitle.Trim();
                if (title.Contains("&"))
                {
                    title = title.Replace("&", "AND");
                }
                if (title.Contains("."))
                {
                    title = title.Replace(".", "");
                }
                if (alert.canFire) LogEmailAlert(alert.template, alert.alertTitle, alert.receiverEmailList, "100442", 0, alert.operationMethod);
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
                if (condition != null)
                {
                    if (systemDate.Date == condition.NEXTRUNDATE) return true; else return false;
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
            var roleEmail = (from r in context.TBL_ALERT_GROUP_EMAIL
                             join t in context.TBL_ALERT_STAFF_ROLE on r.GROUPEMAILID equals t.STAFFROLEID
                             where t.ALERTTITLEID == alerttitleId
                             select new simpleStaffModel
                             {
                                 staffCode = r.GROUPCODE,
                                 staffRoleId = r.GROUPEMAILID,
                                 email = r.GROUPEMAIL,
                             }).ToList();

            foreach (var t in roleEmail)
            {
                list = list + ";" + t.email;
            }
            return list;
        }
        public string GetAllDivisionHeadsEmails(string regionCode)
        {
            var list = "";
            var regionEmails = externalAlertRepository.GetDivisionalOfficersByGroupHeads(regionCode);

            foreach (var t in regionEmails)
            {
                list = list + ";" + t.Email;
                if (t.misCode == "IBG800")
                {
                    list = list + ";ogbonnar@accessbankplc.com;Soji-OkusanyaI@accessbankplc.com";
                }
            }
            return list;
        }
        public void LogEmailAlert(string messageBody, string alertSubject, List<string> recipients, string referenceCode, int targetId, string operationMehtod)
        {
            try
            {
                var title = alertSubject.Trim();
                if (title.Contains("&"))
                {
                    title = title.Replace("&", "AND");
                }
                if (title.Contains("."))
                {
                    title = title.Replace(".", "");
                }
                string recipient = string.Join("", recipients.ToArray());
                string messageSubject = title;
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
                    operationMethod = operationMehtod,
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
                TARGETID = (int)model.targetId,
                OPERATIONMETHOD = model.operationMethod
            };

            context.TBL_MESSAGE_LOG.Add(message);
            context.SaveChanges();

        }

        public string GetAllCreditPortfolioStaffEmails()
        {
            var list = "";
            var role = context.TBL_STAFF_ROLE.Where(r => r.STAFFROLECODE == "CP").FirstOrDefault();
            var roleEmail = context.TBL_STAFF.Where(s => s.STAFFROLEID == role.STAFFROLEID).ToList();

            foreach (var t in roleEmail)
            {
                list = list + ";" + t.EMAIL;
            }
            return list;
        }
        #endregion logic codes

        // trigger alerts methods
        public void GetDigitalLoanExceptionNPLIncrease()
        {
            var exceptionNPLs = (from a in context2.TBL_TRIGGER_MEASURES_PRODUCT select a).ToList();
            List<AlertsViewModel> alerts = new List<AlertsViewModel>();

            if (exceptionNPLs != null && exceptionNPLs.Count() > 0)
            {
                foreach (var exceptionNPL in exceptionNPLs)
                {
                    AlertsViewModel alert = new AlertsViewModel();
                    TBL_ALERT_TITLE alertTemplate = new TBL_ALERT_TITLE();
                    string emailList = "";
                    var template = "";

                    if (exceptionNPL.NPL >= (decimal)onePercentValue && exceptionNPL.NPL < (decimal)onePointFivePercentValue)
                    {
                        alertTemplate = context.TBL_ALERT_TITLE.Where(a => a.BINDINGMETHOD == "GetDigitalLoanExceptionNPLIncreaseByOnePercent").FirstOrDefault();
                        template = alertTemplate.TEMPLATE;
                        template = template.Replace("@{{percentage}}", onePercent);
                        template = template.Replace("@{{productName}}", exceptionNPL.PRODUCTNAME);
                        emailList = alertTemplate.DEFAULTEMAIL;
                    }

                    if (exceptionNPL.NPL >= (decimal)onePointFivePercentValue && exceptionNPL.NPL < (decimal)twoPercentValue)
                    {
                        alertTemplate = context.TBL_ALERT_TITLE.Where(a => a.BINDINGMETHOD == "GetDigitalLoanExceptionNPLIncreaseByOnePointFivePercent").FirstOrDefault();
                        template = alertTemplate.TEMPLATE;
                        template = template.Replace("@{{percentage}}", onePointFivePercent);
                        template = template.Replace("@{{productName}}", exceptionNPL.PRODUCTNAME);
                        emailList = alertTemplate.DEFAULTEMAIL;
                    }
                    if (exceptionNPL.NPL >= (decimal)twoPercentValue)
                    {
                        alertTemplate = context.TBL_ALERT_TITLE.Where(a => a.BINDINGMETHOD == "GetDigitalLoanExceptionNPLIncreaseByTwoPercent").FirstOrDefault();
                        template = alertTemplate.TEMPLATE;
                        template = template.Replace("@{{percentage}}", twoPercent);
                        template = template.Replace("@{{productName}}", exceptionNPL.PRODUCTNAME);
                        emailList = alertTemplate.DEFAULTEMAIL;
                    }
                    if ((exceptionNPL.NPL >= (decimal)onePercentValue && exceptionNPL.NPL < (decimal)onePointFivePercentValue)
                       || (exceptionNPL.NPL >= (decimal)onePointFivePercentValue && exceptionNPL.NPL < (decimal)twoPercentValue) || (exceptionNPL.NPL >= (decimal)twoPercentValue))
                    {
                        var email = "Chibuike.Mbanefo@ACCESSBANKPLC.com;PAUL.ASIEMO@accessbankplc.com;OLUKAYODE.AJAYI@ACCESSBANKPLC.com";
                        alert.receiverEmailList.Add(email);
                        alert.template = template;
                        alert.alertTitle = alertTemplate.TITLE;
                        alert.canFire = true;
                        alert.operationMethod = alertTemplate.BINDINGMETHOD;
                        alerts.Add(alert);
                    }
                }

                if (alerts.Count() > 0)
                {
                    SendAlertNotification(alerts);
                }
            }
        }
        public void GetDigitalLoanExceptionNPLDecrease()
        {
            var exceptionNPLs = (from a in context2.TBL_TRIGGER_MEASURES_PRODUCT select a).ToList();
            List<AlertsViewModel> alerts = new List<AlertsViewModel>();

            if (exceptionNPLs != null && exceptionNPLs.Count() > 0)
            {
                foreach (var exceptionNPL in exceptionNPLs)
                {
                    TBL_ALERT_TITLE alertTemplate = new TBL_ALERT_TITLE();
                    AlertsViewModel alert = new AlertsViewModel();
                    var template = "";
                    string emailList = "";

                    if (exceptionNPL.NPL <= -(decimal)onePercentValue && exceptionNPL.NPL > -(decimal)onePointFivePercentValue)
                    {
                        alertTemplate = context.TBL_ALERT_TITLE.Where(a => a.BINDINGMETHOD == "GetDigitalLoanExceptionNPLDecreaseByOnePercent").FirstOrDefault();
                        template = alertTemplate.TEMPLATE;
                        template = template.Replace("@{{percentage}}", onePercent);
                        template = template.Replace("@{{productName}}", exceptionNPL.PRODUCTNAME);
                        emailList = alertTemplate.DEFAULTEMAIL;
                    }

                    if (exceptionNPL.NPL <= -(decimal)onePointFivePercentValue && exceptionNPL.NPL > -(decimal)twoPercentValue)
                    {
                        alertTemplate = context.TBL_ALERT_TITLE.Where(a => a.BINDINGMETHOD == "GetDigitalLoanExceptionNPLDecreaseByOnePointFivePercent").FirstOrDefault();
                        template = alertTemplate.TEMPLATE;
                        template = template.Replace("@{{percentage}}", onePointFivePercent);
                        template = template.Replace("@{{productName}}", exceptionNPL.PRODUCTNAME);
                        emailList = alertTemplate.DEFAULTEMAIL;
                    }
                    if (exceptionNPL.NPL <= -(decimal)twoPercentValue)
                    {
                        alertTemplate = context.TBL_ALERT_TITLE.Where(a => a.BINDINGMETHOD == "GetDigitalLoanExceptionNPLDecreaseByTwoPercent").FirstOrDefault();
                        template = alertTemplate.TEMPLATE;
                        template = template.Replace("@{{percentage}}", twoPercent);
                        template = template.Replace("@{{productName}}", exceptionNPL.PRODUCTNAME);
                        emailList = alertTemplate.DEFAULTEMAIL;
                    }

                    if ((exceptionNPL.NPL <= -(decimal)onePercentValue && exceptionNPL.NPL > -(decimal)onePointFivePercentValue)
                       || (exceptionNPL.NPL <= -(decimal)onePointFivePercentValue && exceptionNPL.NPL > -(decimal)twoPercentValue) || (exceptionNPL.NPL <= -(decimal)twoPercentValue))
                    {
                        var email = "Chibuike.Mbanefo@ACCESSBANKPLC.com;PAUL.ASIEMO@accessbankplc.com;OLUKAYODE.AJAYI@ACCESSBANKPLC.com";
                        alert.receiverEmailList.Add(email);
                        alert.template = template;
                        alert.alertTitle = alertTemplate.TITLE;
                        alert.canFire = true;
                        alert.operationMethod = alertTemplate.BINDINGMETHOD;
                        alerts.Add(alert);
                    }
                }
                if (alerts.Count() > 0)
                {
                    SendAlertNotification(alerts);
                }
            }
        }
        public void GetDigitalLoanExceptionNPLModuleIncrease()
        {
            var exceptionNPLs = (from a in context2.TBL_TRIGGER_MEASURES_MODULE select a).ToList();
            List<AlertsViewModel> alerts = new List<AlertsViewModel>();

            string emailList = "";

            if (exceptionNPLs != null && exceptionNPLs.Count() > 0)
            {
                foreach (var exceptionNPL in exceptionNPLs)
                {
                    TBL_ALERT_TITLE alertTemplate = new TBL_ALERT_TITLE();
                    AlertsViewModel alert = new AlertsViewModel();
                    var template = "";

                    if (exceptionNPL.NPL >= (decimal)onePercentValue && exceptionNPL.NPL < (decimal)onePointFivePercentValue)
                    {
                        alertTemplate = context.TBL_ALERT_TITLE.Where(a => a.BINDINGMETHOD == "GetDigitalLoanExceptionNPLModuleIncreaseByOnePercent").FirstOrDefault();
                        template = alertTemplate.TEMPLATE;
                        template = template.Replace("@{{percentage}}", onePercent);
                        template = template.Replace("@{{moduleName}}", exceptionNPL.MODULE);
                        emailList = alertTemplate.DEFAULTEMAIL;
                    }

                    if (exceptionNPL.NPL >= (decimal)onePointFivePercentValue && exceptionNPL.NPL < (decimal)twoPercentValue)
                    {
                        alertTemplate = context.TBL_ALERT_TITLE.Where(a => a.BINDINGMETHOD == "GetDigitalLoanExceptionNPLModuleIncreaseByOnePointFivePercent").FirstOrDefault();
                        template = alertTemplate.TEMPLATE;
                        template = template.Replace("@{{percentage}}", onePointFivePercent);
                        template = template.Replace("@{{moduleName}}", exceptionNPL.MODULE);
                        emailList = alertTemplate.DEFAULTEMAIL;
                    }
                    if (exceptionNPL.NPL >= (decimal)twoPercentValue)
                    {
                        alertTemplate = context.TBL_ALERT_TITLE.Where(a => a.BINDINGMETHOD == "GetDigitalLoanExceptionNPLModuleIncreaseByTwoPercent").FirstOrDefault();
                        template = alertTemplate.TEMPLATE;
                        template = template.Replace("@{{percentage}}", twoPercent);
                        template = template.Replace("@{{moduleName}}", exceptionNPL.MODULE);
                        emailList = alertTemplate.DEFAULTEMAIL;
                    }
                    if ((exceptionNPL.NPL >= (decimal)onePercentValue && exceptionNPL.NPL < (decimal)onePointFivePercentValue)
                       || (exceptionNPL.NPL >= (decimal)onePointFivePercentValue && exceptionNPL.NPL < (decimal)twoPercentValue) || (exceptionNPL.NPL >= (decimal)twoPercentValue))
                    {
                        var email = "Chibuike.Mbanefo@ACCESSBANKPLC.com;PAUL.ASIEMO@accessbankplc.com;OLUKAYODE.AJAYI@ACCESSBANKPLC.com";
                        alert.receiverEmailList.Add(email);
                        alert.template = template;
                        alert.alertTitle = alertTemplate.TITLE;
                        alert.canFire = true;
                        alert.operationMethod = alertTemplate.BINDINGMETHOD;
                        alerts.Add(alert);
                    }
                }

                if (alerts.Count() > 0)
                {
                    SendAlertNotification(alerts);
                }
            }
        }
        public void GetDigitalLoanExceptionNPLModuleDecrease()
        {
            var exceptionNPLs = (from a in context2.TBL_TRIGGER_MEASURES_MODULE select a).ToList();
            List<AlertsViewModel> alerts = new List<AlertsViewModel>();
            string emailList = "";

            if (exceptionNPLs != null && exceptionNPLs.Count() > 0)
            {
                foreach (var exceptionNPL in exceptionNPLs)
                {
                    TBL_ALERT_TITLE alertTemplate = new TBL_ALERT_TITLE();
                    AlertsViewModel alert = new AlertsViewModel();
                    var template = "";

                    if (exceptionNPL.NPL <= -(decimal)onePercentValue && exceptionNPL.NPL > -(decimal)onePointFivePercentValue)
                    {
                        alertTemplate = context.TBL_ALERT_TITLE.Where(a => a.BINDINGMETHOD == "GetDigitalLoanExceptionNPLModuleIncreaseByOnePercent").FirstOrDefault();
                        template = alertTemplate.TEMPLATE;
                        template = template.Replace("@{{percentage}}", onePercent);
                        template = template.Replace("@{{moduleName}}", exceptionNPL.MODULE);
                        emailList = alertTemplate.DEFAULTEMAIL;
                    }

                    if (exceptionNPL.NPL <= -(decimal)onePointFivePercentValue && exceptionNPL.NPL > -(decimal)twoPercentValue)
                    {
                        alertTemplate = context.TBL_ALERT_TITLE.Where(a => a.BINDINGMETHOD == "GetDigitalLoanExceptionNPLModuleIncreaseByOnePointFivePercent").FirstOrDefault();
                        template = alertTemplate.TEMPLATE;
                        template = template.Replace("@{{percentage}}", onePointFivePercent);
                        template = template.Replace("@{{moduleName}}", exceptionNPL.MODULE);
                        emailList = alertTemplate.DEFAULTEMAIL;
                    }
                    if (exceptionNPL.NPL <= -(decimal)twoPercentValue)
                    {
                        alertTemplate = context.TBL_ALERT_TITLE.Where(a => a.BINDINGMETHOD == "GetDigitalLoanExceptionNPLModuleIncreaseByTwoPercent").FirstOrDefault();
                        template = alertTemplate.TEMPLATE;
                        template = template.Replace("@{{percentage}}", twoPercent);
                        template = template.Replace("@{{moduleName}}", exceptionNPL.MODULE);
                        emailList = alertTemplate.DEFAULTEMAIL;
                    }

                    if ((exceptionNPL.NPL <= -(decimal)onePercentValue && exceptionNPL.NPL > -(decimal)onePointFivePercentValue)
                       || (exceptionNPL.NPL <= -(decimal)onePointFivePercentValue && exceptionNPL.NPL > -(decimal)twoPercentValue) || (exceptionNPL.NPL <= -(decimal)twoPercentValue))
                    {
                        var email = "Chibuike.Mbanefo@ACCESSBANKPLC.com;PAUL.ASIEMO@accessbankplc.com;OLUKAYODE.AJAYI@ACCESSBANKPLC.com";
                        alert.receiverEmailList.Add(email);
                        alert.template = template;
                        alert.alertTitle = alertTemplate.TITLE;
                        alert.canFire = true;
                        alert.operationMethod = alertTemplate.BINDINGMETHOD;
                        alerts.Add(alert);
                    }
                }
                if (alerts.Count() > 0)
                {
                    SendAlertNotification(alerts);
                }
            }
        }
        //Disbursement
        public void GetDigitalLoanDisbursementIncrease()
        {
            var exceptionNPLs = (from a in context2.TBL_TRIGGER_MEASURES_PRODUCT select a).ToList();
            List<AlertsViewModel> alerts = new List<AlertsViewModel>();

            if (exceptionNPLs != null && exceptionNPLs.Count() > 0)
            {
                foreach (var exceptionNPL in exceptionNPLs)
                {
                    TBL_ALERT_TITLE alertTemplate = new TBL_ALERT_TITLE();
                    AlertsViewModel alert = new AlertsViewModel();
                    var template = "";
                    string emailList = "";

                    if (exceptionNPL.DISBURSEMENT >= (decimal)fivePercentValue && exceptionNPL.DISBURSEMENT < (decimal)tenPercentValue)
                    {
                        alertTemplate = context.TBL_ALERT_TITLE.Where(a => a.BINDINGMETHOD == "GetDigitalLoanDisbursementIncreaseByFivePercent").FirstOrDefault();
                        template = alertTemplate.TEMPLATE;
                        template = template.Replace("@{{percentage}}", fivePercent);
                        template = template.Replace("@{{productName}}", exceptionNPL.PRODUCTNAME);
                        emailList = alertTemplate.DEFAULTEMAIL;
                    }

                    if (exceptionNPL.DISBURSEMENT >= (decimal)tenPercentValue && exceptionNPL.DISBURSEMENT < (decimal)fifteenPercentValue)
                    {
                        alertTemplate = context.TBL_ALERT_TITLE.Where(a => a.BINDINGMETHOD == "GetDigitalLoanDisbursementIncreaseByTenPercent").FirstOrDefault();
                        template = alertTemplate.TEMPLATE;
                        template = template.Replace("@{{percentage}}", tenPercent);
                        template = template.Replace("@{{productName}}", exceptionNPL.PRODUCTNAME);
                        emailList = alertTemplate.DEFAULTEMAIL;
                    }
                    if (exceptionNPL.DISBURSEMENT >= (decimal)fifteenPercentValue)
                    {
                        alertTemplate = context.TBL_ALERT_TITLE.Where(a => a.BINDINGMETHOD == "GetDigitalLoanDisbursementIncreaseByFifteenPercent").FirstOrDefault();
                        template = alertTemplate.TEMPLATE;
                        template = template.Replace("@{{percentage}}", fifteenPercent);
                        template = template.Replace("@{{productName}}", exceptionNPL.PRODUCTNAME);
                        emailList = alertTemplate.DEFAULTEMAIL;
                    }

                    if ((exceptionNPL.DISBURSEMENT >= (decimal)fivePercentValue && exceptionNPL.DISBURSEMENT < (decimal)tenPercentValue)
                       || (exceptionNPL.DISBURSEMENT >= (decimal)tenPercentValue && exceptionNPL.DISBURSEMENT < (decimal)fifteenPercentValue)
                       || (exceptionNPL.DISBURSEMENT >= (decimal)fifteenPercentValue))
                    {
                        var email = "Chibuike.Mbanefo@ACCESSBANKPLC.com;PAUL.ASIEMO@accessbankplc.com;OLUKAYODE.AJAYI@ACCESSBANKPLC.com";
                        alert.receiverEmailList.Add(email);
                        alert.template = template;
                        alert.alertTitle = alertTemplate.TITLE;
                        alert.canFire = true;
                        alert.operationMethod = alertTemplate.BINDINGMETHOD;
                        alerts.Add(alert);
                    }
                }
                if (alerts.Count() > 0)
                {
                    SendAlertNotification(alerts);
                }
            }
        }
        public void GetDigitalLoanDisbursementDecrease()
        {
            var exceptionNPLs = (from a in context2.TBL_TRIGGER_MEASURES_PRODUCT select a).ToList();
            List<AlertsViewModel> alerts = new List<AlertsViewModel>();

            if (exceptionNPLs != null && exceptionNPLs.Count() > 0)
            {
                foreach (var exceptionNPL in exceptionNPLs)
                {
                    TBL_ALERT_TITLE alertTemplate = new TBL_ALERT_TITLE();
                    AlertsViewModel alert = new AlertsViewModel();
                    var template = "";
                    string emailList = "";

                    if (exceptionNPL.DISBURSEMENT <= -(decimal)fivePercentValue && exceptionNPL.DISBURSEMENT > -(decimal)tenPercentValue)
                    {
                        alertTemplate = context.TBL_ALERT_TITLE.Where(a => a.BINDINGMETHOD == "GetDigitalLoanDisbursementDecreaseByFivePercent").FirstOrDefault();
                        template = alertTemplate.TEMPLATE;
                        template = template.Replace("@{{percentage}}", fivePercent);
                        template = template.Replace("@{{productName}}", exceptionNPL.PRODUCTNAME);
                        emailList = alertTemplate.DEFAULTEMAIL;
                    }

                    if (exceptionNPL.DISBURSEMENT <= -(decimal)tenPercentValue && exceptionNPL.DISBURSEMENT > -(decimal)fifteenPercentValue)
                    {
                        alertTemplate = context.TBL_ALERT_TITLE.Where(a => a.BINDINGMETHOD == "GetDigitalLoanDisbursementDecreaseByTenPercent").FirstOrDefault();
                        template = alertTemplate.TEMPLATE;
                        template = template.Replace("@{{percentage}}", tenPercent);
                        template = template.Replace("@{{productName}}", exceptionNPL.PRODUCTNAME);
                        emailList = alertTemplate.DEFAULTEMAIL;
                    }
                    if (exceptionNPL.DISBURSEMENT <= -(decimal)fifteenPercentValue)
                    {
                        alertTemplate = context.TBL_ALERT_TITLE.Where(a => a.BINDINGMETHOD == "GetDigitalLoanDisbursementDecreaseByFifteenPercent").FirstOrDefault();
                        template = alertTemplate.TEMPLATE;
                        template = template.Replace("@{{percentage}}", fifteenPercent);
                        template = template.Replace("@{{productName}}", exceptionNPL.PRODUCTNAME);
                        emailList = alertTemplate.DEFAULTEMAIL;
                    }

                    if ((exceptionNPL.DISBURSEMENT <= -(decimal)fivePercentValue && exceptionNPL.DISBURSEMENT > -(decimal)tenPercentValue)
                       || (exceptionNPL.DISBURSEMENT <= -(decimal)tenPercentValue && exceptionNPL.DISBURSEMENT > -(decimal)fifteenPercentValue)
                       || (exceptionNPL.DISBURSEMENT <= -(decimal)fifteenPercentValue))
                    {
                        var email = "Chibuike.Mbanefo@ACCESSBANKPLC.com;PAUL.ASIEMO@accessbankplc.com;OLUKAYODE.AJAYI@ACCESSBANKPLC.com";
                        alert.receiverEmailList.Add(email);
                        alert.template = template;
                        alert.alertTitle = alertTemplate.TITLE;
                        alert.canFire = true;
                        alert.operationMethod = alertTemplate.BINDINGMETHOD;
                        alerts.Add(alert);
                    }
                }
                if (alerts.Count() > 0)
                {
                    SendAlertNotification(alerts);
                }
            }
        }
        public void GetDigitalLoanDisbursementModuleIncrease()
        {
            var exceptionNPLs = (from a in context2.TBL_TRIGGER_MEASURES_MODULE select a).ToList();
            List<AlertsViewModel> alerts = new List<AlertsViewModel>();

            if (exceptionNPLs != null && exceptionNPLs.Count() > 0)
            {
                foreach (var exceptionNPL in exceptionNPLs)
                {
                    TBL_ALERT_TITLE alertTemplate = new TBL_ALERT_TITLE();
                    AlertsViewModel alert = new AlertsViewModel();
                    var template = "";
                    string emailList = "";

                    if (exceptionNPL.DISBURSEMENT >= (decimal)fivePercentValue && exceptionNPL.DISBURSEMENT < (decimal)tenPercentValue)
                    {
                        alertTemplate = context.TBL_ALERT_TITLE.Where(a => a.BINDINGMETHOD == "GetDigitalLoanDisbursementModuleIncreaseByFivePercent").FirstOrDefault();
                        template = alertTemplate.TEMPLATE;
                        template = template.Replace("@{{percentage}}", fivePercent);
                        template = template.Replace("@{{moduleName}}", exceptionNPL.MODULE);
                        emailList = alertTemplate.DEFAULTEMAIL;
                    }

                    if (exceptionNPL.DISBURSEMENT >= (decimal)tenPercentValue && exceptionNPL.DISBURSEMENT < (decimal)fifteenPercentValue)
                    {
                        alertTemplate = context.TBL_ALERT_TITLE.Where(a => a.BINDINGMETHOD == "GetDigitalLoanDisbursementModuleIncreaseByTenPercent").FirstOrDefault();
                        template = alertTemplate.TEMPLATE;
                        template = template.Replace("@{{percentage}}", tenPercent);
                        template = template.Replace("@{{moduleName}}", exceptionNPL.MODULE);
                        emailList = alertTemplate.DEFAULTEMAIL;
                    }
                    if (exceptionNPL.DISBURSEMENT >= (decimal)fifteenPercentValue)
                    {
                        alertTemplate = context.TBL_ALERT_TITLE.Where(a => a.BINDINGMETHOD == "GetDigitalLoanDisbursementModuleIncreaseByFifteenPercent").FirstOrDefault();
                        template = alertTemplate.TEMPLATE;
                        template = template.Replace("@{{percentage}}", fifteenPercent);
                        template = template.Replace("@{{moduleName}}", exceptionNPL.MODULE);
                        emailList = alertTemplate.DEFAULTEMAIL;
                    }

                    if ((exceptionNPL.DISBURSEMENT >= (decimal)fivePercentValue && exceptionNPL.DISBURSEMENT < (decimal)tenPercentValue)
                       || (exceptionNPL.DISBURSEMENT >= (decimal)tenPercentValue && exceptionNPL.DISBURSEMENT < (decimal)fifteenPercentValue)
                       || (exceptionNPL.DISBURSEMENT >= (decimal)fifteenPercentValue))
                    {
                        var email = "Chibuike.Mbanefo@ACCESSBANKPLC.com;PAUL.ASIEMO@accessbankplc.com;OLUKAYODE.AJAYI@ACCESSBANKPLC.com";
                        alert.receiverEmailList.Add(email);
                        alert.template = template;
                        alert.alertTitle = alertTemplate.TITLE;
                        alert.canFire = true;
                        alert.operationMethod = alertTemplate.BINDINGMETHOD;
                        alerts.Add(alert);
                    }
                }
                if (alerts.Count() > 0)
                {
                    SendAlertNotification(alerts);
                }
            }
        }
        public void GetDigitalLoanDisbursementModuleDecrease()
        {
            var exceptionNPLs = (from a in context2.TBL_TRIGGER_MEASURES_MODULE select a).ToList();
            List<AlertsViewModel> alerts = new List<AlertsViewModel>();

            if (exceptionNPLs != null && exceptionNPLs.Count() > 0)
            {
                foreach (var exceptionNPL in exceptionNPLs)
                {
                    TBL_ALERT_TITLE alertTemplate = new TBL_ALERT_TITLE();
                    AlertsViewModel alert = new AlertsViewModel();
                    var template = "";
                    string emailList = "";

                    if (exceptionNPL.NPL <= -(decimal)fivePercentValue && exceptionNPL.NPL > -(decimal)tenPercentValue)
                    {
                        alertTemplate = context.TBL_ALERT_TITLE.Where(a => a.BINDINGMETHOD == "GetDigitalLoanDisbursementModuleDecreaseByFivePercent").FirstOrDefault();
                        template = alertTemplate.TEMPLATE;
                        template = template.Replace("@{{percentage}}", fivePercent);
                        template = template.Replace("@{{moduleName}}", exceptionNPL.MODULE);
                        emailList = alertTemplate.DEFAULTEMAIL;
                    }

                    if (exceptionNPL.NPL <= -(decimal)tenPercentValue && exceptionNPL.NPL > -(decimal)fifteenPercentValue)
                    {
                        alertTemplate = context.TBL_ALERT_TITLE.Where(a => a.BINDINGMETHOD == "GetDigitalLoanDisbursementModuleDecreaseByTenPercent").FirstOrDefault();
                        template = alertTemplate.TEMPLATE;
                        template = template.Replace("@{{percentage}}", tenPercent);
                        template = template.Replace("@{{moduleName}}", exceptionNPL.MODULE);
                        emailList = alertTemplate.DEFAULTEMAIL;
                    }
                    if (exceptionNPL.NPL <= -(decimal)fifteenPercentValue)
                    {
                        alertTemplate = context.TBL_ALERT_TITLE.Where(a => a.BINDINGMETHOD == "GetDigitalLoanDisbursementModuleDecreaseByFifteenPercent").FirstOrDefault();
                        template = alertTemplate.TEMPLATE;
                        template = template.Replace("@{{percentage}}", fifteenPercent);
                        template = template.Replace("@{{moduleName}}", exceptionNPL.MODULE);
                        emailList = alertTemplate.DEFAULTEMAIL;
                    }

                    if ((exceptionNPL.NPL <= -(decimal)fivePercentValue && exceptionNPL.NPL > -(decimal)tenPercentValue)
                       || (exceptionNPL.NPL <= -(decimal)tenPercentValue && exceptionNPL.NPL > -(decimal)fifteenPercentValue)
                       || (exceptionNPL.NPL <= -(decimal)fifteenPercentValue))
                    {
                        var email = "Chibuike.Mbanefo@ACCESSBANKPLC.com;PAUL.ASIEMO@accessbankplc.com;OLUKAYODE.AJAYI@ACCESSBANKPLC.com";
                        alert.receiverEmailList.Add(email);
                        alert.template = template;
                        alert.alertTitle = alertTemplate.TITLE;
                        alert.canFire = true;
                        alert.operationMethod = alertTemplate.BINDINGMETHOD;
                        alerts.Add(alert);
                    }
                }
                if (alerts.Count() > 0)
                {
                    SendAlertNotification(alerts);
                }
            }
        }
        //DPD
        public void GetDigitalLoanDPDIncrease()
        {
            var exceptionNPLs = (from a in context2.TBL_TRIGGER_MEASURES_PRODUCT select a).ToList();
            List<AlertsViewModel> alerts = new List<AlertsViewModel>();

            if (exceptionNPLs != null && exceptionNPLs.Count() > 0)
            {
                foreach (var exceptionNPL in exceptionNPLs)
                {
                    TBL_ALERT_TITLE alertTemplate = new TBL_ALERT_TITLE();
                    AlertsViewModel alert = new AlertsViewModel();
                    var template = "";
                    string emailList = "";

                    if (exceptionNPL.DPD >= (decimal)tenPercentValue && exceptionNPL.DPD < (decimal)twentyFivePercentValue)
                    {
                        alertTemplate = context.TBL_ALERT_TITLE.Where(a => a.BINDINGMETHOD == "GetDigitalLoanDPDIncreaseByTenPercent").FirstOrDefault();
                        template = alertTemplate.TEMPLATE;
                        template = template.Replace("@{{percentage}}", tenPercent);
                        template = template.Replace("@{{productName}}", exceptionNPL.PRODUCTNAME);
                        emailList = alertTemplate.DEFAULTEMAIL;
                    }

                    if (exceptionNPL.DPD >= (decimal)twentyFivePercentValue && exceptionNPL.DPD < (decimal)fiftyPercentValue)
                    {
                        alertTemplate = context.TBL_ALERT_TITLE.Where(a => a.BINDINGMETHOD == "GetDigitalLoanDPDIncreaseByTwentyFivePercent").FirstOrDefault();
                        template = alertTemplate.TEMPLATE;
                        template = template.Replace("@{{percentage}}", twentyFivePercent);
                        template = template.Replace("@{{productName}}", exceptionNPL.PRODUCTNAME);
                        emailList = alertTemplate.DEFAULTEMAIL;
                    }
                    if (exceptionNPL.DPD >= (decimal)fiftyPercentValue)
                    {
                        alertTemplate = context.TBL_ALERT_TITLE.Where(a => a.BINDINGMETHOD == "GetDigitalLoanDPDIncreaseByFiftyPercent").FirstOrDefault();
                        template = alertTemplate.TEMPLATE;
                        template = template.Replace("@{{percentage}}", fiftyPercent);
                        template = template.Replace("@{{productName}}", exceptionNPL.PRODUCTNAME);
                        emailList = alertTemplate.DEFAULTEMAIL;
                    }

                    if ((exceptionNPL.DPD >= (decimal)tenPercentValue && exceptionNPL.DPD < (decimal)twentyFivePercentValue)
                       || (exceptionNPL.DPD >= (decimal)twentyFivePercentValue && exceptionNPL.DPD < (decimal)fiftyPercentValue)
                       || (exceptionNPL.DPD >= (decimal)fiftyPercentValue))
                    {
                        var email = "Chibuike.Mbanefo@ACCESSBANKPLC.com;PAUL.ASIEMO@accessbankplc.com;OLUKAYODE.AJAYI@ACCESSBANKPLC.com";
                        alert.receiverEmailList.Add(email);
                        alert.template = template;
                        alert.alertTitle = alertTemplate.TITLE;
                        alert.canFire = true;
                        alert.operationMethod = alertTemplate.BINDINGMETHOD;
                        alerts.Add(alert);
                    }
                }
                if (alerts.Count() > 0)
                {
                    SendAlertNotification(alerts);
                }
            }
        }
        public void GetDigitalLoanDPDDecrease()
        {
            var exceptionNPLs = (from a in context2.TBL_TRIGGER_MEASURES_PRODUCT select a).ToList();
            List<AlertsViewModel> alerts = new List<AlertsViewModel>();

            if (exceptionNPLs != null && exceptionNPLs.Count() > 0)
            {
                foreach (var exceptionNPL in exceptionNPLs)
                {
                    TBL_ALERT_TITLE alertTemplate = new TBL_ALERT_TITLE();
                    AlertsViewModel alert = new AlertsViewModel();
                    var template = "";
                    string emailList = "";

                    if (exceptionNPL.DPD >= -(decimal)tenPercentValue && exceptionNPL.DPD > -(decimal)twentyFivePercentValue)
                    {
                        alertTemplate = context.TBL_ALERT_TITLE.Where(a => a.BINDINGMETHOD == "GetDigitalLoanDPDDecreaseByTenPercent").FirstOrDefault();
                        template = alertTemplate.TEMPLATE;
                        template = template.Replace("@{{percentage}}", tenPercent);
                        template = template.Replace("@{{productName}}", exceptionNPL.PRODUCTNAME);
                        emailList = alertTemplate.DEFAULTEMAIL;
                    }

                    if (exceptionNPL.DPD <= -(decimal)twentyFivePercentValue && exceptionNPL.DPD > -(decimal)fiftyPercentValue)
                    {
                        alertTemplate = context.TBL_ALERT_TITLE.Where(a => a.BINDINGMETHOD == "GetDigitalLoanDPDDecreaseByTwentyFivePercent").FirstOrDefault();
                        template = alertTemplate.TEMPLATE;
                        template = template.Replace("@{{percentage}}", twentyFivePercent);
                        template = template.Replace("@{{productName}}", exceptionNPL.PRODUCTNAME);
                        emailList = alertTemplate.DEFAULTEMAIL;
                    }
                    if (exceptionNPL.DPD <= -(decimal)fiftyPercentValue)
                    {
                        alertTemplate = context.TBL_ALERT_TITLE.Where(a => a.BINDINGMETHOD == "GetDigitalLoanDPDDecreaseByFiftyPercent").FirstOrDefault();
                        template = alertTemplate.TEMPLATE;
                        template = template.Replace("@{{percentage}}", fiftyPercent);
                        template = template.Replace("@{{productName}}", exceptionNPL.PRODUCTNAME);
                        emailList = alertTemplate.DEFAULTEMAIL;
                    }

                    if ((exceptionNPL.DPD <= -(decimal)tenPercentValue && exceptionNPL.DPD > -(decimal)twentyFivePercentValue)
                       || (exceptionNPL.DPD <= -(decimal)twentyFivePercentValue && exceptionNPL.DPD > -(decimal)fiftyPercentValue)
                       || (exceptionNPL.DPD <= -(decimal)fiftyPercentValue))
                    {
                        var email = "Chibuike.Mbanefo@ACCESSBANKPLC.com;PAUL.ASIEMO@accessbankplc.com;OLUKAYODE.AJAYI@ACCESSBANKPLC.com";
                        alert.receiverEmailList.Add(email);
                        alert.template = template;
                        alert.alertTitle = alertTemplate.TITLE;
                        alert.canFire = true;
                        alert.operationMethod = alertTemplate.BINDINGMETHOD;
                        alerts.Add(alert);
                    }
                }
                if (alerts.Count() > 0)
                {
                    SendAlertNotification(alerts);
                }
            }
        }
        public void GetDigitalLoanDPDModuleIncrease()
        {
            var exceptionNPLs = (from a in context2.TBL_TRIGGER_MEASURES_MODULE select a).ToList();
            List<AlertsViewModel> alerts = new List<AlertsViewModel>();

            if (exceptionNPLs != null && exceptionNPLs.Count() > 0)
            {
                foreach (var exceptionNPL in exceptionNPLs)
                {
                    TBL_ALERT_TITLE alertTemplate = new TBL_ALERT_TITLE();
                    AlertsViewModel alert = new AlertsViewModel();
                    var template = "";
                    string emailList = "";

                    if (exceptionNPL.DPD >= (decimal)tenPercentValue && exceptionNPL.DPD < (decimal)twentyFivePercentValue)
                    {
                        alertTemplate = context.TBL_ALERT_TITLE.Where(a => a.BINDINGMETHOD == "GetDigitalLoanDPDModuleIncreaseByTenPercent").FirstOrDefault();
                        template = alertTemplate.TEMPLATE;
                        template = template.Replace("@{{percentage}}", tenPercent);
                        template = template.Replace("@{{moduleName}}", exceptionNPL.MODULE);
                        emailList = alertTemplate.DEFAULTEMAIL;
                    }

                    if (exceptionNPL.DPD >= (decimal)twentyFivePercentValue && exceptionNPL.DPD < (decimal)fiftyPercentValue)
                    {
                        alertTemplate = context.TBL_ALERT_TITLE.Where(a => a.BINDINGMETHOD == "GetDigitalLoanDPDModuleIncreaseByTwentyFivePercent").FirstOrDefault();
                        template = alertTemplate.TEMPLATE;
                        template = template.Replace("@{{percentage}}", twentyFivePercent);
                        template = template.Replace("@{{moduleName}}", exceptionNPL.MODULE);
                        emailList = alertTemplate.DEFAULTEMAIL;
                    }
                    if (exceptionNPL.DPD >= (decimal)fiftyPercentValue)
                    {
                        alertTemplate = context.TBL_ALERT_TITLE.Where(a => a.BINDINGMETHOD == "GetDigitalLoanDPDModuleIncreaseByFiftyPercent").FirstOrDefault();
                        template = alertTemplate.TEMPLATE;
                        template = template.Replace("@{{percentage}}", fiftyPercent);
                        template = template.Replace("@{{moduleName}}", exceptionNPL.MODULE);
                        emailList = alertTemplate.DEFAULTEMAIL;
                    }

                    if ((exceptionNPL.DPD >= (decimal)tenPercentValue && exceptionNPL.DPD < (decimal)twentyFivePercentValue)
                       || (exceptionNPL.DPD >= (decimal)twentyFivePercentValue && exceptionNPL.DPD < (decimal)fiftyPercentValue)
                       || (exceptionNPL.DPD >= (decimal)fiftyPercentValue))
                    {
                        var email = "Chibuike.Mbanefo@ACCESSBANKPLC.com;PAUL.ASIEMO@accessbankplc.com;OLUKAYODE.AJAYI@ACCESSBANKPLC.com";
                        alert.receiverEmailList.Add(email);
                        alert.template = template;
                        alert.alertTitle = alertTemplate.TITLE;
                        alert.canFire = true;
                        alert.operationMethod = alertTemplate.BINDINGMETHOD;
                        alerts.Add(alert);
                    }
                }
                if (alerts.Count() > 0)
                {
                    SendAlertNotification(alerts);
                }
            }
        }
        public void GetDigitalLoanDPDModuleDecrease()
        {
            var exceptionNPLs = (from a in context2.TBL_TRIGGER_MEASURES_MODULE select a).ToList();
            List<AlertsViewModel> alerts = new List<AlertsViewModel>();

            if (exceptionNPLs != null && exceptionNPLs.Count() > 0)
            {
                foreach (var exceptionNPL in exceptionNPLs)
                {
                    TBL_ALERT_TITLE alertTemplate = new TBL_ALERT_TITLE();
                    AlertsViewModel alert = new AlertsViewModel();
                    var template = "";
                    string emailList = "";

                    if (exceptionNPL.DPD >= -(decimal)tenPercentValue && exceptionNPL.DPD > -(decimal)twentyFivePercentValue)
                    {
                        alertTemplate = context.TBL_ALERT_TITLE.Where(a => a.BINDINGMETHOD == "GetDigitalLoanDPDModuleDecreaseByTenPercent").FirstOrDefault();
                        template = alertTemplate.TEMPLATE;
                        template = template.Replace("@{{percentage}}", tenPercent);
                        template = template.Replace("@{{moduleName}}", exceptionNPL.MODULE);
                        emailList = alertTemplate.DEFAULTEMAIL;
                    }

                    if (exceptionNPL.DPD <= -(decimal)twentyFivePercentValue && exceptionNPL.DPD > -(decimal)fiftyPercentValue)
                    {
                        alertTemplate = context.TBL_ALERT_TITLE.Where(a => a.BINDINGMETHOD == "GetDigitalLoanDPDModuleDecreaseByTwentyFivePercent").FirstOrDefault();
                        template = alertTemplate.TEMPLATE;
                        template = template.Replace("@{{percentage}}", twentyFivePercent);
                        template = template.Replace("@{{moduleName}}", exceptionNPL.MODULE);
                        emailList = alertTemplate.DEFAULTEMAIL;
                    }
                    if (exceptionNPL.DPD <= -(decimal)fiftyPercentValue)
                    {
                        alertTemplate = context.TBL_ALERT_TITLE.Where(a => a.BINDINGMETHOD == "GetDigitalLoanDPDModuleDecreaseByFiftyPercent").FirstOrDefault();
                        template = alertTemplate.TEMPLATE;
                        template = template.Replace("@{{percentage}}", fiftyPercent);
                        template = template.Replace("@{{moduleName}}", exceptionNPL.MODULE);
                        emailList = alertTemplate.DEFAULTEMAIL;
                    }

                    if ((exceptionNPL.DPD <= -(decimal)tenPercentValue && exceptionNPL.DPD > -(decimal)twentyFivePercentValue)
                       || (exceptionNPL.DPD <= -(decimal)twentyFivePercentValue && exceptionNPL.DPD > -(decimal)fiftyPercentValue)
                       || (exceptionNPL.DPD <= -(decimal)fiftyPercentValue))
                    {
                        var email = "Chibuike.Mbanefo@ACCESSBANKPLC.com;PAUL.ASIEMO@accessbankplc.com;OLUKAYODE.AJAYI@ACCESSBANKPLC.com";
                        alert.receiverEmailList.Add(email);
                        alert.template = template;
                        alert.alertTitle = alertTemplate.TITLE;
                        alert.canFire = true;
                        alert.operationMethod = alertTemplate.BINDINGMETHOD;
                        alerts.Add(alert);
                    }
                }
                if (alerts.Count() > 0)
                {
                    SendAlertNotification(alerts);
                }
            }
        }
        public void GetDigitalLoanLiquidationIncrease()
        {
            var exceptionNPLs = (from a in context2.TBL_TRIGGER_MEASURES_PRODUCT select a).ToList();
            List<AlertsViewModel> alerts = new List<AlertsViewModel>();

            if (exceptionNPLs != null && exceptionNPLs.Count() > 0)
            {
                foreach (var exceptionNPL in exceptionNPLs)
                {
                    TBL_ALERT_TITLE alertTemplate = new TBL_ALERT_TITLE();
                    AlertsViewModel alert = new AlertsViewModel();
                    var template = "";
                    string emailList = "";

                    if (exceptionNPL.LIQUIDATION >= (decimal)fivePercentValue && exceptionNPL.LIQUIDATION < (decimal)tenPercentValue)
                    {
                        alertTemplate = context.TBL_ALERT_TITLE.Where(a => a.BINDINGMETHOD == "GetDigitalLoanLiquidationIncreaseByFivePercent").FirstOrDefault();
                        template = alertTemplate.TEMPLATE;
                        template = template.Replace("@{{percentage}}", tenPercent);
                        template = template.Replace("@{{productName}}", exceptionNPL.PRODUCTNAME);
                        emailList = alertTemplate.DEFAULTEMAIL;
                    }

                    if (exceptionNPL.LIQUIDATION >= (decimal)tenPercentValue && exceptionNPL.LIQUIDATION < (decimal)fifteenPercentValue)
                    {
                        alertTemplate = context.TBL_ALERT_TITLE.Where(a => a.BINDINGMETHOD == "GetDigitalLoanLiquidationIncreaseByTenPercent").FirstOrDefault();
                        template = alertTemplate.TEMPLATE;
                        template = template.Replace("@{{percentage}}", twentyFivePercent);
                        template = template.Replace("@{{productName}}", exceptionNPL.PRODUCTNAME);
                        emailList = alertTemplate.DEFAULTEMAIL;
                    }
                    if (exceptionNPL.LIQUIDATION >= (decimal)fifteenPercentValue)
                    {
                        alertTemplate = context.TBL_ALERT_TITLE.Where(a => a.BINDINGMETHOD == "GetDigitalLoanLiquidationIncreaseByFifteenPercent").FirstOrDefault();
                        template = alertTemplate.TEMPLATE;
                        template = template.Replace("@{{percentage}}", fiftyPercent);
                        template = template.Replace("@{{productName}}", exceptionNPL.PRODUCTNAME);
                        emailList = alertTemplate.DEFAULTEMAIL;
                    }

                    if ((exceptionNPL.LIQUIDATION >= (decimal)fivePercentValue && exceptionNPL.LIQUIDATION < (decimal)tenPercentValue)
                       || (exceptionNPL.LIQUIDATION >= (decimal)tenPercentValue && exceptionNPL.LIQUIDATION < (decimal)fifteenPercentValue)
                       || (exceptionNPL.LIQUIDATION >= (decimal)fifteenPercentValue))
                    {
                        var email = "Chibuike.Mbanefo@ACCESSBANKPLC.com;PAUL.ASIEMO@accessbankplc.com;OLUKAYODE.AJAYI@ACCESSBANKPLC.com";
                        alert.receiverEmailList.Add(email);
                        alert.template = template;
                        alert.alertTitle = alertTemplate.TITLE;
                        alert.canFire = true;
                        alert.operationMethod = alertTemplate.BINDINGMETHOD;
                        alerts.Add(alert);
                    }
                }
                if (alerts.Count() > 0)
                {
                    SendAlertNotification(alerts);
                }
            }
        }
        public void GetDigitalLoanLiquidationModuleIncrease()
        {
            var exceptionNPLs = (from a in context2.TBL_TRIGGER_MEASURES_MODULE select a).ToList();
            List<AlertsViewModel> alerts = new List<AlertsViewModel>();

            if (exceptionNPLs != null && exceptionNPLs.Count() > 0)
            {
                foreach (var exceptionNPL in exceptionNPLs)
                {
                    TBL_ALERT_TITLE alertTemplate = new TBL_ALERT_TITLE();
                    AlertsViewModel alert = new AlertsViewModel();
                    var template = "";
                    string emailList = "";

                    if (exceptionNPL.LIQUIDATION >= (decimal)fivePercentValue && exceptionNPL.LIQUIDATION < (decimal)tenPercentValue)
                    {
                        alertTemplate = context.TBL_ALERT_TITLE.Where(a => a.BINDINGMETHOD == "GetDigitalLoanLiquidationIncreaseByFivePercent").FirstOrDefault();
                        template = alertTemplate.TEMPLATE;
                        template = template.Replace("@{{percentage}}", tenPercent);
                        template = template.Replace("@{{moduleName}}", exceptionNPL.MODULE);
                        emailList = alertTemplate.DEFAULTEMAIL;
                    }

                    if (exceptionNPL.LIQUIDATION >= (decimal)tenPercentValue && exceptionNPL.LIQUIDATION < (decimal)fifteenPercentValue)
                    {
                        alertTemplate = context.TBL_ALERT_TITLE.Where(a => a.BINDINGMETHOD == "GetDigitalLoanLiquidationIncreaseByTenPercent").FirstOrDefault();
                        template = alertTemplate.TEMPLATE;
                        template = template.Replace("@{{percentage}}", twentyFivePercent);
                        template = template.Replace("@{{moduleName}}", exceptionNPL.MODULE);
                        emailList = alertTemplate.DEFAULTEMAIL;
                    }
                    if (exceptionNPL.LIQUIDATION >= (decimal)fifteenPercentValue)
                    {
                        alertTemplate = context.TBL_ALERT_TITLE.Where(a => a.BINDINGMETHOD == "GetDigitalLoanLiquidationIncreaseByFifteenPercent").FirstOrDefault();
                        template = alertTemplate.TEMPLATE;
                        template = template.Replace("@{{percentage}}", fiftyPercent);
                        template = template.Replace("@{{moduleName}}", exceptionNPL.MODULE);
                        emailList = alertTemplate.DEFAULTEMAIL;
                    }

                    if ((exceptionNPL.LIQUIDATION >= (decimal)fivePercentValue && exceptionNPL.LIQUIDATION < (decimal)tenPercentValue)
                       || (exceptionNPL.LIQUIDATION >= (decimal)tenPercentValue && exceptionNPL.LIQUIDATION < (decimal)fifteenPercentValue)
                       || (exceptionNPL.LIQUIDATION >= (decimal)fifteenPercentValue))
                    {
                        var email = "Chibuike.Mbanefo@ACCESSBANKPLC.com;PAUL.ASIEMO@accessbankplc.com;OLUKAYODE.AJAYI@ACCESSBANKPLC.com";
                        alert.receiverEmailList.Add(email);
                        alert.template = template;
                        alert.alertTitle = alertTemplate.TITLE;
                        alert.canFire = true;
                        alert.operationMethod = alertTemplate.BINDINGMETHOD;
                        alerts.Add(alert);
                    }
                }
                if (alerts.Count() > 0)
                {
                    SendAlertNotification(alerts);
                }
            }
        }

        public void GetSectorLimitExceedeBBDReminder()
        {
            // GetSectorLimitExceedeBBDReminder method
            var sectorBankSum = context2.TBL_SECTOR_LIMIT_ALERT.Where(a => a.SECTOR != null).Sum(a => a.BANK);
            var sectorLimitBBD = externalAlertRepository.GetSectorLimitValidationBBD();

            var alertTitleInfo = context.TBL_ALERT_TITLE.Where(a => a.BINDINGMETHOD == "GetSectorLimitExceedeBBDReminder").FirstOrDefault();
            var defaultEmail = "";
            if (alertTitleInfo.DEFAULTEMAIL != null)
            {
                defaultEmail = alertTitleInfo.DEFAULTEMAIL;
            }

            if (sectorLimitBBD != null && sectorLimitBBD.Count() > 0)
            {
                List<AlertsViewModel> alerts = new List<AlertsViewModel>();
                string emailList = "";
                var alertTitle = alertTitleInfo.TITLE;


                foreach (var i in sectorLimitBBD)
                {
                    var sectorPercentage = decimal.Round(((decimal)i.bbd * (decimal)sectorBankSum), 2, MidpointRounding.AwayFromZero);
                    var sectorEightyFivePercent = decimal.Round((sectorPercentage * (decimal)eightyFivePercentValue), 2, MidpointRounding.AwayFromZero);
                    var sectorNinetyPercent = decimal.Round((sectorPercentage * (decimal)ninetyPercentValue), 2, MidpointRounding.AwayFromZero);
                    var sectorNinetyFivePercent = decimal.Round((sectorPercentage * (decimal)ninetyFivePercentValue), 2, MidpointRounding.AwayFromZero);
                    var alertTemplate = alertTitleInfo.TEMPLATE;
                    AlertsViewModel alert = new AlertsViewModel();

                    if (i.exposure >= sectorEightyFivePercent || i.exposure >= sectorNinetyPercent || i.exposure >= sectorNinetyFivePercent)
                    {
                        if (i.exposure >= sectorEightyFivePercent && i.exposure < sectorNinetyPercent)
                        {
                            alertTemplate = alertTemplate.Replace("@{{sector}}", i.sector);
                            alertTemplate = alertTemplate.Replace("@{{percentage}}", eightyFivePercent);
                            alertTemplate = alertTemplate.Replace("@{{indicator}}", eightyFiveToNinety);
                        }

                        if (i.exposure >= sectorNinetyPercent && i.exposure < sectorNinetyFivePercent)
                        {
                            alertTemplate = alertTemplate.Replace("@{{sector}}", i.sector);
                            alertTemplate = alertTemplate.Replace("@{{percentage}}", ninetyPercent);
                            alertTemplate = alertTemplate.Replace("@{{indicator}}", ninetyToNinetyFive);
                        }

                        if (i.exposure >= sectorNinetyFivePercent)
                        {
                            alertTemplate = alertTemplate.Replace("@{{sector}}", i.sector);
                            alertTemplate = alertTemplate.Replace("@{{percentage}}", ninetyFivePercent);
                            alertTemplate = alertTemplate.Replace("@{{indicator}}", ninetyFiveAbove);
                        }

                        emailList = defaultEmail;
                        alert.receiverEmailList.Add(emailList);
                        alert.template = alertTemplate;
                        alert.alertTitle = alertTitle;
                        alert.canFire = true;
                        alert.operationMethod = alertTitleInfo.BINDINGMETHOD;
                        alerts.Add(alert);
                    }
                }

                if (alerts.Count() > 0)
                {
                    SendAlertNotification(alerts);
                }
            }
        }

        public void GetSectorLimitExceedeCBDReminder()
        {
            // GetSectorLimitExceedeCBDReminder method
            var sectorBankSum = context2.TBL_SECTOR_LIMIT_ALERT.Where(a => a.SECTOR != null).Sum(a => a.BANK);
            var sectorLimitCBD = externalAlertRepository.GetSectorLimitValidationCBD();

            var alertTitleInfo = context.TBL_ALERT_TITLE.Where(a => a.BINDINGMETHOD == "GetSectorLimitExceedeCBDReminder").FirstOrDefault();
            var defaultEmail = "";
            if (alertTitleInfo.DEFAULTEMAIL != null)
            {
                defaultEmail = ";" + alertTitleInfo.DEFAULTEMAIL;
            }

            if (sectorLimitCBD != null && sectorLimitCBD.Count() > 0)
            {
                List<AlertsViewModel> alerts = new List<AlertsViewModel>();
                string emailList = "";
                var alertTitle = alertTitleInfo.TITLE;

                foreach (var i in sectorLimitCBD)
                {
                    var sectorPercentage = decimal.Round(((decimal)i.cbd * (decimal)sectorBankSum), 2, MidpointRounding.AwayFromZero);
                    var sectorEightyFivePercent = decimal.Round((sectorPercentage * (decimal)eightyFivePercentValue), 2, MidpointRounding.AwayFromZero);
                    var sectorNinetyPercent = decimal.Round((sectorPercentage * (decimal)ninetyPercentValue), 2, MidpointRounding.AwayFromZero);
                    var sectorNinetyFivePercent = decimal.Round((sectorPercentage * (decimal)ninetyFivePercentValue), 2, MidpointRounding.AwayFromZero);

                    var alertTemplate = alertTitleInfo.TEMPLATE;
                    AlertsViewModel alert = new AlertsViewModel();

                    if (i.exposure >= sectorEightyFivePercent || i.exposure >= sectorNinetyPercent || i.exposure >= sectorNinetyFivePercent)
                    {
                        if (i.exposure >= sectorEightyFivePercent && i.exposure < sectorNinetyPercent)
                        {
                            alertTemplate = alertTemplate.Replace("@{{sector}}", i.sector);
                            alertTemplate = alertTemplate.Replace("@{{percentage}}", eightyFivePercent);
                            alertTemplate = alertTemplate.Replace("@{{indicator}}", eightyFiveToNinety);
                        }

                        if (i.exposure >= sectorNinetyPercent && i.exposure < sectorNinetyFivePercent)
                        {
                            alertTemplate = alertTemplate.Replace("@{{sector}}", i.sector);
                            alertTemplate = alertTemplate.Replace("@{{percentage}}", ninetyPercent);
                            alertTemplate = alertTemplate.Replace("@{{indicator}}", ninetyToNinetyFive);
                        }

                        if (i.exposure >= sectorNinetyFivePercent)
                        {
                            alertTemplate = alertTemplate.Replace("@{{sector}}", i.sector);
                            alertTemplate = alertTemplate.Replace("@{{percentage}}", ninetyFivePercent);
                            alertTemplate = alertTemplate.Replace("@{{indicator}}", ninetyFiveAbove);
                        }

                        emailList = defaultEmail;
                        alert.receiverEmailList.Add(emailList);
                        alert.template = alertTemplate;
                        alert.alertTitle = alertTitle;
                        alert.canFire = true;
                        alert.operationMethod = alertTitleInfo.BINDINGMETHOD;
                        alerts.Add(alert);
                    }
                }

                if (alerts.Count() > 0)
                {
                    SendAlertNotification(alerts);
                }
            }
        }

        public void GetSectorLimitExceedeCIBDReminder()
        {
            // GetSectorLimitExceedeCIBDReminder method
            var sectorBankSum = context2.TBL_SECTOR_LIMIT_ALERT.Where(a => a.SECTOR != null).Sum(a => a.BANK);
            var sectorLimitCIBD = externalAlertRepository.GetSectorLimitValidationCIBD();

            var alertTitleInfo = context.TBL_ALERT_TITLE.Where(a => a.BINDINGMETHOD == "GetSectorLimitExceedeCIBDReminder").FirstOrDefault();
            var defaultEmail = "";
            if (alertTitleInfo.DEFAULTEMAIL != null)
            {
                defaultEmail = ";" + alertTitleInfo.DEFAULTEMAIL;
            }

            if (sectorLimitCIBD != null && sectorLimitCIBD.Count() > 0)
            {
                List<AlertsViewModel> alerts = new List<AlertsViewModel>();
                var alertTitle = alertTitleInfo.TITLE;
                string emailList = "";

                foreach (var i in sectorLimitCIBD)
                {
                    var sectorPercentage = decimal.Round(((decimal)i.cibd * (decimal)sectorBankSum), 2, MidpointRounding.AwayFromZero);
                    var sectorEightyFivePercent = decimal.Round((sectorPercentage * (decimal)eightyFivePercentValue), 2, MidpointRounding.AwayFromZero);
                    var sectorNinetyPercent = decimal.Round((sectorPercentage * (decimal)ninetyPercentValue), 2, MidpointRounding.AwayFromZero);
                    var sectorNinetyFivePercent = decimal.Round((sectorPercentage * (decimal)ninetyFivePercentValue), 2, MidpointRounding.AwayFromZero);

                    var alertTemplate = alertTitleInfo.TEMPLATE;
                    AlertsViewModel alert = new AlertsViewModel();

                    if (i.exposure >= sectorEightyFivePercent || i.exposure >= sectorNinetyPercent || i.exposure >= sectorNinetyFivePercent)
                    {
                        if (i.exposure >= sectorEightyFivePercent && i.exposure < sectorNinetyPercent)
                        {
                            alertTemplate = alertTemplate.Replace("@{{sector}}", i.sector);
                            alertTemplate = alertTemplate.Replace("@{{percentage}}", eightyFivePercent);
                            alertTemplate = alertTemplate.Replace("@{{indicator}}", eightyFiveToNinety);
                        }

                        if (i.exposure >= sectorNinetyPercent && i.exposure < sectorNinetyFivePercent)
                        {
                            alertTemplate = alertTemplate.Replace("@{{sector}}", i.sector);
                            alertTemplate = alertTemplate.Replace("@{{percentage}}", ninetyPercent);
                            alertTemplate = alertTemplate.Replace("@{{indicator}}", ninetyToNinetyFive);
                        }

                        if (i.exposure >= sectorNinetyFivePercent)
                        {
                            alertTemplate = alertTemplate.Replace("@{{sector}}", i.sector);
                            alertTemplate = alertTemplate.Replace("@{{percentage}}", ninetyFivePercent);
                            alertTemplate = alertTemplate.Replace("@{{indicator}}", ninetyFiveAbove);
                        }

                        emailList = defaultEmail;
                        alert.receiverEmailList.Add(emailList);
                        alert.template = alertTemplate;
                        alert.alertTitle = alertTitle;
                        alert.canFire = true;
                        alert.operationMethod = alertTitleInfo.BINDINGMETHOD;
                        alerts.Add(alert);
                    }
                }

                if (alerts.Count() > 0)
                {
                    SendAlertNotification(alerts);
                }
            }
        }

        public void GetSectorLimitExceedeRBDReminder()
        {
            // GetSectorLimitExceedeRBDReminder method
            var sectorBankSum = context2.TBL_SECTOR_LIMIT_ALERT.Where(a => a.SECTOR != null).Sum(a => a.BANK);
            var sectorLimitRBD = externalAlertRepository.GetSectorLimitValidationRBD();

            var alertTitleInfo = context.TBL_ALERT_TITLE.Where(a => a.BINDINGMETHOD == "GetSectorLimitExceedeRBDReminder").FirstOrDefault();
            var defaultEmail = "";
            if (alertTitleInfo.DEFAULTEMAIL != null)
            {
                defaultEmail = ";" + alertTitleInfo.DEFAULTEMAIL;
            }

            if (sectorLimitRBD != null && sectorLimitRBD.Count() > 0)
            {
                List<AlertsViewModel> alerts = new List<AlertsViewModel>();
                var alertTitle = alertTitleInfo.TITLE;
                string emailList = "";

                foreach (var i in sectorLimitRBD)
                {
                    var sectorPercentage = decimal.Round(((decimal)i.rbd * (decimal)sectorBankSum), 2, MidpointRounding.AwayFromZero);
                    var sectorEightyFivePercent = decimal.Round((sectorPercentage * (decimal)eightyFivePercentValue), 2, MidpointRounding.AwayFromZero);
                    var sectorNinetyPercent = decimal.Round((sectorPercentage * (decimal)ninetyPercentValue), 2, MidpointRounding.AwayFromZero);
                    var sectorNinetyFivePercent = decimal.Round((sectorPercentage * (decimal)ninetyFivePercentValue), 2, MidpointRounding.AwayFromZero);

                    var alertTemplate = alertTitleInfo.TEMPLATE;
                    AlertsViewModel alert = new AlertsViewModel();

                    if (i.exposure >= sectorEightyFivePercent || i.exposure >= sectorNinetyPercent || i.exposure >= sectorNinetyFivePercent)
                    {
                        if (i.exposure >= sectorEightyFivePercent && i.exposure < sectorNinetyPercent)
                        {
                            alertTemplate = alertTemplate.Replace("@{{sector}}", i.sector);
                            alertTemplate = alertTemplate.Replace("@{{percentage}}", eightyFivePercent);
                            alertTemplate = alertTemplate.Replace("@{{indicator}}", eightyFiveToNinety);
                        }

                        if (i.exposure >= sectorNinetyPercent && i.exposure < sectorNinetyFivePercent)
                        {
                            alertTemplate = alertTemplate.Replace("@{{sector}}", i.sector);
                            alertTemplate = alertTemplate.Replace("@{{percentage}}", ninetyPercent);
                            alertTemplate = alertTemplate.Replace("@{{indicator}}", ninetyToNinetyFive);
                        }

                        if (i.exposure >= sectorNinetyFivePercent)
                        {
                            alertTemplate = alertTemplate.Replace("@{{sector}}", i.sector);
                            alertTemplate = alertTemplate.Replace("@{{percentage}}", ninetyFivePercent);
                            alertTemplate = alertTemplate.Replace("@{{indicator}}", ninetyFiveAbove);
                        }

                        emailList = defaultEmail;
                        alert.receiverEmailList.Add(emailList);
                        alert.template = alertTemplate;
                        alert.alertTitle = alertTitle;
                        alert.canFire = true;
                        alert.operationMethod = alertTitleInfo.BINDINGMETHOD;
                        alerts.Add(alert);
                    }
                }

                if (alerts.Count() > 0)
                {
                    SendAlertNotification(alerts);
                }
            }
        }

        public void GetSectorLimitExceededBankReminder()
        {
            // GetSectorLimitExceededBankReminder method
            var sectorbankSum = context2.TBL_SECTOR_LIMIT_ALERT.Where(a => a.SECTOR != null).Sum(a => a.BANK);
            var sectorLimitBank = externalAlertRepository.GetSectorLimitValidationBank();

            var alertTitleInfo = context.TBL_ALERT_TITLE.Where(a => a.BINDINGMETHOD == "GetSectorLimitExceededBankReminder").FirstOrDefault();
            var defaultEmail = "";
            if (alertTitleInfo.DEFAULTEMAIL != null)
            {
                defaultEmail = ";" + alertTitleInfo.DEFAULTEMAIL;
            }

            if (sectorLimitBank != null && sectorLimitBank.Count() > 0)
            {
                List<AlertsViewModel> alerts = new List<AlertsViewModel>();
                var alertTitle = alertTitleInfo.TITLE;
                string emailList = "";

                foreach (var i in sectorLimitBank)
                {
                    var sectorPercentage = decimal.Round(((decimal)i.bank * (decimal)sectorbankSum), 2, MidpointRounding.AwayFromZero);
                    var sectorEightyFivePercent = decimal.Round((sectorPercentage * (decimal)eightyFivePercentValue), 2, MidpointRounding.AwayFromZero);
                    var sectorNinetyPercent = decimal.Round((sectorPercentage * (decimal)ninetyPercentValue), 2, MidpointRounding.AwayFromZero);
                    var sectorNinetyFivePercent = decimal.Round((sectorPercentage * (decimal)ninetyFivePercentValue), 2, MidpointRounding.AwayFromZero);

                    var alertTemplate = alertTitleInfo.TEMPLATE;
                    AlertsViewModel alert = new AlertsViewModel();

                    if (i.exposure >= sectorEightyFivePercent || i.exposure >= sectorNinetyPercent || i.exposure >= sectorNinetyFivePercent)
                    {
                        if (i.exposure >= sectorEightyFivePercent && i.exposure < sectorNinetyPercent)
                        {
                            alertTemplate = alertTemplate.Replace("@{{sector}}", i.sector);
                            alertTemplate = alertTemplate.Replace("@{{percentage}}", eightyFivePercent);
                            alertTemplate = alertTemplate.Replace("@{{indicator}}", eightyFiveToNinety);
                        }

                        if (i.exposure >= sectorNinetyPercent && i.exposure < sectorNinetyFivePercent)
                        {
                            alertTemplate = alertTemplate.Replace("@{{sector}}", i.sector);
                            alertTemplate = alertTemplate.Replace("@{{percentage}}", ninetyPercent);
                            alertTemplate = alertTemplate.Replace("@{{indicator}}", ninetyToNinetyFive);
                        }

                        if (i.exposure >= sectorNinetyFivePercent)
                        {
                            alertTemplate = alertTemplate.Replace("@{{sector}}", i.sector);
                            alertTemplate = alertTemplate.Replace("@{{percentage}}", ninetyFivePercent);
                            alertTemplate = alertTemplate.Replace("@{{indicator}}", ninetyFiveAbove);
                        }

                        emailList = defaultEmail;
                        alert.receiverEmailList.Add(emailList);
                        alert.template = alertTemplate;
                        alert.alertTitle = alertTitle;
                        alert.canFire = true;
                        alert.operationMethod = alertTitleInfo.BINDINGMETHOD;
                        alerts.Add(alert);
                    }
                }

                if (alerts.Count() > 0)
                {
                    SendAlertNotification(alerts);
                }
            }
        }

        public void GetGroupCreditFileChecklistReminder()
        {
            // GetGroupCreditFileChecklistReminder method
            var creditFileChecklist = externalAlertRepository.GetCreditFileChecklistReminder();
            var alertTitleInfo = context.TBL_ALERT_TITLE.Where(a => a.BINDINGMETHOD == "GetGroupCreditFileChecklistReminder").FirstOrDefault();

            var defaultEmail = "";
            if (alertTitleInfo.DEFAULTEMAIL != null)
            {
                defaultEmail = alertTitleInfo.DEFAULTEMAIL;
            }
            if (creditFileChecklist != null && creditFileChecklist.Count() > 0)
            {
                AlertsViewModel alert = new AlertsViewModel();
                List<AlertsViewModel> alerts = new List<AlertsViewModel>();
                var result = string.Empty;
                var tempResult = string.Empty;
                var alertTemplate = alertTitleInfo.TEMPLATE;
                var alertTitle = alertTitleInfo.TITLE;
                string emailList = "";
                var n = 0;

                tempResult = $@"
                        <h3><b>Deferred Records</b></h3>
                        <table cellpadding='0' cellspacing='0' border='1' width='800px'>
                        <tr>
                            <td><b>S/N</b></td>
                            <td><b>Condition</b></td>
                            <td><b>Reference Number</b></td>
                            <td><b>Deferred Date</b></td>
                            <td><b>Deferred Days</b></td>
                        </tr>
                        ";
                if (creditFileChecklist.Count() > 0)
                {
                    foreach (var t in creditFileChecklist)
                    {
                        n++;
                        var reference = context.TBL_LOAN_APPLICATION.Find(t.loanApplicationId)?.APPLICATIONREFERENCENUMBER;
                        var deferredDate = t.deferredDate.Value.ToString("dd-MM-yyyy");
                        tempResult = tempResult + $@"
                                    <tr>
                                    <td>{n}</td>
                                    <td>{t.condition}</td>
                                    <td>{reference}</td>
                                    <td>{deferredDate}</td>
                                    <td>{t.deferredDays}</td>
                                </tr>
                                ";
                    }
                }
                tempResult = tempResult + $"</table><br/>";
                result = tempResult;


                if (result.Count() > 0 && alertTemplate.Replace("@{{checkList}}", result).Count() > 0)
                {
                    alertTemplate = alertTemplate.Replace("@{{checkList}}", result);
                    alertTemplate = alertTemplate.Replace("@{{accountOfficer}}", "Colleague");
                    emailList = defaultEmail + ";" + GetAllCreditPortfolioStaffEmails();
                    alert.receiverEmailList.Add(emailList);
                    alert.template = alertTemplate;
                    alert.alertTitle = alertTitle;
                    alert.canFire = true;
                    alert.operationMethod = alertTitleInfo.BINDINGMETHOD;

                    alerts.Add(alert);
                }

                if (alerts.Count() > 0)
                {
                    SendAlertNotification(alerts);
                }
            }
        }


        public void GetRepaymentDefaultersAlert()
        {
            // GetRepaymentDefaultersAlert method
            var repaymentDefaulters = externalAlertRepository.GetRepaymentDefaultersAlert();

            if (repaymentDefaulters != null && repaymentDefaulters.Count() > 0)
            {

                List<AlertsViewModel> alerts = new List<AlertsViewModel>();
                foreach (var repaymentDefaulter in repaymentDefaulters)
                {
                    AlertsViewModel alert = new AlertsViewModel();
                    var amount = string.Format("{0:#,##.00}", Convert.ToDecimal(repaymentDefaulter.periodPaymentAmount));
                    var alertTitle = "REMINDER FOR YOUR PAST DUE LOAN REPAYMENT";
                    var alertTemplate = "Dear " + repaymentDefaulter.customerName + " <br/>You have an outstanding repayment of " + amount + " kindly regularise";
                    string emailList = repaymentDefaulter.customerEmail + ";" + repaymentDefaulter.guarantorEmail;

                    alert.receiverEmailList.Add(emailList);
                    alert.template = alertTemplate;
                    alert.alertTitle = alertTitle;
                    alert.canFire = true;
                    alert.operationMethod = "RepaymentDefault";
                    alerts.Add(alert);
                }

                if (alerts.Count() > 0)
                {
                    SendAlertNotification(alerts);
                }
            }
        }

        public void GetRepaymentPayDownAlert()
        {
            // GetRepaymentPayDownAlert method
            var repaymentPayDownAlerts = externalAlertRepository.GetRepaymentPayDownAlert();

            if (repaymentPayDownAlerts != null && repaymentPayDownAlerts.Count() > 0)
            {

                List<AlertsViewModel> alerts = new List<AlertsViewModel>();
                foreach (var repaymentPayDownAlert in repaymentPayDownAlerts)
                {
                    AlertsViewModel alert = new AlertsViewModel();
                    var amount = string.Format("{0:#,##.00}", Convert.ToDecimal(repaymentPayDownAlert.periodPaymentAmount));
                    var alertTitle = "CONGRATULATIONS FOR LOAN REGULARISATION";
                    var alertTemplate = "Dear " + repaymentPayDownAlert.customerName + " <br/>We are pleased to say thank you for the loan regularisation " + amount + ". Thank you";
                    string emailList = repaymentPayDownAlert.customerEmail + ";" + repaymentPayDownAlert.guarantorEmail;

                    alert.receiverEmailList.Add(emailList);
                    alert.template = alertTemplate;
                    alert.alertTitle = alertTitle;
                    alert.canFire = true;
                    alert.operationMethod = "RepaymentPayDown";
                    alerts.Add(alert);
                }

                if (alerts.Count() > 0)
                {
                    SendAlertNotification(alerts);
                }
            }
        }


        public void GetRecoveryAssignmentDueCompletionDate()
        {
            //GetRecoveryAssignmentDueCompletionDate method
            var aboutToExpiredList = externalAlertRepository.GetRecoveryAssignmentDueCompletionDate();
            var alertTitleInfo = context.TBL_ALERT_TITLE.Where(a => a.BINDINGMETHOD == "GetRecoveryAssignmentDueCompletionDate").FirstOrDefault();

            var defaultEmail = "";
            if (alertTitleInfo.DEFAULTEMAIL != null)
            {
                defaultEmail = ";" + alertTitleInfo.DEFAULTEMAIL;
            }
            if (aboutToExpiredList != null && aboutToExpiredList.Count() > 0)
            {

                List<AlertsViewModel> alerts = new List<AlertsViewModel>();
                foreach (var aboutToExpired in aboutToExpiredList)
                {
                    AlertsViewModel alert = new AlertsViewModel();
                    var alertTitle = alertTitleInfo.TITLE;
                    var alertTemplate = alertTitleInfo.TEMPLATE;
                    string emailList = "";

                    emailList = GetBusinessUsersEmails(aboutToExpired.misCode);

                    var loanInformation = context.TBL_LOAN_RECOVERY_ASSIGNMENT.Where(d => d.REFERENCEID == aboutToExpired.referenceId).ToList();

                    if (loanInformation != null && loanInformation.Count() > 0)
                    {
                        var n = 0;
                        var result = $@"
                     <table cellpadding='0' cellspacing='0' border='1' width='800px'>
                        <tr>
                            <td>LIST FOR ASSIGNED RECOVERY FOR {aboutToExpired.accreditedConsultantCompany}</b></td>
                        </tr>
                        <tr>
                            <td><b>S/N</b></td>
                            <td><b>Customer Name</b></td>
                            <td><b>Reference Number</b></td>
                            <td><b>Amount</b></td>
                            <td><b>Product</b></td>
                            <td><b>Number Of Days Left</b></td>
                        </tr>
                     ";

                        foreach (var t in loanInformation)
                        {
                            n++;

                            var amount = string.Format("{0:#,##.00}", Convert.ToDecimal(t.TOTALAMOUNTRECOVERY));
                            var maturityDate = t.EXPCOMPLETIONDATE?.ToString("dd-MM-yyyy");
                            int numberOfDays = (t.EXPCOMPLETIONDATE.Value - DateTime.Now).Days;
                            var staffFullName = context.TBL_STAFF.Where(s => s.STAFFID == t.CREATEDBY).Select(s => s.FIRSTNAME + " " + s.MIDDLENAME + " " + s.LASTNAME).FirstOrDefault();
                            var customerName = context.TBL_CUSTOMER.Where(s => s.CUSTOMERID == t.CUSTOMERID).Select(s => s.FIRSTNAME + " " + s.MIDDLENAME + " " + s.LASTNAME).FirstOrDefault();
                            result = result + $@"
                        <tr>
                            <td>{n}</td>
                            <td>{customerName}</td>
                            <td>{t.LOANREFERENCE}</td>
                            <td>{$"{amount}"}</td>
                            <td>{$"{maturityDate}"}</td>
                            <td>{numberOfDays}</td>
                        </tr>
                        ";
                        }

                        result = result + $"</table>";

                        if (result.Count() > 0 && alertTemplate.Replace("@{{accountNumbers}}", result).Count() > 0)
                        {
                            alertTemplate = alertTemplate.Replace("@{{accreditedConsultantCompany}}", aboutToExpired.accreditedConsultantCompany);
                            alertTemplate = alertTemplate.Replace("@{{@{{recoveries}}}}", result);

                            emailList = emailList + ";" + aboutToExpired.accreditedConsultantEmail + defaultEmail;
                            alert.receiverEmailList.Add(emailList);
                            alert.template = alertTemplate;
                            alert.alertTitle = alertTitle;
                            alert.canFire = true;
                            alert.operationMethod = alertTitleInfo.BINDINGMETHOD;

                            alerts.Add(alert);
                        }
                    }
                }

                if (alerts.Count() > 0)
                {
                    SendAlertNotification(alerts);
                }
            }
        }


        //prepayment operation
        private void getAPIURLSettings(string typeName = null)
        {
            var apiConfig = APIUrlConfig.Where(x => x.TYPENAME.ToLower() == typeName.ToLower()).FirstOrDefault();
            if (apiConfig != null)
            {
                API_URL = apiConfig.URL.Trim();
                API_KEY = apiConfig.APIKEY;
            }
            if (apiConfig == null)
            {
                apiConfig = APIUrlConfig.Where(x => x.TYPENAME.ToUpper() == "DEFAULT").FirstOrDefault();
                API_URL = apiConfig.URL.Trim();
                API_KEY = apiConfig.APIKEY;
            }
        }

        public async Task<MainResponseLoanPrepaymentViewModel> GetTodayRepaymentLoans(LoanPrepaymentViewModel model)
        {
            HttpClientHandler handler = new HttpClientHandler();
            HttpClient httpClientInstance;

            HttpClient client = new HttpClient(handler);
            var inputJson = new JavaScriptSerializer().Serialize(model);
            DateTime requestDatetime = new DateTime(), responseDateTime = new DateTime();
            HttpResponseMessage response = null;

            MainResponseLoanPrepaymentViewModel responseApi = new MainResponseLoanPrepaymentViewModel();
            ResponseMessage responseMsg = null;
            string responseJson = "";

            getAPIURLSettings("LoanPrepayment");
            string apiUrl = "GetTodayRepaymentLoans";

            try
            {
                var token = new AuthenticationHeaderValue("Authorization", API_KEY);
                var dta = context.TBL_SETUP_GLOBAL.ToList();
                handler.UseDefaultCredentials = true;
                httpClientInstance = new HttpClient();
                httpClientInstance.DefaultRequestHeaders.ConnectionClose = false;
                client.Timeout = TimeSpan.FromSeconds(180);
                client.DefaultRequestHeaders.Authorization = token;
                client.BaseAddress = new Uri(API_URL);
                client.DefaultRequestHeaders.Accept.Clear();
                client.DefaultRequestHeaders.Accept.Add(
                new MediaTypeWithQualityHeaderValue("application/json"));

                ServicePointManager.ServerCertificateValidationCallback += (sender, cert, chain, sslPolicyErrors) => true;
                requestDatetime = DateTime.Now;
                response = client.PostAsync(apiUrl, new StringContent(
                                                new JavaScriptSerializer().Serialize(model), Encoding.UTF8, "application/json")).Result;
                responseDateTime = DateTime.Now;

                if (response.IsSuccessStatusCode)
                {
                    responseApi = await response.Content.ReadAsAsync<MainResponseLoanPrepaymentViewModel>();

                    var res = new ResponseMessageViewModel
                    {
                        responseCode = responseApi.response_code,
                        responseStatus = responseApi.response_message == "Successful" ? true : false,
                    };

                    responseMsg = new ResponseMessage
                    {
                        APIResponse = res,
                        APIStatus = response.IsSuccessStatusCode,
                        Message = response
                    };
                }
                else
                {
                    responseMsg = new ResponseMessage
                    {
                        APIResponse = null,
                        APIStatus = response.IsSuccessStatusCode,
                        Message = response
                    };
                }

                responseJson = await response.Content.ReadAsStringAsync();
                responseMsg.responseMessage = responseJson;
                return responseApi;
            }
            catch (Exception ex)
            {
                var innerExceptionMessage = "";
                if (ex.InnerException != null)
                    innerExceptionMessage = ex.InnerException.Message;

                throw new APIErrorException($"Core Banking API Error - {ex.Message} - inner exception - {innerExceptionMessage}");
            }

            finally
            {
                handler.Dispose();
                client.Dispose();

                var logs = new TBL_CUSTOM_API_LOGS
                {
                    APIURL = API_URL + apiUrl,
                    LOGTYPEID = 5,
                    REFERENCENUMBER = model.user_ref_no,
                    REQUESTDATETIME = requestDatetime,
                    REQUESTMESSAGE = inputJson,
                    RESPONSEDATETIME = responseDateTime,
                    RESPONSEMESSAGE = responseJson,
                };

                FinTrakBankingContext logContext = new FinTrakBankingContext();
                logContext.TBL_CUSTOM_API_LOGS.Add(logs);
                logContext.SaveChanges();
            }

        }

        public async Task<MainResponseLoanPrepaymentViewModel> GetTodayLoanRepaymentByRefNo(LoanPrepaymentViewModel model)
        {
            HttpClientHandler handler = new HttpClientHandler();
            HttpClient httpClientInstance;

            HttpClient client = new HttpClient(handler);
            var inputJson = new JavaScriptSerializer().Serialize(model);
            DateTime requestDatetime = new DateTime(), responseDateTime = new DateTime();
            HttpResponseMessage response = null;

            MainResponseLoanPrepaymentViewModel responseApi = new MainResponseLoanPrepaymentViewModel();
            ResponseMessage responseMsg = null;
            string responseJson = "";

            getAPIURLSettings("LoanPrepayment");
            string apiUrl = "GetTodayLoanRepaymentByRefNo";

            try
            {
                var token = new AuthenticationHeaderValue("Authorization", API_KEY);
                var dta = context.TBL_SETUP_GLOBAL.ToList();
                handler.UseDefaultCredentials = true;
                httpClientInstance = new HttpClient();
                httpClientInstance.DefaultRequestHeaders.ConnectionClose = false;
                client.Timeout = TimeSpan.FromSeconds(180);
                client.DefaultRequestHeaders.Authorization = token;
                client.BaseAddress = new Uri(API_URL);
                client.DefaultRequestHeaders.Accept.Clear();
                client.DefaultRequestHeaders.Accept.Add(
                new MediaTypeWithQualityHeaderValue("application/json"));

                ServicePointManager.ServerCertificateValidationCallback += (sender, cert, chain, sslPolicyErrors) => true;
                requestDatetime = DateTime.Now;
                response = client.PostAsync(apiUrl, new StringContent(
                                                new JavaScriptSerializer().Serialize(model), Encoding.UTF8, "application/json")).Result;
                responseDateTime = DateTime.Now;

                if (response.IsSuccessStatusCode)
                {
                    responseApi = await response.Content.ReadAsAsync<MainResponseLoanPrepaymentViewModel>();

                    var res = new ResponseMessageViewModel
                    {
                        responseCode = responseApi.response_code,
                        responseStatus = responseApi.response_message == "Successful" ? true : false,
                    };

                    responseMsg = new ResponseMessage
                    {
                        APIResponse = res,
                        APIStatus = response.IsSuccessStatusCode,
                        Message = response
                    };
                }
                else
                {
                    responseMsg = new ResponseMessage
                    {
                        APIResponse = null,
                        APIStatus = response.IsSuccessStatusCode,
                        Message = response
                    };
                }

                responseJson = await response.Content.ReadAsStringAsync();
                responseMsg.responseMessage = responseJson;
                return responseApi;
            }
            catch (Exception ex)
            {
                var innerExceptionMessage = "";
                if (ex.InnerException != null)
                    innerExceptionMessage = ex.InnerException.Message;

                throw new APIErrorException($"Core Banking API Error - {ex.Message} - inner exception - {innerExceptionMessage}");
            }

            finally
            {
                handler.Dispose();
                client.Dispose();

                var logs = new TBL_CUSTOM_API_LOGS
                {
                    APIURL = API_URL + apiUrl,
                    LOGTYPEID = 5,
                    REFERENCENUMBER = model.user_ref_no,
                    REQUESTDATETIME = requestDatetime,
                    REQUESTMESSAGE = inputJson,
                    RESPONSEDATETIME = responseDateTime,
                    RESPONSEMESSAGE = responseJson,
                };

                FinTrakBankingContext logContext = new FinTrakBankingContext();
                logContext.TBL_CUSTOM_API_LOGS.Add(logs);
                logContext.SaveChanges();
            }

        }

        public async Task<MainResponseLoanPrepaymentViewModel> GetTodayLoanSumRepaymentByRefNo(LoanPrepaymentViewModel model)
        {
            HttpClientHandler handler = new HttpClientHandler();
            HttpClient httpClientInstance;

            HttpClient client = new HttpClient(handler);
            var inputJson = new JavaScriptSerializer().Serialize(model);
            DateTime requestDatetime = new DateTime(), responseDateTime = new DateTime();
            HttpResponseMessage response = null;

            MainResponseLoanPrepaymentViewModel responseApi = new MainResponseLoanPrepaymentViewModel();
            ResponseMessage responseMsg = null;
            string responseJson = "";

            getAPIURLSettings("LoanPrepayment");
            string apiUrl = "GetTodayLoanSumRepaymentByRefNo";

            try
            {
                var token = new AuthenticationHeaderValue("Authorization", API_KEY);
                var dta = context.TBL_SETUP_GLOBAL.ToList();
                handler.UseDefaultCredentials = true;
                httpClientInstance = new HttpClient();
                httpClientInstance.DefaultRequestHeaders.ConnectionClose = false;
                client.Timeout = TimeSpan.FromSeconds(180);
                client.DefaultRequestHeaders.Authorization = token;
                client.BaseAddress = new Uri(API_URL);
                client.DefaultRequestHeaders.Accept.Clear();
                client.DefaultRequestHeaders.Accept.Add(
                new MediaTypeWithQualityHeaderValue("application/json"));

                ServicePointManager.ServerCertificateValidationCallback += (sender, cert, chain, sslPolicyErrors) => true;
                requestDatetime = DateTime.Now;
                response = client.PostAsync(apiUrl, new StringContent(
                                                new JavaScriptSerializer().Serialize(model), Encoding.UTF8, "application/json")).Result;
                responseDateTime = DateTime.Now;

                if (response.IsSuccessStatusCode)
                {
                    responseApi = await response.Content.ReadAsAsync<MainResponseLoanPrepaymentViewModel>();

                    var res = new ResponseMessageViewModel
                    {
                        responseCode = responseApi.response_code,
                        responseStatus = responseApi.response_message == "Successful" ? true : false,
                    };

                    responseMsg = new ResponseMessage
                    {
                        APIResponse = res,
                        APIStatus = response.IsSuccessStatusCode,
                        Message = response
                    };
                }
                else
                {
                    responseMsg = new ResponseMessage
                    {
                        APIResponse = null,
                        APIStatus = response.IsSuccessStatusCode,
                        Message = response
                    };
                }

                responseJson = await response.Content.ReadAsStringAsync();
                responseMsg.responseMessage = responseJson;
                return responseApi;
            }
            catch (Exception ex)
            {
                var innerExceptionMessage = "";
                if (ex.InnerException != null)
                    innerExceptionMessage = ex.InnerException.Message;

                throw new APIErrorException($"Core Banking API Error - {ex.Message} - inner exception - {innerExceptionMessage}");
            }

            finally
            {
                handler.Dispose();
                client.Dispose();

                var logs = new TBL_CUSTOM_API_LOGS
                {
                    APIURL = API_URL + apiUrl,
                    LOGTYPEID = 5,
                    REFERENCENUMBER = model.user_ref_no,
                    REQUESTDATETIME = requestDatetime,
                    REQUESTMESSAGE = inputJson,
                    RESPONSEDATETIME = responseDateTime,
                    RESPONSEMESSAGE = responseJson,
                };

                FinTrakBankingContext logContext = new FinTrakBankingContext();
                logContext.TBL_CUSTOM_API_LOGS.Add(logs);
                logContext.SaveChanges();
            }

        }

        public async Task<ResponseLoanPrepaymentViewModel> GetOverdraftRepayment(LoanPrepaymentViewModel model)
        {
            HttpClientHandler handler = new HttpClientHandler();
            HttpClient httpClientInstance;

            HttpClient client = new HttpClient(handler);
            var inputJson = new JavaScriptSerializer().Serialize(model);
            DateTime requestDatetime = new DateTime(), responseDateTime = new DateTime();
            HttpResponseMessage response = null;

            ResponseLoanPrepaymentViewModel responseApi = new ResponseLoanPrepaymentViewModel();
            ResponseMessage responseMsg = null;
            string responseJson = "";

            getAPIURLSettings("LoanPrepayment");
            string apiUrl = "GetOverdraftRepayment";

            try
            {
                var token = new AuthenticationHeaderValue("Authorization", API_KEY);
                var dta = context.TBL_SETUP_GLOBAL.ToList();
                handler.UseDefaultCredentials = true;
                httpClientInstance = new HttpClient();
                httpClientInstance.DefaultRequestHeaders.ConnectionClose = false;
                client.Timeout = TimeSpan.FromSeconds(180);
                client.DefaultRequestHeaders.Authorization = token;
                client.BaseAddress = new Uri(API_URL);
                client.DefaultRequestHeaders.Accept.Clear();
                client.DefaultRequestHeaders.Accept.Add(
                new MediaTypeWithQualityHeaderValue("application/json"));

                ServicePointManager.ServerCertificateValidationCallback += (sender, cert, chain, sslPolicyErrors) => true;
                requestDatetime = DateTime.Now;
                response = client.PostAsync(apiUrl, new StringContent(
                                                new JavaScriptSerializer().Serialize(model), Encoding.UTF8, "application/json")).Result;
                responseDateTime = DateTime.Now;

                if (response.IsSuccessStatusCode)
                {
                    responseApi = await response.Content.ReadAsAsync<ResponseLoanPrepaymentViewModel>();

                    var res = new ResponseMessageViewModel
                    {
                        responseCode = responseApi.response_code,
                        responseStatus = responseApi.response_message == "Successful" ? true : false,
                    };

                    responseMsg = new ResponseMessage
                    {
                        APIResponse = res,
                        APIStatus = response.IsSuccessStatusCode,
                        Message = response
                    };
                }
                else
                {
                    responseMsg = new ResponseMessage
                    {
                        APIResponse = null,
                        APIStatus = response.IsSuccessStatusCode,
                        Message = response
                    };
                }

                responseJson = await response.Content.ReadAsStringAsync();
                responseMsg.responseMessage = responseJson;
                return responseApi;
            }
            catch (Exception ex)
            {
                var innerExceptionMessage = "";
                if (ex.InnerException != null)
                    innerExceptionMessage = ex.InnerException.Message;

                throw new APIErrorException($"Core Banking API Error - {ex.Message} - inner exception - {innerExceptionMessage}");
            }

            finally
            {
                handler.Dispose();
                client.Dispose();

                var logs = new TBL_CUSTOM_API_LOGS
                {
                    APIURL = API_URL + apiUrl,
                    LOGTYPEID = 5,
                    REFERENCENUMBER = model.user_ref_no,
                    REQUESTDATETIME = requestDatetime,
                    REQUESTMESSAGE = inputJson,
                    RESPONSEDATETIME = responseDateTime,
                    RESPONSEMESSAGE = responseJson,
                };

                FinTrakBankingContext logContext = new FinTrakBankingContext();
                logContext.TBL_CUSTOM_API_LOGS.Add(logs);
                logContext.SaveChanges();
            }

        }


        public bool GetLoanRepaymentToStaging()
        {
            try
            {
                MainResponseLoanPrepaymentViewModel response = new MainResponseLoanPrepaymentViewModel();
                LoanPrepaymentViewModel model = new LoanPrepaymentViewModel();
                model.auth_key = API_KEY;
                model.channel_code = "FINTRAK";
                //model.review_date = "23-May-2020";
                model.review_date = DateTime.Now.Date.ToString("dd-MMM-yyyy");

                Task.Run(async () => response = await GetTodayRepaymentLoans(model)).GetAwaiter().GetResult();
                if (response.response_code == "00")
                {
                    var existingRecords = context2.STG_CONTRACT_DAILY_REPAY.Where(x => x.AMOUNTPAID > 0 && DbFunctions.TruncateTime(x.PAYMENTDATE) == DbFunctions.TruncateTime(DateTime.Now)).Select(x => x.CONTRACTREFERENCENUMBER).ToList();
                    var repaymentDataReceived = response.getrepaymentdetailsresp.Where(x => !existingRecords.Contains(x.account_number)).ToList();
                    //var repaymentDataReceived = response.getrepaymentdetailsresp.ToList();

                    var stagingdata = new List<STG_CONTRACT_DAILY_REPAY>();
                    foreach (var itemReceived in repaymentDataReceived)
                    {
                        var data = new STG_CONTRACT_DAILY_REPAY
                        {
                            CONTRACTREFERENCENUMBER = itemReceived.account_number,
                            LOANSYSTEMTYPEID = (short)LoanSystemTypeEnum.TermDisbursedFacility,
                            CUSTOMERACCOUNTNUMBER = itemReceived.customer_acct,
                            BRANCHCODE = itemReceived.branch_code,
                            PAYMENTDESCRIPTION = itemReceived.component_name,
                            DUEDATE = itemReceived.due_date,
                            PAYMENTDATE = itemReceived.paid_date,
                            AMOUNTPAID = itemReceived.amount_paid,
                            STATUS = false
                        };
                        stagingdata.Add(data);
                    }
                    context2.STG_CONTRACT_DAILY_REPAY.AddRange(stagingdata);

                    var saved = context2.SaveChanges() > 0;
                    if (saved)
                    {
                        return true;
                    }

                };

                return false;
            }
            catch (Exception e)
            {
                throw e;
            }
        }

        public bool GetOverdraftRepaymentToStaging()
        {
            try
            {
                ResponseLoanPrepaymentViewModel response = new ResponseLoanPrepaymentViewModel();
                LoanPrepaymentViewModel model = new LoanPrepaymentViewModel();
                List<STG_OVERDRAFT_DAILY_REPAY> dataList = new List<STG_OVERDRAFT_DAILY_REPAY>();

                model.auth_key = API_KEY;
                model.channel_code = "FINTRAK";
                //model.review_date = "24-May-2020";
                model.review_date = DateTime.Now.Date.ToString("dd-MMM-yyyy");

                var loans = (from x in context.TBL_LOAN_REVOLVING
                             join c in context.TBL_CASA on x.CASAACCOUNTID equals c.CASAACCOUNTID
                             join cust in context.TBL_CUSTOMER on c.CUSTOMERID equals cust.CUSTOMERID
                             where x.LOANSTATUSID != (short)LoanStatusEnum.Inactive
                             && x.LOANSTATUSID != (short)LoanStatusEnum.Cancelled
                             && x.LOANSTATUSID != (short)LoanStatusEnum.Completed
                              && x.LOANSTATUSID != (short)LoanStatusEnum.Terminated

                             select new SubResponseLoanPrepaymentViewModel()
                             {
                                 account_number = x.LOANREFERENCENUMBER,
                                 customer_acct = c.PRODUCTACCOUNTNUMBER,
                                 user_ref_no = cust.CUSTOMERCODE,
                                 account_balance = 0,
                                 creditTurnover = 0,
                                 debitTurnover = 0,
                                 transactionDate = DateTime.Now,
                             }).ToList();

                foreach (var item in loans)
                {
                    model.account_no = item.customer_acct;
                    Task.Run(async () => response = await GetOverdraftRepayment(model)).GetAwaiter().GetResult();
                    if (response.response_code == "00")
                    {
                        var repaymentDataReceived = response;

                        var data = new STG_OVERDRAFT_DAILY_REPAY
                        {
                            LOANSYSTEMTYPEID = (short)LoanSystemTypeEnum.OverdraftFacility,
                            CUSTOMERACCOUNTNUMBER = item.customer_acct,
                            ACCOUNTBALANCE = item.account_balance,
                            CREDITTURNOVER = item.creditTurnover,
                            DEBITTURNOVER = item.debitTurnover,
                            TRANSACTIONDATE = item.transactionDate,
                            STATUS = false,
                        };

                        dataList.Add(data);

                    }

                }
                context2.STG_OVERDRAFT_DAILY_REPAY.AddRange(dataList);

                return context2.SaveChanges() > 0;
            }
            catch (Exception e)
            {
                throw e;
            }
        }

        public bool postPaymentEntries()
        {
            var unReconciledPayLog = context2.STG_CONTRACT_DAILY_REPAY.Where(x => x.STATUS == false && x.LOANSYSTEMTYPEID == (short)LoanSystemTypeEnum.TermDisbursedFacility).ToList();
            var BatchCode = CommonHelpers.GenerateRandomDigitCode(10);

            List<TBL_FINANCE_TRANSACTION> financePostingList = new List<TBL_FINANCE_TRANSACTION>();
            foreach (var item in unReconciledPayLog)
            {
                var loanAccount = context.TBL_LOAN.Where(x => x.COREBANKINGREF == item.CONTRACTREFERENCENUMBER).FirstOrDefault();

                if (loanAccount != null)
                {
                    var casa = context.TBL_CASA.Where(x => x.CASAACCOUNTID == loanAccount.CASAACCOUNTID && x.COMPANYID == loanAccount.COMPANYID).FirstOrDefault();
                    var product = context.TBL_PRODUCT.Where(x => x.PRODUCTID == loanAccount.PRODUCTID && x.COMPANYID == loanAccount.COMPANYID).FirstOrDefault();
                    var repaymentAccountGL = context.TBL_PRODUCT.Where(x => x.PRODUCTID == casa.PRODUCTID).Select(x => x.PRINCIPALBALANCEGL.Value).FirstOrDefault();



                    FinanceTransactionStagingViewModel newFinancialReturn = new FinanceTransactionStagingViewModel()
                    {
                        creditGlAccountId = repaymentAccountGL,
                        sourceReferenceNumber = loanAccount.LOANREFERENCENUMBER,
                        creditCasaAccountId = loanAccount.CASAACCOUNTID2,
                        debitCasaAccountId = loanAccount.CASAACCOUNTID,
                        description = item.PAYMENTDESCRIPTION,
                        amount = item.AMOUNTPAID,
                        valueDate = item.DUEDATE,
                        currencyId = loanAccount.CURRENCYID,
                        destinationBranchId = loanAccount.BRANCHID,
                        // sourceApplicationId = 0,
                    };

                    if (item.PAYMENTDESCRIPTION == "MAIN_INT") newFinancialReturn.operationId = (short)OperationsEnum.InterestLoanRepayment;
                    //else if (item.PAYMENTDESCRIPTION == "") newFinancialReturn.operationId = (short)OperationsEnum.PrincipalLoanRepayment;


                    //PAYMENT DESCRIPTION IS UNKOWN
                    if (newFinancialReturn.operationId > 0)
                    {

                        TBL_FINANCE_TRANSACTION financePosting = new TBL_FINANCE_TRANSACTION();
                        financePosting.CURRENCYID = (short)newFinancialReturn.currencyId;
                        financePosting.CURRENCYRATE = loanAccount.EXCHANGERATE;
                        financePosting.DEBITAMOUNT = newFinancialReturn.amount;
                        financePosting.CREDITAMOUNT = newFinancialReturn.amount;
                        financePosting.SOURCEREFERENCENUMBER = newFinancialReturn.sourceReferenceNumber;
                        financePosting.SOURCEBRANCHID = (short)loanAccount.TERMLOANID;
                        financePosting.SOURCEAPPLICATIONID = newFinancialReturn.sourceApplicationId;
                        financePosting.GLACCOUNTID = newFinancialReturn.creditGlAccountId;
                        financePosting.CASAACCOUNTID = newFinancialReturn.creditCasaAccountId;
                        financePosting.OPERATIONID = newFinancialReturn.operationId;
                        financePosting.DESCRIPTION = newFinancialReturn.description;
                        financePosting.BATCHCODE = BatchCode;
                        financePosting.BATCHCODE2 = "";
                        financePosting.COMPANYID = loanAccount.COMPANYID;
                        financePosting.APPROVEDDATETIME = item.PAYMENTDATE;
                        // financePosting.APPROVEDBY = item.
                        item.STATUS = true;
                        financePostingList.Add(financePosting);
                    }
                }
            }
            context.TBL_FINANCE_TRANSACTION.AddRange(financePostingList);
            return context.SaveChanges() > 0;

        }

    }
}
