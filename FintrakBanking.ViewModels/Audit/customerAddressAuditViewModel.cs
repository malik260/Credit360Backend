using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FintrakBanking.ViewModels.Audit
{
    public class CustomerAddressAuditViewModel
    {
        public string address { get; set; }

        public string addressType { get; set; }
        public string city { get; set; }
        public string customerName { get; set; }
        public string state { get; set; }
        public string homeTown { get; set; }
        public string pobox { get; set; }
        public string localGovernment { get; set; }
        public string electricMeterNumber { get; set; }
        public string nearestLandmark { get; set; }

    }
}
