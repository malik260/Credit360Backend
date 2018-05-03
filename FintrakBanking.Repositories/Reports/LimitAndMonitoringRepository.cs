using FintrakBanking.Interfaces.Reports;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FintrakBanking.ViewModels.Report;
using FintrakBanking.Entities.Models;
using FintrakBanking.Interfaces.Setups.General;
using FintrakBanking.Interfaces.Admin;
using FintrakBanking.Common.Enum;

namespace FintrakBanking.Repositories.Reports
{
   public class LimitAndMonitoringRepository : ILimitAndMonitoringRepository
    {
        private FinTrakBankingContext context;
        private IGeneralSetupRepository generalSetup;
        private IAuditTrailRepository auditTrail;


        public LimitAndMonitoringRepository(
            FinTrakBankingContext _context,
        IGeneralSetupRepository _generalSetup,
        IAuditTrailRepository _auditTrail
            )
        {
            context = _context;
            generalSetup = _generalSetup;
            auditTrail = _auditTrail;
        }


        public string AddNewEmailMessageSeeting(LimitAndMonitoringViewModel alertSetting)
        {
            try
            {
                //if (alertSetting != null)
                //{
                //    var alertMessage = new TBL_ALERT_MESSAGE_SETTING
                //    {
                //        ALERTTITLE = alertSetting.AlertMessage,
                //        ALERTMESSAGE = alertSetting.AlertMessage,
                //        ALERTDATE = alertSetting.AlertDate,
                //        ALERTTIME = alertSetting.AlertTime,
                //        ALERTREOCCURRENCE = alertSetting.AlertReoccurrance,
                //        DAYSBEFOREALERT = alertSetting.DaysBeforeAlert


                //    };
                //    context.TBL_ALERT_MESSAGE_SETTING.Add(alertMessage);


                //    var audit = new TBL_AUDIT
                //    {
                //        AUDITTYPEID = (short)AuditTypeEnum.AlertAndMonitoryAdded,
                //        STAFFID = alertSetting.staffId,
                //        BRANCHID = (short)alertSetting.userBranchId,
                //        DETAIL = $"New Alert Message with title : {alertSetting.AlertTitle} is added",
                //        IPADDRESS = alertSetting.userIPAddress,
                //        URL = alertSetting.applicationUrl,
                //        APPLICATIONDATE = generalSetup.GetApplicationDate(),
                //        SYSTEMDATETIME = DateTime.Now
                //    };
                //    this.auditTrail.AddAuditTrail(audit);
                //    context.SaveChanges();

                //    return "The record has been added successful";

                //}
                return "The record has not been added";
            }
            catch (Exception ex)
            {

                throw;
            }
        }

       

        public IEnumerable<LimitAndMonitoringViewModel> GetAllSetEmailAlertMessages()
        {
            //return context.TBL_ALERT_MESSAGE_SETTING.Select(x => new LimitAndMonitoringViewModel
            //{
            //    AlertId = x.ALERTID,
            //    AlertMessage = x.ALERTMESSAGE,
            //    AlertTitle = x.ALERTTITLE,
            //    AlertDate = x.ALERTDATE,
            //    AlertTime = x.ALERTTIME,
            //    AlertReoccurrance = x.ALERTREOCCURRENCE,
            //    DaysBeforeAlert = x.DAYSBEFOREALERT
            //});
            return new List<LimitAndMonitoringViewModel>();
        }

        public LimitAndMonitoringViewModel GetAllSetEmailAlertMessages(int alertId)
        {
            //return (context.TBL_ALERT_MESSAGE_SETTING.Where(x => x.ALERTID == alertId).Select(x => new LimitAndMonitoringViewModel
            //{
            //    AlertId = x.ALERTID,
            //    AlertMessage = x.ALERTMESSAGE,
            //    AlertTitle = x.ALERTTITLE,
            //    AlertDate = x.ALERTDATE,
            //    AlertTime = x.ALERTTIME,
            //    AlertReoccurrance = x.ALERTREOCCURRENCE,
            //    DaysBeforeAlert = x.DAYSBEFOREALERT
            //})).FirstOrDefault();
            return new LimitAndMonitoringViewModel();
        }

        public string RemoveEmailMessageSeeting(LimitAndMonitoringViewModel alertSetting)
        {
            //var alertMessage = context.TBL_ALERT_MESSAGE_SETTING.Find(alertSetting.AlertId);
            //if (alertMessage != null)
            //{
            //    context.TBL_ALERT_MESSAGE_SETTING.Remove(alertMessage);

            //    var audit = new TBL_AUDIT
            //    {
            //        AUDITTYPEID = (short)AuditTypeEnum.AlertAndMonitoryAdded,
            //        STAFFID = alertSetting.staffId,
            //        BRANCHID = (short)alertSetting.userBranchId,
            //        DETAIL = $"Alert Message with {alertSetting.AlertId} id is deleted from the table",
            //        IPADDRESS = alertSetting.userIPAddress,
            //        URL = alertSetting.applicationUrl,
            //        //  APPLICATIONDATE = generalSetup.GetApplicationDate(),
            //        SYSTEMDATETIME = DateTime.Now
            //    };
            //    this.auditTrail.AddAuditTrail(audit);
            //    context.SaveChanges();

            //    return "The record has been deleted successful";

            //}
            return "The record has not been deleted";
        }

        public string UpdateEmailMessageSeeting(LimitAndMonitoringViewModel alertSetting)
        {
            //var alertMessage = context.TBL_ALERT_MESSAGE_SETTING.Find(alertSetting.AlertId);
            //if (alertMessage != null)
            //{
            //    alertMessage.ALERTTITLE = alertSetting.AlertMessage;
            //    alertMessage.ALERTMESSAGE = alertSetting.AlertMessage;
            //    alertMessage.ALERTDATE = alertSetting.AlertDate;
            //    alertMessage.ALERTTIME = alertSetting.AlertTime;
            //    alertMessage.ALERTREOCCURRENCE = alertSetting.AlertReoccurrance;
            //    alertMessage.DAYSBEFOREALERT = alertSetting.DaysBeforeAlert;

            //    var audit = new TBL_AUDIT
            //    {
            //        AUDITTYPEID = (short)AuditTypeEnum.AlertAndMonitoryAdded,
            //        STAFFID = alertSetting.staffId,
            //        BRANCHID = (short)alertSetting.userBranchId,
            //        DETAIL = $"Alert Message with  {alertSetting.AlertId} id is edited",
            //        IPADDRESS = alertSetting.userIPAddress,
            //        URL = alertSetting.applicationUrl,
            //        //  APPLICATIONDATE = generalSetup.GetApplicationDate(),
            //        SYSTEMDATETIME = DateTime.Now
            //    };
            //    this.auditTrail.AddAuditTrail(audit);
            //    context.SaveChanges();


            //    return "The record has been updated successful";

            //}
            return "The record has not been updated";
        }
        public IEnumerable<LimitAndMonitoringViewModel> GetAllEmailAlertMessages()
        {
            var data = context.TBL_MONITORING_ALERT_SETUP.Select(x => new LimitAndMonitoringViewModel {
                monitoringItemId = x.MONITORING_ITEMID,
                messageTitle = x.MESSAGE_TITLE,
                messageBody = x.MONITORING_ITEM_NAME,
                notificationPeriod1 = x.NOTIFICATION_PERIOD1,
                escalationLevel1= x.RECIPIENTEMAILS1,
                notificationPeriod2 = x.NOTIFICATION_PERIOD2,
                escalationLevel2 = x.RECIPIENTEMAILS2,
                notificationPeriod3 = x.NOTIFICATION_PERIOD3,
                escalationLevel3 = x.RECIPIENTEMAILS3,
            }).ToList();

            return data;
        }

        public string UpdateEmailAlertMessages(LimitAndMonitoringViewModel data)
        {
            var alertMessage = context.TBL_MONITORING_ALERT_SETUP.Find(data.monitoringItemId);
            if (alertMessage!=null)
            {
                alertMessage.NOTIFICATION_PERIOD1 = data.notificationPeriod1;
                alertMessage.RECIPIENTEMAILS1 = data.escalationLevel1;
                alertMessage.NOTIFICATION_PERIOD2 = data.notificationPeriod2;
                alertMessage.RECIPIENTEMAILS2 = data.escalationLevel2;
                alertMessage.NOTIFICATION_PERIOD3 = data.notificationPeriod3;
                alertMessage.RECIPIENTEMAILS3 = data.escalationLevel3;
                alertMessage.MESSAGE_TEMPLATE = "Alert Message";

                //var audit = new TBL_AUDIT
                //{
                //    AUDITTYPEID = (short)AuditTypeEnum.AlertAndMonitoryAdded,
                //    STAFFID = data.staffId,
                //    BRANCHID = (short)data.userBranchId,
                //    DETAIL = $"Alert Message with  {data.monitoringItemId} id is edited",
                //    IPADDRESS = data.userIPAddress,
                //    URL = data.applicationUrl,
                //    //  APPLICATIONDATE = generalSetup.GetApplicationDate(),
                //    SYSTEMDATETIME = DateTime.Now
                //};
                //this.auditTrail.AddAuditTrail(audit);
                context.SaveChanges();

                return "The record has been updated successful";
            }
            return "The record has not been updated";
        }
    }
}
