using FintrakBanking.Common.Enum;
using FintrakBanking.Entities.Models;
using FintrakBanking.Interfaces.AlertMonitoring;
using FintrakBanking.ViewModels.AlertMonitoring;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.Entity.Core.Objects;
using System.Data.Entity.SqlServer;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FintrakBanking.Repositories.AlertMonitoring
{
    public class SLANotification : ISLANotification
    {
        private FinTrakBankingContext context = new FinTrakBankingContext();
        private DateTime applDate;
        public string response = string.Empty;

        public List<SLANotificationViewModel> RoleBasedApprovalNotification()
        {
            var notificationList = from a in context.TBL_APPROVAL_TRAIL
                                   join b in context.TBL_APPROVAL_LEVEL on a.TOAPPROVALLEVELID equals b.APPROVALLEVELID
                                   join s in context.TBL_STAFF on a.TOSTAFFID equals s.STAFFID
                                   join o in context.TBL_OPERATIONS on a.OPERATIONID equals o.OPERATIONID
                                   let dueTime = DbFunctions.AddHours( a.SYSTEMARRIVALDATETIME,b.SLANOTIFICATIONINTERVAL)
                                   where a.APPROVALSTATUSID == 0
                                   && a.RESPONSESTAFFID == null
                                   && a.TOSTAFFID != null
                                   && b.SLAINTERVAL > 0
                                   && DateTime.Now >= dueTime
                                   select new SLANotificationViewModel
                                   {
                                       approvalTrailId = a.APPROVALTRAILID,
                                       arrivalDate =a.ARRIVALDATE,
                                       fromApprovalLevelId=(int)a.FROMAPPROVALLEVELID,
                                       operationId = a.OPERATIONID,
                                       requestStaffId = a.REQUESTSTAFFID,
                                       salDateLine = DbFunctions.AddHours(a.SYSTEMARRIVALDATETIME,b.SLAINTERVAL),
                                       slaNotificationDate = DbFunctions.AddHours(a.SYSTEMARRIVALDATETIME,b.SLANOTIFICATIONINTERVAL),
                                       salInterval = b.SLAINTERVAL,
                                       systemArrivalDate = a.SYSTEMARRIVALDATETIME,
                                       systemResponseDate = a.SYSTEMRESPONSEDATETIME,
                                       targetId =a.TARGETID,
                                       toApprovalLevelId = a.TOAPPROVALLEVELID,
                                       toStaffId = a.TOSTAFFID,
                                       staffEmail = s.EMAIL,
                                       operationName = o.OPERATIONNAME
                                   };
            return notificationList.ToList();
        }

        public List<SLANotificationViewModel> StaffSetupBasedApprovalNotification()
        {
            var notificationList = from a in context.TBL_APPROVAL_TRAIL
                                   join b in context.TBL_APPROVAL_LEVEL on a.TOAPPROVALLEVELID equals b.APPROVALLEVELID
                                   join s in context.TBL_STAFF on a.TOSTAFFID equals s.STAFFID
                                   join o in context.TBL_OPERATIONS on a.OPERATIONID equals o.OPERATIONID
                                   where a.APPROVALSTATUSID == 0
                                   && a.RESPONSESTAFFID == null
                                   && a.TOSTAFFID == null
                                     && b.SLAINTERVAL > 0
                                   && a.STAFFROLEID !=null
                                   && DateTime.Now >= EntityFunctions.AddHours(a.SYSTEMARRIVALDATETIME, b.SLANOTIFICATIONINTERVAL)
                                   select new SLANotificationViewModel
                                   {
                                       approvalTrailId = a.APPROVALTRAILID,
                                       arrivalDate = a.ARRIVALDATE,
                                       fromApprovalLevelId = (int)a.FROMAPPROVALLEVELID,
                                       operationId = a.OPERATIONID,
                                       requestStaffId = a.REQUESTSTAFFID,
                                       salDateLine = EntityFunctions.AddHours(a.SYSTEMARRIVALDATETIME, b.SLAINTERVAL),
                                       slaNotificationDate = EntityFunctions.AddHours(a.SYSTEMARRIVALDATETIME, b.SLANOTIFICATIONINTERVAL),
                                       salInterval = b.SLAINTERVAL,
                                       systemArrivalDate = a.SYSTEMARRIVALDATETIME,
                                       systemResponseDate = a.SYSTEMRESPONSEDATETIME,
                                       targetId = a.TARGETID,
                                       toApprovalLevelId = a.TOAPPROVALLEVELID,
                                       toStaffId = a.TOSTAFFID,
                                       staffEmail = s.EMAIL,
                                       operationName = o.OPERATIONNAME
                                   };
            return notificationList.ToList();
        }

        public List<SLANotificationViewModel> StaffSpecificBasedApprovalNotification()
        {
            var notificationList = from a in context.TBL_APPROVAL_TRAIL
                                   join b in context.TBL_APPROVAL_LEVEL on a.TOAPPROVALLEVELID equals b.APPROVALLEVELID
                                   join s in context.TBL_STAFF on a.TOSTAFFID equals s.STAFFID
                                   join o in context.TBL_OPERATIONS on a.OPERATIONID equals o.OPERATIONID
                                   where a.APPROVALSTATUSID == 0
                                   && a.RESPONSESTAFFID == null
                                   && a.TOSTAFFID == null
                                   && a.STAFFROLEID == null
                                   && b.SLAINTERVAL >0
                                   && DateTime.Now >= EntityFunctions.AddHours(a.SYSTEMARRIVALDATETIME, b.SLANOTIFICATIONINTERVAL)
                                   select new SLANotificationViewModel
                                   {
                                       approvalTrailId = a.APPROVALTRAILID,
                                       arrivalDate = a.ARRIVALDATE,
                                       fromApprovalLevelId = (int)a.FROMAPPROVALLEVELID,
                                       operationId = a.OPERATIONID,
                                       requestStaffId = a.REQUESTSTAFFID,
                                       salDateLine = EntityFunctions.AddHours(a.SYSTEMARRIVALDATETIME, b.SLAINTERVAL),
                                       slaNotificationDate = EntityFunctions.AddHours(a.SYSTEMARRIVALDATETIME, b.SLANOTIFICATIONINTERVAL),
                                       salInterval = b.SLAINTERVAL,
                                       systemArrivalDate = a.SYSTEMARRIVALDATETIME,
                                       systemResponseDate = a.SYSTEMRESPONSEDATETIME,
                                       targetId = a.TARGETID,
                                       toApprovalLevelId = a.TOAPPROVALLEVELID,
                                       toStaffId = a.TOSTAFFID,
                                       staffEmail = s.EMAIL,
                                       operationName = o.OPERATIONNAME
                                   };
            return notificationList.ToList();
        }

       
    }
}
