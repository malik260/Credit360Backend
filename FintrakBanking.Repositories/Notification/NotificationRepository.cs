using FintrakBanking.Common.Enum;
using FintrakBanking.Entities.Models;
using FintrakBanking.Interfaces.Notification;
using FintrakBanking.Interfaces.Setups.Approval;
using FintrakBanking.ViewModels.Notification;
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
            var approvalLevel = levelStaffRepo.GetAllAssignedApprovalLevelStaff(companyId).Where(c => c.staffId == staffId);
            if (approvalLevel.Any())
            {
                foreach (var level in approvalLevel)
                {
                    NotificationViewModel log = new NotificationViewModel();

                    log = (from c in context.tbl_Approval_Trail
                           where c.CompanyId == companyId &&
                           c.OperationId == level.operationId &&
                            c.ApprovalStatusId == (int)ApprovalStatusEnum.Pending  && c.ResponseStaffId == null &&
                           c.ToApprovalLevelId == level.approvalLevelId
                           group c by c.OperationId into d
                           select new NotificationViewModel
                           {
                               massageCount = d.Count(),
                               message = "You have " + d.Count().ToString() + " " +
                               context.tbl_Operations.FirstOrDefault(c => c.OperationId == d
                             .Select(f => f.OperationId).FirstOrDefault()).OperationName + " request awaiting your action",
                               operationURL = d.Select(h => h.tbl_Operations.OperationURL).FirstOrDefault()
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