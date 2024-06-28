using FintrakBanking.ViewModels;
using System;
using System.Collections.Generic;
using System.Text;

namespace FintrakBanking.Interfaces.Setups.Approval
{
    public class ApprovalLevelStaffViewModel : GeneralEntity
    {
        public int staffRoleId;

        public int staffLevelId { get; set; }
        public int groupId { get; set; }
        public int position { get; set; }
        public int operationId { get; set; }
       // public int staffId { get; set; }
        public int approvalLevelId { get; set; }
        public decimal maximumAmount { get; set; }
        public decimal? investmentGradeAmount { get; set; }
        public decimal? standardGradeAmount { get; set; }
        public decimal? renewalLimit { get; set; }
        public decimal? baseMinimumAmount { get; set; }
        public decimal minimumAmount { get; set; }
        public string staffLevelName {get; set; }
        public string approvalLevelName{ get; set; }

        public int processViewScope { get; set; }
        public bool canViewDocument { get; set; }
        public bool canViewUploadedFile { get; set; }
        public bool canViewApproval { get; set; }
        public bool canApprove { get; set; }
        public bool canUploadFile { get; set; }
        public bool canSendRequest { get; set; }
        public bool canEdit { get; set; }
        public bool vetoPower { get; set; }
        public int tempStaffLevelId { get; set; }
        public short approvalStatusId { get; set; }
        public string comment { get; set; }
        
    }
}
