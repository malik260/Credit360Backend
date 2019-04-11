using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FintrakBanking.Entities.Models
{
    [Table("TBL_COLLATERAL_ISPO")]
    public class TBL_COLLATERAL_ISPO
    {
        [Key]
        public int COLLATERALISPOID { get; set; }

        public int COLLATERALCUSTOMERID { get; set; }

        public string ACCOUNTNAMETODEBIT { get; set; }

        public string ACCOUNTNUMBERTODEBIT { get; set; }

        public string PAYER { get; set; }

        public decimal REGULARPAYMENTAMOUNT { get; set; }

        public short FREQUENCYTYPEID { get; set; }

        public decimal? SECURITYVALUE { get; set; }

        public string REMARK { get; set; }

        public string DESCRIPTION { get; set; }

        public virtual TBL_FREQUENCY_TYPE TBL_FREQUENCY_TYPE { get; set; }

        public virtual TBL_COLLATERAL_CUSTOMER TBL_COLLATERAL_CUSTOMER { get; set; }
    }
}
