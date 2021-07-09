using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FintrakBanking.Entities.Models
{
    [Table("TBL_NEXT_INTEREST_REPAYMENT")]
    public partial class TBL_NEXT_INTEREST_REPAYMENT
    {
        [Key]
        public int id { get; set; }
        public string CUSTOMERNAME { get; set; }
        public string ACCOUNTNUMBER { get; set; }
        public string COMPONENTNAME { get; set; }
        public DateTime? SCHEDULEDUEDATE { get; set; }
        public string CUSTOMERID { get; set; }
        public double? AMOUNTDUE { get; set; }
        public double? OUTSTANDINGBALANCE { get; set; }
        public string CURRENCY { get; set; }
        public DateTime? REPORTDATE { get; set; }
        public double? RNKOFF { get; set; }
    }
}
