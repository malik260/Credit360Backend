using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FintrakBanking.ViewModels.WorkFlow
{
    public class ApprovalTrailViewModel : GeneralEntity
    {
        public string commentStage { get; set; }

        public int? applicationId { get; set; }

        public int approvalTrailId { get; set; }
        public int targetId { get; set; }
        public DateTime arrivalDate { get; set; }
        public DateTime systemArrivalDateTime { get; set; }
        public DateTime? responseDate { get; set; }
        public DateTime? systemResponseDateTime { get; set; }
        public int? responseStaffId { get; set; }
        public int requestStaffId { get; set; }
        public int? fromApprovalLevelId { get; set; }
        public int? toApprovalLevelId { get; set; }
        public int? loopedStaffId { get; set; }
        public int? toStaffId { get; set; }
        public short approvalStateId { get; set; }
        public short approvalStatusId { get; set; }
        public int operationId { get; set; }
        public string comment { get; set; }
        public string staffName { get; set; }
        public string fromApprovalLevelName { get; set; }
        public string approvalStatus { get; set; }
        public string approvalState { get; set; }
        public string fromStaffName { get; set; }
        public string toStaffName { get; set; }
        public string toApprovalLevelName { get; set; }
        public short? vote { get; set; }
        public short? sourceOperationId { get; set; }
        public int? sourceTargetId { get; set; }
        public string responsestaffName { get; set; }
        public int? reliefStaffId { get; set; }
        public string reliefStaff { get; set; }
        public string loopedStaff { get; set; }
        public string oprationName { get; set; }
        public short? referBackState { get; set; }
    }
    public class ApprovalTrailDetailsViewModel : ApprovalTrailViewModel
    {
        public string targetName { get; set; }
        public string operationName { get; set; }
        public string approvalStatusName { get; set; }
        public string approvalLevelName { get; set; }
    }

    public class ApprovalTrailCallMemoViewModel
    {
        public string levelName { get; set; }
        public int approvalLevelId { get; set; }
    }
}
