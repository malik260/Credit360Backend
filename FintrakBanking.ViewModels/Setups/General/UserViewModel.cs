using FintrakBanking.ViewModels.Admin;
using System;
using System.Collections.Generic;

namespace FintrakBanking.ViewModels.Setups.General
{
    public class UserGroupId
    {
        public string groupKey { get; set; }
        public short groupId { get; set; }
    }

    public class UserViewModel
    { 

        public UserViewModel()
        {
            groupId = new List<UserGroupId>();
            activities = new List<UserActivities>();
        }

        public int user_id { get; set; }
        public int staffId { get; set; }
        public int? roleId { get; set; }
        public string staffName { get; set; }
        public int companyId { get; set; }
        public int countryId { get; set; }
        public short? branchId { get; set; }
        public string branchName { get; set; }
        public string companyName { get; set; }
        public string email { get; set; }
        public string username { get; set; }
        public string password { get; set; }
        public bool? IsFirstLoginAttempt { get; set; }
        public bool isLocked { get; set; }
        public int? failedLogonAttempt { get; set; }
        public string securityQuestion { get; set; }
        public string securityAnswer { get; set; }
        public DateTime? nextPasswordChangeDate { get; set; }
        public bool isActive { get; set; }
        public DateTime? deactivatedDate { get; set; }
        public DateTime? lastLoginDate { get; set; }
        public int? createdBy { get; set; }
        public int? lastUpdatedBy { get; set; }
        public DateTime? dateTimeCreated { get; set; }
        public DateTime? dateTimeUpdated { get; set; }
        public bool approvalStatus { get; set; }
        public string logincode { get; set; }
        public List<UserGroupId> groupId { get; set; }
        public List<UserActivities> activities { get; set; }

        public int operationId { get; set; }
        public int approvalStatusId { get; set; }
        public string comment { get; set; }
        public DateTime? lastLockOutDate { get; set; }

        public SessionStatusInfo sessionStatusInfo { get; set; }


        public int corrMatrixId { get; set; }
        public string corrMatrixDescription { get; set; }

        public int userGroupId { get; set; }


        public string strIsFirstLogin
        {
            get { return this.IsFirstLoginAttempt.HasValue ? "First Login" : "Not a first login"; }
        }

        public string strNextPasswordChange
        {
            get
            {
                return this.nextPasswordChangeDate.HasValue
                    ? this.nextPasswordChangeDate.Value.ToString("dd/MM/yyyy")
                    : DateTime.Now.AddDays(30).ToString("dd/MM/yyyy");
            }
        }

        public string strLastLoginDate
        {
            get
            {
                return this.lastLoginDate.HasValue
                    ? this.lastLoginDate.Value.ToString("dd/MM/yyyy")
                    : DateTime.Now.AddDays(-5).ToString("dd/MM/yyyy");
            }
        }

        public int sessionTimeout { get; set; }
        public string businessUnitName { get; set; }
        public int? businessUnitId { get; set; }
    }

    public class SessionStatusInfo
    {
        public Guid loginCode { get; set; }
        public int state { get; set; }
        public string errorMessage { get; set; }
        public string ipaddress { get; set; }
        public bool isPasswordExpired { get; set; }
        public bool isFirstLogin { get; set; }
    }

    public class PasswordChangeViewModel
    {
        public string username { get; set; }
        public string currentPassword { get; set; }
        public string newPassword { get; set; }
    }

    public class ActiveUserDetails : GeneralEntity
    {
        //public int companyId { get; set; }
        //public int staffId { get; set; }
        public int user_id { get; set; }
        //public string username { get; set; }
        public string staffName { get; set; }
        public int branchId { get; set; }
        public int countryId { get; set; }
        public string branchName { get; set; }
        public string companyName { get; set; }
        public string logincode { get; set; }
        public DateTime? lastLoginDate { get; set; }
        public bool isActive { get; set; }
        public bool isLocked { get; set; }
        public int? failedLogonAttempt { get; set; }
        public DateTime? lastLockedOutDate { get; set; }
        public int? lastUpdatedBy { get; set; }
        public string actionMessage { get; set; }
        public bool lockStatus { get; set; }
        public bool accountStatus { get; set; }
        public string grantMessage { get; set; }

    }
}