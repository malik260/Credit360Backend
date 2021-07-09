using System;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FintrakBanking.Entities.Models
{
    [Table("TBL_NEXT_PRINCIPAL_REPAYMENT")]
    public partial class TBL_NEXT_PRINCIPAL_REPAYMENT
    {
        public string ACCOUNTNUMBER { get; set; }
        public string COMPONENTNAME { get; set; }
        public DateTime? SCHEDULEDUEDATE { get; set; }
        public string CUSTOMERID { get; set; }
        public decimal? AMOUNTDUE { get; set; }
        public decimal? OUTSTANDINGBALANCE { get; set; }
        public string CURRENCY { get; set; }
        public DateTime? REPORTDATE { get; set; }
        public decimal? RNKOFF { get; set; }
    }
}
