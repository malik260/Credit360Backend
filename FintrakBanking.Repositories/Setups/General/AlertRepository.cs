using FintrakBanking.Common;
using FintrakBanking.Common.CustomException;
using FintrakBanking.Common.Enum;
using FintrakBanking.Entities.Models;
using FintrakBanking.Entities.StagingModels;
using FintrakBanking.Interfaces.Admin;
using FintrakBanking.Interfaces.Credit;
using FintrakBanking.Interfaces.Setups.General;
using FintrakBanking.ViewModels;
using FintrakBanking.ViewModels.Finance;
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
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Web.Script.Serialization;

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
        private string pass = ConfigurationManager.AppSettings["pass"];
        //private ILoanArchiveRepository loanArchive;
        private string maxUsers = ConfigurationManager.AppSettings["muTrace"];

        private string onePercent = "1%";
        private string onePointFivePercent = "1.5%";
        private string twoPercent = "2%";
        private string fivePercent = "5%";
        
        private string fifteenPercent = "15%";
        private string twentyFivePercent = "25%";
        

        private double onePercentValue = 0.01;
        private double onePointFivePercentValue = 0.015;
        private double twoPercentValue = 0.02;
        private double fivePercentValue = 0.05;
        private double fifteenPercentValue = 0.15;
        private double twentyFivePercentValue = 0.25;

        private double tenPercentValue = 0.1;
        private double twentyPercentValue = 0.2;
        private double thirtyPercentValue = 0.3;
        private double fiftyPercentValue = 0.5;
        private double fortyPercentValue = 0.4;
        private double sixtyPercentValue = 0.6;

        private string eightyFivePercent = "85%";
        private string ninetyPercent = "90%";
        private string ninetyFivePercent = "95%";

        private double eightyFivePercentValue = 0.85;
        private double ninetyPercentValue = 0.9;
        private double ninetyFivePercentValue = 0.95;

        private string eightyFiveToNinety = "YELLOW: 85% - 90%";
        private string ninetyToNinetyFive = "AMBER: 90% - 95%";
        private string ninetyFiveAbove = "RED: 95%";

        private string fortyPercent = "40%";
        private string sixtyPercent = "60%";
        private string twentyPercent = "20%";
        private string thirtyPercent = "30%";
        private string fiftyPercent = "50%";
        private string tenPercent = "10%";


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
                          join b in context.TBL_ALERT_SCHEDULE on a.ALERTTITLEID equals b.ALERTTITLEID into p
                          from b in p.DefaultIfEmpty()
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
                              bindingMethodId = a.ALERTTITLEID,
                              isActive = a.ISACTIVE,
                              alertScheduleId = b.ALERTSCHEDULEID,
                              frequencyId = b.FREQUENCYID,
                              alertTime = b.ALERTTIME,
                              timeFrom = b.TIMEFROM,
                              timeTo = b.TIMETO,
                              frequency = b.FREQUENCYID,
                          }).OrderBy(x=>x.title).ToList(); 

             var alertsMethods = (from c in context.TBL_ALERT_BINDING_METHODS
                                 join a in context.TBL_ALERT_TITLE on c.METHODTNAME equals a.BINDINGMETHOD into p
                                 from a in p.DefaultIfEmpty()
                                 join b in context.TBL_ALERT_SCHEDULE on a.ALERTTITLEID equals b.ALERTTITLEID into x
                                 from b in x.DefaultIfEmpty()
                                 select new AlertTitleViewModel
                                  {
                                      alertTitleId = c.BINDINGMEHTODID,
                                      title = c.METHODTITLE,
                                      bindingMethod = c.METHODTNAME,
                                      bindingMethodId = c.BINDINGMEHTODID,
                                      template = a.TEMPLATE,
                                      templateType = a.TEMPLATETYPE,
                                      businessOwner = a.BUSINESSOWNER,
                                      senderEmail = a.SENDEREMAIL,
                                      senderName = a.SENDERNAME,
                                      templateTypeName = a.TEMPLATETYPE == "1" ? "EMAIL" : "SMS",
                                      defaultEmail = a.DEFAULTEMAIL,
                                      lastSentDate = a.LASTSENTDATE,
                                      actionStatus = a.ACTIONSTATUS,
                                      //isActive = a.ISACTIVE
                                      alertScheduleId = b.ALERTSCHEDULEID,
                                      frequencyId = b.FREQUENCYID,
                                      alertFrequencyId = b.FREQUENCYID,
                                      frequency = b.FREQUENCYID,
                                      alertTime = b.ALERTTIME,
                                      timeFrom = b.TIMEFROM,
                                      timeTo = b.TIMETO,
                                  }).OrderBy(x => x.title).ToList();

            var data = alerts.Union(alertsMethods);
            return data;
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
                          join b in context.TBL_ALERT_SCHEDULE on a.ALERTTITLEID equals b.ALERTTITLEID into p
                          from b in p.DefaultIfEmpty()
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
                              isActive = a.ISACTIVE,
                              alertScheduleId = b.ALERTSCHEDULEID,
                              frequencyId = b.FREQUENCYID,
                              alertTime = b.ALERTTIME,
                              timeFrom = b.TIMEFROM,
                              timeTo = b.TIMETO,
                          }).OrderBy(x => x.title).ToList();

            var alertsMethods = (from c in context.TBL_ALERT_BINDING_METHODS
                                 select new AlertTitleViewModel
                                 {
                                     alertTitleId = c.BINDINGMEHTODID,
                                     title = c.METHODTITLE,
                                     bindingMethod = c.METHODTNAME,
                                     bindingMethodId = c.BINDINGMEHTODID,
                                 }).OrderBy(x => x.title).ToList();

            var data = alerts.Union(alertsMethods);
            return data;
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
                             isActive = a.ISACTIVE
                         }).FirstOrDefault();
            return alert;
        }
        public bool AddAlertTitle(AlertTitleViewModel model)
        {
            //var rec = context.TBL_ALERT_BINDING_METHODS.Where(x=>x.BINDINGMEHTODID == model.bindingMethodId).FirstOrDefault();
            
            //if(rec != null)
            //{
            //    model.bindingMethod = rec.METHODTNAME;
            //}
            //else
            //{
            //    var rec2 = context.TBL_ALERT_TITLE.Where(x=>x.BINDINGMETHOD == model.bindingMethodId);
            //    model.bindingMethod = rec2.BINDINGMETHOD;
            //}

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
                ISACTIVE = true,
                //LASTSENTDATE = general.GetApplicationDate(),
                ACTIONSTATUS = 1,
            };

            context.TBL_ALERT_TITLE.Add(entity);
            if (context.SaveChanges() > 0)
            {
                var schedule = new TBL_ALERT_SCHEDULE
                {
                    FREQUENCYID = (int)model.frequency,
                    ALERTTIME = model.alertTime,
                    TIMEFROM = model.timeFrom,
                    TIMETO = model.timeTo,
                    ALERTTITLEID = entity.ALERTTITLEID,
                };
                context.TBL_ALERT_SCHEDULE.Add(schedule);
            }
            

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
            if (entity != null)
            {
                entity.TITLE = model.title;
                entity.TEMPLATE = model.template;
                entity.BUSINESSOWNER = model.businessOwner;
                entity.SENDERNAME = model.senderName;
                entity.SENDEREMAIL = model.senderEmail;
                entity.TEMPLATETYPE = model.templateType;
                entity.DEFAULTEMAIL = model.defaultEmail;
                //entity.BINDINGMETHOD = model.bindingMethod;
                entity.ACTIONSTATUS = 1;

                var schedule = this.context.TBL_ALERT_SCHEDULE.Find(entity.ALERTTITLEID);
                if (schedule != null)
                {
                    schedule.FREQUENCYID = (int)model.frequency;
                    schedule.ALERTTIME = model.alertTime;
                    schedule.TIMEFROM = model.timeFrom;
                    schedule.TIMETO = model.timeTo;
                    schedule.ALERTTITLEID = entity.ALERTTITLEID;
                }
                context.SaveChanges();
            }
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



        public bool UpdateAlertTitleStatus(AlertTitleViewModel model)
        {
            var entity = this.context.TBL_ALERT_TITLE.Find(model.alertTitleId);
            if (entity != null)
            {
                entity.ISACTIVE = model.isActive;
            }
            var auditStaff = (context.TBL_STAFF.Where(x => x.STAFFID == model.createdBy).Select(x => x.STAFFCODE));
            // Audit Section ---------------------------
            this.audit.AddAuditTrail(new TBL_AUDIT
            {
                AUDITTYPEID = (short)AuditTypeEnum.AlertTitleUpdated,
                STAFFID = model.createdBy,
                BRANCHID = (short)model.userBranchId,
                DETAIL = $"TBL_ALERT_TITLE'{entity.TITLE}' was updated by {auditStaff}",
                IPADDRESS = CommonHelpers.GetLocalIpAddress(),
                URL = model.applicationUrl,
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

        public IEnumerable<AlertPlaceHoldersViewModel> GetAllAlertPlaceHoldersy()
        {
            var placeHolders = (from a in context.TBL_ALERT_PLACEHOLDER
                                select new AlertPlaceHoldersViewModel
                                 {
                                     placeHolderId = a.PLACEHOLDERID,
                                     placeHolder = a.PLACEHOLDER,
                                     description = a.DESCRIPTION,
                                     type = a.TYPE,
                                     alertTitleId = a.ALERTTITLEID,
                                     title = a.ALERTTITLEID != null ? context.TBL_ALERT_TITLE.Where(x => x.ALERTTITLEID == a.ALERTTITLEID).Select(x => x.TITLE).FirstOrDefault() : "General",
                                     partition = a.PARTITION
                                 }).GroupBy(o => o.placeHolder)
               .Select(o => o.FirstOrDefault()).ToList();
            return placeHolders;
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


            //encripted password 
            var requiredPassword = pass;
            //string encryptedstring = EncryptionHelper.Encrypt("sqluser10$");
            //Console.WriteLine("encripted Result  = " + encryptedstring);
            Console.WriteLine("");
            string decryptedstring = EncryptionHelper.Decrypt(requiredPassword);
            //Console.WriteLine("decripted Result  = " + decryptedstring);

            // corporate customer information update 
            int year = DateTime.Now.Year;
            DateTime firstDay = new DateTime(year, 1, 1);
            TimeSpan startCorporateCustomerUpdate = new TimeSpan(24, 0, 0);
            TimeSpan endCorporateCustomerUpdate = new TimeSpan(24, 30, 0);
            if (now >= startCorporateCustomerUpdate && now <= endCorporateCustomerUpdate && firstDay.Date == DateTime.Now.Date)
            {
                corporateCustomerUpdate();
            }

            //general alert
              //GeneralAlert()

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
                TimeSpan startInsurance = new TimeSpan(15, 0, 0);
                TimeSpan endInsurance = new TimeSpan(15, 30, 0);
                if ((now >= startInsurance) && (now <= endInsurance))
                {
                    GetInsurancePolicyExpirationNotification();
                    GetInsurancePolicyExpiredNotification();
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
                     /*GetSectorLimitExceedeBBDReminder();
                     GetSectorLimitExceedeCBDReminder();
                     GetSectorLimitExceedeCIBDReminder();
                     GetSectorLimitExceedeRBDReminder();
                     GetSectorLimitExceededBankReminder();*/
                     state = true;
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
                    //GetDigitalLoanLiquidationModuleDecrease();
                    GetDigitalLoanExceptionNPLModuleIncrease();
                    GetDigitalLoanExceptionNPLModuleDecrease();
                    GetDigitalLoanDPDModuleIncrease();
                    GetDigitalLoanDPDModuleDecrease();
                    GetDigitalLoanDisbursementModuleIncrease();
                    GetDigitalLoanDisbursementModuleDecrease();*/
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
                TimeSpan end = new TimeSpan(16, 30, 0);

                if ((now >= start) && (now <= end))
                {
                    //GroupImminentMaturitiesByGroupHeads();
                    //GetImminentMaturities();
                    //GetPastDueObligationsReminder();
                    //GetPastDueObligationsReminderByGroupHeads();
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

       /* public static bool CompareHash(string attemptedPassword, byte[] hash, int salt)
        {
            PasswordWithSaltHasher pwHasher = new PasswordWithSaltHasher();
            HashWithSaltResult hashResultSha512 = pwHasher.HashWithSalt(attemptedPassword, salt, SHA512.Create());
            string base64Hash = Convert.ToBase64String(hash);
            string base64AttemptedHash = hashResultSha512.Salt;
            Console.WriteLine("password match = " + base64Hash == base64AttemptedHash);
            return base64Hash == base64AttemptedHash;
        }

        private static void TestPasswordHasher()
        {
            PasswordWithSaltHasher pwHasher = new PasswordWithSaltHasher();
            //HashWithSaltResult hashResultSha256 = pwHasher.HashWithSalt("fin360user", 64, SHA256.Create());
            HashWithSaltResult hashResultSha512 = pwHasher.HashWithSalt("fin360user", 64, SHA512.Create());
            Console.WriteLine();
            Console.WriteLine("hash Result Sha512 Salt = " + hashResultSha512.Salt);
            Console.WriteLine("hash Result Sha512 Digest = " + hashResultSha512.Digest);
        }*/

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
                    if (timeDiff > 60)
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
                         || m.OPERATIONMETHOD.Trim() == "GetImminentMaturities"
                         || m.OPERATIONMETHOD.Trim() == "GetPastDueObligationsReminder"
                         || m.OPERATIONMETHOD.Trim() == "GetStaffLoanPortfolioReport"
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
                         || m.OPERATIONMETHOD.Trim() == "GetInsurancePolicyExpiredNotification"
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
            var alertTitleInfo = context.TBL_ALERT_TITLE.Where(a => a.BINDINGMETHOD == "GetImminentMaturities" && a.ISACTIVE == true).FirstOrDefault();

            var defaultEmail = "";
            if (alertTitleInfo != null && alertTitleInfo.DEFAULTEMAIL != null)
            {
                defaultEmail = alertTitleInfo.DEFAULTEMAIL;
            }
            if (alertTitleInfo != null && groupHeadsList != null && groupHeadsList.Count() > 0)
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

                        //List<int> days = new List<int> { 60, 90, 30, 21, 14, 7, 3, 1 };
                        //var loanInformation = context.TBL_GLOBAL_EXPOSURE.Where(d => days.Contains(DbFunctions.DiffDays(DateTime.UtcNow, d.MATURITYDATE).Value) && d.ACCOUNTOFFICERCODE == accountOfficer.misCode && d.PRINCIPALOUTSTANDINGBALLCY > 0).ToList();

                        var loanInformation = context.TBL_GLOBAL_EXPOSURE.Where(d => d.ACCOUNTOFFICERCODE == accountOfficer.misCode && d.PRINCIPALOUTSTANDINGBALLCY > 0).ToList();

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

                        //emailList = groupHeadDetail.EMAIL + ";" + GetAllDivisionHeadsEmails(groupHeadDetail.MISCODE) + ";" + defaultEmail + ";jobomeg@accessbankplc.com";
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
        public void GetPastDueObligationsReminderByGroupHeads()
        {
            // GetPastDueObligationsReminder method by group heads
            var groupHeadsList = externalAlertRepository.GetPastDueObligationsReminderByGroupHeads();
            var alertTitleInfo = context.TBL_ALERT_TITLE.Where(a => a.BINDINGMETHOD == "GetPastDueObligationsReminder" && a.ISACTIVE == true).FirstOrDefault();

            var defaultEmail = "";
            if (alertTitleInfo != null && alertTitleInfo.DEFAULTEMAIL != null)
            {
                defaultEmail = ";" + alertTitleInfo.DEFAULTEMAIL;
            }
            if (alertTitleInfo != null && groupHeadsList != null && groupHeadsList.Count() > 0)
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
            var alertTitleInfo = context.TBL_ALERT_TITLE.Where(a => a.BINDINGMETHOD == "GetImminentMaturities" && a.ISACTIVE == true).FirstOrDefault();

            var defaultEmail = "";
            if (alertTitleInfo != null && alertTitleInfo.DEFAULTEMAIL != null)
            {
                defaultEmail = ";" + alertTitleInfo.DEFAULTEMAIL;
            }
            if (alertTitleInfo != null && staffList != null && staffList.Count() > 0)
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
                    //var loanInformation = context.TBL_GLOBAL_EXPOSURE.Where(d => days.Contains(DbFunctions.DiffDays(DateTime.UtcNow, d.MATURITYDATE).Value) && d.ACCOUNTOFFICERCODE == staff.misCode && d.PRINCIPALOUTSTANDINGBALLCY > 0).ToList();
                    var loanInformation = context.TBL_GLOBAL_EXPOSURE.Where(d => d.ACCOUNTOFFICERCODE == staff.misCode && d.PRINCIPALOUTSTANDINGBALLCY > 0).ToList();

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

                           // emailList = emailList + GetAllStaffRoleEmails(alertTitleInfo.ALERTTITLEID) + defaultEmail;
                            emailList = defaultEmail;
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
           
            List<int> days = new List<int> { 30, 21, 14, 7, 5, 2, 1 };
            var loanRepaymentReminder = context.TBL_NEXT_PRINCIPAL_REPAYMENT.Where(d => d.AMOUNTDUE.Value > 0 && days.Contains(DbFunctions.DiffDays(DateTime.UtcNow, d.SCHEDULEDUEDATE).Value)).ToList();
            var alertTitleInfo = context.TBL_ALERT_TITLE.Where(a => a.BINDINGMETHOD == "GetLoanRepaymentReminder" && a.ISACTIVE == true).FirstOrDefault();
            int numberOfDays = 0;
            int daysToUse = 0;
            int numberOfInterestDays = 0;
            var defaultEmail = "";
            var interestDueDate = "";
            var interestAmountDue = "";

            if (alertTitleInfo!= null && alertTitleInfo.DEFAULTEMAIL != null)
            {
                defaultEmail = alertTitleInfo.DEFAULTEMAIL;
            }

            if (loanRepaymentReminder != null && loanRepaymentReminder.Count() > 0 && alertTitleInfo != null)
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


        public void GeneralAlert()
        {
            // GeneralAlert method
            var alertTitleInfo = context.TBL_ALERT_TITLE.Where(a => a.BINDINGMETHOD == "Others" && a.ISACTIVE == true).FirstOrDefault();
            var defaultEmail = "";

            if (alertTitleInfo != null && alertTitleInfo.DEFAULTEMAIL != null)
            {
                defaultEmail = alertTitleInfo.DEFAULTEMAIL;
            }

            if (alertTitleInfo != null)
            {

                List<AlertsViewModel> alerts = new List<AlertsViewModel>();
                if (alertTitleInfo.TEMPLATE.Contains("@{{businessUnit}}"))
                {
                    defaultEmail = defaultEmail+";"+GetAllAlertGroupEmailToSend(alertTitleInfo.ALERTTITLEID);
                }

                if (alertTitleInfo.TEMPLATE.Contains("@{{roles}}"))
                {
                    defaultEmail = defaultEmail + ";" + GetAllAlertGroupEmailToSendToRole(alertTitleInfo.ALERTTITLEID);
                }

                AlertsViewModel alert = new AlertsViewModel();
                    var alertTitle = alertTitleInfo.TITLE;
                    var alertTemplate = alertTitleInfo.TEMPLATE;
                    
                        string emailList = "";
                        emailList = emailList + ";" + defaultEmail;
                        alert.receiverEmailList.Add(emailList);
                        alert.template = alertTemplate;
                        alert.alertTitle = alertTitle;
                        alert.canFire = true;
                        alert.operationMethod = alertTitleInfo.BINDINGMETHOD;
                        alerts.Add(alert);
                    
                
                SendAlertNotification(alerts);
            }
        }
        


        public void GetInsurancePolicyExpirationNotification()
        {
            // GetInsurancePolicyExpirationNotification method
            List<int> days = new List<int> { 60, 30, 21, 14, 7, 5, 2, 1 };
            var insurancePolicyNotification = context.TBL_COLLATERAL_INSURANCE_TRACKING.Where(d => days.Contains(DbFunctions.DiffDays(DateTime.UtcNow, d.INSURANCEENDDATE).Value) && d.DELETED == false && d.INSURANCESTATUSID != (int)InsuranceStatusEnum.Expired).ToList();
            var alertTitleInfo = context.TBL_ALERT_TITLE.Where(a => a.BINDINGMETHOD == "GetInsurancePolicyExpirationNotification" && a.ISACTIVE == true).FirstOrDefault();
            var defaultEmail = "";
            var emailList = "";
            if (alertTitleInfo != null && alertTitleInfo.DEFAULTEMAIL != null)
            {
                defaultEmail = ";" + alertTitleInfo.DEFAULTEMAIL;
            }

            if (alertTitleInfo != null && insurancePolicyNotification.Count() > 0)
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



        public void GetInsurancePolicyExpiredNotification()
        {
            // GetInsurancePolicyExpirationNotification method
            var insurancePolicyNotification = context.TBL_COLLATERAL_INSURANCE_TRACKING.Where(d => DbFunctions.DiffDays(DateTime.UtcNow, d.INSURANCEENDDATE).Value < 1 && d.DELETED == false).ToList();
            var alertTitleInfo = context.TBL_ALERT_TITLE.Where(a => a.BINDINGMETHOD == "GetInsurancePolicyExpiredNotification" && a.ISACTIVE == true).FirstOrDefault();
            var defaultEmail = "";
            var emailList = "";
            if (alertTitleInfo != null && alertTitleInfo.DEFAULTEMAIL != null)
            {
                defaultEmail = ";" + alertTitleInfo.DEFAULTEMAIL;
            }

            if (alertTitleInfo != null && insurancePolicyNotification.Count() > 0)
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
                    var rmEmail = context.TBL_STAFF.Where(x => x.STATEID == staff.SUPERVISOR_STAFFID).Select(x => x.EMAIL)?.FirstOrDefault();
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
            var alertTitleInfo = context.TBL_ALERT_TITLE.Where(a => a.BINDINGMETHOD == "GetCreditCardMaturingObligations" && a.ISACTIVE == true).FirstOrDefault();

            var defaultEmail = "";
            if (alertTitleInfo != null && alertTitleInfo.DEFAULTEMAIL != null)
            {
                defaultEmail = ";" + alertTitleInfo.DEFAULTEMAIL;
            }

            if (alertTitleInfo != null && staffCreditCardMaturingObligations != null && staffCreditCardMaturingObligations.Count() > 0)
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
            var alertTitleInfo = context.TBL_ALERT_TITLE.Where(a => a.BINDINGMETHOD == "GetUnpaidObligationReminder" && a.ISACTIVE == true).FirstOrDefault();

            var defaultEmail = "";
            if (alertTitleInfo != null && alertTitleInfo.DEFAULTEMAIL != null)
            {
                defaultEmail = ";" + alertTitleInfo.DEFAULTEMAIL;
            }
            if (alertTitleInfo != null && unpaidObligationReminder != null && unpaidObligationReminder.Count() > 0)
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
            var alertTitleInfo = context.TBL_ALERT_TITLE.Where(a => a.BINDINGMETHOD == "GetExpiringFacilityReport" && a.ISACTIVE == true).FirstOrDefault();

            var defaultEmail = "";
            if (alertTitleInfo != null && alertTitleInfo.DEFAULTEMAIL != null)
            {
                defaultEmail = ";" + alertTitleInfo.DEFAULTEMAIL;
            }
            if (alertTitleInfo != null && expiringFacilityReport != null && expiringFacilityReport.Count() > 0)
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
            var alertTitleInfo = context.TBL_ALERT_TITLE.Where(a => a.BINDINGMETHOD == "GetLoanExpirationReminder" && a.ISACTIVE == true).FirstOrDefault();

            var defaultEmail = "";
            if (alertTitleInfo != null && alertTitleInfo.DEFAULTEMAIL != null)
            {
                defaultEmail = ";" + alertTitleInfo.DEFAULTEMAIL;
            }

            if (alertTitleInfo != null && loanExpirationReminder != null && loanExpirationReminder.Count() > 0)
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
            var alertTitleInfo = context.TBL_ALERT_TITLE.Where(a => a.BINDINGMETHOD == "GetLoanExpirationReminderAccountOfficer" && a.ISACTIVE == true).FirstOrDefault();

            var defaultEmail = "";
            if (alertTitleInfo != null && alertTitleInfo.DEFAULTEMAIL != null)
            {
                defaultEmail = ";" + alertTitleInfo.DEFAULTEMAIL;
            }

            if (alertTitleInfo != null && loanExpirationReminderAccountOfficer != null && loanExpirationReminderAccountOfficer.Count() > 0)
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
            var alertTitleInfo = context.TBL_ALERT_TITLE.Where(a => a.BINDINGMETHOD == "GetUnAuthorizedOverdraftReport" && a.ISACTIVE == true).FirstOrDefault();

            var defaultEmail = "";
            if (alertTitleInfo != null && alertTitleInfo.DEFAULTEMAIL != null)
            {
                defaultEmail = ";" + alertTitleInfo.DEFAULTEMAIL;
            }

            if (alertTitleInfo != null && unAuthorizedOverdraftReport != null && unAuthorizedOverdraftReport.Count() > 0)
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
            var alertTitleInfo = context.TBL_ALERT_TITLE.Where(a => a.BINDINGMETHOD == "GetOverlineMonitoringReport" && a.ISACTIVE == true).FirstOrDefault();

            var defaultEmail = "";
            if (alertTitleInfo != null && alertTitleInfo.DEFAULTEMAIL != null)
            {
                defaultEmail = ";" + alertTitleInfo.DEFAULTEMAIL;
            }
            if (alertTitleInfo != null && overlineMonitoringReport != null && overlineMonitoringReport.Count() > 0)
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
            var exceptionNPLs = (from a in context2.TBL_TRIGGER_MEASURES_MODULE where a.MODULE.ToLower() != "pdlp"  select a).ToList();
            List<AlertsViewModel> alerts = new List<AlertsViewModel>();

            string emailList = "";

            if (exceptionNPLs != null && exceptionNPLs.Count() > 0)
            {
                foreach (var exceptionNPL in exceptionNPLs)
                {
                    TBL_ALERT_TITLE alertTemplate = new TBL_ALERT_TITLE();
                    AlertsViewModel alert = new AlertsViewModel();
                    var template = "";

                    if (exceptionNPL.NPL >= (decimal)tenPercentValue && exceptionNPL.NPL < (decimal)twentyPercentValue)
                    {
                        alertTemplate = context.TBL_ALERT_TITLE.Where(a => a.BINDINGMETHOD == "GetDigitalLoanExceptionNPLModuleIncreaseByOnePercent").FirstOrDefault();
                        template = alertTemplate.TEMPLATE;
                        template = template.Replace("@{{percentage}}", tenPercent);
                        template = template.Replace("@{{moduleName}}", exceptionNPL.MODULE);
                        emailList = alertTemplate.DEFAULTEMAIL;
                    }

                    if (exceptionNPL.NPL >= (decimal)twentyPercentValue && exceptionNPL.NPL < (decimal)thirtyPercentValue)
                    {
                        alertTemplate = context.TBL_ALERT_TITLE.Where(a => a.BINDINGMETHOD == "GetDigitalLoanExceptionNPLModuleIncreaseByOnePointFivePercent").FirstOrDefault();
                        template = alertTemplate.TEMPLATE;
                        template = template.Replace("@{{percentage}}", twentyPercent);
                        template = template.Replace("@{{moduleName}}", exceptionNPL.MODULE);
                        emailList = alertTemplate.DEFAULTEMAIL;
                    }
                    if (exceptionNPL.NPL >= (decimal)thirtyPercentValue)
                    {
                        alertTemplate = context.TBL_ALERT_TITLE.Where(a => a.BINDINGMETHOD == "GetDigitalLoanExceptionNPLModuleIncreaseByTwoPercent").FirstOrDefault();
                        template = alertTemplate.TEMPLATE;
                        template = template.Replace("@{{percentage}}", thirtyPercent);
                        template = template.Replace("@{{moduleName}}", exceptionNPL.MODULE);
                        emailList = alertTemplate.DEFAULTEMAIL;
                    }
                    if ((exceptionNPL.NPL >= (decimal)tenPercentValue && exceptionNPL.NPL < (decimal)twentyPercentValue)
                       || (exceptionNPL.NPL >= (decimal)twentyPercentValue && exceptionNPL.NPL < (decimal)thirtyPercentValue) || (exceptionNPL.NPL >= (decimal)thirtyPercentValue))
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
            var exceptionNPLs = (from a in context2.TBL_TRIGGER_MEASURES_MODULE where a.MODULE.ToLower() != "pdlp" select a).ToList();
            List<AlertsViewModel> alerts = new List<AlertsViewModel>();
            string emailList = "";

            if (exceptionNPLs != null && exceptionNPLs.Count() > 0)
            {
                foreach (var exceptionNPL in exceptionNPLs)
                {
                    TBL_ALERT_TITLE alertTemplate = new TBL_ALERT_TITLE();
                    AlertsViewModel alert = new AlertsViewModel();
                    var template = "";

                    if (exceptionNPL.NPL <= -(decimal)tenPercentValue && exceptionNPL.NPL > -(decimal)twentyPercentValue)
                    {
                        alertTemplate = context.TBL_ALERT_TITLE.Where(a => a.BINDINGMETHOD == "GetDigitalLoanExceptionNPLModuleIncreaseByOnePercent").FirstOrDefault();
                        template = alertTemplate.TEMPLATE;
                        template = template.Replace("@{{percentage}}", tenPercent);
                        template = template.Replace("@{{moduleName}}", exceptionNPL.MODULE);
                        emailList = alertTemplate.DEFAULTEMAIL;
                    }

                    if (exceptionNPL.NPL <= -(decimal)twentyPercentValue && exceptionNPL.NPL > -(decimal)thirtyPercentValue)
                    {
                        alertTemplate = context.TBL_ALERT_TITLE.Where(a => a.BINDINGMETHOD == "GetDigitalLoanExceptionNPLModuleIncreaseByOnePointFivePercent").FirstOrDefault();
                        template = alertTemplate.TEMPLATE;
                        template = template.Replace("@{{percentage}}", twentyPercent);
                        template = template.Replace("@{{moduleName}}", exceptionNPL.MODULE);
                        emailList = alertTemplate.DEFAULTEMAIL;
                    }
                    if (exceptionNPL.NPL <= -(decimal)thirtyPercentValue)
                    {
                        alertTemplate = context.TBL_ALERT_TITLE.Where(a => a.BINDINGMETHOD == "GetDigitalLoanExceptionNPLModuleIncreaseByTwoPercent").FirstOrDefault();
                        template = alertTemplate.TEMPLATE;
                        template = template.Replace("@{{percentage}}", thirtyPercent);
                        template = template.Replace("@{{moduleName}}", exceptionNPL.MODULE);
                        emailList = alertTemplate.DEFAULTEMAIL;
                    }

                    if ((exceptionNPL.NPL <= -(decimal)tenPercentValue && exceptionNPL.NPL > -(decimal)twentyPercentValue)
                       || (exceptionNPL.NPL <= -(decimal)twentyPercentValue && exceptionNPL.NPL > -(decimal)thirtyPercentValue) || (exceptionNPL.NPL <= -(decimal)thirtyPercentValue))
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
            var exceptionNPLs = (from a in context2.TBL_TRIGGER_MEASURES_MODULE where a.MODULE.ToLower() != "pdlp" select a).ToList();
            List<AlertsViewModel> alerts = new List<AlertsViewModel>();

            if (exceptionNPLs != null && exceptionNPLs.Count() > 0)
            {
                foreach (var exceptionNPL in exceptionNPLs)
                {
                    TBL_ALERT_TITLE alertTemplate = new TBL_ALERT_TITLE();
                    AlertsViewModel alert = new AlertsViewModel();
                    var template = "";
                    string emailList = "";

                    if (exceptionNPL.DISBURSEMENT >= (decimal)tenPercentValue && exceptionNPL.DISBURSEMENT < (decimal)twentyPercentValue)
                    {
                        alertTemplate = context.TBL_ALERT_TITLE.Where(a => a.BINDINGMETHOD == "GetDigitalLoanDisbursementModuleIncreaseByFivePercent").FirstOrDefault();
                        template = alertTemplate.TEMPLATE;
                        template = template.Replace("@{{percentage}}", tenPercent);
                        template = template.Replace("@{{moduleName}}", exceptionNPL.MODULE);
                        emailList = alertTemplate.DEFAULTEMAIL;
                    }

                    if (exceptionNPL.DISBURSEMENT >= (decimal)twentyPercentValue && exceptionNPL.DISBURSEMENT < (decimal)thirtyPercentValue)
                    {
                        alertTemplate = context.TBL_ALERT_TITLE.Where(a => a.BINDINGMETHOD == "GetDigitalLoanDisbursementModuleIncreaseByTenPercent").FirstOrDefault();
                        template = alertTemplate.TEMPLATE;
                        template = template.Replace("@{{percentage}}", twentyPercent);
                        template = template.Replace("@{{moduleName}}", exceptionNPL.MODULE);
                        emailList = alertTemplate.DEFAULTEMAIL;
                    }
                    if (exceptionNPL.DISBURSEMENT >= (decimal)thirtyPercentValue)
                    {
                        alertTemplate = context.TBL_ALERT_TITLE.Where(a => a.BINDINGMETHOD == "GetDigitalLoanDisbursementModuleIncreaseByFifteenPercent").FirstOrDefault();
                        template = alertTemplate.TEMPLATE;
                        template = template.Replace("@{{percentage}}", thirtyPercent);
                        template = template.Replace("@{{moduleName}}", exceptionNPL.MODULE);
                        emailList = alertTemplate.DEFAULTEMAIL;
                    }

                    if ((exceptionNPL.DISBURSEMENT >= (decimal)tenPercentValue && exceptionNPL.DISBURSEMENT < (decimal)twentyPercentValue)
                       || (exceptionNPL.DISBURSEMENT >= (decimal)twentyPercentValue && exceptionNPL.DISBURSEMENT < (decimal)thirtyPercentValue)
                       || (exceptionNPL.DISBURSEMENT >= (decimal)thirtyPercentValue))
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
            var exceptionNPLs = (from a in context2.TBL_TRIGGER_MEASURES_MODULE where a.MODULE.ToLower() != "pdlp" select a).ToList();
            List<AlertsViewModel> alerts = new List<AlertsViewModel>();

            if (exceptionNPLs != null && exceptionNPLs.Count() > 0)
            {
                foreach (var exceptionNPL in exceptionNPLs)
                {
                    TBL_ALERT_TITLE alertTemplate = new TBL_ALERT_TITLE();
                    AlertsViewModel alert = new AlertsViewModel();
                    var template = "";
                    string emailList = "";

                    if (exceptionNPL.NPL <= -(decimal)tenPercentValue && exceptionNPL.NPL > -(decimal)twentyPercentValue)
                    {
                        alertTemplate = context.TBL_ALERT_TITLE.Where(a => a.BINDINGMETHOD == "GetDigitalLoanDisbursementModuleDecreaseByFivePercent").FirstOrDefault();
                        template = alertTemplate.TEMPLATE;
                        template = template.Replace("@{{percentage}}", tenPercent);
                        template = template.Replace("@{{moduleName}}", exceptionNPL.MODULE);
                        emailList = alertTemplate.DEFAULTEMAIL;
                    }

                    if (exceptionNPL.NPL <= -(decimal)twentyPercentValue && exceptionNPL.NPL > -(decimal)thirtyPercentValue)
                    {
                        alertTemplate = context.TBL_ALERT_TITLE.Where(a => a.BINDINGMETHOD == "GetDigitalLoanDisbursementModuleDecreaseByTenPercent").FirstOrDefault();
                        template = alertTemplate.TEMPLATE;
                        template = template.Replace("@{{percentage}}", twentyPercent);
                        template = template.Replace("@{{moduleName}}", exceptionNPL.MODULE);
                        emailList = alertTemplate.DEFAULTEMAIL;
                    }
                    if (exceptionNPL.NPL <= -(decimal)thirtyPercentValue)
                    {
                        alertTemplate = context.TBL_ALERT_TITLE.Where(a => a.BINDINGMETHOD == "GetDigitalLoanDisbursementModuleDecreaseByFifteenPercent").FirstOrDefault();
                        template = alertTemplate.TEMPLATE;
                        template = template.Replace("@{{percentage}}", thirtyPercent);
                        template = template.Replace("@{{moduleName}}", exceptionNPL.MODULE);
                        emailList = alertTemplate.DEFAULTEMAIL;
                    }

                    if ((exceptionNPL.NPL <= -(decimal)tenPercentValue && exceptionNPL.NPL > -(decimal)twentyPercentValue)
                       || (exceptionNPL.NPL <= -(decimal)twentyPercentValue && exceptionNPL.NPL > -(decimal)thirtyPercentValue)
                       || (exceptionNPL.NPL <= -(decimal)thirtyPercentValue))
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
            var exceptionNPLs = (from a in context2.TBL_TRIGGER_MEASURES_MODULE where a.MODULE.ToLower() != "pdlp" select a).ToList();
            List<AlertsViewModel> alerts = new List<AlertsViewModel>();

            if (exceptionNPLs != null && exceptionNPLs.Count() > 0)
            {

                var totalDpd = exceptionNPLs.Sum(x => x.DPD);
                //foreach (var exceptionNPL in exceptionNPLs)
                //{
                    TBL_ALERT_TITLE alertTemplate = new TBL_ALERT_TITLE();
                    AlertsViewModel alert = new AlertsViewModel();
                    var template = "";
                    string emailList = "";

                    if (totalDpd >= (decimal)tenPercentValue && totalDpd < (decimal)twentyPercentValue)
                    {
                        alertTemplate = context.TBL_ALERT_TITLE.Where(a => a.BINDINGMETHOD == "GetDigitalLoanDPDModuleIncreaseByTenPercent").FirstOrDefault();
                        template = alertTemplate.TEMPLATE;
                        template = template.Replace("@{{percentage}}", tenPercent);
                        emailList = alertTemplate.DEFAULTEMAIL;
                    }

                    if (totalDpd >= (decimal)twentyPercentValue && totalDpd < (decimal)thirtyPercentValue)
                    {
                        alertTemplate = context.TBL_ALERT_TITLE.Where(a => a.BINDINGMETHOD == "GetDigitalLoanDPDModuleIncreaseByTwentyFivePercent").FirstOrDefault();
                        template = alertTemplate.TEMPLATE;
                        template = template.Replace("@{{percentage}}", twentyPercent);
                        //template = template.Replace("@{{moduleName}}", exceptionNPL.MODULE);
                        emailList = alertTemplate.DEFAULTEMAIL;
                    }
                    if (totalDpd >= (decimal)thirtyPercentValue)
                    {
                        alertTemplate = context.TBL_ALERT_TITLE.Where(a => a.BINDINGMETHOD == "GetDigitalLoanDPDModuleIncreaseByFiftyPercent").FirstOrDefault();
                        template = alertTemplate.TEMPLATE;
                        template = template.Replace("@{{percentage}}", thirtyPercent);
                        emailList = alertTemplate.DEFAULTEMAIL;
                    }

                    if ((totalDpd >= (decimal)tenPercentValue && totalDpd < (decimal)twentyPercentValue)
                       || (totalDpd >= (decimal)twentyPercentValue && totalDpd < (decimal)thirtyPercentValue)
                       || (totalDpd >= (decimal)thirtyPercentValue))
                    {
                        var email = "Chibuike.Mbanefo@ACCESSBANKPLC.com;PAUL.ASIEMO@accessbankplc.com;OLUKAYODE.AJAYI@ACCESSBANKPLC.com";
                        alert.receiverEmailList.Add(email);
                        alert.template = template;
                        alert.alertTitle = alertTemplate.TITLE;
                        alert.canFire = true;
                        alert.operationMethod = alertTemplate.BINDINGMETHOD;
                        alerts.Add(alert);
                    }
                //}
                if (alerts.Count() > 0)
                {
                    SendAlertNotification(alerts);
                }
            }
        }
        public void GetDigitalLoanDPDModuleDecrease()
        {
            var exceptionNPLs = (from a in context2.TBL_TRIGGER_MEASURES_MODULE where a.MODULE.ToLower() != "pdlp" select a).ToList();
            List<AlertsViewModel> alerts = new List<AlertsViewModel>();

            if (exceptionNPLs != null && exceptionNPLs.Count() > 0)
            {
                var totalDpd = exceptionNPLs.Sum(x => x.DPD);
                //foreach (var exceptionNPL in exceptionNPLs)
                //{
                    TBL_ALERT_TITLE alertTemplate = new TBL_ALERT_TITLE();
                    AlertsViewModel alert = new AlertsViewModel();
                    var template = "";
                    string emailList = "";

                    if (totalDpd >= -(decimal)tenPercentValue && totalDpd > -(decimal)twentyPercentValue)
                    {
                        alertTemplate = context.TBL_ALERT_TITLE.Where(a => a.BINDINGMETHOD == "GetDigitalLoanDPDModuleDecreaseByTenPercent").FirstOrDefault();
                        template = alertTemplate.TEMPLATE;
                        template = template.Replace("@{{percentage}}", tenPercent);
                        emailList = alertTemplate.DEFAULTEMAIL;
                    }

                    if (totalDpd <= -(decimal)twentyPercentValue && totalDpd > -(decimal)thirtyPercentValue)
                    {
                        alertTemplate = context.TBL_ALERT_TITLE.Where(a => a.BINDINGMETHOD == "GetDigitalLoanDPDModuleDecreaseByTwentyFivePercent").FirstOrDefault();
                        template = alertTemplate.TEMPLATE;
                        template = template.Replace("@{{percentage}}", twentyPercent);
                        emailList = alertTemplate.DEFAULTEMAIL;
                    }
                    if (totalDpd <= -(decimal)thirtyPercentValue)
                    {
                        alertTemplate = context.TBL_ALERT_TITLE.Where(a => a.BINDINGMETHOD == "GetDigitalLoanDPDModuleDecreaseByFiftyPercent").FirstOrDefault();
                        template = alertTemplate.TEMPLATE;
                        template = template.Replace("@{{percentage}}", fiftyPercent);
                        emailList = alertTemplate.DEFAULTEMAIL;
                    }

                    if ((totalDpd <= -(decimal)tenPercentValue && totalDpd > -(decimal)twentyPercentValue)
                       || (totalDpd <= -(decimal)twentyPercentValue && totalDpd > -(decimal)thirtyPercentValue)
                       || (totalDpd <= -(decimal)thirtyPercentValue))
                    {
                        var email = "Chibuike.Mbanefo@ACCESSBANKPLC.com;PAUL.ASIEMO@accessbankplc.com;OLUKAYODE.AJAYI@ACCESSBANKPLC.com";
                        alert.receiverEmailList.Add(email);
                        alert.template = template;
                        alert.alertTitle = alertTemplate.TITLE;
                        alert.canFire = true;
                        alert.operationMethod = alertTemplate.BINDINGMETHOD;
                        alerts.Add(alert);
                    }
                //}
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
                        alertTemplate = context.TBL_ALERT_TITLE.Where(a => a.BINDINGMETHOD == "GetDigitalLoanLiquidationModuleIncrease").FirstOrDefault();
                        template = alertTemplate.TEMPLATE;
                        template = template.Replace("@{{percentage}}", tenPercent);
                        template = template.Replace("@{{productName}}", exceptionNPL.PRODUCTNAME);
                        emailList = alertTemplate.DEFAULTEMAIL;
                    }

                    if (exceptionNPL.LIQUIDATION >= (decimal)tenPercentValue && exceptionNPL.LIQUIDATION < (decimal)fifteenPercentValue)
                    {
                        alertTemplate = context.TBL_ALERT_TITLE.Where(a => a.BINDINGMETHOD == "GetDigitalLoanLiquidationModuleIncrease").FirstOrDefault();
                        template = alertTemplate.TEMPLATE;
                        template = template.Replace("@{{percentage}}", twentyFivePercent);
                        template = template.Replace("@{{productName}}", exceptionNPL.PRODUCTNAME);
                        emailList = alertTemplate.DEFAULTEMAIL;
                    }
                    if (exceptionNPL.LIQUIDATION >= (decimal)fifteenPercentValue)
                    {
                        alertTemplate = context.TBL_ALERT_TITLE.Where(a => a.BINDINGMETHOD == "GetDigitalLoanLiquidationModuleIncrease").FirstOrDefault();
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
            var exceptionNPLs = (from a in context2.TBL_TRIGGER_MEASURES_MODULE where a.MODULE.ToLower() != "pdlp" select a).ToList();
            List<AlertsViewModel> alerts = new List<AlertsViewModel>();

            if (exceptionNPLs != null && exceptionNPLs.Count() > 0)
            {
                foreach (var exceptionNPL in exceptionNPLs)
                {
                    TBL_ALERT_TITLE alertTemplate = new TBL_ALERT_TITLE();
                    AlertsViewModel alert = new AlertsViewModel();
                    var template = "";
                    string emailList = "";

                    if (exceptionNPL.LIQUIDATION >= (decimal)fortyPercentValue && exceptionNPL.LIQUIDATION < (decimal)fiftyPercentValue)
                    {
                        alertTemplate = context.TBL_ALERT_TITLE.Where(a => a.BINDINGMETHOD == "GetDigitalLoanLiquidationIncreaseByFivePercent").FirstOrDefault();
                        template = alertTemplate.TEMPLATE;
                        template = template.Replace("@{{percentage}}", fortyPercent);
                        template = template.Replace("@{{moduleName}}", exceptionNPL.MODULE);
                        emailList = alertTemplate.DEFAULTEMAIL;
                    }

                    if (exceptionNPL.LIQUIDATION >= (decimal)fiftyPercentValue && exceptionNPL.LIQUIDATION < (decimal)sixtyPercentValue)
                    {
                        alertTemplate = context.TBL_ALERT_TITLE.Where(a => a.BINDINGMETHOD == "GetDigitalLoanLiquidationIncreaseByTenPercent").FirstOrDefault();
                        template = alertTemplate.TEMPLATE;
                        template = template.Replace("@{{percentage}}", fiftyPercent);
                        template = template.Replace("@{{moduleName}}", exceptionNPL.MODULE);
                        emailList = alertTemplate.DEFAULTEMAIL;
                    }
                    if (exceptionNPL.LIQUIDATION >= (decimal)sixtyPercentValue)
                    {
                        alertTemplate = context.TBL_ALERT_TITLE.Where(a => a.BINDINGMETHOD == "GetDigitalLoanLiquidationIncreaseByFifteenPercent").FirstOrDefault();
                        template = alertTemplate.TEMPLATE;
                        template = template.Replace("@{{percentage}}", sixtyPercent);
                        template = template.Replace("@{{moduleName}}", exceptionNPL.MODULE);
                        emailList = alertTemplate.DEFAULTEMAIL;
                    }

                    if ((exceptionNPL.LIQUIDATION >= (decimal)fortyPercentValue && exceptionNPL.LIQUIDATION < (decimal)fiftyPercentValue)
                       || (exceptionNPL.LIQUIDATION >= (decimal)fiftyPercentValue && exceptionNPL.LIQUIDATION < (decimal)sixtyPercentValue)
                       || (exceptionNPL.LIQUIDATION >= (decimal)sixtyPercentValue))
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

        public void GetDigitalLoanLiquidationModuleDecrease()
        {
            var exceptionNPLs = (from a in context2.TBL_TRIGGER_MEASURES_MODULE where a.MODULE.ToLower() != "pdlp" select a).ToList();
            List<AlertsViewModel> alerts = new List<AlertsViewModel>();

            if (exceptionNPLs != null && exceptionNPLs.Count() > 0)
            {
                foreach (var exceptionNPL in exceptionNPLs)
                {
                    TBL_ALERT_TITLE alertTemplate = new TBL_ALERT_TITLE();
                    AlertsViewModel alert = new AlertsViewModel();
                    var template = "";
                    string emailList = "";

                    if (exceptionNPL.LIQUIDATION >= -(decimal)fortyPercentValue && exceptionNPL.LIQUIDATION > -(decimal)fiftyPercentValue)
                    {
                        alertTemplate = context.TBL_ALERT_TITLE.Where(a => a.BINDINGMETHOD == "GetDigitalLoanLiquidationDecrease").FirstOrDefault();
                        template = alertTemplate.TEMPLATE;
                        template = template.Replace("@{{percentage}}", fortyPercent);
                        template = template.Replace("@{{moduleName}}", exceptionNPL.MODULE);
                        emailList = alertTemplate.DEFAULTEMAIL;
                    }

                    if (exceptionNPL.LIQUIDATION <= -(decimal)fiftyPercentValue && exceptionNPL.LIQUIDATION > -(decimal)sixtyPercentValue)
                    {
                        alertTemplate = context.TBL_ALERT_TITLE.Where(a => a.BINDINGMETHOD == "GetDigitalLoanLiquidationDecrease").FirstOrDefault();
                        template = alertTemplate.TEMPLATE;
                        template = template.Replace("@{{percentage}}", fiftyPercent);
                        template = template.Replace("@{{moduleName}}", exceptionNPL.MODULE);
                        emailList = alertTemplate.DEFAULTEMAIL;
                    }
                    if (exceptionNPL.LIQUIDATION <= -(decimal)sixtyPercentValue)
                    {
                        alertTemplate = context.TBL_ALERT_TITLE.Where(a => a.BINDINGMETHOD == "GetDigitalLoanLiquidationDecrease").FirstOrDefault();
                        template = alertTemplate.TEMPLATE;
                        template = template.Replace("@{{percentage}}", sixtyPercent);
                        template = template.Replace("@{{moduleName}}", exceptionNPL.MODULE);
                        emailList = alertTemplate.DEFAULTEMAIL;
                    }

                    if ((exceptionNPL.LIQUIDATION <= -(decimal)fortyPercentValue && exceptionNPL.LIQUIDATION > -(decimal)fiftyPercentValue)
                       || (exceptionNPL.LIQUIDATION <= -(decimal)fiftyPercentValue && exceptionNPL.LIQUIDATION > -(decimal)sixtyPercentValue)
                       || (exceptionNPL.LIQUIDATION <= (decimal)sixtyPercentValue))
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
            var OverdueObligation = context.TBL_ALERT_TITLE.Where(x => x.BINDINGMETHOD == "OverdueObligation" && x.ISACTIVE == true).FirstOrDefault();

            if (OverdueObligation != null && repaymentDefaulters != null && repaymentDefaulters.Count() > 0)
            {

                List<AlertsViewModel> alerts = new List<AlertsViewModel>();
                foreach (var repaymentDefaulter in repaymentDefaulters)
                {
                    AlertsViewModel alert = new AlertsViewModel();
                    var amount = string.Format("{0:#,##.00}", Convert.ToDecimal(repaymentDefaulter.periodPaymentAmount));
                    var alertTitle = OverdueObligation.TITLE;
                    var alertTemplate = OverdueObligation.TEMPLATE;
                    alertTemplate = alertTemplate.Replace("@{{loanAmount}}", amount);
                    alertTemplate = alertTemplate.Replace("@{{customerName}}", repaymentDefaulter.customerName);
                    alertTemplate = alertTemplate.Replace("@{{loanAmount}}", amount);
                    alertTemplate = alertTemplate.Replace("@{{loanAmount}}", amount);
                    alertTemplate = alertTemplate.Replace("@{{loanAmount}}", amount);

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
            var alertTitleInfo = context.TBL_ALERT_TITLE.Where(a => a.BINDINGMETHOD == "GetRecoveryAssignmentDueCompletionDate" && a.ISACTIVE == true).FirstOrDefault();

            var defaultEmail = "";
            if (alertTitleInfo != null && alertTitleInfo.DEFAULTEMAIL != null)
            {
                defaultEmail = ";" + alertTitleInfo.DEFAULTEMAIL;
            }
            if (alertTitleInfo != null && aboutToExpiredList != null && aboutToExpiredList.Count() > 0)
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
                            var customerName = context.TBL_CUSTOMER.Where(s => s.CUSTOMERCODE == t.CUSTOMERID).Select(s => s.FIRSTNAME + " " + s.MIDDLENAME + " " + s.LASTNAME).FirstOrDefault();
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


        public bool corporateCustomerUpdate()
        {
            var nullifyCorpporateProfileCompletion = context.TBL_CUSTOMER.Where(x => x.DELETED == false && x.CUSTOMERTYPEID == (short)CustomerTypeEnum.Corporate).ToList();
            if (nullifyCorpporateProfileCompletion.Any())
            {
                foreach (var item in nullifyCorpporateProfileCompletion)
                {
                    item.ACCOUNTCREATIONCOMPLETE = false;
                    item.DATETIMEUPDATED = DateTime.Now;
                }
            }
            return context.SaveChanges() > 0;
        }

        public string GetAllAlertGroupEmailToSend(int alertTitleId)
        {
            var list = "";
            var alerts = (from a in context.TBL_ALERT_STAFF_ROLE
                          join b in context.TBL_ALERT_GROUP_EMAIL on a.STAFFROLEID equals b.GROUPEMAILID
                          join c in context.TBL_ALERT_TITLE on a.ALERTTITLEID equals c.ALERTTITLEID
                          where c.ALERTTITLEID == alertTitleId
                          select new AlertLevelViewModel
                          {
                              groupEmail = b.GROUPEMAIL,
                          }).ToList();
            foreach (var t in alerts)
            {
                list = list + ";" + t.groupEmail;
            }
            return list;
        }

        public string GetAllAlertGroupEmailToSendToRole(int alertTitleId)
        {
            var list = "";
            var alerts = (from a in context.TBL_ALERT_STAFF_ROLE
                          join b in context.TBL_ALERT_GROUP_EMAIL on a.STAFFROLEID equals b.GROUPEMAILID
                          join c in context.TBL_ALERT_TITLE on a.ALERTTITLEID equals c.ALERTTITLEID
                          join s in context.TBL_STAFF on a.STAFFROLEID equals s.STAFFROLEID
                          where c.ALERTTITLEID == alertTitleId
                          select new AlertLevelViewModel
                          {
                              groupEmail = s.EMAIL,
                          }).ToList();
            foreach (var t in alerts)
            {
                list = list + ";" + t.groupEmail;
            }
            return list;
        }


        public bool AddAlertPlaceholder(AlertPlaceHoldersViewModel model)
        {
            var entity = new TBL_ALERT_PLACEHOLDER
            {
                ALERTTITLEID = model.alertTitleId,
                PLACEHOLDER = model.placeHolder,
                DESCRIPTION = model.description,
                PARTITION = model.partition,
            };

            context.TBL_ALERT_PLACEHOLDER.Add(entity);

            var auditStaff = (context.TBL_STAFF.Where(x => x.STAFFID == model.createdBy).Select(x => x.STAFFCODE));
            // Audit Section ---------------------------
            this.audit.AddAuditTrail(new TBL_AUDIT
            {
                AUDITTYPEID = (short)AuditTypeEnum.AlertLevelAdded,
                STAFFID = model.createdBy,
                BRANCHID = (short)model.userBranchId,
                DETAIL = $"TBL_ALERT_PLACEHOLDER '{entity.ToString()}' created by {auditStaff}",
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

        public bool AddAlertPlaceholderUpdate(int id, AlertPlaceHoldersViewModel model)
        {
            var entity = this.context.TBL_ALERT_PLACEHOLDER.Find(id);
            entity.ALERTTITLEID = model.alertTitleId;
            entity.PLACEHOLDER = model.placeHolder;
            entity.DESCRIPTION = model.description;
            entity.PARTITION = model.partition;

            var auditStaff = (context.TBL_STAFF.Where(x => x.STAFFID == model.createdBy).Select(x => x.STAFFCODE));
            // Audit Section ---------------------------
            this.audit.AddAuditTrail(new TBL_AUDIT
            {
                AUDITTYPEID = (short)AuditTypeEnum.AlertConditionUpdated,
                STAFFID = model.createdBy,
                BRANCHID = (short)model.userBranchId,
                DETAIL = $"TBL_ALERT_PLACEHOLDER'{entity.ToString()}' was updated by {auditStaff}",
                IPADDRESS = CommonHelpers.GetLocalIpAddress(),
                URL = model.applicationUrl,
                APPLICATIONDATE = general.GetApplicationDate(),
                SYSTEMDATETIME = DateTime.Now,
                TARGETID = entity.PLACEHOLDERID,
                DEVICENAME = CommonHelpers.GetDeviceName(),
                OSNAME = CommonHelpers.FriendlyName(),
            });
            // Audit Section end ------------------------

            return context.SaveChanges() != 0;
        }

        public bool AddAlertBindingMethod(AlertBindingMethodsViewModel model)
        {
            var validateAlertMethod = context.TBL_ALERT_BINDING_METHODS.Where(x => model.methodName.Contains(x.METHODTNAME));
            if (validateAlertMethod.Any())
            {
                throw new SecureException($"method name '{model.methodName}'' already exist");
            }
            var entity = new TBL_ALERT_BINDING_METHODS
            {
                METHODTITLE = model.methodTitle,
                METHODTNAME = model.methodName,
                CREATEDBY = model.createdBy,
                DATETIMECREATED = DateTime.Now,
            };

            context.TBL_ALERT_BINDING_METHODS.Add(entity);

            var auditStaff = (context.TBL_STAFF.Where(x => x.STAFFID == model.createdBy).Select(x => x.STAFFCODE));
            // Audit Section ---------------------------
            this.audit.AddAuditTrail(new TBL_AUDIT
            {
                AUDITTYPEID = (short)AuditTypeEnum.AlertLevelAdded,
                STAFFID = model.createdBy,
                BRANCHID = (short)model.userBranchId,
                DETAIL = $"TBL_ALERT_BINDING_METHODS '{entity.ToString()}' created by {auditStaff}",
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

        public bool AddAlertBindingMethosUpdate(int id, AlertBindingMethodsViewModel model)
        {
            var entity = this.context.TBL_ALERT_BINDING_METHODS.Find(id);
            entity.METHODTITLE = model.methodTitle;
            entity.METHODTNAME = model.methodName;

            var auditStaff = (context.TBL_STAFF.Where(x => x.STAFFID == model.createdBy).Select(x => x.STAFFCODE));
            // Audit Section ---------------------------
            this.audit.AddAuditTrail(new TBL_AUDIT
            {
                AUDITTYPEID = (short)AuditTypeEnum.AlertConditionUpdated,
                STAFFID = model.createdBy,
                BRANCHID = (short)model.userBranchId,
                DETAIL = $"TBL_ALERT_BINDING_METHODS'{entity.ToString()}' was updated by {auditStaff}",
                IPADDRESS = CommonHelpers.GetLocalIpAddress(),
                URL = model.applicationUrl,
                APPLICATIONDATE = general.GetApplicationDate(),
                SYSTEMDATETIME = DateTime.Now,
                TARGETID = entity.BINDINGMEHTODID,
                DEVICENAME = CommonHelpers.GetDeviceName(),
                OSNAME = CommonHelpers.FriendlyName(),
            });
            // Audit Section end ------------------------

            return context.SaveChanges() != 0;
        }

    }
}
