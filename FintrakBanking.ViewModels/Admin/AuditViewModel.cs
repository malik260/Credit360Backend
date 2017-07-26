using System;
using System.Collections.Generic;
using System.Text;

namespace FintrakBanking.ViewModels.Admin
{
    public class AuditViewModel
    {
        public long auditId { get; set; }
        public string auditType { get; set; }
        public string firstName { get; set; }
        public string lastName { get; set; }
        public DateTime systemDate { get; set; }
        public DateTime applicationDate { get; set; }
        public string details { get; set; }
        public string url { get; set; }

        public string formattedSystemDate { get { return this.systemDate.ToString("dd/MM/yyy"); } }
        public string formattedApplicationDate { get { return this.systemDate.ToString("dd/MM/yyy"); } }

        public string staffName
        {
            get { return $"{this.firstName} {this.lastName}"; }
        }
    }
}
