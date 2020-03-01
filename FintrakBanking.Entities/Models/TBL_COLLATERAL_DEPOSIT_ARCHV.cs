using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FintrakBanking.Entities.Models
{
    [Table("TBL_COLLATERAL_DEPOSIT_ARCHV")]
    public partial class TBL_COLLATERAL_DEPOSIT_ARCHV
    {
        [Key]
        public int COLLATERALDEPOSITARCHID { get; set; }
        public int COLLATERALDEPOSITID { get; set; }

        public int COLLATERALCUSTOMERID { get; set; }

        ////[StringLength(100)]
        public string BANK { get; set; }

        //[StringLength(50)]
        public string DEALREFERENCENUMBER { get; set; }

        //[Required]
       // //[StringLength(50)]
        public string ACCOUNTNUMBER { get; set; }

        public string ACCOUNTNAME { get; set; }
        public decimal EXISTINGLIENAMOUNT { get; set; }
        public decimal LIENAMOUNT { get; set; }
        public decimal AVAILABLEBALANCE { get; set; }
        public decimal SECURITYVALUE { get; set; }
        public DateTime EFFECTIVEDATE { get; set; }
        public DateTime MATURITYDATE { get; set; }
        public decimal MATURITYAMOUNT { get; set; }
        ////[StringLength(500)]
        public string REMARK { get; set; }
    }
}
