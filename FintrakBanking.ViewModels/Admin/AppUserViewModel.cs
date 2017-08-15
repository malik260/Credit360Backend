using System;
using System.Collections.Generic;
using System.Text;

namespace FintrakBanking.ViewModels.Admin
{
    public class AppUserViewModel : GeneralEntity
    {
        public AppUserViewModel()
        {
            group = new List<UserGroup>();
            activities = new List<UserActivities>();
        }

        public int staffId { get; set; }
        public string username { get; set; }
        public string password { get; set; }
        public string confirmPassword { get; set; }
        public string securityQuestion { get; set; }
        public string securityAnswer { get; set; } 

        public List<UserGroup> group { get; set; }
        public List<UserActivities> activities { get; set; }
    }

    public class UserGroup
    {
        public string groupKey { get; set; }
        public short groupId { get; set; }
    }

    public class UserActivities
    {
        public int userId { get; set; }
        public int activityId { get; set; }
        public string activityName { get; set; }
    }
}
