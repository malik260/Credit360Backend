using FintrakBanking.ViewModels.Admin;
using System;
using System.Collections.Generic;

namespace FintrakBanking.ViewModels.Setups.General
{
    public class StaffRoleViewModel : GeneralEntity
    {
        public StaffRoleViewModel()
        {
            userGroup = new List<UserGroup>();
            activities = new List<UserActivities>();
        }
        public int staffRoleId { get; set; }
        public string staffRoleCode { get; set; }
        public string staffRoleName { get; set; }
        public decimal? workStartDuration { get; set; }
        public decimal? workEndDuration { get; set; }
        public int operationId { get; set; }
        public int approvalStatusId { get; set; }
        public List<UserGroup> userGroup { get; set; }
        public List<UserActivities> activities { get; set; }
        public short? allocationTypeId { get; set; }


        public List<int> userGroupIds { get; set; }
        public List<int> activitieIds { get; set; }
        public string staffRoleShortCode { get; set; }
        public bool useRoundRublin { get; set; }
        public bool useSbuRouting { get; set; }
        public short? approvalFlowTypeId { get; set; }
    }

    public class ApprovalSetUpViewModel: GeneralEntity
    {
        
        public int approvalsetupId { get; set; }
        public bool useRoundRublin { get; set; }
        public bool isRetailOnlyRoundRobin { get; set; }

    }

    public class OperationPageOrderViewModel: GeneralEntity
    {
        public int operationId { get; set; }
        public short floworderId { get; set; }
        public string operationName { get; set; }
        public string tag { get; set; }
        public bool requiredAppraisal { get; set; }
        public bool requiredAvailment { get; set; }
        public bool requiredOfferLetter { get; set; }
    }

    public class StaffGroupEmailViewModel
    {
        public int groupEmailId { get; set; }
        public string groupCode { get; set; }
        public string groupName { get; set; }
        public string groupEmail { get; set; }

    }

    public class ApprovalFlowTypeViewModel : GeneralEntity
    {
        public int approvalFlowTypeId { get; set; }
        public string flowTypeName { get; set; }
    }
}