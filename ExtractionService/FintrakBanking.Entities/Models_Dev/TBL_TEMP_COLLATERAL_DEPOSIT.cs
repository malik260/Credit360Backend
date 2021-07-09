namespace FintrakBanking.Entities.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("FINTRAKBANKING.TBL_TEMP_COLLATERAL_DEPOSIT")]
    public partial class TBL_TEMP_COLLATERAL_DEPOSIT
    {
        [Key]
        [Column(Order = 0)]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public int TEMPCOLLATERALDEPOSITID { get; set; }

        [Key]
        [Column(Order = 1)]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public int TEMPCOLLATERALCUSTOMERID { get; set; }

        [StringLength(100)]
        public string BANK { get; set; }

        [StringLength(50)]
        public string DEALREFERENCENUMBER { get; set; }

        [Key]
        [Column(Order = 2)]
        [StringLength(50)]
        public string ACCOUNTNUMBER { get; set; }

        [Key]
        [Column(Order = 3)]
        public decimal EXISTINGLIENAMOUNT { get; set; }

        [Key]
        [Column(Order = 4)]
        public decimal LIENAMOUNT { get; set; }

        [Key]
        [Column(Order = 5)]
        public decimal AVAILABLEBALANCE { get; set; }

        [Key]
        [Column(Order = 6)]
        public decimal SECURITYVALUE { get; set; }

        [Key]
        [Column(Order = 7)]
        public decimal MATURITYAMOUNT { get; set; }

        [StringLength(500)]
        public string REMARK { get; set; }

        public DateTime MATURITYDATE { get; set; }

        public DateTime EFFECTIVEDATE { get; set; }
    }
}
