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
        public string username { get; set; }
        public string branchName { get; set; }
        public string  fullName { get { return firstName + " " + lastName;  }}
        public string formattedSystemDate { get { return this.systemDate.ToString("dd/MM/yyy"); } }
        public string formattedApplicationDate { get { return this.systemDate.ToString("dd/MM/yyy"); } }

        public string staffName => $"{this.firstName} {this.lastName}";
        public string ipAddress { get; set; }
    }
}