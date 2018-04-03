namespace FintrakBanking.Entities.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("credit.TBL_COLLATERAL_DEPOSIT")]
    public partial class TBL_COLLATERAL_DEPOSIT
    {
        [Key]
        public int COLLATERALDEPOSITID { get; set; }

        public int COLLATERALCUSTOMERID { get; set; }

        [StringLength(100)]
        public string BANK { get; set; }

        [StringLength(50)]
        public string DEALREFERENCENUMBER { get; set; }

        [Required]
        [StringLength(50)]
        public string ACCOUNTNUMBER { get; set; }

        [Column(TypeName = "money")]
        public decimal EXISTINGLIENAMOUNT { get; set; }

        [Column(TypeName = "money")]
        public decimal LIENAMOUNT { get; set; }

        [Column(TypeName = "money")]
        public decimal AVAILABLEBALANCE { get; set; }

        [Column(TypeName = "money")]
        public decimal SECURITYVALUE { get; set; }

        [Column(TypeName = "date")]
        public DateTime EFFECTIVEDATE { get; set; }

        [Column(TypeName = "date")]
        public DateTime MATURITYDATE { get; set; }

        [Column(TypeName = "money")]
        public decimal MATURITYAMOUNT { get; set; }

        [StringLength(500)]
        public string REMARK { get; set; }

        public virtual TBL_COLLATERAL_CUSTOMER TBL_COLLATERAL_CUSTOMER { get; set; }
    }
}
