using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FintrakBanking.Entities.AlertReportingModels
{

    [Table("EXTERNAL_ALERT")]
    public partial class EXTERNAL_ALERT
    {
       
        [Key]
        public long id { get; set; }
        public string AccountName { get; set; }
        public string AccountNumber { get; set; }
        public string AccountOfficerName { get; set; }
        public string AccountStatus { get; set; }
        public string AccountType { get; set; }
        public string CBNClassification { get; set; }
        public string CustomerName { get; set; }
        public string DormancyDays { get; set; }
        public string IFRSClassification { get; set; }
        public string DivisionName { get; set; }
        public string GroupHeadName { get; set; }
        public string GroupObligorName { get; set; }
        public string Date { get; set; }
        public string ExpiringBand { get; set; }
        public string AgeLastCreditDate { get; set; }
        public string BookingDate { get; set; }
        public decimal? npl { get; set; }
        public decimal? OriginalAmountDisbursed { get; set; }
        public decimal? TotalExposureLCY { get; set; }
        public decimal? LoanAmountLCY { get; set; }
        public decimal? LastCreditAmount { get; set; }
        public decimal? PrincipalBalance { get; set; }
        public decimal? ShareOfSHF { get; set; }
        public decimal? PrincipalOutstandingBalanceFCY { get; set; }
        public string UNPODaysOverdue { get; set; }
        public decimal? AverageBalance { get; set; }
        public decimal? AmountDue { get; set; }
        public decimal? TotalUnpaidObligation { get; set; }
        public string MaturityDate { get; set; }
        public string ScheduleDueDate { get; set; }
        //public string DIVISIONID { get; set; }
    }
}