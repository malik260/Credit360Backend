namespace FintrakBanking.Entities.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("credit.tbl_Collateral_PreciousMetal")]
    public partial class tbl_Collateral_PreciousMetal
    {
        [Key]
        public int CollateralPreciousMetalId { get; set; }

        public int CollateralCustomerId { get; set; }

        public short CollateralSubTypeId { get; set; }

        public bool IsOwnedByCustomer { get; set; }

        [Required]
        [StringLength(100)]
        public string PreciousMetalName { get; set; }

        [Required]
        [StringLength(50)]
        public string MetalType { get; set; }

        [Required]
        [StringLength(10)]
        public string WeightInGrammes { get; set; }

        [Column(TypeName = "money")]
        public decimal? ValuationAmount { get; set; }

        public double? UnitRate { get; set; }

        [StringLength(100)]
        public string PreciousMetalForm { get; set; }

        [StringLength(500)]
        public string Remark { get; set; }

        public virtual tbl_Collateral_Customer tbl_Collateral_Customer { get; set; }
    }
}
