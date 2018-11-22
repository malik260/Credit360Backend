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

       
    }
    
}