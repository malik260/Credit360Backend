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
                              templateTypeName = a.TEMPLATETYPE=="1"? "EMAIL":"SMS",
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

                    /*GetDigitalLoanLiquidationModuleIncrease();
                    GetDigitalLoanExceptionNPLModuleIncrease();
                    GetDigitalLoanExceptionNPLModuleDecrease();
                    GetDigitalLoanDPDModuleIncrease();
                    GetDigitalLoanDPDModuleDecrease();
                    GetDigitalLoanDisbursementModuleIncrease();
                    GetDigitalLoanDisbursementModuleDecrease();
                    state = true;*/
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
                        
                        TimeSpan start11 = new TimeSpan(long.Parse(c.STARTTIME));
                        TimeSpan end13 = new TimeSpan(long.Parse(c.ENDTIME));

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

                        TimeSpan start11 = new TimeSpan(long.Parse(c.STARTTIME));
                        TimeSpan end13 = new TimeSpan(long.Parse(c.ENDTIME));

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
            return state;
        }


        private void CheckFailedAlertByDate()
        {
            DateTime currentDate = DateTime.Now;
            var records = context.TBL_MESSAGE_LOG.Where(m => DbFunctions.TruncateTime(m.SENDONDATETIME) < DbFunctions.TruncateTime(currentDate) && m.MESSAGESTATUSID == 1).ToList();

            if (records.Count() > 0)
            {
                foreach(var r in records)
                {
                    r.MESSAGESTATUSID = 3;
                    r.GATEWAYRESPONSE = "Email Sent Successfully";
                    r.DATETIMESENT = DateTime.Now;
                    r.DATETIMERECEIVED = DateTime.Now;
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
            }else
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
                        }if(accountOfficerFullName == null)
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
                    defaultEmail = ";" +alertTitleInfo.DEFAULTEMAIL;
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
                    if(staffFullName == "vacant" || staffFullName == "")
                    {
                        staffFullName = context.TBL_STAFF.Where(b => b.STAFFCODE == staff.misCode).Select(b => b.FIRSTNAME +""+b.MIDDLENAME+ ""+b.LASTNAME).FirstOrDefault();
                    }
                    emailList = GetBusinessUsersEmails(staff.misCode);

                    List<int> days = new List<int> { 60, 90, 30, 21, 14, 7, 3, 1 };
                    var loanInformation = context.TBL_GLOBAL_EXPOSURE.Where(d => days.Contains(DbFunctions.DiffDays(DateTime.UtcNow, d.MATURITYDATE).Value) && d.ACCOUNTOFFICERCODE == staff.misCode && d.PRINCIPALOUTSTANDINGBALLCY>0).ToList();

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
"010705099","002613587","012414846","009186554","007575637","009183686","000269367","001228108","027018975","008221731","007991841","000275291",
"009605606","000539810","009714280","009433457","008263983","009620604","009521489","027164696","013424794","009315840","000089989","009714283","007186611",
"009714276","005487999","009619908","009504575","007649222","000555921","009408770","009081261","008221745","009638717","099000806","004595330","000555948","007635219","014917842","008192575","002943518","005504494","008036216","009150751","000197038","000059060","005848102",
"007695249","005953767","000063647","009714295","014079234","007031866","000858844","014234333","006001886","000048900","000089328"};
            List<int> days = new List<int> { 30, 21, 14, 7, 5, 2, 1 };
            //&& days.Contains(DbFunctions.DiffDays(DateTime.UtcNow, d.SCHEDULEDUEDATE).Value)
            var loanRepaymentReminder = context.TBL_GLOBAL_EXPOSURE.Where(d => customerIds.Contains(d.CUSTOMERID) && d.AMOUNTDUE.Value > 0).ToList();
            var alertTitleInfo = context.TBL_ALERT_TITLE.Where(a => a.BINDINGMETHOD == "GetLoanRepaymentReminder").FirstOrDefault();
                int numberOfDays = 0;
                int daysToUse = 0;
                int numberOfInterestDays = 0;
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
                        numberOfDays = (i.SCHEDULEDUEDATE.Value - DateTime.Now).Days + 1;
                        numberOfInterestDays = (i.NEXTREPAYMENTINTDATE.Value - DateTime.Now).Days + 1;
                        if (numberOfDays > numberOfInterestDays)
                        {
                            daysToUse = numberOfInterestDays;
                        }
                        else { daysToUse = numberOfDays; }

                        var dueDate = i.SCHEDULEDUEDATE?.ToString("dd-MM-yyyy");
                        var interestDueDate = i.NEXTREPAYMENTINTDATE?.ToString("dd-MM-yyyy");
                        var amountDue = i.ALPHACODE+""+ string.Format("{0:#,##.00}", Convert.ToDecimal(i.AMOUNTDUE.Value));
                        var interestAmountDue = i.ALPHACODE + "" + string.Format("{0:#,##.00}", Convert.ToDecimal(i.UNPOINTERESTAMOUNT.Value));
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
                            emailList = i.EMAIL;
                            //emailList = emailList+";"+defaultEmail;
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
                    
                    emailList = emailList + GetAllStaffRoleEmails(alertTitleInfo.ALERTTITLEID)+ defaultEmail;
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
                    
                    emailList = emailList + GetAllStaffRoleEmails(alertTitleInfo.ALERTTITLEID)+ defaultEmail;
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

                    emailList = i.customerName+defaultEmail;
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
                                  

                    var emailList =  GetAllStaffRoleEmails(alertTitleInfo.ALERTTITLEID) + defaultEmail;

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
        public void GetFacilityRestructuredNotification() {
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

                    var loanInformation = context.TBL_LOAN_REVIEW_OPERATION.Where(d =>d.LOANID == facilityRestructured.LOANID && d.OPERATIONCOMPLETED == true && d.APPROVALSTATUSID == (int)ApprovalStatusEnum.Approved).ToList();

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
                if (alert.canFire) LogEmailAlert(alert.template, alert.alertTitle, alert.receiverEmailList, "100442", 0,alert.operationMethod);
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
            var roleEmail = (from r in context.TBL_ALERT_GROUP_EMAIL
                            join t in context.TBL_ALERT_STAFF_ROLE on r.GROUPEMAILID equals t.STAFFROLEID
                            where t.ALERTTITLEID == alerttitleId
                              select new simpleStaffModel
                              {
                                   staffCode= r.GROUPCODE,
                                   staffRoleId = r.GROUPEMAILID,
                                   email = r.GROUPEMAIL,
                              }).ToList();

            foreach (var t in roleEmail)
            {
                list = list+";"+t.email;
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
                if(t.misCode == "IBG800")
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
                        //var email = "kwaghngyise@gmail.com";
                        var email = "kwaghngyise@gmail.com;Chibuike.Mbanefo@ACCESSBANKPLC.com;PAUL.ASIEMO@accessbankplc.com;OLUKAYODE.AJAYI@ACCESSBANKPLC.com";
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
                        //var email = "kwaghngyise@gmail.com";
                        var email = "kwaghngyise@gmail.com;Chibuike.Mbanefo@ACCESSBANKPLC.com;PAUL.ASIEMO@accessbankplc.com;OLUKAYODE.AJAYI@ACCESSBANKPLC.com";
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
                       // var email = "kwaghngyise@gmail.com";
                        var email = "kwaghngyise@gmail.com;Chibuike.Mbanefo@ACCESSBANKPLC.com;PAUL.ASIEMO@accessbankplc.com;OLUKAYODE.AJAYI@ACCESSBANKPLC.com";
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
                        //var email = "kwaghngyise@gmail.com";
                        var email = "kwaghngyise@gmail.com;Chibuike.Mbanefo@ACCESSBANKPLC.com;PAUL.ASIEMO@accessbankplc.com;OLUKAYODE.AJAYI@ACCESSBANKPLC.com";
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
                        //var email = "kwaghngyise@gmail.com";
                        var email = "kwaghngyise@gmail.com;Chibuike.Mbanefo@ACCESSBANKPLC.com;PAUL.ASIEMO@accessbankplc.com;OLUKAYODE.AJAYI@ACCESSBANKPLC.com";
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
                        //var email = "kwaghngyise@gmail.com";
                        var email = "kwaghngyise@gmail.com;Chibuike.Mbanefo@ACCESSBANKPLC.com;PAUL.ASIEMO@accessbankplc.com;OLUKAYODE.AJAYI@ACCESSBANKPLC.com";
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
                        //var email = "kwaghngyise@gmail.com";
                        var email = "kwaghngyise@gmail.com;Chibuike.Mbanefo@ACCESSBANKPLC.com;PAUL.ASIEMO@accessbankplc.com;OLUKAYODE.AJAYI@ACCESSBANKPLC.com";
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
                        //var email = "kwaghngyise@gmail.com";
                        var email = "kwaghngyise@gmail.com;Chibuike.Mbanefo@ACCESSBANKPLC.com;PAUL.ASIEMO@accessbankplc.com;OLUKAYODE.AJAYI@ACCESSBANKPLC.com";
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
                        //var email = "kwaghngyise@gmail.com";
                        var email = "kwaghngyise@gmail.com;Chibuike.Mbanefo@ACCESSBANKPLC.com;PAUL.ASIEMO@accessbankplc.com;OLUKAYODE.AJAYI@ACCESSBANKPLC.com";
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
                        //var email = "kwaghngyise@gmail.com";
                        var email = "kwaghngyise@gmail.com;Chibuike.Mbanefo@ACCESSBANKPLC.com;PAUL.ASIEMO@accessbankplc.com;OLUKAYODE.AJAYI@ACCESSBANKPLC.com";
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
                        //var email = "kwaghngyise@gmail.com";
                        var email = "kwaghngyise@gmail.com;Chibuike.Mbanefo@ACCESSBANKPLC.com;PAUL.ASIEMO@accessbankplc.com;OLUKAYODE.AJAYI@ACCESSBANKPLC.com";
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
                        //var email = "kwaghngyise@gmail.com";
                        var email = "kwaghngyise@gmail.com;Chibuike.Mbanefo@ACCESSBANKPLC.com;PAUL.ASIEMO@accessbankplc.com;OLUKAYODE.AJAYI@ACCESSBANKPLC.com";
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
            var sectorBankSum = context2.TBL_SECTOR_LIMIT_ALERT.Where(a=>a.SECTOR != null).Sum(a=>a.BANK);
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
                        emailList = defaultEmail+";"+ GetAllCreditPortfolioStaffEmails();
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
                    var alertTemplate = "Dear "+ repaymentDefaulter.customerName+" <br/>You have an outstanding repayment of "+ amount+" kindly regularise";
                    string emailList = repaymentDefaulter.customerEmail+";"+ repaymentDefaulter.guarantorEmail;
                           
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

                            emailList = emailList + ";"+ aboutToExpired.accreditedConsultantEmail + defaultEmail;
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
                    var repaymentDataReceived = response.getrepaymentdetailsresp.Where(x=> !existingRecords.Contains(x.account_number)).ToList();
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
