using FintrakBanking.Common.Enum;
using FintrakBanking.Entities.Models;
using FintrakBanking.Interfaces.Notification;
using FintrakBanking.Interfaces.Setups.Approval;
using FintrakBanking.ViewModels.Notification;
using System;
using System.Collections.Generic;
using System.Linq;

namespace FintrakBanking.Repositories.Notification
{
    public class NotificationRepository : INotificationRepository
    {
        private FinTrakBankingContext context;
        private IApprovalLevelStaffRepository levelStaffRepo;
        private IApprovalLevelRepository approvelRepo;

        public NotificationRepository(FinTrakBankingContext _context,
            IApprovalLevelStaffRepository _levelStaffRepo,
            IApprovalLevelRepository _approvelRepo)
        {
            context = _context;
            approvelRepo = _approvelRepo;
            levelStaffRepo = _levelStaffRepo;
        }

        public IEnumerable<NotificationViewModel> GetNotification(int staffId, int companyId)
        {
            List<NotificationViewModel> logs = new List<NotificationViewModel>();
            var approvalLevel = levelStaffRepo.GetAllAssignedApprovalLevelStaff(companyId).Where(c => c.staffId == staffId).ToList();
            if (approvalLevel.Any())
            {
                foreach (var level in approvalLevel)
                {
                    var log = (from c in context.TBL_APPROVAL_TRAIL
                           where c.COMPANYID == companyId &&
                                 c.OPERATIONID == level.operationId &&
                                 c.APPROVALSTATUSID == (int)ApprovalStatusEnum.Pending
                                 && c.RESPONSESTAFFID == null &&
                                 c.TOAPPROVALLEVELID == level.approvalLevelId
                           group c by c.OPERATIONID into d
                           select new NotificationViewModel
                           {
                               messageCount = d.Count(),
                               message = "You have " + d.Count().ToString() + " " +
                                         context.TBL_OPERATIONS.FirstOrDefault(c => c.OPERATIONID == d
                                                                                        .Select(f => f.OPERATIONID).FirstOrDefault()).OPERATIONNAME + " request awaiting your action",
                               operationURL = d.Select(h => h.TBL_OPERATIONS.OPERATIONURL).FirstOrDefault()
                           }).FirstOrDefault();
                    if (log != null)
                    {
                        logs.Add(log);
                    }
                }
            }
            return logs;
        }

        public IEnumerable<NotificationViewModel> GetNotificationForFinalState(int staffId, int companyId)
        {
            List<NotificationViewModel> logs = new List<NotificationViewModel>();
            var approvalLevel = levelStaffRepo.GetAllAssignedApprovalLevelStaff(companyId).Where(c => c.staffId == staffId)
                .ToList();
            if (approvalLevel.Any())
            {
                foreach (var level in approvalLevel)
                {
                    var log = (from c in context.TBL_APPROVAL_TRAIL
                        where c.COMPANYID == companyId &&
                              c.OPERATIONID == level.operationId &&
                              (c.APPROVALSTATEID == (int)ApprovalState.Ended && c.APPROVALSTATUSID == (int)ApprovalStatusEnum.Approved)
                              && c.RESPONSESTAFFID == null && c.TOAPPROVALLEVELID == null
                              
                        group c by c.OPERATIONID into d
                        select new NotificationViewModel
                        {
                            messageCount = d.Count(),
                            message = d.Count().ToString() + " " +
                                      context.TBL_OPERATIONS.FirstOrDefault(c => c.OPERATIONID == d
                                                                                     .Select(f => f.OPERATIONID).FirstOrDefault()).OPERATIONNAME + " request have been completed",
                            operationURL = d.Select(h => h.TBL_OPERATIONS.OPERATIONURL).FirstOrDefault()
                        }).FirstOrDefault();
                    if (log != null)
                    {
                        logs.Add(log);
                    }
                }
            }
            return logs;
        }
    }
}