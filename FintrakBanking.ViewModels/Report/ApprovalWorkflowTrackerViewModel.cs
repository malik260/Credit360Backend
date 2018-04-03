using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FintrakBanking.ReportObjects.ViewModels
{
    public class GroupWorkflowTracker
    {
        public string groupName { get; set; }
        public string levelName { get; set; }
        public string order { get; set; }
    }
    public class LevelWorkflowTracker
    {
        public string levelName { get; set; }
        public string levelLimitAmount { get; set; }
        public string order { get; set; }
        public bool MyProperty { get; set; }
    }

    

    public class GroupWorkFlowSetup
    {

       public string CompanyName { get; set; }
        public int OperationId { get; set; }
        public string GroupName { get; set; }
        public int CompanyId { get; set; }
        public bool IsCommittee { get; set; }
        public bool IsBeforeCAMApproval { get; set; }
        public string LevelName { get; set; }
        public decimal MinimumAmount { get; set; }
        public int NumberOfUsers { get; set; }
        public int NumberOfApprovals { get; set; }
        public int SLAInterval { get; set; }
        public bool CanRouteBack { get; set; }
        public bool IsPoliticallyExposed { get; set; }
        public bool IsActive { get; set; }
        public bool CanEdit { get; set; }
        public bool CanDoRiskAssessment { get; set; }
        public bool CanRecieveAdjustment { get; set; }
        public bool CanRecieveEmail { get; set; }
        public bool CanRecieveSMS { get; set; }
        public bool HasChecklist { get; set; }
        public bool CanPerformFinancialAnalysis { get; set; }
        public bool RequireAuthorisation { get; set; }
        public bool CanOverideAuthorisation { get; set; }
        public bool RouteViaStaffOrganogram { get; set; }
        public string StaffName { get; set; }
        public decimal MaximumAmount { get; set; }
        public short ProcessViewScopeId { get; set; }
        public bool CanViewCAMDocument { get; set; }
        public bool CanViewUploadedFile { get; set; }
        public bool CanViewApproval { get; set; }
        public bool CanApprove { get; set; }
        public bool CanUploadFile { get; set; }
        public bool CanSendJobRequest { get; set; }
        public bool VetoPower { get; set; }
    }

    public class WorkFlowViewModel
    {
        public string operationName { get; set; }
        public string groupName { get; set; }
        public string vetoPower { get; set; }
        public string levelName { get; set; }
        public string username { get; set; }
        public string scope { get; set; }
        public int grpPosition { get; set; }
        public int levelPosition { get; set; }
        public string canApprove { get; set; }
        public string canEdit { get; set; }
        public string canUploadFile { get; set; }
        public string canSendJobRequest { get; set; }
        public string staffLevelId { get; set; }
        public DateTime reportDateTime { get { return DateTime.Now; } }
    }
}
