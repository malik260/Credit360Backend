namespace FintrakBanking.Entities.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("credit.tbl_Collateral_Stock")]
    public partial class tbl_Collateral_Stock
    {
        [Key]
        public int CollateralStockId { get; set; }

        public int CollateralCustomerId { get; set; }

        [Required]
        [StringLength(250)]
        public string CompanyName { get; set; }

        public int ShareQuantity { get; set; }

        [Column(TypeName = "money")]
        public decimal MarketPrice { get; set; }

        [Column(TypeName = "money")]
        public decimal Amount { get; set; }

        [Column(TypeName = "money")]
        public decimal SharesSecurityValue { get; set; }

        [Column(TypeName = "money")]
        public decimal ShareValueAmountToUse { get; set; }

        public virtual tbl_Collateral_Customer tbl_Collateral_Customer { get; set; }
    }
}
