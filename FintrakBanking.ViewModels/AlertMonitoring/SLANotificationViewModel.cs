using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FintrakBanking.ViewModels.AlertMonitoring
{
  public  class SLANotificationViewModel
    {
        public int salInterval { get; set; }
        public DateTime systemArrivalDate { get; set; }
        public DateTime? salDateLine { get; set; }
        public int approvalTrailId { get; set; }
        public int targetId { get; set; }
        public DateTime arrivalDate { get; set; }
        public DateTime? systemResponseDate { get; set; }
        public int requestStaffId { get; set; }
        public int? toStaffId { get; set; }
        public int fromApprovalLevelId { get; set; }
        public int? toApprovalLevelId { get; set; }
        public int operationId { get; set; }
        public DateTime? slaNotificationDate { get; set; }
        public string staffEmail { get; set; }
        public string operationName { get; set; }
    }
}
