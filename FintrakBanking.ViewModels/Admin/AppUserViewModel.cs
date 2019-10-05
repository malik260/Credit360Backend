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


        public string password { get; set; }
        public string confirmPassword { get; set; }
        public string securityQuestion { get; set; }
        public string securityAnswer { get; set; }
        public bool changePassword { get; set; }
        public bool changeSecutirtyQuestion { get; set; }

        public List<UserGroup> group { get; set; }
        public List<UserActivities> activities { get; set; }
    }

    public class UserGroup
    {
        public int staffRoleId { get; set; }
        public string groupKey { get; set; }
        public short groupId { get; set; }
    }

    public class UserActivities
    {
        public int userId { get; set; }
        public int activityId { get; set; }
        public string activityName { get; set; }
        public int activityParentId { get; set; }
        public string activityParentName { get; set; }

        public bool selected { get; set; }
        public DateTime? expireOn { get; set; }

    }
    public class Users
    {
        public string Email { get; set; }
        public string UserName { get; set; }
        public string DisplayName { get; set; }
        public bool isMapped { get; set; }
        public string firstName { get; set; }
        public string middleName { get; set; }
        public string lastName { get; set; }
        public string department { get; set; }
        public string fullName { get; set; }
        public string staffRole { get; set; }
        public int? staffRoleId { get; set; }

        public string roleCode { get; set; }
        public string roleDescription { get; set; }
        public string solID { get; set; }


    }
}
