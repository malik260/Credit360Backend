using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FintrakBanking.ViewModels.Report
{
   public class LimitAndMonitoringViewModel : GeneralEntity
    {
        public int monitoringItemId { get; set; }
        public string messageBody { get; set; }
        public string messageTitle { get; set; }
        public int notificationPeriod1 { get; set; }
        public string escalationLevel1 { get; set; }
        public int? notificationPeriod2 { get; set; }
        public string escalationLevel2 { get; set; }
        public int? notificationPeriod3 { get; set; }
        public string escalationLevel3 { get; set; }

    }
}
