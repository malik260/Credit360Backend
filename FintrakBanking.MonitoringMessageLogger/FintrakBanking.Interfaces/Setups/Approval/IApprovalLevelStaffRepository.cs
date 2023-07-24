using FintrakBanking.ViewModels;
using FintrakBanking.ViewModels.Reports;
using FintrakBanking.ViewModels.WorkFlow;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace FintrakBanking.Interfaces.Setups.Approval
{
    public interface IApprovalLevelStaffRepository
    {
        IEnumerable<WorkflowTrackerViewModel> GetApprovalTrailBySiteTargetId(int targetId, int companyId);
        bool AddApprovalLevelStaff(ApprovalLevelStaffViewModel model);

        bool UpdateApprovalLevelStaff(int staffLevelId, ApprovalLevelStaffViewModel model);

        Task<bool> DeleteApprovalLevelStaff(int staffLevelId, UserInfo user);

        //IEnumerable<ApprovalLevelStaffViewModel> GetAllAssignedApprovalLevelStaff(int companyId);

        IEnumerable<ApprovalLevelStaffViewModel> GetAllApprovalLevelStaffByOperationId(int operationId, int companyId);

        ApprovalLevelStaffViewModel GetAllApprovalLevelStaffByStaffId(int staffId, int companyId, int operationId);

        IEnumerable<ApprovalLevelStaffViewModel> GetAllApprovalLevelStaff(int companyId);

        IEnumerable<ApprovalLevelStaffViewModel> GetApprovalLevelStaffById(int staffLevelId, int companyId);

        //Task<IEnumerable<WorkflowTrackerViewModel>> GetApprovalTrailByOperationIdAndTargetId(int operationId,
        //    int targetId, int companyId);

        IEnumerable<WorkflowTrackerViewModel> GetApprovalTrailByOperationIdAndTargetId(int operationId,
            int targetId, int companyId);

        IQueryable<WorkflowTrackerViewModel> GetAllRecordsOnApprovalTrail(int companyId);

        IQueryable<ApprovalLevelStaffViewModel> GetAllAssignedApprovalLevelStaff(int companyId);

        ApprovalLevelStaffViewModel GetAllApprovalLevelStaffByStaffId(int staffId, int companyId);
        List<WorkflowTrackerViewModel> GetAllApprovalStatus();
        List<WorkflowTrackerViewModel> GetAllApprovalOperations();
        int GoForApproval(ApprovalLevelStaffViewModel model);
        IEnumerable<ApprovalLevelStaffViewModel> GetTempApprovalLevelStaff(int staffId);
        List<WorkflowTrackerViewModel> GetApprovalMointoring(DateRange param);
        List<WorkflowTrackerViewModel> GetBookingMointoring(DateRange param);
        List<WorkflowTrackerViewModel> GetContractReviewMointoring(DateRange param);
        List<WorkflowTrackerViewModel> GetBookingApprovalTrailByTargetId(int targetId, int companyId);
        List<WorkflowTrackerViewModel> GetApprovalTrailByTargetId(int targetId, int companyId);
        WorkflowTrackerViewModel GenerateApprovalMonitoringReport(DateRange param);
        WorkflowTrackerViewModel ExportApprovalComments(List<ApprovalTrailViewModel> commentsData, bool requireAll);
        IQueryable<int> GetAllDetailedApprovalLevelStaffApprovalLevelId(int companyId, int staffId);
    }
}