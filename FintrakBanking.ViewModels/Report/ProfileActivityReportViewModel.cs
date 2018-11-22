using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FintrakBanking.ViewModels.Report
{
    public class ProfileActivityReportViewModel
    {
        public string staffRoleName { get; set; }
        public string activityName { get; set; }
        public string createdBy { get; set; }
        public DateTime dateTimeCreated { get; set; }
        public DateTime dateTimeUpdated { get; set; }
    }
}
