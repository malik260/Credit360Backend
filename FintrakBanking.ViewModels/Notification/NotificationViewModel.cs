using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FintrakBanking.ViewModels.Notification
{
   public  class NotificationViewModel
    {
        public int messageCount { get; set; }
        public string message { get; set; }
        public string operationURL { get; set; }
        public long notificationId { get; set; }
        public int staffId { get; set; }
        public string actionUrl { get; set; }
        public bool isActive { get; set; }
    }

    public class ExternalAlertViewModel
    {
        public long id { get; set; }
        public string accountName { get; set; }
        public string accountNumber { get; set; }
        public string accountOfficerName { get; set; }
        public string accountStatus { get; set; }
        public string accountType { get; set; }
        public string cbnClassification { get; set; }
        public string customerName { get; set; }
        public string dormancyDays { get; set; }
        public string ifrsClassification { get; set; }
        public string divisionName { get; set; }
        public string groupHeadName { get; set; }
        public string groupObligorName { get; set; }
        public string date { get; set; }
        public string expiringBand { get; set; }
        public string ageLastCreditDate { get; set; }
        public string bookingDate { get; set; }
        public decimal? npl { get; set; }
        public decimal? originalAmountDisbursed { get; set; }
        public decimal? totalExposureLcy { get; set; }
        public decimal? loanAmountLcy { get; set; }
        public decimal? lastCreditAmount { get; set; }
        public decimal? principalBalance { get; set; }
        public decimal? shareOfShf { get; set; }
        public decimal? principalOutstandingBalanceFcy { get; set; }
        public string unpoDaysOverdue { get; set; }
        public decimal? averageBalance { get; set; }
        public decimal? amountDue { get; set; }
        public decimal? totalUnpaidObligation { get; set; }
        public string maturityDate { get; set; }
        public string scheduleDueDate { get; set; }
        //public string divisionId { get; set; }
    }
}
