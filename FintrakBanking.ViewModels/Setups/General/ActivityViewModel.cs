using System.Collections.Generic;

namespace FintrakBanking.ViewModels.Setups.General
{
    public class ActivityViewModel
    {
        public int activityId { get; set; }
        public int activityParentId { get; set; }
        public string activityName { get; set; }
        public bool selected { get; set; }

    }

    public class ActivityParent
    {
        public ActivityParent()
        {
            activities = new List<ActivityViewModel>();
        }

        public int activityParentId { get; set; }
        public string activityParentName { get; set; }

        public List<ActivityViewModel> activities { get; set; }
    }


    public class GroupVModel
    {
        public GroupVModel()
        {
            this.activities = new List<GroupActivitiesModel>();
        }

        public short groupId { get; set; }
        public string name { get; set; }
        public List<GroupActivitiesModel> activities { get; set; }
    }


    public class GroupActivitiesModel
    {
        public int groupActivityId { get; set; }
        public int activityId { get; set; }
        public string activityName { get; set; }
        public bool canEdit { get; set; }
        public bool canAdd { get; set; }
        public bool canView { get; set; }
        public bool canDelete { get; set; }
        public bool canApprove { get; set; }
    }

    public class ActivitiesUpdateVm
    {
        public bool canEdit { get; set; }
        public bool canAdd { get; set; }
        public bool canView { get; set; }
        public bool canDelete { get; set; }
        public bool canApprove { get; set; }
    }
}