using System;
using System.Collections.Generic;
using System.Text;

namespace FintrakBanking.ViewModels.Setups.Approval
{
    public class ApprovalLevelViewModel : GenaralEntity
    {
        public int approvalLevelId { get; set; }
        public string levelName { get; set; } 
        public int groupId { get; set; } 
        public int groupOperationMappingId { get; set; } 
        public int position { get; set; }
        public decimal minimumAmount { get; set; }
        public int numberOfUsers { get; set; }
        public int numberOfApprovals { get; set; }
        public int slaInterval { get; set; }
        public int operationId { get; set; }
        public bool isPoliticallyExposed { get; set; }
        public int? tenor { get; set; }
        public short? tenorModeId { get; set; }
        public bool isActive { get; set; }
        public bool canEdit { get; set; }
        public bool canDoRiskAssessment { get; set; }
        public bool canRecieveAdjustment { get; set; }
        public bool canRecieveEmail { get; set; }
        public bool canRecieveSms { get; set; }
        public bool hasChecklist { get; set; }
        public bool canPerformFinancialAnalysis { get; set; }
        public bool requireAuthorisation { get; set; }
        public bool canOverideAuthorisation { get; set; }
        public bool routeViaStaffOrganogram { get; set; }
        public string operationName { get; set; }        
        public string tenorModename { get; set; }

    }
}
