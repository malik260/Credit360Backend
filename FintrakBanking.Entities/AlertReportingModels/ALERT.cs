using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FintrakBanking.Entities.AlertReportingModels
{

    [Table("ALERT")]
    public partial class ALERT
    {
       
        [Key]
        public long ID { get; set; }
        public string ACCOUNTNAME { get; set; }
        public string ACCOUNTNUMBER { get; set; }
        public string ACCOUNTOFFICERNAME { get; set; }
        public string ACCOUNTSTATUS { get; set; }
        public string ACCOUNTTYPE { get; set; }
        public string CBNCLASSIFICATION { get; set; }
        public string CUSTOMERNAME { get; set; }
        public string DORMANCYDAYS { get; set; }
        public string IFRSCLASSIFICATION { get; set; }
        public string DIVISIONNAME { get; set; }
        public string GROUPHEADNAME { get; set; }
        public string GROUPOBLIGORNAME { get; set; }
        public string DATE { get; set; }
        public string EXPIRINGBAND { get; set; }
        public string AGELASTCREDITDATE { get; set; }
        public string BOOKINGDATE { get; set; }
        public decimal? NPL { get; set; }
        public decimal? ORIGINALAMOUNTDISBURSED { get; set; }
        public decimal? TOTALEXPOSURELCY { get; set; }
        public decimal? LOANAMOUNTLCY { get; set; }
        public decimal? LASTCREDITAMOUNT { get; set; }
        public decimal? PRINCIPALBALANCE { get; set; }
        public decimal? SHAREOFSHF { get; set; }
        public decimal? PRINCIPALOUTSTANDINGBALANCEFCY { get; set; }
        public string UNPODAYSOVERDUE { get; set; }
        public decimal? AVERAGEBALANCE { get; set; }
        public decimal? AMOUNTDUE { get; set; }
        public decimal? TOTALUNPAIDOBLIGATION { get; set; }
        public string MATURITYDATE { get; set; }
        public string SCHEDULEDUEDATE { get; set; }
        //public string DIVISIONID { get; set; }
    }
}