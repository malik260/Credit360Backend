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
        public int? CollateralStockId { get; set; }

        [Key]
        [Column(Order = 0)]
        [StringLength(250)]
        public string CompanyName { get; set; }

        [Key]
        [Column(Order = 1)]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public int ShareQuantity { get; set; }

        [Key]
        [Column(Order = 2, TypeName = "money")]
        public decimal MarketPrice { get; set; }

        [Key]
        [Column(Order = 3, TypeName = "money")]
        public decimal Amount { get; set; }

        [Key]
        [Column(Order = 4, TypeName = "money")]
        public decimal SharesSecurityValue { get; set; }

        [Key]
        [Column(Order = 5, TypeName = "money")]
        public decimal ShareValueAmountToUse { get; set; }
    }
}
