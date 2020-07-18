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
    public class SLANotification 
    {
         FinTrakBankingContext context = new FinTrakBankingContext();

        // private FinTrakBankingContext context = new FinTrakBankingContext();
        //private DateTime applDate;
        //  public string response = string.Empty;

        public IEnumerable<SLANotificationViewModel> RoleBasedApprovalNotification()
        {
            var list = new List<SLANotificationViewModel>();

            var notificationList = from a in context.TBL_APPROVAL_TRAIL
                                   join b in context.TBL_APPROVAL_LEVEL on a.TOAPPROVALLEVELID equals b.APPROVALLEVELID
                                   join s in context.TBL_STAFF on a.TOSTAFFID equals s.STAFFID
                                   join o in context.TBL_OPERATIONS on a.OPERATIONID equals o.OPERATIONID
                                   where a.APPROVALSTATUSID == (int)ApprovalStatusEnum.Pending
                                   && a.RESPONSESTAFFID == null
                                   && a.TOSTAFFID != null
                                   && b.SLAINTERVAL > 0
                                   select new SLANotificationViewModel
                                   {
                                       approvalTrailId = a.APPROVALTRAILID,
                                       arrivalDate = a.ARRIVALDATE,
                                       fromApprovalLevelId = (int)a.FROMAPPROVALLEVELID,
                                       operationId = a.OPERATIONID,
                                       requestStaffId = a.REQUESTSTAFFID,
                                       salInterval = b.SLAINTERVAL,
                                       systemArrivalDate = a.SYSTEMARRIVALDATETIME,
                                       systemResponseDate = (DateTime)a.SYSTEMRESPONSEDATETIME,
                                       targetId = a.TARGETID,
                                       toApprovalLevelId = a.TOAPPROVALLEVELID,
                                       toStaffId = a.TOSTAFFID,
                                       staffEmail = s.EMAIL,
                                       operationName = o.OPERATIONNAME,
                                       slaNotificationInterval = b.SLANOTIFICATIONINTERVAL,
                                       

                                   };

            var data = new SLANotificationViewModel();

            foreach (var x in notificationList)
            {
                if (DateTime.Now >= (DateTime)x.systemArrivalDate.AddHours(x.slaNotificationInterval))
                {
                    data = new SLANotificationViewModel
                    {
                        approvalTrailId = x.approvalTrailId,
                        arrivalDate = x.arrivalDate,
                        fromApprovalLevelId = x.fromApprovalLevelId,
                        operationId = x.operationId,
                        requestStaffId = x.requestStaffId,
                        salDateLine = (DateTime)x.systemArrivalDate.AddHours(x.salInterval),
                        slaNotificationDate = (DateTime)x.systemArrivalDate.AddHours(x.slaNotificationInterval),
                        salInterval = x.salInterval,
                        systemArrivalDate = x.systemArrivalDate,
                        systemResponseDate = x.systemResponseDate,
                        targetId = x.targetId,
                        toApprovalLevelId = x.toApprovalLevelId,
                        toStaffId = x.toStaffId,
                        staffEmail = x.staffEmail,
                        operationName = x.operationName,
                        slaNotificationInterval = x.slaNotificationInterval,
                       
                    };
                    list.Add(data);
                }
            }
            return list.ToList();
       }

        public IEnumerable<SLANotificationViewModel> StaffSetupBasedApprovalNotification()
        {
            var list = new List<SLANotificationViewModel>();

            var notificationList = from a in context.TBL_APPROVAL_TRAIL
                                   join b in context.TBL_APPROVAL_LEVEL on a.TOAPPROVALLEVELID equals b.APPROVALLEVELID
                                   join s in context.TBL_STAFF on a.TOSTAFFID equals s.STAFFID
                                   join o in context.TBL_OPERATIONS on a.OPERATIONID equals o.OPERATIONID
                                   where a.APPROVALSTATUSID == (int)ApprovalStatusEnum.Pending
                                   && a.RESPONSESTAFFID == null
                                   && a.TOSTAFFID == null
                                     && b.SLAINTERVAL > 0
                                     && b.STAFFROLEID != null
                                   select new SLANotificationViewModel
                                   {
                                       approvalTrailId = a.APPROVALTRAILID,
                                       arrivalDate = a.ARRIVALDATE,
                                       fromApprovalLevelId = (int)a.FROMAPPROVALLEVELID,
                                       operationId = a.OPERATIONID,
                                       requestStaffId = a.REQUESTSTAFFID,
                                       salInterval = b.SLAINTERVAL,
                                       systemArrivalDate = a.SYSTEMARRIVALDATETIME,
                                       systemResponseDate = (DateTime)a.SYSTEMRESPONSEDATETIME,
                                       targetId = a.TARGETID,
                                       toApprovalLevelId = a.TOAPPROVALLEVELID,
                                       toStaffId = a.TOSTAFFID,
                                       staffEmail = s.EMAIL,
                                       operationName = o.OPERATIONNAME,
                                       slaNotificationInterval = b.SLANOTIFICATIONINTERVAL
                                   };
            var data = new SLANotificationViewModel();

            foreach (var x in notificationList)
            {
                if (DateTime.Now >= (DateTime)x.systemArrivalDate.AddHours(x.slaNotificationInterval))
                {
                    data = new SLANotificationViewModel
                    {
                        approvalTrailId = x.approvalTrailId,
                        arrivalDate = x.arrivalDate,
                        fromApprovalLevelId = x.fromApprovalLevelId,
                        operationId = x.operationId,
                        requestStaffId = x.requestStaffId,
                        salDateLine = (DateTime)x.systemArrivalDate.AddHours(x.salInterval),
                        slaNotificationDate = (DateTime)x.systemArrivalDate.AddHours(x.slaNotificationInterval),
                        salInterval = x.salInterval,
                        systemArrivalDate = x.systemArrivalDate,
                        systemResponseDate = x.systemResponseDate,
                        targetId = x.targetId,
                        toApprovalLevelId = x.toApprovalLevelId,
                        toStaffId = x.toStaffId,
                        staffEmail = x.staffEmail,
                        operationName = x.operationName,
                        slaNotificationInterval = x.slaNotificationInterval
                    };
                    list.Add(data);
                }
            }
            return list.ToList();
        }

        public IEnumerable<SLANotificationViewModel> StaffSpecificBasedApprovalNotification()
        {
            var list = new List<SLANotificationViewModel>();

            var notificationList = from a in context.TBL_APPROVAL_TRAIL
                                   join b in context.TBL_APPROVAL_LEVEL on a.TOAPPROVALLEVELID equals b.APPROVALLEVELID
                                   join s in context.TBL_STAFF on a.TOSTAFFID equals s.STAFFID
                                   join o in context.TBL_OPERATIONS on a.OPERATIONID equals o.OPERATIONID
                                   where a.APPROVALSTATUSID == (int)ApprovalStatusEnum.Pending
                                   && a.RESPONSESTAFFID == null
                                   && a.TOSTAFFID == null
                                   && b.STAFFROLEID == null
                                   && b.SLAINTERVAL > 0
                                   select new SLANotificationViewModel
                                   {
                                       approvalTrailId = a.APPROVALTRAILID,
                                       arrivalDate = a.ARRIVALDATE,
                                       fromApprovalLevelId = (int)a.FROMAPPROVALLEVELID,
                                       operationId = a.OPERATIONID,
                                       requestStaffId = a.REQUESTSTAFFID,
                                       salInterval = b.SLAINTERVAL,
                                       systemArrivalDate = a.SYSTEMARRIVALDATETIME,
                                       systemResponseDate = (DateTime)a.SYSTEMRESPONSEDATETIME,
                                       targetId = a.TARGETID,
                                       toApprovalLevelId = a.TOAPPROVALLEVELID,
                                       toStaffId = a.TOSTAFFID,
                                       staffEmail = s.EMAIL,
                                       operationName = o.OPERATIONNAME,
                                       slaNotificationInterval = b.SLANOTIFICATIONINTERVAL
                                   };

            var data = new SLANotificationViewModel();

            foreach (var x in notificationList)
            {
                if (DateTime.Now >= (DateTime)x.systemArrivalDate.AddHours(x.slaNotificationInterval))
                {
                    data = new SLANotificationViewModel
                    {
                        approvalTrailId = x.approvalTrailId,
                        arrivalDate = x.arrivalDate,
                        fromApprovalLevelId = x.fromApprovalLevelId,
                        operationId = x.operationId,
                        requestStaffId = x.requestStaffId,
                        salDateLine = (DateTime)x.systemArrivalDate.AddHours(x.salInterval),
                        slaNotificationDate = (DateTime)x.systemArrivalDate.AddHours(x.slaNotificationInterval),
                        salInterval = x.salInterval,
                        systemArrivalDate = x.systemArrivalDate,
                        systemResponseDate = x.systemResponseDate,
                        targetId = x.targetId,
                        toApprovalLevelId = x.toApprovalLevelId,
                        toStaffId = x.toStaffId,
                        staffEmail = x.staffEmail,
                        operationName = x.operationName,
                        slaNotificationInterval = x.slaNotificationInterval
                    };
                    list.Add(data);
                }
            }
            return list.ToList();
        }


    }
}
