namespace FintrakBanking.Entities.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("FINTRAKBANKING.TBL_COLLATERAL_MKT_SECURITY")]
    public partial class TBL_COLLATERAL_MKT_SECURITY
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public int COLLATERALMARKETABLESECURITYID { get; set; }

        public int COLLATERALCUSTOMERID { get; set; }

        [Required]
        [StringLength(100)]
        public string SECURITYTYPE { get; set; }

        public DateTime EFFECTIVEDATE { get; set; }

        public DateTime MATURITYDATE { get; set; }

        public decimal DEALAMOUNT { get; set; }

        public decimal SECURITYVALUE { get; set; }

        public decimal LIENUSABLEAMOUNT { get; set; }

        [StringLength(100)]
        public string BANKPURCHASEDFROM { get; set; }

        [Required]
        [StringLength(150)]
        public string ISSUERNAME { get; set; }

        [Required]
        [StringLength(50)]
        public string ISSUERREFERENCENUMBER { get; set; }

        [StringLength(150)]
        public string FUNDNAME { get; set; }

        public decimal UNITVALUE { get; set; }

        public int NUMBEROFUNITS { get; set; }

        public int RATING { get; set; }

        public int PERCENTAGEINTEREST { get; set; }

        public int? INTERESTPAYMENTFREQUENCY { get; set; }

        [StringLength(500)]
        public string REMARK { get; set; }

        public virtual TBL_COLLATERAL_CUSTOMER TBL_COLLATERAL_CUSTOMER { get; set; }
    }
}
