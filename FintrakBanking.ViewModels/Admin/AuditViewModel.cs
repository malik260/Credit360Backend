using System;
using System.Collections.Generic;
using System.Text;

namespace FintrakBanking.ViewModels.Admin
{
    public class AuditViewModel : GeneralEntity
    {
        public long auditId { get; set; }
        public string auditType { get; set; }
        public int auditTypeId { get; set; }
        public string firstName { get; set; }
        public string lastName { get; set; }
        public DateTime systemDate { get; set; }
        public DateTime applicationDate { get; set; }
        public string details { get; set; }
        public string url { get; set; }
        //public string username { get; set; }
        public string branchName { get; set; }
        public string  fullName { get { return firstName + " " + lastName;  }}
        public string formattedSystemDate { get { return this.systemDate.ToString("dd/MM/yyy"); } }
        public string formattedApplicationDate { get { return this.systemDate.ToString("dd/MM/yyy"); } }

        public string staffName => $"{this.firstName} {this.lastName}";
        public string ipAddress { get; set; }
        public string logo { get; set; }
        public string deviceName { get; set; }
        public string osName { get; set; }
    }

    public class LoggingActivities
    {
        public string names { get; set; }
        public string userName { get; set; }
        public bool isUserLocked { get; set; }
        public int? failedLoggingAttempts { get; set; }
        public bool isUserActive { get; set; }
        public DateTime? deactivatedDate { get; set; }
        public DateTime? lastLogginDate { get; set; }
        public DateTime? lastLogOutDate { get; set; }
        public DateTime? dateCreated { get; set; }
        public string approvalStatus { get; set; }
        public string branchName { get; set; }
        public string branchCode { get; set; }
        public string deviceName { get; set; }
        public string osName { get; set; }
        public string userIp { get; set; }
    }
    public class DormantStaffLog
    {
        public int userId { get; set; }
        public string staffName { get; set; }
        public string staffCode { get; set; }

        public int userName { get; set; }

        public DateTime? lastLoginDate { get; set; }

    }

    public class DeletedStaffLog
    {
        public string deletedByName { get; set; }
        public string deletedByStaffCode { get; set; }

        public int? deletedById { get; set; }
        public string deletedStaffCode { get; set; }

        public string deletedStaffName { get; set; }
        public int? deletedStaffId { get; set; }
        public DateTime? deletedDate { get; set; }

    }
}