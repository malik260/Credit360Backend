using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FintrakBanking.Entities.Models
{
    [Table("TBL_TEMP_COLLATERAL_ISPO")]
    public class TBL_TEMP_COLLATERAL_ISPO
    {
        [Key]
        public int TEMPCOLLATERALISPOID { get; set; }

        public int TEMPCOLLATERALCUSTOMERID { get; set; }

        public string ACCOUNTNAMETODEPOSIT { get; set; }

        public string ACCOUNTNUMBERTODEPOSIT { get; set; }

        public string PAYER { get; set; }

        public decimal REGULARPAYMENTAMOUNT { get; set; }

        public decimal? SECURITYVALUE { get; set; }

        public short? FREQUENCYTYPEID { get; set; }

        public string REMARK { get; set; }

        public int COMPANYID { get; set; }

        public int CREATEDBY { get; set; }

        public int? LASTUPDATEDBY { get; set; }

        public DateTime DATETIMECREATED { get; set; }

        public DateTime? DATETIMEUPDATED { get; set; }

        public bool DELETED { get; set; }

        public int? DELETEDBY { get; set; }

        public DateTime? DATETIMEDELETED { get; set; }

        public short APPROVALSTATUSID { get; set; }


        public virtual TBL_FREQUENCY_TYPE TBL_FREQUENCY_TYPE { get; set; }

        public virtual TBL_TEMP_COLLATERAL_CUSTOMER TBL_TEMP_COLLATERAL_CUSTOMER { get; set; }
    }
}
