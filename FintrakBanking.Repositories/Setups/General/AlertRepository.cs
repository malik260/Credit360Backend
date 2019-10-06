using FintrakBanking.Common;
using FintrakBanking.Common.CustomException;
using FintrakBanking.Common.Enum;
using FintrakBanking.Entities.Models;
using FintrakBanking.Interfaces.Admin;
using FintrakBanking.Interfaces.Setups.General;
using FintrakBanking.ViewModels;
using FintrakBanking.ViewModels.Setups.General;
using System;
using System.Collections.Generic;
using System.Configuration;
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

        public IEnumerable<AlertTitleViewModel> GetAllAlerts()
        {
            var alerts = (from a in context.TBL_ALERT_TITLE
                                      select new AlertTitleViewModel
                                      {
                                          alertTitleId = a.ALERTTITLEID,
                                          title = a.TITLE,
                                          template = a.TEMPLATE
                                      });
            return alerts;
        }

        public AlertTitleViewModel GetAlertById(int id)
        {
            var alert = (from a in context.TBL_ALERT_TITLE.Where(x=>x.ALERTTITLEID == id)
                          select new AlertTitleViewModel
                          {
                              alertTitleId = a.ALERTTITLEID,
                              title = a.TITLE,
                              template = a.TEMPLATE
                          }).FirstOrDefault();
            return alert;
        }

        public bool AddAlertTitle(AlertTitleViewModel model)
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

        public bool UpdateAlertTitle(int id, AlertTitleViewModel model, UserInfo user)
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
                DETAIL = $"TBL_ALERT_TITLE'{entity.TITLE}' was updated by {auditStaff}",
                IPADDRESS = user.userIPAddress,
                URL = user.applicationUrl,
                APPLICATIONDATE = general.GetApplicationDate(),
                SYSTEMDATETIME = DateTime.Now,
                TARGETID = entity.ALERTTITLEID
            });
            // Audit Section end ------------------------

            return context.SaveChanges() != 0;
        }

       public bool DeleteAlertTitle(int id, UserInfo user)
        {
            var entity = this.context.TBL_ALERT_TITLE.Find(id);
            
            var auditStaff = (context.TBL_STAFF.Where(x => x.STAFFID == user.createdBy).Select(x => x.STAFFCODE));
            // Audit Section ---------------------------
            this.audit.AddAuditTrail(new TBL_AUDIT
            {
                AUDITTYPEID = (short)AuditTypeEnum.AlertTitleDeleted,
                STAFFID = user.createdBy,
                BRANCHID = (short)user.BranchId,
                DETAIL = $"TBL_ALERT_TITLE '{entity.ToString()}' was deleted by {auditStaff}",
                IPADDRESS = user.userIPAddress,
                URL = user.applicationUrl,
                APPLICATIONDATE = general.GetApplicationDate(),
                SYSTEMDATETIME = DateTime.Now,
                TARGETID = entity.ALERTTITLEID
            });
            // Audit Section end ------------------------

            return context.SaveChanges() != 0;
        }

        
        public IEnumerable<AlertSetupViewModel> GetAllAlertSetup()
        {
            var alerts = (from a in context.TBL_ALERT_SETUP
                          select new AlertSetupViewModel
                          {
                              alertSetupId = a.ALERTSETUPID,
                              titleId = a.TITLEID,
                              levelGroupMappingId = a.LEVELGROUPID,
                              frequencyId = a.FREQUENCYID
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
                             levelGroupMappingId = a.LEVELGROUPID,
                             frequencyId = a.FREQUENCYID
                         }).FirstOrDefault();
            return alert;
        }

        public bool AddAlertSetup(AlertSetupViewModel model)
        {
            var entity = new TBL_ALERT_SETUP
            {
               // LEVELGROUPMAPPINGID = model.levelGroupMappingId,
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
                DETAIL = $"TBL_ALERT_TITLE '{entity.ToString()}' created by {auditStaff}",
                IPADDRESS = model.userIPAddress,
                URL = model.applicationUrl,
                APPLICATIONDATE = general.GetApplicationDate(),
                SYSTEMDATETIME = DateTime.Now
            });
            // Audit Section end ------------------------

            return context.SaveChanges() != 0;
        }

        public bool UpdateAlertSetup(int id, AlertSetupViewModel model, UserInfo user)
        {
            var entity = this.context.TBL_ALERT_SETUP.Find(id);
            entity.TITLEID = model.titleId;
            entity.FREQUENCYID = model.frequencyId;
            //entity.LEVELGROUPMAPPINGID = model.levelGroupMappingId;
            entity.CONDITIONID = model.conditionId;

            var auditStaff = (context.TBL_STAFF.Where(x => x.STAFFID == user.createdBy).Select(x => x.STAFFCODE));
            // Audit Section ---------------------------
            this.audit.AddAuditTrail(new TBL_AUDIT
            {
                AUDITTYPEID = (short)AuditTypeEnum.AlertSetupUpdated,
                STAFFID = user.createdBy,
                BRANCHID = (short)user.BranchId,
                DETAIL = $"TBL_ALERT_TITLE'{entity}' was updated by {auditStaff}",
                IPADDRESS = user.userIPAddress,
                URL = user.applicationUrl,
                APPLICATIONDATE = general.GetApplicationDate(),
                SYSTEMDATETIME = DateTime.Now,
                TARGETID = entity.ALERTSETUPID
            });
            // Audit Section end ------------------------

            return context.SaveChanges() != 0;
        }

        public bool DeleteAlertSetup(int id, UserInfo user)
        {
            var entity = this.context.TBL_ALERT_SETUP.Find(id);

            var auditStaff = (context.TBL_STAFF.Where(x => x.STAFFID == user.createdBy).Select(x => x.STAFFCODE));
            // Audit Section ---------------------------
            this.audit.AddAuditTrail(new TBL_AUDIT
            {
                AUDITTYPEID = (short)AuditTypeEnum.AlertSetupDeleted,
                STAFFID = user.createdBy,
                BRANCHID = (short)user.BranchId,
                DETAIL = $"TBL_ALERT_TITLE '{entity.ToString()}' was deleted by {auditStaff}",
                IPADDRESS = user.userIPAddress,
                URL = user.applicationUrl,
                APPLICATIONDATE = general.GetApplicationDate(),
                SYSTEMDATETIME = DateTime.Now,
                TARGETID = entity.ALERTSETUPID
            });
            // Audit Section end ------------------------

            return context.SaveChanges() != 0;
        }
       
        
        public IEnumerable<LevelGroupMappingViewModel> GetAllAlertLevelGroupMapping()
        {
            var alerts = (from a in context.TBL_ALERT_LEVEL_GRP_MAPPING
                          select new LevelGroupMappingViewModel
                          {
                              alertLevelGroupMapId = a.ALERTLEVELGROUPMAPID,
                              levelGroupId = a.LEVELGROUPID,
                              levelCode = a.LEVELCODE
                          });
            return alerts;
        }

        public LevelGroupMappingViewModel GetAlertLevelGroupMappingById(int id)
        {
            var alert = (from a in context.TBL_ALERT_LEVEL_GRP_MAPPING.Where(x => x.ALERTLEVELGROUPMAPID == id)
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
            var entity = new TBL_ALERT_LEVEL_GRP_MAPPING
            {
                LEVELCODE = model.levelCode,
                LEVELGROUPID = model.levelGroupId
            };

            context.TBL_ALERT_LEVEL_GRP_MAPPING.Add(entity);

            var auditStaff = (context.TBL_STAFF.Where(x => x.STAFFID == model.createdBy).Select(x => x.STAFFCODE));
            // Audit Section ---------------------------
            this.audit.AddAuditTrail(new TBL_AUDIT
            {
                AUDITTYPEID = (short)AuditTypeEnum.AlertLevelGroupMappingAdded,
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

        public bool UpdateAlertLevelGroupMapping(int id, LevelGroupMappingViewModel model, UserInfo user)
        {
            var entity = this.context.TBL_ALERT_LEVEL_GRP_MAPPING.Find(id);
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
                IPADDRESS = user.userIPAddress,
                URL = user.applicationUrl,
                APPLICATIONDATE = general.GetApplicationDate(),
                SYSTEMDATETIME = DateTime.Now,
                TARGETID = entity.ALERTLEVELGROUPMAPID
            });
            // Audit Section end ------------------------

            return context.SaveChanges() != 0;
        }

        public bool DeleteAlertLevelGroupMapping(int id, UserInfo user)
        {
            var entity = this.context.TBL_ALERT_LEVEL_GRP_MAPPING.Find(id);

            var auditStaff = (context.TBL_STAFF.Where(x => x.STAFFID == user.createdBy).Select(x => x.STAFFCODE));
            // Audit Section ---------------------------
            this.audit.AddAuditTrail(new TBL_AUDIT
            {
                AUDITTYPEID = (short)AuditTypeEnum.AlertLevelGroupMappingDeleted,
                STAFFID = user.createdBy,
                BRANCHID = (short)user.BranchId,
                DETAIL = $"TBL_ALERT_TITLE '{entity.ToString()}' was deleted by {auditStaff}",
                IPADDRESS = user.userIPAddress,
                URL = user.applicationUrl,
                APPLICATIONDATE = general.GetApplicationDate(),
                SYSTEMDATETIME = DateTime.Now,
                TARGETID = entity.ALERTLEVELGROUPMAPID
            });
            // Audit Section end ------------------------

            return context.SaveChanges() != 0;
        }

        
        public IEnumerable<AlertLevelGroupViewModel> GetAllAlertLevelGroup()
        {
            var alerts = (from a in context.TBL_ALERT_LEVEL_GROUP
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
            var alert = (from a in context.TBL_ALERT_LEVEL_GROUP.Where(x => x.ALERTLEVELGROUPID == id)
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
            var entity = new TBL_ALERT_LEVEL_GROUP
            {
                LEVELGROUPNAME = model.levelGroupName,
                DESCRIPTION = model.description
            };

            context.TBL_ALERT_LEVEL_GROUP.Add(entity);

            var auditStaff = (context.TBL_STAFF.Where(x => x.STAFFID == model.createdBy).Select(x => x.STAFFCODE));
            // Audit Section ---------------------------
            this.audit.AddAuditTrail(new TBL_AUDIT
            {
                AUDITTYPEID = (short)AuditTypeEnum.AlertLevelGroupAdded,
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

        public bool UpdateAlertLevelGroup(int id, AlertLevelGroupViewModel model, UserInfo user)
        {
            var entity = this.context.TBL_ALERT_LEVEL_GROUP.Find(id);
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
                IPADDRESS = user.userIPAddress,
                URL = user.applicationUrl,
                APPLICATIONDATE = general.GetApplicationDate(),
                SYSTEMDATETIME = DateTime.Now,
                TARGETID = entity.ALERTLEVELGROUPID
            });
            // Audit Section end ------------------------

            return context.SaveChanges() != 0;
        }

        public bool DeleteAlertLevelGroup(int id, UserInfo user)
        {
            var entity = this.context.TBL_ALERT_LEVEL_GROUP.Find(id);

            var auditStaff = (context.TBL_STAFF.Where(x => x.STAFFID == user.createdBy).Select(x => x.STAFFCODE));
            // Audit Section ---------------------------
            this.audit.AddAuditTrail(new TBL_AUDIT
            {
                AUDITTYPEID = (short)AuditTypeEnum.AlertLevelGroupDeleted,
                STAFFID = user.createdBy,
                BRANCHID = (short)user.BranchId,
                DETAIL = $"TBL_ALERT_TITLE '{entity.ToString()}' was deleted by {auditStaff}",
                IPADDRESS = user.userIPAddress,
                URL = user.applicationUrl,
                APPLICATIONDATE = general.GetApplicationDate(),
                SYSTEMDATETIME = DateTime.Now,
                TARGETID = entity.ALERTLEVELGROUPID
            });
            // Audit Section end ------------------------

            return context.SaveChanges() != 0;
        }

        
        public IEnumerable<AlertLevelViewModel> GetAllAlertLevel()
        {
            var alerts = (from a in context.TBL_ALERT_LEVEL
                          select new AlertLevelViewModel
                          {
                              alertLevelId = a.ALERTLEVELID,
                              emailList = a.EMAILLIST,
                              levelCode = a.LEVELCODE
                          });
            return alerts;
        }

        public AlertLevelViewModel GetAlertLevelById(int id)
        {
            var alert = (from a in context.TBL_ALERT_LEVEL.Where(x => x.ALERTLEVELID == id)
                         select new AlertLevelViewModel
                         {
                             alertLevelId = a.ALERTLEVELID,
                             emailList = a.EMAILLIST,
                             levelCode = a.LEVELCODE
                         }).FirstOrDefault();
            return alert;
        }

        public bool AddAlertLevel(AlertLevelViewModel model)
        {
            var entity = new TBL_ALERT_LEVEL
            {
                EMAILLIST = model.emailList,
                LEVELCODE = model.levelCode
            };

            context.TBL_ALERT_LEVEL.Add(entity);

            var auditStaff = (context.TBL_STAFF.Where(x => x.STAFFID == model.createdBy).Select(x => x.STAFFCODE));
            // Audit Section ---------------------------
            this.audit.AddAuditTrail(new TBL_AUDIT
            {
                AUDITTYPEID = (short)AuditTypeEnum.AlertLevelAdded,
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

        public bool UpdateAlertLevel(int id, AlertLevelViewModel model, UserInfo user)
        {
            var entity = this.context.TBL_ALERT_LEVEL.Find(id);
            entity.EMAILLIST = model.emailList;
            entity.LEVELCODE = model.levelCode;

            var auditStaff = (context.TBL_STAFF.Where(x => x.STAFFID == user.createdBy).Select(x => x.STAFFCODE));
            // Audit Section ---------------------------
            this.audit.AddAuditTrail(new TBL_AUDIT
            {
                AUDITTYPEID = (short)AuditTypeEnum.AlertLevelUpdated,
                STAFFID = user.createdBy,
                BRANCHID = (short)user.BranchId,
                DETAIL = $"TBL_ALERT_TITLE'{entity}' was updated by {auditStaff}",
                IPADDRESS = user.userIPAddress,
                URL = user.applicationUrl,
                APPLICATIONDATE = general.GetApplicationDate(),
                SYSTEMDATETIME = DateTime.Now,
                TARGETID = entity.ALERTLEVELID
            });
            // Audit Section end ------------------------

            return context.SaveChanges() != 0;
        }

        public bool DeleteAlertLevel(int id, UserInfo user)
        {
            var entity = this.context.TBL_ALERT_LEVEL.Find(id);

            var auditStaff = (context.TBL_STAFF.Where(x => x.STAFFID == user.createdBy).Select(x => x.STAFFCODE));
            // Audit Section ---------------------------
            this.audit.AddAuditTrail(new TBL_AUDIT
            {
                AUDITTYPEID = (short)AuditTypeEnum.AlertLevelDeleted,
                STAFFID = user.createdBy,
                BRANCHID = (short)user.BranchId,
                DETAIL = $"TBL_ALERT_TITLE '{entity.ToString()}' was deleted by {auditStaff}",
                IPADDRESS = user.userIPAddress,
                URL = user.applicationUrl,
                APPLICATIONDATE = general.GetApplicationDate(),
                SYSTEMDATETIME = DateTime.Now,
                TARGETID = entity.ALERTLEVELID
            });
            // Audit Section end ------------------------

            return context.SaveChanges() != 0;
        }

        public void validateAlertCheck()
        {
            List<AlertsViewModel> alerts = new List<AlertsViewModel>();

            var alertSuject = context.TBL_ALERT_TITLE;
            var alertSetup = context.TBL_ALERT_SETUP;
            foreach(var i in alertSetup)
            {
                AlertsViewModel alert = new AlertsViewModel();

                var alertcategory = context.TBL_ALERT_TITLE.Where(x => x.ALERTTITLEID == i.TITLEID).FirstOrDefault();
                if(alertcategory != null)
                {
                    alert.alertTitle = alertcategory.TITLE;
                    alert.template = alertcategory.TEMPLATE;

                    if (validateConditionTrigger(i.FREQUENCYID, i.CONDITIONID ?? 0))
                    {
                        alert.canFire = true;
                        var levels = GetReceivergroup(i.LEVELGROUPID);
                        foreach (var level in levels)
                        {
                            var levelRecord = context.TBL_ALERT_LEVEL.Where(x => x.LEVELCODE == level.levelCode).ToList();
                            alert.receiverEmailList.AddRange(levelRecord.Select(x => x.EMAILLIST));
                        }
                    }
                    else { alert.canFire = false; }
                }

                alerts.Add(alert);
            }
            postAlertNotification(alerts);
        }

        private void postAlertNotification(List<AlertsViewModel> alerts)
        {
            foreach(var alert in alerts)
            {
                if (alert.canFire) LogEmailAlert(alert.template, alert.alertTitle, alert.receiverEmailList.ToString(), "100442", 0);
            }
        }

        private bool validateConditionTrigger(short frequencyId, short conditionId)
        {
            if (ValidateFrequency(frequencyId))
            {
                return true;
            }
            else { return false; }
        }

        private bool ValidateFrequency(short frequencyId)
        {
            var frequency = context.TBL_ALERT_FREQUENCY.Find(frequencyId);
            var systemDate = general.GetApplicationDate();
            var condition = "";
            if(frequencyId == (short)AlertFrequencyEnum.DATE)
            {
                //systemDate.Date == 
            }
            return true;
        }

        private List<AlertLevelViewModel> GetReceivergroup(int levelGroupId)
        {
          var levels =  (from g in context.TBL_ALERT_LEVEL_GROUP
                          join m in context.TBL_ALERT_LEVEL_GRP_MAPPING on g.ALERTLEVELGROUPID equals m.LEVELGROUPID
                          join l in context.TBL_ALERT_LEVEL on m.LEVELCODE equals l.LEVELCODE
                          where g.ALERTLEVELGROUPID == levelGroupId select new AlertLevelViewModel
                          {
                              levelCode = l.LEVELCODE,
                              levelGroupId = l.LEVELGROUPID
                          }).ToList();

            return levels;
        }

        private void LogEmailAlert(string messageBody, string alertSubject, string recipients, string jobReQuestCode, int targetId)
        {
            try
            {
                string recipient = recipients.Trim();

                string messageSubject = alertSubject;
                string messageContent = messageBody;
                string templateUrl = "~/EmailTemplates/Monitoring.html";
                string mailBody = EmailHelpers.PopulateBody(messageContent, templateUrl);
                MessageLogViewModel messageModel = new MessageLogViewModel
                {
                    MessageSubject = messageSubject,
                    MessageBody = mailBody,
                    MessageStatusId = 1,
                    MessageTypeId = 1,
                    FromAddress = ConfigurationManager.AppSettings["SupportEmailAddr"],
                    ToAddress = $"{recipient}",
                    DateTimeReceived = DateTime.Now,
                    SendOnDateTime = DateTime.Now,
                    ReferenceCode = jobReQuestCode,
                    targetId = targetId,
                };
                SaveMessageDetails(messageModel);
            }
            catch (Exception ex)
            {
                throw new SecureException(ex.Message);
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

        }

    }
}
