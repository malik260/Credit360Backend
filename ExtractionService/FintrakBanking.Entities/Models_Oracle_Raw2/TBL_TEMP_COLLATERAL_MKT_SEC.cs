namespace FintrakBanking.Entities.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("FINTRAKBANKING.TBL_TEMP_COLLATERAL_MKT_SEC")]
    public partial class TBL_TEMP_COLLATERAL_MKT_SEC
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public int TEMPCOLLATERALMARKETSECURITYID { get; set; }

        public int TEMPCOLLATERALCUSTOMERID { get; set; }

        public int COLLATERALSUBTYPEID { get; set; }

        [Required]
        [StringLength(100)]
        public string SECURITYTYPE { get; set; }

        [Required]
        [StringLength(20)]
        public string DEALREFERENCENUMBER { get; set; }

        public DateTime EFFECTIVEDATE { get; set; }

        public DateTime MATURITYDATE { get; set; }

        public decimal DEALAMOUNT { get; set; }

        public decimal SECURITYVALUE { get; set; }

        public decimal LIENUSABLEAMOUNT { get; set; }

        [Required]
        [StringLength(150)]
        public string ISSUERNAME { get; set; }

        [Required]
        [StringLength(50)]
        public string ISSUERREFERENCENUMBER { get; set; }

        public decimal UNITVALUE { get; set; }

        public int NUMBEROFUNITS { get; set; }

        public int RATING { get; set; }

        public int PERCENTAGEINTEREST { get; set; }

        public int? INTERESTPAYMENTFREQUENCY { get; set; }

        [StringLength(500)]
        public string REMARK { get; set; }

        [StringLength(150)]
        public string FUNDNAME { get; set; }

        [StringLength(100)]
        public string BANKPURCHASEDFROM { get; set; }

        public virtual TBL_TEMP_COLLATERAL_CUSTOMER TBL_TEMP_COLLATERAL_CUSTOMER { get; set; }
    }
}
