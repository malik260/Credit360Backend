using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FintrakBanking.ViewModels.Credit
{
    public class EODErrorLogDetailsViewModel
    {
        public int eodOperationLogDetailId { get; set; }
        public int eodOperationLogId { get; set; }
        public DateTime startDateTime { get; set; }
        public DateTime endDateTime { get; set; }
        public int eodStatusId { get; set; }
        public string eodStatusName { get; set; }
        public string errorInformation { get; set; }
        public string referenceNumber { get; set; }
        public int eodOperationId { get; set; }
        public string eodOperationName { get; set; }
        public DateTime eodDate { get; set; }
        public string eodUserName { get; set; }
    }
}
