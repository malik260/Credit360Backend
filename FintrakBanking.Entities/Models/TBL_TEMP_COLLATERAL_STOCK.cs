namespace FintrakBanking.Entities.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("temp.TBL_TEMP_COLLATERAL_STOCK")]
    public partial class TBL_TEMP_COLLATERAL_STOCK
    {
        [Key]
        public int TEMPCOLLATERALSTOCKID { get; set; }

        public int TEMPCOLLATERALCUSTOMERID { get; set; }

        [Required]
        [StringLength(250)]
        public string COMPANYNAME { get; set; }

        public int SHAREQUANTITY { get; set; }

        [Column(TypeName = "money")]
        public decimal MARKETPRICE { get; set; }

        [Column(TypeName = "money")]
        public decimal AMOUNT { get; set; }

        [Column(TypeName = "money")]
        public decimal SHARESSECURITYVALUE { get; set; }

        [Column(TypeName = "money")]
        public decimal SHAREVALUEAMOUNTTOUSE { get; set; }
    }
}
