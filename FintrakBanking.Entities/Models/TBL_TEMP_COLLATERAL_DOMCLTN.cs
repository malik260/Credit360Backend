using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FintrakBanking.Entities.Models
{
    [Table("TBL_TEMP_COLLATERAL_DOMCLTN")]
   public class TBL_TEMP_COLLATERAL_DOMCLTN
    {
        [Key]
        public int TEMPCOLLATERALDOMICILIATIONID { get; set; }

        public int TEMPCOLLATERALCUSTOMERID { get; set; }

        public string CONTRACTDETAILS { get; set; }

        public string EMPLOYER { get; set; }

        public decimal? CONTRACTVALUE { get; set; }

        public decimal? OUTSTANDINGINVOICEAMOUNT { get; set; }

        public string ACCOUNTNAMETODEBIT { get; set; }

        public string PAYER { get; set; }

        public string ACCOUNTNUMBERTODEBIT { get; set; }

        public decimal? REGULARPAYMENTAMOUNT { get; set; }

        public decimal? SECURITYVALUE { get; set; }

        public short? FREQUENCYTYPEID { get; set; }

        public string INVOICENUMBER { get; set; }

        public DateTime? INVOICEDATE { get; set; }

        public decimal? MONTHLYSALARY { get; set; }

        public decimal? ANNUALSALARY { get; set; }

        public decimal? ANNUALALLOWANCES { get; set; }

        public decimal? ANNUALEMOLUMENT { get; set; }

        public string ACCOUNTNUMBER { get; set; }

        public string REMARK { get; set; }

        public string DESCRIPTION { get; set; }

        //public short APPROVALSTATUSID { get; set; }


        public virtual TBL_FREQUENCY_TYPE TBL_FREQUENCY_TYPE { get; set; }

        public virtual TBL_TEMP_COLLATERAL_CUSTOMER TBL_TEMP_COLLATERAL_CUSTOMER { get; set; }
    }
}
