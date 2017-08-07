namespace FintrakBanking.Entities.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("temp.tbl_Temp_Collateral_Marketable_Security")]
    public partial class tbl_Temp_Collateral_Marketable_Security
    {
        [Key]
        public int CollateralMarketableSecurityId { get; set; }

        public int CollateralCustomerId { get; set; }

        public short CollateralSubTypeId { get; set; }

        [Required]
        [StringLength(100)]
        public string SecurityType { get; set; }

        [Required]
        [StringLength(20)]
        public string DealReferenceNumber { get; set; }

        public DateTime EffectiveDate { get; set; }

        public DateTime MaturityDate { get; set; }

        [Column(TypeName = "money")]
        public decimal DealAmount { get; set; }

        [Column(TypeName = "money")]
        public decimal SecurityValue { get; set; }

        [Column(TypeName = "money")]
        public decimal LienUsableAmount { get; set; }

        [Required]
        [StringLength(150)]
        public string IssuerName { get; set; }

        [Required]
        [StringLength(50)]
        public string IssuerReferenceNumber { get; set; }

        [Column(TypeName = "money")]
        public decimal UnitValue { get; set; }

        public int NumberOfUnits { get; set; }

        public short Rating { get; set; }

        public short PercentageInterest { get; set; }

        public short? InterestPaymentFrequency { get; set; }

        [StringLength(500)]
        public string Remark { get; set; }

        public virtual tbl_Temp_Collateral_Customer tbl_Temp_Collateral_Customer { get; set; }
    }
}
