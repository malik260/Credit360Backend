namespace FintrakBanking.Entities.SQLServerModel
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("temp.TBL_TEMP_COLLATERAL_MKT_SEC")]
    public partial class TBL_TEMP_COLLATERAL_MKT_SEC
    {
        [Key]
        public int TEMPCOLLATERALMARKETSECURITYID { get; set; }

        public int TEMPCOLLATERALCUSTOMERID { get; set; }

        public short COLLATERALSUBTYPEID { get; set; }

        [Required]
        //[StringLength(100)]
        public string SECURITYTYPE { get; set; }

        [Required]
        //[StringLength(20)]
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
        //[StringLength(150)]
        public string ISSUERNAME { get; set; }

        [Required]
        //[StringLength(50)]
        public string ISSUERREFERENCENUMBER { get; set; }

        [Column(TypeName = "money")]
        public decimal UNITVALUE { get; set; }

        public int NUMBEROFUNITS { get; set; }

        public short RATING { get; set; }

        public short PERCENTAGEINTEREST { get; set; }

        public short? INTERESTPAYMENTFREQUENCY { get; set; }

        //[StringLength(500)]
        public string REMARK { get; set; }

        //[StringLength(150)]
        public string FUNDNAME { get; set; }

        //[StringLength(100)]
        public string BANKPURCHASEDFROM { get; set; }

        public virtual TBL_TEMP_COLLATERAL_CUSTOMER TBL_TEMP_COLLATERAL_CUSTOMER { get; set; }
    }
}
