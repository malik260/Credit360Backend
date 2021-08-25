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
        [Column(Order = 0)]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public int TEMPCOLLATERALMARKETSECURITYID { get; set; }

        [Key]
        [Column(Order = 1)]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public int TEMPCOLLATERALCUSTOMERID { get; set; }

        [Key]
        [Column(Order = 2)]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public int COLLATERALSUBTYPEID { get; set; }

        [Key]
        [Column(Order = 3)]
        [StringLength(100)]
        public string SECURITYTYPE { get; set; }

        [Key]
        [Column(Order = 4)]
        [StringLength(20)]
        public string DEALREFERENCENUMBER { get; set; }

        [Key]
        [Column(Order = 5)]
        public DateTime EFFECTIVEDATE { get; set; }

        [Key]
        [Column(Order = 6)]
        public DateTime MATURITYDATE { get; set; }

        [Key]
        [Column(Order = 7)]
        public decimal DEALAMOUNT { get; set; }

        [Key]
        [Column(Order = 8)]
        public decimal SECURITYVALUE { get; set; }

        [Key]
        [Column(Order = 9)]
        public decimal LIENUSABLEAMOUNT { get; set; }

        [Key]
        [Column(Order = 10)]
        [StringLength(150)]
        public string ISSUERNAME { get; set; }

        [Key]
        [Column(Order = 11)]
        [StringLength(50)]
        public string ISSUERREFERENCENUMBER { get; set; }

        [Key]
        [Column(Order = 12)]
        public decimal UNITVALUE { get; set; }

        [Key]
        [Column(Order = 13)]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public int NUMBEROFUNITS { get; set; }

        [Key]
        [Column(Order = 14)]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public int RATING { get; set; }

        [Key]
        [Column(Order = 15)]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
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
