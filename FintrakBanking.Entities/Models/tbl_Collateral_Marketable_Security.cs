namespace FintrakBanking.Entities.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("credit.TBL_COLLATERAL_MARKETABLE_SECURITY")]
    public partial class TBL_COLLATERAL_MARKETABLE_SECURITY
    {
        [Key]
        public int COLLATERALMARKETABLESECURITYID { get; set; }

        public int COLLATERALCUSTOMERID { get; set; }

        [Required]
        [StringLength(100)]
        public string SECURITYTYPE { get; set; }

        [Required]
        [StringLength(20)]
        public string DEALREFERENCENUMBER { get; set; }

        public DateTime EFFECTIVEDATE { get; set; }

        public DateTime MATURITYDATE { get; set; }

        [Column(TypeName = "money")]
        public decimal DEALAMOUNT { get; set; }

        [Column(TypeName = "money")]
        public decimal SECURITYVALUE { get; set; }

        [Column(TypeName = "money")]
        public decimal LIENUSABLEAMOUNT { get; set; }

        [Required]
        [StringLength(150)]
        public string ISSUERNAME { get; set; }

        [Required]
        [StringLength(50)]
        public string ISSUERREFERENCENUMBER { get; set; }

        [Column(TypeName = "money")]
        public decimal UNITVALUE { get; set; }

        public int NUMBEROFUNITS { get; set; }

        public short RATING { get; set; }

        public short PERCENTAGEINTEREST { get; set; }

        public short? INTERESTPAYMENTFREQUENCY { get; set; }

        [StringLength(500)]
        public string REMARK { get; set; }

        public virtual TBL_COLLATERAL_CUSTOMER TBL_COLLATERAL_CUSTOMER { get; set; }
    }
}
